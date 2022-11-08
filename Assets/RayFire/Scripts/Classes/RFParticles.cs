using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayFire
{
    [System.Serializable]
    public class RFParticleNoise
    {
        [Header("  Main")]
        [Space (3)]
        
        public bool enabled;
        [Space (2)]
        public ParticleSystemNoiseQuality quality;
        
        [Header("  Strength")]
        [Space (3)]
        
        [Range (0f, 3f)] public float strengthMin;
        [Space (2)]
        [Range (0f, 3f)] public float strengthMax;
        
        [Header("  Other")]
        [Space (3)]
        
        [Range (0.001f, 3f)] public float frequency;
        [Space (2)]
        [Range (0f, 2f)] public float scrollSpeed;
        [Space (2)]
        public bool damping;
        
        // Constructor
        public RFParticleNoise()
        {
            enabled     = false;
            strengthMin = 0.3f;
            strengthMax = 0.6f;
            frequency   = 0.3f;
            scrollSpeed = 0.7f;
            damping     = true;
            quality     = ParticleSystemNoiseQuality.High;
        }
        
        // Copy from
        public void CopyFrom (RFParticleNoise source)
        {
            enabled     = source.enabled;
            strengthMin = source.strengthMin;
            strengthMax = source.strengthMax;
            frequency   = source.frequency;
            scrollSpeed = source.scrollSpeed;
            damping     = source.damping;
            quality     = source.quality;
        }
    }

    [System.Serializable]
    public class RFParticleRendering
    {
        [Header("  Shadows")]
        [Space (3)]
        
        public bool castShadows;
        [Space (2)]
        public bool receiveShadows;
        
        [Header("  Light")]
        [Space (3)]
        
        [Space (2)]
        public LightProbeUsage lightProbes;
        
        // Constructor
        public RFParticleRendering()
        {
             castShadows    = true;
             receiveShadows = true;
             lightProbes    = LightProbeUsage.Off;
        }
        
        // Copy from
        public void CopyFrom (RFParticleRendering source)
        {
            castShadows    = source.castShadows;
            receiveShadows = source.receiveShadows;
            lightProbes    = source.lightProbes;
        }
    }
    
    [System.Serializable]
    public class RFParticleDynamicDebris
    {
        [Header("  Speed")]
        [Space (3)]
        
        // TODO Speed by size, separate speed for Impact
        [Range(0f, 10f)] public float speedMin;
        [Space (2)]
        [Range(0f, 10f)] public float speedMax;
        
        [Header("  Inherit Velocity")]
        [Space (3)]
        
        [Range(0f, 3f)] public float velocityMin;
        [Space (2)]
        [Range(0f, 3f)] public float velocityMax;
        
        [Header("  Gravity Modifier")]
        [Space (3)]
        
        [Range(-2f, 2f)] public float gravityMin;
        [Space (2)]
        [Range(-2f, 2f)] public float gravityMax;

        [Header("  Rotation")]
        [Space (3)]
        
        [Range(0f, 1f)] public float rotationSpeed;
        
        // Constructor
        public RFParticleDynamicDebris()
        {
            speedMin = 1f;
            speedMax = 4f;
            velocityMin = 0.5f;
            velocityMax = 1.5f;
            rotationSpeed = 0.5f;
            gravityMin = 0.8f;
            gravityMax = 1.1f;
        }
        
        // Copy from
        public void CopyFrom (RFParticleDynamicDebris source)
        {
            speedMin        = source.speedMin;
            speedMax        = source.speedMax;
            velocityMin     = source.velocityMin;
            velocityMax     = source.velocityMax;
            rotationSpeed   = source.rotationSpeed;
            gravityMin      = source.gravityMin;
            gravityMax      = source.gravityMax;
        }
    }
    
    [System.Serializable]
    public class RFParticleDynamicDust
    {
        [Header("  Speed")]
        [Space (3)]
        
        // TODO Speed by size, separate speed for Impact
        [Range(0f, 10f)] public float speedMin;
        [Space (2)]
        [Range(0f, 10f)] public float speedMax;
        
        [Header("  Rotation")]
        [Space (3)]
        
        [Range(0f, 1f)] public float rotation;

        [Header("  Gravity Modifier")]
        [Space (3)]
        
        [Range(-2f, 2f)] public float gravityMin;
        [Space (2)]
        [Range(-2f, 2f)] public float gravityMax;

        // Constructor
        public RFParticleDynamicDust()
        {
            speedMin        = 0.5f;
            speedMax        = 1f;
            rotation        = 0.5f;
            gravityMin      = 0.01f;
            gravityMax      = 0.6f;
        }
        
        // Copy from
        public void CopyFrom (RFParticleDynamicDust source)
        {
            speedMin        = source.speedMin;
            speedMax        = source.speedMax;
            rotation        = source.rotation;
            gravityMin      = source.gravityMin;
            gravityMax      = source.gravityMax;
        }
    }
    
    [System.Serializable]
    public class RFParticleEmission
    {
        [Header("  Burst")]
        [Space (3)]
        
        public RFParticles.BurstType burstType;
        [Space (2)]
        [Range(0, 500)] public int burstAmount;
        
        [Header("  Distance")]
        [Space (3)]
        
        [Range(0f, 5f)] public float distanceRate;
        [Space (2)]
        [Range(0.5f, 10)] public float duration;
        
        [Header("  Lifetime")]
        [Space (3)]
        
        [Range(1f, 60f)] public float lifeMin;
        [Space (2)]
        [Range(1f, 60f)] public float lifeMax;

        [Header("  Size")]
        [Space (3)]
        
        [Range(0.1f, 10f)] public float sizeMin;
        [Space (2)]
        [Range(0.1f, 10f)] public float sizeMax;
        
        // Constructor
        public RFParticleEmission()
        {
            burstType = RFParticles.BurstType.PerOneUnitSize;
            burstAmount = 20;
            duration = 4;
            distanceRate = 1f;
            lifeMin = 2f;
            lifeMax = 13f;
            sizeMin = 0.5f;
            sizeMax = 2.5f;
        }
        
        // Copy from
        public void CopyFrom (RFParticleEmission source)
        {
            burstType = source.burstType;
            burstAmount = source.burstAmount;
            distanceRate = source.distanceRate;
            lifeMin = source.lifeMin;
            lifeMax = source.lifeMax;
            sizeMin = source.sizeMin;
            sizeMax = source.sizeMax;
        }
    }
    
    [System.Serializable]
    public class RFParticleLimitations
    {
        [Header("  Particle system")]
        [Space (3)]
        
        [Range(3, 100)] public int minParticles;
        [Space (2)]
        [Range(5, 100)] public int maxParticles;
        
        [Header("  Fragments")]
        [Space (3)]
                
        [Range(10, 100)] public int percentage;
        [Space (2)]
        [Range(0.05f, 5)] public float sizeThreshold;

        [HideInInspector] [Range(0, 5)] public int demolitionDepth;

        // Constructor
        public RFParticleLimitations()
        {
            minParticles = 3;
            maxParticles = 20;
            percentage = 50;
            sizeThreshold = 0.5f;
            demolitionDepth = 2;
        }
        
        // Copy from
        public void CopyFrom (RFParticleLimitations source)
        {
            minParticles = source.minParticles;
            maxParticles = source.maxParticles;
            percentage    = source.percentage;
            sizeThreshold = source.sizeThreshold;
            demolitionDepth = source.demolitionDepth;
        }
    }

    [System.Serializable]
    public class RFParticleCollisionDebris
    {
        // death on collision
        // dynamic collision
        
        public enum RFParticleCollisionMatType
        {
            ByPhysicalMaterial = 0,
            ByProperties       = 1
        }
        
        [Header("  Common")]
        [Space (3)]
        
        public LayerMask collidesWith;
        [Space (2)]
        
        public ParticleSystemCollisionQuality quality;
        
        [Space (2)]
        
        [Range(0.1f, 2f)] public float radiusScale;
        
        [Header("  Dampen")]
        [Space (3)]
        
        public RFParticleCollisionMatType dampenType;
        [Space (2)]
        
        [Range (0f, 1f)] public float dampenMin;
        [Space (2)]
        
        [Range (0f, 1f)] public float dampenMax;
        
        [Header("  Bounce")]
        [Space (3)]
        
        public RFParticleCollisionMatType bounceType;
        [Space (2)]
        
        [Range (0f, 1f)] public float bounceMin;
        [Space (2)]
        
        [Range (0f, 1f)] public float bounceMax;
        
        [HideInInspector] public bool propertiesSet = false;
        
        // Constructor
        public RFParticleCollisionDebris()
        {
            collidesWith = -1; // -1 Everything, 0 Nothing
            quality = ParticleSystemCollisionQuality.High;
            radiusScale = 0.1f;

            dampenType = RFParticleCollisionMatType.ByPhysicalMaterial;
            dampenMin = 0.1f;
            dampenMax = 0.4f;
            
            bounceType = RFParticleCollisionMatType.ByPhysicalMaterial;
            bounceMin  = 0.2f;
            bounceMax  = 0.4f;
        }
        
        // Copy from
        public void CopyFrom (RFParticleCollisionDebris source)
        {
            collidesWith = source.collidesWith;
            quality      = source.quality;
            radiusScale  = source.radiusScale;

            dampenType = source.dampenType;
            dampenMin  = source.dampenMin;
            dampenMax  = source.dampenMax;
            
            bounceType = source.bounceType;
            bounceMin  = source.bounceMin;
            bounceMax  = source.bounceMax;

            propertiesSet = source.propertiesSet;
        }
        
        // Set material properties
        public void SetMaterialProps (RayfireDebris debris)
        {
            // Properties was set. Exclude this step
            if (propertiesSet == true)
                return;
            
            // Do this method once
            propertiesSet = true;
            
            // No need to set collision properties. NO collision expected
            if (debris.collision.collidesWith == 0) // Nothing)
                return;

            // own props should be used
            if (dampenType == RFParticleCollisionMatType.ByProperties && bounceType == RFParticleCollisionMatType.ByProperties)
                return;
            
            // Set collider to take properties
            Collider collider;
            if (debris.rigid == null || debris.rigid.physics.meshCollider == null)
                collider = debris.GetComponent<Collider>();
            else
                collider = debris.rigid.physics.meshCollider;

            // No collider
            if (collider == null)
                return;

            // No collider material
            if (collider.sharedMaterial == null)
                return;
            
            // Set dampen
            if (dampenType == RFParticleCollisionMatType.ByPhysicalMaterial)
            {
                dampenMin = collider.sharedMaterial.dynamicFriction;
                dampenMax = dampenMin * 0.05f + dampenMin;
            }
            
            // Set bounce
            if (bounceType == RFParticleCollisionMatType.ByPhysicalMaterial)
            {
                bounceMin = collider.sharedMaterial.bounciness;
                bounceMax = bounceMin * 0.05f + bounceMin;
            }
        }
    }
    
    [System.Serializable]
    public class RFParticleCollisionDust
    {
        [Header("  Common")]
        [Space (3)]
        
        public LayerMask collidesWith;
        [Space (2)]
        public ParticleSystemCollisionQuality quality;
        [Space (2)]
        [Range(0.1f, 2f)] public float radiusScale;

        // Constructor
        public RFParticleCollisionDust()
        {
            collidesWith = -1;
            quality      = ParticleSystemCollisionQuality.High;
            radiusScale  = 1f;
        }
        
        // Copy from
        public void CopyFrom (RFParticleCollisionDust source)
        {
            collidesWith = source.collidesWith;
            quality      = source.quality;
            radiusScale  = source.radiusScale;
        }
    }
    
    [System.Serializable]
    public class RFParticles
    {
        /// /////////////////////////////////////////////////////////
        /// Enums
        /// /////////////////////////////////////////////////////////

        public enum RFParticleCollisionMatType
        {
            ByPhysicalMaterial = 0,
            ByProperties       = 1
        }
        
        public enum BurstType
        {
            None            = 0,
            TotalAmount     = 1,
            PerOneUnitSize  = 2,
            FragmentAmount  = 3
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static vars
        /// /////////////////////////////////////////////////////////
        
        public static ParticleSystem.MinMaxCurve constantCurve = new ParticleSystem.MinMaxCurve(1, 2);

        static string debrisStr = "_debris";
        static string dustStr = "_dust";
        
        /// /////////////////////////////////////////////////////////
        /// Rigid/ Rigid Root components set
        /// /////////////////////////////////////////////////////////
        
        // Set Particle Components: Initialize, collect
        public static void SetParticleComponents (RayfireRigid scr)
        {
            // Clear or new
            if (scr.HasDebris == true)
                scr.debrisList.Clear();
            else
                scr.debrisList = new List<RayfireDebris>(); 
        
            // Get all Debris and initialize
            RayfireDebris[] debrisArray = scr.GetComponents<RayfireDebris>();
            if (debrisArray.Length > 0)
            {
                for (int i = 0; i < debrisArray.Length; i++)
                    debrisArray[i].Initialize();

                scr.debrisList = new List<RayfireDebris>();
                for (int i = 0; i < debrisArray.Length; i++)
                    if (debrisArray[i].initialized == true)
                    {
                        scr.debrisList.Add (debrisArray[i]);
                        debrisArray[i].rigid = scr;
                    }
            }
        
            // Clear or new
            if (scr.HasDust == true)
                scr.dustList.Clear();
            else
                scr.dustList = new List<RayfireDust>(); 

            // Get all Dust and initialize
            RayfireDust[] dustArray = scr.GetComponents<RayfireDust>();
            if (dustArray.Length > 0)
            {
                for (int i = 0; i < dustArray.Length; i++)
                    dustArray[i].Initialize();

                scr.dustList = new List<RayfireDust>();
                for (int i = 0; i < dustArray.Length; i++)
                    if (dustArray[i].initialized == true)
                    {
                        scr.dustList.Add (dustArray[i]);
                        dustArray[i].rigid = scr;
                    }
            }
        }
        
        // Set Particle Components: Initialize, collect
        public static void SetParticleComponents(RayfireRigidRoot scr)
        {
            // Set particle components for meshRoots
            for (int i = 0; i < scr.meshRoots.Count; i++)
                SetParticleComponents (scr.meshRoots[i]);

            // Set size sum
            scr.sizeSum = 0;
            for (int i = 0; i < scr.rigidRootShards.Count; i++)
                scr.sizeSum += scr.rigidRootShards[i].sz;

            // Clear or new
            if (scr.HasDebris == true)
                scr.debrisList.Clear();
            else
                scr.debrisList = new List<RayfireDebris>(); 
            
            RayfireDebris[] debrisArray = scr.GetComponents<RayfireDebris>();
            if (debrisArray.Length > 0)
            {
                for (int i = 0; i < debrisArray.Length; i++)
                    debrisArray[i].Initialize();
                scr.debrisList = new List<RayfireDebris>();
                for (int i = 0; i < debrisArray.Length; i++)
                    if (debrisArray[i].initialized == true)
                        scr.debrisList.Add (debrisArray[i]);
            }
            
            // Clear or new
            if (scr.HasDust == true)
                scr.dustList.Clear();
            else
                scr.dustList = new List<RayfireDust>(); 
            
            // Get all Dust and initialize
            RayfireDust[] dustArray = scr.GetComponents<RayfireDust>();
            if (dustArray.Length > 0)
            {
                for (int i = 0; i < dustArray.Length; i++)
                    dustArray[i].Initialize();
                scr.dustList = new List<RayfireDust>();
                for (int i = 0; i < dustArray.Length; i++)
                    if (dustArray[i].initialized == true)
                        scr.dustList.Add (dustArray[i]);
            }

            // List for creates particle system to reset them
            scr.particleList = new List<Transform>();
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////
        
        // Init particles on demolition
        public static void InitDemolitionParticles(RayfireRigid source)
        { 
            // No frags. Reference demolition can create debris without fragments
            if (source.demolitionType != DemolitionType.ReferenceDemolition && source.HasFragments == false)
                return;
 
            // Create debris particles
            if (source.HasDebris == true)
                CreateDemolitionDebris (source);
                
            // Create dust particles
            if (source.HasDust == true)
                CreateDemolitionDust (source);
            
            // Detach child particles in case object has child particles and about to be deleted
            DetachParticles(source);
        }

        // Create debris particle system
        public static void CreateDemolitionDebris(RayfireRigid rigid)
        {
            List<RayfireDebris> particlesTargets = new List<RayfireDebris>();
            for (int i = 0; i < rigid.debrisList.Count; i++)
            {
                // Skip if no demolition debris required
                if (rigid.debrisList[i].onDemolition == false)
                    continue;
                
                // Empty reference particles
                if (rigid.demolitionType == DemolitionType.ReferenceDemolition && rigid.HasFragments == false)
                {
                    CreateReferenceDemolitionDebris (rigid.debrisList[i]);
                    continue;
                }

                // Skip if has no child debris
                if (rigid.debrisList[i].HasChildren == false)
                    continue;
                
                // Debris hosts
                GetDebrisTargets (particlesTargets, rigid.debrisList[i].children, rigid.debrisList[i].limitations.sizeThreshold, rigid.debrisList[i].limitations.percentage, 0);

                Debug.Log ("------");
                
                Debug.Log (rigid.debrisList[i].children.Count);
                
                // No targets
                if (particlesTargets.Count == 0)
                    continue;
                
                Debug.Log (rigid.fragments.Count);
                
                // Get amount list
                SetRigidDebrisFinalAmount (particlesTargets, rigid.debrisList[i].emission.burstType, rigid.debrisList[i].emission.burstAmount);

                // Create particle systems
                int seed = 0;
                for (int p = 0; p < particlesTargets.Count; p++)
                {
                    // Create single debris particle system
                    Random.InitState (seed++);
                    CreateDebrisRigid (particlesTargets[p]);
                }
                particlesTargets.Clear();
            }
        }

        // Creat demolition debris for reference demolition with Empty reference
        static void CreateReferenceDemolitionDebris (RayfireDebris debris)
        {
            // Get amount list
            SetRigidDebrisFinalAmount (new List<RayfireDebris>(){debris}, debris.emission.burstType, debris.emission.burstAmount);
                    
            // Create debris
            CreateDebrisRigid (debris);
        }
        
        // Create dust particle system
        public static void CreateDemolitionDust(RayfireRigid source)
        {
            List<RayfireDust> particlesTargets = new List<RayfireDust>();
            for (int i = 0; i < source.dustList.Count; i++)
            {
                // Skip if no demolition debris required
                if (source.dustList[i].onDemolition == false)
                    continue;
                
                // Empty reference particles
                if (source.demolitionType == DemolitionType.ReferenceDemolition && source.HasFragments == false)
                {
                    SetDustFinalAmount (new List<RayfireDust>(){source.dustList[i]}, source.dustList[i].emission.burstType, source.dustList[i].emission.burstAmount);
                    CreateDustRigid (source.dustList[i]);
                    continue;
                }
                
                // Skip if has no child debris
                if (source.dustList[i].HasChildren == false)
                    continue;
                
                // Dust hosts
                GetDustTargets (particlesTargets, source.dustList[i].children, source.dustList[i].limitations.sizeThreshold, source.dustList[i].limitations.percentage, 1);

                // No targets
                if (particlesTargets.Count == 0)
                    continue;
                
                // Get amount list
                SetDustFinalAmount (particlesTargets, source.dustList[i].emission.burstType, source.dustList[i].emission.burstAmount);
                
                // Create particle systems
                int seed = 0;
                for (int p = 0; p < particlesTargets.Count; p++)
                {
                    // Create single dust particle system
                    Random.InitState (seed++);
                    CreateDustRigid (particlesTargets[p]);
                }
                particlesTargets.Clear();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////
        
        // Init Rigid particles on activation
        public static void InitActivationParticlesRigid(RayfireRigid rigid)
        {
            // Create debris particles
            if (rigid.HasDebris == true)
                for (int i = 0; i < rigid.debrisList.Count; i++)
                    if (rigid.debrisList[i].onActivation == true)
                        if (rigid.objectType == ObjectType.Mesh)
                            CreateDebrisRigid (rigid.debrisList[i]);
                        else if (rigid.IsCluster == true)
                            CreateDebrisCluster (rigid, rigid.debrisList[i]);

            // Create dust particles
            if (rigid.HasDust == true)
                for (int i = 0; i < rigid.dustList.Count; i++)
                    if (rigid.dustList[i].onActivation == true)
                        if (rigid.objectType == ObjectType.Mesh)
                            CreateDustRigid (rigid.dustList[i]);
                        else if (rigid.IsCluster == true)
                            CreateDustCluster (rigid, rigid.dustList[i]);
        }
        
        // Init Shard particles on activation
        public static void InitActivationParticlesShard(RayfireRigidRoot root, RFShard shard)
        {
            // RigidRoot shard
            if (shard.rigid == null)
            {
                // Create debris particles
                if (root.HasDebris == true)
                    for (int i = 0; i < root.debrisList.Count; i++)
                        if (root.debrisList[i].onActivation == true)
                            CreateDebrisShard (root, root.debrisList[i], shard);

                // Create dust particles
                if (root.HasDust == true)
                    for (int i = 0; i < root.dustList.Count; i++)
                        if (root.dustList[i].onActivation == true)
                            CreateDustShard (root, root.dustList[i], shard);
            }

            // RigidRoot -> MeshRoot shard
            else if (shard.rigid.objectType == ObjectType.MeshRoot)
            {
                // Create debris particles
                if (shard.rigid.HasDebris == true)
                    for (int i = 0; i < shard.rigid.debrisList.Count; i++)
                        if (shard.rigid.debrisList[i].onActivation == true)
                            CreateDebrisShard (root, shard.rigid.debrisList[i], shard);

                // Create dust particles
                if (shard.rigid.HasDust == true)
                    for (int i = 0; i < shard.rigid.dustList.Count; i++)
                        if (shard.rigid.dustList[i].onActivation == true)
                            CreateDustShard (root, shard.rigid.dustList[i], shard);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Create Debris
        /// /////////////////////////////////////////////////////////
        
        // Create single debris particle system for Rigid mesh object
        public static void CreateDebrisRigid(RayfireDebris target)
        {
            // No particles
            if (target.amountFinal < target.limitations.minParticles && target.emission.distanceRate == 0)
                return;

            // Particle system
            CreateParticleSystemDebris (target, target.transform);
            
            // Get emit material index
            int emitMatIndex = GetEmissionMatIndex (target.rigid.meshRenderer, target.emissionMaterial);

            // Create debris
            target.CreateDebris (target, target.rigid.meshFilter, emitMatIndex, target.pSystem);
        }
        
        // Create single debris particle system for Connected Cluster
        public static void CreateDebrisCluster(RayfireRigid rigid, RayfireDebris debris)
        {
            for (int j = rigid.clusterDemolition.cluster.shards.Count - 1; j >= 0; j--)
            {
                // If has detached neib shard
                if (rigid.clusterDemolition.cluster.shards[j].neibShards.Count < rigid.clusterDemolition.cluster.shards[j].nAm)
                {
                    // Cluster crated by RigidRoot shards
                    if (rigid.rigidRoot != null)
                        CreateDebrisShard (rigid.rigidRoot, debris, rigid.clusterDemolition.cluster.shards[j]);
                    
                    // Cluster created by MeshRoot fragments
                    else if (rigid.meshRoot != null)
                    {
                        // Set amount
                        debris.amountFinal = GetShardFinalAmount (rigid.clusterDemolition.cluster.shards[j],
                            debris.emission.burstType, debris.emission.burstAmount, rigid.clusterDemolition.cluster.shards[j].sz);

                        // No particles
                        if (debris.amountFinal < debris.limitations.minParticles && debris.emission.distanceRate == 0)
                            continue;

                        // Particle system
                        CreateParticleSystemDebris (debris, rigid.clusterDemolition.cluster.shards[j].tm);
                        
                        // Collect particles to reset them
                        rigid.meshRoot.particleList.Add (debris.hostTm);
                        
                        // Get emit material index
                        int emitMatIndex = GetEmissionMatIndex (rigid.clusterDemolition.cluster.shards[j].rigid.meshRenderer, debris.emissionMaterial);

                        // Create debris
                        debris.CreateDebris (debris, rigid.clusterDemolition.cluster.shards[j].mf, emitMatIndex, debris.pSystem);
                    }
                }
            }
        }
        
        // Create single debris particle system for RigidRoot shard
        public static void CreateDebrisShard(RayfireRigidRoot root, RayfireDebris debris, RFShard shard)
        {
            // Set amount
            debris.amountFinal = GetShardFinalAmount (shard, debris.emission.burstType, debris.emission.burstAmount, root.sizeSum);
            
            // No particles
            if (debris.amountFinal < debris.limitations.minParticles && debris.emission.distanceRate == 0)
                return;

            // Particle system
            CreateParticleSystemDebris (debris, shard.tm);
            
            // Collect particles to reset them
            root.particleList.Add (debris.hostTm);
            
            // Get emit material index
            int emitMatIndex = GetEmissionMatIndex (shard.tm.GetComponent<Renderer>(), debris.emissionMaterial);

            // Create debris
            debris.CreateDebris (debris, shard.mf, emitMatIndex, debris.pSystem);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Create Dust
        /// /////////////////////////////////////////////////////////
        
        // Create single dust particle system
        public static void CreateDustRigid(RayfireDust target)
        {
            // No particles
            if (target.amountFinal < target.limitations.minParticles && target.emission.distanceRate == 0)
                return;
            
            // Particle system
            CreateParticleSystemDust (target, target.transform);

            // Get emit material index and create
            int emitMatIndex = GetEmissionMatIndex (target.rigid.meshRenderer, target.emissionMaterial);

            // Create debris
            target.CreateDust (target, target.rigid.meshFilter, emitMatIndex, target.pSystem);
        }
        
        // Create single debris particle system for Connected Cluster
        public static void CreateDustCluster(RayfireRigid rigid, RayfireDust dust)
        {
            for (int j = rigid.clusterDemolition.cluster.shards.Count - 1; j >= 0; j--)
            {
                // If has detached neib shard
                if (rigid.clusterDemolition.cluster.shards[j].neibShards.Count < rigid.clusterDemolition.cluster.shards[j].nAm)
                {
                    // Cluster crated by RigidRoot shards
                    if (rigid.rigidRoot != null)
                        CreateDustShard (rigid.rigidRoot, dust, rigid.clusterDemolition.cluster.shards[j]);
                    
                    // Cluster created by MeshRoot fragments
                    else if (rigid.meshRoot != null)
                    {
                        // Set amount
                        dust.amountFinal = GetShardFinalAmount (rigid.clusterDemolition.cluster.shards[j],
                            dust.emission.burstType, dust.emission.burstAmount, rigid.clusterDemolition.cluster.shards[j].sz);

                        // No particles
                        if (dust.amountFinal < dust.limitations.minParticles && dust.emission.distanceRate == 0)
                            continue;

                        // Particle system
                        CreateParticleSystemDust (dust, rigid.clusterDemolition.cluster.shards[j].tm);
                        
                        // Collect particles to reset them
                        rigid.meshRoot.particleList.Add (dust.hostTm);
                        
                        // Get emit material index
                        int emitMatIndex = GetEmissionMatIndex (rigid.clusterDemolition.cluster.shards[j].rigid.meshRenderer, dust.emissionMaterial);

                        // Create debris
                        dust.CreateDust (dust, rigid.clusterDemolition.cluster.shards[j].mf, emitMatIndex, dust.pSystem);
                    }
                }
            }
        }
        
        // Create single dust particle system
        public static void CreateDustShard(RayfireRigidRoot root, RayfireDust dust, RFShard shard)
        {
            // Set amount
            dust.amountFinal = GetShardFinalAmount (shard, dust.emission.burstType, dust.emission.burstAmount, root.sizeSum);
            
            // No particles
            if (dust.amountFinal < dust.limitations.minParticles && dust.emission.distanceRate == 0)
                return;
            
            // Particle system
            CreateParticleSystemDust (dust, shard.tm);

            // Collect particles to reset them
            root.particleList.Add (dust.hostTm);
            
            // Get emit material index
            int emitMatIndex = GetEmissionMatIndex (shard.tm.GetComponent<Renderer>(), dust.emissionMaterial);

            // Create debris
            dust.CreateDust (dust, shard.mf, emitMatIndex, dust.pSystem);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Create particle system
        /// /////////////////////////////////////////////////////////
        
        // Create host and particle system
        public static ParticleSystem CreateParticleSystemDebris (RayfireDebris debris, Transform tm)
        {
            // Create ps
            debris.pSystem      = CreateParticleSystem (tm);
            debris.pSystem.name = debris.name + debrisStr; 
            debris.hostTm       = debris.pSystem.transform;
            
            // Destroy after all particles death TODO replace with back to pool
            Object.Destroy(debris.pSystem.gameObject, debris.emission.lifeMax + debris.pSystem.main.duration);

            return debris.pSystem;
        }
        
        // Create host and particle system
        public static ParticleSystem CreateParticleSystemDust (RayfireDust dust, Transform tm)
        {
            // Create ps
            dust.pSystem      = CreateParticleSystem (tm);
            dust.pSystem.name = dust.name + dustStr;
            dust.hostTm       = dust.pSystem.transform;
            
            // Destroy after all particles death TODO replace with back to pool
            Object.Destroy(dust.pSystem.gameObject, dust.emission.lifeMax + dust.pSystem.main.duration);

            return dust.pSystem;
        }
        
        // Create host and particle system
        static ParticleSystem CreateParticleSystem(Transform tm)
        {
            // Get object from pool or create
            ParticleSystem ps = RayfireMan.inst == null
                ? RFPoolingParticles.CreateParticleInstance()
                : RayfireMan.inst.particles.GetPoolObject(RayfireMan.inst.transForm);

            // Create root
            ps.transform.position = tm.position;
            ps.transform.rotation = tm.rotation;
            ps.transform.SetParent (tm);
            ps.transform.localScale = Vector3.one;

            // Activate
            ps.gameObject.SetActive (true);
            
            // Stop for further properties set
            ps.Stop();
            
            return ps;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Impact
        /// /////////////////////////////////////////////////////////
        
        // Create single debris particle system
        public static void CreateDebrisImpact(RayfireDebris debris, Vector3 impactPos, Vector3 impactNormal)
        {
            // Particle system
            ParticleSystem ps = CreateParticleSystemDebris (debris, debris.transform);
            
            // Align over impact
            debris.hostTm.position = impactPos;
            debris.hostTm.LookAt (impactPos + impactNormal);
            debris.hostTm.parent     = RayfireMan.inst.transForm;
            debris.hostTm.localScale = Vector3.one;
            
            // Set amount
            debris.amountFinal = debris.emission.burstAmount;
            
            // Create debris
            debris.CreateDebris (debris, null, -1, ps);
        }

        // Create single debris particle system
        public static void CreateDustImpact(RayfireDust dust, Vector3 impactPos, Vector3 impactNormal)
        {
            // Particle system
            ParticleSystem ps = CreateParticleSystemDust (dust, dust.transform);

            // Align over impact
            dust.hostTm.position = impactPos;
            dust.hostTm.LookAt (impactPos + impactNormal);
            dust.hostTm.parent     = RayfireMan.inst.transForm;
            dust.hostTm.localScale = Vector3.one;
            
            // Set amount
            dust.amountFinal = dust.emission.burstAmount;
            
            // Create debris
            dust.CreateDust (dust, null, -1, ps);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Detach child particles in case object has child particles and about to be deleted
        static void DetachParticles(RayfireRigid source)
        {
            // Detach debris particle system if fragment was already demolished/activated before
            if (source.HasDebris == true) 
            {
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    if (source.debrisList[i].HasChildren == true)
                    {
                        for (int j = 0; j < source.debrisList[i].children.Count; j++)
                        {
                            if (source.debrisList[i].children[j].hostTm != null)
                            {
                                source.debrisList[i].children[j].hostTm.parent     = RayfireMan.inst.transForm;
                                source.debrisList[i].children[j].hostTm.localScale = Vector3.one;
                            }
                        }
                    }
                    
                    if (source.debrisList[i].hostTm != null)
                    {
                        source.debrisList[i].hostTm.parent     = RayfireMan.inst.transForm;
                        source.debrisList[i].hostTm.localScale = Vector3.one;
                    }
                }
            }

            // Detach dust particle system if fragment was already demolished/activated before
            if (source.HasDust == true)
            {
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    if (source.dustList[i].HasChildren == true)
                    {
                        for (int j = 0; j < source.dustList[i].children.Count; j++)
                        {
                            if (source.dustList[i].children[j].hostTm != null)
                            {
                                source.dustList[i].children[j].hostTm.parent     = RayfireMan.inst.transForm;
                                source.dustList[i].children[j].hostTm.localScale = Vector3.one;
                            }
                        }
                    }
                    
                    if (source.dustList[i].hostTm != null)
                    {
                        source.dustList[i].hostTm.parent     = RayfireMan.inst.transForm;
                        source.dustList[i].hostTm.localScale = Vector3.one;
                    }
                }
            }
        }

        // Copy debris and dust
        public static void CopyRigidParticles(RayfireRigid source, RayfireRigid target)
        {
            // TODO reference to original component, do not create new
            
            // Copy debris
            if (source.HasDebris == true)
            {
                // Prepare target debris list
                if (target.debrisList == null)
                    target.debrisList = new List<RayfireDebris>();
                else
                    target.debrisList.Clear();
                
                // Copy every debris from source to target
                RayfireDebris targetDebris;
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    targetDebris = target.gameObject.AddComponent<RayfireDebris>();
                    targetDebris.CopyFrom (source.debrisList[i]);
                    targetDebris.rigid = target;

                    // Collect child debris in parent source debris
                    if (source.debrisList[i].HasChildren == false)
                        source.debrisList[i].children = new List<RayfireDebris>();
                    source.debrisList[i].children.Add (targetDebris);
                    
                    // Collect debris for target
                    target.debrisList.Add (targetDebris);
                }
            }
            
            // Copy dust
            if (source.HasDust == true)
            {
                // Prepare target dust list
                if (target.dustList == null)
                    target.dustList = new List<RayfireDust>();
                else
                    target.dustList.Clear();
                
                RayfireDust targetDust;
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    targetDust = target.gameObject.AddComponent<RayfireDust>();
                    targetDust.CopyFrom (source.dustList[i]);
                    targetDust.rigid = target;

                    if (source.dustList[i].HasChildren == false)
                        source.dustList[i].children = new List<RayfireDust>();
                    source.dustList[i].children.Add (targetDust);
                    
                    // Collect debris for target
                    target.dustList.Add (targetDust);
                }
            }
        }
        
        // Copy debris and dust
        public static void CopyRootMeshParticles(RayfireRigid source, List<RayfireRigid> targets)
        {
            // Clean null debris
            if (source.HasDebris == true)
                for (int i = source.debrisList.Count - 1; i >= 0; i--)
                    if (source.debrisList[i] == null)
                        source.debrisList.RemoveAt (i);

            // Copy debris. only initialized debris in this list
            if (source.HasDebris == true)
            {
                for (int d = 0; d < source.debrisList.Count; d++)
                {
                    // Set max amount
                    int maxAmount = targets.Count;
                   
                    // TODO optional???
                    // if (source.debrisList[d].limitations.percentage < 100)
                    //    maxAmount = targets.Count * source.debrisList[d].limitations.percentage / 100;

                    // Copy component
                    for (int i = 0; i < targets.Count; i++)
                    {
                        // Max amount reached
                        if (maxAmount <= 0)
                            break;

                        // TODO consider size threshold

                        // Filter by percentage
                        if (Random.Range (0, 100) > source.debrisList[d].limitations.percentage)
                            continue;

                        // Copy
                        RayfireDebris targetDebris = targets[i].gameObject.AddComponent<RayfireDebris>();
                        targetDebris.CopyFrom (source.debrisList[d]);
                        targetDebris.rigid = targets[i];
                        
                        // Collect debris for Rigid
                        if (targets[i].debrisList == null)
                            targets[i].debrisList = new List<RayfireDebris>();
                        targets[i].debrisList.Add (targetDebris);
                        
                        // Collect debris for parent debris
                        if (source.debrisList[d].children == null)
                            source.debrisList[d].children = new List<RayfireDebris>();
                        source.debrisList[d].children.Add (targetDebris);
                        
                        maxAmount--;
                    }
                    
                    // Get amount list
                    SetRigidDebrisFinalAmount (source.debrisList[d].children, source.debrisList[d].emission.burstType, source.debrisList[d].emission.burstAmount);
                }
            }
            
            // Clean null dust
            if (source.HasDust == true)
                for (int i = source.dustList.Count - 1; i >= 0; i--)
                    if (source.dustList[i] == null)
                        source.dustList.RemoveAt (i);
            
            // Copy dust
            if (source.HasDust == true)
            {
                for (int d = 0; d < source.dustList.Count; d++)
                {
                    // Set max amount
                    int maxAmount = targets.Count;
                    if (source.dustList[d].limitations.percentage < 100)
                        maxAmount = targets.Count * source.dustList[d].limitations.percentage / 100;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        // Max amount reached
                        if (maxAmount <= 0)
                            break;

                        // Filter by percentage
                        if (Random.Range (0, 100) > source.dustList[d].limitations.percentage)
                            continue;

                        // Copy
                        RayfireDust targetDust = targets[i].gameObject.AddComponent<RayfireDust>();
                        targetDust.CopyFrom (source.dustList[d]);
                        targetDust.rigid = targets[i];
                        
                        // Collect debris for Rigid
                        if (targets[i].dustList == null)
                            targets[i].dustList = new List<RayfireDust>();
                        targets[i].dustList.Add (targetDust);
                        
                        // Collect debris for parent debris
                        if (source.dustList[d].children == null)
                            source.dustList[d].children = new List<RayfireDust>();
                        source.dustList[d].children.Add (targetDust);
                        
                        maxAmount--;
                    }
                    
                    // Get amount list
                    SetDustFinalAmount (source.dustList[d].children, source.dustList[d].emission.burstType, source.dustList[d].emission.burstAmount);
                }
            }
            
            // List for creates particle system to reset them
            source.particleList = new List<Transform>();
        }
        
         // Copy debris and dust
        public static void CopyRigidRootParticles(RayfireRigidRoot source, RayfireRigid target)
        {
            // Copy debris
            if (source.HasDebris == true)
            {
                // Prepare target debris list
                if (target.debrisList == null)
                    target.debrisList = new List<RayfireDebris>();
                else
                    target.debrisList.Clear();
                
                // Copy every debris from source to target
                RayfireDebris targetDebris;
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    targetDebris = target.gameObject.AddComponent<RayfireDebris>();
                    targetDebris.CopyFrom (source.debrisList[i]);
                    targetDebris.rigid = target;

                    // Collect child debris in parent source debris
                    if (source.debrisList[i].HasChildren == false)
                        source.debrisList[i].children = new List<RayfireDebris>();
                    source.debrisList[i].children.Add (targetDebris);
                    
                    // Collect debris for target
                    target.debrisList.Add (targetDebris);
                }
            }
            
            // Copy dust
            if (source.HasDust == true)
            {
                // Prepare target dust list
                if (target.dustList == null)
                    target.dustList = new List<RayfireDust>();
                else
                    target.dustList.Clear();
                
                RayfireDust targetDust;
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    targetDust = target.gameObject.AddComponent<RayfireDust>();
                    targetDust.CopyFrom (source.dustList[i]);
                    targetDust.rigid = target;

                    if (source.dustList[i].HasChildren == false)
                        source.dustList[i].children = new List<RayfireDust>();
                    source.dustList[i].children.Add (targetDust);
                    
                    // Collect debris for target
                    target.dustList.Add (targetDust);
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Main Module
        /// /////////////////////////////////////////////////////////

        // Set main module
        public static void SetMain (ParticleSystem.MainModule main, 
            float lifeMin, float lifeMax, 
            float sizeMin, float sizeMax, 
            float gravityMin, float gravityMax, 
            float speedMin, float speedMax,
            float divergence, int maxParticles,
            float duration)
        {
            main.duration = duration;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;
            main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
            
            constantCurve.constantMin = lifeMin;
            constantCurve.constantMax = lifeMax;
            main.startLifetime = constantCurve;
            
            constantCurve.constantMin = speedMin;
            constantCurve.constantMax = speedMax;
            main.startSpeed = constantCurve;
            
            constantCurve.constantMin = sizeMin;
            constantCurve.constantMax = sizeMax;
            main.startSize = constantCurve;
            
            constantCurve.constantMin = -divergence;
            constantCurve.constantMax = divergence;
            main.startRotation = constantCurve; // Max 6.25f = 360 degree
            
            constantCurve.constantMin = gravityMin;
            constantCurve.constantMax = gravityMax;
            main.gravityModifier = constantCurve;
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        // Set emission
        public static void SetEmission(ParticleSystem.EmissionModule emissionModule, float distanceRate, int burstAmount)
        {
            emissionModule.enabled = true;
            emissionModule.rateOverTimeMultiplier = 0f;
            emissionModule.rateOverDistanceMultiplier = distanceRate;

            // Set burst
            if (burstAmount > 0)
            {
                ParticleSystem.Burst burst = new ParticleSystem.Burst(0.0f, (short)burstAmount, (short)burstAmount, 1, 999f);
                ParticleSystem.Burst[] bursts = new [] { burst };
                emissionModule.SetBursts(bursts);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////

        // Set emitter mesh shape
        public static void SetShapeMesh (ParticleSystem.ShapeModule shapeModule, Mesh mesh, int emitMatIndex, Vector3 shapeScale)
        {
            shapeModule.normalOffset = 0f;
            shapeModule.shapeType = ParticleSystemShapeType.Mesh;
            shapeModule.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shapeModule.mesh = mesh;
            shapeModule.scale = shapeScale;
            
            // Emit from inner surface
            if (emitMatIndex > 0)
            {
                shapeModule.useMeshMaterialIndex = true;
                shapeModule.meshMaterialIndex = emitMatIndex;
            }
        }

        // Set emitter mesh shape
        public static void SetShapeObject(ParticleSystem.ShapeModule shapeModule)
        {
            shapeModule.shapeType = ParticleSystemShapeType.Hemisphere;
            shapeModule.radius = 0.2f;
            shapeModule.radiusThickness = 0f;
        }

        // Set emitter mesh shape
        public static int GetEmissionMatIndex(Renderer renderer, Material mat)
        {
            if (mat != null && renderer != null)
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                      if (renderer.sharedMaterials[i] == mat)
                          return i;
            return -1;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Velocity
        /// /////////////////////////////////////////////////////////

        // Set velocity
        public static void SetVelocity(ParticleSystem.InheritVelocityModule velocity, RFParticleDynamicDebris dynamic)
        {
            if (dynamic.velocityMin > 0 || dynamic.velocityMax > 0)
            {
                velocity.enabled = true;
                velocity.mode    = ParticleSystemInheritVelocityMode.Initial;
                
                constantCurve.constantMin = dynamic.velocityMin;
                constantCurve.constantMax = dynamic.velocityMax;
                velocity.curve   = constantCurve;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rotation Over Lifetime
        /// /////////////////////////////////////////////////////////
        
        // Set Rotation
        public static void SetRotationOverLifeTime(ParticleSystem.RotationOverLifetimeModule rotation, RFParticleDynamicDust dynamic)
        {
            if (dynamic.rotation > 0)
            {
                rotation.enabled = true;
                rotation.separateAxes = true;
                rotation.z = GetCurveRotationByLife(dynamic.rotation);
            }
        }
        
        // Get Curve for Rotation by Speed
        public static ParticleSystem.MinMaxCurve GetCurveRotationByLife(float spin)
        {
            // Value 1f = 57 degrees
            float maxVal = spin * 4f;;
            
            // Max curve
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, maxVal);
            keys[1] = new Keyframe(1f, maxVal * 0.1f);
            AnimationCurve curveMax = new AnimationCurve (keys);
            
            // Min curve
            keys[0] = new Keyframe(0f, -maxVal);
            keys[1] = new Keyframe(1f, -maxVal * 0.1f);
            AnimationCurve curveMin = new AnimationCurve (keys);
            
            return new ParticleSystem.MinMaxCurve(1f, curveMin, curveMax);
        }

        /// /////////////////////////////////////////////////////////
        /// Size Over Life Time
        /// /////////////////////////////////////////////////////////

        // Set size over life time. same axis. Increase almost instantly particles after birth
        public static void SetSizeOverLifeTime(ParticleSystem.SizeOverLifetimeModule sizeOverLifeTime, float size)
        {
            sizeOverLifeTime.enabled = true;
            sizeOverLifeTime.size = GetCurveSizeOverLifeTime(size);
        }

        // Set size over life time. different axis. Increase almost instantly particles after birth
        public static void SetSizeOverLifeTime(ParticleSystem.SizeOverLifetimeModule sizeOverLifeTime, Vector3 size)
        {
            sizeOverLifeTime.enabled = true;
            sizeOverLifeTime.separateAxes = true;
            sizeOverLifeTime.x = GetCurveSizeOverLifeTime(size.x);
            sizeOverLifeTime.y = GetCurveSizeOverLifeTime(size.y);
            sizeOverLifeTime.z = GetCurveSizeOverLifeTime(size.z);
        }
        
        // Get Curve for Size Over Life Time
        public static ParticleSystem.MinMaxCurve GetCurveSizeOverLifeTime(float val)
        {
            Keyframe[] keysSize = new Keyframe[4];
            keysSize[0] = new Keyframe(0f, 0f);
            keysSize[1] = new Keyframe(0.01f, val);
            keysSize[2] = new Keyframe(0.95f, val);
            keysSize[3] = new Keyframe(1f, 0f);

            AnimationCurve curveSize = new AnimationCurve(keysSize);
            ParticleSystem.MinMaxCurve curveSizeTime = new ParticleSystem.MinMaxCurve(1f, curveSize);
            return curveSizeTime;
        }

        /// /////////////////////////////////////////////////////////
        /// Rotation by Speed
        /// /////////////////////////////////////////////////////////

        // Set Rotation by Speed
        public static void SetRotationBySpeed(ParticleSystem.RotationBySpeedModule rotationBySpeed, float rotationSpeed)
        {
            if (rotationSpeed > 0f)
            {
                rotationBySpeed.enabled = true;
                rotationBySpeed.range   = new Vector2 (1f, 0f);
                rotationBySpeed.z       = GetCurveRotationBySpeed (rotationSpeed);
            }
        }

        // Get Curve for Rotation by Speed
        public static ParticleSystem.MinMaxCurve GetCurveRotationBySpeed(float rotationSpeed)
        {
            // Value 1f = 57 degrees
            float maxVal = rotationSpeed * 40f;
            
            // Max curve
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, maxVal);
            keys[1] = new Keyframe(0.5f, 0f);
            AnimationCurve curveMax = new AnimationCurve (keys);
            
            // Min curve
            keys[0] = new Keyframe(0f, -maxVal);
            AnimationCurve curveMin = new AnimationCurve (keys);
            
            return new ParticleSystem.MinMaxCurve(1f, curveMin, curveMax);
        }

        /// /////////////////////////////////////////////////////////
        /// Color Over Lifetime
        /// /////////////////////////////////////////////////////////

        // Set color over life time
        public static void SetColorOverLife(ParticleSystem.ColorOverLifetimeModule colorLife, float opacity)
        {
            colorLife.enabled = true;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
            alphaKeys[0] = new GradientAlphaKey (0f,      0f);
            alphaKeys[1] = new GradientAlphaKey (opacity, 0.1f);
            alphaKeys[2] = new GradientAlphaKey (opacity, 0.2f);
            alphaKeys[3] = new GradientAlphaKey (0f,      1f);
            Gradient gradient = new Gradient();
            gradient.alphaKeys = alphaKeys;
            colorLife.color    = new ParticleSystem.MinMaxGradient (gradient);
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////

        // Set particle system noise
        public static void SetNoise (ParticleSystem.NoiseModule psNoise, RFParticleNoise scrNoise)
        {
            psNoise.enabled     = scrNoise.enabled;
            if (scrNoise.enabled == true)
            {
                psNoise.strength     = new ParticleSystem.MinMaxCurve (scrNoise.strengthMin, scrNoise.strengthMax);
                psNoise.frequency    = scrNoise.frequency;
                psNoise.scrollSpeed  = scrNoise.scrollSpeed;
                psNoise.damping      = scrNoise.damping;
                psNoise.quality      = scrNoise.quality;
                psNoise.separateAxes = true;

                constantCurve.constantMin = scrNoise.strengthMin;
                constantCurve.constantMax = scrNoise.strengthMax;
                psNoise.strengthX         = constantCurve;

                constantCurve.constantMin = scrNoise.strengthMin * 0.3f;
                constantCurve.constantMax = scrNoise.strengthMax * 0.3f;
                psNoise.strengthY         = constantCurve;

                constantCurve.constantMin = scrNoise.strengthMin;
                constantCurve.constantMax = scrNoise.strengthMax;
                psNoise.strengthZ         = constantCurve;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Set collision for debris
        public static void SetCollisionDebris (ParticleSystem.CollisionModule psCollision, RFParticleCollisionDebris coll) {
            psCollision.enabled      = true;
            psCollision.type         = ParticleSystemCollisionType.World;
            psCollision.collidesWith = coll.collidesWith;
            psCollision.quality      = coll.quality;
            psCollision.radiusScale  = coll.radiusScale;
            
            constantCurve.constantMin = coll.dampenMin;
            constantCurve.constantMax = coll.dampenMax;
            psCollision.dampen        = constantCurve;
            
            constantCurve.constantMin = coll.bounceMin;
            constantCurve.constantMax = coll.bounceMax;
            psCollision.bounce        = constantCurve;
            
            //psCollision.enableDynamicColliders = false;
            //psCollision.lifetimeLoss = new ParticleSystem.MinMaxCurve(1f);
        }

        // Set collision for dust
        public static void SetCollisionDust (ParticleSystem.CollisionModule psCollision, RFParticleCollisionDust coll) {
            psCollision.enabled                = true;
            psCollision.type                   = ParticleSystemCollisionType.World;
            psCollision.collidesWith           = coll.collidesWith;
            psCollision.quality                = coll.quality;
            psCollision.radiusScale            = coll.radiusScale;
            psCollision.dampenMultiplier       = 0f;
            psCollision.bounceMultiplier       = 0f;
            psCollision.enableDynamicColliders = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Get debris hosts
        public static void GetDebrisTargets (List<RayfireDebris> filtered, List<RayfireDebris> targets, float sizeThreshold, int percentage, int pType)
        {
            // Set max amount
            int maxAmount = 0;
            for (int i = 0; i < targets.Count; i++)
                if (targets[i].oldChild == false)
                    maxAmount++;
            if (percentage < 100)
                maxAmount = maxAmount * percentage / 100;
            
            // Collect hosts list
            for (int i = 0; i < targets.Count; i++)
            {
                // Max amount reached
                if (filtered.Count >= maxAmount)
                    break;

                // Create only for mesh rigid
                if (targets[i].rigid.objectType != ObjectType.Mesh)
                    continue;

                // Cluster dust, already generated particles
                if (targets[i].oldChild == true)
                    continue;

                // Filter by size threshold
                if (targets[i].rigid.limitations.bboxSize < sizeThreshold)
                    continue;
                
                // Filter by percentage
                if (Random.Range(0, 100) > percentage)
                    continue;

                // Collect particle hosts
                filtered.Add(targets[i]);
            }
            
            // Mark as old child
            for (int i = 0; i < targets.Count; i++)
                targets[i].oldChild = true;   
        }
        
        // Get debris hosts
        public static void GetDustTargets (List<RayfireDust> filtered, List<RayfireDust> targets, float sizeThreshold, int percentage, int pType)
        {
            // Set max amount
            int maxAmount = 0;
            for (int i = 0; i < targets.Count; i++)
                if (targets[i].oldChild == false)
                    maxAmount++;
            if (percentage < 100)
                maxAmount = maxAmount * percentage / 100;

            // Collect hosts list
            for (int i = 0; i < targets.Count; i++)
            {
                // Max amount reached
                if (filtered.Count >= maxAmount)
                    break;
                
                // Create only for mesh rigid
                if (targets[i].rigid.objectType != ObjectType.Mesh)
                    continue;
                
                // Cluster dust, already generated particles
                if (targets[i].oldChild == true)
                    continue;
                
                // Filter by size threshold
                if (targets[i].rigid.limitations.bboxSize < sizeThreshold)
                    continue;

                // Filter by percentage
                if (Random.Range(0, 100) > percentage)
                    continue;

                // Collect particle hosts
                filtered.Add(targets[i]);
            }
            
            // Mark as old child
            for (int i = 0; i < targets.Count; i++)
                targets[i].oldChild = true;
        }

        /// /////////////////////////////////////////////////////////
        /// Copy
        /// /////////////////////////////////////////////////////////
        
        // Get amount list
        public static void SetRigidDebrisFinalAmount(List<RayfireDebris> targets, BurstType burstType, int burstAmount)
        {
            // No burst
            if (burstType == BurstType.None)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = 0;

            // Same burst amount for every fragment
            if (burstType == BurstType.FragmentAmount)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = burstAmount;

            // Burst amount per particles per fragment size
            else if (burstType == BurstType.PerOneUnitSize)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = (int)(burstAmount * targets[i].rigid.limitations.bboxSize);
            
            // Burst amount by total amount divided among hosts by their amount and size
            else if (burstType == BurstType.TotalAmount)
            {
                // Get sum of all sizes
                float totalSize = 0f;
                for (int i = 0; i < targets.Count; i++)
                    totalSize += targets[i].rigid.limitations.bboxSize;

                // Get size per particle
                float sizePerParticle = totalSize / burstAmount;
               
                // Get size for every host by it's size
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = (int)(targets[i].rigid.limitations.bboxSize / sizePerParticle);
            }
        }
        
        // Get amount list
        public static void SetDustFinalAmount(List<RayfireDust> targets, BurstType burstType, int burstAmount)
        {
            // No burst
            if (burstType == BurstType.None)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = 0;

            // Same burst amount for every fragment
            if (burstType == BurstType.FragmentAmount)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = burstAmount;

            // Burst amount per particles per fragment size
            else if (burstType == BurstType.PerOneUnitSize)
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = (int)(burstAmount * targets[i].rigid.limitations.bboxSize);
            
            // Burst amount by total amount divided among hosts by their amount and size
            else if (burstType == BurstType.TotalAmount)
            {
                // Get sum of all sizes
                float totalSize = 0f;
                for (int i = 0; i < targets.Count; i++)
                    totalSize += targets[i].rigid.limitations.bboxSize;

                // Get size per particle
                float sizePerParticle = totalSize / burstAmount;
               
                // Get size for every host by it's size
                for (int i = 0; i < targets.Count; i++)
                    targets[i].amountFinal = (int)(targets[i].rigid.limitations.bboxSize / sizePerParticle);
            }
        }
        
        // Get amount list
        public static int GetShardFinalAmount(RFShard shard, BurstType burstType, int burstAmount, float sizeSum)
        {
            // No burst
            if (burstType == BurstType.None)
                return 0;

            // Same burst amount for every fragment
            if (burstType == BurstType.FragmentAmount)
                return burstAmount;

            // Burst amount per particles per fragment size
            if (burstType == BurstType.PerOneUnitSize)
                return (int)(burstAmount * shard.sz);
            
            // Burst amount by total amount divided among hosts by their amount and size
            if (burstType == BurstType.TotalAmount)
                return (int)(shard.sz / (sizeSum / burstAmount));

            return 0;
        }
        
        // Check for positive amount TODO not used
        public static bool AmountCheck(RayfireRigid source, int pType)
        {
            // Check debris burst amount
            if (pType == 0)
            {
                for (int i = 0; i < source.debrisList.Count; i++)
                {
                    if (source.debrisList[i].emission.burstType == BurstType.None && source.debrisList[i].emission.distanceRate == 0)
                    {
                        Debug.Log (source.name + " has debris enabled but has no amount");
                        return false;
                    }
                }
            }

            // Check dust burst amount
            if (pType == 1)
            {
                for (int i = 0; i < source.dustList.Count; i++)
                {
                    if (source.dustList[i].emission.burstType == BurstType.None && source.dustList[i].emission.distanceRate == 0)
                    {
                        Debug.Log (source.name + " has dust enabled but has no amount");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}


