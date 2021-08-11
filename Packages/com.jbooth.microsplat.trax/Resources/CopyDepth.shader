Shader "Hidden/MicroSplat/CopyDepth"
{
	Properties
	{

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
         
			sampler2D_float _DepthRT;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			float _CamCaptureHeight;
			float _CamFarClipPlane;
			
			float frag (v2f i) : SV_Target
			{
				float depth = _CamCaptureHeight + ((1 - tex2D(_DepthRT, i.uv).r) * _CamFarClipPlane);
				return depth;
			}
			ENDCG
		}
	}
}
