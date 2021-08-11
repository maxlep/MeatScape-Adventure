Shader "Hidden/MicroSplatMeshAutoDampening"
{
   Properties
   {
      _MainTex ("Texture", 2D) = "white" {}
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
         float4 _MainTex_TexelSize;

         v2f vert (appdata v)
         {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            
            return o;
         }
         
         fixed4 frag (v2f i) : SV_Target
         {
            // shrink area by one pixel
            float val = 0;
            float2 o = _MainTex_TexelSize.xy;

            val += tex2D(_MainTex, i.uv).g;
            val += tex2D(_MainTex, i.uv + o).g;
            val += tex2D(_MainTex, i.uv - o).g;
            val += tex2D(_MainTex, i.uv + float2(o.x, 0)).g;
            val += tex2D(_MainTex, i.uv + float2(-o.x, 0)).g;
            val += tex2D(_MainTex, i.uv + float2(0, o.y)).g;
            val += tex2D(_MainTex, i.uv + float2(0, -o.y)).g;
            val += tex2D(_MainTex, i.uv + float2(-o.x, o.y)).g;
            val += tex2D(_MainTex, i.uv + float2(o.x, -o.y)).g;

            if (val < 9)
            {
                return fixed4(1,1,1,1);
            }

            return fixed4(0,0,0,1);
            
         }
         ENDCG
      }
   }
}
