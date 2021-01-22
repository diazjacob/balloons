Shader "StencilBuffer/write" {
	//show values to edit in inspector
	Properties{
		_Amplitude ("Amplitude", Range(0,10)) = 1
		_Frequency("Frequency", Range(0,10)) = 1
		_Speed("Speed", Range(0,10)) = 1

		[IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0
	}

		SubShader{
			//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
			Tags{ "RenderType" = "Opaque" "Queue" = "Geometry-1"}

			//stencil operation
			Stencil{
				Ref[_StencilRef]
				Comp Always
				Pass Replace
			}

			Pass{

			//don't draw color or depth
			Blend Zero One
			ZWrite Off

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float _Amplitude;
			float _Frequency;
			float _Speed;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 position : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;

				float4 p = v.vertex;
				p.x += sin((_Frequency * v.vertex.x) + ( _Time[1] * _Speed)) * _Amplitude;
				p.y += sin((_Frequency * v.vertex.y * 1.4832) + (_Time[1] * _Speed)) * _Amplitude;
				p.z += sin((_Frequency * v.vertex.z * .332) + (_Time[1] * _Speed)) * _Amplitude;
				v.vertex = p;

				o.position = UnityObjectToClipPos(v.vertex);

				

				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
				return 0;
			}

			ENDCG
		}
	}
}
