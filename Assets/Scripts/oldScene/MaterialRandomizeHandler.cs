using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Used by the old scene, Use the new scene instead")]
public class MaterialRandomizeHandler
{
    private DatasetInformation dataset;
    private RandomNumberGenerator rng;
    private TextureResampler texResampler;

    private UnityEngine.Object[] albedoTextures;

    private ComputeShader detailmapGenerationShader;
    private ComputeShader rustmapGenerationShader;
    private ComputeShader LineTextureGenerationShader;

    private RenderTexture scaledImperfectionMask;
    private int materialTextureIndex;
    private int materialIndex;
    List<MaterialTextures> materialTextureList;

    Color resetVaryingMatColor;

    public MaterialRandomizeHandler(DatasetInformation p_dataset)
    {
        dataset = p_dataset;
        rng = new RandomNumberGenerator(dataset.seed + 1);//todo: sync rng to the rng of randomize script?
        texResampler = new TextureResampler(dataset);

        detailmapGenerationShader = (ComputeShader) Resources.Load("ComputeShaders/DetailmapGenerator");
        rustmapGenerationShader = (ComputeShader) Resources.Load("ComputeShaders/rustMapGenerator");
        LineTextureGenerationShader = (ComputeShader) Resources.Load("ComputeShaders/LineTextureGenerator");

        if (dataset.applyRandomAlbedoTexture)
            albedoTextures = Resources.LoadAll(dataset.texturesPath, typeof(Texture2D));

        materialTextureList = new List<MaterialTextures>();
    }

    public void RandomizeMaterialObject(UnityEngine.Object[] materials)
    {

        // random select material to override material
        if (dataset.overrideWithRandomMaterial)
        {
            if (materials.Length > 0)
            {
                Material mat = (Material)materials[rng.IntRange(0, materials.Length)];

                dataset.varyingMaterial.shader = mat.shader;
                dataset.varyingMaterial.CopyPropertiesFromMaterial(mat); // override material with settings of other material
            }
            else
                Debug.LogError("Overwriting varying material but no materials are loaded. check the material path in the dataset file.");
        }

        if (dataset.overrideWithRandomMaterialColor)
        {
            // generate a random color based on min and max hsv values
            float H_min, S_min, V_min;
            Color.RGBToHSV(dataset.minColor, out H_min, out S_min, out V_min);
            float H_max, S_max, V_max;
            Color.RGBToHSV(dataset.maxColor, out H_max, out S_max, out V_max);



            Color randomColor = Color.HSVToRGB(rng.Range(H_min, H_max), rng.Range(S_min, S_max), rng.Range(V_min, V_max));

            dataset.varyingMaterial.SetColor("_ColorTint", randomColor);
            dataset.varyingMaterial.SetColor("_Color", randomColor);
            dataset.varyingMaterial.SetColor("_PaintColor", randomColor);
            dataset.varyingMaterial.SetColor("_BaseColor", randomColor);
        }
    }


    public void RandomizeMaterials(List<GameObject> instantiatedModels, UnityEngine.Object[] materials)
    {
        materialTextureIndex = 0;

        foreach (GameObject model in instantiatedModels)
        {
            Renderer rend = model.GetComponent<Renderer>();
            if (rend != null)
                RandomizeMaterials(rend, materials);

            foreach (Transform child in model.transform)
            {
                Renderer rendChild = child.gameObject.GetComponent<Renderer>();
                if (rendChild != null)
                    RandomizeMaterials(rendChild, materials);
            }
        }
    }
    private void RandomizeMaterials(Renderer rend, UnityEngine.Object[] materials)
    {

        for (materialIndex = 0; materialIndex < rend.materials.Length; ++materialIndex)
        {
            if (materials.Length > 0)
            {
                Material mat = (Material)materials[rng.IntRange(0, materials.Length)];
                rend.materials[materialIndex] = mat;
            }
            ApplyMaterialVariations(rend);
            materialTextureIndex++;
        }
    }

