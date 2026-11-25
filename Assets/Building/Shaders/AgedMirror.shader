Shader "Building/Aged Mirror"
{
 Properties{_BaseMap("Live reflection",2D)="black"{} _Dirt("Smudge variation",2D)="white"{} _Age("Edge deterioration",Range(0,1))=.65}
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_Dirt);SAMPLER(sampler_Dirt);
   float4x4 _ReflectionVP;float _Age;
   struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};
   struct V{float4 p:SV_POSITION;float4 reflection:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.reflection=ComputeScreenPos(mul(_ReflectionVP,float4(TransformObjectToWorld(a.p.xyz),1)));o.uv=a.uv;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
   half4 frag(V i):SV_Target
   {
    float2 uv=i.reflection.xy/i.reflection.w;half3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
    float edge=min(min(i.uv.x,1-i.uv.x),min(i.uv.y,1-i.uv.y));
    float variation=noise(i.uv*155)*.6+noise(i.uv*41)*.4;
    float rot=(1-smoothstep(.004,.032+.018*variation,edge))*smoothstep(.27,.61,variation)*_Age;
    float wipe=noise(i.uv*float2(4,15))*.10;
    float scratchDist=abs(i.uv.y-(.21+.23*i.uv.x));
    float scratch=(1-smoothstep(.00025,max(.0007,fwidth(scratchDist)*1.5),scratchDist))*smoothstep(.06,.4,i.uv.x)*(1-smoothstep(.57,.74,i.uv.x));
    float scratch2=abs(i.uv.y-(.65-.15*i.uv.x));
    scratch+=.7*(1-smoothstep(.00025,max(.0007,fwidth(scratch2)*1.5),scratch2))*smoothstep(.32,.48,i.uv.x)*(1-smoothstep(.68,.86,i.uv.x));
    c*=.84-wipe;
    c=lerp(c,half3(.023,.025,.020),rot);
    c+=scratch*.024;
    return half4(c,1);
   }
   ENDHLSL
  }
 }
}
