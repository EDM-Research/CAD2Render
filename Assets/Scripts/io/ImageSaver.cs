//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using UnityEngine;
using System.IO;
//using Pngcs.Unity;

public class ImageSaver
{
    private RenderTexture renderTexSRGB;
    private RenderTexture renderTexLin;

    private RenderTexture arraySlice;

    private Texture2D saveTexture;
    private Texture2D saveTextureFloat;
    private Texture2D saveSingleChannelTexture;
    private Texture2D saveSingleChannelTextureFloat;

    public enum Extension { png, jpg, exr };

    public ImageSaver(int width, int height)
    {
        renderTexSRGB = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        renderTexSRGB.Create();
        renderTexLin = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        renderTexLin.Create();

        arraySlice = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        arraySlice.Create();

        saveTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        saveTextureFloat = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        saveSingleChannelTextureFloat = new Texture2D(width, height, TextureFormat.RFloat, false);
        saveSingleChannelTexture = new Texture2D(width, height, TextureFormat.R16, false);
    }

    public void SaveArray(RenderTexture renderTex, int depth, string filename, Extension outputExt, bool gammaCorrection, bool singleChannel = false)
    {
        for(int i = 0; i < depth; ++i)
        {
            Graphics.Blit(renderTex, arraySlice, i, 0);
            Save(arraySlice, filename + '_' + (i+1).ToString("D6"), outputExt, gammaCorrection, singleChannel);
        }
    }

    public void Save(RenderTexture renderTex, string filename, Extension outputExt, bool gammaCorrection, bool singleChannel = false)
    {
        if (renderTex == null)
            return;
        var oldRT = RenderTexture.active;


        if (gammaCorrection != renderTex.sRGB)
        {
            // Use blit to convert between linear and SRGB format
            if (gammaCorrection) { 
                Graphics.Blit(renderTex, renderTexSRGB);
                RenderTexture.active = renderTexSRGB;
            }
            else { 
                Graphics.Blit(renderTex, renderTexLin);
                RenderTexture.active = renderTexLin;
            }
        }
        else
        {
            RenderTexture.active = renderTex;
        }

        Texture2D saveTextureReference = singleChannel ? saveSingleChannelTexture : saveTexture;
        if(outputExt == Extension.exr)
            saveTextureReference = singleChannel ? saveSingleChannelTextureFloat : saveTextureFloat;
        saveTextureReference.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        saveTextureReference.Apply();
        RenderTexture.active = oldRT;

        byte[] bytes = null;
        switch (outputExt)
        {
            case Extension.png:
                bytes = saveTextureReference.EncodeToPNG();
                break;
            case Extension.jpg:
                bytes = ImageConversion.EncodeToJPG(saveTextureReference, 100);
                break;
            case Extension.exr:
                bytes = ImageConversion.EncodeToEXR(saveTextureReference, Texture2D.EXRFlags.OutputAsFloat);
                break;
        }
        File.WriteAllBytes(filename + "." + outputExt.ToString().Substring(0,3), bytes);
    }
}