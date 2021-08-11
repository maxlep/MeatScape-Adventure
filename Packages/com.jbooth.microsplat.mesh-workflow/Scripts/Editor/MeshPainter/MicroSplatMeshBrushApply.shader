﻿Shader "Hidden/MicroSplatMeshBrushApply"
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
         float _BrushTarget;
         int _channel;
         float2 _EdgeBuffer;

         int _ControlIndex;   // texture we are modifying
         sampler2D _Control0;
         sampler2D _Control1;
         sampler2D _Control2;
         sampler2D _Control3;
         sampler2D _Control4;
         sampler2D _Control5;
         sampler2D _Control6;
         sampler2D _Control7;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
            
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
            fixed4 controlBuffer[8];
            controlBuffer[0] = tex2D(_Control0, i.uv);
            controlBuffer[1] = tex2D(_Control1, i.uv);
            controlBuffer[2] = tex2D(_Control2, i.uv);
            controlBuffer[3] = tex2D(_Control3, i.uv);
            controlBuffer[4] = tex2D(_Control4, i.uv);
            controlBuffer[5] = tex2D(_Control5, i.uv);
            controlBuffer[6] = tex2D(_Control6, i.uv);
            controlBuffer[7] = tex2D(_Control7, i.uv);

            fixed4 col = controlBuffer[_ControlIndex];

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
                  if (col.r > _BrushTarget) 
                  { 
                     col.r -= brush;
                     col.r = max(col.r, _BrushTarget);
                  }
                  else
                  {
                     col.r += brush;
                     col.r = min(col.r, _BrushTarget);
                  }
               }
               else if (_channel == 1)
               {
                  if (col.g > _BrushTarget) 
                  { 
                     col.g -= brush;
                     col.g = max(col.g, _BrushTarget);
                  }
                  else
                  {
                     col.g += brush;
                     col.g = min(col.g, _BrushTarget);
                  }
               }
               else if (_channel == 2)
               {
                  if (col.b > _BrushTarget) 
                  { 
                     col.b -= brush;
                     col.b = max(col.b, _BrushTarget);
                  }
                  else
                  {
                     col.b += brush;
                     col.b = min(col.b, _BrushTarget);
                  }
               }
               else if (_channel == 3)
               {
                  if (col.a > _BrushTarget) 
                  { 
                     col.a -= brush;
                     col.a = max(col.a, _BrushTarget);
                  }
                  else
                  {
                     col.a += brush;
                     col.a = min(col.a, _BrushTarget);
                  }
               }
            }

            return col;
				
			}
			ENDCG
		}
	}
}
