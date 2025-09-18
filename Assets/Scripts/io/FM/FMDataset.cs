using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.io.FM
{
    public class FMDataset
    {
        [Serializable]
        public struct SaveModel
        {

            public string name;
            public int instance;
            public float[] locToWorld;
            public float[] imgPos; // 2d image position
            public bool occluded;// waypoint occluded or not

        }
        [Serializable]
        public struct SaveObject
        {
            public int id;
            public float[] proj;
            public float[] worldToCam;
            public List<SaveModel> models;
        }


        [Serializable]
        public struct ModelColor
        {
            public ModelColor(string name, int r, int g, int b)
            {
                model = name;
                color = new int[3] { r, g, b };
            }

            public string model;
            public int[] color;
        }
        [Serializable]
        public struct ModelColors
        {
            public List<ModelColor> modelColors;
        }
    }
}