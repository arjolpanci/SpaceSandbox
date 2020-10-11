using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderHelper
{
    public enum PropertyType {vector, num, color}

    public static void UpdateMaterialProperty(Renderer renderer, string propertyName, PropertyType propertyType, MaterialInput input){
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        if(propertyType == PropertyType.vector){
            propertyBlock.SetVector(propertyName, input.vectorData);
            //material.SetVector(propertyName, input.vectorData);
        }else if(propertyType == PropertyType.num){
            propertyBlock.SetFloat(propertyName, input.numData);
            //material.SetFloat(propertyName, input.numData);
        }else if(propertyType == PropertyType.color){
            propertyBlock.SetColor(propertyName, input.vectorData);
            //material.SetColor(propertyName, input.vectorData);
        }
        renderer.SetPropertyBlock(propertyBlock);
        return;
    }

    public static void UpdateWaterProperties(Renderer renderer, float minTerrainValue, float terrainRadius, Color shallowColor, Color deepColor, Vector3 lightPos){
        ShaderHelper.UpdateMaterialProperty(renderer, 
        "minTerrainValue", 
        ShaderHelper.PropertyType.num, 
        new ShaderHelper.MaterialInput(minTerrainValue));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "terrainRadius", 
        ShaderHelper.PropertyType.num, 
        new ShaderHelper.MaterialInput(terrainRadius));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "shoreColor", 
        ShaderHelper.PropertyType.color, 
        new ShaderHelper.MaterialInput(shallowColor));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "deepColor", 
        ShaderHelper.PropertyType.color, 
        new ShaderHelper.MaterialInput(deepColor));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "waterOffset", 
        ShaderHelper.PropertyType.num, 
        new ShaderHelper.MaterialInput(-1.15F));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "spotSize", 
        ShaderHelper.PropertyType.num, 
        new ShaderHelper.MaterialInput(Random.Range(0.15F, 0.35F)));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "glossAmplifier", 
        ShaderHelper.PropertyType.num, 
        new ShaderHelper.MaterialInput(Random.Range(2.7F, 4.7F)));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "lightPos", 
        ShaderHelper.PropertyType.vector, 
        new ShaderHelper.MaterialInput(lightPos));

        ShaderHelper.UpdateMaterialProperty(renderer, 
        "waveSize", 
        ShaderHelper.PropertyType.vector, 
        new ShaderHelper.MaterialInput(Random.Range(8, 12)));
    }

    public class MaterialInput{

        public Vector4 vectorData;
        public float numData;

        public MaterialInput(Vector4 vectorData){
            this.vectorData = vectorData;
        }

        public MaterialInput(float numData){
            this.numData = numData;
        }
    }
}
