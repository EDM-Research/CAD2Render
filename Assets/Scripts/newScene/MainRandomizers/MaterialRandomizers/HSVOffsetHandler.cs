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

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        Color color = textures.GetCurrentLinkedColor("_Color");

        // generate a random color based on min and max hsv values
        float H, S, V, A;
        Color.RGBToHSV(color, out H, out S, out V);

        H = H * 360.0f + rng.Range(-dataset.H_maxOffset, +dataset.H_maxOffset);
        S = S * 100.0f + rng.Range(-dataset.S_maxOffset, +dataset.S_maxOffset);
        V = V * 100.0f + rng.Range(-dataset.V_maxOffset, +dataset.V_maxOffset);
        A = color.a * 100.0f + rng.Range(-dataset.A_maxOffset, +dataset.A_maxOffset);

        if (H < 0.0f)
            H = H + 360.0f;
        if (H >= 360.0f)
            H = H - 360.0f;
        S = Mathf.Min(S, 100.0f);
        S = Mathf.Max(S, 0.0f);
        V = Mathf.Min(V, 100.0f);
        V = Mathf.Max(V, 0.0f);

        Color randomColor = Color.HSVToRGB(H / 360.0f, S / 100.0f, V / 100.0f);
        randomColor.a = A/100.0f;

        textures.newProperties.SetColor("_ColorTint", randomColor);
        textures.newProperties.SetColor("_Color", randomColor);
        textures.newProperties.SetColor("_PaintColor", randomColor);
        textures.newProperties.SetColor("_BaseColor", randomColor);//lit shader (recomended)
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}