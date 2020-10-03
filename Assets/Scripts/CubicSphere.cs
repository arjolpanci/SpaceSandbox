using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicSphere
{
    
    public static Mesh GetSphereMesh(int resolution){
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;

        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
        Vector3[] vertices = new Vector3[(resolution+1) * (resolution+1) * 6];
        Vector2[] uvMap = new Vector2[vertices.Length];

        int[] triangles = new int[6 * resolution * resolution * 6];
        int vertexCount = 0;
        int triangleCount = 0;

        for(int i=0; i<directions.Length; i++){
            Vector3 up = directions[i];
            Vector3 right = new Vector3(up.y, up.z, up.x);
            Vector3 back = Vector3.Cross(up, right);
            for(int z=0; z<=resolution; z++){
                for(int x=0; x<=resolution; x++){
                    Vector2 percent = new Vector2(x, z) / resolution;
                    Vector3 pointOnCube = up + (percent.x-.5F)*2*right + (percent.y-.5F)*2*back;
                    Vector3 pointOnSphere = pointOnCube.normalized;
                    vertices[vertexCount] = pointOnSphere;
                    uvMap[vertexCount] = new Vector2((float)x/resolution, (float)z/resolution);

                    if (x != resolution && z != resolution) {
                        triangles[triangleCount] = vertexCount;
                        triangles[triangleCount+1] = vertexCount + resolution + 2;
                        triangles[triangleCount+2] = vertexCount + resolution + 1;

                        triangles[triangleCount+3] = vertexCount;
                        triangles[triangleCount+4] = vertexCount + 1;
                        triangles[triangleCount+5] = vertexCount + resolution + 2;
                        triangleCount+=6;
                    }
                    vertexCount++;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = vertices;
        mesh.uv = uvMap;
         
        return mesh;
    }
    
    
}
