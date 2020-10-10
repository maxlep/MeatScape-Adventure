// Upgrade NOTE: upgraded instancing buffer 'MeatClump' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MeatClump"
{
	Properties
	{
		_AmbientFactor2("AmbientFactor", Range( 0 , 100)) = 27.34496
		_AO2("AO", 2D) = "bump" {}
		_MinimumAmbient2("MinimumAmbient", Color) = (0.4172381,0.4092143,0.674,1)
		_BaseColor1("BaseColor", Color) = (1,1,1,1)
		_RimColor1("RimColor", Color) = (1,1,1,1)
		_Float1("Float 0", Float) = 0
		_Float2("Float 1", Float) = 1
		_Float3("Float 2", Float) = 5
		_Float4("Float 3", Float) = -5.93
		_AOFactor2("AO Factor", Range( 0 , 1)) = 0
		_SplatSpread("SplatSpread", Range( 0 , 2)) = 0.5
		_FadeFac("FadeFac", Range( 0 , 1)) = 0
		_SplatFac("SplatFac", Range( 0 , 0.99)) = 0
		_SplatNormal("SplatNormal", Vector) = (0,0,1,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
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
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _SplatSpread;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_AO2);
		SamplerState sampler_AO2;
		uniform float _AOFactor2;
		uniform float _AmbientFactor2;
		uniform float4 _MinimumAmbient2;
		uniform float4 _BaseColor1;
		uniform float4 _RimColor1;
		uniform float _Float1;
		uniform float _Float2;
		uniform float _Float3;
		uniform float _Float4;

		UNITY_INSTANCING_BUFFER_START(MeatClump)
			UNITY_DEFINE_INSTANCED_PROP(float4, _AO2_ST)
#define _AO2_ST_arr MeatClump
			UNITY_DEFINE_INSTANCED_PROP(float3, _SplatNormal)
#define _SplatNormal_arr MeatClump
			UNITY_DEFINE_INSTANCED_PROP(float, _SplatFac)
#define _SplatFac_arr MeatClump
			UNITY_DEFINE_INSTANCED_PROP(float, _FadeFac)
#define _FadeFac_arr MeatClump
		UNITY_INSTANCING_BUFFER_END(MeatClump)


		float2 voronoihash2( float2 p )
		{
			p = p - 11 * floor( p / 11 );
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi2( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash2( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return F1;
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


		float2 voronoihash17( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi17( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash17( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return F1;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float time2 = 0.0;
			float2 coords2 = v.texcoord.xy * 10.41;
			float2 id2 = 0;
			float2 uv2 = 0;
			float voroi2 = voronoi2( coords2, time2, id2, uv2, 0 );
			float4 appendResult27 = (float4(0.0 , 0.0 , ( voroi2 * -1.0 ) , 0.0));
			float _SplatFac_Instance = UNITY_ACCESS_INSTANCED_PROP(_SplatFac_arr, _SplatFac);
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 _SplatNormal_Instance = UNITY_ACCESS_INSTANCED_PROP(_SplatNormal_arr, _SplatNormal);
			float4 transform51 = mul(unity_WorldToObject,float4( _SplatNormal_Instance , 0.0 ));
			float4 temp_output_6_0_g1 = transform51;
			float dotResult1_g1 = dot( float4( ase_vertex3Pos , 0.0 ) , temp_output_6_0_g1 );
			float dotResult2_g1 = dot( temp_output_6_0_g1 , temp_output_6_0_g1 );
			float4 temp_output_34_0 = ( ( dotResult1_g1 / dotResult2_g1 ) * temp_output_6_0_g1 );
			float4 temp_output_58_0 = ( float4( ase_vertex3Pos , 0.0 ) - temp_output_34_0 );
			float4 normalizeResult59 = normalize( temp_output_58_0 );
			float _FadeFac_Instance = UNITY_ACCESS_INSTANCED_PROP(_FadeFac_arr, _FadeFac);
			v.vertex.xyz += ( ( ( appendResult27 * (0.5 + (( 1.0 - _SplatFac_Instance ) - 0.0) * (1.0 - 0.5) / (1.0 - 0.0)) ) + ( distance( temp_output_34_0 , ( -1.0 * 1.0 * transform51 ) ) * transform51 * _SplatFac_Instance * -1.0 ) + ( normalizeResult59 * length( temp_output_58_0 ) * _SplatFac_Instance * _SplatSpread ) ) + ( transform51 * _FadeFac_Instance * 1.0 * -1.0 ) ).xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			float3 temp_output_16_0_g5 = ( ase_worldPos * 100.0 );
			float3 crossY18_g5 = cross( ase_worldNormal , ddy( temp_output_16_0_g5 ) );
			float3 worldDerivativeX2_g5 = ddx( temp_output_16_0_g5 );
			float dotResult6_g5 = dot( crossY18_g5 , worldDerivativeX2_g5 );
			float crossYDotWorldDerivX34_g5 = abs( dotResult6_g5 );
			float time2 = 0.0;
			float2 coords2 = i.uv_texcoord * 10.41;
			float2 id2 = 0;
			float2 uv2 = 0;
			float voroi2 = voronoi2( coords2, time2, id2, uv2, 0 );
			float temp_output_3_0 = ( 1.0 - voroi2 );
			float temp_output_4_0 = ( temp_output_3_0 * temp_output_3_0 );
			float temp_output_20_0_g5 = ( (0.2 + (temp_output_4_0 - 0.0) * (0.55 - 0.2) / (1.0 - 0.0)) * 6.0 );
			float3 crossX19_g5 = cross( ase_worldNormal , worldDerivativeX2_g5 );
			float3 break29_g5 = ( sign( crossYDotWorldDerivX34_g5 ) * ( ( ddx( temp_output_20_0_g5 ) * crossY18_g5 ) + ( ddy( temp_output_20_0_g5 ) * crossX19_g5 ) ) );
			float3 appendResult30_g5 = (float3(break29_g5.x , -break29_g5.y , break29_g5.z));
			float3 normalizeResult39_g5 = normalize( ( ( crossYDotWorldDerivX34_g5 * ase_worldNormal ) - appendResult30_g5 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir42_g5 = mul( ase_worldToTangent, normalizeResult39_g5);
			o.Normal = worldToTangentDir42_g5;
			Gradient gradient11 = NewGradient( 0, 3, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 0.2 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 _AO2_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_AO2_ST_arr, _AO2_ST);
			float2 uv_AO2 = i.uv_texcoord * _AO2_ST_Instance.xy + _AO2_ST_Instance.zw;
			float4 ao24_g4 = ( 1.0 - ( ( 1.0 - SampleGradient( gradient11, UnpackNormal( SAMPLE_TEXTURE2D( _AO2, sampler_AO2, uv_AO2 ) ).x ) ) * _AOFactor2 ) );
			float4 lightAmbient14_g4 = ( _AmbientFactor2 * UNITY_LIGHTMODEL_AMBIENT );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult9_g4 = dot( ase_worldNormal , ase_worldlightDir );
			float lightDirectional13_g4 = dotResult9_g4;
			float4 clampResult20_g4 = clamp( ( lightAmbient14_g4 + lightDirectional13_g4 ) , _MinimumAmbient2 , float4( 1,1,1,1 ) );
			float4 lightCombined22_g4 = clampResult20_g4;
			float4 temp_cast_1 = (lightDirectional13_g4).xxxx;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV46_g4 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode46_g4 = ( _Float1 + _Float2 * pow( 1.0 - fresnelNdotV46_g4, _Float3 ) );
			float4 blendOpSrc53_g4 = temp_cast_1;
			float4 blendOpDest53_g4 = ( _RimColor1 * fresnelNode46_g4 );
			float4 lerpBlendMode53_g4 = lerp(blendOpDest53_g4,( blendOpDest53_g4/ max( 1.0 - blendOpSrc53_g4, 0.00001 ) ),_Float4);
			float4 clampResult56_g4 = clamp( ( ( ao24_g4 * ( lightCombined22_g4 * _BaseColor1 ) ) + ( saturate( lerpBlendMode53_g4 )) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			float4 color14 = IsGammaSpace() ? float4(0.7735849,0,0,1) : float4(0.5600193,0,0,1);
			float time17 = 0.0;
			float2 coords17 = i.uv_texcoord * 5.33;
			float2 id17 = 0;
			float2 uv17 = 0;
			float fade17 = 0.5;
			float voroi17 = 0;
			float rest17 = 0;
			for( int it17 = 0; it17 <2; it17++ ){
			voroi17 += fade17 * voronoi17( coords17, time17, id17, uv17, 0 );
			rest17 += fade17;
			coords17 *= 2;
			fade17 *= 0.5;
			}//Voronoi17
			voroi17 /= rest17;
			float4 color20 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
			o.Albedo = ( clampResult56_g4 * ( ( color14 * ( 1.0 - voroi17 ) ) + ( color20 * voroi17 ) ) ).rgb;
			o.Occlusion = temp_output_4_0;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				vertexDataFunc( v, customInputData );
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
0;73;1910;1286;1950.428;275.323;1.3;True;True
Node;AmplifyShaderEditor.Vector3Node;30;-2127.601,955.9316;Inherit;False;InstancedProperty;_SplatNormal;SplatNormal;17;0;Create;True;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;37;-1923.701,587.4174;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldToObjectTransfNode;51;-1936.736,1013.577;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VoronoiNode;2;-2000.489,309.9445;Inherit;True;0;0;1;0;1;True;11;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;10.41;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.PosVertexDataNode;57;-1717.737,1238.538;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;-1535.548,1014.989;Inherit;False;InstancedProperty;_SplatFac;SplatFac;16;0;Create;True;0;0;False;0;False;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;34;-1619.388,699.6608;Inherit;False;Projection;-1;;1;3249e2c8638c9ef4bbd1902a2d38a67c;0;2;5;FLOAT3;0,0,0;False;6;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-1881.875,768.1218;Inherit;False;Constant;_Float5;Float 5;3;0;Create;True;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-1906.434,874.5941;Inherit;False;Constant;_SphereRadius;SphereRadius;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;58;-1444.737,1234.538;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VoronoiNode;17;-2058.135,-260.8695;Inherit;True;0;0;1;0;2;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5.33;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.OneMinusNode;3;-1800.489,67.34453;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1619.261,831.6062;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-1440.846,372.0116;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;53;-1341.457,629.5547;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1308.63,1485.188;Inherit;False;Property;_SplatSpread;SplatSpread;14;0;Create;True;0;0;False;0;False;0.5;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-1627.489,69.34453;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;-1602.241,-467.8022;Inherit;False;Constant;_Color1;Color 1;0;0;Create;True;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-1607.433,-655.3356;Inherit;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;False;0;False;0.7735849,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;25;-1783.177,-346.4584;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;54;-1088.457,492.5547;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;-1241.044,337.4118;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1403.885,1109.515;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;60;-1174.737,1355.538;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;35;-1417.098,772.0913;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;59;-1191.737,1237.538;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-968.7368,1219.538;Inherit;True;4;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1158.241,-568.8022;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-758.3109,1046.863;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-846.4573,375.5547;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-878.0994,954.3943;Inherit;False;InstancedProperty;_FadeFac;FadeFac;15;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-724.1775,1148.435;Inherit;False;Constant;_Float3;Float 3;3;0;Create;True;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1155.241,-308.8022;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;-1343.792,76.46218;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.2;False;4;FLOAT;0.55;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1192.569,835.3732;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-440.46,885.5906;Inherit;False;4;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-907.2411,-483.8022;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1035.616,70.61344;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;6;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;15;-884.4329,-613.3356;Inherit;False;OdysseyLighting;0;;4;d7276cf1e824d06478e4762dcf51e0c7;0;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-604.9502,388.0186;Inherit;True;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;-193.8101,678.7238;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;24;-1419.241,-212.8022;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;11;-668.5992,43;Inherit;True;Normal From Height;-1;;5;1942fe2c5f1a1f94881a33d532e4afeb;0;1;20;FLOAT;0;False;2;FLOAT3;40;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-560.4329,-561.3356;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;19;-1875.241,-209.8022;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1722.241,-209.8022;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MeatClump;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;51;0;30;0
WireConnection;34;5;37;0
WireConnection;34;6;51;0
WireConnection;58;0;57;0
WireConnection;58;1;34;0
WireConnection;3;0;2;0
WireConnection;43;0;46;0
WireConnection;43;1;44;0
WireConnection;43;2;51;0
WireConnection;28;0;2;0
WireConnection;53;0;29;0
WireConnection;4;0;3;0
WireConnection;4;1;3;0
WireConnection;25;0;17;0
WireConnection;54;0;53;0
WireConnection;27;2;28;0
WireConnection;60;0;58;0
WireConnection;35;0;34;0
WireConnection;35;1;43;0
WireConnection;59;0;58;0
WireConnection;61;0;59;0
WireConnection;61;1;60;0
WireConnection;61;2;29;0
WireConnection;61;3;62;0
WireConnection;23;0;14;0
WireConnection;23;1;25;0
WireConnection;52;0;27;0
WireConnection;52;1;54;0
WireConnection;18;0;20;0
WireConnection;18;1;17;0
WireConnection;12;0;4;0
WireConnection;38;0;35;0
WireConnection;38;1;51;0
WireConnection;38;2;29;0
WireConnection;38;3;49;0
WireConnection;64;0;51;0
WireConnection;64;1;55;0
WireConnection;64;2;63;0
WireConnection;64;3;68;0
WireConnection;21;0;23;0
WireConnection;21;1;18;0
WireConnection;13;0;12;0
WireConnection;39;0;52;0
WireConnection;39;1;38;0
WireConnection;39;2;61;0
WireConnection;65;0;39;0
WireConnection;65;1;64;0
WireConnection;24;0;22;0
WireConnection;11;20;13;0
WireConnection;16;0;15;0
WireConnection;16;1;21;0
WireConnection;19;0;17;0
WireConnection;22;0;19;0
WireConnection;22;1;19;0
WireConnection;0;0;16;0
WireConnection;0;1;11;40
WireConnection;0;5;4;0
WireConnection;0;11;65;0
ASEEND*/
//CHKSM=271E5AB141825824B4D1F7F0952CD1E909C930AB