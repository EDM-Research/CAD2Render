using UnityEngine;



[AddComponentMenu("Cad2Render/MaterialRandomizers/Rotation")]
public class RotationHandler : MaterialRandomizerInterface
{
    Quaternion previousRotation;
    private void Start()
    {
        previousRotation = Quaternion.Euler(0, 0, 0);
    }

    public RotationData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }


    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        Quaternion nextRoation = Quaternion.Euler(rng.Angle(dataset.rotation_X.x, dataset.rotation_X.y), 
                                                    rng.Angle(dataset.rotation_Y.x, dataset.rotation_Y.y),
                                                    rng.Angle(dataset.rotation_Z.x, dataset.rotation_Z.y));
        
        this.gameObject.transform.rotation *= Quaternion.Inverse(previousRotation) * nextRoation;
        previousRotation = nextRoation;
    }
}
