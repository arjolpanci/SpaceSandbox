using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSource
{
    public NoiseSettings noiseSettings;


    public float GetNoise(Vector3 point, Vector3 origin, Vector3 offset){
        if(!noiseSettings.enabled) return 1F;

        if(noiseSettings.noiseType == NoiseSettings.NoiseType.Simple){
            return SimpleNoise(point, origin, offset);
        }else if(noiseSettings.noiseType == NoiseSettings.NoiseType.Ripple){
            return RipleNoise(point, origin, offset);
        }else{
            return 1F;
        }
        
    }

    private float SimpleNoise(Vector3 point, Vector3 origin, Vector3 offset){
        Vector3 position = point+origin+offset;
        float noise = 0;
        float localScale = noiseSettings.roughness;
        float localMagnitude = 1;

        for(int i=0; i<noiseSettings.noiseLevels; i++){
            float detail = (float)(NoiseS3D.Noise(position.x*localScale, position.y*localScale, position.z*localScale)); 
            detail = (detail+1)*0.5F * localMagnitude;
            noise += detail;
            localScale *= noiseSettings.frequency;
            localMagnitude *= noiseSettings.dampValue;
        }

        noise = Mathf.Max(0, noise-noiseSettings.minValue);
        return noise * noiseSettings.magnitude;
    }

    private float RipleNoise(Vector3 point, Vector3 origin, Vector3 offset){
        Vector3 position = point+origin+offset;
        float noise = 0;
        float localScale = noiseSettings.roughness;
        float localMagnitude = 1;

        for(int i=0; i<noiseSettings.noiseLevels; i++){
            float detail = 1 - Mathf.Abs((float)(NoiseS3D.Noise(position.x*localScale, position.y*localScale, position.z*localScale))); 
            detail *= detail;
            detail = (detail) * localMagnitude;
            noise += detail;
            localScale *= noiseSettings.frequency;
            localMagnitude *= noiseSettings.dampValue;
        }
        noise = Mathf.Max(0, noise-noiseSettings.minValue);
        return noise * noiseSettings.magnitude;
    }

}
