using System;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFManDemolition
    {
        public enum FragmentParentType
        {
            Manager = 0,
            Parent  = 1
        }

        public FragmentParentType parent;
        public int                maximumAmount = 1000;
        public int                badMeshTry    = 3;
        public float              sizeThreshold = 0.05f;
        public int                currentAmount;
        
        // TODO Inherit velocity by impact normal
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Change current amount value
        public void ChangeCurrentAmount (int am)
        {
            // Add/subtract
            currentAmount += am;

            // Warning
            if (currentAmount >= maximumAmount)
                AmountWarning();
        }

        public void AmountWarning()
        {
            Debug.Log ("RayFire Man: Maximum fragments amount reached. Increase Maximum Amount property in Rayfire Man / Advanced Properties.");
        }

        public void ResetCurrentAmount()
        {
            currentAmount = 0;
        }
    }
}