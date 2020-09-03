// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DevGrid"
{
	Properties
	{
		_Gridthreshold("Grid threshold", Float) = 0.5
		_Albedo("Albedo", Color) = (0.9056604,0.9056604,0.9056604,0)
		_Gridspacing("Grid spacing", Float) = 1
		_Gridalbedo("Grid albedo", Color) = (0,0.8584906,0.02163776,0)
		_Gridsoftness("Grid softness", Float) = 0.1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _Gridalbedo;
		uniform float _Gridthreshold;
		uniform float _Gridsoftness;
		uniform float _Gridspacing;
		uniform float4 _Albedo;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float smoothstepResult6 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.x - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float smoothstepResult23 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.y - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float smoothstepResult32 = smoothstep( _Gridthreshold , ( _Gridthreshold + _Gridsoftness ) , sin( (( 0.5 * UNITY_PI ) + (ase_worldPos.z - 0.0) * (( 2.5 * UNITY_PI ) - ( 0.5 * UNITY_PI )) / (_Gridspacing - 0.0)) ));
			float clampResult38 = clamp( ( smoothstepResult6 + smoothstepResult23 + smoothstepResult32 ) , 0.0 , 1.0 );
			o.Albedo = ( ( _Gridalbedo * clampResult38 ) + ( _Albedo * ( 1.0 - clampResult38 ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
75;117;1736;1205;97.26605;12.65192;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;36;-1064.73,434.505;Inherit;False;Constant;_Float1;Float 0;10;0;Create;True;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1065.73,359.505;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;10;-815.5522,425.0078;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;2;-810.5522,146.0078;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PiNode;27;-814.9966,1168.786;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-797.5522,287.0078;Inherit;False;Property;_Gridspacing;Grid spacing;2;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;15;-815.5522,361.0078;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;26;-814.9966,1104.786;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;17;-809.9966,729.7856;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;18;-809.9966,793.7856;Inherit;False;1;0;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-568.9966,930.7855;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-543.5522,346.0078;Inherit;False;Property;_Gridthreshold;Grid threshold;0;0;Create;True;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;9;-569.5522,187.0078;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;19;-563.9966,555.7856;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;6.28;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-544.5522,441.0078;Inherit;False;Property;_Gridsoftness;Grid softness;4;0;Create;True;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;24;-367.9966,557.7856;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-340.9966,788.7856;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;33;-372.9967,932.7855;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;7;-373.5522,189.0078;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-346.5522,420.0078;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-345.9967,1163.786;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;23;-219.9967,558.7856;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;32;-224.9968,933.7855;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;6;-225.5522,190.0078;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;31.85327,518.39;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;38;262.8533,517.39;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;447.4504,293.5336;Inherit;False;Property;_Gridalbedo;Grid albedo;3;0;Create;True;0;0;False;0;False;0,0.8584906,0.02163776,0;0,0.8584906,0.02163776,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;39;479.4504,761.5336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;449.601,561.2865;Inherit;False;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;False;0.9056604,0.9056604,0.9056604,0;0.9056604,0.9056604,0.9056604,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;696.7339,643.3481;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;707.7339,370.3481;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;887.7339,457.3481;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1214,350;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DevGrid;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;36;0
WireConnection;27;0;36;0
WireConnection;15;0;35;0
WireConnection;26;0;35;0
WireConnection;17;0;35;0
WireConnection;18;0;36;0
WireConnection;28;0;2;3
WireConnection;28;2;8;0
WireConnection;28;3;26;0
WireConnection;28;4;27;0
WireConnection;9;0;2;1
WireConnection;9;2;8;0
WireConnection;9;3;15;0
WireConnection;9;4;10;0
WireConnection;19;0;2;2
WireConnection;19;2;8;0
WireConnection;19;3;17;0
WireConnection;19;4;18;0
WireConnection;24;0;19;0
WireConnection;22;0;12;0
WireConnection;22;1;13;0
WireConnection;33;0;28;0
WireConnection;7;0;9;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;31;0;12;0
WireConnection;31;1;13;0
WireConnection;23;0;24;0
WireConnection;23;1;12;0
WireConnection;23;2;22;0
WireConnection;32;0;33;0
WireConnection;32;1;12;0
WireConnection;32;2;31;0
WireConnection;6;0;7;0
WireConnection;6;1;12;0
WireConnection;6;2;14;0
WireConnection;37;0;6;0
WireConnection;37;1;23;0
WireConnection;37;2;32;0
WireConnection;38;0;37;0
WireConnection;39;0;38;0
WireConnection;41;0;3;0
WireConnection;41;1;39;0
WireConnection;42;0;40;0
WireConnection;42;1;38;0
WireConnection;43;0;42;0
WireConnection;43;1;41;0
WireConnection;0;0;43;0
ASEEND*/
//CHKSM=57FCE6C5EA7CED0E0F61FED70542E8C56E3A8732