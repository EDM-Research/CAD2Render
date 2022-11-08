Shader "Unlit/Depth"
{
    Properties{
        _DepthMaxDistance("max distance in unity units", Float) = 100
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // include file that contains UnityObjectToWorldNormal helper function
            #include "UnityCG.cginc"


            float _DepthMaxDistance;

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 screenPos : TEXCOORD1;
            };

            struct appdata {
                float4 vertex : POSITION;
            };

            // vertex shader: takes object space normal as input too
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex).xyz;
                COMPUTE_EYEDEPTH(o.screenPos.z);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = float4(0,0,0,1);
                c.rgb = i.screenPos.z / _DepthMaxDistance;
                return c;
            }
        ENDCG
        }
    }
}