using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("Used by the old scene, Use the new scene instead")]
public class GuiButtonLinker : MonoBehaviour
{
    void Start()
    {
        randomize generator = GameObject.Find("SynthethicGenerator")?.GetComponent<randomize>();
        if (!generator)
        {
            Debug.LogError("Failed to link buttons: SyntheticGenerator not found");
            return;
        }

        LinkButton("RandomizeEnvironment", generator.RandomizeEnvironment);
        LinkButton("RandomizeView", generator.RandomizeView);
        LinkButton("RandomizeMaterial", generator.RandomizeMaterials);
        LinkButton("RandomizeObjects", generator.RandomizeModels);
        LinkButton("RandomizeTable", generator.RandomizeTable);
        LinkButton("FullRandom", generator.FullRandomize);
        
        LinkButton("SaveObjectColors", generator.SaveObjectColors);
        LinkButton("SaveMitsuba", generator.SaveMitsuba);
        
        var capturingGameObject = GameObject.Find("Capturing");
        Button recordButton = capturingGameObject?.transform.Find("Capturing_Button")?.GetComponent<Button>();
        if (recordButton)
            recordButton.onClick.AddListener(generator.ToggleRecording);
        else
            Debug.LogError("Record button not found");
    }

    private void LinkButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        Button currentButton = transform.Find(buttonName)?.gameObject.GetComponent<Button>();
        if (currentButton)
            currentButton.onClick.AddListener(action);
        else
            Debug.LogError("Failed to link button: " + buttonName);
    }
}
