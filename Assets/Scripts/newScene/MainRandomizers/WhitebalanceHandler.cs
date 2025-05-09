using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WhitebalanceHandler : RandomizerInterface
{
    public override ScriptableObject getDataset()
    {
        return null;
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if(wb == null)
            MainRandomizer.postProcesingSettings.GetComponent<Volume>().profile.TryGet<WhiteBalance>(out wb);// postProcesingSettings can be not initialized in start
        if (wb != null)
        {
            wb.temperature.value = rng.Next() * 140 - 70;
            wb.tint.value = rng.Next() * 140 - 70;
        }
    }

    WhiteBalance wb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Light;
        LinkGui();

    }
    

}
