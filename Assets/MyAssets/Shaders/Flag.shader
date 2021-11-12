// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Flag"
{
	Properties
	{
		_AlbedoTexture("AlbedoTexture", 2D) = "white" {}
		_DisplacementStrength("DisplacementStrength", Float) = 1
		_MaskStartU("Mask Start U", Float) = 0
		_MaskEndU("Mask End U", Float) = 1
		_WaveSpeed("WaveSpeed", Float) = 1
		_WaveScale("WaveScale", Float) = 1
		_SmallWaveScale("SmallWaveScale", Float) = 0.2
		_SmallWaveStrength("SmallWaveStrength", Float) = 0.2
		_WaveStrength("WaveStrength", Float) = 1
		_WaveNoiseStrength("WaveNoiseStrength", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
		#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex.SampleBias(samplerTex,coord,bias)
		#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex.SampleGrad(samplerTex,coord,ddx,ddy)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
		#define SAMPLE_TEXTURE2D_BIAS(tex,samplerTex,coord,bias) tex2Dbias(tex,float4(coord,0,bias))
		#define SAMPLE_TEXTURE2D_GRAD(tex,samplerTex,coord,ddx,ddy) tex2Dgrad(tex,coord,ddx,ddy)
		#endif//ASE Sampling Macros

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _MaskStartU;
		uniform float _MaskEndU;
		uniform float _WaveSpeed;
		uniform float _WaveNoiseStrength;
		uniform float _WaveStrength;
		uniform float _WaveScale;
		uniform float _SmallWaveScale;
		uniform float _SmallWaveStrength;
		uniform float _DisplacementStrength;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_AlbedoTexture);
		uniform float4 _AlbedoTexture_ST;
		SamplerState sampler_AlbedoTexture;


		float4 mod289( float4 x )
		{
			return x - floor(x * (1.0 / 289.0)) * 289.0;
		}


		float4 perm( float4 x )
		{
			return mod289(((x * 34.0) + 1.0) * x);
		}


		float SimpleNoise3D( float3 p )
		{
			    float3 a = floor(p);
			    float3 d = p - a;
			    d = d * d * (3.0 - 2.0 * d);
			    float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
			    float4 k1 = perm(b.xyxy);
			    float4 k2 = perm(k1.xyxy + b.zzww);
			    float4 c = k2 + a.zzzz;
			    float4 k3 = perm(c);
			    float4 k4 = perm(c + 1.0);
			    float4 o1 = frac(k3 * (1.0 / 41.0));
			    float4 o2 = frac(k4 * (1.0 / 41.0));
			    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
			    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
			    return o4.y * d.y + o4.x * (1.0 - d.y);
		}


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1);
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1);
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float ClothMask35 = ( (0.0 + (v.texcoord.xy.x - _MaskStartU) * (1.0 - 0.0) / (_MaskEndU - _MaskStartU)) - 0.1 );
			float mulTime24 = _Time.y * _WaveSpeed;
			float temp_output_26_0 = ( mulTime24 + ClothMask35 );
			float temp_output_71_0 = ( unity_WorldTransformParams.x + unity_WorldTransformParams.y + unity_WorldTransformParams.z );
			float PositionSeed70 = temp_output_71_0;
			float temp_output_59_0 = ( temp_output_26_0 + PositionSeed70 );
			float4 appendResult101 = (float4(temp_output_59_0 , v.texcoord.xy.y , 0.0 , 0.0));
			float3 p1_g32 = appendResult101.xyz;
			float localSimpleNoise3D1_g32 = SimpleNoise3D( p1_g32 );
			float temp_output_84_0 = localSimpleNoise3D1_g32;
			Gradient gradient85 = NewGradient( 0, 5, 2, float4( 0, 0, 0, 0.1000076 ), float4( 1, 1, 1, 0.2382391 ), float4( 0.3, 0.3, 0.3, 0.5000076 ), float4( 1, 1, 1, 0.7088273 ), float4( 0, 0, 0, 0.8 ), 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float temp_output_83_0 = ( sin( ( temp_output_59_0 * _SmallWaveScale ) ) * _SmallWaveStrength * SampleGradient( gradient85, ( temp_output_26_0 % 1.0 ) ).r );
			float4 appendResult11 = (float4(0.0 , ( ( ClothMask35 * ( ( ( temp_output_84_0 * _WaveNoiseStrength ) * ( _WaveStrength * sin( ( temp_output_59_0 * _WaveScale ) ) ) ) + temp_output_83_0 ) ) * _DisplacementStrength ) , ( ClothMask35 * -1.0 * temp_output_83_0 ) , 0.0));
			v.vertex.xyz += appendResult11.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_AlbedoTexture = i.uv_texcoord * _AlbedoTexture_ST.xy + _AlbedoTexture_ST.zw;
			o.Albedo = SAMPLE_TEXTURE2D( _AlbedoTexture, sampler_AlbedoTexture, uv_AlbedoTexture ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;73;3440;270;3814.086;132.3045;2.658709;True;False
Node;AmplifyShaderEditor.CommentaryNode;56;-2296.327,-572.3912;Inherit;False;1203.264;465.3138;UV Displacement Strength Mask;6;44;45;43;6;21;35;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-2245.327,-360.369;Inherit;False;Property;_MaskStartU;Mask Start U;3;0;Create;True;0;0;False;0;False;0;0.617;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-2242.011,-510.3912;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;-2246.327,-232.3689;Inherit;False;Property;_MaskEndU;Mask End U;4;0;Create;True;0;0;False;0;False;1;0.633;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;72;-2301.693,-62.93018;Inherit;False;1205.714;305.6507;World Position Seed;5;71;69;70;108;109;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;43;-1957.327,-361.369;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldTransformParams;69;-2273.693,-1.832256;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;21;-1605.001,-361.0774;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-2012.47,-0.7180901;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-1317.063,-359.9654;Inherit;False;ClothMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-2046.39,384.1937;Inherit;False;Property;_WaveSpeed;WaveSpeed;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-1790.045,640.6475;Inherit;False;35;ClothMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;24;-1790.861,386.675;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1296.556,-6.684526;Inherit;False;PositionSeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1533.232,385.4481;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-1535.844,641.0462;Inherit;False;70;PositionSeed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-1148.589,639.7617;Inherit;False;Property;_WaveScale;WaveScale;6;0;Create;True;0;0;False;0;False;1;3.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;102;-1052.277,64.87894;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1212.516,384.0256;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-895.02,383.8302;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;101;-769.8712,66.32658;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-1149.957,767.4014;Inherit;False;Property;_SmallWaveScale;SmallWaveScale;7;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-896.3875,642.2717;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-603.7629,265.3061;Inherit;False;Property;_WaveStrength;WaveStrength;9;0;Create;True;0;0;False;0;False;1;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-606.3285,99.85363;Inherit;False;Property;_WaveNoiseStrength;WaveNoiseStrength;10;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;90;-1248.035,933.3003;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;27;-603.202,382.4304;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;85;-989.8327,928.0794;Inherit;False;0;5;2;0,0,0,0.1000076;1,1,1,0.2382391;0.3,0.3,0.3,0.5000076;1,1,1,0.7088273;0,0,0,0.8;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.FunctionNode;84;-605.1667,-126.3571;Inherit;True;Simple Noise 3D;-1;;32;af06c8bfeddda644eae2803374c9c63b;0;1;4;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-632.3451,895.2877;Inherit;False;Property;_SmallWaveStrength;SmallWaveStrength;8;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-378.4119,91.84457;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-383.3771,385.874;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;87;-724.0588,1000.351;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;80;-604.3636,640.6357;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-64.54771,353.9511;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-384.0572,639.4186;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;81;260.6179,510.8304;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;240.8706,183.1224;Inherit;False;35;ClothMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;521.3456,254.9842;Inherit;True;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;548.4258,602.5759;Inherit;False;35;ClothMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;992.8647,-318.6853;Inherit;False;Property;_DisplacementStrength;DisplacementStrength;1;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;828.9006,674.4377;Inherit;True;3;3;0;FLOAT;1;False;1;FLOAT;-1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;1378.948,-320.0676;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;105;-96.64647,119.7605;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;375.7247,-222.0148;Inherit;False;Property;_GravityMultiplier;GravityMultiplier;2;0;Create;True;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;1761.576,-254.8457;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.InstanceIdNode;110;-1788.608,770.2606;Inherit;False;0;1;INT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;109;-1488.29,53.85141;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;37;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-1729.426,58.06586;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;179;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;993.6234,-572.7232;Inherit;True;Property;_AlbedoTexture;AlbedoTexture;0;0;Create;True;0;0;False;0;False;-1;None;b88546e5b4d243b429b4b53800f5a6b7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;107;-1532.63,769.3992;Inherit;False;Random Range;-1;;34;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2014.81,-576.1918;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Flag;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;43;0;6;1
WireConnection;43;1;45;0
WireConnection;43;2;44;0
WireConnection;21;0;43;0
WireConnection;71;0;69;1
WireConnection;71;1;69;2
WireConnection;71;2;69;3
WireConnection;35;0;21;0
WireConnection;24;0;55;0
WireConnection;70;0;71;0
WireConnection;26;0;24;0
WireConnection;26;1;65;0
WireConnection;59;0;26;0
WireConnection;59;1;73;0
WireConnection;63;0;59;0
WireConnection;63;1;58;0
WireConnection;101;0;59;0
WireConnection;101;1;102;2
WireConnection;79;0;59;0
WireConnection;79;1;78;0
WireConnection;90;0;26;0
WireConnection;27;0;63;0
WireConnection;84;4;101;0
WireConnection;103;0;84;0
WireConnection;103;1;104;0
WireConnection;99;0;98;0
WireConnection;99;1;27;0
WireConnection;87;0;85;0
WireConnection;87;1;90;0
WireConnection;80;0;79;0
WireConnection;106;0;103;0
WireConnection;106;1;99;0
WireConnection;83;0;80;0
WireConnection;83;1;82;0
WireConnection;83;2;87;1
WireConnection;81;0;106;0
WireConnection;81;1;83;0
WireConnection;8;0;36;0
WireConnection;8;1;81;0
WireConnection;113;0;112;0
WireConnection;113;2;83;0
WireConnection;16;0;8;0
WireConnection;16;1;41;0
WireConnection;105;0;84;0
WireConnection;11;1;16;0
WireConnection;11;2;113;0
WireConnection;109;0;108;0
WireConnection;108;0;71;0
WireConnection;0;0;2;0
WireConnection;0;11;11;0
ASEEND*/
//CHKSM=616AEF849A92B08B39EA237B74C2C22D1805F673