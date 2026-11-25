Shader "Building/Aged Terrazzo"
{
    Properties
    {
        _BaseMap("Terrazzo",2D)="white"{}
        _BumpMap("Normal",2D)="bump"{}
        _MaskMap("Metal AO Rough Smooth",2D)="white"{}
        _BaseColor("Tint",Color)=(.7,.7,.7,1)
        _BumpScale("Normal Strength",Float)=.45
        _Meters("Texture Size (m)",Vector)=(2,2,0,0)
        _Metallic("Metallic Scale",Range(0,1))=1
        _Smoothness("Smoothness Scale",Range(0,1))=1
        _Cutoff("Cutoff",Float)=.5
        _Cull("Cull",Float)=2
        _FloorHalfSize("Floor half width and length",Vector)=(2.6,6.25,0,0)
        _WearOffset("Wear variation",Vector)=(0,0,0,0)
        _WearStrength("Surface age",Range(0,1))=.65
        _EdgeGrime("Edge grime",Range(0,1))=.38
        _LocalStains("Localized old marks",Range(0,1))=.6
        _JoinedEdge("Joined edge: side, start, end, enabled",Vector)=(0,0,0,0)
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MaskMap);SAMPLER(sampler_MaskMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;float4 _BaseMap_ST;float _BumpScale,_Metallic,_Smoothness;float4 _Meters;
            float4 _FloorHalfSize,_WearOffset,_JoinedEdge;float _WearStrength,_EdgeGrime,_LocalStains;
            CBUFFER_END
            struct A {float4 pos:POSITION;float3 normal:NORMAL;float2 lm:TEXCOORD1;};
            struct V {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;DECLARE_LIGHTMAP_OR_SH(lm,sh,2);float fog:TEXCOORD3;float2 floorPos:TEXCOORD4;};
            V vert(A a){V o=(V)0;VertexPositionInputs p=GetVertexPositionInputs(a.pos.xyz);o.pos=p.positionCS;o.world=p.positionWS;o.normal=TransformObjectToWorldNormal(a.normal);o.floorPos=a.pos.xz;OUTPUT_LIGHTMAP_UV(a.lm,unity_LightmapST,o.lm);OUTPUT_SH(o.normal,o.sh);o.fog=ComputeFogFactor(o.pos.z);return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
            half4 frag(V i):SV_Target
            {
                float3 n=normalize(i.normal);float3 w=pow(abs(n),8);w/=max(.0001,w.x+w.y+w.z);
                float2 u=i.world.zy/_Meters.xy,v=i.world.xz/_Meters.xy,t=i.world.xy/_Meters.xy;
                half3 albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,u).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,v).rgb*w.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,t).rgb*w.z;
                half4 mask=SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,u)*w.x+SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,v)*w.y+SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,t)*w.z;
                half3 nx=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,u),_BumpScale);
                half3 ny=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,v),_BumpScale);
                half3 nz=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,t),_BumpScale);
                float3 normal=normalize(float3(sign(n.x)*nx.z,nx.y,nx.x)*w.x+float3(ny.x,sign(n.y)*ny.z,ny.y)*w.y+float3(nz.x,nz.y,sign(n.z)*nz.z)*w.z);
                InputData data=(InputData)0;data.positionWS=i.world;data.normalWS=normal;data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);data.shadowCoord=TransformWorldToShadowCoord(i.world);
                data.bakedGI=SAMPLE_GI(i.lm,i.sh,data.normalWS);data.shadowMask=SAMPLE_SHADOWMASK(i.lm);data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.pos);
                SurfaceData surface=(SurfaceData)0;surface.albedo=albedo*_BaseColor.rgb;surface.alpha=1;surface.metallic=mask.r*_Metallic;surface.specular=half3(.04,.04,.04);surface.occlusion=lerp(.65,1,mask.g);surface.smoothness=mask.a*_Smoothness;surface.normalTS=half3(0,0,1);
                // Centimetre-scale scuffs and metre-scale wear stay continuous across the slab.
                // Everything modifies the existing PBR surface; no coplanar overlay geometry.
                float2 p=i.floorPos+_WearOffset.xy+floor(i.world.y/2.9+.1)*float2(17.3,29.7);
                float broad=noise(p*.66)+.35*noise(p*2.1+19.3);
                float stain=smoothstep(.49,.97,broad);
                float fine=noise(p*13.7+4.2);
                float sideDistance=_FloorHalfSize.x-abs(i.floorPos.x);
                if(_JoinedEdge.w>.5 && i.floorPos.x*_JoinedEdge.x>0 && i.floorPos.y>=_JoinedEdge.y && i.floorPos.y<=_JoinedEdge.z)
                    sideDistance=_FloorHalfSize.x+abs(i.floorPos.x);
                float edgeDistance=min(sideDistance,_FloorHalfSize.y-abs(i.floorPos.y));
                float edge=(1-smoothstep(.015,.32+.12*noise(p*4),edgeDistance))*(.65+.35*fine);
                float path=(1-smoothstep(.35,1.3,abs(i.floorPos.x+_WearOffset.x)))*(.45+.55*noise(p*1.2));
                float marks=smoothstep(.62,.86,noise(float2(p.x*35,p.y*3.8)))*smoothstep(.55,.9,noise(p*5.2));
                float dirt=saturate(_WearStrength*(stain*.66+marks*.19)+edge*_EdgeGrime);
                float2 sp=i.floorPos;
                float patchA=length((sp-_FloorHalfSize.xy*float2(.80,.03))/float2(.48,1.4));
                float patchB=length((sp-_FloorHalfSize.xy*float2(-.22,-.77))/float2(.8,.58));
                float patchC=length((sp-_FloorHalfSize.xy*float2(-.82,.60))/float2(.42,.70));
                float patchDistance=min(patchA,min(patchB,patchC));
                float oldSpill=1-smoothstep(.30,.98,patchDistance+.40*(noise(p*9)-.5)+.22*(fine-.5));
                dirt=saturate(dirt+oldSpill*_LocalStains*(.65+.35*fine));
                surface.albedo*=lerp(half3(1,1,1),half3(.39,.35,.29),dirt);
                float2 cell=floor(p*1.1),q=frac(p*1.1)-.5;
                float2 spot=(float2(hash(cell+2.7),hash(cell+9.1))-.5)*.4;
                float ring=abs(length((q-spot)*float2(1,1.25))-.10);
                float oldMark=(1-smoothstep(.008,.025,ring))*step(.83,hash(cell+3.4));
                oldMark*=.4+.6*noise(p*31);
                surface.albedo*=1-oldMark*_WearStrength*.16;
                surface.smoothness=saturate(lerp(.23,.55,mask.a)*(.9-.24*stain)+path*.045-edge*.12);
                // Restrained old repair joints, irregular and antialiased instead of a repeated crack web.
                float jointDist=min(abs(p.y-1.73-.011*sin(p.x*8.7)-.005*sin(p.x*23)),abs(p.y+3.19-.008*sin(p.x*11.3)));
                float aa=max(fwidth(jointDist),.0006);
                float joint=(1-smoothstep(.001,.001+aa,jointDist))*_WearStrength*.50;
                surface.albedo*=1-joint;
                surface.smoothness*=1-joint*.3;
                half4 col=UniversalFragmentPBR(data,surface);col.rgb=MixFog(col.rgb,i.fog);return col;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
    }
}
