using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlanetShapeSettings : ScriptableObject
{
    [Header("Size&Detail")]
    [Range(5, 1000)]
    public int resolution;
    public float terrainRadius, waterRadius;

    [Header("Characteristics")]
    public bool hasWater, hasAtmosphere, isMoon, hasCraters;

    [Header("Noise Parameters")]
    public NoiseSource[] noiseLayers;
    public Vector3 offset;

    [Header("Color Parameters")]
    public Gradient colorGradient;
    public Material terrainMaterial, waterMaterial, atmosphereMaterial;

    float minTerrainValue = 1000000000;
    float maxTerrainValue = 0;
    Vector2[] waterLevelData;

    public void GenerateCraters(Mesh mesh){
        Vector3[] vertices = mesh.vertices;
        Vector3[] workVertices = mesh.vertices;
        int nrOfCraters = Random.Range(50, 150);

        while(nrOfCraters > 0){
            float craterRadius = Random.Range(0.5F, 1.5F)/terrainRadius;
            float craterDepth = Random.Range(0.9F, 0.98F);
            int craterBase = Random.Range(0, vertices.Length-1);
            for(int i=0; i<vertices.Length; i++){
                float distance = Vector3.Distance(workVertices[craterBase], workVertices[i]);
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
        float value = Mathf.Pow(input, 2);
        float result = value * (0.99F-craterDepth) + craterDepth;
        return result;
    }


    public Mesh CreateTerrain(Vector3 origin){
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        Vector2[] uvMaps;
        //int[] triangles;
        Color[] colors = new Color[(resolution+1) * (resolution+1)];
        mesh.Clear();

        if(isMoon){
            mesh = CubicSphere.GetSphereMesh(resolution);
            GenerateCraters(mesh);
        }else{
            mesh = IcoSphere.GetIcosphereMesh(resolution);
        }

        vertices = mesh.vertices;
        uvMaps = new Vector2[vertices.Length];
        waterLevelData = new Vector2[vertices.Length];

        for(int i=0; i<vertices.Length; i++){
            float elevation = 0;
            Vector3 pointOnSphere = vertices[i];
            for(int j=0; j<noiseLayers.Length; j++){
                float currentLevelNoise = noiseLayers[j].GetNoise(pointOnSphere, origin, offset);
                if(j != 0 && noiseLayers[j].noiseSettings.useFirstLayerAsMask){
                    float firstLayerNoise = noiseLayers[0].GetNoise(pointOnSphere, origin, offset);
                    currentLevelNoise *= firstLayerNoise;
                }
                elevation += currentLevelNoise;
            }
            pointOnSphere *= terrainRadius * (elevation+1);
            vertices[i] = pointOnSphere;

            /*float pointHeight = Vector3.Distance(vertices[i], Vector3.zero);
            if(pointHeight > maxTerrainValue) maxTerrainValue=pointHeight;
            if(pointHeight < minTerrainValue) minTerrainValue=pointHeight;

            float waterHeight = pointHeight;
            if(waterHeight > terrainRadius) waterHeight = terrainRadius;
            waterLevelData[i] = new Vector2(waterHeight, 0);*/ 
        }

        mesh.vertices = vertices;

        minTerrainValue = 1000000000;
        maxTerrainValue = 0;

        for(int i=0; i<vertices.Length; i++){
            float pointHeight = Vector3.Distance(vertices[i], Vector3.zero);
            if(pointHeight > maxTerrainValue) maxTerrainValue=pointHeight;
            if(pointHeight < minTerrainValue) minTerrainValue=pointHeight;

            float waterHeight = pointHeight;
            if(waterHeight > terrainRadius) waterHeight = terrainRadius;
            waterLevelData[i] = new Vector2(waterHeight, 0);
        }

        if(hasCraters) GenerateCraters(mesh);

        if(!isMoon){
            for(int i=0; i<vertices.Length; i++){
                Vector3 unitVector = vertices[i].normalized;
                Vector2 ICOuv = new Vector2(0, 0);
                ICOuv.x = (Mathf.Atan2(unitVector.x, Mathf.Abs(unitVector.z)) + Mathf.PI) / Mathf.PI / 2;
                ICOuv.y = (Mathf.Acos(unitVector.y) + Mathf.PI) / Mathf.PI - 1;
                uvMaps[i] = new Vector2(ICOuv.x, ICOuv.y);
            }
            mesh.uv = uvMaps;
        }
        
        mesh.RecalculateNormals();
        mesh.colors = GetColors(vertices);  
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

        return mesh;
    }

    public Mesh CreateAtmosphere(Vector3 origin){
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        Vector2[] uvMaps;
        mesh.Clear();

        //mesh = IcoSphere.GetIcosphereMesh(resolution/3);
        mesh = CubicSphere.GetSphereMesh(resolution);
        vertices = mesh.vertices;
        uvMaps = new Vector2[vertices.Length];

        for(int i=0; i<vertices.Length; i++){
            /*Vector3 unitVector = vertices[i].normalized;
			Vector2 ICOuv = new Vector2(0, 0);
			ICOuv.x = (Mathf.Atan2(unitVector.x, Mathf.Abs(unitVector.z)) + Mathf.PI) / Mathf.PI / 2;
			ICOuv.y = (Mathf.Acos(unitVector.y) + Mathf.PI) / Mathf.PI - 1;
			uvMaps[i] = new Vector2(ICOuv.x, ICOuv.y);*/
            vertices[i] *= terrainRadius * 1.15F;
        }


        mesh.vertices = vertices;
        //mesh.normals = vertices;
        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        //mesh.uv = uvMaps;

        return mesh;
    }

    public Mesh CreateWater(Vector3 origin){
        Mesh mesh = new Mesh();
        Vector3[] vertices;
        //Vector2[] uvMaps;
        //int[] triangles;
        mesh.Clear();

        mesh = IcoSphere.GetIcosphereMesh(resolution);
        vertices = mesh.vertices;
        //uvMaps = new Vector2[vertices.Length];
        
        for(int i=0; i<vertices.Length; i++){
            //Vector3 unitVector = vertices[i].normalized;
			//Vector2 ICOuv = new Vector2(0, 0);
			//ICOuv.x = (Mathf.Atan2(unitVector.x, Mathf.Abs(unitVector.z)) + Mathf.PI) / Mathf.PI / 2;
			//ICOuv.y = (Mathf.Acos(unitVector.y) + Mathf.PI) / Mathf.PI - 1;
			//uvMaps[i] = new Vector2(ICOuv.x, ICOuv.y);
            vertices[i] *= waterRadius;
        }

        mesh.vertices = vertices;
        //mesh.normals = vertices;
        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(origin, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        
        mesh.uv = waterLevelData;
        waterMaterial.SetFloat("_MinLevel", minTerrainValue);
        waterMaterial.SetFloat("_WaterLine", terrainRadius);

        return mesh;
    }

    public Color[] GetColors(Vector3[] vertices){
        Color[] colors = new Color[vertices.Length];
        
        for(int i=0; i<vertices.Length; i++){
            float distance = Vector3.Distance(vertices[i], Vector3.zero);
            distance = Mathf.Clamp(distance, minTerrainValue, maxTerrainValue);
            float height = Mathf.InverseLerp(minTerrainValue, maxTerrainValue, distance);
            colors[i] = colorGradient.Evaluate(height);   
        }

        return colors;
    }
}


