
Shader "Hidden/MicroSplatMeshPaintProjector" 
{
   Properties 
   {
      _Tex ("Cookie", 2D) = "gray" {}
   }
   Subshader 
   {
      Tags {"Queue"="Transparent"}
      Pass 
      {
         ZWrite Off
         ColorMask RGB
         Blend SrcAlpha OneMinusSrcAlpha
         Offset -1, -1
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         #include "UnityCG.cginc"
         struct v2f 
         {
            float4 uvShadow : TEXCOORD0;
            float4 uvFalloff : TEXCOORD1;
            float4 pos : SV_POSITION;
         };

         float4x4 unity_Projector;
         float4x4 unity_ProjectorClip;
         fixed3 _BrushColor;

         v2f vert (float4 vertex : POSITION)
         {
            v2f o;
            o.pos = UnityObjectToClipPos (vertex);
            o.uvShadow = mul (unity_Projector, vertex);
            o.uvFalloff = mul (unity_ProjectorClip, vertex);
            return o;
         }

         sampler2D _Tex;
          
         fixed4 frag (v2f i) : SV_Target
         {
            fixed4 p = tex2Dproj (_Tex, UNITY_PROJ_COORD(i.uvShadow));
            fixed edge = 1.0 - saturate(abs(p.r - 0.1) * 15);
            if (i.uvShadow.x <= 0 || i.uvShadow.y <= 0 || i.uvShadow.x >= 1 || i.uvShadow.y >= 1)
            {
               edge = 0;
            }
            edge *= 2;
            return fixed4(_BrushColor, edge);
         }
         ENDCG
      }
   }
}