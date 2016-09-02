Shader "Leap Shell/TransparentZWrite" {
	Properties {
    _Color   ("Tint", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent-50" "RenderType"="Transparent" }
		LOD 100

    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite On
    ZTest On

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
        float4 color : COLOR;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
        float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
      float4 _Color;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.color = v.color * _Color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
        fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				return col;
			}
			ENDCG
		}
	}
}
