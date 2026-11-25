Shader "Building/Obscured Glass"
{
    Properties
    {
        _DirtMap("Deposits and water streaks",2D)="white"{}
        _Transmission("Night transmission",Range(0,.3))=.095
        _Blur("Frost spread",Range(.01,.15))=.065
        _Seed("Pane age variation",Float)=1
        _DirtColor("Dust and mud",Color)=(.20,.195,.16,1)
        _GlassTint("Transmitted tint",Color)=(.49,.61,.62,1)
        _HazeTint("Faint night scatter",Color)=(.105,.12,.13,1)
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            // Composite the frosted scene sample, rather than retaining a sharp alpha-blended view.
            Blend One Zero
            ZWrite On
            Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURE2D(_DirtMap);SAMPLER(sampler_DirtMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _DirtColor,_GlassTint,_HazeTint;float _Transmission,_Blur,_Seed;
            CBUFFER_END
            struct A{float4 position:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
            struct V{float4 position:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;};
            V vert(A i){V o;o.position=TransformObjectToHClip(i.position.xyz);o.world=TransformObjectToWorld(i.position.xyz);o.normal=TransformObjectToWorldNormal(i.normal);o.uv=i.uv;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
            half4 frag(V i,FRONT_FACE_TYPE facing:FRONT_FACE_SEMANTIC):SV_Target
            {
                float2 uv=i.uv;float2 p=uv+float2(_Seed*.173,_Seed*.297);
                float2 pane=float2(frac(uv.x*2),uv.y);float edge=min(min(pane.x,1-pane.x),min(pane.y,1-pane.y));
                float cloud=noise(p*float2(5,7))*.65+noise(p*float2(17,12))*.35;
                float grit=noise(p*float2(90,160));
                float deposits=SAMPLE_TEXTURE2D(_DirtMap,sampler_DirtMap,uv).a;
                float border=1-smoothstep(.015,.17,edge);
                float mud=(1-smoothstep(.04,.38,uv.y))*(.55+.45*noise(p*float2(13,5)));
                float trails=smoothstep(.56,.77,noise(float2(p.x*65,p.y*2.2)))*(1-smoothstep(.0,.90,uv.y));
                float dirt=saturate(.28+cloud*.30+border*.38+mud*.30+deposits*.28+trails*.17);
                float2 screen=GetNormalizedScreenSpaceUV(i.position);
                float2 radius=float2(_Blur/_ScaledScreenParams.x*_ScaledScreenParams.y,_Blur)*(1+cloud*.35);
                float2 distortion=(float2(noise(p*17),noise(p*21+9))-.5)*radius*.5;
                float2 s=saturate(screen+distortion);
                half3 blurred=SampleSceneColor(s)*.16;
                blurred+=(SampleSceneColor(saturate(s+radius*float2(1,0)))+SampleSceneColor(saturate(s-radius*float2(1,0)))+SampleSceneColor(saturate(s+radius*float2(0,1)))+SampleSceneColor(saturate(s-radius*float2(0,1))))*.12;
                blurred+=(SampleSceneColor(saturate(s+radius*.707))+SampleSceneColor(saturate(s-radius*.707))+SampleSceneColor(saturate(s+radius*float2(.707,-.707)))+SampleSceneColor(saturate(s+radius*float2(-.707,.707))))*.09;
                float3 n=normalize(i.normal)*IS_FRONT_VFACE(facing,1,-1);
                InputData data=(InputData)0;data.positionWS=i.world;data.normalWS=n;data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);data.shadowCoord=TransformWorldToShadowCoord(i.world);data.bakedGI=SampleSH(n);data.shadowMask=half4(1,1,1,1);data.normalizedScreenSpaceUV=screen;
                SurfaceData surface=(SurfaceData)0;surface.albedo=_DirtColor.rgb*(.40+dirt*.6+grit*.10);surface.alpha=1;surface.specular=half3(.04,.04,.04);surface.smoothness=lerp(.53,.24,dirt);surface.occlusion=.85;surface.normalTS=half3(0,0,1);
                half3 reflected=UniversalFragmentPBR(data,surface).rgb;
                float transmission=_Transmission*lerp(1.2,.15,dirt);
                half3 transmitted=blurred*_GlassTint.rgb*transmission;
                // A faint cool edge survives through thinner deposits; no bright rim or emissive outline.
                float thin=smoothstep(.04,.09,edge)*(1-smoothstep(.09,.16,edge))*(1-dirt);
                half3 light=half3(.0007,.0013,.0018)*thin;
                half3 haze=_HazeTint.rgb*lerp(.65,1.35,cloud)*(1-border*.70)*(1-mud*.45)*(1-trails*.25);
                haze+=_DirtColor.rgb*deposits*.012;
                return half4(reflected+transmitted+light+haze,1);
            }
            ENDHLSL
        }
    }
}
