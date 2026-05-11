/*
This file is a substantial portion from the software taken from https://github.com/sugi-cho/CCL-GPU under the MIT license.
With some changes to make it compatibel with CAD2Render.

MIT License
Copyright (c) 2018 Hironori Sugino

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Assets.Scripts.io;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Defect Texture CCL")]
public class DefectTextureCCLHandler : MaterialRandomizerInterface
{
    int width;
    int height;
    const int numMaxLabels = 255 * 2;

    private ComputeShader cclCompute;

    bool buffersCreated = false;

    ComputeBuffer labelFlgBuffer;
    ComputeBuffer labelAppendBuffer;
    ComputeBuffer countBuffer;
    RenderTexture labelTex;


    public void Awake()
    {
        cclCompute = MyResourceManager.loadComputeShader("CCL");
    }

    private void createBuffers(Vector2Int resolution)
    {
        width = resolution.x;
        height = resolution.y;

        labelTex = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        labelTex.filterMode = FilterMode.Point;
        labelTex.enableRandomWrite = true;
        labelTex.Create();

        labelFlgBuffer = new ComputeBuffer(width * height, sizeof(int));
        labelAppendBuffer = new ComputeBuffer(numMaxLabels, sizeof(int), ComputeBufferType.Append);
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        buffersCreated = true;
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if (textures.get(MaterialTextures.MapTypes.defectMap) == null)
            return;
        if (!buffersCreated)
            createBuffers(textures.resolution);

        cclCompute.SetInt("texWidth", width);
        cclCompute.SetInt("texHeight", height);

        var kernel = cclCompute.FindKernel("init");
        cclCompute.SetTexture(kernel, "inTex", textures.get(MaterialTextures.MapTypes.defectMap));
        cclCompute.SetTexture(kernel, "labelTex", labelTex);
        cclCompute.Dispatch(kernel, width / 8, height / 8, 1);

        kernel = cclCompute.FindKernel("columnWiseLabel");
        cclCompute.SetTexture(kernel, "labelTex", labelTex);
        cclCompute.Dispatch(kernel, width / 8, 1, 1);

        var itr = Mathf.Log(width, 2);
        var div = 2;
        for (var i = 0; i < itr; i++)
        {
            kernel = cclCompute.FindKernel("mergeLabels");
            cclCompute.SetTexture(kernel, "labelTex", labelTex);
            cclCompute.SetInt("div", div);

            cclCompute.Dispatch(kernel, Mathf.Max(width / (2 << i) / 8, 1), 1, 1);
            div *= 2;
        }

        kernel = cclCompute.FindKernel("clearLabelFlag");
        cclCompute.SetTexture(kernel, "labelTex", labelTex);
        cclCompute.SetBuffer(kernel, "labelBuffer", labelAppendBuffer);
        cclCompute.SetBuffer(kernel, "labelFlg", labelFlgBuffer);
        cclCompute.Dispatch(kernel, width / 8, height / 8, 1);

        kernel = cclCompute.FindKernel("setRootLabel");
        cclCompute.SetTexture(kernel, "labelTex", labelTex);
        cclCompute.SetBuffer(kernel, "labelFlg", labelFlgBuffer);
        cclCompute.Dispatch(kernel, width / 8, height / 8, 1);

        labelAppendBuffer.SetCounterValue(0);
        kernel = cclCompute.FindKernel("countLabel");
        cclCompute.SetBuffer(kernel, "labelFlg", labelFlgBuffer);
        cclCompute.SetBuffer(kernel, "labelAppend", labelAppendBuffer);
        cclCompute.Dispatch(kernel, width / 8, height / 8, 1);

        ComputeBuffer.CopyCount(labelAppendBuffer, countBuffer, 0);
        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);
        int nrOfLabels = counter[0];
        cclCompute.SetInt("numLabels", nrOfLabels);

        kernel = cclCompute.FindKernel("maskEachLabelSequential");
        cclCompute.SetBuffer(kernel, "labelBuffer", labelAppendBuffer);
        cclCompute.SetTexture(kernel, "labelTex", labelTex);
        cclCompute.SetTexture(kernel, "inTex", textures.get(MaterialTextures.MapTypes.defectMap));
        cclCompute.Dispatch(kernel, width / 8, height / 8, 1);

        return;
    }
    public override int getPriority()
    {
        return -50;//execute after rust generator
    }

    private void OnDestroy()
    {
        new List<RenderTexture>(new[] { labelTex })
            .ForEach(rt => rt?.Release());
        new List<ComputeBuffer>(new[] { labelFlgBuffer, labelAppendBuffer, countBuffer })
            .ForEach(bf => bf?.Dispose());
    }
}