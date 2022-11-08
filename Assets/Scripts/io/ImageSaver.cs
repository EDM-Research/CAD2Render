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
    private Texture2D BlackWhiteSaveTexture;

    public ImageSaver(int width, int height)
    {
        renderTexSRGB = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        renderTexSRGB.Create();
        renderTexLin = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        renderTexLin.Create();

        arraySlice = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        arraySlice.Create();

        saveTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        BlackWhiteSaveTexture = new Texture2D(width, height, TextureFormat.R16, false);
    }

    public void SaveArray(RenderTexture renderTex, int depth, string filename, DatasetInformation.Extension outputExt, bool gammaCorrection, bool blackWhite = false)
    {
        for(int i = 0; i < depth; ++i)
        {
            Graphics.Blit(renderTex, arraySlice, i, 0);
            Save(arraySlice, filename + '_' + (i+1).ToString("D6"), outputExt, gammaCorrection, blackWhite);
        }
    }

    public void Save(RenderTexture renderTex, string filename, DatasetInformation.Extension outputExt, bool gammaCorrection, bool blackWhite = false)
    {
        var oldRT = RenderTexture.active;

        // Use blit to convert linear renderTex to SRGB format

        if (gammaCorrection)
        {
            Graphics.Blit(renderTex, renderTexSRGB);
            RenderTexture.active = renderTexSRGB;
        }
        else
        {
            Graphics.Blit(renderTex, renderTexLin);
            RenderTexture.active = renderTexLin;
        }

        Texture2D ActiveTexture = blackWhite ? BlackWhiteSaveTexture : saveTexture;

        ActiveTexture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        ActiveTexture.Apply();

        RenderTexture.active = oldRT;

        byte[] bytes = null;
        if (outputExt == DatasetInformation.Extension.png)
            bytes = ActiveTexture.EncodeToPNG();
        else if (outputExt == DatasetInformation.Extension.jpg)
            bytes = ImageConversion.EncodeToJPG(ActiveTexture, 100);
           
        
        File.WriteAllBytes(filename + "." + outputExt.ToString(), bytes);
    }
}