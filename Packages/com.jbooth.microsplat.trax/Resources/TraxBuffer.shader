Shader "Hidden/MicroSplat/TraxBuffer"
{
	Properties
	{
      _MainTex("MainTex", 2D) = "white" {}

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
            float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
			};
         
         float2 _Offset;
         
         sampler2D_float _MainTex;
         sampler2D_float _DepthRT;
         float4 _MainTex_TexelSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
				return o;
			}

         float _UseTime;
         float _RepairDelay;
         float _RepairRate;
         float _RepairTotal;
         float _BufferBlend;
         float _SinkStrength;

         float _CamCaptureHeight;
         float _CamFarClipPlane;
      
			
			float4 frag (v2f i) : SV_Target
			{
            float2 uv = i.uv;
            uv -= _Offset;
            
            float2 old = tex2D(_MainTex, uv).rg;
            
            float2 rtUV = uv;
            rtUV.y = 1 - rtUV.y;

            float depth = _CamCaptureHeight + ((1 - tex2D(_DepthRT, rtUV).r) * _CamFarClipPlane);
            

            // filter depth information to avoid infinite pixels. This transports the data to edge pixels
            float offset = _MainTex_TexelSize.x * 0.5;
            float2 old1 = tex2D(_MainTex, uv - float2(offset, 0)).rg;
            float2 old2 = tex2D(_MainTex, uv + float2(offset, 0)).rg;
            float2 old3 = tex2D(_MainTex, uv - float2(0, offset)).rg;
            float2 old4 = tex2D(_MainTex, uv + float2(0, offset)).rg;
            float2 old5 = tex2D(_MainTex, uv - float2(-offset, offset)).rg;
            float2 old6 = tex2D(_MainTex, uv + float2(offset, offset)).rg;
            float2 old7 = tex2D(_MainTex, uv - float2(-offset, -offset)).rg;
            float2 old8 = tex2D(_MainTex, uv + float2(offset, -offset)).rg;

            float thresh = _BufferBlend * (2048/_MainTex_TexelSize.z);
			   float bf = thresh * 0.3;

            float mn = min(min(min(min(min(min(min(old1.r, old2.r), old3.r), old4.r), old5.r + bf), old6.r + bf), old7.r + bf), old8.r + bf);
            if (mn+thresh < old.r)
            {
               old.r = mn + thresh;
               old.g = max(max(max(max(max(max(max(old1.g, old2.g), old3.g), old4.g), old5.g), old6.g), old7.g), old8.g);
            }

            if (depth < old.r)
            {
			      // don't sink too fast
               old.r = lerp(min(old.r, depth + 1), depth, _SinkStrength);
               old.g = _Time.y;
            }
            // repair stuff
            if (_UseTime > 0.5 && _RepairRate > 0)
            {
               // past begin
               if (_RepairDelay > 0 || _RepairTotal > 0)
               {
                  float dt = _Time.y - old.g;
                  // in repair window
                  if (dt > _RepairDelay && (dt <= _RepairTotal + _RepairDelay || _RepairTotal <= 0))
                  {
                     old.r += unity_DeltaTime * _RepairRate;
                  }
               }
               else
               {
                  old.r += unity_DeltaTime * _RepairRate;
               }

            }
            
            // clear edge, so that it doesn't smear tracks forever
            if (uv.x != saturate(uv).x || uv.y != saturate(uv.y))
            {
               old.r = 99999;
               old.g = _Time.y;
            }

				return float4(old.r, old.g, 0, 1); 
			}
			ENDCG
		}
	}
}
