using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[RequireComponent(typeof(MaterialTextureData))]
public abstract class MaterialRandomizerInterface : MonoBehaviour
{
    //this methode is called on every material for every renderer of the game object and its children.
    public abstract void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null);

    /*** 
     * This methode is called once on each gameobject by the material randomizer
     * This methode is called before the RandomizeSingleMaterial methode
     */
    public virtual void RandomizeSingleInstance(GameObject instance, ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null) { return; }
    public virtual ScriptableObject getDataset() { return null; }
}
