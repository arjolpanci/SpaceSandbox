Shader "Unlit/Test"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1) 
        _Glossines ("Glossines", float) = 1
        _Offset ("Offset", float) = 0
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader{
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            
            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float4 _Color;
            float _Glossines;
            float _Offset;

            v2f vert (appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                float3 nor = normalize(i.normal);

                //Lighting

                //Ditect Diffuse Light
                float3 lightDir = normalize(_WorldSpaceLightPos0);
                float3 lightColor = _LightColor0.rgb;
                float diffuse = saturate(dot(lightDir, nor));
                
                //Ambient Light
                float3 ambientLight = float3(0.1, 0.1, 0.1);

                //Direct Specular Light
                float3 camPos = _WorldSpaceCameraPos;
                float3 fragPos = i.worldPos;
                float3 pathToCam = (camPos - fragPos);
                float3 dirToCam = normalize(pathToCam);
                float3 viewReflect = reflect(-dirToCam, nor) + _Offset;

                float specularFallof = max(0, dot(viewReflect, lightDir));
                specularFallof = pow(specularFallof, _Glossines);
                float3 directSpecular = specularFallof * lightColor;

                //return float4(directSpecular, 1);

                //Composite 
                float3 color = diffuse * lightColor;
                float3 diffuseLight = (color + ambientLight) * _Color;
                float3 finalObjectLight = diffuseLight + specularFallof;

                return float4(finalObjectLight, 1);
            }
            ENDCG
        }
    }
}
