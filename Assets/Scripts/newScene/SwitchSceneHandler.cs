using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchSceneHandler : RandomizerInterface
{
    public SwitchSceneData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        SceneManager.LoadSceneAsync(dataset.scenePath);//make sure it is not a child of the main randomizer
    }
    private void Start()
    {
        if(dataset != null && dataset.scenePath != "")
            this.LinkGui("ViewRandomizerList");
    }
    public override ScriptableObject getDataset()
    {
        return dataset;
    }
}
