using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTextures
{
    private Dictionary<MapTypes, RenderTexture> textures = new Dictionary<MapTypes, RenderTexture>();
    private static RenderTexture resampleLocations;

    public MaterialTextures(Vector2Int resolution)
    {
        resolutionX = Math.Max(0, resolution.x);
        resolutionY = Math.Max(0, resolution.y);
    }

    public void releaseTextures()
    {
        foreach (var texture in textures)
            texture.Value.Release();
        if(resampleLocations != null)
            resampleLocations.Release();
    }

    public enum MapTypes
    {
        colorMap,
        maskMap,
        detailMap,
        normalMap,
        defectMap,
        //resampleLocationMap// this is a static map to save ram memory.
    }
    public void set(MapTypes type, Texture baseTexture, Color backupColor)
    {
        if (!textures.ContainsKey(type))
            textures.Add(type, null);
        RenderTexture newTexture = textures[type];
        setTexture(baseTexture, backupColor, ref newTexture, type != MapTypes.colorMap);
        textures[type] = newTexture;
    }
    public RenderTexture get(MapTypes type)
    {
        if (textures.ContainsKey(type))
        {
            if (!textures[type].IsCreated())
                textures[type].Create();
            return textures[type];
        }
        else
            return null;
    }
    public RenderTexture getResamplelocations()
    {
        //return get(MapTypes.resampleLocationMap);
        if(resampleLocations == null || resampleLocations.width != this.resolutionX || resampleLocations.height != this.resolutionY)
        {
            if(resampleLocations != null)
                resampleLocations.Release();
            resampleLocations = new RenderTexture(resolutionX, resolutionY, 0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
            resampleLocations.enableRandomWrite = true;
            resampleLocations.wrapMode = TextureWrapMode.Mirror;
            resampleLocations.Create();
        }
        return resampleLocations;
    }

    public string getTextureName(MapTypes type)
    {
        switch (type)
        {
            case MapTypes.colorMap:
                return "_BaseColorMap";
            case MapTypes.maskMap:
                return "_MaskMap";
            case MapTypes.detailMap:
                return "_DetailMap";
            case MapTypes.normalMap:
                return "_NormalMap";
            default:
                return "";
        }
    }

    public bool linkTexture(MaterialPropertyBlock newProperties, MapTypes type)
    {
        string textureName = getTextureName(type);
        if (textureName == "" || !textures.ContainsKey(type))
            return false;

        newProperties.SetTexture(textureName, textures[type]);
        return true;
    }

    public int resolutionX { get; private set; }
    public int resolutionY { get; private set; }

    private void setTexture(Texture source, Color backupColor, ref RenderTexture destination, bool liniearColorSpace = true)
    {
        if (destination == null)
        {
            if (resolutionX == 0)
            {
                if (source != null)
                    resolutionX = source.width;
                else
                    resolutionX = 2048;
            }
            if (resolutionY == 0)
            {
                if (source != null)
                    resolutionY = source.height;
                else
                    resolutionY = 2048;
            }


            if (liniearColorSpace)
                destination = new RenderTexture(resolutionX, resolutionY, 0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
            else
                destination = new RenderTexture(resolutionX, resolutionY, 0, UnityEngine.Experimental.Rendering.DefaultFormat.HDR);
            destination.enableRandomWrite = true;
            destination.wrapMode = TextureWrapMode.Mirror;
            destination.Create();
        }

        if (source != null)
        {
            //Graphics.Blit(source, destination, new Vector2(((float)resolutionX) / source.width, ((float)resolutionY) / source.height), new Vector2(0, 0)); scale works difrent then expected.  source is automaticly scaled to output texture, and then the scale is applied
            Graphics.Blit(source, destination);
        }
        else
        {
            if (backupColor == null)
                backupColor = new Color(0, 0, 0);
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = destination;
            GL.Clear(true, true, backupColor);
            RenderTexture.active = rt;
        }
    }

}
