Shader "Unlit/Outline Hand" {
  Properties {
    _Color   ("Color",         Color) = (1,1,1,1)
    _Outline ("Outline Color", Color) = (1,1,1,1)
    _Width   ("Outline Width", Float) = 0.01
    [MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
  }

  CGINCLUDE
  #include "UnityCG.cginc"
#include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
  #pragma fragmentoption ARB_precision_hint_fastest
  #pragma target 2.0

  float4 _Color;
  float4 _Outline;
  float _Width;
  int _isLeftHand;

  struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
  };

  struct v2f {
    float4 vertex : SV_POSITION;
  };

  v2f vert(appdata v) {
    v2f o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex);
    return o;
  }

  v2f vert_extrude(appdata v) {
    v2f o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * v.normal, 0));
    return o;
  }

  fixed4 frag(v2f i) : SV_Target {
    return _Color;
  }

  fixed4 frag_outline(v2f i) : SV_Target{
    return _Outline;
  }

  ENDCG

	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

    Pass {
      Cull Back

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }

    Pass{
      Cull Front

      CGPROGRAM
      #pragma vertex vert_extrude
      #pragma fragment frag_outline
      ENDCG
    }
	}
}
