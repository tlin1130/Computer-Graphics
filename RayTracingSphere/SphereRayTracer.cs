using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SphereRayTracer : MonoBehaviour {

    // Initialization
    Texture2D RayTracingResult;

    // Start is called before the first frame update
    void Start() {

        Camera this_camera = gameObject.GetComponent<Camera>();
        Debug.Assert(this_camera);

        int pixel_width = this_camera.pixelWidth;
        int pixel_height = this_camera.pixelHeight;

        // viewport (image plane) initializtion
        float viewport_width = Screen.width;
        float viewport_height = Screen.height;
        float left = viewport_width / -2;
        float right = viewport_width / 2;
        float bottom = viewport_height / -2;
        float top = viewport_height / 2;

        float distance = viewport_width / 2;
        Vector3 viewportCenter = new Vector3(0.0f, 0.0f, distance * -1);

        // sphere initialization
        Vector3 sphereCenter = new Vector3(0.0f, 0.0f, distance * -2);
        float sphereRadius = 0.25f * distance;

        // light initialization
        Vector3 lightsource = new Vector3(-0.5f * distance, 1.0f * distance, -1.5f * distance);
        float lightIntensity = 0.7f;
        Vector3 diffuseColor = new Vector3(1f,0.92f,0.016f);
        Vector3 ambientColor = new Vector3(1f, 0.92f, 0.016f);
        float ambientIntensity = 0.3f;
        Vector3 ambient = new Vector3(ambientColor.x * ambientIntensity, ambientColor.y * ambientIntensity, ambientColor.z * ambientIntensity);

        RayTracingResult = new Texture2D(pixel_width, pixel_height);

        // (U,V) is the world coordinates of pixel (i,j) on the image plane
        float U;
        float V;

        for (int i = 0; i < pixel_width; ++i) {
            for (int j = 0; j < pixel_height; ++j) {

                //compute viewing ray
                U = left + (right - left) * (i + 0.5f) / pixel_width;
                V = bottom + (top - bottom) * (j + 0.5f) / pixel_height;
                Vector3 rayDirection = new Vector3(U, V, -1 * distance);
                rayDirection = Vector3.Normalize(rayDirection);
                Vector3 rayOrigin = new Vector3(0.0f, 0.0f, 0.0f);

                //compute discriminant 
                float B_squared = Mathf.Pow(Vector3.Dot(rayDirection, rayOrigin - sphereCenter),2);
                float fourAC = Vector3.Dot(rayDirection, rayDirection) * (Vector3.Dot(rayOrigin - sphereCenter, rayOrigin - sphereCenter) - sphereRadius * sphereRadius);

                float dis = B_squared - fourAC;

                // compute intersection point and shading
                if (dis < 0){

                    RayTracingResult.SetPixel(i, j, Color.gray);

                } else if (dis > 0){

                    float t1 = (-1 * Vector3.Dot(rayDirection, rayOrigin - sphereCenter) + Mathf.Sqrt(dis)) / Vector3.Dot(rayDirection, rayDirection);
                    float t2 = (-1 * Vector3.Dot(rayDirection, rayOrigin - sphereCenter) - Mathf.Sqrt(dis)) / Vector3.Dot(rayDirection, rayDirection);
                    float t_min = Mathf.Min(t1,t2);

                    // compute n, p, l
                    Vector3 normal = new Vector3((rayOrigin + t_min * rayDirection - sphereCenter).x, (rayOrigin + t_min * rayDirection - sphereCenter).y, (rayOrigin + t_min * rayDirection - sphereCenter).z);
                    normal = (1 / sphereRadius) * normal;
                    Vector3 p = new Vector3((rayOrigin + rayDirection * t_min).x, (rayOrigin + rayDirection * t_min).y, (rayOrigin + rayDirection * t_min).z);
                    Vector3 l = new Vector3((lightsource - p).x, (lightsource - p).y, (lightsource - p).z);
                    l = Vector3.Normalize(l);


                    Vector3 h = new Vector3((rayDirection + l).x, (rayDirection + l).y, (rayDirection + l).z);
                    h = Vector3.Normalize(h);

                    float max = Mathf.Max(0f, Vector3.Dot(normal,l)); //for Lambertian shading
                    float max2 = (Mathf.Pow(Mathf.Max(0f, Vector3.Dot(normal, h)), 100f)) * lightIntensity; //for specular shading

                    Vector3 pixelColor = new Vector3((diffuseColor*lightIntensity*max).x + ambient.x + max2*0.5f, (diffuseColor*lightIntensity*max).y + ambient.y + max2*0.5f, (diffuseColor*lightIntensity*max).z + ambient.z + max2*0.5f);
                    Color C = new Color(pixelColor.x, pixelColor.y, pixelColor.z, 1.0f);

                    RayTracingResult.SetPixel(i, j, C);

                } else {

                    float t = (-1 * Vector3.Dot(rayDirection, rayOrigin - sphereCenter)) / Vector3.Dot(rayDirection, rayDirection);

                    // compute n, p, l
                    Vector3 normal = new Vector3((rayOrigin + t * rayDirection - sphereCenter).x, (rayOrigin + t * rayDirection - sphereCenter).y, (rayOrigin + t * rayDirection - sphereCenter).z);
                    normal = (1 / sphereRadius) * normal;
                    Vector3 p = new Vector3((rayOrigin + rayDirection * t).x, (rayOrigin + rayDirection * t).y, (rayOrigin + rayDirection * t).z);
                    Vector3 l = new Vector3((lightsource - p).x, (lightsource - p).y, (lightsource - p).z);
                    l = Vector3.Normalize(l);

                    Vector3 h = new Vector3((rayDirection + l).x, (rayDirection + l).y, (rayDirection + l).z);
                    h = Vector3.Normalize(h);

                    float max = Mathf.Max(0f, Vector3.Dot(normal, l)); //for Lambertian shading
                    float max2 = (Mathf.Pow(Mathf.Max(0f, Vector3.Dot(normal, h)), 100f)) * lightIntensity; //for specular shading

                    Vector3 pixelColor = new Vector3((diffuseColor*lightIntensity*max).x + ambient.x + max2*0.5f, (diffuseColor*lightIntensity*max).y + max2*0.5f + ambient.y, (diffuseColor*lightIntensity*max).z + ambient.z + max2*0.5f);
                    Color C = new Color(pixelColor.x, pixelColor.y, pixelColor.z, 1.0f);

                    RayTracingResult.SetPixel(i, j, C);

                }

            }
        }
   
        RayTracingResult.Apply();

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        //Show the generated ray tracing image on screen
        Graphics.Blit(RayTracingResult, destination);
    }

}
