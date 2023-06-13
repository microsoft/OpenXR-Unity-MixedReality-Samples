Shader "Custom/GridShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", Any) = "" {}
        _Scale ("Scale", Float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            fixed4 _Color;
            half _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Convert the position and normal to world space for proper tiling
                v.vertex = mul(unity_ObjectToWorld, v.vertex);
                v.normal = normalize(mul(unity_ObjectToWorld, v.normal));

                // Choose the axis which is most aligned with the surface normal, then use world space coordinates from
                // the other two axes to drive the uvs. As a result, the texture is tiled relative to the world space.
                // By default, the most-aligned normal is assumed to be the z-axis. A value > 1/sqrt(2) = 0.707 in the
                // x or y value of the normal signifies a rotation > 45 degrees towards these other axes, and different
                // axes should be used to calculate the uvs (yz and xz, respectively).
                o.uv = v.vertex.xy;
                if( abs(v.normal.x) > 0.707 ) o.uv = v.vertex.yz;
                if( abs(v.normal.y) > 0.707 ) o.uv = v.vertex.xz;

                o.uv.x *= _Scale;
                o.uv.y *= _Scale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
