using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{

    public PlanetShapeSettings shapeSettings;
    public GameObject player, mainStar;

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

    public void GenerateObject(){
        origin = transform.position;
        
        SetupObject(ref terrainSphere, "TerrainSphere", shapeSettings.terrainMaterial, shapeSettings.CreateTerrain(origin), true);
        if(shapeSettings.hasWater) 
            SetupObject(ref waterSphere, "WaterSphere", shapeSettings.waterMaterial, shapeSettings.CreateWater(origin), false);
            if(waterSphere != null) shapeSettings.UpdateWaterProperites(waterSphere.GetComponent<Renderer>(), mainStar.transform.position);
        if(shapeSettings.hasAtmosphere) 
            SetupObject(ref atmosphereSphere, "AtmosphereSphere", shapeSettings.atmosphereMaterial, shapeSettings.CreateAtmosphere(origin), false);   
    }

    public void SetupObject(ref GameObject gameObject, string objectName, Material material, Mesh mesh, bool hasCollider){
        if(gameObject == null) gameObject = new GameObject(objectName);
        gameObject.transform.parent = this.gameObject.transform;
        if(gameObject.GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        if(gameObject.GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.transform.position = origin;
        meshFilter.sharedMesh = mesh;
        meshFilter.sharedMesh.RecalculateBounds();
        if(!hasCollider) return;
        if(gameObject.GetComponent<MeshCollider>() == null) gameObject.AddComponent<MeshCollider>();
        gameObject.GetComponent<MeshCollider>().sharedMesh = null;
        gameObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;
        //if(objectName.Equals("WaterSphere")) waterRenderer = gameObject.GetComponent<Renderer>();
        //if(objectName.Equals("AtmosphereSphere")) atmosphereRenderer = gameObject.GetComponent<Renderer>();
    }
    
}
