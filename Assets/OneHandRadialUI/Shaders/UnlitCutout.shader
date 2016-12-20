Shader "LeapMotion/UnlitCutout" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)
    _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 0)
    _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "Queue"="AlphaTest" "RenderType"="Opaque" }
		LOD 200
    AlphaToMask On
		
		CGPROGRAM
		#pragma surface surf Standard noforwardadd alphatest:_Cutoff addshadow
		#pragma target 2.0

		sampler2D _MainTex;
		fixed4 _Color;
    fixed4 _AdditiveColor;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 texInfo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
      
      o.Alpha = texInfo.a * _Color.a;
      o.Emission = texInfo.rgb + _AdditiveColor.rgb;
		}
		ENDCG
	}
	Fallback Off
}
