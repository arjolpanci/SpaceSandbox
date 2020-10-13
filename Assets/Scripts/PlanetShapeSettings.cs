using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetShapeSettings : ScriptableObject
{

    public enum SphereMeshType {Icosphere, CubicSphere}

    [Header("Size&Detail")]
    [Range(5, 1000)]
    public int resolution;
    public SphereMeshType meshType;
    public float terrainRadius, waterRadius;

    [Header("Characteristics")]
    public bool hasWater, hasAtmosphere, hasCraters;
    public int nrOfCraters;

    [Header("Noise Parameters")]
    public NoiseSource[] noiseLayers;
    public Vector3 offset;

    [Header("Color Parameters")]
    public Vector3 atmosphereWaveLengths;
    public Color shallowColor, deepColor;
    public Gradient shoreGradient, mainTerrainGradien, peaksGradient;
    public Material terrainMaterial, waterMaterial, atmosphereMaterial;
    [Range(0,1)]
    public float colorChangeHeight;

    float minTerrainValue = 1000000000;
    float maxTerrainValue = 0;
    Vector4[] waterLevelData;

    public void GenerateCraters(Mesh mesh, int nrOfCraters){
        Vector3[] vertices = mesh.vertices;
        Vector3[] workVertices = mesh.vertices;
        
        while(nrOfCraters > 0){
            float craterRadius = Random.Range(0.05F, 0.15F);
            float craterDepth = Random.Range(0.9F, 0.98F);
            int craterBase = Random.Range(0, vertices.Length-1);
            for(int i=0; i<vertices.Length; i++){
                float distance = Vector3.Distance(workVertices[craterBase].normalized, workVertices[i].normalized);
                if(distance <= craterRadius){
                    vertices[i] *= GetCraterVertexDepth(distance, craterDepth, craterRadius);
                }
            }
            nrOfCraters --;
        }
        
        mesh.vertices = vertices;
    }

    public float GetCraterVertexDepth(float distance, float craterDepth, float craterRadius){
        float input = Mathf.InverseLerp(0, craterRadius, distance);
        float value = Mathf.Min(1.5F * Mathf.Pow(input, 3) * Mathf.Sin(input), Mathf.Log(7*input)/2);
        if(value >= 1.2F || value <= 0) value = 0;
        float result = value * (1.0F-craterDepth) + craterDepth;
        return result;
    }

    public void AddMeshDetail(Mesh mesh, Vector3 origin, float radius, bool hasElevation){
        Vector3[] vertices = mesh.vertices;

        for(int i=0; i<vertices.Length; i++){
            float elevation = 0;
            if(hasElevation){
                for(int j=0; j<noiseLayers.Length; j++){
                    float currentLevelNoise = noiseLayers[j].GetNoise(vertices[i], origin, offset);
                    if(noiseLayers[j].noiseSettings.useFirstLayerAsMask && j != 0){
                        float firstLayerNoise = noiseLayers[0].GetNoise(vertices[i], origin, offset);
                        currentLevelNoise *= firstLayerNoise;
                    }
                    elevation += currentLevelNoise;
                }
            }
            
            vertices[i] *= radius * (elevation + 1);
        }

        mesh.vertices = vertices;
    }

    public void UpdateTerrainMinMax(Mesh mesh){
        Vector3[] vertices = mesh.vertices;
        waterLevelData = new Vector4[vertices.Length];

        minTerrainValue = 1000000000;
        maxTerrainValue = 0;

        for(int i=0; i<vertices.Length; i++){
            float pointHeight = Vector3.Distance(vertices[i], Vector3.zero);
            if(pointHeight > maxTerrainValue) maxTerrainValue=pointHeight;
            if(pointHeight < minTerrainValue) minTerrainValue=pointHeight;

            float waterHeight = pointHeight;
            if(waterHeight > terrainRadius) waterHeight = terrainRadius;
            waterLevelData[i] = new Vector4(waterHeight, 0, 0, 0);
        }
    }

    public Mesh CreateTerrain(Vector3 origin){
        Mesh mesh = new Mesh();
        mesh.Clear();

        if(meshType == SphereMeshType.Icosphere){
            mesh = IcoSphere.GetIcosphereMesh(resolution);
        }else{
            mesh = CubicSphere.GetSphereMesh(resolution);
        }

        if(hasCraters) GenerateCraters(mesh, nrOfCraters);

        AddMeshDetail(mesh, origin, terrainRadius, true);
        UpdateTerrainMinMax(mesh);
        
        mesh.RecalculateNormals();
        mesh.colors = GetColors(mesh.vertices, mesh.normals);  
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

        return mesh;
    }

    public Mesh CreateAtmosphere(Vector3 origin){
        Mesh mesh = new Mesh();
        mesh.Clear();

        mesh = CubicSphere.GetSphereMesh(resolution);
        //AddMeshDetail(mesh, origin, terrainRadius * 1.15F, false);
        AddMeshDetail(mesh, origin, maxTerrainValue * 1.01F, false);
        
        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

        return mesh;
    }

    public Mesh CreateWater(Vector3 origin){
        Mesh mesh = new Mesh();
        mesh.Clear();

        mesh = IcoSphere.GetIcosphereMesh(resolution);
        AddMeshDetail(mesh, origin, waterRadius, false);

        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        mesh.tangents = waterLevelData;
        
        return mesh;
    }

    public Color[] GetColors(Vector3[] vertices, Vector3[] normals){
        Color[] colors = new Color[vertices.Length];
        
        for(int i=0; i<vertices.Length; i++){
            float distance = Vector3.Distance(vertices[i], Vector3.zero);
            distance = Mathf.Clamp(distance, minTerrainValue, maxTerrainValue);
            
            if(distance > terrainRadius && distance < (colorChangeHeight*(maxTerrainValue-terrainRadius) + terrainRadius)){
                float height = Mathf.InverseLerp(terrainRadius, (float)colorChangeHeight*(maxTerrainValue-terrainRadius) + terrainRadius, distance);
                colors[i] = mainTerrainGradien.Evaluate(height);
            }else if(distance <= terrainRadius){
                float height = Mathf.InverseLerp(minTerrainValue, terrainRadius, distance);
                colors[i] = shoreGradient.Evaluate(height); 
            }else if(distance >= (colorChangeHeight*(maxTerrainValue-terrainRadius) + terrainRadius) && distance <= maxTerrainValue) {
                float height = Mathf.InverseLerp((float)colorChangeHeight*(maxTerrainValue-terrainRadius) + terrainRadius, maxTerrainValue, distance);
                colors[i] = peaksGradient.Evaluate(height);
            }
        }

        return colors;
    }

    public void UpdateWaterProperites(Renderer renderer, Vector3 starPos){
        ShaderHelper.UpdateWaterProperties(renderer, minTerrainValue, terrainRadius, 
        shallowColor, deepColor, starPos);
    }

    public void UpdateAtmosphereProperties(Renderer renderer, Vector3 starPos, Vector3 planetPos){
        ShaderHelper.UpdateAtmosphereProperties(renderer, planetPos, atmosphereWaveLengths, 
        starPos, terrainRadius, maxTerrainValue * 1.01F);
    }

}


