// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CorruptMeat"
{
	Properties
	{
		_Color("Color", 2D) = "white" {}
		[Toggle(UNITY_PASS_FORWARDBASE)] _Keyword0("Keyword 0", Float) = 0
		_Float1("Float 1", Float) = 100
		_FogDistance("Fog Depth", Float) = 50
		_FogColor("Fog Color", Color) = (0.3102527,0.6036025,0.7924528,0)
		[Normal]_Normal("Normal", 2D) = "bump" {}
		_Corruptfac("Corrupt fac", Range( 0 , 1)) = 1
		_Noisescale("Noise scale", Float) = 1
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 10
		_Noisespeed("Noise speed", Float) = 1
		_Texturenoisescale("Texture noise scale", Range( 0 , 1)) = 1
		_Noisebase("Noise base", Float) = 0.1
		_CorruptColor2("Corrupt Color 2", Color) = (0.6784314,0,0.8235294,1)
		_CorruptColor1("Corrupt Color 1", Color) = (1,1,1,1)
		_Smoothnesss("Smoothnesss", Range( 0 , 1)) = 0
		_Fresnelmax("Fresnel max", Range( 0 , 100)) = 0
		_Displacementscale("Displacement scale", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
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
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
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

		UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal);
		uniform float4 _Normal_ST;
		SamplerState sampler_Normal;
		uniform float _Noisespeed;
		uniform float _Noisebase;
		uniform float _Noisescale;
		uniform float _Displacementscale;
		uniform float4 _CorruptColor1;
		uniform float4 _CorruptColor2;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Color);
		uniform float _Texturenoisescale;
		SamplerState sampler_Color;
		uniform float _Smoothnesss;
		uniform float _Fresnelmax;
		uniform float4 _FogColor;
		uniform float _FogDistance;
		uniform float _Float1;
		uniform float _Corruptfac;
		uniform float _EdgeLength;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
			float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * tangentSign;
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float2 uv_Normal = v.texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 tex2DNode2 = UnpackNormal( SAMPLE_TEXTURE2D_LOD( _Normal, sampler_Normal, uv_Normal, 0.0 ) );
			float4 transform19 = mul(unity_ObjectToWorld,float4( tex2DNode2 , 0.0 ));
			float mulTime25 = _Time.y * _Noisespeed;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float simplePerlin3D22 = snoise( ( mulTime25 + ase_worldPos )*( _Noisebase * 2.0 * _Noisescale ) );
			simplePerlin3D22 = simplePerlin3D22*0.5 + 0.5;
			float3 temp_cast_3 = (mulTime25).xxx;
			float simplePerlin3D29 = snoise( ( temp_cast_3 - ase_worldPos )*( _Noisebase * _Noisescale ) );
			simplePerlin3D29 = simplePerlin3D29*0.5 + 0.5;
			float blendOpSrc33 = simplePerlin3D22;
			float blendOpDest33 = simplePerlin3D29;
			float temp_output_33_0 = ( saturate( ( 1.0 - ( ( 1.0 - blendOpDest33) / max( blendOpSrc33, 0.00001) ) ) ));
			v.vertex.xyz += ( float4( mul( ase_worldToTangent, transform19.xyz ) , 0.0 ) * (0.0 + (temp_output_33_0 - 0.0) * (1.0 - 0.0) / (255.0 - 0.0)) * _Displacementscale ).xyz;
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
			float mulTime25 = _Time.y * _Noisespeed;
			float3 ase_worldPos = i.worldPos;
			float simplePerlin3D22 = snoise( ( mulTime25 + ase_worldPos )*( _Noisebase * 2.0 * _Noisescale ) );
			simplePerlin3D22 = simplePerlin3D22*0.5 + 0.5;
			float3 temp_cast_1 = (mulTime25).xxx;
			float simplePerlin3D29 = snoise( ( temp_cast_1 - ase_worldPos )*( _Noisebase * _Noisescale ) );
			simplePerlin3D29 = simplePerlin3D29*0.5 + 0.5;
			float blendOpSrc33 = simplePerlin3D22;
			float blendOpDest33 = simplePerlin3D29;
			float temp_output_33_0 = ( saturate( ( 1.0 - ( ( 1.0 - blendOpDest33) / max( blendOpSrc33, 0.00001) ) ) ));
			float4 temp_cast_2 = (temp_output_33_0).xxxx;
			float4 temp_output_5_0_g3 = temp_cast_2;
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 tex2DNode2 = UnpackNormal( SAMPLE_TEXTURE2D( _Normal, sampler_Normal, uv_Normal ) );
			float4 temp_output_83_0_g4 = float4( tex2DNode2 , 0.0 );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult52_g4 = dot( (WorldNormalVector( i , temp_output_83_0_g4.rgb )) , ase_worldlightDir );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi77_g4 = gi;
			float3 diffNorm77_g4 = WorldNormalVector( i , temp_output_83_0_g4.rgb );
			gi77_g4 = UnityGI_Base( data, 1, diffNorm77_g4 );
			float3 indirectDiffuse77_g4 = gi77_g4.indirect.diffuse + diffNorm77_g4 * 0.0001;
			float mulTime58 = _Time.y * 0.01;
			float2 temp_cast_9 = (( mulTime58 + ( _Texturenoisescale * simplePerlin3D22 ) )).xx;
			float2 uv_TexCoord53 = i.uv_texcoord + temp_cast_9;
			float3 indirectNormal78_g4 = WorldNormalVector( i , temp_output_83_0_g4.rgb );
			Unity_GlossyEnvironmentData g78_g4 = UnityGlossyEnvironmentSetup( _Smoothnesss, data.worldViewDir, indirectNormal78_g4, float3(0,0,0));
			float3 indirectSpecular78_g4 = UnityGI_IndirectSpecular( data, 1.0, indirectNormal78_g4, g78_g4 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV65_g4 = dot( mul(ase_tangentToWorldFast,temp_output_83_0_g4.rgb), ase_worldViewDir );
			float fresnelNode65_g4 = ( 0.04 + 1.0 * pow( 1.0 - fresnelNdotV65_g4, 5.0 ) );
			float clampResult66_g4 = clamp( fresnelNode65_g4 , 0.0 , _Fresnelmax );
			float4 lerpResult69_g4 = lerp( ( float4( ( ( max( dotResult52_g4 , 0.0 ) * ( ase_lightAtten * ase_lightColor.rgb ) ) + indirectDiffuse77_g4 ) , 0.0 ) * SAMPLE_TEXTURE2D( _Color, sampler_Color, uv_TexCoord53 ) ) , float4( indirectSpecular78_g4 , 0.0 ) , clampResult66_g4);
			float4 temp_cast_13 = (0.0).xxxx;
			#ifdef UNITY_PASS_FORWARDBASE
				float4 staticSwitch74_g4 = _FogColor;
			#else
				float4 staticSwitch74_g4 = temp_cast_13;
			#endif
			float4 ase_vertex4Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 ase_viewPos = UnityObjectToViewPos( ase_vertex4Pos );
			float ase_screenDepth = -ase_viewPos.z;
			float cameraDepthFade70_g4 = (( ase_screenDepth -_ProjectionParams.y - _Float1 ) / _FogDistance);
			float4 lerpResult71_g4 = lerp( lerpResult69_g4 , staticSwitch74_g4 , saturate( cameraDepthFade70_g4 ));
			float4 blendOpSrc39 = ( ( _CorruptColor1 * temp_output_5_0_g3 ) + ( _CorruptColor2 * ( 1.0 - temp_output_5_0_g3 ) ) );
			float4 blendOpDest39 = lerpResult71_g4;
			float4 lerpBlendMode39 = lerp(blendOpDest39,(( blendOpSrc39 > 0.5 ) ? max( blendOpDest39, 2.0 * ( blendOpSrc39 - 0.5 ) ) : min( blendOpDest39, 2.0 * blendOpSrc39 ) ),_Corruptfac);
			c.rgb = ( saturate( lerpBlendMode39 )).rgb;
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
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc tessellate:tessFunction 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
				vertexDataFunc( v );
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
0;73;2049;1286;2602.8;823.5413;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;52;-2253.59,1055.836;Inherit;False;Property;_Noisespeed;Noise speed;15;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;25;-2016,1056;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;23;-2016,1152;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;51;-2027.59,900.8358;Inherit;False;Property;_Noisebase;Noise base;17;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-2042.59,1344.836;Inherit;False;Property;_Noisescale;Noise scale;9;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-1721.59,886.8358;Inherit;False;3;3;0;FLOAT;0.1;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-1728,1056;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;22;-1504,928;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-2215.858,-375.525;Inherit;False;Property;_Texturenoisescale;Texture noise scale;16;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;58;-2139.8,-507.5413;Inherit;False;1;0;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-2054.858,-178.525;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1874.8,-326.5413;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1711.59,1319.836;Inherit;False;2;2;0;FLOAT;0.1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;-1728,1184;Inherit;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;53;-1682.801,-252.9511;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;29;-1504,1184;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1407,-126;Inherit;True;Property;_Normal;Normal;7;1;[Normal];Create;True;0;0;False;0;False;-1;91276d2ac7dfbbe47812bf35d04b8e48;91276d2ac7dfbbe47812bf35d04b8e48;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-972,-88;Inherit;False;Property;_Smoothnesss;Smoothnesss;23;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-672,1296;Inherit;False;Property;_CorruptColor2;Corrupt Color 2;18;0;Create;True;0;0;False;0;False;0.6784314,0,0.8235294,1;0.6784314,0,0.8235294,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1405,-381;Inherit;True;Property;_Color;Color;0;0;Create;True;0;0;False;0;False;-1;0f9913c03c6638d43a1b5c7a115ad002;0f9913c03c6638d43a1b5c7a115ad002;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldToTangentMatrix;18;-860.1946,205.8042;Inherit;False;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-967,31;Inherit;False;Property;_Fresnelmax;Fresnel max;24;0;Create;True;0;0;False;0;False;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;19;-852.1946,292.8042;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;45;-672,1088;Inherit;False;Property;_CorruptColor1;Corrupt Color 1;19;0;Create;True;0;0;False;0;False;1,1,1,1;0.6784314,0,0.8235294,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;33;-1180.991,1352.565;Inherit;True;ColorBurn;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-647.1946,271.8042;Inherit;False;2;2;0;FLOAT3x3;0,0,0,0,0,1,1,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;44;-421.7799,1543.364;Inherit;True;Mix;-1;;3;b228439c3a573c4458a1b50c1fc4fdbb;0;3;3;FLOAT4;0.8,0.8,0.8,0.8;False;4;FLOAT4;1,1,1,1;False;5;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-676.1532,910.8433;Inherit;False;Property;_Corruptfac;Corrupt fac;8;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1093,941;Inherit;False;Property;_Displacementscale;Displacement scale;25;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;6;-609,-261;Inherit;False;Meat Lighting;1;;4;73b54e17a4cbe7f4eb8f5dc9960b8200;0;4;83;COLOR;1,1,1,0;False;75;COLOR;0,0,0,0;False;79;FLOAT;0;False;80;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;14;-1038.642,641.2986;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;255;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;36;-835.9236,1629.745;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;39;-182.2468,920.6835;Inherit;True;PinLight;True;3;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;27;-1647.916,-2.628662;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1700,644;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;40;-846.5081,1386.841;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-1406,641;Inherit;True;Property;_Displacement;Displacement;22;0;Create;True;0;0;False;0;False;-1;59bc557d7c3e3734bac0a83c84bf0d4d;59bc557d7c3e3734bac0a83c84bf0d4d;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;10;-1893,645;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;32;-894.991,988.5654;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;35;-1080.924,1616.745;Inherit;False;0;2;2;0.6789749,0,0.823,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-772,679;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BlendOpsNode;31;-1180.916,1085.371;Inherit;True;HardMix;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-1407,128;Inherit;True;Property;_Specular;Specular;20;0;Create;True;0;0;False;0;False;-1;d46f7e461cbdeb446a2a36cd7fb1908b;d46f7e461cbdeb446a2a36cd7fb1908b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-195.2467,1254.422;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1408,386;Inherit;True;Property;_AO;AO;21;0;Create;True;0;0;False;0;False;-1;ddb27bc36da151749a6e910c27600732;ddb27bc36da151749a6e910c27600732;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;CorruptMeat;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;10;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;10;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;52;0
WireConnection;50;0;51;0
WireConnection;50;2;47;0
WireConnection;24;0;25;0
WireConnection;24;1;23;0
WireConnection;22;0;24;0
WireConnection;22;1;50;0
WireConnection;56;0;57;0
WireConnection;56;1;22;0
WireConnection;59;0;58;0
WireConnection;59;1;56;0
WireConnection;49;0;51;0
WireConnection;49;1;47;0
WireConnection;28;0;25;0
WireConnection;28;1;23;0
WireConnection;53;1;59;0
WireConnection;29;0;28;0
WireConnection;29;1;49;0
WireConnection;1;1;53;0
WireConnection;19;0;2;0
WireConnection;33;0;22;0
WireConnection;33;1;29;0
WireConnection;20;0;18;0
WireConnection;20;1;19;0
WireConnection;44;3;45;0
WireConnection;44;4;42;0
WireConnection;44;5;33;0
WireConnection;6;83;2;0
WireConnection;6;75;1;0
WireConnection;6;79;7;0
WireConnection;6;80;8;0
WireConnection;14;0;33;0
WireConnection;36;0;35;0
WireConnection;36;1;33;0
WireConnection;39;0;44;0
WireConnection;39;1;6;0
WireConnection;39;2;46;0
WireConnection;9;1;10;1
WireConnection;40;0;33;0
WireConnection;3;1;9;0
WireConnection;32;0;31;0
WireConnection;13;0;20;0
WireConnection;13;1;14;0
WireConnection;13;2;12;0
WireConnection;31;0;22;0
WireConnection;31;1;29;0
WireConnection;0;13;39;0
WireConnection;0;11;13;0
ASEEND*/
//CHKSM=EA4C8899366BD5DD8FE3FCEE903417513CC168A3