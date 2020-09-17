// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DevGrid"
{
	Properties
	{
		_Gridthreshold("Grid threshold", Float) = 0.5
		_Gridfactor("Grid factor", Range( 0 , 1)) = 1
		_TopColor("TopColor", Color) = (0.03592289,0.990566,0,0)
		_TuftColor("TuftColor", Color) = (0.03592289,0.990566,0,0)
		_SideColor("SideColor", Color) = (0.03592289,0.990566,0,0)
		_Gridspacing("Grid spacing", Float) = 1
		_Gridalbedo("Grid albedo", Color) = (0,0.8584906,0.02163776,0)
		_Gridsoftness("Grid softness", Float) = 0.1
		_GrassStrictness("Grass Strictness", Range( -1 , 1)) = 0.9
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldNormal;
			float3 worldPos;
		};

		uniform float4 _SideColor;
		uniform float _GrassStrictness;
		uniform float4 _TopColor;
		uniform float4 _TuftColor;
		uniform float _Gridfactor;
		uniform float _Gridthreshold;
		uniform float _Gridsoftness;
		uniform float _Gridspacing;
		uniform float4 _Gridalbedo;


		float2 voronoihash51( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi51( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash51( n + g );
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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float4 transform68 = mul(unity_ObjectToWorld,float4( ase_vertexNormal , 0.0 ));
			float dotResult64 = dot( transform68 , float4( float3(0,1,0) , 0.0 ) );
			float temp_output_66_0 = step( _GrassStrictness , dotResult64 );
			float temp_output_72_0 = ( 1.0 - temp_output_66_0 );
			float time51 = 0.0;
			float3 ase_worldPos = i.worldPos;
			float2 coords51 = (ase_worldPos).xz * 5.33;
			float2 id51 = 0;
			float2 uv51 = 0;
			float voroi51 = voronoi51( coords51, time51, id51, uv51, 0 );
			float4 appendResult93 = (float4(0.0 , (( ( id51 * float2( 17,17 ) ) % float2( 3,3 ) )).x , 0.0 , 0.0));
			float4 blendOpSrc82 = _TopColor;
			float4 blendOpDest82 = appendResult93;
			float4 clampResult98 = clamp( ( saturate( ( blendOpSrc82 * blendOpDest82 ) )) , ( _TopColor - _TuftColor ) , ( _TopColor + _TuftColor ) );
			float temp_output_65_0 = ( temp_output_66_0 * ( 1.0 - voroi51 ) );
			float temp_output_85_0 = step( 0.89 , temp_output_65_0 );
			float4 albedo53 = ( ( _SideColor * temp_output_72_0 ) + ( clampResult98 * temp_output_85_0 ) + ( _TopColor * ( 1.0 - ( temp_output_72_0 + temp_output_85_0 ) ) ) );
			float smoothstepResult6 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.x - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float smoothstepResult23 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.y - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float smoothstepResult32 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.z - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float clampResult38 = clamp( ( smoothstepResult6 + smoothstepResult23 + smoothstepResult32 ) , 0.0 , 1.0 );
			float gridMask48 = ( _Gridfactor * clampResult38 );
			o.Albedo = ( ( albedo53 * ( 1.0 - gridMask48 ) ) + ( _Gridalbedo * gridMask48 ) ).rgb;
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
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
0;73;2073;1286;507.1088;1377.118;1;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;55;-2467.018,585.3376;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;70;-2272.445,582.4314;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalVertexDataNode;67;-2452.445,223.4314;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;47;-210.2778,-1318.097;Inherit;False;1951.908;1199.788;;29;48;45;46;38;37;6;32;23;22;24;14;31;33;7;12;13;19;9;28;15;10;2;27;18;26;17;8;35;36;Grid mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-160.2778,-1054.599;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-159.2778,-979.5993;Inherit;False;Constant;_Float1;Float 1;10;0;Create;True;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;51;-2052.154,582.9197;Inherit;False;0;0;1;3;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5.33;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;68;-2259.445,220.4314;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;63;-2229.327,409.5438;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;71;-2232.029,111.9826;Inherit;False;Property;_GrassStrictness;Grass Strictness;8;0;Create;True;0;0;False;0;False;0.9;0.8;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;27;90.45561,-245.3179;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;18;95.45561,-620.318;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;10;89.89995,-989.0964;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;2;94.89995,-1268.097;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2727.209,-528.2669;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;17,17;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;8;107.8999,-1127.097;Inherit;False;Property;_Gridspacing;Grid spacing;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;15;89.89995,-1053.096;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;26;90.45561,-309.3181;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-1991.88,339.2109;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;17;95.45561,-684.3181;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;336.4557,-483.3179;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;354.9,-970.0966;Inherit;False;Property;_Gridsoftness;Grid softness;7;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;91;-2564.209,-534.2669;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;3,3;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;12;352.9,-1057.096;Inherit;False;Property;_Gridthreshold;Grid threshold;0;0;Create;True;0;0;False;0;False;0.5;0.96;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;66;-1838.219,242.8759;Inherit;False;2;0;FLOAT;0.9;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;57;-1854.619,577.8378;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;9;335.9001,-1227.097;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;19;341.4558,-858.318;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;564.4565,-625.318;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;559.4564,-250.3179;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;7;531.9005,-1225.097;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;558.9009,-994.0964;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;33;532.4559,-481.3179;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;537.4561,-856.318;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;96;-2268.209,-476.2669;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-1672.375,494.0452;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;100;-1683.248,-587.3253;Inherit;False;Property;_TuftColor;TuftColor;3;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.216981,0.216981,0.216981,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;6;679.9014,-1224.097;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;23;685.4568,-855.3179;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;32;680.4568,-480.3179;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;93;-1665.416,-395.1251;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;72;-1646.421,224.9687;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-1687.005,-206.9381;Inherit;False;Property;_TopColor;TopColor;2;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.2274508,0.6784314,0.2666665,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;85;-1386.558,482.9572;Inherit;True;2;0;FLOAT;0.89;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;937.3073,-895.7141;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-1246.208,305.5779;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-1320.342,-117.9711;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;82;-1403.171,-392.7567;Inherit;True;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;-1299.248,-523.3253;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;88;-1108.585,-134.5526;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;98;-1091.342,-385.9711;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;1041.807,-999.3718;Inherit;False;Property;_Gridfactor;Grid factor;1;0;Create;True;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-1708.439,25.05325;Inherit;False;Property;_SideColor;SideColor;4;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.7264151,0.5008718,0.1816037,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;38;1168.307,-896.7141;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;1352.412,-937.7061;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1091.85,446.0388;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1361.785,74.23619;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-864.585,-199.5526;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-768.2871,237.2977;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;50;-232.333,343.584;Inherit;False;934.1725;553.4321;Comment;7;43;42;41;40;54;39;49;Overlay grid;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;1544.255,-939.2506;Inherit;False;gridMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-181.333,709.0159;Inherit;False;48;gridMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-543.5678,270.0968;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;40;18.65556,580.6704;Inherit;False;Property;_Gridalbedo;Grid albedo;6;0;Create;True;0;0;False;0;False;0,0.8584906,0.02163776,0;0,0.8588235,0.02352941,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;54;0.1264985,406.9294;Inherit;False;53;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;39;13.5557,501.2697;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;239.7392,665.8851;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;240.9395,446.584;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;78;-1409.461,989.9041;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.41;False;2;FLOAT;1.45;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;474.8393,543.6844;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;58;-1425.765,717.3109;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;95;-2254.709,-302.2669;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;83;-769.8721,615.0433;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-2417.709,-296.2669;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;39,39;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-1143.461,982.9041;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;934.962,499.6576;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DevGrid;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;70;0;55;0
WireConnection;51;0;70;0
WireConnection;68;0;67;0
WireConnection;27;0;36;0
WireConnection;18;0;36;0
WireConnection;10;0;36;0
WireConnection;92;0;51;1
WireConnection;15;0;35;0
WireConnection;26;0;35;0
WireConnection;64;0;68;0
WireConnection;64;1;63;0
WireConnection;17;0;35;0
WireConnection;28;0;2;3
WireConnection;28;2;8;0
WireConnection;28;3;26;0
WireConnection;28;4;27;0
WireConnection;91;0;92;0
WireConnection;66;0;71;0
WireConnection;66;1;64;0
WireConnection;57;0;51;0
WireConnection;9;0;2;1
WireConnection;9;2;8;0
WireConnection;9;3;15;0
WireConnection;9;4;10;0
WireConnection;19;0;2;2
WireConnection;19;2;8;0
WireConnection;19;3;17;0
WireConnection;19;4;18;0
WireConnection;22;0;12;0
WireConnection;22;1;13;0
WireConnection;31;0;12;0
WireConnection;31;1;13;0
WireConnection;7;0;9;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;33;0;28;0
WireConnection;24;0;19;0
WireConnection;96;0;91;0
WireConnection;65;0;66;0
WireConnection;65;1;57;0
WireConnection;6;0;7;0
WireConnection;6;1;12;0
WireConnection;6;2;14;0
WireConnection;23;0;24;0
WireConnection;23;1;12;0
WireConnection;23;2;22;0
WireConnection;32;0;33;0
WireConnection;32;1;12;0
WireConnection;32;2;31;0
WireConnection;93;1;96;0
WireConnection;72;0;66;0
WireConnection;85;1;65;0
WireConnection;37;0;6;0
WireConnection;37;1;23;0
WireConnection;37;2;32;0
WireConnection;102;0;72;0
WireConnection;102;1;85;0
WireConnection;99;0;3;0
WireConnection;99;1;100;0
WireConnection;82;0;3;0
WireConnection;82;1;93;0
WireConnection;101;0;3;0
WireConnection;101;1;100;0
WireConnection;88;0;102;0
WireConnection;98;0;82;0
WireConnection;98;1;101;0
WireConnection;98;2;99;0
WireConnection;38;0;37;0
WireConnection;45;0;46;0
WireConnection;45;1;38;0
WireConnection;60;0;98;0
WireConnection;60;1;85;0
WireConnection;74;0;73;0
WireConnection;74;1;72;0
WireConnection;89;0;3;0
WireConnection;89;1;88;0
WireConnection;75;0;74;0
WireConnection;75;1;60;0
WireConnection;75;2;89;0
WireConnection;48;0;45;0
WireConnection;53;0;75;0
WireConnection;39;0;49;0
WireConnection;42;0;40;0
WireConnection;42;1;49;0
WireConnection;41;0;54;0
WireConnection;41;1;39;0
WireConnection;78;0;65;0
WireConnection;43;0;41;0
WireConnection;43;1;42;0
WireConnection;58;0;65;0
WireConnection;95;0;94;0
WireConnection;94;0;51;1
WireConnection;79;0;78;0
WireConnection;79;1;78;0
WireConnection;0;0;43;0
ASEEND*/
//CHKSM=0A9F25494BEF4084B9960E79382134A281C3D723