    private void ApplyMaterialVariations(Renderer renderer, bool clearoldPropertyBlock = true)
    {
        MaterialPropertyBlock newProperties = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(newProperties, materialIndex);
        MaterialTextures textures = getCurrentMaterialTextures();

        if (clearoldPropertyBlock)
        {
            newProperties.Clear();
            FalseColor falseColor = renderer.gameObject.GetComponent<FalseColor>();
            if (falseColor)
            {
                falseColor.falseColorTex = null;
                falseColor.ApplyFalseColorProperties(newProperties);
            }
        }


        if (dataset.applyRandomAlbedoTexture)
        {
            Texture2D albedoMap = (Texture2D)albedoTextures[rng.IntRange(0, albedoTextures.Length)];
            newProperties.SetTexture("_BaseColorMap", albedoMap);
        }
        if (dataset.applyTextureResampling)
        {
            ApplyTextureResampling(renderer, newProperties);
        }

        if (dataset.applyManufacturingLines)
        {
            ApplyManufacturingLines(renderer, newProperties);
        }
        if (dataset.applyRandomHSVOffset)
        {
            ApplyHSVDOffsets(renderer, newProperties, new Vector3(dataset.H_maxOffset, dataset.S_maxOffset, dataset.V_maxOffset));
        }


        if (dataset.applyRandomUV)
        {
            //vector4(scale.x, scale.y, offset.x, offset.y)
            newProperties.SetVector("_NormalMap_ST", new Vector4(rng.Range(0.1f, 3.0f), rng.Range(0.1f, 3.0f), rng.Range(0.0f, 1.0f), rng.Range(0.0f, 1.0f)));
            newProperties.SetVector("_MaskMap_ST", new Vector4(rng.Range(0.1f, 3.0f), rng.Range(0.1f, 3.0f), rng.Range(0.0f, 1.0f), rng.Range(0.0f, 1.0f)));
            newProperties.SetVector("_BaseColorMap_ST", new Vector4(rng.Range(0.1f, 3.0f), rng.Range(0.1f, 3.0f), rng.Range(0.0f, 1.0f), rng.Range(0.0f, 1.0f)));
            newProperties.SetVector("_DetailMap_ST", new Vector4(rng.Range(0.9f, 1.2f), rng.Range(1.1f, 1.2f), rng.Range(0.0f, 1.0f), rng.Range(0.0f, 1.0f)));
        }

        if (dataset.applyDetailMapVariations)
        {
            CreateDetailmap();
            renderer.materials[materialIndex].EnableKeyword("_DETAILMAP");
            textures.linkTexture(newProperties, MaterialTextures.MapTypes.detailMap);
            newProperties.SetVector("_DetailMap_ST", new Vector4(rng.Range(0.9f, 1.2f), rng.Range(1.1f, 1.2f), rng.Range(0.0f, 1.0f), rng.Range(0.0f, 1.0f)));
            newProperties.SetFloat("_DetailAlbedoScale", dataset.detailAlbedoScale);
            newProperties.SetFloat("_DetailNormalScale", dataset.detailNormalScale);
            newProperties.SetFloat("_DetailSmoothnessScale", dataset.detailSmoothnessScale);
        }

        if (dataset.applyRust)
        {
            ApplyRust(renderer, newProperties);
        }

        renderer.SetPropertyBlock(newProperties, materialIndex);
    }

    public void ApplyTextureResampling(Renderer renderer, MaterialPropertyBlock newProperties)
    {
        MaterialTextures textures = getCurrentMaterialTextures();
        bool first = true;
        foreach (MaterialTextures.MapTypes type in dataset.resampleTextures)
        {
            if (first)
                texResampler.ResampleTexture(textures, GetTexture(renderer, textures.getTextureName(type), newProperties), type, ref rng);
            else
                texResampler.applyPreviousResample(textures, type);
            first = false;
            textures.linkTexture(newProperties, type);
        }

    }

