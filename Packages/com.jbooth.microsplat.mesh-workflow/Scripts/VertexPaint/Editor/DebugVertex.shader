Shader "Custom/DebugVertex" {
	Properties {
		_Channel ("Metallic", Int) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		int _Channel;

		struct Input {
			fixed4 w0, w7;
		};

      float4 DecodeToFloat4(float v)
      {
         uint vi = (uint)(v * (256.0f * 256.0f * 256.0f * 256.0f));
         int ex = (int)(vi / (256 * 256 * 256) % 256);
         int ey = (int)((vi / (256 * 256)) % 256);
         int ez = (int)((vi / (256)) % 256);
         int ew = (int)(vi % 256);
         float4 e = float4(ex / 255.0, ey / 255.0, ez / 255.0, ew / 255.0);
         return e;
      }
      
      void EncodeVertex(appdata_full i, inout Input IN)
      {
         IN.w0 = DecodeToFloat4(i.color.r);
         //IN.w1 = DecodeToFloat4(i.color.g);
         //IN.w2 = DecodeToFloat4(i.color.b);
         //IN.w3 = DecodeToFloat4(i.color.a);
         //IN.w4 = DecodeToFloat4(i.texcoord1.z);
         //IN.w5 = DecodeToFloat4(i.texcoord1.w);
         //IN.w6 = DecodeToFloat4(i.texcoord2.z);
         IN.w7 = DecodeToFloat4(i.texcoord2.w);
      }

      void vert(inout appdata_full v, out Input i)
      {
         i = (Input)0;
         EncodeVertex(v, i);
      }
    
		void surf (Input i, inout SurfaceOutputStandard o) 
      {
			int c = clamp(_Channel, 0, 7);
         float fs[8];
         fs[0] = i.w0.x;
         fs[1] = i.w0.y;
         fs[2] = i.w0.z;
         fs[3] = i.w0.w;
         fs[4] = i.w7.x;
         fs[5] = i.w7.y;
         fs[6] = i.w7.z;
         fs[7] = i.w7.w;
         

         float f = fs[c];
         o.Albedo = f;
         
		}
		ENDCG
	}
	FallBack "Diffuse"
}
