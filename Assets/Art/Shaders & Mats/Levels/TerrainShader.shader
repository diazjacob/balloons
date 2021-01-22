Shader "Balloon/TerrainShader"
{
    Properties
    {
        _MainTex("MainTexture", 2D) = "Red" {}
        _MainColor ("Main Color", Color) = (0,0,0,0)

        [IntRange] _StencilRef("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Transparent"}

        Stencil{
            Ref[_StencilRef]
            Comp Equal
        }

       Lighting On
       ZWrite Off
       Cull Back
       Blend SrcAlpha OneMinusSrcAlpha

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed3 worldPos : TEXCOORD1;
            };

            float4 _MainColor;
            float _Height;

            sampler2D _MainTex;

            //Texture2D _MainTex;
            //SamplerState my_point_clamp_sampler;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //fixed4 col = _MainTex.Sample(my_point_clamp_sampler, i.uv);// +MainColor;
                fixed4 col = _MainColor;

                fixed f = tex2D(_MainTex, i.uv);
                col.a = clamp(1 - ceil(clamp(_Height - f,0,1)), 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
