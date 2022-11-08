Shader "Unlit/FalseColor"
{
    Properties
    {
		_FalseColor("False Color", Color) = (0,0,0,1)
        _FalseColorTex ("False Color Texture", 2D) = "black" {}
        _useFalseColorTex("Use texture as false color", Float) = -1
        _objectId("current object being renderd", Float) = -1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenPos : TEXCOORD2;
            };


            sampler2D _FalseColorTex;
            float4 _FalseColorTex_ST;
            int _useFalseColorTex;
            float4 _FalseColor;
            int _objectId;
            int _currentObjectId;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                if (_objectId == _currentObjectId && _currentObjectId >= 0)
                    o.vertex.z = 1;

                o.uv = TRANSFORM_TEX(v.uv, _FalseColorTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 chosenColor;
                if (_useFalseColorTex > 0) {
                    // sample the texture
                    fixed4 col = tex2D(_FalseColorTex, i.uv);
                    chosenColor = col;
                }
                else {
                    chosenColor = _FalseColor.rgba;
                }
                if (_currentObjectId < 0)
                    return chosenColor;

                if (_objectId == _currentObjectId) {
                    return fixed4(1.0f, 1.0f, 1.0f, 1.0f);
                }
                else
                    return fixed4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            ENDCG
        }
    }
}
