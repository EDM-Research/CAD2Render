using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu ("RayFire/Rayfire Dust")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/components/unity-dust-component/")]
    public class RayfireDust : MonoBehaviour
    {
        [Header("  Emit Dust")]
                
        public bool onDemolition;
        [Space (1)]
        public bool onActivation;
        [Space (1)]
        public bool onImpact;

        [Header("  Main")]
        [Space (3)]

        [Range(0.01f, 1f)] public float opacity;
        [Space (2)]
        public Material dustMaterial;
        
        [Space (2)]
        public List<Material> dustMaterials;
        
        [Space (2)]
        public Material emissionMaterial;
        
        [Header("  Properties")]
        [Space (3)]

        public RFParticleEmission emission;
        [Space (2)]
        public RFParticleDynamicDust dynamic;
        [Space (2)]
        public RFParticleNoise noise;
        [Space (2)]
        public RFParticleCollisionDust collision;
        [Space (2)]
        public RFParticleLimitations limitations;
        [Space (2)]
        public RFParticleRendering rendering;
        
        // Hidden
        [HideInInspector] public RayfireRigid rigid;
        [HideInInspector] public ParticleSystem pSystem = null;
        [HideInInspector] public Transform hostTm   = null;
        [HideInInspector] public bool initialized;
        [HideInInspector] public List<RayfireDust> children;
        [HideInInspector] public int amountFinal;
        [HideInInspector] public bool oldChild;
        
        // auto alpha fade
        // few dust textures with separate alphas

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RayfireDust()
        {
            onDemolition = false;
            onActivation = false;
            onImpact = false;  
            
            dustMaterial = null;
            opacity = 0.25f;
            emissionMaterial = null;

            emission    = new RFParticleEmission();
            dynamic     = new RFParticleDynamicDust();
            noise       = new RFParticleNoise();
            collision   = new RFParticleCollisionDust();
            limitations = new RFParticleLimitations();
            rendering   = new RFParticleRendering();
            
            // Hidden
            //pSystem = null;
            hostTm = null;
            initialized = false;
            amountFinal = 5;
        }

        // Copy from
        public void CopyFrom(RayfireDust source)
        {
            onDemolition = source.onDemolition;
            onActivation = source.onActivation;
            onImpact     = source.onImpact;

            opacity          = source.opacity;
            dustMaterial     = source.dustMaterial;
            dustMaterials    = source.dustMaterials;
            emissionMaterial = source.emissionMaterial;
            
            emission.CopyFrom (source.emission);
            dynamic.CopyFrom (source.dynamic);
            noise.CopyFrom (source.noise);
            collision.CopyFrom (source.collision);
            limitations.CopyFrom (source.limitations);
            rendering.CopyFrom (source.rendering);
            
            initialized = source.initialized;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 

        // Initialize
        public void Initialize()
        {
            // TODO AmountCheck(RayfireRigid scrSource, int pType) and collect if ok

            // Remove null materials
            if (HasMaterials == true)
                for (int i = dustMaterials.Count - 1; i >= 0; i--)
                    if (dustMaterials[i] == null)
                        dustMaterials.RemoveAt (i);
            
            // No material
            if (dustMaterial == null && HasMaterials == false)
            {
                Debug.Log (gameObject.name + ": Dust material not defined.", gameObject);
                initialized = false;
                return;
            }
            
            initialized = true;
        }
        
        // Emit particles
        public ParticleSystem Emit()
        {
            // Initialize
            Initialize();
            
            // Emitter is not ready
            if (initialized == false)
                return null;
            
            // Particle system
            ParticleSystem ps = RFParticles.CreateParticleSystemDust(this, transform);

            // Get components
            MeshFilter emitMeshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            // Get emit material index
            int emitMatIndex = RFParticles.GetEmissionMatIndex (meshRenderer, emissionMaterial);
            
            // Set amount
            amountFinal = emission.burstAmount;
            
            // Create debris
            CreateDust(this, emitMeshFilter, emitMatIndex, ps);

            return ps;
        }
        
        // Clean particle systems
        public void Clean()
        {
            // Destroy own particles
            if (hostTm != null)
                Destroy (hostTm.gameObject);

            // Destroy particles on children debris
            if (HasChildren == true)
                for (int i = 0; i < children.Count; i++)
                    if (children[i] != null)
                        if (children[i].hostTm != null)
                            Destroy (children[i].hostTm.gameObject);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Create common
        /// /////////////////////////////////////////////////////////

        // Create single dust particle system
        public void CreateDust(RayfireDust scr, MeshFilter emitMeshFilter, int emitMatIndex, ParticleSystem ps)
        {
            // Set main module
            RFParticles.SetMain(ps.main, scr.emission.lifeMin, scr.emission.lifeMax, scr.emission.sizeMin, scr.emission.sizeMax, 
                scr.dynamic.gravityMin, scr.dynamic.gravityMax, scr.dynamic.speedMin, scr.dynamic.speedMax, 
                6f, scr.limitations.maxParticles, scr.emission.duration);

            // Emission over distance
            RFParticles.SetEmission(ps.emission, scr.emission.distanceRate, (short)scr.amountFinal);
            
            // Emission from mesh or from impact point
            if (emitMeshFilter != null)
                RFParticles.SetShapeMesh(ps.shape, emitMeshFilter.sharedMesh, emitMatIndex, emitMeshFilter.transform.localScale);
            else
                RFParticles.SetShapeObject(ps.shape);
            
            // Collision
            RFParticles.SetCollisionDust(ps.collision,  scr.collision);

            // Color over life time
            RFParticles.SetColorOverLife(ps.colorOverLifetime, scr.opacity);

            // Rotation over lifetime
            RFParticles.SetRotationOverLifeTime (ps.rotationOverLifetime, scr.dynamic);
            
            // Noise
            RFParticles.SetNoise(ps.noise, scr.noise);

            // Renderer
            SetParticleRendererDust(ps.GetComponent<ParticleSystemRenderer>(), scr);
            
            // Start playing
            ps.Play();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////
        
        // Set renderer
        void SetParticleRendererDust(ParticleSystemRenderer rend, RayfireDust scr)
        {
            // Common vars
            rend.renderMode = ParticleSystemRenderMode.Billboard;
            rend.alignment = ParticleSystemRenderSpace.World;
            rend.normalDirection = 1f;

            // Set material
            if (scr.dustMaterials != null && scr.dustMaterials.Count > 0)
            {
                int id = Random.Range (0, scr.dustMaterials.Count);
                rend.sharedMaterial = scr.dustMaterials[id];
            }
            else
                rend.sharedMaterial = scr.dustMaterial;

            // Shadow casting
            rend.shadowCastingMode = scr.rendering.castShadows == true 
                ? UnityEngine.Rendering.ShadowCastingMode.On 
                : UnityEngine.Rendering.ShadowCastingMode.Off;

            // Shadow receiving
            rend.receiveShadows = scr.rendering.receiveShadows;

            // Dust vars
            rend.sortMode = ParticleSystemSortMode.OldestInFront;
            rend.minParticleSize = 0.0001f;
            rend.maxParticleSize = 999999f;
            rend.alignment = ParticleSystemRenderSpace.Facing;

            // Light probes
            rend.lightProbeUsage = scr.rendering.lightProbes;

            // Set Roll in 2018.3 and older builds TODO
            //           if (Application.unityVersion == "2018.3.0f2")
            //               renderer.shadowBias = 0.55f;
            //               renderer.allowRoll = false;
        }
        
        // Set material
        void SetMaterialDust(ParticleSystemRenderer rend, List<Material> mats)
        {
            // No material
            if (mats.Count == 0)
            {
                Debug.Log("Define dust material");
                return;
            }

            // Set material
            if (mats.Count == 1)
                rend.sharedMaterial = mats[0];
            else
                rend.sharedMaterial = mats[Random.Range(0, mats.Count - 1)];
        }
        
        public bool HasChildren { get { return children != null && children.Count > 0; } }
        public bool HasMaterials { get { return dustMaterials != null && dustMaterials.Count > 0; } }
    }
}