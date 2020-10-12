Shader "Unlit/Atmosphere"
{
    Properties
    {
        lightSamplePoints ("Light Sample Points", int) = 5
        viewSamplePoints ("View Sample Points", int) = 5
        planetCenter ("Planet Center", vector) = (0,0,0)
        waveLengths ("Wave Lengths", vector) = (440,500,770)
        lightPos ("Light Position", vector) = (6000,0,0)
        scatteringCoefficient ("Scattering Coefficient", float) = 2
        scaleHeight ("Scale Height", float) = 0.5
        rayLeightHeight ("RayLeigh Height", float) = 0.5
        planetRadius ("Planet Radius", float) = 1
        intensity ("Intensity", float) = 1
        atmosphereRadius ("Atmosphere Radius", float) = 2
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 200

        ZWrite Off
        Cull Off
        //ZTest Always
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD01;
                float3 center : TEXCOORD2;
                float3 normal :TEXCOORD3;
                float3 viewDir : TEXCOORD4;
            };

            int viewSamplePoints;
            int lightSamplePoints;
            float3 planetCenter;
            float3 lightPos;
            float3 waveLengths;
            float planetRadius;
            float atmosphereRadius;
            float scatteringCoefficient;
            float scaleHeight;
            float rayLeightHeight;
            float intensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.center = mul (unity_ObjectToWorld, half4(0,0,0,1));
                o.viewDir = normalize(-WorldSpaceViewDir(v.vertex));
                o.normal = v.normal;
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

            float CalculateRayDepth(float3 atmospherePoint){
                float lightOpticalDepth = 0;
                
                float3 dirToLight = normalize(lightPos - atmospherePoint);
                float2 hitInfo = rayIntersect(atmospherePoint, dirToLight, planetCenter, atmosphereRadius);
                float distanceThrough = hitInfo.y;
                if(distanceThrough < 0) return -1;
                
                float step = distanceThrough / (lightSamplePoints-1);
                float3 currentPoint = atmospherePoint;

                for(int i=0; i<lightSamplePoints; i++){
                    float pointHeight = distance(planetCenter, currentPoint) - planetRadius;
                    if(pointHeight < 0) return lightOpticalDepth;
                    //pointHeight = smoothstep(planetRadius, atmosphereRadius, pointHeight);
                    float currentLightOpticalDepth = exp(-pointHeight / rayLeightHeight) * step;
                    lightOpticalDepth += currentLightOpticalDepth;
                    currentPoint += (dirToLight * step);
                }
                return lightOpticalDepth;
            }
            
            float3 CalculateLight(float segmentLength, float3 rayOrigin, float3 rayDir, int viewSamples){
                
                float3 finalLightValue = 0;
                float totalOpticalDepth = 0;

                float step = segmentLength / (viewSamples-1);
                float3 currentPoint = rayOrigin;
                
                float3 colors = 0;
                colors.r = 400 / pow(waveLengths.x, 4) * scatteringCoefficient;
                colors.g = 400 / pow(waveLengths.y, 4) * scatteringCoefficient;
                colors.b = 400 / pow(waveLengths.z, 4) * scatteringCoefficient;

                for(int i=0; i<viewSamples; i++){
                    float height = distance(planetCenter, currentPoint) - planetRadius;
                    float pointDepth = exp(-height / scaleHeight) * step;
                    
                    float lightOpticalDepth = CalculateRayDepth(currentPoint);
                    if(lightOpticalDepth == -1) continue;
                    totalOpticalDepth += pointDepth;

                    float3 transmittance = exp(-(lightOpticalDepth + totalOpticalDepth) * colors);
                    finalLightValue += (transmittance * pointDepth * step); 
                    currentPoint += (rayDir * step);
                }
                
                finalLightValue *= colors * intensity * step / planetRadius;

                return finalLightValue;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float3 pointPos = i.worldPos;
                float3 dir = normalize(pointPos - lightPos);
                //float3 dir = i.viewDir;
                //if(distance(planetCenter, camPos) <= atmosphereRadius) 
                
                const float epsilon = 0.0001;
                float2 hitInfo = rayIntersect(pointPos + epsilon, dir, planetCenter, atmosphereRadius);
                float distanceTo = hitInfo.x;
                float distanceThrough = hitInfo.y - hitInfo.x;

                if(distanceThrough < 0) return float4(0,0,0,0);
                      
                float3 light = CalculateLight(distanceThrough - epsilon, pointPos, dir, viewSamplePoints);
                
                return float4(light , 1);

            }
            ENDCG
        }

    }
}