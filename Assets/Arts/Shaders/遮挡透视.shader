Shader "三角猫Shader/遮挡透视"
{
    Properties
    {
        // 定义属性
        [Header(The Blocked Part)] // 标题，表示以下属性是用于被遮挡部分的设置
        [Space(10)] // 在Inspector中留出10像素的空白
        _Color ("X-Ray Color", Color) = (0,1,1,1) // X射线颜色，默认青色
        _Width ("X-Ray Width", Range(1, 2)) = 1 // X射线宽度，范围1到2，默认1
        _Brightness ("X-Ray Brightness",Range(0, 2)) = 1 // X射线亮度，范围0到2，默认1
    }

    SubShader
    {
        Tags{"RenderType" = "Opaque" "Queue" = "Geometry"} // 渲染类型为不透明，队列为几何体

        //---------- 被遮挡部分的效果 ----------
        Pass
        {
            ZTest Greater // 深度测试设置为大于当前深度值时才渲染（即渲染被遮挡的部分）
            ZWrite Off // 关闭深度写入，避免影响后续渲染

            Blend SrcAlpha OneMinusSrcAlpha // 设置混合模式，实现透明效果

            CGPROGRAM
            #pragma vertex vert // 顶点着色器
            #pragma fragment frag // 片段着色器
            #include "UnityCG.cginc" // 引入Unity的CG库

            // 定义顶点着色器的输出结构
            struct v2f
            {
                float4 vertexPos : SV_POSITION; // 顶点在裁剪空间中的位置
                float3 viewDir : TEXCOORD0; // 视线方向
                float3 worldNor : TEXCOORD1; // 世界空间中的法线方向
            };

            // 顶点着色器
            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertexPos = UnityObjectToClipPos(v.vertex); // 将顶点从对象空间转换到裁剪空间
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex)); // 计算视线方向
                o.worldNor = UnityObjectToWorldNormal(v.normal); // 将法线从对象空间转换到世界空间

                return o;
            }

            // 声明属性变量
            fixed4 _Color; // X射线颜色
            fixed _Width; // X射线宽度
            half _Brightness; // X射线亮度

            // 片段着色器
            float4 frag(v2f i) : SV_Target
            {
                //计算边缘光强度
                half NDotV = saturate(dot(i.worldNor, i.viewDir)); // 计算法线与视线方向的点积
                NDotV = pow(1 - NDotV, _Width) * _Brightness; // 根据宽度和亮度调整边缘光强度

                fixed4 color;
                color.rgb = _Color.rgb; // 设置颜色
                color.a = NDotV; // 设置透明度（基于Fresnel值）
                return color; // 返回最终颜色
            }
            ENDCG
        }
    }
}