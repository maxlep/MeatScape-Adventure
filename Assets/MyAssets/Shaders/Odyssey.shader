// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Odyssey"
{
	Properties
	{
		_AmbientFactor("AmbientFactor", Range( 0 , 100)) = 27.34496
		_Normal("Normal", 2D) = "bump" {}
		_AO("AO", 2D) = "bump" {}
		_MinimumAmbient("MinimumAmbient", Color) = (0.4172381,0.4092143,0.674,1)
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_RimColor("RimColor", Color) = (1,1,1,1)
		_Float0("Float 0", Float) = 0
		_Float1("Float 1", Float) = 1
		_Float2("Float 2", Float) = 5
		_Float3("Float 3", Float) = -5.93
		_AOFactor("AO Factor", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
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
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal);
		uniform float4 _Normal_ST;
		SamplerState sampler_Normal;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_AO);
		uniform float4 _AO_ST;
		SamplerState sampler_AO;
		uniform float _AOFactor;
		uniform float _AmbientFactor;
		uniform float4 _MinimumAmbient;
		uniform float4 _BaseColor;
		uniform float4 _RimColor;
		uniform float _Float0;
		uniform float _Float1;
		uniform float _Float2;
		uniform float _Float3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( SAMPLE_TEXTURE2D( _Normal, sampler_Normal, uv_Normal ) );
			float2 uv_AO = i.uv_texcoord * _AO_ST.xy + _AO_ST.zw;
			float4 lightAmbient84 = ( _AmbientFactor * UNITY_LIGHTMODEL_AMBIENT );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult10 = dot( ase_worldNormal , ase_worldlightDir );
			float lightDirectional48 = dotResult10;
			float4 clampResult95 = clamp( ( lightAmbient84 + lightDirectional48 ) , _MinimumAmbient , float4( 1,1,1,1 ) );
			float4 lightCombined93 = clampResult95;
			float4 temp_cast_1 = (lightDirectional48).xxxx;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV102 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode102 = ( _Float0 + _Float1 * pow( 1.0 - fresnelNdotV102, _Float2 ) );
			float4 blendOpSrc112 = temp_cast_1;
			float4 blendOpDest112 = ( _RimColor * fresnelNode102 );
			float4 lerpBlendMode112 = lerp(blendOpDest112,( blendOpDest112/ max( 1.0 - blendOpSrc112, 0.00001 ) ),_Float3);
			float4 clampResult115 = clamp( ( ( lightCombined93 * _BaseColor ) + ( saturate( lerpBlendMode112 )) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			o.Albedo = ( float4( ( 1.0 - ( ( 1.0 - UnpackNormal( SAMPLE_TEXTURE2D( _AO, sampler_AO, uv_AO ) ) ) * _AOFactor ) ) , 0.0 ) * clampResult115 ).rgb;
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
0;73;2071;1286;777.5034;113.823;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;101;-639.3469,-1107.497;Inherit;False;2237.914;1416.38;;7;93;95;92;97;98;99;100;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;99;-126.8078,-626.7375;Inherit;False;859.36;407.4947;;4;8;10;48;9;World Directional;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;98;-130.3848,-1000.316;Inherit;False;892.1353;304;;4;46;58;84;57;World Ambient;1,1,1,1;0;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;46;-65.04293,-829.0812;Inherit;False;UNITY_LIGHTMODEL_AMBIENT;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-80.38497,-935.0624;Inherit;False;Property;_AmbientFactor;AmbientFactor;0;0;Create;True;0;0;False;0;False;27.34496;26.2;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;8;-76.80791,-402.2431;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;9;-49.85284,-576.7377;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;274.4517,-950.3165;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;10;247.1946,-517.2432;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;537.7493,-944.6528;Inherit;False;lightAmbient;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;506.5514,-508.7855;Inherit;False;lightDirectional;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;97;847.5579,-340.2895;Inherit;False;Property;_MinimumAmbient;MinimumAmbient;4;0;Create;True;0;0;False;0;False;0.4172381,0.4092143,0.674,1;0.4062818,0.4018334,0.6603774,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;92;854.5115,-613.1283;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-688.6303,1583.78;Inherit;False;Property;_Float1;Float 1;8;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;95;1097.558,-462.6895;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-687.6303,1678.78;Inherit;False;Property;_Float2;Float 2;9;0;Create;True;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;-693.6303,1489.78;Inherit;False;Property;_Float0;Float 0;7;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;1376.611,-461.4284;Inherit;False;lightCombined;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;105;-338.6305,1248.78;Inherit;False;Property;_RimColor;RimColor;6;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;102;-455.4666,1455.851;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;-328.032,843.3176;Inherit;False;93;lightCombined;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;113;17.36956,1199.78;Inherit;False;Property;_Float3;Float 3;10;0;Create;True;0;0;False;0;False;-5.93;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;-112.7132,1083.848;Inherit;False;48;lightDirectional;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;103;-336.6666,990.8502;Inherit;False;Property;_BaseColor;BaseColor;5;0;Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;117;-60.87409,338.1239;Inherit;True;Property;_AO;AO;2;0;Create;True;0;0;False;0;False;-1;None;a2fca8d02e7b01e46b82e9cf3ea1ed7c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-13.63045,1275.78;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-12.86596,855.1495;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;112;254.3696,1048.78;Inherit;True;ColorDodge;True;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;120;277.376,319.9137;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-97.62383,569.9137;Inherit;False;Property;_AOFactor;AO Factor;11;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;114;288.3697,743.7789;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;491.4574,515.4573;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;100;-591.3927,-154.558;Inherit;False;1379.674;428.2418;;7;43;42;41;39;40;90;44;World Lightmap;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;123;733.3764,598.9138;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;115;505.3703,742.7789;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;564.2804,46.46984;Inherit;False;lightMap;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;68;774.6326,1209.796;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;False;0;False;-1;None;a2fca8d02e7b01e46b82e9cf3ea1ed7c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;40;-541.3928,31.75156;Inherit;False;Global;unity_LightmapST;unity_LightmapST;5;0;Create;True;0;0;False;0;False;0,0,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;42;-281.8988,22.66407;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;122;954.2761,781.9134;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;39;218.1867,43.68374;Inherit;True;Property;unity_Lightmap;unity_Lightmap;3;0;Create;False;0;0;False;0;False;-1;None;230811116fd67424ab535a4963eedb99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;43;-278.8698,120.6053;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;41;-540.3828,-104.5581;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;44;-27.4549,-41.95684;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1270.29,951.027;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Odyssey;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;58;0;57;0
WireConnection;58;1;46;0
WireConnection;10;0;9;0
WireConnection;10;1;8;0
WireConnection;84;0;58;0
WireConnection;48;0;10;0
WireConnection;92;0;84;0
WireConnection;92;1;48;0
WireConnection;95;0;92;0
WireConnection;95;1;97;0
WireConnection;93;0;95;0
WireConnection;102;1;106;0
WireConnection;102;2;108;0
WireConnection;102;3;109;0
WireConnection;110;0;105;0
WireConnection;110;1;102;0
WireConnection;104;0;91;0
WireConnection;104;1;103;0
WireConnection;112;0;116;0
WireConnection;112;1;110;0
WireConnection;112;2;113;0
WireConnection;120;0;117;0
WireConnection;114;0;104;0
WireConnection;114;1;112;0
WireConnection;118;0;120;0
WireConnection;118;1;119;0
WireConnection;123;0;118;0
WireConnection;115;0;114;0
WireConnection;90;0;39;0
WireConnection;42;0;40;0
WireConnection;122;0;123;0
WireConnection;122;1;115;0
WireConnection;39;1;44;0
WireConnection;43;0;40;0
WireConnection;44;0;41;0
WireConnection;44;1;42;0
WireConnection;44;2;43;0
WireConnection;0;0;122;0
WireConnection;0;1;68;0
ASEEND*/
//CHKSM=19186CD8307F6C583D1D5B6A1D000B5F1A0FF5BB