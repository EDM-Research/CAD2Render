using System.Data;
using UnityEngine;

public class RotationHandlerTemp : RandomizerInterface
{
    public RotationData dataset;
    public override ScriptableObject getDataset()
    {
        return dataset;
    }

    Quaternion previousRotation;
    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        Quaternion nextRoation = Quaternion.Euler(rng.Angle(dataset.rotation_X.x, dataset.rotation_X.y),
                                                    rng.Angle(dataset.rotation_Y.x, dataset.rotation_Y.y),
                                                    rng.Angle(dataset.rotation_Z.x, dataset.rotation_Z.y));
        this.gameObject.transform.rotation *= Quaternion.Inverse(previousRotation) * nextRoation;
        previousRotation = nextRoation;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Material;
        LinkGui();

        previousRotation = Quaternion.Euler(0, 0, 0);
    }
}
