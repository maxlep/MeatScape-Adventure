// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Meat Terrain"
{
	Properties
	{
		_Gridthreshold1("Grid threshold", Float) = 0.5
		_Gridfactor1("Grid factor", Range( 0 , 1)) = 1
		_Gridspacing1("Grid spacing", Float) = 1
		_Gridsoftness1("Grid softness", Float) = 0.1
		_Gridalignmentthreshold1("Grid alignment threshold", Float) = 0
		[Toggle(UNITY_PASS_FORWARDBASE)] _Keyword0("Keyword 0", Float) = 0
		_Float1("Float 1", Float) = 100
		_FogDistance("Fog Depth", Float) = 50
		_FogColor("Fog Color", Color) = (0.3102527,0.6036025,0.7924528,0)
		_GridColor("Grid Color", Color) = (0,0,0,0)
		_TopColor1("TopColor", Color) = (0.03592289,0.990566,0,0)
		_TuftColor1("TuftColor", Color) = (0.03592289,0.990566,0,0)
		_SideColor1("SideColor", Color) = (0.03592289,0.990566,0,0)
		_GrassStrictness1("Grass Strictness", Range( -1 , 1)) = 0.9
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_FresnelMax("Fresnel Max", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
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
			float eyeDepth;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float4 _SideColor1;
		uniform float _GrassStrictness1;
		uniform float4 _TopColor1;
		uniform float4 _TuftColor1;
		uniform float4 _GridColor;
		uniform float _Gridfactor1;
		uniform float _Gridthreshold1;
		uniform float _Gridsoftness1;
		uniform float _Gridspacing1;
		uniform float _Gridalignmentthreshold1;
		uniform float _Smoothness;
		uniform float _FresnelMax;
		uniform float4 _FogColor;
		uniform float _FogDistance;
		uniform float _Float1;


		float2 voronoihash5_g4( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi5_g4( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash5_g4( n + g );
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
			return (F2 + F1) * 0.5;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float4 appendResult58 = (float4(mul( ase_worldToTangent, ase_worldNormal ) , 0.0));
			float4 temp_output_83_0_g2 = appendResult58;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult52_g2 = dot( (WorldNormalVector( i , temp_output_83_0_g2.rgb )) , ase_worldlightDir );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi77_g2 = gi;
			float3 diffNorm77_g2 = WorldNormalVector( i , temp_output_83_0_g2.rgb );
			gi77_g2 = UnityGI_Base( data, 1, diffNorm77_g2 );
			float3 indirectDiffuse77_g2 = gi77_g2.indirect.diffuse + diffNorm77_g2 * 0.0001;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float4 transform4_g4 = mul(unity_ObjectToWorld,float4( ase_vertexNormal , 0.0 ));
			float dotResult9_g4 = dot( transform4_g4 , float4( float3(0,1,0) , 0.0 ) );
			float temp_output_12_0_g4 = step( _GrassStrictness1 , dotResult9_g4 );
			float temp_output_17_0_g4 = ( 1.0 - temp_output_12_0_g4 );
			float time5_g4 = 0.0;
			float2 coords5_g4 = (ase_worldPos).xz * 5.33;
			float2 id5_g4 = 0;
			float2 uv5_g4 = 0;
			float voroi5_g4 = voronoi5_g4( coords5_g4, time5_g4, id5_g4, uv5_g4, 0 );
			float4 appendResult16_g4 = (float4(0.0 , (( ( id5_g4 * float2( 17,17 ) ) % float2( 3,3 ) )).x , 0.0 , 0.0));
			float4 blendOpSrc23_g4 = _TopColor1;
			float4 blendOpDest23_g4 = appendResult16_g4;
			float4 clampResult25_g4 = clamp( ( saturate( ( blendOpSrc23_g4 * blendOpDest23_g4 ) )) , ( _TopColor1 - _TuftColor1 ) , ( _TopColor1 + _TuftColor1 ) );
			float temp_output_14_0_g4 = ( temp_output_12_0_g4 * ( 1.0 - voroi5_g4 ) );
			float temp_output_15_0_g4 = step( 0.89 , temp_output_14_0_g4 );
			float4 albedo31_g4 = ( ( _SideColor1 * temp_output_17_0_g4 ) + ( clampResult25_g4 * temp_output_15_0_g4 ) + ( _TopColor1 * ( 1.0 - ( temp_output_17_0_g4 + temp_output_15_0_g4 ) ) ) );
			float gridThreshold19_g3 = _Gridthreshold1;
			float gridSoftness26_g3 = _Gridsoftness1;
			float gridSpacing13_g3 = _Gridspacing1;
			float gridMin14_g3 = ( 0.5 * UNITY_PI );
			float gridMax15_g3 = ( 2.5 * UNITY_PI );
			float smoothstepResult59_g3 = smoothstep( gridThreshold19_g3 , ( gridThreshold19_g3 + gridSoftness26_g3 ) , sin( (gridMin14_g3 + (ase_worldPos.x - 0.0) * (gridMax15_g3 - gridMin14_g3) / (gridSpacing13_g3 - 0.0)) ));
			float dotResult17_g3 = dot( ase_worldNormal , float3( 1,0,0 ) );
			float gridAlignmentThreshold18_g3 = _Gridalignmentthreshold1;
			float xAlignment53_g3 = step( abs( dotResult17_g3 ) , gridAlignmentThreshold18_g3 );
			float smoothstepResult61_g3 = smoothstep( gridThreshold19_g3 , ( gridThreshold19_g3 + gridSoftness26_g3 ) , sin( (gridMin14_g3 + (ase_worldPos.y - 0.0) * (gridMax15_g3 - gridMin14_g3) / (gridSpacing13_g3 - 0.0)) ));
			float dotResult11_g3 = dot( ase_worldNormal , float3( 0,1,0 ) );
			float yAlignment47_g3 = step( abs( dotResult11_g3 ) , gridAlignmentThreshold18_g3 );
			float smoothstepResult60_g3 = smoothstep( gridThreshold19_g3 , ( gridThreshold19_g3 + gridSoftness26_g3 ) , sin( (gridMin14_g3 + (ase_worldPos.z - 0.0) * (gridMax15_g3 - gridMin14_g3) / (gridSpacing13_g3 - 0.0)) ));
			float dotResult10_g3 = dot( ase_worldNormal , float3( 0,0,1 ) );
			float zAlignment48_g3 = step( abs( dotResult10_g3 ) , gridAlignmentThreshold18_g3 );
			float clampResult66_g3 = clamp( ( ( smoothstepResult59_g3 * xAlignment53_g3 ) + ( smoothstepResult61_g3 * yAlignment47_g3 ) + ( smoothstepResult60_g3 * zAlignment48_g3 ) ) , 0.0 , 1.0 );
			float gridMask69_g3 = ( _Gridfactor1 * clampResult66_g3 );
			float4 lerpResult37 = lerp( albedo31_g4 , _GridColor , gridMask69_g3);
			float3 indirectNormal78_g2 = WorldNormalVector( i , temp_output_83_0_g2.rgb );
			Unity_GlossyEnvironmentData g78_g2 = UnityGlossyEnvironmentSetup( _Smoothness, data.worldViewDir, indirectNormal78_g2, float3(0,0,0));
			float3 indirectSpecular78_g2 = UnityGI_IndirectSpecular( data, 1.0, indirectNormal78_g2, g78_g2 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV65_g2 = dot( mul(ase_tangentToWorldFast,temp_output_83_0_g2.rgb), ase_worldViewDir );
			float fresnelNode65_g2 = ( 0.04 + 1.0 * pow( 1.0 - fresnelNdotV65_g2, 5.0 ) );
			float clampResult66_g2 = clamp( fresnelNode65_g2 , 0.0 , _FresnelMax );
			float4 lerpResult69_g2 = lerp( ( float4( ( ( max( dotResult52_g2 , 0.0 ) * ( ase_lightAtten * ase_lightColor.rgb ) ) + indirectDiffuse77_g2 ) , 0.0 ) * lerpResult37 ) , float4( indirectSpecular78_g2 , 0.0 ) , clampResult66_g2);
			float4 temp_cast_13 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDBASE
				float4 staticSwitch74_g2 = _FogColor;
			#else
				float4 staticSwitch74_g2 = temp_cast_13;
			#endif
			float cameraDepthFade70_g2 = (( i.eyeDepth -_ProjectionParams.y - _Float1 ) / _FogDistance);
			float4 lerpResult71_g2 = lerp( lerpResult69_g2 , staticSwitch74_g2 , saturate( cameraDepthFade70_g2 ));
			c.rgb = lerpResult71_g2.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				float1 customPack1 : TEXCOORD1;
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
				o.customPack1.x = customInputData.eyeDepth;
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
				surfIN.eyeDepth = IN.customPack1.x;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
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
161;73;1480;1286;1343.997;1843.711;1.887059;True;True
Node;AmplifyShaderEditor.WorldToTangentMatrix;53;-513.1658,-1531.101;Inherit;False;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.WorldNormalVector;49;-514.0929,-1434.863;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;36;-254.8095,-957.2789;Inherit;False;Grid;0;;3;875ca43246faee14cbcec748e511899d;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;38;-253.7324,-1151.767;Inherit;False;Grass;13;;4;a05bb52d51a392d4ea2b48b9e1b3f9fa;0;0;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-479.4717,-1086.951;Inherit;False;Property;_GridColor;Grid Color;12;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-288.1658,-1533.101;Inherit;False;2;2;0;FLOAT3x3;0,0,0,0,0,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;37;-95.72493,-1087.629;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;58;-128.7484,-1531.983;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-477.5638,-800.342;Inherit;False;Property;_Smoothness;Smoothness;18;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-476.1711,-702.4933;Inherit;False;Property;_FresnelMax;Fresnel Max;19;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;61;257.8397,-1023.009;Inherit;False;Meat Lighting;6;;2;73b54e17a4cbe7f4eb8f5dc9960b8200;0;4;83;COLOR;1,1,1,0;False;75;COLOR;0,0,0,0;False;79;FLOAT;0;False;80;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;641.8745,-1275.923;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;Meat Terrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;53;0
WireConnection;54;1;49;0
WireConnection;37;0;38;0
WireConnection;37;1;39;0
WireConnection;37;2;36;0
WireConnection;58;0;54;0
WireConnection;61;83;58;0
WireConnection;61;75;37;0
WireConnection;61;79;23;0
WireConnection;61;80;41;0
WireConnection;0;13;61;0
ASEEND*/
//CHKSM=D0290B9A00053A7C1669B1B81C8C1C333C1D2564