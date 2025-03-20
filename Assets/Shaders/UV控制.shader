Shader "三角猫Shader/UVTransform" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Rotation ("Rotation", Range(0, 360)) = 0
        _Scale ("Scale", Vector) = (1,1,0,0)
        _Offset ("Offset", Vector) = (0,0,0,0)
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;
            float2 _Scale;
            float2 _Offset;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2x2 buildRotationMatrix(float angle) {
                float rad = radians(angle);
                float s, c;
                sincos(rad, s, c);
                return float2x2(c, -s, s, c);
            }

            fixed4 frag (v2f i) : SV_Target {
                // 中心点偏移
                float2 uv = i.uv - 0.5;
                
                // 构建变换矩阵
                float2x2 rotMat = buildRotationMatrix(_Rotation);
                uv = mul(rotMat, uv); // 旋转
                uv *= _Scale;         // 缩放
                uv += _Offset;        // 位移
                
                // 恢复坐标系
                uv += 0.5;
                
                // 边界处理
                uv = frac(uv);
                
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}