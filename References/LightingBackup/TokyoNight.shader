Shader "Alley/Tokyo Night"
{
    Properties
    {
        _MainTex("Tokyo Photo",2D)="black"{}
        _Crop("Photo Crop (UV)",Vector)=(0.839,0.79,0.032,0.21)
        _Bearing("Tower Bearing",Float)=0
        _Elevation("Crop Center Elevation",Float)=25
        _AngularSize("Angular Size",Vector)=(28,43,0,0)
        _PhotoExposure("Photo Exposure",Float)=0.7
    }
    SubShader
    {
        Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _Crop, _AngularSize;
            float _Bearing,_Elevation,_PhotoExposure;
            struct v2f {float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
            v2f vert(float4 vertex:POSITION) {v2f o;o.pos=UnityObjectToClipPos(vertex);o.dir=vertex.xyz;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            half4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.dir);
                float az=atan2(d.x,d.z)*57.29578;
                float el=asin(clamp(d.y,-1,1))*57.29578;
                float t=saturate(el/65);
                float3 sky=lerp(float3(.009,.023,.060),float3(.002,.006,.023),t);
                float cloud=(sin(az*.065+sin(el*.12)*2)+sin(az*.13-el*.2))*.5;
                sky+=float3(.002,.003,.004)*smoothstep(.55,1,cloud)*smoothstep(0,12,el);
                float2 cell=float2(az*5,el*5);float2 id=floor(cell);
                float star=smoothstep(.06,0,length(frac(cell)-.5))*step(.9975,hash(id));
                sky+=star*float3(.08,.09,.11)*smoothstep(8,30,el);
                float delta=az-_Bearing;delta=delta-360*floor((delta+180)/360);
                float2 uv=float2(delta/_AngularSize.x+.5,(el-_Elevation)/_AngularSize.y+.5);
                float mask=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1);
                float3 photo=tex2D(_MainTex,_Crop.xy+saturate(uv)*_Crop.zw).rgb;
                float l=max(photo.r,max(photo.g,photo.b));
                float edge=smoothstep(0,.12,uv.x)*smoothstep(1,.88,uv.x)*smoothstep(0,.1,uv.y)*smoothstep(1,.88,uv.y);
                // Retain photographed lights and landmark; let the original gray sky dissolve into blue.
                float alpha=smoothstep(.065,.24,l)*mask*edge;
                float3 result=lerp(sky,photo*float3(1,.91,.82)*_PhotoExposure,alpha);
                return half4(result,1);
            }
            ENDHLSL
        }
    }
}
