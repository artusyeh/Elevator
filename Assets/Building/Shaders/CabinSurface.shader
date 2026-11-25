Shader "Building/Cabin Surface"
{
 Properties
 {
  _BaseMap("Photographic surface",2D)="white"{}
  _BumpMap("Normal",2D)="bump"{}
  _MaskMap("Metal AO Smoothness",2D)="white"{}
  _BaseColor("Finish",Color)=(.32,.36,.27,1)
  _Meters("Map size in metres",Vector)=(1.6,2.5,0,0)
  _UVOffset("Panel variation",Vector)=(0,0,0,0)
  _PanelHalfSize("Face half size",Vector)=(.25,1.13,0,0)
  _ReferenceLuma("Source luminance",Float)=.055
  _BumpScale("Normal strength",Float)=.25
  _Smoothness("Finish smoothness",Range(0,1))=.5
  _Metallic("Metalness",Range(0,1))=.08
  _Wear("Localized paint wear",Range(0,1))=.7
  _Mode("Paint, bare metal, rubber",Float)=0
  _TouchWear("Control fingerprints",Range(0,1))=0
  _Cull("Cull",Float)=2
 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Name "ForwardLit"
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
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
   TEXTURE2D(_MaskMap);SAMPLER(sampler_MaskMap);
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseColor,_BaseMap_ST,_Meters,_UVOffset,_PanelHalfSize;
   float _ReferenceLuma,_BumpScale,_Smoothness,_Metallic,_Wear,_Mode,_TouchWear;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 local:TEXCOORD1;float3 n:TEXCOORD2;float fog:TEXCOORD3;};
   V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.local=a.p.xyz;o.n=a.n;o.fog=ComputeFogFactor(o.p.z);return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n),w=pow(abs(n),10);w/=max(.001,w.x+w.y+w.z);
    float2 uvx=i.local.zy/_Meters.xy+_UVOffset.xy,uvy=i.local.xz/_Meters.xy+_UVOffset.xy,uvz=i.local.xy/_Meters.xy+_UVOffset.xy;
    half3 source=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uvx).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uvy).rgb*w.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uvz).rgb*w.z;
    half4 mask=SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,uvx)*w.x+SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,uvy)*w.y+SAMPLE_TEXTURE2D(_MaskMap,sampler_MaskMap,uvz)*w.z;
    half3 nx=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uvx),_BumpScale),ny=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uvy),_BumpScale),nz=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,uvz),_BumpScale);
    float3 normal=normalize(float3(sign(n.x)*nx.z,nx.y,nx.x)*w.x+float3(ny.x,sign(n.y)*ny.z,ny.y)*w.y+float3(nz.x,nz.y,sign(n.z)*nz.z)*w.z);
    float2 face=i.local.zy*w.x+i.local.xz*w.y+i.local.xy*w.z;
    float2 p=face+_UVOffset.xy*3.7;
    float edge=min(_PanelHalfSize.x-abs(face.x),_PanelHalfSize.y-abs(face.y));
    float dirtyEdge=1-smoothstep(.002,.028,edge);
    float lower=(1-smoothstep(0,.30,i.local.y+_PanelHalfSize.y))*(.45+.55*noise(p*17));
    float gray=dot(source,half3(.2126,.7152,.0722));
    half3 albedo=_BaseColor.rgb*clamp(gray/max(.005,_ReferenceLuma),.38,1.65);
    float rough=lerp(.21,.67,mask.a)*_Smoothness;
    float metal=_Metallic;
    if(_Mode<.5)
    {
     // Photographic scratches remain the primary detail. Chips only develop at folds and kick zones.
     float broken=noise(p*95)*.66+noise(p*31)*.34;
     float edgeZone=1-smoothstep(.002,.025,edge);
     float kickZone=lower*noise(p*8);
     float chip=smoothstep(.60,.79,broken)*saturate(edgeZone+kickZone*.38)*_Wear;
     float rust=chip*(.35+.65*noise(p*57+7));
     albedo=lerp(albedo,half3(.105,.062,.031),rust);
     albedo*=1-_Wear*(dirtyEdge*.30+lower*.27);
     rough*=1-chip*.6;
     metal=lerp(metal,.50,chip*.6);
    }
    else if(_Mode<1.5)
    {
     float brush=noise(float2(p.x*920,p.y*2.5));
     albedo*=lerp(.87,1.06,brush);
     albedo*=1-dirtyEdge*.30;
     rough=clamp(_Smoothness*(.52+.33*mask.a+.15*brush),.15,.65);
     float row=round((face.y+1.10-.73)/.145);
     float2 touch=float2(face.x-.037,face.y+1.10-(.73+row*.145)-.018);
     float thumb=(1-smoothstep(.015,.054,length(touch*float2(1,1.15))))*step(0,row)*step(row,5)*_TouchWear;
     float ridges=.5+.5*sin(length(touch*float2(1,1.15))*2900);
     albedo*=1-thumb*(.11+.04*ridges);
     rough*=1-thumb*.25;
    }
    else
    {
     // 36 mm coin flooring, moulded relief with polished foot traffic and dirty rim recesses.
     float2 q=(frac(i.local.xz/.036)-.5)*.036;
     float d=length(q),rim=exp(-pow((d-.011)/.0015,2));
     float stud=1-smoothstep(.0105,.0117,d);
     float traffic=(1-smoothstep(.12,.59,abs(i.local.x)))*(.7+.3*noise(p*9));
     normal=normalize(float3(-q.x/max(d,.001)*rim*.40*(1-traffic*.5),1,-q.y/max(d,.001)*rim*.40*(1-traffic*.5)));
     normal=normalize(normal+float3(ny.x,0,ny.y)*.16);
     float scuff=smoothstep(.57,.82,noise(float2(p.x*48,p.y*5.3)))*noise(p*15);
     albedo=_BaseColor.rgb*(.83+.3*stud+.22*traffic)*(1-dirtyEdge*.45-scuff*.20)*clamp(gray/.025,.65,1.25);
     rough=.14+traffic*.13+stud*.035+mask.a*.06;
     metal=0;
    }
    InputData data=(InputData)0;data.positionWS=i.world;data.normalWS=TransformObjectToWorldNormal(normal);data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);data.shadowCoord=TransformWorldToShadowCoord(i.world);data.bakedGI=SampleSH(data.normalWS);data.shadowMask=half4(1,1,1,1);data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);
    SurfaceData surface=(SurfaceData)0;surface.albedo=albedo;surface.alpha=1;surface.metallic=metal;surface.specular=half3(.04,.04,.04);surface.occlusion=lerp(.60,1,mask.g)*(1-dirtyEdge*.10);surface.smoothness=rough;surface.normalTS=half3(0,0,1);
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

 }
}
