Shader "Terrain/Draw/Vertex"
{
	Properties
	{
	    // 绘制颜色
		_DrawColor ("DrawColor", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
	
	    // 渲染队列
	    Tags { "Queue" = "Overlay" }
		// 剔除关闭 & 深度写入关闭 & 一直深度测试
		Cull off ZWrite Off ZTest Always

		Pass
		{
		    // 最后被渲染
		    // 不透明物体 & 没有阴影
		    Tags { "RenderType" = "Opaque" "ForceNoShadowCasting" = "False" }
		    
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// 顶点着色器
			float4 vert (float4 v : POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(v);
			}
			
			float4 _DrawColor;

            // 片元着色器
			fixed4 frag () : SV_Target
			{
				return _DrawColor;
			}
			ENDCG
		}
	}
}
