using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialRandomizeHandler : RandomizerInterface
{
    private List<MaterialTextures> materialTextureTable = new List<MaterialTextures>();
    private List<GameObject> subjectInstances;
    public MaterialRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private MaterialRandomizerInterface[] linkedMaterialRandomizers;

    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }
    public void Awake()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Material;
        LinkGui();
        linkedMaterialRandomizers = GetComponentsInChildren<MaterialRandomizerInterface>();
    }

    public void initialize(ref List<GameObject> instantiatedModels)
    {
        if(instantiatedModels != null)
            subjectInstances = instantiatedModels;
        else
        {
            subjectInstances = new List<GameObject>();
            subjectInstances.Add(this.gameObject);
        }
    }

    public MaterialTextures getTextures(int index)
    {
        if (index < materialTextureTable.Count)
            return materialTextureTable[index];
        else
            return null;
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        if (subjectInstances == null)
            initialize(ref subjectInstances);
        int index = 0;

        foreach (GameObject instance in subjectInstances)
        {
            foreach (MaterialRandomizerInterface randomizer in linkedMaterialRandomizers)
                if(randomizer.isActiveAndEnabled)
                    randomizer.RandomizeSingleInstance(instance, ref rng, bopSceneIterator);
            if (instance != this.gameObject)
                foreach (MaterialRandomizerInterface randomizer in instance.GetComponents<MaterialRandomizerInterface>())
                    if (randomizer.isActiveAndEnabled)
                        randomizer.RandomizeSingleInstance(instance, ref rng, bopSceneIterator);

            foreach (Renderer rend in instance.GetComponentsInChildren<Renderer>())
            {

                if (instance != rend.gameObject)
                    foreach (MaterialRandomizerInterface randomizer in rend.gameObject.GetComponents<MaterialRandomizerInterface>())
                        if (randomizer.isActiveAndEnabled)
                            randomizer.RandomizeSingleInstance(rend.gameObject, ref rng, bopSceneIterator);

                for (int materialIndex = 0; materialIndex < rend.materials.Length; ++materialIndex)
                {
                    if (index < materialTextureTable.Count)
                        materialTextureTable[index].UpdateLinkedRenderer(rend, materialIndex);
                    else
                        materialTextureTable.Add(new MaterialTextures(dataset.generatedTextureResolution, rend, materialIndex));
                    
                    foreach (MaterialRandomizerInterface randomizer in linkedMaterialRandomizers)
                        if (randomizer.isActiveAndEnabled)
                            randomizer.RandomizeSingleMaterial(materialTextureTable[index], ref rng, bopSceneIterator);
                    if (instance != this)
                        foreach (MaterialRandomizerInterface randomizer in rend.gameObject.GetComponentsInParent<MaterialRandomizerInterface>())
                            if (randomizer.isActiveAndEnabled)
                                randomizer.RandomizeSingleMaterial(materialTextureTable[index], ref rng, bopSceneIterator);
                    materialTextureTable[index].linkpropertyBlock();
                    ++index;
                }
            }
        }
        resetFrameAccumulation();
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }

    [System.Obsolete]
    public override List<GameObject> getExportObjects()
    {
        if (subjectInstances != null)
            return subjectInstances;
        else
            return new List<GameObject>();
    }
}