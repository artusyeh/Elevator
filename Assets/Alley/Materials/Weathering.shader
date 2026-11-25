Shader "Alley/Surface Weathering"
{
    Properties { _BaseMap("Mask",2D)="white"{} _Opacity("Opacity",Range(0,1))=.5 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off Offset -1,-1
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float _Opacity;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float3 n:NORMAL;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float fog:TEXCOORD2;float3 n:TEXCOORD3;};
            V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.uv=a.uv;o.fog=ComputeFogFactor(o.p.z);o.n=TransformObjectToWorldNormal(a.n);return o;}
            half4 frag(V i):SV_Target {half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);c.rgb*=max(.15,SampleSH(normalize(i.n))+.15);c.rgb=MixFog(c.rgb,i.fog);c.a*=_Opacity;return c;}
            ENDHLSL
        }
    }
}
