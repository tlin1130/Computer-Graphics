using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TriangleRayTracer : MonoBehaviour {

    // Initialization
    Texture2D RayTracingResult;

    // Start is called before the first frame update
    void Start() {

        Camera this_camera = gameObject.GetComponent<Camera>();
        Debug.Assert(this_camera);

        int pixel_width = this_camera.pixelWidth;
        int pixel_height = this_camera.pixelHeight;

        // viewport (image plane) initializtion
        int viewport_width = Screen.width;
        int viewport_height = Screen.height;
        float left = viewport_width / -2;
        float right = viewport_width / 2;
        float bottom = viewport_height / -2;
        float top = viewport_height / 2;

        float distance = viewport_width / 2;
        Vector3 viewportCenter = new Vector3(0.0f, 0.0f, distance * -1);
        Vector3 rayOrigin = new Vector3(0.0f, 0.0f, 0.0f);

        Color BackgroundColor = Color.gray;

        // triangle vertices initialization
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(new Vector3(-0.4f * distance, -0.3f * distance, distance * -2f)); //0
        vertices.Add(new Vector3(-0.4f * distance, 0.3f * distance, distance * -2f)); //1
        vertices.Add(new Vector3(0f, -0.3f * distance, distance * -1.8f )); //2
        vertices.Add(new Vector3(0f, 0.3f * distance, distance * -1.8f)); //3
        vertices.Add(new Vector3(0.4f * distance, -0.3f * distance, distance * -2f)); //4
        vertices.Add(new Vector3(0.4f * distance, 0.3f * distance, distance * -2f)); //5

        List<int> indices = new List<int>();
        //triangle 1
        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        //triangle 2
        indices.Add(2);
        indices.Add(1);
        indices.Add(3);
        //triangle 3
        indices.Add(2);
        indices.Add(3);
        indices.Add(5);
        //triangle 4
        indices.Add(2);
        indices.Add(5);
        indices.Add(4);

        RayTracingResult = new Texture2D(pixel_width, pixel_height);

        // color each pixel background color
        for (int i = 0; i < pixel_width; i++) {
            for (int j = 0; j < pixel_height; j++) {
                RayTracingResult.SetPixel(i, j, BackgroundColor);
            }
        }

        for (int id = 0; id <= 11; id += 3) {
            Color PixelColor = BackgroundColor;
            for (int i = 0; i < pixel_width; i++) {
                for (int j = 0; j < pixel_height; j++) {

                    // compute viewing ray
                    float U = left + (right - left) * (i + 0.5f) / pixel_width;
                    float V = bottom + (top - bottom) * (j + 0.5f) / pixel_height;
                    Vector3 rayDirection = new Vector3(U, V, -1f * distance);
                    rayDirection = Vector3.Normalize(rayDirection);

                    // compute ray-triangle intersection
                    float a = vertices[indices[id]].x - vertices[indices[id + 1]].x;
                    float b = vertices[indices[id]].y - vertices[indices[id + 1]].y;
                    float c = vertices[indices[id]].z - vertices[indices[id + 1]].z;
                    float d = vertices[indices[id]].x - vertices[indices[id + 2]].x;
                    float e = vertices[indices[id]].y - vertices[indices[id + 2]].y;
                    float f = vertices[indices[id]].z - vertices[indices[id + 2]].z;
                    float g = rayDirection.x;
                    float h = rayDirection.y;
                    float m = rayDirection.z;
                    float n = vertices[indices[id]].x - rayOrigin.x;
                    float k = vertices[indices[id]].y - rayOrigin.y;
                    float l = vertices[indices[id]].z - rayOrigin.z;
                    float emhf = e * m - h * f;
                    float gfdm = g * f - d * m;
                    float dheg = d * h - e * g;
                    float aknb = a * k - n * b;
                    float ncal = n * c - a * l;
                    float blkc = b * l - k * c;

                    float M = a * emhf + b * gfdm + c * dheg;

                    float beta = (n * emhf + k * gfdm + l * dheg) / M;
                    float gama = (m * aknb + h * ncal + g * blkc) / M;
                    float t = -(f * aknb + e * ncal + d * blkc) / M;

                    if (beta >= 0 && gama > 0 && t >= 0 && (beta + gama) <= 1) {
                        PixelColor.r = 1 - beta - gama; //red
                        PixelColor.g = beta; //green
                        PixelColor.b = gama; //blue
                        PixelColor.a = 1; //alpha
                    } else {
                        PixelColor = BackgroundColor;
                    }

                    RayTracingResult.SetPixel(i, j, beta >= 0 && gama > 0 && t >= 0 && (beta + gama) <= 1 ? PixelColor : RayTracingResult.GetPixel(i, j));

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
