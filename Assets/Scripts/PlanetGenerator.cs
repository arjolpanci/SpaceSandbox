using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{

    public PlanetShapeSettings shapeSettings;

    public GameObject player;
    
    private float minTerrainValue;
    private float maxTerrainValue;

    GameObject terrainSphere, atmosphereSphere, waterSphere;
    float gravity = -10F;
    Vector3 origin;
    
    void OnValidate(){
        if(this.enabled == true) GenerateObject();
    }

    void Start()
    {
        origin = transform.position;
        //GenerateObject();
    }

    public void GenerateObject(){
        origin = transform.position;
        
        if(terrainSphere == null) terrainSphere = new GameObject("TerrainSphere");
        terrainSphere.transform.parent = this.gameObject.transform;

        if(terrainSphere.GetComponent<MeshRenderer>() == null) terrainSphere.AddComponent<MeshRenderer>().sharedMaterial = shapeSettings.terrainMaterial;
        if(terrainSphere.GetComponent<MeshFilter>() == null) terrainSphere.AddComponent<MeshFilter>();
        MeshFilter terrainMeshFilter = terrainSphere.GetComponent<MeshFilter>();
        terrainMeshFilter.transform.position = origin;
        terrainMeshFilter.sharedMesh = shapeSettings.CreateTerrain(origin);
        terrainMeshFilter.sharedMesh.RecalculateBounds();
        if(terrainSphere.GetComponent<MeshCollider>() == null) terrainSphere.AddComponent<MeshCollider>();
        terrainSphere.GetComponent<MeshCollider>().sharedMesh = null;
        terrainSphere.GetComponent<MeshCollider>().sharedMesh = terrainMeshFilter.sharedMesh;

        
        if(shapeSettings.hasWater){
            if(waterSphere == null) waterSphere = new GameObject("WaterSphere");
            waterSphere.transform.parent = this.gameObject.transform;

            if(waterSphere.GetComponent<MeshRenderer>() == null) waterSphere.AddComponent<MeshRenderer>().sharedMaterial = shapeSettings.waterMaterial;
            if(waterSphere.GetComponent<MeshFilter>() == null) waterSphere.AddComponent<MeshFilter>();
            MeshFilter waterMeshFilter = waterSphere.GetComponent<MeshFilter>();
            waterMeshFilter.transform.position = origin;
            waterMeshFilter.sharedMesh = shapeSettings.CreateWater(origin);
        }
        

        if(shapeSettings.hasAtmosphere){
            if(atmosphereSphere == null) atmosphereSphere = new GameObject("AtmosphereSphere");
            atmosphereSphere.transform.parent = this.gameObject.transform;

            if(atmosphereSphere.GetComponent<MeshRenderer>() == null) atmosphereSphere.AddComponent<MeshRenderer>().sharedMaterial = shapeSettings.atmosphereMaterial;
            if(atmosphereSphere.GetComponent<MeshFilter>() == null) atmosphereSphere.AddComponent<MeshFilter>();
            MeshFilter atmosphereMeshFilter = atmosphereSphere.GetComponent<MeshFilter>();
            atmosphereMeshFilter.transform.position = origin;
            atmosphereMeshFilter.sharedMesh = shapeSettings.CreateAtmosphere(origin);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null){
            Rigidbody rb = player.GetComponent<Rigidbody>();
            //CharacterController playerController = player.GetComponent<CharacterController>();
            Transform playerTransform = player.GetComponent<Transform>();
            Vector3 gravityDirection = (playerTransform.position - origin).normalized;

            if(rb != null){
                rb.AddForce(gravityDirection * gravity);
            }

            if(playerTransform != null){
                Vector3 playerUp = playerTransform.up;
                Quaternion targetRotation = Quaternion.FromToRotation(playerUp, gravityDirection) * playerTransform.rotation;
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * 50);
            }
        }
        
    }
}
