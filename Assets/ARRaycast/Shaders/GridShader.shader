Shader "Custom/GridShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Scale ("Scale", Float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _Scale;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.worldPos.xy;
            
            if( abs(IN.worldNormal.x) > 0.707 ) uv = IN.worldPos.yz;
            if( abs(IN.worldNormal.y) > 0.707 ) uv = IN.worldPos.xz;

            uv.x *= _Scale;
            uv.y *= _Scale;
            
            // Albedo comes from a texture tinted by color
            fixed4 c = (tex2D (_MainTex, uv) + 0.05) * _Color;
            o.Albedo = c.rgb;

            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
