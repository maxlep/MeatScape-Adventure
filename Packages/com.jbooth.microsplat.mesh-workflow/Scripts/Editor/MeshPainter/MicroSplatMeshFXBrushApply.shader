Shader "Hidden/MicroSplatMeshFXBrushApply"
{
   Properties
   {
      _MainTex ("Texture", 2D) = "white" {}
   }
   SubShader
   {
      Tags { "RenderType"="Opaque" }
      LOD 100

      Pass
      {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         
         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
         };

         struct v2f
         {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
         };

         sampler2D _MainTex;
         float4 _MainTex_ST;
         sampler2D _BrushBuffer;
         float _BrushFlow;
         int _channel;
         float2 _EdgeBuffer;
         float _TargetValue;
         half3 _TargetColor;

         int _ControlIndex;   // texture we are modifying
         sampler2D _Control0;
         
         v2f vert (appdata v)
         {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
         }
         
         fixed4 frag (v2f i) : SV_Target
         {
            fixed4 col = tex2D(_Control0, i.uv);

            // sample brush
            fixed4 brushSample = tex2D(_BrushBuffer, i.uv);
            fixed brush = brushSample.r;
            fixed edge = brushSample.g;

            brush *= _BrushFlow;
            brush = saturate(brush);

   
            float offset = _EdgeBuffer * 0.5;
            fixed boost = tex2D(_BrushBuffer, i.uv + float2(offset, offset)).r +
                          tex2D(_BrushBuffer, i.uv + float2(-offset, -offset)).r +
                          tex2D(_BrushBuffer, i.uv + float2(offset, -offset)).r +
                          tex2D(_BrushBuffer, i.uv + float2(-offset, offset)).r;


            
            if (edge < 0.5 && boost > 0.5)
            {
               brush = boost/4.0;
            }

            float thresh = 1 / 255.0;

            if (brush > thresh)
            {
               if (_channel == 0)
               {
                  col.r = lerp(col.r, _TargetValue, brush);
               }
               else if (_channel == 1)
               {
                  col.g = lerp(col.g, _TargetValue, brush);
               }
               else if (_channel == 2)
               {
                  col.b = lerp(col.b, _TargetValue, brush);
               }
               else if (_channel == 3)
               {
                  col.a = lerp(col.a, _TargetValue, brush);
               }
               else if (_channel == 4) // displacement dampen
               {
                  col.g = lerp(col.g, _TargetValue, brush);
               }
               else if (_channel == 5)
               {
                  col.rgb = lerp(col.rgb, _TargetColor, brush);
               }
            }


            col = saturate(col);

            return col;
            
         }
         ENDCG
      }
   }
}
