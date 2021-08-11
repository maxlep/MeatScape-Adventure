Shader "Hidden/MicroSplatMeshNormalize"
{
   Properties
   {
      _MainTex ("Texture", 2D) = "white" {}
      _Control0("CT0", 2D) = "black" {}
      _Control1("CT1", 2D) = "black" {}
      _Control2("CT2", 2D) = "black" {}
      _Control3("CT3", 2D) = "black" {}
      _Control4("CT4", 2D) = "black" {}
      _Control5("CT5", 2D) = "black" {}
      _Control6("CT6", 2D) = "black" {}
      _Control7("CT7", 2D) = "black" {}
      _ControlIndex("Index", int) = 0
   }
   SubShader
   {
      Tags { "RenderType"="Opaque" }
      LOD 100

      Pass
      {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
         
         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
         };

         struct v2f
         {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
         };

         sampler2D _MainTex;

         int _ControlIndex;   // texture we are modifying
         sampler2D _Control0;
         sampler2D _Control1;
         sampler2D _Control2;
         sampler2D _Control3;
         sampler2D _Control4;
         sampler2D _Control5;
         sampler2D _Control6;
         sampler2D _Control7;
         
         v2f vert (appdata v)
         {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            
            return o;
         }
         
         fixed4 frag (v2f i) : SV_Target
         {
            fixed4 controlBuffer[8];
            controlBuffer[0] = tex2D(_Control0, i.uv);
            controlBuffer[1] = tex2D(_Control1, i.uv);
            controlBuffer[2] = tex2D(_Control2, i.uv);
            controlBuffer[3] = tex2D(_Control3, i.uv);
            controlBuffer[4] = tex2D(_Control4, i.uv);
            controlBuffer[5] = tex2D(_Control5, i.uv);
            controlBuffer[6] = tex2D(_Control6, i.uv);
            controlBuffer[7] = tex2D(_Control7, i.uv);

            float total = 0;
            for (int i = 0; i < 8; ++i)
            {
               total += controlBuffer[i].r;
               total += controlBuffer[i].g;
               total += controlBuffer[i].b;
               total += controlBuffer[i].a;
            }

            total = max(total, 0.001);

            for (int x = 0; x < 8; ++x)
            {
               controlBuffer[x].r /= total;
               controlBuffer[x].g /= total;
               controlBuffer[x].b /= total;
               controlBuffer[x].a /= total;
            }
            return controlBuffer[_ControlIndex];
            
         }
         ENDCG
      }
   }
}
