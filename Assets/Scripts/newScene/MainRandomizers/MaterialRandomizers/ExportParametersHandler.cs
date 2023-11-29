using System.Collections;
using UnityEngine;


public class ExportParametersHandler : MaterialRandomizerInterface
{

    public override int getPriority() { return 100; }

    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        var falseColor = instance.GetComponent<FalseColor>();
        if (falseColor == null)
            falseColor = instance.AddComponent<FalseColor>();

        falseColor.objectId = ColorEncoding.NextGlobalColorIndex();
        falseColor.falseColor = ColorEncoding.GetColorByIndex(falseColor.objectId);

        instance.tag = "ExportInstanceInfo";
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        var parent = textures.rend.gameObject.GetComponentInParent<FalseColor>();
        if(parent == null)
        {   //should not be happening since RandomizeSingleInstance is called first
            textures.falseColor = textures.rend.gameObject.AddComponent<FalseColor>();
            textures.falseColor.objectId = ColorEncoding.NextGlobalColorIndex();
            textures.falseColor.falseColor = ColorEncoding.GetColorByIndex(textures.falseColor.objectId);
            textures.rend.gameObject.tag = "ExportInstanceInfo";
        }
        else
        {
            textures.falseColor = parent;
        }
    }
}