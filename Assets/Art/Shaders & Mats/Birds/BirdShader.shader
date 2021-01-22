Shader "Balloon/BirdMat"
{
    Properties
    {
        _Color("Main Color", Color) = (0,0,0,0)
        _WaveFrequency("Wing Flap Frequency", float) = 0
        _WaveAmplitude("Wing Flap Amplitude", float) = 0
        _Offset("Wing Offset", float) = 0

       [IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque"}
        LOD 100
        Cull Off

        Stencil{
            Ref[_StencilRef]
            Comp Equal
        }

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed3 worldPos : TEXCOORD1;
            };

            float4 _Color;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _WaveSpeed;
            float _Offset;

            v2f vert(appdata v)
            {
                v2f o;

                v.vertex.y += cos((_Time.y + _Offset) * _WaveFrequency) * _WaveAmplitude * abs(v.vertex.x);
                v.vertex.x += sin((_Time.y + _Offset) * _WaveFrequency) * _WaveAmplitude * v.vertex.x;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = (abs(i.worldPos.x));
                float y = (abs(i.worldPos.z));
                
                clip(5-x);
                clip(5-y);

                fixed4 col = _Color;
                col.a = 1;

                return col;
            }
            ENDCG
        }
    }
}