    public void ApplyHSVDOffsets(Renderer renderer, MaterialPropertyBlock newProperties, Color minColor, Color maxColor)
    {

        // generate a random color based on min and max hsv values
        float H_min, S_min, V_min;
        Color.RGBToHSV(minColor, out H_min, out S_min, out V_min);
        float H_max, S_max, V_max;
        Color.RGBToHSV(maxColor, out H_max, out S_max, out V_max);

        newProperties.SetColor("_Color", Color.HSVToRGB((H_min + H_max) / 2, (S_min + S_max) / 2, (V_min + V_max) / 2));
        ApplyHSVDOffsets(renderer, newProperties, new Vector3((H_max - H_min) / 2, (S_max - S_min) / 2, (V_max - V_min) / 2) );
    }

    public void ApplyHSVDOffsets(Renderer renderer, MaterialPropertyBlock newProperties, Vector3 HSV_maxOffset)
    {
        Color color = GetColor(renderer, "_Color", newProperties);

        // generate a random color based on min and max hsv values
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);

        H = H * 360.0f + rng.Range(-HSV_maxOffset.x, +HSV_maxOffset.x);
        S = S * 100.0f + rng.Range(-HSV_maxOffset.y, +HSV_maxOffset.y);
        V = V * 100.0f + rng.Range(-HSV_maxOffset.z, +HSV_maxOffset.z);

        if (H < 0.0f)
            H = H + 360.0f;
        if (H >= 360.0f)
            H = H - 360.0f;
        S = Mathf.Min(S, 100.0f);
        S = Mathf.Max(S, 0.0f);
        V = Mathf.Min(V, 100.0f);
        V = Mathf.Max(V, 0.0f);

        Color randomColor = Color.HSVToRGB(H / 360.0f, S / 100.0f, V / 100.0f);

