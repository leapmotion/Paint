Shader "Leap Shell/Gradient Transparency" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
    _Gradient ("Gradient", Vector) = (1,1,1,1)
	}

	SubShader { 
    Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			
			struct appdata_t {
				float4 vertex   : POSITION;
				float2 uv : TEXCOORD0;
        float2 uvPos : TEXCOORD1;
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				half2 uv  : TEXCOORD0;
        half2 uvPos : TEXCOORD1;
			};
			
			fixed4 _Color;
      fixed4 _Gradient;

			v2f vert(appdata_t IN) {
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv = IN.uv;
        OUT.uvPos = IN.uvPos;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target {
				half4 color = tex2D(_MainTex, IN.uv) * _Color;
        float alpha = saturate(pow(1.0 - (IN.uvPos.x - _Gradient.x) / _Gradient.y, _Gradient.z) * _Gradient.w);
        color.a *= alpha;

				return color;
			}
		ENDCG
		}
	}
}
