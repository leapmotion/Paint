Shader "Custom/GhostShader" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
 
SubShader {
    Tags {"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
    LOD 200
   
    Pass {
        ZWrite On
        ColorMask 0
   
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
 
        struct v2f {
            float4 pos : SV_POSITION;
        };
 
        v2f vert (appdata_base v)
        {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
            return o;
        }
 
        half4 frag (v2f i) : COLOR
        {
            return half4 (0, 0, 0, 0);
        }
        ENDCG  
    }
   
    CGPROGRAM
    #pragma surface surf Lambert alpha
    #pragma debug
 
    sampler2D _MainTex;
    fixed4 _Color;
 
    struct Input {
        float2 uv_MainTex;
    };
 
    void surf (Input IN, inout SurfaceOutput o) {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        o.Alpha = _Color.a;
    }
    ENDCG
}
 
Fallback "Transparent/Diffuse"
}