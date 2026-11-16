Shader "SoftStrange/Reference" {
 Properties {
  _MainTex("Image",2D)="white"{}
  _Tint("Recolor strokes",Float)=1
  _Opacity("Opacity",Range(0,1))=0.6
  _Outline("Outline",Float)=0
  _Threshold("Threshold",Range(0.01,1))=0.18
  _Thickness("Thickness",Range(1,4))=1
  _Ink("Ink",Color)=(1,1,1,1)
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _MainTex; float4 _MainTex_TexelSize, _Ink; float _Opacity,_Outline,_Threshold,_Thickness,_Tint;
   v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
   float2 signal(float2 uv) { float4 c=tex2D(_MainTex,uv); return float2(dot(c.rgb,float3(.299,.587,.114))*c.a,c.a); }
   fixed4 frag(v2f i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float4 c=tex2D(_MainTex,i.uv);
    if(_Outline<.5) return float4(_Tint>.5?_Ink.rgb:c.rgb,c.a*_Opacity);
    float2 d=_MainTex_TexelSize.xy*_Thickness;
    float2 a=signal(i.uv+d*float2(-1,1)),b=signal(i.uv+d*float2(0,1)),e=signal(i.uv+d*float2(1,1));
    float2 f=signal(i.uv+d*float2(-1,0)),g=signal(i.uv+d*float2(1,0));
    float2 h=signal(i.uv+d*float2(-1,-1)),j=signal(i.uv+d*float2(0,-1)),k=signal(i.uv+d*float2(1,-1));
    float2 gx=-a-2*f-h+e+2*g+k,gy=a+2*b+e-h-2*j-k;
    float2 magnitude=sqrt(gx*gx+gy*gy);
    float edge=smoothstep(_Threshold,_Threshold+.12,max(magnitude.x,magnitude.y));
    return float4(_Ink.rgb,edge*_Opacity);
   }
   ENDCG
  }
 }
}
