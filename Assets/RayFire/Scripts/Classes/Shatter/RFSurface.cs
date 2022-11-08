using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [System.Serializable]
    public class RFSurface
    {
        public Material innerMaterial;
        public float    mappingScale;
        public Material outerMaterial;
        public bool needNewMat;
        
        // static public Material[] newMaterials;
                    
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
         
        // Constructor
        public RFSurface()
        {
            innerMaterial = null;
            mappingScale = 0.1f;
            needNewMat = false;
            outerMaterial = null;
        }

        // Copy from
        public void CopyFrom(RFSurface interior)
        {
            innerMaterial = interior.innerMaterial;
            mappingScale = interior.mappingScale;
            needNewMat = interior.needNewMat;
            outerMaterial = interior.outerMaterial;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        // Set material to fragment by it's interior properties and parent material
        public static void SetMaterial(List<RFDictionary> origSubMeshIdsRF, Material[] sharedMaterials, RFSurface interior, MeshRenderer targetRend, int i, int amount)
        {
            if (origSubMeshIdsRF != null && origSubMeshIdsRF.Count == amount)
            {
                Material[] newMaterials = new Material[origSubMeshIdsRF[i].values.Count];
                
                //System.Array.Clear (newMaterials, );
                //newMaterials.
                //newMaterials = null;
                //newMaterials = new Material[origSubMeshIdsRF[i].values.Count];
                
                for (int j = 0; j < origSubMeshIdsRF[i].values.Count; j++)
                {
                    int matId = origSubMeshIdsRF[i].values[j];
                    if (matId < sharedMaterials.Length)
                    {
                        if (interior.outerMaterial == null)
                            newMaterials[j] = sharedMaterials[matId];
                        else
                            newMaterials[j] = interior.outerMaterial;
                    }
                    else
                        newMaterials[j] = interior.innerMaterial;
                }
                
                targetRend.sharedMaterials = newMaterials;
                //newMaterials               = null;
            }
        }

        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireRigid scr)
        {
            // No inner material
            if (scr.materials.innerMaterial == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skinnedMeshRend != null 
                ? scr.skinnedMeshRend.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.materials.innerMaterial)
                    return i;
            
            return -1;
        }
        
        // Get inner faces sub mesh id
        public static int SetInnerSubId(RayfireShatter scr)
        {
            // No inner material
            if (scr.material.innerMaterial == null) 
                return 0;
            
            // Get materials
            Material[] mats = scr.skinnedMeshRend != null 
                ? scr.skinnedMeshRend.sharedMaterials 
                : scr.meshRenderer.sharedMaterials;
            
            // Get outer id if outer already has it
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == scr.material.innerMaterial)
                    return i;
            
            return -1;
        }
    }
}

