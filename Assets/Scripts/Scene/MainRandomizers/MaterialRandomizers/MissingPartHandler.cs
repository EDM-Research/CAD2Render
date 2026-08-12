using System.ComponentModel.Composition;
using UnityEngine;
using UnityEngine.Assertions.Must;



[AddComponentMenu("Cad2Render/MaterialRandomizers/MissingPartHandler")]
public class MissingPartHandler : MaterialRandomizerInterface
{

    public MissingPartData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if(textures.rend.gameObject.GetComponent<FalseColor>() != null)
            textures.rend.gameObject.GetComponent<FalseColor>().falseColor = ColorEncoding.GetColorByIndex(textures.falseColor.objectId);

        if (rng.Next() < dataset.missingChance)
        {
            textures.rend.gameObject.layer = LayerMask.NameToLayer("DefectMissingPart");
            if (textures.rend.gameObject != textures.falseColor.gameObject)
            {
                //if this is a childobject, create a new false color component with the same properties as the parent so the defect flag in the falsecolor is not propagated to other siblings
                var previousFalseColor = textures.falseColor;
                textures.falseColor = textures.rend.gameObject.AddComponent<FalseColor>();
                textures.falseColor.objectId = previousFalseColor.objectId;
            }
            textures.falseColor.falseColor = Color.cyan;//previousFalseColor.falseColor;
            //textures.falseColor.falseColor.a = 0;
            if (textures.get(MaterialTextures.MapTypes.defectMap) != null)
                textures.set(MaterialTextures.MapTypes.defectMap, null, textures.falseColor.falseColor);
        }
        else if (textures.rend.gameObject.layer == LayerMask.NameToLayer("DefectMissingPart"))
        {
            textures.rend.gameObject.layer = LayerMask.NameToLayer("Default");
        }

    }
}
