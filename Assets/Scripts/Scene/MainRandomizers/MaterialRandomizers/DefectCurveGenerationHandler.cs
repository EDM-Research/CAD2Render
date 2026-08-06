using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using MyResourceManager = Assets.Scripts.io.MyResourceManager;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Defect Line generation")]
public class DefectCurveGenerationHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    public RustGenerationData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;

    private RenderTexture RustZoneTexture;
    private ComputeShader defectCurveGenerationShader;
    private LocalKeyword changeMaskmap;
    private LocalKeyword changeNormalMap;
    private LocalKeyword changeColor;

    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public void Awake()
    {
        defectCurveGenerationShader = MyResourceManager.loadComputeShader("DefectCurveGenerator");
        changeNormalMap = new LocalKeyword(defectCurveGenerationShader, "changeNormalMap");
        changeMaskmap = new LocalKeyword(defectCurveGenerationShader, "changeMaskMap");
        changeColor = new LocalKeyword(defectCurveGenerationShader, "changeColor");
    }


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        int kernelHandle = defectCurveGenerationShader.FindKernel("CSMain");
        defectCurveGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));
        defectCurveGenerationShader.SetFloat("sharpness", dataset.sharpness);
        
        if (dataset.changeColor)
        {
            defectCurveGenerationShader.EnableKeyword(changeColor);
            var colorMap = textures.ensureExistence(MaterialTextures.MapTypes.colorMap, textures.GetCurrentLinkedColor("_Color"));
            defectCurveGenerationShader.SetTexture(kernelHandle, "ColorMapInOut", colorMap);
            defectCurveGenerationShader.SetVector("defectColor1", dataset.rustColor1);
            defectCurveGenerationShader.SetVector("defectColor2", dataset.rustColor2);
        }
        else
            defectCurveGenerationShader.DisableKeyword(changeColor);

        if (dataset.changeMaskMap)
        {
            defectCurveGenerationShader.EnableKeyword(changeMaskmap);

            var maskMap = textures.ensureExistence(MaterialTextures.MapTypes.maskMap, new Color(textures.GetCurrentLinkedFloat("_Metallic"), 1, 0,
                                                                                                textures.GetCurrentLinkedFloat("_Smoothness")));
            defectCurveGenerationShader.SetTexture(kernelHandle, "MaskMapInOut", maskMap);
            defectCurveGenerationShader.SetFloat("metalicnessOffset", dataset.metalicnessOffset);
        }
        else
            defectCurveGenerationShader.DisableKeyword(changeMaskmap);

        if (dataset.changeNormalMap) { 
            defectCurveGenerationShader.EnableKeyword(changeNormalMap);
            var normalMap = textures.ensureExistence(MaterialTextures.MapTypes.normalMap, new Color(0.5f, 0.5f, 1.0f, 1.0f));
            defectCurveGenerationShader.SetTexture(kernelHandle, "NormalMapInOut", normalMap);
            defectCurveGenerationShader.SetFloat("dentModifier", dataset.dentModifier);
        }
        else
            defectCurveGenerationShader.DisableKeyword(changeNormalMap);


        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        defectCurveGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", defectMap);

        defectCurveGenerationShader.SetFloat("defectWidthModifier", dataset.rustCoeficient.x);
        defectCurveGenerationShader.SetFloat("defectLength", dataset.rustCoeficient.y);
        defectCurveGenerationShader.SetFloat("sharpness", dataset.sharpness);
        defectCurveGenerationShader.SetFloat("defectCutoff", dataset.defectCutoff);
        defectCurveGenerationShader.SetInt("nrOfOctaves", (int)dataset.nrOfOctaves);

        //execute shader
        defectCurveGenerationShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);


    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}