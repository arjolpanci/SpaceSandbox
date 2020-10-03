using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IcoSphere
{

    public static float r = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
    public static Vector3[] vertices;
    public static List<Face> faces;

    public static Mesh GetIcosphereMesh(int resolution){
        Mesh mesh = new Mesh();
        mesh.Clear();
        if(resolution < 5) return mesh;
        mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        CalculateFirstVertices();
        List<Face> triangles = SubDivideFaces(resolution);
        mesh.vertices = vertices;
        mesh.triangles = GetTrianglesFromFaces(triangles);
        mesh.normals = vertices;
        return mesh;
    }

    public static void CalculateFirstVertices(){
        vertices = new Vector3[12];
        faces = new List<Face>();

        //Initial 12 vertices
        vertices[0] = (new Vector3(-1,  r,  0));
        vertices[1] = (new Vector3( 1,  r,  0));
        vertices[2] = (new Vector3(-1, -r,  0));
        vertices[3] = (new Vector3( 1, -r,  0));

        vertices[4] = (new Vector3( 0, -1,  r));
        vertices[5] = (new Vector3( 0,  1,  r));
        vertices[6] = (new Vector3( 0, -1, -r));
        vertices[7] = (new Vector3( 0,  1, -r));

        vertices[8] = (new Vector3( r,  0, -1));
        vertices[9] = (new Vector3( r,  0,  1));
        vertices[10] = (new Vector3(-r,  0, -1));
        vertices[11] = (new Vector3(-r,  0,  1));


        //The 20 icosahedron faces

        // 5 faces around point 0
        faces.Add(new Face(0, 11, 5));
        faces.Add(new Face(0, 5, 1));
        faces.Add(new Face(0, 1, 7));
        faces.Add(new Face(0, 7, 10));
        faces.Add(new Face(0, 10, 11));

        // 5 adjacent faces
        faces.Add(new Face(1, 5, 9));
        faces.Add(new Face(5, 11, 4));
        faces.Add(new Face(11, 10, 2));
        faces.Add(new Face(10, 7, 6));
        faces.Add(new Face(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new Face(3, 9, 4));
        faces.Add(new Face(3, 4, 2));
        faces.Add(new Face(3, 2, 6));
        faces.Add(new Face(3, 6, 8));
        faces.Add(new Face(3, 8, 9));

        // 5 adjacent faces
        faces.Add(new Face(4, 9, 5));
        faces.Add(new Face(2, 4, 11));
        faces.Add(new Face(6, 2, 10));
        faces.Add(new Face(8, 6, 7));
        faces.Add(new Face(9, 8, 1));
    }

    public static int[] GetTrianglesFromFaces(List<Face> faces){
        int[] triangles = new int[faces.Count*3];
        int triangleCount = 0;

        for(int i=0; i<faces.Count; i++){
            triangles[triangleCount] = faces.ElementAt(i).v1;
            triangles[triangleCount+1] = faces.ElementAt(i).v2;
            triangles[triangleCount+2] = faces.ElementAt(i).v3;
            triangleCount += 3;
        }

        return triangles;
    }

    public static List<Face> SubDivideFaces(int div){
        List<Face> triangleList = new List<Face>();
        List<Vector3> allVertices = vertices.ToList();

        int indexOffset=0, faceCount=0, vertexCount=0;
        float x,y,z;
        Vector3 inc, start;

        foreach(Face f in faces){
            indexOffset = faceCount*vertexCount;
            vertexCount = 0;

            //Adding Left-Side Vertices
            x = vertices[f.v3].x - vertices[f.v1].x;
            y = vertices[f.v3].y - vertices[f.v1].y;
            z = vertices[f.v3].z - vertices[f.v1].z;
            inc = (new Vector3(x,y,z)) / div;

            start = vertices[f.v1];
            for(int i=0; i<div-1; i++){
                Vector3 newPoint = start+inc;
                allVertices.Add(newPoint);
                vertexCount++;
                start = newPoint;
            }

            //Adding Bottom Vertices
            x = vertices[f.v2].x - vertices[f.v1].x;
            y = vertices[f.v2].y - vertices[f.v1].y;
            z = vertices[f.v2].z - vertices[f.v1].z;
            inc = (new Vector3(x,y,z)) / div;

            start = vertices[f.v1];
            for(int i=0; i<div-1; i++){
                Vector3 newPoint = start+inc;
                allVertices.Add(newPoint);
                vertexCount++;
                start = newPoint;
            }

            //Adding Right Vertices
            x = vertices[f.v3].x - vertices[f.v2].x;
            y = vertices[f.v3].y - vertices[f.v2].y;
            z = vertices[f.v3].z - vertices[f.v2].z;
            inc = (new Vector3(x,y,z)) / div;

            start = vertices[f.v2];
            for(int i=0; i<div-1; i++){
                Vector3 newPoint = start+inc;
                allVertices.Add(newPoint);
                vertexCount++;
                start = newPoint;
            }

            //Adding Middle Vertices
            for(int i=1; i<=div-1; i++){
                x = allVertices.ElementAt(9+i+(2*div) + indexOffset).x - allVertices.ElementAt(11+i + indexOffset).x;
                y = allVertices.ElementAt(9+i+(2*div) + indexOffset).y - allVertices.ElementAt(11+i + indexOffset).y;
                z = allVertices.ElementAt(9+i+(2*div) + indexOffset).z - allVertices.ElementAt(11+i + indexOffset).z;
                inc = (new Vector3(x,y,z)) / (div-i);
                start = allVertices.ElementAt(11+i + indexOffset);

                for(int j=0; j<div-i-1; j++){
                    Vector3 newPoint = start+inc;
                    //if(DoesVertexExist(newPoint, allVertices, div)) continue;
                    allVertices.Add(newPoint);
                    vertexCount++;
                    start = newPoint;
                }
            }

            faceCount++;

            //Adding new faces
            for(int i=0; i<div; i++){
                int v1=0, v2=0, v3=0;
                for(int j=0; j<div-i; j++){
                    //First row
                    if(i==0){
                        if(j==0){
                            v1 = f.v1;
                            v2 = GetLeftIndex(i+1, indexOffset);
                            v3 = GetBottomIndex(j+1, div, indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            continue;
                        }else if(j==div-i-1){
                            v1 = GetBottomIndex(j, div, indexOffset);
                            v2 = GetMiddleIndex(i+1,j-1,div,indexOffset);
                            v3 = GetRightIndex(i+1, div, indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            v1 = GetBottomIndex(j, div, indexOffset);
                            v2 = GetRightIndex(i+1, div, indexOffset);
                            v3 = f.v2;
                            triangleList.Add(new Face(v3, v2, v1));
                            continue;
                        }else if (j==1){
                            v1 = GetBottomIndex(j, div, indexOffset);
                            v2 = GetLeftIndex(i+1, indexOffset);
                            v3 = GetMiddleIndex(i+1,j,div,indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            v1 = GetBottomIndex(j, div, indexOffset);
                            v2 = GetMiddleIndex(i+1, j, div, indexOffset);
                            v3 = GetBottomIndex(j+1, div, indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            continue;
                        }else{
                            v1 = GetBottomIndex(j, div, indexOffset);
                            v2 = GetMiddleIndex(i+1, j-1, div, indexOffset);
                            v3 = GetMiddleIndex(i+1, j, div, indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            v1 = (div+j+10)+indexOffset;
                            v2 = (3*div+5+j+3*(i+1))+indexOffset;
                            v3 = (div+j+1+10)+indexOffset;
                            triangleList.Add(new Face(v3, v2, v1));
                            continue;
                        }
                    //Last Row
                    }else if(i==div-1){
                        v1 = GetLeftIndex(i, indexOffset);
                        v2 = f.v3;
                        v3 = GetRightIndex(i, div, indexOffset);
                        triangleList.Add(new Face(v3, v2, v1));
                        continue;
                    //Mid Rows
                    }else{
                        if(j==0){
                            v1 = GetLeftIndex(i, indexOffset);
                            v2 = GetLeftIndex(i+1, indexOffset);
                            v3 = GetMiddleIndex(i, j+1, div, indexOffset);
                            triangleList.Add(new Face(v3, v2, v1));
                            continue;
                        }else if(j==div-i-1){
                            if(i<div-2){
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetMiddleIndex(i+1, j-1, div, indexOffset);
                                v3 = GetRightIndex(i+1, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetRightIndex(i+1, div, indexOffset);
                                v3 = GetRightIndex(i, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                continue;
                            }else{
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetLeftIndex(i+1, indexOffset);
                                v3 = GetRightIndex(i+1, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetRightIndex(i+1, div, indexOffset);
                                v3 = GetRightIndex(i, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                continue;
                            }
                        }else{
                            if(j==1){
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetLeftIndex(i+1, indexOffset);
                                v3 = GetMiddleIndex(i+1, j, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetMiddleIndex(i+1, j, div, indexOffset);
                                v3 = GetMiddleIndex(i, j+1, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));  
                                continue; 
                            }else{
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetMiddleIndex(i+1, j-1, div, indexOffset);
                                v3 = GetMiddleIndex(i+1, j, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                v1 = GetMiddleIndex(i, j, div, indexOffset);
                                v2 = GetMiddleIndex(i+1, j, div, indexOffset);
                                v3 = GetMiddleIndex(i, j+1, div, indexOffset);
                                triangleList.Add(new Face(v3, v2, v1));
                                continue;
                            }
                        }
                    }
                }
            }
        }

        vertices = allVertices.ToArray();
        for(int i=0; i<vertices.Length; i++){
            vertices[i] = vertices[i].normalized;
        }
        return triangleList;
    }

    public static int GetLeftIndex(int i, int indexOffset){
        return (11+i)+indexOffset;
    }

    public static int GetBottomIndex(int j, int div, int indexOffset){
        return 10+j+div+indexOffset;
    }

    public static int GetRightIndex(int i, int div, int indexOffset){
        return 9+i+2*div+indexOffset;
    }

    public static int GetMiddleIndex(int i, int j, int div, int indexOffset){
        int index = 8+3*div+j;
        for(int k=1; k<i; k++){
            index += div-k-1;
        }
        return index + indexOffset;
    }

    public static void OutputInfo(List<Vector3> vertices, int v1, int v2, int v3, int i, int j){
        string message = "";
        int tot = vertices.Count;
        if(v1 >= tot) message += ("v1 problem:" + v1 + ", " + tot + ", " + i + ", " + j);
        if(v2 >= tot) message += ("v2 problem:" + v2 + ", " + tot + ", " + i + ", " + j);
        if(v3 >= tot) message += ("v3 problem:" + v3 + ", " + tot + ", " + i + ", " + j);
        Debug.Log(message);
    }

    public static bool DoesVertexExist(Vector3 vertex, List<Vector3> vertices, int div){

        foreach(Vector3 v in vertices){
            float dist = Vector3.Distance(vertex, v);
            if(dist < (0.1F/div)) return true;
        }

        return false;
    }

    public class Face{
        public int v1, v2, v3;

        public Face(int v1, int v2, int v3){
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

}
