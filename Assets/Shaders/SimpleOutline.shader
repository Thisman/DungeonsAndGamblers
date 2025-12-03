Shader "Custom/SimpleOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Opaque" }

        // Рисуем только задние грани, чтобы получить "ободок"
        Cull Front
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float  _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // смещаем вершины вдоль нормали
                float3 norm = normalize(v.normal);
                float3 offset = norm * _OutlineWidth;

                float4 pos = v.vertex + float4(offset, 0);
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }

    FallBack Off
}
