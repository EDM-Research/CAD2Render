using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[AddComponentMenu("Cad2Render/MaterialRandomizers/Main Material Randomizer", 1)]
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

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        if (subjectInstances == null)
            initialize(ref subjectInstances);
        int index = 0;

        foreach (GameObject instance in subjectInstances)
        {
            MaterialRandomizerInterface[] combinedMaterialRandomizers;

            //Combine material randomizers linked to the instance and the randomizer (unless ransomizer and instance are the same)
            if (instance != this.gameObject)
                combinedMaterialRandomizers = linkedMaterialRandomizers.Concat(instance.GetComponentsInChildren<MaterialRandomizerInterface>()).ToArray();
            else
                combinedMaterialRandomizers = linkedMaterialRandomizers;

            foreach (MaterialRandomizerInterface randomizer in combinedMaterialRandomizers.OrderByDescending(o => o.getPriority())) { 
                if (randomizer.isActiveAndEnabled) {
                    if (randomizer.gameObject == this || randomizer.gameObject.transform.IsChildOf(this.transform))//should check for full desendants list.
                        randomizer.RandomizeSingleInstance(instance, ref rng);
                    else
                        randomizer.RandomizeSingleInstance(randomizer.gameObject, ref rng);
                }
            }
            foreach (Renderer rend in instance.GetComponentsInChildren<Renderer>())
            {
                for (int materialIndex = 0; materialIndex < rend.materials.Length; ++materialIndex)
                {
                    //Reuse the MaterialTextures objects to limit the amount of textures that need to be created and destroyed
                    if (index < materialTextureTable.Count)
                        materialTextureTable[index].UpdateLinkedRenderer(rend, materialIndex);
                    else
                        materialTextureTable.Add(new MaterialTextures(dataset.generatedTextureResolution, rend, materialIndex));

                    //Combine material randomizers linked to the instance and the randomizer (unless ransomizer and instance are the same)
                    if (instance != this.gameObject)
                        combinedMaterialRandomizers = linkedMaterialRandomizers.Concat(instance.GetComponentsInParent<MaterialRandomizerInterface>()).ToArray();
                    else
                        combinedMaterialRandomizers = rend.gameObject.GetComponentsInParent<MaterialRandomizerInterface>();//linkedMaterialRandomizers contain all randomizers from the children but only parent randomizers need to be used

                    //Run all RandomizeSingleMaterial functions
                    foreach (MaterialRandomizerInterface randomizer in combinedMaterialRandomizers.OrderByDescending(o => o.getPriority()))
                        if (randomizer.isActiveAndEnabled)
                            randomizer.RandomizeSingleMaterial(materialTextureTable[index], ref rng);

                    //Submit the changes done by the randomizers to the GPU
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