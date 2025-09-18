using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.io.ExportDatasetInterface;

namespace Assets.Scripts.io.BOP
{
    public class BOPImportSettings : ScriptableObject
    {

        [Tooltip("Path to input dataset folder.")]
        public string inputPath = "../renderings/";

    }
}
