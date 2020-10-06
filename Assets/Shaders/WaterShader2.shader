Shader "Unlit/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        minTerrainValue ("Minimum Terrain Height", float) = 0
        terrainRadius ("Terrain Radius", float) = 0
        waterOffset ("Water Line Offset", float) = 0
        spotSize ("Reflection Spot Size", float) = 0
        glossAmplifier ("Glossness", float) = 0
        shoreColor ("Water Shore Color", Color) = (1,1,1,1)
        deepColor ("Water Deep Color", Color) = (1,1,1,1)
        lightPos ("Light Location", vector) = (0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent-1"}
        LOD 200

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 tangent : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float minTerrainValue;
            float terrainRadius;
            float waterOffset;
            float spotSize;
            float glossAmplifier;
            float4 shoreColor;
            float4 deepColor;
            float3 lightPos;

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float2 rayIntersect(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius) {
                float3 offsetFromOrigin = rayOrigin - sphereCenter;
                float a = dot(rayDir, rayDir);
                float b = 2 * (dot(rayDir, offsetFromOrigin));
                float c = dot(offsetFromOrigin, offsetFromOrigin) - (sphereRadius * sphereRadius);
                
                float discriminant = (b*b) - (4*a*c);
                float sd = sqrt(discriminant);

                if(discriminant >= 0){
                    float distanceFromOrigin = max(0, (-b-sd) / (2*a));
                    //distanceFromOrigin = (-b-sd) / (2*a);
                    float distanceFromOrginFar = (-b+sd) / (2*a);

                    return float2(distanceFromOrigin, distanceFromOrginFar);
                }

                return float2(0,-1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 resolution = float2(1024, 1024);
                float4 noiseVal = tex2D(_MainTex, (i.uv + (_SinTime/1000)) * 20);
                //noiseVal = step(0.6, noiseVal);
                noiseVal = smoothstep(0.6, 0.9, noiseVal);

                float3 camPos = _WorldSpaceCameraPos;

                //Direct Diffuse Light
                float3 pointPos = i.worldPos;
                float3 lightDir = normalize(lightPos - pointPos);
                float3 normal = normalize(i.normal);
                float diffuse = dot(lightDir, normal);

                //Direct Specular Light
                float3 pathToCam = (camPos - pointPos);
                float3 dirToCam = normalize(pathToCam);
                float3 viewReflect = reflect(-dirToCam, normal) + spotSize;

                float specularFallof = max(0, dot(viewReflect, lightDir));
                specularFallof = pow(specularFallof, glossAmplifier) * noiseVal;
                //float3 directSpecular = specularFallof * lightColor;

                //Calculating Color Value
                float deepness = i.tangent.x + waterOffset;
                float height = smoothstep(minTerrainValue, terrainRadius, deepness);
                float4 pointColor = lerp(shoreColor, deepColor, 1-height);

                //Applying final transformations
                float3 finalColor = pointColor.xyz * diffuse + specularFallof;
                float alpha = min(0.9, exp(-height * 0.3));
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
