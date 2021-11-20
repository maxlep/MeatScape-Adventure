// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Eye"
{
	Properties
	{
		_FresnelPower("FresnelPower", Float) = 3
		_FresnelScale("FresnelScale", Float) = 1
		_FresnelBias("FresnelBias", Float) = 0
		_TimeScale("TimeScale", Float) = 0
		_SimpleNoiseTiling("SimpleNoiseTiling", Float) = 0
		_SimpleNoiseStrength("SimpleNoiseStrength", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
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

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 viewDir;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _TimeScale;
		uniform float _SimpleNoiseTiling;
		uniform float _SimpleNoiseStrength;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color70 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV1 = dot( ase_worldNormal, i.viewDir );
			float fresnelNode1 = ( _FresnelBias + _FresnelScale * pow( 1.0 - fresnelNdotV1, _FresnelPower ) );
			float mulTime2_g1 = _Time.y * _TimeScale;
			float2 temp_cast_0 = (_SimpleNoiseTiling).xx;
			float2 uv_TexCoord96 = i.uv_texcoord * temp_cast_0;
			float3 p1_g2 = float3( uv_TexCoord96 ,  0.0 );
			float localSimpleNoise3D1_g2 = SimpleNoise3D( p1_g2 );
			float temp_output_45_0 = step( fresnelNode1 , ( ( sin( ( ( ( atan2( i.uv_texcoord.x , i.uv_texcoord.y ) / ( 2.0 * UNITY_PI ) ) * ( 81.4 * UNITY_PI ) ) + mulTime2_g1 ) ) * 0.02 ) + 0.09 + (0.0 + (localSimpleNoise3D1_g2 - 0.0) * (_SimpleNoiseStrength - 0.0) / (1.0 - 0.0)) ) );
			float4 color14 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
			float temp_output_66_0 = (-0.5 + (_SinTime.w - -0.99) * (1.0 - -0.5) / (-0.95 - -0.99));
			float temp_output_80_0 = pow( max( 0.0 , ( ( abs( temp_output_66_0 ) * 2.0 ) - 1.0 ) ) , 3.0 );
			float temp_output_60_0 = step( ( ( 1.0 - distance( i.uv_texcoord.y , 0.0 ) ) * 0.5 ) , ( 0.0 + (0.0 + (temp_output_80_0 - 0.0) * (0.35 - 0.0) / (1.0 - 0.0)) ) );
			float4 color71 = IsGammaSpace() ? float4(0.7921569,0.3411765,0.3411765,1) : float4(0.5906189,0.0953075,0.0953075,1);
			o.Albedo = ( ( ( ( color70 * temp_output_45_0 ) + ( color14 * ( 1.0 - temp_output_45_0 ) ) ) * temp_output_60_0 ) + ( color71 * ( 1.0 - temp_output_60_0 ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;0;1920;1139;2690.162;-243.9098;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-2783.506,550.441;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;65;-2692.436,1425.331;Inherit;True;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;-2599.506,818.441;Inherit;False;Constant;_Diameter;Diameter;2;0;Create;True;0;0;False;0;False;81.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ATan2OpNode;33;-2496.506,569.441;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;34;-2555.506,701.441;Inherit;False;1;0;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;66;-2426.436,1437.331;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;-0.99;False;2;FLOAT;-0.95;False;3;FLOAT;-0.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-2276.175,915.3951;Inherit;False;Property;_TimeScale;TimeScale;3;0;Create;True;0;0;False;0;False;0;5.84;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;36;-2354.506,585.441;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;35;-2435.506,817.441;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;92;-2082.175,860.3951;Inherit;False;Get Time;-1;;1;9d5093cc36f8a0247895d5b90bfbdf31;0;1;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-2350.162,1023.91;Inherit;False;Property;_SimpleNoiseTiling;SimpleNoiseTiling;4;0;Create;True;0;0;False;0;False;0;7.54;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;82;-1890.105,1479.118;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-2148.506,563.441;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-1743.225,606.1827;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;96;-2140.162,966.9098;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1750.105,1479.118;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;84;-1590.105,1483.118;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-1883.162,702.9098;Inherit;False;Property;_SimpleNoiseStrength;SimpleNoiseStrength;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;94;-1826.162,783.9098;Inherit;True;Simple Noise 3D;-1;;2;af06c8bfeddda644eae2803374c9c63b;0;1;4;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;23;-1541.506,485.441;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;51;-2176.042,1150.971;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;88;-1859.346,440.9918;Inherit;False;Property;_FresnelPower;FresnelPower;0;0;Create;True;0;0;False;0;False;3;2.55;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;6;-1687.368,-14.44167;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMaxOpNode;81;-1428.105,1483.118;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-1659.368,160.5584;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;86;-1863.899,257.6955;Inherit;False;Property;_FresnelBias;FresnelBias;2;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1302.308,745.7157;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;False;0.09;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1327.523,538.5286;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.02;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;95;-1594.162,690.9098;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-1861.622,351.0513;Inherit;False;Property;_FresnelScale;FresnelScale;1;0;Create;True;0;0;False;0;False;1;2.69;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;80;-1286.105,1483.118;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-1122.7,688.0515;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;1;-1382.368,99.55836;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.02;False;2;FLOAT;3.5;False;3;FLOAT;3.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;56;-1680.436,877.3309;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;76;-1095.105,1288.118;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.35;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;58;-1519.436,839.3309;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;45;-921.6601,549.2665;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;-449.5713,-117.1401;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-964.292,981.951;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1355.436,839.3309;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-671.1187,-415.4674;Inherit;False;Constant;_Color1;Color 1;1;0;Create;True;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;70;-971.3102,-108.6726;Inherit;False;Constant;_Color0;Color 0;1;0;Create;True;0;0;False;0;False;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-246.172,-262.0239;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;60;-802.4363,871.3309;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-404.5104,173.4273;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;73;-384.6939,830.8197;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;71;-573.5267,441.8058;Inherit;False;Constant;_Color2;Color 2;1;0;Create;True;0;0;False;0;False;0.7921569,0.3411765,0.3411765,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;20;58.93331,-198.0073;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-54.89391,702.9196;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;320.7638,30.13094;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1240.115,1073.428;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.16;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;505.4061,102.3198;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1491.36,1292.708;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;False;0.23;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;50;-1456.105,1064.059;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;85;-2091.105,1563.118;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;54;-1923.005,1200.295;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;79;-1117.105,1483.118;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1697.005,1100.295;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;826.8879,-35.34284;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Eye;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;33;0;27;1
WireConnection;33;1;27;2
WireConnection;66;0;65;4
WireConnection;36;0;33;0
WireConnection;36;1;34;0
WireConnection;35;0;37;0
WireConnection;92;4;93;0
WireConnection;82;0;66;0
WireConnection;38;0;36;0
WireConnection;38;1;35;0
WireConnection;90;0;38;0
WireConnection;90;1;92;0
WireConnection;96;0;97;0
WireConnection;83;0;82;0
WireConnection;84;0;83;0
WireConnection;94;4;96;0
WireConnection;23;0;90;0
WireConnection;81;1;84;0
WireConnection;46;0;23;0
WireConnection;95;0;94;0
WireConnection;95;4;98;0
WireConnection;80;0;81;0
WireConnection;48;0;46;0
WireConnection;48;1;47;0
WireConnection;48;2;95;0
WireConnection;1;0;6;0
WireConnection;1;4;7;0
WireConnection;1;1;86;0
WireConnection;1;2;87;0
WireConnection;1;3;88;0
WireConnection;56;0;51;2
WireConnection;76;0;80;0
WireConnection;58;0;56;0
WireConnection;45;0;1;0
WireConnection;45;1;48;0
WireConnection;13;0;45;0
WireConnection;63;1;76;0
WireConnection;59;0;58;0
WireConnection;49;0;14;0
WireConnection;49;1;13;0
WireConnection;60;0;59;0
WireConnection;60;1;63;0
WireConnection;69;0;70;0
WireConnection;69;1;45;0
WireConnection;73;0;60;0
WireConnection;20;0;69;0
WireConnection;20;1;49;0
WireConnection;75;0;71;0
WireConnection;75;1;73;0
WireConnection;68;0;20;0
WireConnection;68;1;60;0
WireConnection;62;0;50;0
WireConnection;62;1;72;0
WireConnection;74;0;68;0
WireConnection;74;1;75;0
WireConnection;50;0;53;0
WireConnection;85;0;66;0
WireConnection;79;0;80;0
WireConnection;53;0;51;1
WireConnection;53;1;54;0
WireConnection;0;0;74;0
ASEEND*/
//CHKSM=2711C5BB39A3A921C04FE4FF301E8A0F886395C6