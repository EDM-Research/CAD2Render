
using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New Scale randomize Data")]
public class ScaleData: ScriptableObject
{

    [Tooltip("Keep Aspect Ratio when scaling. Only x scale is used when enabled.")]
    public bool keepAspectRatio = false;

    [Tooltip("min random scale factor")]
    public Vector3 minScale = new Vector3(0.5f, 0.5f, 0.5f);
    [Tooltip("max random scale factor")]
    public Vector3 maxScale = new Vector3(2.0f, 2.0f, 2.0f);
}
