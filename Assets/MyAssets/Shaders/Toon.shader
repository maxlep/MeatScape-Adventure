// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon"
{
	Properties
	{
		_StepSoftness("StepSoftness", Range( 0 , 1)) = 0
		_SurfaceColor("SurfaceColor", Color) = (0,0,0,0)
		_ShadowColor("ShadowColor", Color) = (0,0,0,0)
		_AmbientFactor("AmbientFactor", Range( 0 , 100)) = 0
		_Normal("Normal", 2D) = "bump" {}
		_NumSteps("NumSteps", Float) = 4
		_Float0("Float 0", Float) = 9.34
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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
		uniform float4 _ShadowColor;
		uniform float4 _SurfaceColor;
		uniform float _NumSteps;
		uniform float _StepSoftness;
		uniform float _AmbientFactor;
		uniform float _Float0;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( SAMPLE_TEXTURE2D( _Normal, sampler_Normal, uv_Normal ) );
			float stepSize53 = ( 1.0 / _NumSteps );
			float temp_output_14_0 = ( 0.0 * stepSize53 );
			float4 temp_cast_0 = (temp_output_14_0).xxxx;
			float4 temp_cast_1 = (( temp_output_14_0 + _StepSoftness )).xxxx;
			float4 ambientAmt84 = ( _AmbientFactor * UNITY_LIGHTMODEL_AMBIENT );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult10 = dot( ase_worldNormal , ase_worldlightDir );
			float4 lighting48 = ( ambientAmt84 + dotResult10 );
			float4 smoothstepResult6 = smoothstep( temp_cast_0 , temp_cast_1 , lighting48);
			float temp_output_17_0 = ( 1.0 * stepSize53 );
			float4 temp_cast_2 = (temp_output_17_0).xxxx;
			float4 temp_cast_3 = (( temp_output_17_0 + _StepSoftness )).xxxx;
			float4 smoothstepResult18 = smoothstep( temp_cast_2 , temp_cast_3 , lighting48);
			float temp_output_21_0 = ( 2.0 * stepSize53 );
			float4 temp_cast_4 = (temp_output_21_0).xxxx;
			float4 temp_cast_5 = (( temp_output_21_0 + _StepSoftness )).xxxx;
			float4 smoothstepResult20 = smoothstep( temp_cast_4 , temp_cast_5 , lighting48);
			float4 temp_output_23_0 = (float4( 0,0,0,0 ) + (( smoothstepResult6 + smoothstepResult18 + smoothstepResult20 ) - float4( 0,0,0,0 )) * (float4( 1,1,1,1 ) - float4( 0,0,0,0 )) / (( ambientAmt84 + _NumSteps ) - float4( 0,0,0,0 )));
			float4 lerpResult59 = lerp( _ShadowColor , _SurfaceColor , temp_output_23_0);
			o.Albedo = lerpResult59.rgb;
			o.Emission = ( _ShadowColor * ( 1.0 - temp_output_23_0 ) * _Float0 ).rgb;
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
0;73;2071;1286;1563.904;-250.9536;1.3;True;True
Node;AmplifyShaderEditor.RangedFloatNode;82;-187.1888,1226.568;Inherit;False;Property;_NumSteps;NumSteps;7;0;Create;True;0;0;False;0;False;4;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-1359.293,-414.0609;Inherit;False;Property;_AmbientFactor;AmbientFactor;4;0;Create;True;0;0;False;0;False;0;26.2;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;46;-1343.951,-308.0796;Inherit;False;UNITY_LIGHTMODEL_AMBIENT;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;13;62.8683,1266.809;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1034.292,-363.0608;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;9;-1172.603,-166.8004;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;8;-1211.603,9.200218;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;226.0992,1279.355;Inherit;False;stepSize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-843.0333,-336.0237;Inherit;False;ambientAmt;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;10;-887.6003,-105.8002;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;-1091.309,1032.694;Inherit;False;53;stepSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-1102.309,726.6936;Inherit;False;53;stepSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-585.621,-269.7164;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-1096.309,1335.694;Inherit;False;53;stepSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1488.534,834.1717;Inherit;False;Property;_StepSoftness;StepSoftness;0;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-900.262,1032.172;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-914.0314,1334.716;Inherit;False;2;2;0;FLOAT;2;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-906.4866,735.1445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-324.1204,-251.6694;Inherit;False;lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-758.4866,781.1445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-1095.871,1250.252;Inherit;False;48;lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-767.0314,1389.715;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1102.225,644.9698;Inherit;False;48;lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-1095.725,951.2699;Inherit;False;48;lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-766.2619,1091.172;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;18;-632.2621,974.1721;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;6;-624.4869,664.1445;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-246.1165,1098.525;Inherit;False;84;ambientAmt;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;20;-633.0314,1272.715;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-5.138592,1092.761;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-99.36697,940.3975;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;23;175.4711,984.45;Inherit;True;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;25;233.5691,535.2468;Inherit;False;Property;_ShadowColor;ShadowColor;2;0;Create;True;0;0;False;0;False;0,0,0,0;0.56,0.05543999,0.1135189,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;24;231.5691,734.2468;Inherit;False;Property;_SurfaceColor;SurfaceColor;1;0;Create;True;0;0;False;0;False;0,0,0,0;1,0.2862745,0.2862745,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;89;524.115,1196.48;Inherit;False;Property;_Float0;Float 0;8;0;Create;True;0;0;False;0;False;9.34;1.68;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;88;518.115,1075.48;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;79;894.7194,612.8682;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0.8,0,0,0;False;2;COLOR;0.8,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;40;93.65598,231.291;Inherit;False;Global;unity_LightmapST;unity_LightmapST;5;0;Create;True;0;0;False;0;False;0,0,0,0;0.3843606,0.3843606,-0.007687213,-0.007687213;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;719.115,1021.48;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;43;356.1781,320.1447;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;643.6473,605.4508;Inherit;False;48;lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;42;353.149,222.2035;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;41;94.66569,94.98109;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;59;650.6495,768.6896;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;68;848.0389,1298.715;Inherit;True;Property;_Normal;Normal;5;0;Create;True;0;0;False;0;False;-1;None;a2fca8d02e7b01e46b82e9cf3ea1ed7c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;44;607.5935,157.5827;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;86;-49.33203,545.8058;Inherit;False;Property;_ShadowColor1;ShadowColor;3;0;Create;True;0;0;False;0;False;0,0,0,0;0.2823529,0.06666665,0.06666665,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;39;853.2357,243.2232;Inherit;True;Property;unity_Lightmap;unity_Lightmap;6;0;Create;False;0;0;False;0;False;-1;None;230811116fd67424ab535a4963eedb99;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1270.29,951.027;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Toon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;1;82;0
WireConnection;58;0;57;0
WireConnection;58;1;46;0
WireConnection;53;0;13;0
WireConnection;84;0;58;0
WireConnection;10;0;9;0
WireConnection;10;1;8;0
WireConnection;47;0;84;0
WireConnection;47;1;10;0
WireConnection;17;1;55;0
WireConnection;21;1;54;0
WireConnection;14;1;56;0
WireConnection;48;0;47;0
WireConnection;15;0;14;0
WireConnection;15;1;11;0
WireConnection;19;0;21;0
WireConnection;19;1;11;0
WireConnection;16;0;17;0
WireConnection;16;1;11;0
WireConnection;18;0;50;0
WireConnection;18;1;17;0
WireConnection;18;2;16;0
WireConnection;6;0;49;0
WireConnection;6;1;14;0
WireConnection;6;2;15;0
WireConnection;20;0;51;0
WireConnection;20;1;21;0
WireConnection;20;2;19;0
WireConnection;60;0;85;0
WireConnection;60;1;82;0
WireConnection;22;0;6;0
WireConnection;22;1;18;0
WireConnection;22;2;20;0
WireConnection;23;0;22;0
WireConnection;23;2;60;0
WireConnection;88;0;23;0
WireConnection;79;0;73;0
WireConnection;87;0;25;0
WireConnection;87;1;88;0
WireConnection;87;2;89;0
WireConnection;43;0;40;0
WireConnection;42;0;40;0
WireConnection;59;0;25;0
WireConnection;59;1;24;0
WireConnection;59;2;23;0
WireConnection;44;0;41;0
WireConnection;44;1;42;0
WireConnection;44;2;43;0
WireConnection;39;1;44;0
WireConnection;0;0;59;0
WireConnection;0;1;68;0
WireConnection;0;2;87;0
ASEEND*/
//CHKSM=3921B5E0B716DE808301C4601AC56BAF9CD2F62D