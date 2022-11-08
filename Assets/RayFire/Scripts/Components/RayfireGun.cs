using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace RayFire
{
    [AddComponentMenu("RayFire/Rayfire Gun")]
    [HelpURL("https://rayfirestudios.com/unity-online-help/components/unity-gun-component/")]
    public class RayfireGun : MonoBehaviour
    {
        public enum ImpactType
        {
            AddExplosionForce  = 0,
            AddForceAtPosition = 1
        }
        
        public AxisType   axis;
        public float      maxDistance = 50f;
        public Transform  target;
        public int        rounds = 2;
        public float      rate   = 0.3f;
        public ImpactType type;
        public float      strength        = 1f;
        public float      radius          = 1f;
        public bool       demolishCluster = true;
        public bool       affectInactive  = true;
        public bool       rigid           = true;
        public bool       rigidRoot       = true;
        [FormerlySerializedAs ("affectRigidBodies")]
        public bool rigidBody = true;
        public float damage = 100f;
        public bool  debris = true;
        public bool  dust   = true;
        public bool  flash  = false;

        public RFFlash Flash = new RFFlash();
        
        public int    mask      = -1;
        public string tagFilter = "Untagged";
        public bool   showRay   = true;
        public bool   showHit   = true; 
        public bool shooting = false;

        static string untagged = "Untagged";

        // Event
        public RFShotEvent shotEvent = new RFShotEvent();


        Collider[] impactColliders;
        
        // Impact Sparks
        //[Header("Shotgun")]
        //public int pellets = 1;
        //public int spread = 2;
        //public float recoilStr = 1f;
        //public float recoilFade = 1f;
        // Projectile: laser, bullet, pellets
        // Muzzle flash: position, color, str
        // Shell drop: position, direction, prefab, str, rotation
        // Impact decals
        // Impact blood
        // Ricochet

        /// /////////////////////////////////////////////////////////
        /// Shooting main
        /// /////////////////////////////////////////////////////////

        // Start shooting
        public void StartShooting()
        {
            if (shooting == false)
            {
                StartCoroutine(StartShootCor());
            }
        }

        // Start shooting
        IEnumerator StartShootCor()
        {
            // Vars
            int shootId = 0;
            shooting = true;

            while (shooting == true)
            {
                // Single shot
                Shoot(shootId);
                shootId++;

                yield return new WaitForSeconds(rate);
            }
        }

        // Stop shooting
        public void StopShooting()
        {
            shooting = false;
        }

        // Shoot over axis
        public void Shoot(int shootId = 1)
        {
            // Set vector
            Vector3 shootVector = ShootVector;

            // Consider burst recoil // TODO
            if (shootId > 1)
                shootVector = ShootVector;

            // Set position
            Vector3 shootPosition = transform.position;

            // Shoot
            Shoot(shootPosition, shootVector);
        }
        
        // Shoot over axis
        public void Burst()
        {
            if (shooting == false)
                StartCoroutine(BurstCor());
        }

        // Burst shooting coroutine
        IEnumerator BurstCor()
        {
            shooting = true;
            for (int i = 0; i < rounds; i++)
            {
                // Stop shooting
                if (shooting == false)
                    break;

                // Single shot
                Shoot(i);

                yield return new WaitForSeconds(rate);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Shot Logic
        /// /////////////////////////////////////////////////////////
        
        // Shoot over axis
        public void Shoot (Vector3 shootPos, Vector3 shootVector)
        {
            // Event
            shotEvent.InvokeLocalEvent(this);
            RFShotEvent.InvokeGlobalEvent(this);
            
            // Get intersection collider
            RaycastHit hit;
            bool hitState = Physics.Raycast(shootPos, shootVector, out hit, maxDistance, mask, QueryTriggerInteraction.Ignore);
            
            // No hits
            if (hitState == false)
                return;

            // Check for tag
            if (tagFilter != untagged && CompareTag (hit.transform.tag) == false)
                return;
            
            // Pos and normal info
            Vector3 impactPoint  = hit.point;
            Vector3 impactNormal = hit.normal;
            
            // IMPORTANT. Fix for internal bounding box contact position. Point should be inside bounding box.
            impactPoint -= impactNormal.normalized * 0.001f;

            // Create impact flash
            VfxFlash (impactPoint, impactNormal);

            // Affected components
            RayfireRigid     rigidScr = null;
            RayfireRigidRoot rootScr  = null;
            Rigidbody        rbScr    = null;  
            
            // Affect Rigid component
            if (rigid == true)
            {
                // Get rigid from collider or rigid body
                rigidScr = hit.collider.attachedRigidbody == null 
                    ? hit.collider.GetComponent<RayfireRigid>() 
                    : hit.collider.attachedRigidbody.transform.GetComponent<RayfireRigid>();
                
                // Target is Rigid
                if (rigidScr != null)
                {
                    // Impact Debris and dust
                    VfxDebris (rigidScr.debrisList, impactPoint, impactNormal);
                    VfxDust (rigidScr.dustList, impactPoint, impactNormal);

                    // Apply damage and return new demolished rigid fragment over shooting line
                    rigidScr = ApplyDamage (rigidScr, hit, shootPos, shootVector, impactPoint);

                    // Impact hit to rigid bodies. Activated inactive, detach clusters
                    if (rigidScr != null)
                        ImpactRigid(rigidScr, hit, impactPoint, shootVector);
                }
            }
            
            // Affect Rigid Root component
            if (rigidRoot == true)
            {
                // Get rigid from collider or rigid body
                rootScr = hit.collider.GetComponentInParent<RayfireRigidRoot>();
                
                // Target is Rigid Root
                if (rootScr != null)
                {
                    // Impact Debris and dust
                    VfxDebris (rootScr.debrisList, impactPoint, impactNormal);
                    VfxDust (rootScr.dustList, impactPoint, impactNormal);
                    
                    // TODO Damage
                    
                    // Impact hit to rigid bodies. Activated inactive, detach clusters
                    ImpactRoot(rootScr, hit, impactPoint, shootVector);
                }
            }
            
            // Affect Rigid Body component
            if (rigidBody == true)
            {
                if (rigidScr == null && rootScr == null)
                {
                    rbScr = hit.collider.attachedRigidbody;
                    
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Impact
        /// /////////////////////////////////////////////////////////
        
        // Impact hit to rigid bodies. Activated inactive, detach clusters
        void ImpactRigid(RayfireRigid rigidScr, RaycastHit hit, Vector3 impactPoint, Vector3 shootVector)
        {
            // Prepare impact list
            List<Rigidbody> impactRbList = new List<Rigidbody>();
            
            // Hit object Impact activation and detach before impact force
            if (radius == 0)
            {
                // Inactive Activation
                if (rigidScr.objectType == ObjectType.Mesh)
                    if (rigidScr.simulationType == SimType.Inactive || rigidScr.simulationType == SimType.Kinematic)
                        if (rigidScr.activation.byImpact == true)
                            rigidScr.Activate();

                // Connected cluster one fragment detach
                if (rigidScr.objectType == ObjectType.ConnectedCluster && demolishCluster == true)
                    RFDemolitionCluster.DemolishConnectedCluster (rigidScr, new[] {hit.collider});

                // Collect for impact
                if (strength > 0)
                {
                    // Skip inactive objects
                    if (rigidScr.simulationType == SimType.Inactive && affectInactive == false)
                        return;
                    
                    impactRbList.Add (hit.collider.attachedRigidbody);
                }
            }
            
            // Group by radius Impact activation and detach before impact force
            if (radius > 0)
            {
                // Get all colliders
                impactColliders = null;
                impactColliders = Physics.OverlapSphere (impactPoint, radius, mask);
                
                // TODO tag filter
                if (tagFilter != untagged)
                {
                   //  && colliders[i].CompareTag (tagFilter) == false)
                }
                 
                // No colliders. Stop
                if (impactColliders == null) 
                    return;
                
                // Connected cluster group detach first, check for rigids in range next
                if (rigidScr.objectType == ObjectType.ConnectedCluster)
                    if (demolishCluster == true)
                        RFDemolitionCluster.DemolishConnectedCluster (rigidScr, impactColliders);
                
                // Collect all rigid bodies in range
                RayfireRigid scr;
                List<RayfireRigid> impactRigidList = new List<RayfireRigid>();
                for (int i = 0; i < impactColliders.Length; i++)
                {
                    // Get rigid from collider or rigid body
                    scr = impactColliders[i].attachedRigidbody == null 
                        ? impactColliders[i].GetComponent<RayfireRigid>() 
                        : impactColliders[i].attachedRigidbody.transform.GetComponent<RayfireRigid>();
                    
                    // Collect uniq rigids in radius
                    if (scr != null)
                    {
                        if (impactRigidList.Contains (scr) == false)
                            impactRigidList.Add (scr);
                    }
                    // Collect RigidBodies without rigid script
                    else 
                    {
                        if (strength > 0 && rigidBody == true)
                            if (impactColliders[i].attachedRigidbody == null)
                                if (impactRbList.Contains (impactColliders[i].attachedRigidbody) == false)
                                    impactRbList.Add (impactColliders[i].attachedRigidbody);
                    }
                }
                
                // Group Activation first
                for (int i = 0; i < impactRigidList.Count; i++)
                    if (impactRigidList[i].activation.byImpact == true)
                        if (impactRigidList[i].simulationType == SimType.Inactive || impactRigidList[i].simulationType == SimType.Kinematic)
                            impactRigidList[i].Activate();
                
                // Collect rigid body from rigid components
                if (strength > 0)
                {
                    for (int i = 0; i < impactRigidList.Count; i++)
                    {
                        // Skip inactive objects
                        if (impactRigidList[i].simulationType == SimType.Inactive && affectInactive == false)
                            continue;

                        // Collect
                        impactRbList.Add (impactRigidList[i].physics.rigidBody);
                    }
                }
            }

            // Add force to rigid bodies
            AddForce (impactRbList, impactPoint, shootVector);
        }
        
         // Impact hit to rigid bodies. Activated inactive, detach clusters
        void ImpactRoot(RayfireRigidRoot rootScr, RaycastHit hit, Vector3 impactPoint, Vector3 shootVector)
        {
            // Prepare impact list
            List<Rigidbody> impactRbList = new List<Rigidbody>();
            
            // Impact activation before impact force
            if (radius == 0)
            {
                // Get impact shard
                RFShard hitShard = RFShard.GetShardByCollider(rootScr.cluster.shards, hit.collider);
                if (hitShard == null)
                    return;;
                
                // Inactive Activation
                if (rootScr.simulationType == SimType.Inactive || rootScr.simulationType == SimType.Kinematic)
                    if (rootScr.activation.byImpact == true)
                        RFActivation.ActivateShard (hitShard, rootScr);
                
                // Collect for impact
                if (strength > 0)
                {
                    // Skip inactive objects
                    if (hitShard.sm == SimType.Inactive && affectInactive == false)
                        return;
                    
                    impactRbList.Add (hitShard.rb);
                }
            }
            
            // Group by radius Impact activation and detach before impact force
            if (radius > 0)
            {
                // Get all colliders
                impactColliders = null;
                impactColliders = Physics.OverlapSphere (impactPoint, radius, mask);
                
                // TODO tag filter
                if (tagFilter != untagged)
                {
                   //  && colliders[i].CompareTag (tagFilter) == false)
                }
                
                // No colliders. Stop
                if (impactColliders == null) 
                    return;

                // Get shards by colliders
                List<RFShard> shards = RFShard.GetShardsByColliders (rootScr.cluster.shards, impactColliders.ToList());

                // No shards among hit colliders TODO input shards list to avoid list creations
                if (shards.Count == 0)
                    return;

                // Group Activation first
                for (int i = 0; i < shards.Count; i++)
                    if (rootScr.activation.byImpact == true)
                        if (rootScr.simulationType == SimType.Inactive || rootScr.simulationType == SimType.Kinematic)
                            RFActivation.ActivateShard (shards[i], rootScr);
                
                // TODO avoid collction of rigidboides with severl colliders

                // Collect rigid body from rigid components
                if (strength > 0)
                {
                    for (int i = 0; i < shards.Count; i++)
                    {
                        // Skip inactive objects
                        if (shards[i].sm == SimType.Inactive && affectInactive == false)
                            continue;

                        // Collect
                        impactRbList.Add (shards[i].rb);
                    }
                }
            }

            // Add force to rigid bodies
            AddForce (impactRbList, impactPoint, shootVector);
        }
        
        // Add force to rigid bodies
        void AddForce(List<Rigidbody> impactRbList, Vector3 impactPoint, Vector3 shootVector)
        {
            // No rigid bodies
            if (impactRbList == null || impactRbList.Count == 0)
                return;
            
            // Apply force
            for (int i = 0; i < impactRbList.Count; i++)
            {
                // Skip static and kinematic objects
                if (impactRbList[i] == null || impactRbList[i].isKinematic == true)
                    continue;

                // Add force
                if (type == ImpactType.AddExplosionForce)
                    impactRbList[i].AddExplosionForce (strength, impactPoint, 0, 0, ForceMode.VelocityChange);
                else
                    impactRbList[i].AddForceAtPosition(shootVector * strength, impactPoint, ForceMode.VelocityChange);
                
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////
        
        // Apply damage. Return new rigid
        RayfireRigid ApplyDamage (RayfireRigid scr, RaycastHit hit, Vector3 shootPos, Vector3 shootVector, Vector3 impactPoint)
        {
            // No damage or damage disabled
            if (damage == 0 || scr.damage.enable == false)
                return scr;
            
            // Check for demolition TODO input collision collider if radius is 0
            bool damageDemolition = scr.ApplyDamage(damage, impactPoint, radius, hit.collider);
            
            // Object was not demolished
            if (damageDemolition == false)
                return scr;
            
            // Target was demolished
            if (scr.HasFragments == true)
            {
                // Get new fragment target
                bool dmlHitState = Physics.Raycast(shootPos, shootVector, out hit, maxDistance, mask, QueryTriggerInteraction.Ignore);
                
                // Get new hit rigid
                if (dmlHitState == true)
                {
                    if (hit.collider.attachedRigidbody != null)
                        return hit.collider.attachedRigidbody.transform.GetComponent<RayfireRigid>();
                    if (hit.collider != null)
                        return hit.collider.transform.GetComponent<RayfireRigid>();
                }
            }
            
            return null;
        }

        /// /////////////////////////////////////////////////////////
        /// Vfx
        /// /////////////////////////////////////////////////////////

        // Create impact flash
        void VfxFlash(Vector3 position, Vector3 normal)
        {
            if (flash == true)
            {
                // Get light position
                Vector3 lightPos = normal * Flash.distance + position;

                // Create light object
                GameObject impactFlashGo = new GameObject ("impactFlash");
                impactFlashGo.transform.position = lightPos;

                // Create light
                Light lightScr = impactFlashGo.AddComponent<Light>();
                lightScr.color     = Flash.color;
                lightScr.intensity = Random.Range (Flash.intensityMin, Flash.intensityMax);
                lightScr.range     = Random.Range (Flash.rangeMin,     Flash.rangeMax);

                lightScr.shadows = LightShadows.Hard;

                // Destroy with delay
                Destroy (impactFlashGo, 0.2f);
            }
        }

        // Impact Debris
        void VfxDebris(List<RayfireDebris> debrisList, Vector3 impactPos, Vector3 impactNormal)
        {
            if (debris == true && debrisList != null && debrisList.Count > 0)
                for (int i = 0; i < debrisList.Count; i++)
                    if (debrisList[i] != null)
                        if (debrisList[i].onImpact == true)
                            RFParticles.CreateDebrisImpact(debrisList[i], impactPos, impactNormal);
        }

        // Impact Dust
        void VfxDust(List<RayfireDust> dustList, Vector3 impactPos, Vector3 impactNormal)
        {
            if (dust == true && dustList != null && dustList.Count > 0)
                for (int i = 0; i < dustList.Count; i++)
                    if (dustList[i] != null)
                        if (dustList[i].onImpact == true)
                            RFParticles.CreateDustImpact(dustList[i], impactPos, impactNormal);
        }

        /// /////////////////////////////////////////////////////////
        /// Impact Activation
        /// /////////////////////////////////////////////////////////
        
        // Activate all rigid scripts in radius range
        List<RayfireRigid> ActivationCheck(RayfireRigid scrTarget, Vector3 position)
        {
            // Get rigid list with target object
            List<RayfireRigid> rigidList = new List<RayfireRigid>();
            if (scrTarget != null)
                rigidList.Add (scrTarget);

            // Check fo radius activation
            if (radius > 0)
            {
                // Get all colliders
                Collider[] colliders = Physics.OverlapSphere(position, radius, mask);

                // Collect all rigid bodies in range
                for (int i = 0; i < colliders.Length; i++)
                {
                    // Tag filter
                    if (tagFilter != untagged && colliders[i].CompareTag (tagFilter) == false)
                        continue;

                    // Get attached rigid body
                    RayfireRigid scrRigid = colliders[i].gameObject.GetComponent<RayfireRigid>();

                    // TODO check for connected cluster

                    // Collect new Rigid bodies and rigid scripts
                    if (scrRigid != null && rigidList.Contains(scrRigid) == false)
                        rigidList.Add(scrRigid);
                }
            }

            // Activate Rigid
            for (int i = 0; i < rigidList.Count; i++)
                if (rigidList[i].simulationType == SimType.Inactive || rigidList[i].simulationType == SimType.Kinematic)
                    if (rigidList[i].activation.byImpact == true)
                        rigidList[i].Activate();

            return rigidList;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////

        // Get shooting ray
        public Vector3 ShootVector
        {
            get
            {
                // Vector to target if defined
                if (target != null)
                {
                    Vector3 targetRay = target.position - transform.position;
                    return targetRay.normalized;
                }

                // Vectors by axis
                if (axis == AxisType.XRed)
                    return transform.right;
                if (axis == AxisType.YGreen)
                    return transform.up;
                if (axis == AxisType.ZBlue)
                    return transform.forward;
                return transform.up;
            }
        }
    }
}
