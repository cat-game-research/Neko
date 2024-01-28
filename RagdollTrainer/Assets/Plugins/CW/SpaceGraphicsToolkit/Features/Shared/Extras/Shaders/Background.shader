Shader "Space Graphics Toolkit/Background"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		[Toggle(_POWER_RGB)] _RgbPower("RGB Power", Float) = 0
		[Toggle(_CLAMP_SIZE)] _ClampSize("Clamp Size", Float) = 0
		_ClampSizeMin("Clamp Size Min", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend One One
			ZWrite Off
			ZTest LEqual
			Cull Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile_local __ _POWER_RGB // RGB Power
			#pragma multi_compile_local __ _CLAMP_SIZE // Clamp Size
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4    _Color;
			float     _ClampSizeMin;

			struct a2v
			{
				float4 vertex    : POSITION;
				float4 color     : COLOR;
				float2 texcoord0 : TEXCOORD0;

				#if _CLAMP_SIZE
					float3 texcoord1 : TEXCOORD1;
				#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float4 color     : COLOR;
				float2 texcoord0 : TEXCOORD0;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out v2f o)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				#if _CLAMP_SIZE
					float3 center    = i.texcoord1;
					float3 direction = i.vertex.xyz - center.xyz;
					float  size      = length(direction);

					// Normalize
					direction /= size;

					float sizeMin = _ClampSizeMin / _ScreenParams.y;
					float scale   = saturate(size / sizeMin);
					size /= scale; // Scale up to min size
					i.color.a *= scale; // Darken by shrunk amount
					i.vertex.xyz = center.xyz + direction * size;
				#endif

				o.vertex    = UnityObjectToClipPos(i.vertex);
				o.color     = i.color * _Color;
				o.texcoord0 = i.texcoord0;
			}

			void Frag(v2f i, out f2g o)
			{
				o.color = tex2D(_MainTex, i.texcoord0);

				#if _POWER_RGB
					o.color.rgb = pow(o.color.rgb, float3(1.0f, 1.0f, 1.0f) + (1.0f - i.color.rgb) * 10.0f);
				#else
					o.color *= i.color;
				#endif

				o.color.a = saturate(o.color.a);
				o.color *= i.color.a;
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader