//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//[System.Serializable]
//public class Location {
//	public string ID;
//	public string path;
//	public string extension;
//	public string videoFile;
//}

//[HelpURL("Documentation/DatasetInformation.html")] // TODO
[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "HDRPSyntheticDataGenerator/New Material randomize Data", order = 2)]
public class MaterialRandomizeData : ScriptableObject {
[Header("Input/output paths")]
    [Tooltip("Overide all materials with a random material")]
    public bool applyRandomMaterials = true;
    [Tooltip("Path to materials (relative to Resources dir)")]
    public string materialsPath;
    [Tooltip("Path to material textures (relative to Resources dir)")]
    public string texturesPath;


    public enum FalseColorAssignmentType { globalIndex, localIndex, predefined, none };
    [Header("General Material settings")]
    [Tooltip("Select the false color assignment algoritme.")]
    public FalseColorAssignmentType FalseColorType = FalseColorAssignmentType.globalIndex;
    [Tooltip("Export the linked models.")]
    public bool export = true;
    [Tooltip("The resolution that generated texture wil have. when 0 it wil either default to 2048 or take the same resolution as the first input texture used for texture generation")]
    public Vector2Int generatedTextureResolution = new Vector2Int(0, 0);

    //public TextureDimension generatedTextureYResolution = 0;

    [Header("Simple texture variations settings")]
    [Tooltip("Enable random HSV offets. Allows for indivudual variations of H, S or V. Offset is the maximum change in HSV. The variation is chosen randomly between 0 and offset.")]
    public bool applyRandomHSVOffset = false;
    [Tooltip("Random Hue offset")]
    [Range(0.0f, 180.0f)]
    public float H_maxOffset = 0.0f;
    [Tooltip("Random Saturation offset")]
    [Range(0.0f, 50.0f)]
    public float S_maxOffset = 0.0f;
    [Tooltip("Random Value offset")]
    [Range(0.0f, 50.0f)]
    public float V_maxOffset = 0.0f;
    [Tooltip("Enable random changes in normal map scales, uv scales and uv tilings")]
    public bool applyRandomUV = false;
    [Tooltip("Enable random changes albedo texture for material")]
    public bool applyRandomAlbedoTexture = false;

    [Header("Detailmap generation settings")]
    [Tooltip("Enable random changes in the detail map")]
    public bool applyDetailMapVariations = false;
    [Range(0, 10)]
    public uint nrOfScratches = 3;
    [Range(1.0f, 10.0f)]
    public float scratchWidth = 5;
    [Range(1,8)]
    public uint nrAntiAliasSamples = 1;
    [Range(0, 10)]
    public uint nrOfDarkspots = 3;
    [Range(1.0f, 10.0f)]
    public float spotSize = 5;
    [Space(5)] // 5 pixels of spacing here
    [Range(0.0f, 2.0f)]
    public float detailAlbedoScale = 0.0f;
    [Range(0.0f, 2.0f)]
    public float detailNormalScale = 0.3f;
    [Range(0.0f, 2.0f)]
    public float detailSmoothnessScale = 0.0f;

    [Header("Rust generation settings")]
    [Tooltip("Add rust to the material")]
    public bool applyRust = false;
    [Range(0.0f, 1.0f)]
    [Tooltip("Amount of rust to be applied")]
    public float rustCoeficient = 0.39f;
    [Tooltip("determines the size of rust spots")]
    [Range(0.0151f, 0.1f)]
    public float rustMaskZoom = 0.009f;
    [Tooltip("determines the size of color variations in rust spots")]
    [Range(0.0021f, 0.49f)]
    public float rustPaternZoom = 0.39f;
    public Color rustColor1 = new Color(133.0f / 255, 60.0f / 255, 42.0f / 255, 1);
    public Color rustColor2 = new Color( 65.0f / 255, 33.0f / 255, 15.0f / 255, 1);
    [Tooltip("determines the detail in the rust mask and patern, comes with computational cost.")]
    [Range(1, 10)]
    public uint nrOfOctaves = 5;

    [Header("Polishing lines generation settings")]
    [Tooltip("Add polishing lines to the material")]
    public bool applyManufacturingLines = false;
    [Range(0.001f, 0.2f)]
    public float lineSpacing = 0.08f;
    [Tooltip("Red: polishing lines area, Green: line color diference, Blue: rust coeficient")]
    public Texture LineAndRustMask;

    [Header("Texture resampler settings (preview)")]
    public bool applyTextureResampling = false;
    public MaterialTextures.MapTypes[] resampleTextures = new MaterialTextures.MapTypes[0];
    [Range(1, 50)]
    public int nrResampleSamples = 9;
    [Range(5, 17)]
    public int nrResampleGenerations = 17;

    [Header("Override Model Material/Color")]
    [Tooltip("Material to subsitute.")]
    public Material varyingMaterial;
    [Tooltip("Substitutes material of model with one specific material.")]
    public bool overideVaryingMaterialProperties = false;
    [Tooltip("Substitutes color of model material with different random color.")]
    public bool overideVaryingMaterialColor = false;
    [Tooltip("Min color range of random color.")]
    public Color minColor;
    [Tooltip("Max color range of random color.")]
    public Color maxColor;

    public MaterialRandomizeData(DatasetInformation data)
    {
        materialsPath = data.materialsPath;
        texturesPath = data.texturesPath;
        if(data.materialVariatons == false)
            materialsPath = "";
        //public FalseColorAssignmentType FalseColorType = FalseColorAssignmentType.globalIndex;
        generatedTextureResolution = data.generatedTextureResolution;
        applyRandomHSVOffset = data.applyRandomHSVOffset;
        H_maxOffset = data.H_maxOffset;
        S_maxOffset = data.S_maxOffset;
        V_maxOffset = data.V_maxOffset;
        applyRandomUV = data.applyRandomUV;
        applyRandomAlbedoTexture = data.applyRandomAlbedoTexture;
        applyDetailMapVariations = data.applyDetailMapVariations;
        nrOfScratches = data.nrOfScratches;
        scratchWidth = data.scratchWidth;
        nrAntiAliasSamples = data.nrAntiAliasSamples;
        nrOfDarkspots = data.nrOfDarkspots;
        spotSize = data.spotSize;
        detailAlbedoScale = data.detailAlbedoScale;
        detailNormalScale = data.detailNormalScale;
        detailSmoothnessScale = data.detailSmoothnessScale;
        applyRust = data.applyRust;
        rustCoeficient = data.rustCoeficient;
        rustMaskZoom = data.rustMaskZoom;
        rustPaternZoom = data.rustPaternZoom;
        rustColor1 = data.rustColor1;
        rustColor2 = data.rustColor2;
        nrOfOctaves = data.nrOfOctaves;
        applyManufacturingLines = data.applyManufacturingLines;
        lineSpacing = data.lineSpacing;
        LineAndRustMask = data.LineAndRustMask;
        applyTextureResampling = data.applyTextureResampling;
        resampleTextures = data.resampleTextures;
        nrResampleSamples = data.nrResampleSamples;
        nrResampleGenerations = data.nrResampleGenerations;
        overideVaryingMaterialColor = data.overrideWithRandomMaterialColor;
        overideVaryingMaterialProperties = data.overrideWithRandomMaterial;
        varyingMaterial = data.varyingMaterial;
        minColor = data.minColor;
        maxColor = data.maxColor;
    }
}
