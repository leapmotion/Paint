Shader "LeapMotion/MagicSphereShader" {
	Properties {
		_Tint ("Tint", Color) = (1,1,1,1)
    _BrightColor ("Bright Color", Color) = (1,1,1,1)
    _DimColor ("Dim Color", Color) = (0,0,0,0)
    _RimMultiplier ("Rim Multiplier", Range(-1, 1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque"}
    LOD 200

    Pass {
      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
		    #pragma target 3.0

        #include "UnityCG.cginc"
        #include "noise4d.cginc"
        #include "classicnoise4d.cginc"
        
        // === Noise ===
        // float cnoise(float4 noiseIn) via classicnoise4d
        float noiseFunc(float4 noiseIn) {
          float noiseVal = snoise(noiseIn);
          return noiseVal;
        }

        float noiseSqFunc(float4 noiseIn) {
          float noiseVal = noiseFunc(noiseIn);
          return noiseVal * noiseVal;
        }

        // === VERT / FRAG ===

		    struct VertInput {
          float4 position : POSITION;
          float3 worldNormal : NORMAL;
		    };

        struct FragInput {
          float4 position : SV_POSITION;
          float3 worldNormal : NORMAL;
          float3 viewDir : TEXCOORD0;
          float3 vertPos : TEXCOORD1;
        };

        fixed4 makeFour(fixed3 three, fixed one) {
          return fixed4(three.x, three.y, three.z, one.x);
        }

        FragInput vert(VertInput vIn) {
         FragInput vOut;
         vOut.position = UnityObjectToClipPos(vIn.position);
         vOut.vertPos = vIn.position;
         vOut.worldNormal = vIn.worldNormal;
         vOut.viewDir = normalize(WorldSpaceViewDir(vIn.position));
         return vOut;
        }

		    fixed4 _Tint;
        fixed4 _BrightColor;
        fixed4 _DimColor;
        fixed _RimMultiplier;

        float noiseOctave(float4 xyzt, float phase, float freq, float amp) {
          return noiseSqFunc((xyzt + phase) * freq) * amp;
        }

        fixed4 frag(FragInput fIn) : SV_Target {
          // === Color ===
          float4 tint = _Tint;

          // === Noise ===
          float4 xyzt = makeFour(fIn.vertPos, (100 + _Time.x * 4));
          
          fixed noiseSum = 0;
                noiseSum += noiseOctave(xyzt,    0, 0.8, 0.5);
                noiseSum += noiseOctave(xyzt,   20, 1.0, 0.5);
                noiseSum += noiseOctave(xyzt,  2.5, 1.4, 0.5);
                noiseSum += noiseOctave(xyzt,  300, 2.5, 0.5);
                noiseSum += noiseOctave(xyzt,    2, 4.0, 0.2);
                noiseSum += noiseOctave(xyzt,    2, 5.0, 0.2);

          // === Rim ===
          float rim = 1 - dot(fIn.worldNormal, fIn.viewDir);
          float rimValue = rim;

          // === Output ===
          return tint * lerp(_DimColor, _BrightColor, noiseSum + _RimMultiplier * rimValue);
        }
		  ENDCG
    }
	}
	FallBack "Diffuse"
}
