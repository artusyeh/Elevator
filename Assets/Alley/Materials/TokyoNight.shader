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
            sampler2D _MainTex; float4 _MainTex_TexelSize; float4 _Crop, _AngularSize;
            float _Bearing,_Elevation,_PhotoExposure;
            struct v2f {float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
            v2f vert(float4 vertex:POSITION) {v2f o;o.pos=UnityObjectToClipPos(vertex);o.dir=vertex.xyz;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float hash3(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float noise3(float3 p){float3 c=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash3(c),hash3(c+float3(1,0,0)),f.x),lerp(hash3(c+float3(0,1,0)),hash3(c+float3(1,1,0)),f.x),f.y),lerp(lerp(hash3(c+float3(0,0,1)),hash3(c+float3(1,0,1)),f.x),lerp(hash3(c+float3(0,1,1)),hash3(c+float3(1,1,1)),f.x),f.y),f.z);}
            half4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.dir);
                float az=atan2(d.x,d.z)*57.29578;
                float el=asin(clamp(d.y,-1,1))*57.29578;
                float t=saturate(el/65);
                float3 sky=lerp(float3(.016,.030,.068),float3(.0025,.007,.026),pow(t,.7));
                float haze=exp(-max(el,0)*.12);sky+=float3(.008,.006,.004)*haze;
                float3 cloudP=d*float3(7,19,7)+float3(7.1,3.7,19.2);
                float cloud=noise3(cloudP)*.56+noise3(cloudP*2.13)*.28+noise3(cloudP*4.57)*.16;
                float veil=smoothstep(.49,.72,cloud)*smoothstep(1,17,el);
                sky=lerp(sky,sky*.64+float3(.014,.018,.028),veil*.52);
                // Direction-space stars avoid an azimuth seam and a repeated rectangular grid.
                float3 cell=d*530,id=floor(cell);float seed=hash3(id);
                float3 offset=float3(hash3(id+17),hash3(id+43),hash3(id+71));
                float radius=lerp(.07,.20,hash3(id+97));float star=(1-smoothstep(radius*.3,radius,length(frac(cell)-offset)))*step(.9982,seed);
                sky+=star*lerp(float3(.08,.095,.13),float3(.30,.28,.23),hash3(id+7))*smoothstep(4,22,el)*(1-veil);
                float2 starCell=float2((az+180)*2,(el+90)*2),sid=floor(starCell);
                sid.x=fmod(sid.x,720);float2 jitter=float2(hash(sid+13),hash(sid+47));
                float starPoint=(1-smoothstep(.07,lerp(.18,.34,hash(sid+71)),length(frac(starCell)-jitter)))*step(.9977,hash(sid));
                sky+=starPoint*lerp(float3(.20,.27,.40),float3(.72,.66,.51),hash(sid+37))*smoothstep(7,25,el)*(1-veil*.85);
                float delta=az-_Bearing;delta=delta-360*floor((delta+180)/360);
                float2 uv=float2(delta/_AngularSize.x+.5,(el-_Elevation)/_AngularSize.y+.5);
                float mask=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1);
                float2 photoUV=_Crop.xy+saturate(uv)*_Crop.zw;
                float3 photo=tex2D(_MainTex,photoUV).rgb;
                float3 neighbors=(tex2D(_MainTex,photoUV+float2(_MainTex_TexelSize.x,0)).rgb+tex2D(_MainTex,photoUV-float2(_MainTex_TexelSize.x,0)).rgb+tex2D(_MainTex,photoUV+float2(0,_MainTex_TexelSize.y)).rgb+tex2D(_MainTex,photoUV-float2(0,_MainTex_TexelSize.y)).rgb)*.25;
                photo=max(0,photo*1.6-neighbors*.6);
                float l=max(photo.r,max(photo.g,photo.b));
                float edge=smoothstep(0,.12,uv.x)*smoothstep(1,.88,uv.x)*smoothstep(0,.1,uv.y)*smoothstep(1,.88,uv.y);
                // Retain photographed lights and landmark; let the original gray sky dissolve into blue.
                float towerWidth=lerp(.20,.035,smoothstep(.15,.85,uv.y));
                float towerMask=(1-smoothstep(towerWidth,towerWidth+.025,abs(uv.x-.447)))*(1-smoothstep(.855,.875,uv.y));
                float alpha=smoothstep(.025,.12,l)*mask*edge*towerMask;
                float3 result=lerp(sky,photo*float3(1,.98,.95)*_PhotoExposure,alpha);
                return half4(result,1);
            }
            ENDHLSL
        }
    }
}



