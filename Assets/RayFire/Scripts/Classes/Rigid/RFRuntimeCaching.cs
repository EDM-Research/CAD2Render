using System;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFRuntimeCaching
    {
        public CachingType type;
        [Range (2, 300)] public int frames;
        [Range (1, 20)]  public int fragments;
        public bool skipFirstDemolition;

        [HideInInspector] public bool inProgress;
        [HideInInspector] public bool wasUsed;
        [HideInInspector] public bool stop;
           
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFRuntimeCaching()
        {
            type = CachingType.Disable;
            frames = 3;
            fragments = 4;
            skipFirstDemolition = false;
            inProgress = false;
            wasUsed = false;
            stop = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Get batches amount for continuous fragmentation
        public static List<int> GetBatchByFrames (int frames, int amount)
        {
            // Get basic list
            int       div         = amount / frames;
            List<int> batchAmount = new List<int>();
            for (int i = 0; i < frames; i++)
                batchAmount.Add (div);

            // Consider difference
            int dif = amount % frames;
            if (dif > 0)
                for (int i = 0; i < dif; i++)
                    batchAmount[i] += 1;

            // Remove 0
            if (frames > amount)
                for (int i = batchAmount.Count - 1; i >= 0; i--)
                    if (batchAmount[i] == 0)
                        batchAmount.RemoveAt (i);

            return batchAmount;
        }
        
        // Get batches amount for continuous fragmentation
        public static List<int> GetBatchByFragments (int fragments, int amount)
        {
            // Get basic list
            int       steps         = amount / fragments;
            List<int> batchAmount = new List<int>();
            if (steps > 0)
                for (int i = 0; i < steps; i++)
                    batchAmount.Add (fragments);

            // Consider difference
            int dif = amount % fragments;
            if (dif > 0)
                batchAmount.Add (dif);
            
            return batchAmount;
        }

        // Get list of marked elements index
        public static List<int> GetMarkedElements (int batchInd, List<int> batchAmount)
        {
            // Get offset
            int offset = 0;
            if (batchInd > 0)
                for (int i = 0; i < batchInd; i++)
                    offset += batchAmount[i];
            
            // Collect marked elements ids
            List<int> markedElements = new List<int>();
            for (int i = 0; i < batchAmount[batchInd]; i++)
                markedElements.Add (i + offset);

            return markedElements;
        }

        // Create tm reference
        public static GameObject CreateTmRef(RayfireRigid rfScr)
        {
            GameObject go = new GameObject("RFTempGo");
            go.SetActive (false);
            go.transform.position = rfScr.transForm.position;
            go.transform.rotation = rfScr.transForm.rotation;
            go.transform.localScale = rfScr.transForm.localScale;
            go.transform.parent = RayfireMan.inst.transform;
            return go;
        }
    }
}