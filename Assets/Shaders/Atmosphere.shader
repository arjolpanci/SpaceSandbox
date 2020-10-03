Shader "Unlit/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        lightSamplePoints ("Light Sample Points", int) = 5
        viewSamplePoints ("View Sample Points", int) = 5
        atmosphereCenter ("Atmosphere Center", vector) = (0,0,0)
        planetCenter ("Planet Center", vector) = (0,0,0)
        waveLengths ("Wave Lengths", vector) = (440,500,770)
        lightPos ("Light Position", vector) = (6000,0,0)
        densityFallof ("Density Fallof", float) = 0.5
        scatteringCoefficient ("Scattering Coefficient", float) = 2
        scaleHeight ("Scale Height", float) = 0.5
        rayLeightHeight ("RayLeigh Height", float) = 0.5
        planetRadius ("Planet Radius", float) = 1
        intensity ("Intensity", float) = 1
        atmosphereRadius ("Atmosphere Radius", float) = 2
        
        //_SphereCenter ("Sphere Center", vector) = (0,0,0)
        //_SphereRadius ("Sphere Radius", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 200

        //ZWrite Off
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
                float3 viewVector : TEXCOORD3;
                float3 normal :TEXCOORD4;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _MainTex_ST;
            int viewSamplePoints;
            int lightSamplePoints;
            float3 atmosphereCenter;
            float3 planetCenter;
            float3 lightPos;
            float3 waveLengths;
            float planetRadius;
            float atmosphereRadius;
            float densityFallof;
            float scatteringCoefficient;
            float scaleHeight;
            float rayLeightHeight;
            float intensity;
            //float3 _SphereCenter;
            //float _SphereRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.center = mul (unity_ObjectToWorld, half4(0,0,0,1));
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
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
                    float distanceFromOrginFar = (-b+sd) / (2*a);

                    return float2(distanceFromOrigin, distanceFromOrginFar);
                }

                return float2(0,-1);
            }

            float DensityAtPoint(float3 pointPos){
                float heighAboveSurface = length(pointPos - planetCenter) - planetRadius;
                float height01 = heighAboveSurface / (atmosphereRadius - planetRadius);
                float localDensity = exp(-height01 * densityFallof) * (1-height01);
                return localDensity;
            }

            float CalculateRayDepth(float3 atmospherePoint){
                float lightOpticalDepth = 0;
                //float3 dirToLight = normalize(_WorldSpaceLightPos0);
                float3 dirToLight = normalize(lightPos - planetCenter);
                float2 hitInfo = rayIntersect(atmospherePoint, dirToLight, planetRadius, atmosphereRadius);
                float distanceThrough = hitInfo.y - hitInfo.x;
                if(distanceThrough < 0) return -1;
                
                float step = distanceThrough / (lightSamplePoints-1);
                float3 currentPoint = atmospherePoint;
                for(int i=0; i<lightSamplePoints; i++){
                    float pointHeight = distance(currentPoint, planetCenter) - planetRadius;
                    //float height01 = pointHeight / (atmosphereRadius - planetRadius);
                    if(pointHeight <= 0) return -1;
                    //float currentLightOpticalDepth = exp(-pointHeight / rayLeightHeight) * step;
                    float currentLightOpticalDepth = DensityAtPoint(currentPoint);
                    lightOpticalDepth += currentLightOpticalDepth;
                    currentPoint += (dirToLight * step);
                }
                return lightOpticalDepth;
            }
            
            float CalculateOpticalDepth(float3 rayOrigin, float3 rayDir, float rayLength){
                float step = rayLength / (lightSamplePoints - 1);
                float opticalDepth = 0;
                float currentPoint = rayOrigin;
                for( int i=0; i<lightSamplePoints; i++){
                    float localDensity = DensityAtPoint(currentPoint);
                    opticalDepth += localDensity * step;
                    currentPoint += rayDir * step;
                }
                return opticalDepth;
            }

            float3 CalculateLight(float segmentLength, float3 rayOrigin, float3 rayDir, int viewSamples){
                float3 finalLightValue = 0;
                float totalOpticalDepth = 0;
                float step = segmentLength / (viewSamples-1);
                float3 currentPoint = rayOrigin;
                float3 colors = float3(0,0,0);
                colors.x = pow(400 / waveLengths.x, 4) * scatteringCoefficient;
                colors.y = pow(400 / waveLengths.y, 4) * scatteringCoefficient;
                colors.z = pow(400 / waveLengths.z, 4) * scatteringCoefficient;

                float3 lightDir = normalize(lightPos - planetCenter);

                for(int i=0; i<viewSamples; i++){
                    /*float2 lightRayHitInfo = rayIntersect(currentPoint, lightDir, planetCenter, atmosphereRadius);
                    float sunRayLength = (lightRayHitInfo.y - lightRayHitInfo.x);
                    if(sunRayLength < 0) continue;
                    float rayOpticalDepth = CalculateOpticalDepth(currentPoint, lightDir, sunRayLength);
                    float viewRayDepth = CalculateOpticalDepth(currentPoint, -rayDir, step * i);
                    float pointDensity = DensityAtPoint(currentPoint);
                    float transmittance = exp(-rayOpticalDepth);

                    finalLightValue += pointDensity *transmittance * step;
                    currentPoint += (rayDir * step);*/

                    float height = distance(atmosphereRadius, currentPoint) - atmosphereRadius;
                    float opticalDepthSegment = exp(-height / scaleHeight) * step;
                    float lightOpticalDepth = CalculateRayDepth(currentPoint);
                    if(lightOpticalDepth == -1) continue;
                    totalOpticalDepth += opticalDepthSegment;
                    float3 transmittance = exp(-(lightOpticalDepth + totalOpticalDepth));
                    finalLightValue += transmittance * opticalDepthSegment * step; 
                    currentPoint += (rayDir * step);
                }
                
                //finalLightValue *= intensity * step / planetRadius;

                return finalLightValue;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 camPos = _WorldSpaceCameraPos;
                float3 pointPos = i.worldPos;
                float3 dirToPoint = normalize(pointPos - camPos);
                //float3 dirToPoint = normalize(i.viewVector);

                if(length(camPos - atmosphereCenter) < atmosphereRadius){
                    //camPos += dirToPoint * length(camPos - atmosphereCenter);
                    //dirToPoint *= -1;
                    pointPos = camPos;
                }
                //float3 dirToPoint = normalize(i.viewVector);
                
                float2 hitInfo = rayIntersect(pointPos, dirToPoint, atmosphereCenter, atmosphereRadius);
                float distanceTo = hitInfo.x;
                float distanceThrough = hitInfo.y - hitInfo.x;
                //distanceThrough = min(distanceThrough, sceneDepth - distanceTo);
                if(distanceThrough < 0) return float4(0,0,0,0);

                //float pointInAtmosphere = (camPos + dirToPoint * distanceTo);
                const float epsilon = 0.00001;
                
                float3 light = CalculateLight(distanceThrough - epsilon, pointPos + epsilon, dirToPoint, viewSamplePoints);
                return float4(light , 1);

                //if(distanceThrough < 0) return float4(0,0,0,0);
                //float4 col = float4(distanceThrough.xxx/4,1);
                //return col;

            }
            ENDCG
        }

    }
}