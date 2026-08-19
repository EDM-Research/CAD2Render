using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using MyResourceManager = Assets.Scripts.io.MyResourceManager;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Defect Curve generation")]
public class DefectCurveGenerationHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    public CurveDefectGenerationData dataset;
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


        defectCurveGenerationShader.SetInt("randSeed", rng.IntRange(128, Int32.MaxValue));

        var defectMap = textures.ensureExistence(MaterialTextures.MapTypes.defectMap, textures.falseColor != null ? textures.falseColor.falseColor : Color.black);
        defectCurveGenerationShader.SetTexture(kernelHandle, "DefectMapInOut", defectMap);

        defectCurveGenerationShader.SetVector("defectWidth", new Vector2(dataset.defectWidth.x * 0.1f, dataset.defectWidth.y * 0.1f));
        defectCurveGenerationShader.SetVector("defectLength", new Vector2(dataset.defectLength.x, dataset.defectLength.y));
        defectCurveGenerationShader.SetVector("defectAngle", new Vector2(dataset.defectAngle.x, dataset.defectAngle.y));
        defectCurveGenerationShader.SetVector("defectControlPointOffset", new Vector2(dataset.controlPointOffset.x, dataset.controlPointOffset.y));
        defectCurveGenerationShader.SetFloat("sharpness1", dataset.sharpness1);
        defectCurveGenerationShader.SetFloat("sharpness2", dataset.sharpness2);
        defectCurveGenerationShader.SetFloat("defectCutoff", dataset.defectCutoff);

        //execute shader
        defectCurveGenerationShader.Dispatch(kernelHandle, textures.resolution.x / 8, textures.resolution.y / 8, 1);


    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}