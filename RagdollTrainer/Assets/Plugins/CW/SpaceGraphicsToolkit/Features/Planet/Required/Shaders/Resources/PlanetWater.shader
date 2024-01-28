Shader "Hidden/SgtPlanetWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "bump" {}
		_OffsetTex ("Offset Tex", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _OffsetTex;
			float _Age;
			float _NormalStrength;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv     : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out v2f o)
			{
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.uv     = i.uv;
			}

			void Frag(v2f i, out f2g o)
			{
				float offset = tex2D(_OffsetTex, i.uv).w * 6.2832f + _Age;
				float time   = sin(offset) * 0.5f + 0.5f;

				float2 sampleA = UnpackNormal(tex2D(_MainTex, i.uv));
				float2 sampleB = UnpackNormal(tex2D(_MainTex, i.uv + 0.5f));
				float3 normal;

				normal.xy = lerp(sampleA, sampleB, time) * _NormalStrength;
				normal.z  = sqrt(1.0f - saturate(dot(normal.xy, normal.xy)));

				o.color.xyz = normal * 0.5f + 0.5f;
				o.color.w   = 1.0f;
			}
			ENDCG
		}
	}
}
