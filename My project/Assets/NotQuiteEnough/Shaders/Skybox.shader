Shader "Unlit/Skybox"
{
    Properties
    {
        [HDR] _SunColor ("Sun Color", Color) = (1, 1, 0, 1)
        _SunSize ("Sun Size", Range(0, 1)) = 0.1
        _SunSharpness ("Sun Sharpness", Range(1, 1000)) = 20
        _TopColor ("Sky Top Color", Color) = (0.4, 0.6, 1, 1)
        _BottomColor ("Sky Bottom Color", Color) = (0.1, 0.3, 0.6, 1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _SunColor;
            float _SunSize;
            float _SunSharpness;
            float4 _TopColor;
            float4 _BottomColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize the world position to get the view direction
                float3 viewDir = normalize(i.worldPos);
                
                // Get the dot product between view direction and light direction
                float sunDot = dot(viewDir, _WorldSpaceLightPos0.xyz);
                
                // Create sun disk using smoothstep
                float sun = smoothstep(1 - _SunSize, (1 - _SunSize) + (1/_SunSharpness), sunDot);
                
                // Create gradient based on world Y position
                float gradientT = viewDir.y * 0.5 + 0.5;
                float4 skyColor = lerp(_BottomColor, _TopColor, pow(gradientT,1));
                
                // Combine sun and sky colors
                return lerp(skyColor, _SunColor, sun);
            }
            ENDCG
        }
    }
}
