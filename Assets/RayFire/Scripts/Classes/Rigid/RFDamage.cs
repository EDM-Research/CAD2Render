using System;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFDamage
    {
        public bool  enable;
        public float maxDamage;
        public float currentDamage;
        public bool  collect;
        public float multiplier;



        public bool toShards = true;
        
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFDamage()
        {
            enable     = false;
            maxDamage  = 100f;
            collect    = false;
            multiplier = 1f;
                        
            Reset();
        }

        // Copy from
        public void CopyFrom(RFDamage damage)
        {
            enable     = damage.enable;
            maxDamage  = damage.maxDamage;
            collect    = damage.collect;
            multiplier = damage.multiplier;
            
            Reset();
        }
        
        // Reset
        public void Reset()
        {
            currentDamage = 0f;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////     
       
        // Add damage
        public static bool ApplyTo(RayfireRigid scr, float value, Vector3 point, float radius = 0f, Collider collider = null)
        {
            // Apply damage to connected cluster per shard level
            if (scr.objectType == ObjectType.ConnectedCluster && scr.damage.toShards == true)
                return ApplyToShard (scr, value, point, radius, collider);
            
            // Apply to rigid
            return ApplyToRigid (scr, value);
        }
        
        // Add damage to Rigid
        public static bool ApplyToRigid(RayfireRigid scr, float damageValue)
        {
            // Add damage
            scr.damage.currentDamage += damageValue;
            
            // Check
            if (scr.damage.enable == true && scr.damage.currentDamage >= scr.damage.maxDamage)
                return true;

            return false;
        }
        
        // Add damage to shard
        public static bool ApplyToShard(RayfireRigid scr, float value, Vector3 point, float radius, Collider collider)
        {
            bool hasDamagedShard = false;
            
            // Add damage by collider
            if (collider != null)
            {
                for (int i = 0; i < scr.clusterDemolition.cluster.shards.Count; i++)
                    if (scr.clusterDemolition.cluster.shards[i].col == collider)
                    {
                        // Apply damage to shard
                        scr.clusterDemolition.cluster.shards[i].dm += value;

                        // Flag damaged shard
                        if (scr.clusterDemolition.cluster.shards[i].dm > scr.damage.maxDamage)
                            hasDamagedShard = true;

                        // TODO add damage in radius?

                        break;
                    }
            }

            /*
            // Add damage by radius
            if (radius > 0)
            {
                impactColliders = Physics.OverlapSphere (point, radius, mask);
            }
            */
            
            return hasDamagedShard;
        }
        
        // Apply damage
        public static bool ApplyDamage (RayfireRigid scr, float value, Vector3 point, float radius, Collider collider)
        {
            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();
            
            // Already demolished or should be
            if (scr.limitations.demolished == true || scr.limitations.demolitionShould == true)
                return false;

            // Apply damage and get demolition state
            bool demolitionState = ApplyTo (scr, value, point, radius, collider);
            
            // TODO demolish first to activate only demolished fragments AND activate if object can't be demolished
            // TODO avoid demolition by radius in case of shard damage with radius
            
            // Set demolition info
            if (demolitionState == true)
            {
                // Demolition available check
                if (scr.DemolitionState() == false)
                    return false;
                
                // Set damage position
                scr.limitations.contactVector3     = point;
                scr.clusterDemolition.damageRadius = radius;

                // Set small radius for cluster demolition by gun with 0 radius. IMPORTANT
                if (radius == 0)
                    scr.clusterDemolition.damageRadius = 0.05f;

                // Demolish object
                scr.limitations.demolitionShould = true;

                // Debug.Log (scr.limitations.contactVector3);
                
                // Demolish
                scr.Demolish();

                // Mesh Was demolished
                if (scr.limitations.demolished == true)
                    return true;
                
                // Cluster was 
                if (scr.IsCluster == true)
                    if (scr.HasFragments == true)
                        return true;
                
            }
            
            // Check for activation
            if (scr.activation.byDamage > 0 && scr.damage.currentDamage > scr.activation.byDamage)
                scr.Activate();
            
            return false;
        }
    }
}