//Copyright (c) 2020 Nick Michiels <nick.michiels@uhasselt.be>, Hasselt University, Belgium, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Renderer))]
public class FalseColor : MonoBehaviour
{
    public Color falseColor = new Color(0f, 1.0f, 0f, 1f);
    public Texture falseColorTex { get; set; } = null;
    public Vector4 scaleOffset { get; set; } = new Vector4(0, 0, 1, 1);
    public int objectId  = -1;

    // Start is called before the first frame update
    void Start()
    {
        SetColor();
    }
    
    void OnValidate()
    {
        SetColor();
    }
    
    void SetColor()
    {
        Renderer rndr;
        TryGetComponent<Renderer>(out rndr);

        if (rndr == null)
            return;
        var propertyBlock = new MaterialPropertyBlock();
        rndr.GetPropertyBlock(propertyBlock);
    
        ApplyFalseColorProperties(propertyBlock);
    
        rndr.SetPropertyBlock(propertyBlock);
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
