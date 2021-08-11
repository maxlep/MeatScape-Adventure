
Shader "Hidden/MicroSplatMeshBrush"
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
            float3 normal : NORMAL;
            float2 uv     : TEXCOORD0;
            float2 uv2    : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex           : SV_POSITION;
            float3 normal           : TEXCOORD0;
            float4 brushClipSpace   : TEXCOORD1;
            float3 worldPos         : TEXCOORD2;
			};

			sampler2D _MainTex;
         float4x4 _BrushWorldToProjMtx;
         float4x4 _Obj2World;
         float4x4 _Obj2Scale;
         float useUV2;
         float3 _MouseWorldPos;
         float2 _AngleFilter;
         float _IsFill;
         float4 _UVMeshRange;
         float _ProjectionFilter;
         float _BrushSize;

         half  InverseLerp(half x, half y, half v) { return (v-x)/max(y-x, 0.001); }
         half2 InverseLerp(half2 x, half2 y, half2 v) { return (v-x)/max(y-x, half2(0.001, 0.001)); }
         half3 InverseLerp(half3 x, half3 y, half3 v) { return (v-x)/max(y-x, half3(0.001, 0.001, 0.001)); }
         half4 InverseLerp(half4 x, half4 y, half4 v) { return (v-x)/max(y-x, half4(0.001, 0.001, 0.001, 0.001)); }
			
			v2f vert (appdata v)
			{
				v2f o;
            float2 uv = v.uv;
            if (useUV2 > 0.5)
            {
               uv = v.uv2;
            }

            uv = InverseLerp(_UVMeshRange.xy, _UVMeshRange.zw, uv); // remap into range

            #if UNITY_UV_STARTS_AT_TOP
            uv.y = 1 - uv.y;
            #endif
            uv = uv * 2 - 1;
            o.vertex = float4(uv.xy, 0, 1); 

            // Use the world position to set the texture tiling projection
            float4 worldPos = mul(_Obj2World, mul(_Obj2Scale, v.vertex));
            o.normal = mul(_Obj2World, float4(v.normal, 0)).xyz;
            // Also use the world position to project to the brush camera space
            o.brushClipSpace = mul(_BrushWorldToProjMtx, worldPos);
            o.worldPos = worldPos.xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
            // Now, convert the brush clip space to UV coordinate space...
            // Divide by w to which is the perspective divide
            float4 brushClipSpace = i.brushClipSpace / i.brushClipSpace.wwww;

            // Space is from -1 to +1, remap it
            float2 brushUV = (brushClipSpace.xy + 1) / 2.0;
           

            float4 brushTex = tex2D(_MainTex, brushUV);
            brushTex.a = 1;

            //float face = (dot(normalize(i.normal), normalize(_WorldSpaceCameraPos - i.worldPos)));
            //if (_ProjectionFilter <= 1)
            //{
            //   face = abs(face);
            //}


            float str = 1;

            if (_IsFill > 0.5)
            {
               brushTex.rgb = 1;
               str = 1;
            }

            float ang = dot(i.normal, float3(0,1,0));
            if (ang < _AngleFilter.x || ang > _AngleFilter.y)
               str = 0;
         
            // distance falloff (sharp, it's more of a filter)
            float dist = distance(_MouseWorldPos, i.worldPos);
            if (dist > _BrushSize)
            {
               str = 0;
            }

            brushTex.rgb *= str;
            brushTex.g = 1;
            return brushTex;
				
			}
			ENDCG
		}
	}
}

