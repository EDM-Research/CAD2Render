using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


//public static class CoordinateConverter
//{
//    public static float[] CartesianToSpherical(float x, float y, float z)
//    {
//        float radius = 1.0f;
//        float[] retVal = new float[2];

//        if (x == 0)
//        {
//            x = Mathf.Epsilon;
//        }
//        retVal[0] = Mathf.Atan(z / x);

//        if (x < 0)
//        {
//            retVal[0] += Mathf.PI;
//        }

//        retVal[1] = Mathf.Asin(y / radius);

//        return retVal;
//    }

//    public static float[] CartesianToSpherical(Vector3 v)
//    {
//        return CartesianToSpherical(v.x, v.y, v.z);
//    }


//    public static Vector3 SphericalToCartesian(float latitude, float longitude, float radius)
//    {
//        float a = radius * Mathf.Cos(longitude);
//        float x = a * Mathf.Cos(latitude);
//        float y = radius * Mathf.Sin(longitude);
//        float z = a * Mathf.Sin(latitude);

//        return new Vector3(x, y, z);
//    }
//}


public class CameraGenerator : MonoBehaviour
{
    public Transform poi;
    public float radius;
    public float speed;
    public float longitude = 30;

    float alpha = 0.0f;
    int frameCount = 0;

    public int fileCounter;
    private Camera Camera
    {
        get
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }
    private Camera _camera;


    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();


        Vector3 offset = SphericalCoordinates.SphericalToCartesian(alpha * Mathf.PI / 180.0f, longitude * Mathf.PI / 180.0f, radius);
        transform.position = poi.position + offset;
        transform.LookAt(poi);
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;

        if (frameCount > 200) {
            frameCount = 0;
            alpha += 5;// Time.time * speed;

            Capture();

            Vector3 offset = SphericalCoordinates.SphericalToCartesian(alpha * Mathf.PI / 180.0f, longitude * Mathf.PI / 180.0f, radius);

            // Rotate the camera every frame so it keeps looking at the target

            transform.position = poi.position + offset;
            transform.LookAt(poi);
        }
    }


    private void LateUpdate()
    {
        //Capture();
    }


    public void Capture()
    {
        if (Camera.targetTexture == null)
            return;

        RenderTexture outputMap = new RenderTexture(1024, 1024, 32);
        outputMap.name = "Whatever";
        outputMap.enableRandomWrite = true;
        outputMap.Create();
        //Put the above stuff in Awake()  if you need to update this every frame...
        RenderTexture.active = outputMap;
        GL.Clear(true, true, Color.black);
        //Graphics.Blit(mainTexture, outputMap, rtMat);



        RenderTexture activeRenderTexture = RenderTexture.active;
        Camera.targetTexture = RenderTexture.active;
        //RenderTexture.active = Camera.targetTexture;

        Camera.Render();

        //RenderTexture.active = null;
        //Camera.targetTexture = null;

        Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        File.WriteAllBytes("../renderings/" + fileCounter + ".png", bytes);
        fileCounter++;
    }
}
