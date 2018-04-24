// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MetaShader" {
  Properties{ _MainTex("", any) = "" {} }
  CGINCLUDE
  #include "UnityCG.cginc"

  sampler2D _MainTex;
  half4 _MainTex_TexelSize;

  struct v2f {
    float4 pos : SV_POSITION;
    half2 taps[5] : TEXCOORD1;
  };

  v2f vert(appdata_img v) {
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);

    float2 offset = _MainTex_TexelSize;

    o.taps[0] = v.texcoord;
    o.taps[1] = v.texcoord + float2(offset.x, 0);
    o.taps[2] = v.texcoord - float2(offset.x, 0);
    o.taps[3] = v.texcoord + float2(0, offset.y);
    o.taps[4] = v.texcoord - float2(0, offset.y);
    return o;
  }

  half4 frag(v2f i) : SV_Target{
    float4 n = tex2D(_MainTex, i.taps[0]);
    n += tex2D(_MainTex, i.taps[1]);
    n += tex2D(_MainTex, i.taps[2]);
    n += tex2D(_MainTex, i.taps[3]);
    n += tex2D(_MainTex, i.taps[4]);

    //return float4(0, 0, 0, 0);
    return float4(n.xyz / 5.0, 0);
  }
  
  ENDCG
  SubShader {
    ZTest Always Cull Off ZWrite Off

    Pass{
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
    }
  }
  Fallback off
}
