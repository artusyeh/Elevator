Shader "Building/CCTV Screen"
{
 Properties
 {
  _MainTex("Live feed",2D)="black"{}
  _LabelTex("Camera label",2D)="black"{}
  _NoSignalTex("Signal loss label",2D)="black"{}
  _Power("Monitor power",Float)=1
  _Signal("Camera signal",Float)=1
  _Gain("Night camera gain",Float)=1.6
  _Interference("Scripted interference",Float)=0
  _Clock("Clock",Vector)=(23,10,0,0)
 }
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct Varyings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   TEXTURE2D(_LabelTex);SAMPLER(sampler_LabelTex);
   TEXTURE2D(_NoSignalTex);SAMPLER(sampler_NoSignalTex);
   CBUFFER_START(UnityPerMaterial)
    float _Power,_Signal,_Gain,_Interference;float4 _Clock;
   CBUFFER_END
   Varyings vert(Attributes i){Varyings o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
   float rectangle(float2 p,float2 center,float2 halfSize){float2 d=abs(p-center)-halfSize;return 1-step(0,max(d.x,d.y));}
   float digit(float2 p,int number)
   {
    int masks[10]={63,6,91,79,102,109,125,7,127,111};int mask=masks[clamp(number,0,9)];float ink=0;
    if((mask&1)!=0)ink+=rectangle(p,float2(.5,.92),float2(.30,.055));
    if((mask&2)!=0)ink+=rectangle(p,float2(.84,.71),float2(.055,.17));
    if((mask&4)!=0)ink+=rectangle(p,float2(.84,.27),float2(.055,.17));
    if((mask&8)!=0)ink+=rectangle(p,float2(.5,.05),float2(.30,.055));
    if((mask&16)!=0)ink+=rectangle(p,float2(.16,.27),float2(.055,.17));
    if((mask&32)!=0)ink+=rectangle(p,float2(.16,.71),float2(.055,.17));
    if((mask&64)!=0)ink+=rectangle(p,float2(.5,.49),float2(.30,.055));return saturate(ink);
   }
   half4 frag(Varyings i):SV_Target
   {
    float2 uv=i.uv;uv.x+=sin(uv.y*53+_Time.y*21)*.035*_Interference;float3 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
    float luminance=pow(saturate(dot(source,float3(.2126,.7152,.0722))*_Gain),.78);
    float random=frac(sin(dot(floor(uv*float2(384,256)),float2(12.9898,78.233))+floor(_Time.y*12))*43758.5453);
    float lines=1-.035*(.5+.5*sin(uv.y*256*3.14159));
    float value=lerp(.025,luminance,_Signal)*lines+(random-.5)*lerp(.018,.38,_Interference);
    float label=SAMPLE_TEXTURE2D(_LabelTex,sampler_LabelTex,uv).a;
    float lost=SAMPLE_TEXTURE2D(_NoSignalTex,sampler_NoSignalTex,uv).a*(1-_Signal);
    float2 clock=(uv-float2(.035,.045))/float2(.018,.047);int cell=(int)floor(clock.x/1.25);float2 glyph=float2(frac(clock.x/1.25)*1.25,clock.y);float timeInk=0;
    if(clock.y>=0&&clock.y<=1&&cell>=0&&cell<8)
    {
     int n=0;if(cell==0)n=(int)_Clock.x/10;if(cell==1)n=(int)_Clock.x%10;if(cell==3)n=(int)_Clock.y/10;if(cell==4)n=(int)_Clock.y%10;if(cell==6)n=(int)_Clock.z/10;if(cell==7)n=(int)_Clock.z%10;
     timeInk=(cell==2||cell==5)?rectangle(glyph,float2(.5,.3),float2(.08,.07))+rectangle(glyph,float2(.5,.7),float2(.08,.07)):digit(glyph,n);
    }
    value=lerp(value,.72,saturate(max(max(label,lost),timeInk)*.9));float vignette=saturate(1-.12*dot((uv-.5)*2,(uv-.5)*2));
    return half4(lerp(float3(.003,.004,.003),value*float3(.95,1,.96)*vignette,saturate(_Power)),1);
   }
   ENDHLSL
  }
 }
}
