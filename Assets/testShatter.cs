using System.Collections.Generic;
using UnityEngine;

public class testShatter : ObjectRandomizeHandler
{
    public override ScriptableObject getDataset()
    {
        return null;
    }


    RayFire.RayfireShatter shatterer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public new void Start()
    {
        base.Start();
        shatterer = this.GetComponent<RayFire.RayfireShatter>();
    }

    public override void Randomize(ref RandomNumberGenerator rng, BOPDatasetExporter.SceneIterator bopSceneIterator = null)
    {
        base.Randomize(ref rng, bopSceneIterator);
        fragmentObjects();

        resetFrameAccumulation();
    }

    protected void fragmentObjects()
    {
        List<GameObject> shatteredModels = new List<GameObject>();
        foreach (var gobject in instantiatedModels)
        {
            var tag = gobject.tag;//for some reason the tag gets removed when adding the rayfire shatter component
            gobject.AddComponent<RayFire.RayfireShatter>(shatterer);
            var currentShatterer = gobject.GetComponent<RayFire.RayfireShatter>();

            currentShatterer.Fragment();
            currentShatterer.fragmentsLast[rng.IntRange(0, currentShatterer.fragmentsLast.Count - 1)].SetActive(false);
            shatteredModels.AddRange(currentShatterer.fragmentsLast);

            var originalRenderers = gobject.GetComponentsInChildren<Renderer>();
            foreach (var rend in originalRenderers)
            {
                rend.enabled = false;
            }

            FalseColor falseColor;
            gobject.TryGetComponent<FalseColor>(out falseColor);
            if (falseColor != null)
                currentShatterer.fragmentsLast[0].transform.parent.gameObject.AddComponent<FalseColor>(falseColor);
            currentShatterer.fragmentsLast[0].transform.parent.gameObject.tag = tag;//cant assign to gobject, cause for some reason it then applies the tag to all children

            currentShatterer.fragmentsLast[0].transform.parent.transform.parent = gobject.transform;
            foreach (var shatterFragment in currentShatterer.fragmentsLast)
            {
                if (falseColor != null)
                    shatterFragment.AddComponent<FalseColor>(falseColor);

                for(int rendIndex = 0; rendIndex < shatterFragment.GetComponentsInChildren<Renderer>().Length; rendIndex++ )
                {
                    var rend = shatterFragment.GetComponentsInChildren<Renderer>()[rendIndex];
                    var propertyBlock = new MaterialPropertyBlock();
                    for (int materialIndex = 0; materialIndex < rend.materials.Length-1; ++materialIndex)
                    {
                        originalRenderers[rendIndex].GetPropertyBlock(propertyBlock, materialIndex);//todo check for multiple renderer on same object
                        rend.SetPropertyBlock(propertyBlock, materialIndex);
                    }

                    if (falseColor == null)
                        continue;
                    var falseColorText = falseColor.falseColorTex;
                    falseColor.falseColorTex = null;
                    var falseColorColor = falseColor.falseColor;
                    falseColor.falseColor.a = 0.0f;

                    //rend.GetPropertyBlock(propertyBlock, rend.materials.Length - 1);
                    falseColor.ApplyFalseColorProperties(propertyBlock);
                    rend.SetPropertyBlock(propertyBlock, rend.materials.Length - 1);
                    falseColor.falseColorTex = falseColorText;
                    falseColor.falseColor = falseColorColor;

                }
            }
        }

    }
}
