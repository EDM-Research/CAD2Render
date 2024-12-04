using UnityEngine;



[AddComponentMenu("Cad2Render/MaterialRandomizers/Scale")]
public class ScaleHandler : MaterialRandomizerInterface
{
    Quaternion previousRotation;
    private void Start()
    {
    }

    public override void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        instance.transform.localScale *= rng.Next() * 0.4f + 0.8f;
    }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {

    }
}
