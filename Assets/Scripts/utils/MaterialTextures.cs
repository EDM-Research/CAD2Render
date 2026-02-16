using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class MaterialTextures
{
    private Dictionary<MapTypes, RenderTexture> textures = new Dictionary<MapTypes, RenderTexture>();
    private Dictionary<MapTypes, bool> textureDisabled = new Dictionary<MapTypes, bool>();
    private static RenderTexture resampleLocations;
    public Renderer rend { get; private set; }
    public int materialIndex { get; private set; }
    public FalseColor falseColor { get; set; }

    public MaterialPropertyBlock newProperties { get; private set; } = new MaterialPropertyBlock();

    public Vector2Int _resolution;
    public Vector2Int resolution { get => _resolution;}

    ~MaterialTextures()
    {
        foreach (var texture in textures)
            texture.Value.Release();
        if (resampleLocations != null)
            resampleLocations.Release();
        resampleLocations = null;
    }

    public MaterialTextures(Vector2Int resolution, Renderer rend, int materialIndex)
    {
        _resolution = new Vector2Int();
        _resolution.x = Math.Max(0, resolution.x);
        _resolution.y = Math.Max(0, resolution.y);
        UpdateLinkedRenderer(rend, materialIndex);
    }
    public void UpdateLinkedRenderer(Renderer rend, int materialIndex)
    {
        foreach (var keyValue in this.textures)
        {
            if (keyValue.Value == null)
                continue;
            if (!textureDisabled.ContainsKey(keyValue.Key))
                textureDisabled.Add(keyValue.Key, true);
        }

        this.rend = rend;
        this.materialIndex = materialIndex;
        this.falseColor = null;

        rend.GetPropertyBlock(newProperties, materialIndex);
        newProperties.Clear();
    }

    public enum MapTypes
    {
        colorMap,
        maskMap,
        detailMap,
        normalMap,
        defectMap,
        layerMask,
    }

    public RenderTexture ensureExistence(MapTypes type, Color backupColor)
    {
        return this.set(type, this.GetCurrentLinkedTexture(type), backupColor);
    }
    public RenderTexture set(MapTypes type, Texture baseTexture, Color backupColor)
    {
        if (!textures.ContainsKey(type))
            textures.Add(type, null);
        if (textureDisabled.ContainsKey(type))
            textureDisabled.Remove(type);

        RenderTexture newTexture = textures[type];
        setTexture(baseTexture, backupColor, ref newTexture, type != MapTypes.colorMap);
        textures[type] = newTexture;
        return newTexture;
    }
    public RenderTexture get(MapTypes type)
    {
        if (textures.ContainsKey(type) && !textureDisabled.ContainsKey(type))
        {
            if (!textures[type].IsCreated())
                textures[type].Create();
            return textures[type];
        }
        else
            return null;
    }

    [Obsolete("Textures are now reused instead of released, use UpdateLinkedRenderer instead.")]
    internal void releaseTextures()
    {
        textureDisabled.Clear();
        foreach (var texture in textures)
            texture.Value.Release();
        if (resampleLocations != null)
            resampleLocations.Release();
        resampleLocations = null;
    }

    public RenderTexture getResamplelocations()
    {
        //return get(MapTypes.resampleLocationMap);
        if (resampleLocations == null || resampleLocations.width != this.resolution.x || resampleLocations.height != this.resolution.y)
        {
            if (resampleLocations != null)
                resampleLocations.Release();
            resampleLocations = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            resampleLocations.enableRandomWrite = true;
            resampleLocations.wrapMode = TextureWrapMode.Mirror;
            resampleLocations.Create();
        }
        return resampleLocations;
    }
    
    public string getTextureName(MapTypes type, bool bottomLayer = true)
    {
        if (rend.material.shader.name != "HDRP/LayeredLit")
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
                case MapTypes.defectMap:
                    return "_FalseColorTex";
                case MapTypes.layerMask:
                    return "_LayerMaskMap";
                default:
                    return "";
            }

        int layerIndex = this.GetCurrentLinkedInt("_LayerCount") - 1;
        if (bottomLayer)
            layerIndex = 0;

        switch (type)
        {
            case MapTypes.colorMap:
                return "_BaseColorMap" + layerIndex;
            case MapTypes.maskMap:
                return "_MaskMap" + layerIndex;
            case MapTypes.detailMap:
                return "_DetailMap" + layerIndex;
            case MapTypes.normalMap:
                return "_NormalMap" + layerIndex;
            case MapTypes.defectMap:
                return "_FalseColorTex";
            case MapTypes.layerMask:
                return "_LayerMaskMap";
            default:
                return "";
        }
    }

    public void linkpropertyBlock()
    {
        //texture assignment
        foreach (var keyValue in this.textures)
        {
            if (keyValue.Value == null || textureDisabled.ContainsKey(keyValue.Key))
                continue;
            string textureName = getTextureName(keyValue.Key, false);
            newProperties.SetTexture(textureName, keyValue.Value);
        }

        if (falseColor != null)
        {
            falseColor.falseColorTex = get(MapTypes.defectMap);
            Vector4 scaleOffsetVector;
            if (rend.material.shader.name == "HDRP/LayeredLit")
                scaleOffsetVector = GetCurrentLinkedVector($"_BaseColorMap{this.GetCurrentLinkedInt("_LayerCount") - 1}_ST");
            else
                scaleOffsetVector = GetCurrentLinkedVector("_BaseColorMap_ST");
            if (scaleOffsetVector == new Vector4(0, 0, 0, 0))
                scaleOffsetVector = new Vector4(1, 1, 0, 0);
            falseColor.scaleOffset = scaleOffsetVector;
            falseColor.ApplyFalseColorProperties(newProperties);
        }

        rend.SetPropertyBlock(newProperties, materialIndex);
    }

    private void setTexture(Texture source, Color backupColor, ref RenderTexture destination, bool liniearColorSpace = true)
    {
        if (destination == null)
        {
            if (resolution.x == 0)
                _resolution.x = (source != null ? source.width : 2048);
            if (resolution.y == 0)
                _resolution.y = (source != null ? source.height : 2048);

            if (liniearColorSpace)
                destination = new RenderTexture(resolution.x, resolution.y, 0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
            else
            {
                destination = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }
            destination.enableRandomWrite = true;
            destination.wrapMode = TextureWrapMode.Mirror;
            destination.autoGenerateMips = true;
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

    public Vector4 GetCurrentLinkedVector(string propertyName)
    {
        if (newProperties.HasVector(propertyName))
            return newProperties.GetVector(propertyName);

        else if (rend.materials[materialIndex].HasVector(propertyName))
            return rend.materials[materialIndex].GetVector(propertyName);

        Debug.LogWarning("Error occured while requesting a property of a material. Probably an unsuported material shader is used. <br>Shader: <b>" + rend.material.shader.name + "</b> has no attribute: <b>" + propertyName + "</b>");
        return new Vector4(0, 0, 0, 0);

    }
    public Color GetCurrentLinkedColor(string propertyName)
    {
        if (newProperties.HasColor(propertyName))
            return newProperties.GetColor(propertyName);

        else if (rend.materials[materialIndex].HasColor(propertyName))
            return rend.materials[materialIndex].GetColor(propertyName);

        Debug.LogWarning("Error occured while requesting a property of a material. Probably an unsuported material shader is used. <br>Shader: <b>" + rend.material.shader.name + "</b> has no attribute: <b>" + propertyName + "</b>");
        return new Color(0, 0, 0, 0);
    }

    public Texture GetCurrentLinkedTexture(MapTypes map)
    {
        if(get(map) != null)
            return get(map);
        return GetCurrentLinkedTexture(getTextureName(map), map == MapTypes.defectMap);
    }
    public Texture GetCurrentLinkedTexture(string propertyName, bool suppressWarning = false)
    {
        if (newProperties.HasTexture(propertyName))
            return newProperties.GetTexture(propertyName);

        else if (rend.materials[materialIndex].HasTexture(propertyName))
            return rend.materials[materialIndex].GetTexture(propertyName);
        if(!suppressWarning)
            Debug.LogWarning("Error occured while requesting a property of a material. Probably an unsuported material shader is used. <br>Shader: <b>" + rend.material.shader.name + "</b> has no attribute: <b>" + propertyName + "</b>");
        return null;
        
    }

    public float GetCurrentLinkedFloat(string propertyName)
    {
        if (newProperties.HasFloat(propertyName))
            return newProperties.GetFloat(propertyName);

        else if (rend.materials[materialIndex].HasFloat(propertyName))
            return rend.materials[materialIndex].GetFloat(propertyName);

        Debug.LogWarning("Error occured while requesting a property of a material. Probably an unsuported material shader is used. <br>Shader: <b>" + rend.material.shader.name + "</b> has no attribute: <b>" + propertyName + "</b>");
        return 0.0f;
        
    }

    public int GetCurrentLinkedInt(string propertyName)
    {
        if (newProperties.HasInt(propertyName))
            return newProperties.GetInt(propertyName);

        else if (rend.materials[materialIndex].HasInt(propertyName))
            return rend.materials[materialIndex].GetInt(propertyName);

        Debug.LogWarning("Error occured while requesting a property of a material. Probably an unsuported material shader is used. <br>Shader: <b>" + rend.material.shader.name + "</b> has no attribute: <b>" + propertyName + "</b>");
        return 0;

    }
}
