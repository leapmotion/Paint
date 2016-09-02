Shader "Leap Shell/VertexAnimated" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
      #pragma multi_compile __ ANIM_1 ANIM_2 ANIM_3
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
        fixed3 normal : NORMAL;
        fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
      fixed3 _Target0;
      fixed3 _Target1;
      fixed3 _Target2;

      fixed2 getFactor(fixed4 color, fixed3 target) {
        return (abs(color.w - target.z) < 0.001) ? target.xy : float2(0, 0);
      }

			v2f vert (appdata v) {
				v2f o;

//#if defined(ANIM_1) || defined(ANIM_2) || defined(ANIM_3)
        fixed2 factor = getFactor(v.color, _Target0);

//#ifdef defined(ANIM_2) || defined(ANIM_3)
        factor += getFactor(v.color, _Target1);
//#ifdef defined(ANIM_3)
        factor += getFactor(v.color, _Target2);
//#endif
//#endif

        v.vertex.xyz += factor.x * v.normal;
        v.vertex.xyz += factor.y * v.color.xyz;
//#endif
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
