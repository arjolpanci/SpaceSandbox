Shader "Unlit/StandardUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 100
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
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
                float3 viewVector : TEXCOORD1;
            };

            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

            float2 raySphere(float3 sphereCenter, float3 rayOrigin, float3 rayDir, float sphereRadius){
                float3 distance = rayOrigin - sphereCenter;
                float a = dot(rayDir, rayDir);
                //float b = 2 * dot(rayDir, distance);
                //float c = dot(distance, distance) - sphereRadius*sphereRadius;
                float b = 2 * dot(distance, rayDir);
		        float c = dot (distance, distance) - sphereRadius * sphereRadius;
		        
                float d = b * b - 4 * a * c;

                if(d>0){
                    float s = sqrt(d);
                    float dstNear = max(0, (-b-s) / (2*a));
                    float dstFar = (-b+s) / (2*a);

                    if(dstFar >= 0){
                        return float2(dstNear, dstFar - dstNear);
                    }
                }
                

                /*if(d == 0){
                    float point1 = -b / 2*a;
                    return float2(point1, 0);
                }else if(d>0){
                    float point2 = (-b + sqrt(d)) / 2*a;
                    float point1 = (-b - sqrt(d)) / 2*a;
                    return float2(point1, point2);
                }*/

                return float2(5000000, 0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hitInfo = raySphere(float3(-180, 41, -51), rayOrigin, rayDir, 2);
                float dist = hitInfo.x;
                float distThrough = min(hitInfo.y, sceneDepth - dist);

                return distThrough / (2 * 2);
            }

            

            ENDCG
        }
    }
}
