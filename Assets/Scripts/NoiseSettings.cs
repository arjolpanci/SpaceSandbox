using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum NoiseType {Simple, Ripple};

    [Header("Noise Parameters")]
    public bool enabled = true;
    public int noiseLevels=6;
    public float magnitude=0.2F, roughness=0.2F, minValue=0F, dampValue=0.5F, frequency=1.1F;
    public NoiseType noiseType;
    public bool useFirstLayerAsMask=false;
}
