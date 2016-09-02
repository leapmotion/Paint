Shader "Leap Shell/StencilBlocker" {
	Properties { }
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

    Cull Back

    Stencil {
      Ref 4
      ReadMask 4
      WriteMask 4
      Comp Always
      Pass Replace
      Fail Replace
      ZFail Replace
    }

    ColorMask 0
    ZWrite Off

    Pass { }
	}
}
