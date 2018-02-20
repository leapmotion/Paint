Shader "Custom/Outline Hand Standard" {
  Properties {
    _Color        ("Color",         Color)      = (1,1,1,1)
    _Outline      ("Outline Color", Color)      = (1,1,1,1)
    _OutlineWidth ("Outline Width", Float)      = 0.002
    _Glossiness   ("Smoothness",    Range(0,1)) = 0.5
    _Metallic     ("Metallic",      Range(0,1)) = 0.0
    [MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0
    [MaterialToggle] _reverseNormals("Reverse normals?", Int) = 0
  }

  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
    
  // Use shader model 3.0 target, to get nicer looking lighting
  //#pragma target 3.0

  float4 _Color;
  float4 _Outline;
  half _OutlineWidth;
  half _Glossiness;
  half _Metallic;
  int _isLeftHand;
  int _reverseNormals;

  struct appdata2 {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
  };

  struct v2f {
    float4 vertex : SV_POSITION;
  };
  struct v2f_normal {
    float4 vertex : SV_POSITION;
    float3 normal : NORMAL;
  };

  //void vert_model(inout appdata_full v) {
  //  v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
  //  v.vertex = UnityObjectToClipPos(v.vertex);
  //  if (_reverseNormals) v.normal = -v.normal;
  //}

  v2f vert_outline(appdata2 v) {
    v2f o;
    v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand); // late-latch support
    o.vertex = UnityObjectToClipPos(v.vertex + float4(_OutlineWidth * v.normal, 0));
    return o;
  }

  fixed4 frag_outline(v2f i) : SV_Target{
    return _Outline;
  }

  ENDCG

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		LOD 80
    ZWrite On

    Cull Back

    CGPROGRAM

    // Physically based Standard lighting model, and enable shadows on all light types
    #pragma surface surf Standard fullforwardshadows

		struct Input {
      float4 color : COLOR;
		};

    void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

    ENDCG

    Pass{
      Cull Front

      CGPROGRAM

      #pragma vertex vert_outline
      #pragma fragment frag_outline

      ENDCG
    }
	}
}


//Shader "Unlit/FakeLit Outline" {
//  Properties {
//    _Color   ("Color",         Color) = (1,1,1,1)
//    _Outline ("Outline Color", Color) = (1,1,1,1)
//    _Width   ("Outline Width", Float) = 0.01
//  }
//
//  CGINCLUDE
//  #include "UnityCG.cginc"
//  #pragma fragmentoption ARB_precision_hint_fastest
//  #pragma target 2.0
//
//  float4 _Color;
//  float4 _Outline;
//  float _Width;
//  int _isLeftHand;
//
//  struct appdata {
//    float4 vertex : POSITION;
//    float3 normal : NORMAL;
//  };
//
//  struct v2f_n {
//    float4 vertex : SV_POSITION;
//    float3 normal : NORMAL;
//  };
//
//  struct v2f {
//    float4 vertex : SV_POSITION;
//  };
//
//  v2f_n vert_rev_extrude(appdata v) {
//    v2f_n o;
//    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * -1.0 * v.normal, 0));
//    o.normal = v.normal;
//    return o;
//  }
//
//  v2f vert_extrude(appdata v) {
//    v2f o;
//    o.vertex = UnityObjectToClipPos(v.vertex + float4(_Width * 0.0 * v.normal, 0));
//    return o;
//  }
//
//  fixed4 frag(v2f_n i) : SV_Target {
//    fixed4 color = fixed4(1,1,1,1);
//    float litAmount = dot(normalize(i.normal.xyz), normalize(float3(1, 1.3, 0)));
//    color = litAmount * 0.25 + color;
//    color *= _Color;
//    return color;
//  }
//
//  fixed4 frag_outline(v2f i) : SV_Target{
//    return _Outline;
//  }
//
//  ENDCG
//
//	SubShader {
//		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
//		LOD 100
//
//    Pass {
//      Cull Back
//      ColorMask 0
//
//      CGPROGRAM
//      #pragma vertex vert_rev_extrude
//      #pragma fragment frag
//      ENDCG
//    }
//
//    Pass{
//      Cull Front
//
//      CGPROGRAM
//      #pragma vertex vert_extrude
//      #pragma fragment frag_outline
//      ENDCG
//    }
//	}
//}
