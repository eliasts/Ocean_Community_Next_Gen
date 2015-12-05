Shader "Mobile/ColoredBumpedDiffuse" {
    Properties {
	    _Color ("Main Color", Color) = (1,1,1,1)
	    _MainTex ("Base (RGB)", 2D) = "white" {}
	    _BumpMap ("Normalmap", 2D) = "bump" {}
    }

    SubShader {
	    Tags { "RenderType" = "Opaque" "Queue"="Geometry"}

        CGPROGRAM
        #pragma surface surf Lambert
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half4 _Color;

        struct Input {
	        float2 uv_MainTex;
	        float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
	        half3 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	        o.Albedo = tex;
        }
        ENDCG  
    }
    FallBack "Diffuse"
}
