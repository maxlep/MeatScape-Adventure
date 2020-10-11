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
		_Gridalignmentthreshold("Grid alignment threshold", Float) = 0
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
		uniform float _Gridalignmentthreshold;
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
			float gridThreshold117 = _Gridthreshold;
			float gridSoftness118 = _Gridsoftness;
			float gridSpacing108 = _Gridspacing;
			float gridMin104 = ( 0.5 * UNITY_PI );
			float gridMax105 = ( 2.5 * UNITY_PI );
			float smoothstepResult6 = smoothstep( gridThreshold117 , ( gridThreshold117 + gridSoftness118 ) , sin( (gridMin104 + (ase_worldPos.x - 0.0) * (gridMax105 - gridMin104) / (gridSpacing108 - 0.0)) ));
			float dotResult125 = dot( ase_worldNormal , float3( 1,0,0 ) );
			float gridAlignmentThreshold129 = _Gridalignmentthreshold;
			float xAlignment145 = step( abs( dotResult125 ) , gridAlignmentThreshold129 );
			float smoothstepResult23 = smoothstep( gridThreshold117 , ( gridThreshold117 + gridSoftness118 ) , sin( (gridMin104 + (ase_worldPos.y - 0.0) * (gridMax105 - gridMin104) / (gridSpacing108 - 0.0)) ));
			float dotResult126 = dot( ase_worldNormal , float3( 0,1,0 ) );
			float yAlignment146 = step( abs( dotResult126 ) , gridAlignmentThreshold129 );
			float smoothstepResult32 = smoothstep( gridThreshold117 , ( gridThreshold117 + gridSoftness118 ) , sin( (gridMin104 + (ase_worldPos.z - 0.0) * (gridMax105 - gridMin104) / (gridSpacing108 - 0.0)) ));
			float dotResult127 = dot( ase_worldNormal , float3( 0,0,1 ) );
			float zAlignment147 = step( abs( dotResult127 ) , gridAlignmentThreshold129 );
			float clampResult38 = clamp( ( ( smoothstepResult6 * xAlignment145 ) + ( smoothstepResult23 * yAlignment146 ) + ( smoothstepResult32 * zAlignment147 ) ) , 0.0 , 1.0 );
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
0;73;1910;1286;543.8055;1959.079;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;47;-555.0463,-1908.294;Inherit;False;2638.364;1868.732;;40;2;48;45;46;38;37;138;135;139;6;32;23;150;148;149;14;7;33;31;22;24;121;9;123;124;28;19;120;122;119;109;111;115;107;106;113;112;114;110;116;Grid mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;116;996.3151,-1780.339;Inherit;False;1037.997;1307.086;Comment;28;129;117;118;128;105;13;108;12;104;10;8;15;35;36;103;127;125;126;140;142;131;141;137;133;136;145;147;146;Grid vars;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;36;1048.811,-1550.159;Inherit;False;Constant;_Float1;Float 1;10;0;Create;True;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;1049.208,-1644.723;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;15;1208.555,-1639.028;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;1046.315,-1730.339;Inherit;False;Property;_Gridspacing;Grid spacing;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;10;1208.555,-1545.682;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;55;-2467.018,585.3376;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;128;1036.138,-1267.371;Inherit;False;Property;_Gridalignmentthreshold;Grid alignment threshold;8;0;Create;True;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;103;1031.284,-966.6012;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;127;1281.619,-838.5296;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;126;1281.619,-969.5299;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,1,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;1033.14,-1450.512;Inherit;False;Property;_Gridthreshold;Grid threshold;0;0;Create;True;0;0;False;0;False;0.5;0.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;108;1223.524,-1729.968;Inherit;False;gridSpacing;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;129;1293.139,-1265.371;Inherit;False;gridAlignmentThreshold;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;67;-2452.445,223.4314;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;104;1398.62,-1643.393;Inherit;False;gridMin;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;70;-2272.445,582.4314;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;1401.854,-1549.075;Inherit;False;gridMax;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;1035.14,-1363.512;Inherit;False;Property;_Gridsoftness;Grid softness;7;0;Create;True;0;0;False;0;False;0.1;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;125;1279.283,-1095.19;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;1269.425,-707.2137;Inherit;False;129;gridAlignmentThreshold;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;141;1414.823,-968.8617;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-188.6207,-1228.799;Inherit;False;108;gridSpacing;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-185.6206,-1060.799;Inherit;False;105;gridMax;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-200.8484,-516.3152;Inherit;False;104;gridMin;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;68;-2259.445,220.4314;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;140;1415.153,-1095.781;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;1223.609,-1451.633;Inherit;False;gridThreshold;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;51;-2052.154,582.9197;Inherit;False;0;0;1;3;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;5.33;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;1225.609,-1361.633;Inherit;False;gridSoftness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-198.8484,-432.3157;Inherit;False;105;gridMax;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-186.363,-1659.904;Inherit;False;105;gridMax;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-201.8484,-600.3161;Inherit;False;108;gridSpacing;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-187.6207,-1144.799;Inherit;False;104;gridMin;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-200.3369,-1813.637;Inherit;False;108;gridSpacing;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-188.363,-1738.417;Inherit;False;104;gridMin;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-510.7274,-1177.368;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;63;-2229.327,409.5438;Inherit;False;Constant;_Vector0;Vector 0;6;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.AbsOpNode;142;1414.823,-839.8616;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;136;1637.456,-970.528;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;137;1638.36,-842.127;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-214.442,-251.8924;Inherit;False;118;gridSoftness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-6.868562,-597.5869;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;133;1638.133,-1096.461;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-203.758,-1484.005;Inherit;False;118;gridSoftness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-206.2142,-966.0298;Inherit;False;117;gridThreshold;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-206.758,-1573.005;Inherit;False;117;gridThreshold;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;19;1.359252,-1229.269;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-217.442,-340.8919;Inherit;False;117;gridThreshold;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-2232.029,111.9826;Inherit;False;Property;_GrassStrictness;Grass Strictness;9;0;Create;True;0;0;False;0;False;0.9;0.3;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;9;17.69666,-1817.294;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-203.2142,-877.0297;Inherit;False;118;gridSoftness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2727.209,-528.2669;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;17,17;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-1991.88,339.2109;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;1791.483,-967.7427;Inherit;False;yAlignment;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;1790.483,-836.7426;Inherit;False;zAlignment;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;197.3597,-1227.269;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;57;-1854.619,577.8378;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;91;-2564.209,-534.2669;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;3,3;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinOpNode;7;213.6971,-1815.294;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;33;189.1316,-595.5869;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;88.21184,-329.5779;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;106.1635,-1565.481;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;66;-1838.219,242.8759;Inherit;False;2;0;FLOAT;0.9;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;145;1791.483,-1095.743;Inherit;False;xAlignment;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;96.43991,-951.8337;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;435.539,-1489.734;Inherit;False;145;xAlignment;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;436.539,-880.7332;Inherit;False;146;yAlignment;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;371.3081,-237.3178;Inherit;False;147;zAlignment;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;96;-2268.209,-476.2669;Inherit;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-1672.375,494.0452;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;6;383.2431,-1729.463;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;32;342.5193,-476.0925;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;23;364.2123,-1125.279;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;599.8969,-274.252;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;93;-1665.416,-395.1251;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;72;-1646.421,224.9687;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;85;-1386.558,482.9572;Inherit;True;2;0;FLOAT;0.89;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;638.5092,-1551.909;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;633.5472,-969.8884;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-1687.005,-206.9381;Inherit;False;Property;_TopColor;TopColor;2;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.2274508,0.6784314,0.2666665,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;100;-1683.248,-587.3253;Inherit;False;Property;_TuftColor;TuftColor;3;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.216981,0.216981,0.216981,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;37;1255.335,-291.0127;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-1246.208,305.5779;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;-1299.248,-523.3253;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-1320.342,-117.9711;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;82;-1403.171,-392.7567;Inherit;True;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;1359.836,-394.6707;Inherit;False;Property;_Gridfactor;Grid factor;1;0;Create;True;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;98;-1091.342,-385.9711;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;73;-1708.439,25.05325;Inherit;False;Property;_SideColor;SideColor;4;0;Create;True;0;0;False;0;False;0.03592289,0.990566,0,0;0.7264151,0.5008718,0.1816037,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;88;-1108.585,-134.5526;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;38;1486.336,-292.0127;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;1670.441,-333.0046;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-864.585,-199.5526;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1091.85,446.0388;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1361.785,74.23619;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;1862.286,-334.5487;Inherit;False;gridMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;50;-232.333,343.584;Inherit;False;934.1725;553.4321;Comment;7;43;42;41;40;54;39;49;Overlay grid;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-768.2871,237.2977;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-543.5678,270.0968;Inherit;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-181.333,709.0159;Inherit;False;48;gridMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;0.1264985,406.9294;Inherit;False;53;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;40;18.65556,580.6704;Inherit;False;Property;_Gridalbedo;Grid albedo;6;0;Create;True;0;0;False;0;False;0,0.8584906,0.02163776,0;0.02830189,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;39;13.5557,501.2697;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;239.7392,665.8851;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;240.9395,446.584;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;78;-1409.461,989.9041;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.41;False;2;FLOAT;1.45;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;95;-2254.709,-302.2669;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;152;-914.3803,792.967;Inherit;False;146;yAlignment;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;58;-1425.765,717.3109;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-1143.461,982.9041;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;474.8393,543.6844;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-2417.709,-296.2669;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;39,39;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;934.962,499.6576;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DevGrid;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;35;0
WireConnection;10;0;36;0
WireConnection;127;0;103;0
WireConnection;126;0;103;0
WireConnection;108;0;8;0
WireConnection;129;0;128;0
WireConnection;104;0;15;0
WireConnection;70;0;55;0
WireConnection;105;0;10;0
WireConnection;125;0;103;0
WireConnection;141;0;126;0
WireConnection;68;0;67;0
WireConnection;140;0;125;0
WireConnection;117;0;12;0
WireConnection;51;0;70;0
WireConnection;118;0;13;0
WireConnection;142;0;127;0
WireConnection;136;0;141;0
WireConnection;136;1;131;0
WireConnection;137;0;142;0
WireConnection;137;1;131;0
WireConnection;28;0;2;3
WireConnection;28;2;115;0
WireConnection;28;3;114;0
WireConnection;28;4;113;0
WireConnection;133;0;140;0
WireConnection;133;1;131;0
WireConnection;19;0;2;2
WireConnection;19;2;112;0
WireConnection;19;3;111;0
WireConnection;19;4;110;0
WireConnection;9;0;2;1
WireConnection;9;2;109;0
WireConnection;9;3;106;0
WireConnection;9;4;107;0
WireConnection;92;0;51;1
WireConnection;64;0;68;0
WireConnection;64;1;63;0
WireConnection;146;0;136;0
WireConnection;147;0;137;0
WireConnection;24;0;19;0
WireConnection;57;0;51;0
WireConnection;91;0;92;0
WireConnection;7;0;9;0
WireConnection;33;0;28;0
WireConnection;31;0;124;0
WireConnection;31;1;123;0
WireConnection;14;0;119;0
WireConnection;14;1;120;0
WireConnection;66;0;71;0
WireConnection;66;1;64;0
WireConnection;145;0;133;0
WireConnection;22;0;122;0
WireConnection;22;1;121;0
WireConnection;96;0;91;0
WireConnection;65;0;66;0
WireConnection;65;1;57;0
WireConnection;6;0;7;0
WireConnection;6;1;119;0
WireConnection;6;2;14;0
WireConnection;32;0;33;0
WireConnection;32;1;124;0
WireConnection;32;2;31;0
WireConnection;23;0;24;0
WireConnection;23;1;122;0
WireConnection;23;2;22;0
WireConnection;139;0;32;0
WireConnection;139;1;150;0
WireConnection;93;1;96;0
WireConnection;72;0;66;0
WireConnection;85;1;65;0
WireConnection;138;0;6;0
WireConnection;138;1;148;0
WireConnection;135;0;23;0
WireConnection;135;1;149;0
WireConnection;37;0;138;0
WireConnection;37;1;135;0
WireConnection;37;2;139;0
WireConnection;102;0;72;0
WireConnection;102;1;85;0
WireConnection;101;0;3;0
WireConnection;101;1;100;0
WireConnection;99;0;3;0
WireConnection;99;1;100;0
WireConnection;82;0;3;0
WireConnection;82;1;93;0
WireConnection;98;0;82;0
WireConnection;98;1;101;0
WireConnection;98;2;99;0
WireConnection;88;0;102;0
WireConnection;38;0;37;0
WireConnection;45;0;46;0
WireConnection;45;1;38;0
WireConnection;89;0;3;0
WireConnection;89;1;88;0
WireConnection;60;0;98;0
WireConnection;60;1;85;0
WireConnection;74;0;73;0
WireConnection;74;1;72;0
WireConnection;48;0;45;0
WireConnection;75;0;74;0
WireConnection;75;1;60;0
WireConnection;75;2;89;0
WireConnection;53;0;75;0
WireConnection;39;0;49;0
WireConnection;42;0;40;0
WireConnection;42;1;49;0
WireConnection;41;0;54;0
WireConnection;41;1;39;0
WireConnection;78;0;65;0
WireConnection;95;0;94;0
WireConnection;58;0;65;0
WireConnection;79;0;78;0
WireConnection;79;1;78;0
WireConnection;43;0;41;0
WireConnection;43;1;42;0
WireConnection;94;0;51;1
WireConnection;0;0;43;0
ASEEND*/
//CHKSM=B3BDC26750940772D9ACF9A93015F4739AFC4549