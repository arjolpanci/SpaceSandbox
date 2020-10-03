using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcosphereTest : MonoBehaviour
{

    public Shader shader;
    public int resolution;

    void OnValidate()
    {
        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        if(meshFilter != null){
            meshFilter.mesh = IcoSphere.GetIcosphereMesh(resolution);
        }
        if(meshRenderer != null){
            meshRenderer.sharedMaterial = new Material(shader);
        }
    }

    private void OnDrawGizmos() {
        for(int i=0; i<IcoSphere.vertices.Length; i++){
            if(i == 13){
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(IcoSphere.vertices[i], 0.04F);
            }else{
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(IcoSphere.vertices[i], 0.04F);
            }
            
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
