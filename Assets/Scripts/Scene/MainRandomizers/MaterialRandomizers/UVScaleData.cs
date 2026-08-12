
using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New UV Scale Data")]
public class UVScaleData: ScriptableObject
{
    public bool syncScales = true;

    [Tooltip("UV Scale of the X direction. (This scale is used when scales are synced)")]
    [MinMaxRange(0.1f, 20.0f, 1)]
    public Vector2 scaleX = new Vector2(0.5f, 2.0f);

    [Tooltip("UV Scale of the Y direction.")]
    [MinMaxRange(0.1f, 20.0f, 1)]
    public Vector2 scaleY = new Vector2(0.5f, 2.0f);


    [Tooltip("UV Offset in the X direction.")]
    [MinMaxRange(-5.0f, 5.0f, 1)]
    public Vector2 offsetX = new Vector2(-0.5f, 0.5f);
    [Tooltip("UV Offset in the Y direction.")]
    [MinMaxRange(-5.0f, 5.0f, 1)]
    public Vector2 offsetY = new Vector2(-0.5f, 0.5f);
}
