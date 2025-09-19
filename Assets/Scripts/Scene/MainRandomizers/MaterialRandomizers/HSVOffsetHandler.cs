using System.Collections;
using UnityEngine;


[AddComponentMenu("Cad2Render/MaterialRandomizers/HSV Offset")]
public class HSVOffsetHandler : MaterialRandomizerInterface
{
    //private RandomNumberGenerator rng;
    public HSVOffsetData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Color generateRandomcolor(ref RandomNumberGenerator rng, Color baseColor)
    {
        float H, S, V;
        Color.RGBToHSV(baseColor, out H, out S, out V);

        H = H * 360.0f + rng.Range(-dataset.H_maxOffset, +dataset.H_maxOffset);
        S = S * 100.0f + rng.Range(-dataset.S_maxOffset, +dataset.S_maxOffset);
        V = V * 100.0f + rng.Range(-dataset.V_maxOffset, +dataset.V_maxOffset);

        if (H < 0.0f)
            H = H + 360.0f;
        if (H >= 360.0f)
            H = H - 360.0f;
        S = Mathf.Min(S, 100.0f);
        S = Mathf.Max(S, 0.0f);
        V = Mathf.Min(V, 100.0f);
        V = Mathf.Max(V, 0.0f);

        return Color.HSVToRGB(H / 360.0f, S / 100.0f, V / 100.0f);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        bool gLTFshader = true;
        // glTF shaders
        Color initialColor = textures.GetCurrentLinkedColor("baseColorFactor");
        //HDRP lit shader
        if (initialColor == Color.clear)
        {
            initialColor = textures.GetCurrentLinkedColor("_BaseColor");
            gLTFshader = false;
        }
        // if(color == Color.clear)
        //     color = textures.GetCurrentLinkedColor("_Color");
        // if(color == Color.clear)
        //     color = textures.GetCurrentLinkedColor("_ColorTint");
        // if(color == Color.clear)
        //     color = textures.GetCurrentLinkedColor("_PaintColor");

        // generate a random color based on min and max hsv values
        Color randomColor = generateRandomcolor(ref rng, initialColor);

        if (gLTFshader)
            textures.newProperties.SetColor("baseColorFactor", randomColor);//glb shader
        else
            textures.newProperties.SetColor("_BaseColor", randomColor);//lit shader (recomended)

        // textures.newProperties.SetColor("_ColorTint", randomColor);
        // textures.newProperties.SetColor("_Color", randomColor);
        // textures.newProperties.SetColor("_PaintColor", randomColor);
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}