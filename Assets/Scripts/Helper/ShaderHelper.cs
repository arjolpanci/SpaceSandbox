using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderHelper
{
    public enum PropertyType {vector, floatNum, intNum, color}

    public static void UpdateMaterialProperty(Renderer renderer, string propertyName, PropertyType propertyType, MaterialInput input){
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        if(propertyType == PropertyType.vector){
            propertyBlock.SetVector(propertyName, input.vectorData);
        }else if(propertyType == PropertyType.floatNum){
            propertyBlock.SetFloat(propertyName, input.floatData);
        }else if(propertyType == PropertyType.intNum){
        
        }else if(propertyType == PropertyType.color){
            propertyBlock.SetColor(propertyName, input.vectorData);
        }
        renderer.SetPropertyBlock(propertyBlock);
        return;
    }

    public static void UpdateWaterProperties(Renderer renderer, float minTerrainValue, float terrainRadius, Color shallowColor, Color deepColor, Vector3 lightPos){
        List<MaterialInput> waterParamters = new List<MaterialInput>();
        waterParamters.Add(new MaterialInput(minTerrainValue, "minTerrainValue", PropertyType.floatNum));
        waterParamters.Add(new MaterialInput(terrainRadius, "terrainRadius", PropertyType.floatNum));
        waterParamters.Add(new MaterialInput(shallowColor, "shoreColor", PropertyType.color));
        waterParamters.Add(new MaterialInput(deepColor, "deepColor", PropertyType.color));
        waterParamters.Add(new MaterialInput(-1.15F, "waterOffset", PropertyType.floatNum));
        waterParamters.Add(new MaterialInput(Random.Range(0.15F, 0.35F), "spotSize", PropertyType.floatNum));
        waterParamters.Add(new MaterialInput(Random.Range(2.7F, 4.7F), "glossAmplifier", PropertyType.floatNum));
        waterParamters.Add(new MaterialInput(lightPos, "lightPos", PropertyType.vector));
        waterParamters.Add(new MaterialInput(Random.Range(8, 12), "waveSize", PropertyType.floatNum));
        
        foreach(MaterialInput input in waterParamters){
            UpdateMaterialProperty(renderer, input.paramterName, input.type, input);
        }
    }

    public static void UpdateAtmosphereProperties(Renderer renderer, Vector3 planetCenter, Vector3 waveLengths, Vector3 lightPos, float planetRadius, float atmosphereRadius){
        List<MaterialInput> atmosphereParamters = new List<MaterialInput>();
        atmosphereParamters.Add(new MaterialInput(10, "lightSamplePoints", PropertyType.intNum));
        atmosphereParamters.Add(new MaterialInput(10, "viewSamplePoints", PropertyType.intNum));
        atmosphereParamters.Add(new MaterialInput(planetCenter, "planetCenter", PropertyType.vector));
        atmosphereParamters.Add(new MaterialInput(waveLengths, "waveLengths", PropertyType.vector));
        atmosphereParamters.Add(new MaterialInput(lightPos, "lightPos", PropertyType.vector));
        atmosphereParamters.Add(new MaterialInput(Random.Range(2.5F,3.2F), "scatteringCoefficient", PropertyType.floatNum));
        atmosphereParamters.Add(new MaterialInput(Random.Range(1.4F,2F), "scaleHeight", PropertyType.floatNum));
        atmosphereParamters.Add(new MaterialInput(Random.Range(0.8F,0.95F), "rayLeightHeight", PropertyType.floatNum));
        atmosphereParamters.Add(new MaterialInput(planetRadius, "planetRadius", PropertyType.floatNum));
        atmosphereParamters.Add(new MaterialInput(Random.Range(0.4F, 0.6F), "intensity", PropertyType.floatNum));
        atmosphereParamters.Add(new MaterialInput(atmosphereRadius, "atmosphereRadius", PropertyType.floatNum));

        foreach(MaterialInput input in atmosphereParamters){
            UpdateMaterialProperty(renderer, input.paramterName, input.type, input);
        }
    }

    public class MaterialInput{

        public Vector4 vectorData;
        public float floatData;
        public int intData;
        public string paramterName;
        public PropertyType type;

        public MaterialInput(Vector4 vectorData, string paramterName, PropertyType type){
            this.vectorData = vectorData;
            this.paramterName = paramterName;
            this.type = type;
        }

        public MaterialInput(float floatData, string paramterName, PropertyType type){
            this.floatData = floatData;
            this.paramterName = paramterName;
            this.type = type;
        }

        public MaterialInput(int intData, string paramterName, PropertyType type){
            this.intData = intData;
            this.paramterName = paramterName;
            this.type = type;
        }
    }
}
