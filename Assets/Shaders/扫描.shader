Shader "三角猫Shader/圆形扩散扫描"
{
    Properties
    {
        // 主要纹理，用于物体表面的纹理
        _MainTex ("Texture", 2D) = "white" {}

        // 扫描纹理，用于显示扫描的图像或效果
        _ScanTex("ScanTexure", 2D) = "white" {}

        // 扫描的范围，控制扫描从中心开始的距离
        _ScanRange("ScanRange", float) = 0

        // 扫描宽度，控制扫描的宽度，影响扫描的可见区域
        _ScanWidth("ScanWidth", float) = 0

        // 扫描的背景颜色
        _ScanBgColor("ScanBgColor", color) = (1, 1, 1, 1)

        // 扫描时网格的颜色
        _ScanMeshColor("ScanMeshColor", color) = (1, 1, 1, 1)

        // 网格线的宽度
        _MeshLineWidth("MeshLineWidth", float) = 0.3

        // 网格的宽度，用于控制网格分割的尺寸
        _MeshWidth("MeshWidth", float) = 1

        // 缝隙的平滑度，控制缝隙的过渡效果
        _Smoothness("SeamBlending", Range(0, 0.5)) = 0.25
    }

    SubShader
    {
        // 不使用背面剔除（Cull Off），不写入深度缓存（ZWrite Off），且始终通过深度测试（ZTest Always）
        //Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 引用Unity的内置CG代码库
            #include "UnityCG.cginc"

            // 定义顶点着色器的数据结构
            struct appdata
            {
                float4 vertex : POSITION; // 顶点位置
                float2 uv : TEXCOORD0; // 顶点纹理坐标
            };

            // 定义片段着色器的数据结构
            struct v2f
            {
                float2 uv : TEXCOORD0; // 传递给片段着色器的纹理坐标
                float2 uv_depth : TEXCOORD1; // 用于传递深度信息的纹理坐标
                float4 interpolatedRay : TEXCOORD2; // 传递与扫描效果相关的视锥体数据
                float4 vertex : SV_POSITION; // 顶点最终位置
            };

            // 定义一个矩阵，用于描述视锥体四个角的坐标
            float4x4 _FrustumCorner;

            // 顶点着色器：计算顶点的最终位置，并计算视锥体四个角的插值数据
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 计算顶点的最终位置
                o.uv = v.uv; // 将纹理坐标传递给片段着色器
                o.uv_depth = v.uv; // 将纹理坐标传递给深度计算

                // 根据UV坐标的不同区域选择不同的视锥体角
                int rayIndex;
                if (v.uv.x < 0.5 && v.uv.y < 0.5)
                {
                    rayIndex = 0;
                }
                else if (v.uv.x > 0.5 && v.uv.y < 0.5)
                {
                    rayIndex = 1;
                }
                else if (v.uv.x > 0.5 && v.uv.y > 0.5)
                {
                    rayIndex = 2;
                }
                else
                {
                    rayIndex = 3;
                }

                // 从视锥体四个角中选择一个，传递给片段着色器
                o.interpolatedRay = _FrustumCorner[rayIndex];

                return o;
            }

            // 声明材质参数，允许外部设置
            sampler2D _MainTex;
            sampler2D _ScanTex;
            float _ScanRange;
            float _ScanWidth;
            float3 _ScanCenter;
            fixed4 _ScanBgColor;
            fixed4 _ScanMeshColor;
            float _MeshLineWidth;
            float _MeshWidth;
            float4x4 _CamToWorld;
            fixed _Smoothness;

            // 声明用于获取深度信息的纹理
            sampler2D_float _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;

            // 片段着色器：计算像素的最终颜色
            fixed4 frag(v2f i) : SV_Target
            {
                float tempDepth;
                half3 normal;  
                
                // 获取该像素的法线和深度值
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), tempDepth, normal);
                
                // 将法线从相机空间转换到世界空间
                normal = mul((float3x3)_CamToWorld, normal);
                normal = normalize(max(0, (abs(normal) - _Smoothness)));  // 对法线进行平滑处理

                // 获取该像素的颜色
                fixed4 col = tex2D(_MainTex, i.uv);

                // 通过深度纹理获取该像素的深度值，并转换为线性深度
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
                float linearDepth = Linear01Depth(depth);

                // 计算该像素在世界空间中的位置
                float3 pixelWorldPos = _WorldSpaceCameraPos + linearDepth * i.interpolatedRay;

                // 计算像素到扫描中心的距离
                float pixelDistance = distance(pixelWorldPos, _ScanCenter);

                // 计算该像素的方向向量
                float3 pixelDir = pixelWorldPos - _ScanCenter;

                // 使用网格宽度将像素位置进行取模操作，用于创建网格效果
                float3 modulo = pixelWorldPos - _MeshWidth * floor(pixelWorldPos / _MeshWidth);
                modulo = modulo / _MeshWidth;

                // 使用平滑插值创建网格线效果
                float3 meshCol = smoothstep(_MeshLineWidth, 0, modulo) + smoothstep(1 - _MeshLineWidth, 1, modulo);

                // 将扫描背景颜色和网格颜色进行插值
                fixed4 scanMeshCol = lerp(_ScanBgColor, _ScanMeshColor, saturate(dot(meshCol, 1 - normal)));

                // 如果像素距离扫描中心在指定范围内，执行扫描效果
                if (_ScanRange - pixelDistance > 0 && _ScanRange - pixelDistance < _ScanWidth && linearDepth < 1)
                {
                    // 根据像素距离计算扫描百分比
                    fixed scanPercent = 1 - (_ScanRange - pixelDistance) / _ScanWidth;

                    // 通过插值将当前颜色与网格颜色混合，生成扫描效果
                    col = lerp(col, scanMeshCol, scanPercent);
                }

                // 返回最终颜色
                return col;
            }
            ENDCG
        }
    }
}