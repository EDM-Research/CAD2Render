using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(MaterialTextureData))]
public abstract class MaterialRandomizerInterface : MonoBehaviour
{
    //this methode is called on every material for every renderer of the game object and its children.
    public abstract void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null);
    //used to determine the order of the material randomizers (higher == first executed)
    public virtual int getPriority() { return 0; }

    /*** 
     * This methode is called once on each gameobject by the material randomizer
     * The instance parameter will be either the gameObject that is inside the subjectList of the  material randomizer or the gameObject that is linked to this randomizer
     * This methode is called before the RandomizeSingleMaterial methode
     */
    public virtual void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null) { return; }
    public virtual ScriptableObject getDataset() { return null; }
}
