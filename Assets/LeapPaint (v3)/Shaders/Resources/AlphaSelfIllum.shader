// http://answers.unity3d.com/answers/18175/view.html
Shader "Unlit/AlphaSelfIllum" {
    Properties {
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("SelfIllum Color (RGB) Alpha (A)", 2D) = "white"
    }
    Category {
       Lighting On
       ZWrite Off
       Cull Back
       Blend SrcAlpha OneMinusSrcAlpha
       Tags {"Queue"="Transparent-1"}
       SubShader {
            Material {
               Emission [_Color]
            }
            Pass {
               SetTexture [_MainTex] {
                      Combine Texture * Primary, Texture * Primary
                }
            }
        } 
    }
}