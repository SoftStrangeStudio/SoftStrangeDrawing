Shader "SoftStrange/CurveStroke" {
 Properties {_Ink("Line color",Color)=(1,1,1,1) _Opacity("Opacity",Range(0,1))=0.8 _Dotted("Dotted",Float)=0 _DotSpacing("Dot spacing",Float)=0.02 _DotSize("Dot size",Range(.1,.5))=.32 _LineWidth("Width",Float)=.004}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 float4 _Ink;float _Opacity,_Dotted,_DotSpacing,_DotSize,_LineWidth;
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float a=1;if(_Dotted>.5){float2 p=float2((frac(i.uv.x/max(.001,_DotSpacing))-.5)*_DotSpacing/max(.0001,_LineWidth),i.uv.y-.5);a=1-smoothstep(_DotSize-.05,_DotSize,length(p));}return float4(_Ink.rgb,_Ink.a*_Opacity*a);}
 ENDCG}}
}