        newProperties.SetColor("_ColorTint", randomColor);
        newProperties.SetColor("_Color", randomColor);
        newProperties.SetColor("_PaintColor", randomColor);
        newProperties.SetColor("_BaseColor", randomColor);
    }

    private void updateImperfectionMaskScale()
    {
        MaterialTextures textures = getCurrentMaterialTextures();

        if (scaledImperfectionMask == null || scaledImperfectionMask.width != textures.resolutionX || scaledImperfectionMask.height != textures.resolutionY)
        {
            scaledImperfectionMask = new RenderTexture(textures.resolutionX, textures.resolutionY, 0);
            //((RenderTexture)scaledLineMask).enableRandomWrite = true;
            scaledImperfectionMask.Create();
            //Graphics.Blit(dataset.LineMask, scaledLineMask, new Vector2(((float) textures.colorMap.width) / dataset.LineMask.width , ((float) textures.colorMap.height) / dataset.LineMask.height ), new Vector2(0, 0));
            if(dataset.LineAndRustMask == null)
            {
                RenderTexture rt = RenderTexture.active;
                RenderTexture.active = scaledImperfectionMask;
                GL.Clear(true, true, new Color(0,1,1)); //red = line creation zones, green = color variation, blue = rust zones
                RenderTexture.active = rt;
            }
            else
                Graphics.Blit(dataset.LineAndRustMask, scaledImperfectionMask);
        }
    }

    public void ApplyManufacturingLines(Renderer renderer, MaterialPropertyBlock newProperties)
    {
        MaterialTextures textures = getCurrentMaterialTextures();

        textures.set(MaterialTextures.MapTypes.colorMap, GetTexture(renderer, "_BaseColorMap", newProperties), GetColor(renderer, "_Color", newProperties));

        updateImperfectionMaskScale();

        addLinesToTexture(textures.get(MaterialTextures.MapTypes.colorMap), scaledImperfectionMask);
        textures.linkTexture(newProperties, MaterialTextures.MapTypes.colorMap);
    }

    public void ApplyRust(Renderer renderer, MaterialPropertyBlock newProperties)
    {
        FalseColor falseColor = renderer.gameObject.GetComponent<FalseColor>();

        int kernelHandle = rustmapGenerationShader.FindKernel("CSMain");
        rustmapGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        rustmapGenerationShader.SetInt("applyRust", 1);

        MaterialTextures textures = getCurrentMaterialTextures();

        textures.set(MaterialTextures.MapTypes.maskMap, GetTexture(renderer, "_MaskMap", newProperties), new Color(1,1,0,1));//todo get backup smoothnes and metalicness from material
        rustmapGenerationShader.SetTexture(kernelHandle, "MaskMapInOut", textures.get(MaterialTextures.MapTypes.maskMap));

        textures.set(MaterialTextures.MapTypes.colorMap, GetTexture(renderer, "_BaseColorMap", newProperties), GetColor(renderer, "_Color", newProperties));
        rustmapGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", textures.get(MaterialTextures.MapTypes.colorMap));

        textures.set(MaterialTextures.MapTypes.defectMap, falseColor.falseColorTex, falseColor.falseColor);
        rustmapGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", textures.get(MaterialTextures.MapTypes.defectMap));

        var normalMap = GetTexture(renderer, "_NormalMap", newProperties);
        textures.set(MaterialTextures.MapTypes.normalMap, normalMap, new Color(0.5f, 0.5f, 1.0f));
        rustmapGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", textures.get(MaterialTextures.MapTypes.normalMap));
        if (normalMap != null)
            rustmapGenerationShader.SetInt("useNormalMapInput", 1);
        else
            rustmapGenerationShader.SetInt("useNormalMapInput", 0);

        updateImperfectionMaskScale();
        rustmapGenerationShader.SetTexture(kernelHandle, "rustMask", scaledImperfectionMask);

        
        rustmapGenerationShader.SetVector("colorRust1", dataset.rustColor1);
        rustmapGenerationShader.SetVector("colorRust2", dataset.rustColor2);
        rustmapGenerationShader.SetFloat("maskZoom", dataset.rustMaskZoom / textures.resolutionX * 100);
        rustmapGenerationShader.SetFloat("rustPaternZoom", dataset.rustPaternZoom / textures.resolutionY * 100);
        rustmapGenerationShader.SetFloat("rustCo", dataset.rustCoeficient);
        rustmapGenerationShader.SetInt("nrOfOctaves", (int)dataset.nrOfOctaves);

        //execute shader
        rustmapGenerationShader.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);

        //set new calculated values
        textures.linkTexture(newProperties, MaterialTextures.MapTypes.colorMap);
        textures.linkTexture(newProperties, MaterialTextures.MapTypes.normalMap);
        textures.linkTexture(newProperties, MaterialTextures.MapTypes.maskMap);
        if(normalMap != null)
            textures.linkTexture(newProperties, MaterialTextures.MapTypes.normalMap);

        Vector4 scaleOffset = GetVector(renderer, "_BaseColorMap_ST", newProperties);
        if (scaleOffset.x == 0)
            scaleOffset.x = 1.0f;
        if (scaleOffset.y == 0)
            scaleOffset.y = 1.0f;
        if (dataset.applyRandomUV)
        {
            scaleOffset.x *= rng.Range(0.91f, 1.10f);
            scaleOffset.y *= rng.Range(0.91f, 1.10f);
            scaleOffset.z += rng.Range(0.0f, 1.0f);
            scaleOffset.w += rng.Range(0.0f, 1.0f);
            newProperties.SetVector("_BaseColorMap_ST", scaleOffset);
            newProperties.SetVector("_MaskMap_ST", scaleOffset);
            newProperties.SetVector("_NormalMap_ST", scaleOffset);
        }

        if (falseColor)
        {
            falseColor.falseColorTex = textures.get(MaterialTextures.MapTypes.defectMap);
            falseColor.scaleOffset = scaleOffset;
            falseColor.ApplyFalseColorProperties(newProperties);
        }

    }

    struct lineSegment { public Vector2 a; public Vector2 b; };
    private RenderTexture CreateDetailmap()
    {
        MaterialTextures textures = getCurrentMaterialTextures();
        textures.set(MaterialTextures.MapTypes.detailMap, null, new Color(0, 0, 0));

        int kernelHandle = detailmapGenerationShader.FindKernel("CSMain");
        detailmapGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        detailmapGenerationShader.SetInt("nrAASamples", (int)dataset.nrAntiAliasSamples);
        detailmapGenerationShader.SetTexture(kernelHandle, "Result", textures.get(MaterialTextures.MapTypes.detailMap));

        Vector2[] spotData = new Vector2[dataset.nrOfDarkspots];
        for (int i = 0; i < spotData.Length; ++i)
        {
            spotData[i] = new Vector2(rng.Next() * textures.resolutionX, rng.Next() * textures.resolutionY);
        }
        ComputeBuffer spotBuffer = new ComputeBuffer(spotData.Length, sizeof(float) * 2);
        spotBuffer.SetData(spotData);
        detailmapGenerationShader.SetBuffer(kernelHandle, "spotLocations", spotBuffer);
        detailmapGenerationShader.SetFloat("spotSize", dataset.spotSize);


        lineSegment[] scratchData = new lineSegment[dataset.nrOfScratches];
        for (int i = 0; i < scratchData.Length; ++i)
        {
            scratchData[i] = new lineSegment
            {
                a = new Vector2(rng.Next() * textures.resolutionX, rng.Next() * textures.resolutionY),
                b = new Vector2(rng.Next() * textures.resolutionX, rng.Next() * textures.resolutionY)
            };
        }
        ComputeBuffer scratchBuffer = new ComputeBuffer(scratchData.Length, sizeof(float) * 4);
        scratchBuffer.SetData(scratchData);
        detailmapGenerationShader.SetBuffer(kernelHandle, "scratchLines", scratchBuffer);
        detailmapGenerationShader.SetFloat("scratchWidth", dataset.scratchWidth);


        detailmapGenerationShader.Dispatch(kernelHandle, textures.resolutionX / 8, textures.resolutionY / 8, 1);
        scratchBuffer.Dispose();
        spotBuffer.Dispose();
        return textures.get(MaterialTextures.MapTypes.detailMap);
    }

    private RenderTexture addLinesToTexture(RenderTexture lineTexture, Texture LineMask)
    {
        int texSizeX = lineTexture.width;
        int texSizeY = lineTexture.height;

        int kernelHandle = LineTextureGenerationShader.FindKernel("CSMain");
        LineTextureGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        LineTextureGenerationShader.SetFloat("lineSpacing", dataset.lineSpacing);
        LineTextureGenerationShader.SetTexture(kernelHandle, "Result", lineTexture);
        LineTextureGenerationShader.SetTexture(kernelHandle, "parameterTexture", LineMask);

        LineTextureGenerationShader.Dispatch(kernelHandle, texSizeX / 8, texSizeY / 8, 1);
        return lineTexture;
    }

    private MaterialTextures getCurrentMaterialTextures()
    {
        while (materialTextureIndex >= materialTextureList.Count)
            materialTextureList.Add(new MaterialTextures(dataset.generatedTextureResolution));
        return materialTextureList[materialTextureIndex];
    }

    public Vector4 GetVector(Renderer renderer, string propertyName, MaterialPropertyBlock overwritenProperties = null)
    {
        if (overwritenProperties == null)
            renderer.GetPropertyBlock(overwritenProperties, materialIndex);

        var property = overwritenProperties.GetVector(propertyName);
        if (property == new Vector4(0, 0, 0, 0))
            return renderer.materials[materialIndex].GetVector(propertyName);
        return property;
    }
    public Color GetColor(Renderer renderer, string propertyName, MaterialPropertyBlock overwritenProperties = null)
    {
        if( overwritenProperties == null)
            renderer.GetPropertyBlock(overwritenProperties, materialIndex);

        var property = overwritenProperties.GetColor(propertyName);
        if (property == new Color(0, 0, 0, 0))
            return renderer.materials[materialIndex].GetColor(propertyName);
        return property;
    }
    public Texture GetTexture(Renderer renderer, string propertyName, MaterialPropertyBlock overwritenProperties = null)
    {
        try
        {
            if (overwritenProperties == null)
                renderer.GetPropertyBlock(overwritenProperties, materialIndex);

            var property = overwritenProperties.GetTexture(propertyName);
            if (property == null)
                return renderer.materials[materialIndex].GetTexture(propertyName);
            return property;
        }
        catch{ return null;}
    }
}
