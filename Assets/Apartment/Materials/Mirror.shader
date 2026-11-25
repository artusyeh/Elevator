Shader "Apartment/Planar Mirror"
{
 Properties{_BaseMap("Reflection",2D)="black"{} _Dirt("Dirt",2D)="white"{}}
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
   float4x4 _ReflectionVP;
   struct A{float4 position:POSITION;float2 uv:TEXCOORD0;};
   struct V{float4 position:SV_POSITION;float4 reflection:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.position=TransformObjectToHClip(a.position.xyz);o.reflection=ComputeScreenPos(mul(_ReflectionVP,float4(TransformObjectToWorld(a.position.xyz),1)));o.uv=a.uv;return o;}
   half4 frag(V i):SV_Target{float2 uv=i.reflection.xy/i.reflection.w;half3 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;half dirt=SAMPLE_TEXTURE2D(_Dirt,sampler_Dirt,i.uv).r;return half4(c*lerp(.69,.86,dirt)+.012,1);}
   ENDHLSL
  }
 }
}
