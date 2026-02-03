
using SneakySquirrelLabs.MinMaxRangeAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Untitled Dataset", menuName = "Cad2Render/Material randomizer Data/New Missing part randomize Data")]
public class MissingPartData: ScriptableObject
{
    [Tooltip("Chance the part is missing.")]
    [Range(0, 1)]
    public float missingChance = 0.5f;
}
