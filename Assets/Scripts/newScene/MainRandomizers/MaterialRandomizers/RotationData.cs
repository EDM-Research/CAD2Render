
using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/New Rotation randomize Data")]
public class RotationData: ScriptableObject
{
    [Tooltip("max random degree of rotations around x axis")]
    [MinMaxRange(-360, 360, 1)]
    public Vector2 rotation_X = new Vector2(0.0f, 360.0f);

    [Tooltip("max random degree of rotations around y axis")]
    [MinMaxRange(-360, 360, 1)]
    public Vector2 rotation_Y = new Vector2(0.0f, 360.0f);

    [Tooltip("max random degree of rotations around z axis")]
    [MinMaxRange(-360, 360, 1)]
    public Vector2 rotation_Z = new Vector2(0.0f, 360.0f);
}
