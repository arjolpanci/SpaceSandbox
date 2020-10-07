using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySphereTest : MonoBehaviour
{

    public Transform sphere;

    Vector2 raySphere (Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDir) {
        //Vector3 offset = rayOrigin - sphereCentre;
        //const float a = 1; // set to dot(rayDir, rayDir) if rayDir might be unnormalized
        //float b = 2 * Vector3.Dot (offset, rayDir);
        //float c = Vector3.Dot (offset, offset) - sphereRadius * sphereRadius;
        //float discriminant = b * b - 4 * a * c;
        // No intersections: discriminant < 0
        // 1 intersection: discriminant == 0
        // 2 intersections: discriminant > 0
        /*if (discriminant > 0) {
            float s = Mathf.Sqrt (discriminant);
            float dstToSphereNear = Mathf.Max (0, (-b - s) / (2 * a));
            float dstToSphereFar = (-b + s) / (2 * a);

            if (dstToSphereFar >= 0) {
                return new Vector2 (dstToSphereNear, dstToSphereFar - dstToSphereNear);
            }
        }*/

        Vector3 distance = rayOrigin - sphereCenter;
        float a = Vector3.Dot(rayDir, rayDir);
        float b = 2 * Vector3.Dot(rayDir,distance);
        float c = Vector3.Dot(distance, distance) - sphereRadius*sphereRadius;

        float d = b*b - 4*a*c;

        if(d == 0){
                float point1 = -b / 2*a;
                return new Vector2(point1, 0);
            }else if(d>0){
                float point1 = (-b + Mathf.Sqrt(d)) / 2*a;
                float point2 = (-b - Mathf.Sqrt(d)) / 2*a;
                return new Vector2(point1, point2);
            }

        return new Vector2(5000000, 0);
    }

    void OnDrawGizmos () {
        var r = raySphere (sphere.position, sphere.localScale.x/2, transform.position, transform.forward);
        Gizmos.color = Color.green;
        Gizmos.DrawRay (transform.position, transform.forward * r.x);
        Gizmos.color = Color.red;
        Gizmos.DrawRay (transform.position + transform.forward * r.x, transform.forward * r.y);
        Debug.Log(r.y);
    }

}
