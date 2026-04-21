using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.io;
using Assets.Scripts.io.FM;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class LidarScanExporter : ExportDatasetInterface
{
    public LidarExportSettings exportSettings;

    private int currentFileId = 0;
    private LidarCustomPass LidarRenderer;
    NativeArray<Color32> colorData;
    NativeArray<Color> positionData;
    
public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
    {
        yield return new WaitUntil(() => colorData == default && positionData == default);
        currentFileId = fileID;
        AsyncGPUReadback.Request(LidarRenderer.color360Texture, 0, TextureFormat.RGBA32, OnColorReadback);
        AsyncGPUReadback.Request(LidarRenderer.depth360Texture, 0, TextureFormat.RGBAFloat, OnPositionReadback);
    }

    void OnColorReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("Color RT readback failed!");
            return;
        }
        colorData = request.GetData<Color32>();

        if (exportSettings.binary)
            PLYExportBinary();
        else
            PLYExportASCII();
    }

    void OnPositionReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("Position RT readback failed!");
            return;
        }
        positionData = request.GetData<Color>();

        if (exportSettings.binary)
            PLYExportBinary();
        else
            PLYExportASCII();
    }

    void PLYExportBinary()
    {
        if (!colorData.IsCreated || !positionData.IsCreated) return;
        if (colorData == default || positionData == default) return;

        List<Color32> localColorData = new List<Color32>(colorData);
        List<Color> localVertexData = new List<Color>(positionData);
        positionData.Dispose();
        positionData = default;
        colorData.Dispose();
        colorData = default;

        int vertexStride = 3 * sizeof(float) + 3 * sizeof(byte);
        byte[] vertexBuffer = new byte[localVertexData.Count * vertexStride];

        int count = 0;
        // Parallel fill
        Parallel.For(0, localVertexData.Count, i =>
        {
            var v = localVertexData[i];
            if (v.r == 0 && v.g == 0 && v.b == 0)
                return;

            var c = localColorData[i];
            int index = Interlocked.Increment(ref count) - 1;
            int offset = index * vertexStride;

            Buffer.BlockCopy(BitConverter.GetBytes(v.r), 0, vertexBuffer, offset + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(v.g), 0, vertexBuffer, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(v.b), 0, vertexBuffer, offset + 8, 4);
            vertexBuffer[offset + 12] = (byte)(c.r);
            vertexBuffer[offset + 13] = (byte)(c.g );
            vertexBuffer[offset + 14] = (byte)(c.b );
        });

        string filePath = getFullPath() + "pointCloud_" + currentFileId + ".ply";

        using (var fs = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            // --- Write PLY header (ASCII) ---
            string header =
                "ply\n" +
                "format binary_little_endian 1.0\n" +
                $"element vertex {count}\n" +
                "property float x\n" +
                "property float y\n" +
                "property float z\n" +
                "property uchar red\n" +
                "property uchar green\n" +
                "property uchar blue\n" +
                "end_header\n";

            // Header must be written as ASCII
            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
            writer.Write(headerBytes);

            writer.Write(vertexBuffer, 0, count * vertexStride);
        }
    }

    void PLYExportASCII()
    {
        if (!colorData.IsCreated || !positionData.IsCreated) return;
        if (colorData == default || positionData == default) return;

        List<Color32> localColorData = new List<Color32>(colorData);
        List<Color> localVertexData = new List<Color>(positionData);
        positionData.Dispose();
        positionData = default;
        colorData.Dispose();
        colorData = default;

        int count = 0;
        for(int i = 0; i < localVertexData.Count ; ++i)
            if (localVertexData[i].r != 0 || localVertexData[i].g != 0 | localVertexData[i].b != 0)
                count++;

        string filePath = getFullPath() + "pointCloud_" + currentFileId + ".ply";
        using (var writer = new StreamWriter(filePath))
        {
            // Write PLY header
            writer.WriteLine("ply");
            writer.WriteLine("format ascii 1.0");
            writer.WriteLine($"element vertex {count}");
            writer.WriteLine("property float x");
            writer.WriteLine("property float y");
            writer.WriteLine("property float z");
            writer.WriteLine("property uchar red");
            writer.WriteLine("property uchar green");
            writer.WriteLine("property uchar blue");
            writer.WriteLine("end_header");

            // Write vertices
            for (int i = 0; i < localVertexData.Count; ++i)
            {
                var v = localVertexData[i];
                var c = localColorData[i];
                if (v.r != 0 || v.g != 0 | v.b != 0)
                    writer.WriteLine($"{v.r.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{v.g.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{v.b.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{c.r} {c.g} {c.b}");
            }
        }
    }



    protected override void setupExportPath()
    {
        datasetPrefixPath = "lidar/";
        ensureDir(getFullPath());
    }

    public override void onRenderStart(Camera camera, int fileID)
    {
        LidarRenderer.prepareExecute();
    }

    protected override void setupCustomPasses(Camera camera)
    {
        LidarRenderer = new LidarCustomPass(exportSettings, camera);
        LidarRenderer.name = "LidarPass";
        LidarRenderer.enabled = true;
        customPassVolume.customPasses.Add(LidarRenderer);
    }
}
