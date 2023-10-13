//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Renderer))]
public class FalseColor : MonoBehaviour
{
    public Color falseColor = new Color(0f, 1.0f, 0f, 1f);
    public Texture falseColorTex { get; set; } = null;
    public Vector4 scaleOffset { get; set; } = new Vector4(1, 1, 0, 0);
    public int objectId  = -1;

    [Obsolete("Use ApplyFalseColorProperties instead")]
    public void SetColor(Color newColor)
    {
        falseColor = newColor;

        Renderer rend;
        TryGetComponent<Renderer>(out rend);

        if (rend == null)
            return;

        var propertyBlock = new MaterialPropertyBlock();
        for (int materialIndex = 0; materialIndex < rend.materials.Length; ++materialIndex)
        {
            rend.GetPropertyBlock(propertyBlock, materialIndex);

            ApplyFalseColorProperties(propertyBlock);

            rend.SetPropertyBlock(propertyBlock, materialIndex);
        }
    }

    public void ApplyFalseColorProperties(MaterialPropertyBlock propertyBlock)
    {
        propertyBlock.SetColor("_FalseColor", falseColor);
        propertyBlock.SetInt("_objectId", objectId);

        if (falseColorTex == null)
            propertyBlock.SetFloat("_useFalseColorTex", -1.0f);
        else
        {
            propertyBlock.SetFloat("_useFalseColorTex", 1.0f);
            propertyBlock.SetTexture("_FalseColorTex", falseColorTex);
            propertyBlock.SetVector("_FalseColorTex_ST", scaleOffset);
        }
    }
}
