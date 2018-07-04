Shader "Unlit/Lightning"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaTex("Alpha Tex", 2D) = "white" {}
		_HDRMulti("HDR Multi", Range(1, 5)) = 1
		_AlphaScale("Scale", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCoutout" }
		LOD 100
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainTex_ST;
			float _HDRMulti;
			float _AlphaScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = tex2D(_AlphaTex, i.uv).r;
				clip(col.a - _AlphaScale);
				return fixed4(col.rgb * _HDRMulti, 1);
			}
			ENDCG
		}
	}
}
