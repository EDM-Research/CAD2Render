using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref exportSettings);
    }

    private LidarCustomPass LidarRenderer;

    public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
    {
        // Scan in this frame: the colour cameras have built up their samples now, and the scene gets
        // randomized again next frame. The pass reads the result back right after the scan and calls
        // writePointCloud when the data has arrived.
        LidarRenderer.requestScan(fileID);
        yield break;
    }

    // Called by the LiDAR pass a few frames after the export scan, with the read-back scan data
    void writePointCloud(int fileId, Matrix4x4 cameraToWorld, Color32[] colors, Color[] positions)
    {
        if (exportSettings.binary)
            PLYExportBinary(fileId, cameraToWorld, colors, positions);
        else
            PLYExportASCII(fileId, cameraToWorld, colors, positions);
    }

    void PLYExportBinary(int fileId, Matrix4x4 cameraToWorld, Color32[] localColorData, Color[] localVertexData)
    {
        int vertexStride = 3 * sizeof(float) + 3 * sizeof(byte);
        byte[] vertexBuffer = new byte[localVertexData.Length * vertexStride];

        int count = 0;
        // Parallel fill
        Parallel.For(0, localVertexData.Length, i =>
        {
            var v = localVertexData[i];
            if (v.a == 0) // ray missed
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

        string filePath = getFullPath() + "pointCloud_" + fileId + ".ply";

        using (var fs = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            // --- Write PLY header (ASCII) ---
            string header =
                "ply\n" +
                "format binary_little_endian 1.0\n" +
                headerComments(cameraToWorld) +
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

    void PLYExportASCII(int fileId, Matrix4x4 cameraToWorld, Color32[] localColorData, Color[] localVertexData)
    {
        int count = 0;
        for(int i = 0; i < localVertexData.Length ; ++i)
            if (localVertexData[i].a != 0) // skip rays that missed
                count++;

        string filePath = getFullPath() + "pointCloud_" + fileId + ".ply";
        using (var writer = new StreamWriter(filePath))
        {
            // Write PLY header
            writer.WriteLine("ply");
            writer.WriteLine("format ascii 1.0");
            writer.Write(headerComments(cameraToWorld));
            writer.WriteLine($"element vertex {count}");
            writer.WriteLine("property float x");
            writer.WriteLine("property float y");
            writer.WriteLine("property float z");
            writer.WriteLine("property uchar red");
            writer.WriteLine("property uchar green");
            writer.WriteLine("property uchar blue");
            writer.WriteLine("end_header");

            // Write vertices
            for (int i = 0; i < localVertexData.Length; ++i)
            {
                var v = localVertexData[i];
                var c = localColorData[i];
                if (v.a != 0) // skip rays that missed
                    writer.WriteLine($"{v.r.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{v.g.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{v.b.ToString(CultureInfo.InvariantCulture)} " +
                                    $"{c.r} {c.g} {c.b}");
            }
        }
    }



    // PLY comment lines (ignored by viewers) describing the coordinate frame and the camera pose
    string headerComments(Matrix4x4 cameraToWorld)
    {
        var m = cameraToWorld;
        string row(int r) => string.Join(" ", new[] { m[r, 0], m[r, 1], m[r, 2], m[r, 3] }
            .Select(value => value.ToString("R", CultureInfo.InvariantCulture)));

        string frame = exportSettings.coordinateSystem == LidarCoordinateSystem.Camera
            ? "comment coordinate_system camera (+X right, +Y down, +Z forward), units mm\n"
            : "comment coordinate_system unity_world, units unity\n";
        return frame +
            "comment camera_to_world maps camera coordinates (+X right, +Y down, +Z forward, mm) to unity world coordinates (unity units), row-major 4x4:\n" +
            $"comment camera_to_world {row(0)}\n" +
            $"comment camera_to_world {row(1)}\n" +
            $"comment camera_to_world {row(2)}\n" +
            $"comment camera_to_world {row(3)}\n";
    }

    protected override void setupExportPath()
    {
        datasetPrefixPath = "lidar/";
        ensureDir(getFullPath());
    }

    // MainExporter switches the main camera on and off in Update (off during the physics frames while recording).
    // The colour cameras follow it, so they run every frame while not recording (live preview) and restart
    // their path tracing together with the main camera while recording.
    private void LateUpdate()
    {
        if (LidarRenderer != null && LidarRenderer.bakingCamera != null)
            LidarRenderer.setColorCamerasEnabled(LidarRenderer.bakingCamera.enabled);
    }

    protected override void setupCustomPasses(Camera camera)
    {
        LidarRenderer = new LidarCustomPass(exportSettings, camera);
        LidarRenderer.name = "LidarPass";
        LidarRenderer.enabled = true;
        LidarRenderer.ScanCompleted += writePointCloud;
        customPassVolume.customPasses.Add(LidarRenderer);
    }

    public override List<(string, RenderTexture)> getTextureOutputs()
    {
        var list = new List<(string, RenderTexture)>
        {
            (datasetPrefixPath + "Cubemap", LidarRenderer.equirect),
            (datasetPrefixPath + "Distance", LidarRenderer.distancePreview)
        };
        return list;
    }
}
