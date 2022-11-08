using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFReferenceDemolition
    {
        public enum ActionType
        {
            Instantiate    = 0,
            SetActive       = 1
        }

        public GameObject       reference;
        public List<GameObject> randomList;
        public ActionType       action;
        public bool             addRigid;
        public bool             inheritScale;
        public bool             inheritMaterials;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RFReferenceDemolition()
        {
            reference        = null;
            addRigid         = true;
            inheritScale     = true;
            inheritMaterials = false;
        }

        // Copy from
        public void CopyFrom (RFReferenceDemolition referenceDemolitionDml)
        {
            reference    = referenceDemolitionDml.reference;
            if (referenceDemolitionDml.randomList != null && referenceDemolitionDml.randomList.Count > 0)
            {
                if (randomList == null)
                    randomList = new List<GameObject>();
                randomList = referenceDemolitionDml.randomList;
            }
            addRigid         = referenceDemolitionDml.addRigid;
            inheritScale     = referenceDemolitionDml.inheritScale;
            inheritMaterials = referenceDemolitionDml.inheritMaterials;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////   
        
        // Get reference
        public GameObject GetReference()
        {
            // Return reference if action type is SetActive
            if (action == ActionType.SetActive)
            {
                // Reference not defined or destroyed
                if (reference == null)
                    return null;
                
                // Reference is prefab asset
                if (reference.scene.rootCount == 0)
                    return null;
                
                return reference;
            }

            // Return single ref
            if (reference != null && randomList.Count == 0)
                return reference;
            
            // Get random ref
            List<GameObject> refs = new List<GameObject>();
            if (randomList.Count > 0)
            {
                for (int i = 0; i < randomList.Count; i++)
                    if (randomList[i] != null)
                        refs.Add (randomList[i]);
                if (refs.Count > 0)
                    return refs[Random.Range (0, refs.Count)];
            }

            return null;
        }
        
        // Demolish object to reference
        public static bool DemolishReference (RayfireRigid scr)
        {
            if (scr.demolitionType == DemolitionType.ReferenceDemolition)
            {
                // Demolished
                scr.limitations.demolished = true;
                
                // Turn off original
                scr.gameObject.SetActive (false);
                
                // Get reference
                GameObject refGo = scr.referenceDemolition.GetReference();

                // Has no reference
                if (refGo == null)
                    return true;

                // Check if reference has already initialized Rigid
                RayfireRigid refScr = refGo.gameObject.GetComponent<RayfireRigid>();
                if (refScr != null && refScr.initialized == true)
                {
                    Debug.Log (RFLimitations.rigidStr + scr.name + "Reference object has already initialized Rigid. Set By Method Initialization type or Deactivate reference.", scr.gameObject);
                    return true;
                }
                
                // Set object to swap
                GameObject instGo = GetInstance (scr, refGo);

                // Set root to manager or to the same parent
                RayfireMan.SetParentByManager (instGo.transform, scr.transForm);
                
                // Set tm
                scr.rootChild = instGo.transform;
                
                // Copy scale
                if (scr.referenceDemolition.inheritScale == true)
                    scr.rootChild.localScale = scr.transForm.localScale;

                // Inherit materials
                InheritMaterials (scr, instGo);

                // Clear list for fragments
                scr.fragments = new List<RayfireRigid>();
                
                // Check root for rigid props
                RayfireRigid instScr = instGo.gameObject.GetComponent<RayfireRigid>();

                // Reference Root has not rigid. Add to
                if (instScr == null && scr.referenceDemolition.addRigid == true)
                {
                    // Add rigid and copy
                    instScr = instGo.gameObject.AddComponent<RayfireRigid>();

                    // Copy rigid
                    scr.CopyPropertiesTo (instScr);

                    // Copy particles from demolished rigid to instanced rigid
                    RFParticles.CopyRigidParticles (scr, instScr);   
                    
                    // Single mesh TODO improve
                    if (instGo.transform.childCount == 0)
                    {
                        instScr.objectType = ObjectType.Mesh;
                    }

                    // Multiple meshes
                    if (instGo.transform.childCount > 0)
                    {
                        instScr.objectType = ObjectType.MeshRoot;
                    }
                }

                // Activate and init rigid
                instGo.transform.gameObject.SetActive (true);

                // Reference has rigid
                if (instScr != null)
                {
                    // Init if not initialized yet
                    instScr.Initialize();
                    
                    // Create rigid for root children
                    if (instScr.objectType == ObjectType.MeshRoot)
                    {
                        // Increment demolition depth
                        for (int i = 0; i < instScr.fragments.Count; i++)
                            instScr.fragments[i].limitations.currentDepth++;
                        
                        // Collect referenced fragments
                        scr.fragments.AddRange (instScr.fragments);
                        
                        // Destroy mesh root rigid TODO ??? why
                        scr.DestroyRigid (instScr);
                        
                        // Copy fragments debris to demolished rigid debris
                        for (int i = 0; i < scr.debrisList.Count; i++)
                        {
                            scr.debrisList[i].children.Clear();
                            scr.debrisList[i].children.AddRange(instScr.debrisList[i].children);
                        }
                        
                        // Copy fragments dust to demolished rigid debris
                        for (int i = 0; i < scr.dustList.Count; i++)
                        {
                            scr.dustList[i].children.Clear();
                            scr.dustList[i].children.AddRange(instScr.dustList[i].children);
                        }
                    }

                    // Get ref rigid
                    else if (instScr.objectType == ObjectType.Mesh || instScr.objectType == ObjectType.SkinnedMesh)
                    {
                        // Disable runtime caching
                        instScr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                        
                        // Instance has no meshes
                        if (instScr.meshFilter == null && instScr.skinnedMeshRend == null)
                            return true;

                        // Demolish mesh instance
                        RFDemolitionMesh.DemolishMesh(instScr);

                        // TODO COPY MESH DATA FROM ROOTSCR TO THIS TO REUSE
                        
                        // Collect fragments
                        if (instScr.HasFragments == true)
                            scr.fragments.AddRange (instScr.fragments);
                        
                        // Destroy instance
                        RayfireMan.DestroyFragment (instScr, instScr.rootParent, 1f);
                    }

                    // Get ref rigid
                    else if (instScr.objectType == ObjectType.NestedCluster || instScr.objectType == ObjectType.ConnectedCluster)
                    {
                        instScr.Default();
                        
                        // Copy contact data
                        instScr.limitations.contactPoint   = scr.limitations.contactPoint;
                        instScr.limitations.contactVector3 = scr.limitations.contactVector3;
                        instScr.limitations.contactNormal  = scr.limitations.contactNormal;
                        
                        // Demolish
                        RFDemolitionCluster.DemolishCluster (instScr);
                        
                        // Collect new fragments
                        scr.fragments.AddRange (instScr.fragments);
                        
                        // Collect demolished cluster
                        if (instScr.clusterDemolition.cluster.shards.Count > 0)
                            scr.fragments.Add (instScr);
                    }
                }

                else
                {
                    Rigidbody rb = instGo.GetComponent<Rigidbody>();
                    if (rb != null && scr.physics.rigidBody != null)
                    {
                        rb.velocity        = scr.physics.rigidBody.velocity;
                        rb.angularVelocity = scr.physics.rigidBody.angularVelocity;
                    }
                }
            }

            return true;
        }

        // Get final instance accordingly to action type
        static GameObject GetInstance (RayfireRigid scr, GameObject refGo)
        {
            GameObject instGo;
            
            // Instantiate turned off reference with null parent
            if (scr.referenceDemolition.action == ActionType.Instantiate)
            {
                instGo = Object.Instantiate (refGo, scr.transForm.position, scr.transForm.rotation);
                instGo.name = refGo.name;
            }
                
            // Set active
            else
            {
                instGo = refGo;
                instGo.transform.position = scr.transform.position;
                instGo.transform.rotation = scr.transform.rotation;
            }
            return instGo;
        }

        // Inherit materials from original object to referenced fragments
        static void InheritMaterials (RayfireRigid scr, GameObject instGo)
        {
            if (scr.referenceDemolition.inheritMaterials == true)
            {
                Renderer[] renderers = instGo.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        int min = Math.Min (scr.meshRenderer.materials.Length, renderers[r].materials.Length);
                        for (int m = 0; m < min; m++)
                            renderers[r].materials[m] = scr.meshRenderer.materials[m];
                    }
            }
        }
    }
}