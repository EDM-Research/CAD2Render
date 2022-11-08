using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu ("RayFire/Rayfire Debris")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/components/unity-debris-component/")]
    public class RayfireDebris : MonoBehaviour
    {
      
        [Header("  Emit Debris")]
                
        public bool onDemolition;
        [Space (1)]
        public bool onActivation;
        [Space (1)]
        public bool onImpact;
        
        [Header("  Main")]
        [Space (3)]
        
        public GameObject debrisReference;
        [Space (2)]
        public Material   debrisMaterial;
        [Space (2)]
        public Material   emissionMaterial;
        
        [Header ("  Properties")]
        [Space (3)]
        
        public RFParticleEmission emission;
        [Space (3)]
        public RFParticleDynamicDebris dynamic;
        [Space (3)]
        public RFParticleNoise noise;
        [Space (3)]
        public RFParticleCollisionDebris collision;
        [Space (3)]
        public RFParticleLimitations limitations;
        [Space (3)]
        public RFParticleRendering rendering;
        
        // Hidden
        [HideInInspector] public RayfireRigid        rigid;
        [HideInInspector] public ParticleSystem      pSystem;
        [HideInInspector] public Transform           hostTm;
        [HideInInspector] public bool                initialized;
        [HideInInspector] public List<Mesh>          debrisMeshList;
        [HideInInspector] public Vector3             refScale = Vector3.one;
        [HideInInspector] public List<RayfireDebris> children;
        [HideInInspector] public int                 amountFinal;
        [HideInInspector] public bool                oldChild;
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Constructor
        public RayfireDebris()
        {
            onDemolition = false;
            onActivation = false;
            onImpact = false;

            debrisReference = null;
            debrisMaterial = null;
            emissionMaterial = null;

            emission    = new RFParticleEmission();
            dynamic     = new RFParticleDynamicDebris();
            noise       = new RFParticleNoise();
            collision   = new RFParticleCollisionDebris();
            limitations = new RFParticleLimitations();
            rendering   = new RFParticleRendering();
            
            // Hidden
            debrisMeshList = new List<Mesh>();
            
            //pSystem = null;
            hostTm = null;
            initialized = false;
            amountFinal = 0;
        }

        // Copy from
        public void CopyFrom(RayfireDebris source)
        {
            onDemolition = source.onDemolition;
            onActivation = source.onActivation;
            onImpact     = source.onImpact;

            debrisReference  = source.debrisReference;
            debrisMaterial   = source.debrisMaterial;
            emissionMaterial = source.emissionMaterial;
            
            emission.CopyFrom (source.emission);
            dynamic.CopyFrom (source.dynamic);
            noise.CopyFrom (source.noise);
            collision.CopyFrom (source.collision);
            limitations.CopyFrom (source.limitations);
            rendering.CopyFrom (source.rendering);

            // Hidden
            debrisMeshList = source.debrisMeshList;
            initialized    = source.initialized;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 
        
        // Initialize
        public void Initialize()
        {
            // Do not initialize if already initialized or parent was ini   tialized
            if (initialized == true)
                return;

            // Has rigid check or transform caching
            
            // TODO AmountCheck(RayfireRigid scrSource, int pType) and xollect if ok
            
            // Set debris ref meshes
            SetReferenceMeshes (debrisReference);
        }
        
        // Emit particles
        public ParticleSystem Emit()
        {
            // Initialize
            Initialize();
            
            // Emitter is not ready
            if (initialized == false)
                return null;

            // Set material properties in case object has no rigid
            collision.SetMaterialProps (this);
            
            // Particle system
            ParticleSystem ps = RFParticles.CreateParticleSystemDebris(this, transform);

            // Get components
            MeshFilter emitMeshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            // Get emit material index
            int emitMatIndex = RFParticles.GetEmissionMatIndex (meshRenderer, emissionMaterial);

            // Set amount
            amountFinal = emission.burstAmount;
            
            // Create debris
            CreateDebris(this, emitMeshFilter, emitMatIndex, ps);

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
        
        // Create single debris particle system
        public void CreateDebris(RayfireDebris scr, MeshFilter emitMeshFilter, int emitMatIndex, ParticleSystem ps)
        {
            // Set main module
            RFParticles.SetMain(ps.main, scr.emission.lifeMin, scr.emission.lifeMax, scr.emission.sizeMin, scr.emission.sizeMax, 
                scr.dynamic.gravityMin, scr.dynamic.gravityMax, scr.dynamic.speedMin, scr.dynamic.speedMax, 
                3.1f, scr.limitations.maxParticles, scr.emission.duration);

            // Emission over distance
            RFParticles.SetEmission(ps.emission, scr.emission.distanceRate, scr.amountFinal);

            // Emission from mesh or from impact point
            if (emitMeshFilter != null)
                RFParticles.SetShapeMesh(ps.shape, emitMeshFilter.sharedMesh, emitMatIndex, emitMeshFilter.transform.localScale);
            else
                RFParticles.SetShapeObject(ps.shape);

            // Inherit velocity 
            RFParticles.SetVelocity(ps.inheritVelocity, scr.dynamic);
            
            // Size over lifetime
            RFParticles.SetSizeOverLifeTime(ps.sizeOverLifetime, scr.refScale);

            // Rotation by speed
            RFParticles.SetRotationBySpeed(ps.rotationBySpeed, scr.dynamic.rotationSpeed);

            // Collision
            RFParticles.SetCollisionDebris(ps.collision, scr.collision);

            // Noise
            RFParticles.SetNoise (ps.noise, scr.noise);
            
            // Renderer
            SetParticleRendererDebris(ps.GetComponent<ParticleSystemRenderer>(), scr);

            // Start playing
            ps.Play();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Renderer
        /// /////////////////////////////////////////////////////////

        // Set renderer
        void SetParticleRendererDebris (ParticleSystemRenderer rend, RayfireDebris scr)
        {
            // Common vars
            rend.renderMode = ParticleSystemRenderMode.Mesh;
            rend.alignment = ParticleSystemRenderSpace.World;
            
            // Set predefined meshes
            if (scr.debrisMeshList.Count > 0)
            {
                if (scr.debrisMeshList.Count <= 4)
                {
                    rend.SetMeshes (scr.debrisMeshList.ToArray());
                    rend.mesh = scr.debrisMeshList[0];
                }
                else
                {
                    List<Mesh> newList = new List<Mesh>();
                    for (int i = 0; i < 4; i++)
                        newList.Add (scr.debrisMeshList[Random.Range (0, scr.debrisMeshList.Count)]);
                    rend.SetMeshes (newList.ToArray());
                    rend.mesh = newList[0];
                }
            }

            // Set material
            rend.sharedMaterial = scr.debrisMaterial;

            // Shadow casting
            rend.shadowCastingMode = scr.rendering.castShadows == true 
                ? UnityEngine.Rendering.ShadowCastingMode.On 
                : UnityEngine.Rendering.ShadowCastingMode.Off;

            // Shadow receiving
            rend.receiveShadows = scr.rendering.receiveShadows;

            // Light probes
            rend.lightProbeUsage = scr.rendering.lightProbes;
        }
        
        // Get reference meshes
        void SetReferenceMeshes(GameObject refs)
        {
            // Local lists
            debrisMeshList = new List<Mesh>();
            
            // No reference. Use own mesh
            if (refs == null)
            {
                Debug.Log ("RayFire Debris: " + gameObject.name + ": Debris reference not defined.", gameObject);
                return;
            }
            
            // Local lists
            List<MeshFilter> mfs = new List<MeshFilter>();

            // Add local mf
            MeshFilter meshFilter = refs.GetComponent<MeshFilter>();
            if (meshFilter != null)
                mfs.Add (meshFilter);

            // Add children mf
            if (refs.transform.childCount > 0)
                mfs.AddRange (refs.GetComponentsInChildren<MeshFilter>().ToList());

            // No mesh filters
            if (mfs.Count == 0)
            {
                Debug.Log ("RayFire Debris: " + gameObject.name + ": Debris reference mesh is not defined.", gameObject);
                return;
            }

            // Get all meshes
            debrisMeshList = (from mf in mfs where mf.sharedMesh != null && mf.sharedMesh.vertexCount > 3 select mf.sharedMesh).ToList();

            // No meshes. Use own mesh
            if (debrisMeshList.Count == 0)
            {
                Debug.Log ("RayFire Debris: " + gameObject.name + ": Debris reference mesh is not defined.", gameObject);
                return;
            }

            // Set debris material
            SetDebrisMaterial (mfs);
            
            // Set scale
            refScale = mfs[0].transform.lossyScale;
            initialized = true;
        }
        
        // Set debris material
        void SetDebrisMaterial(List<MeshFilter> mfs)
        {
            // Already defined
            if (debrisMaterial != null)
                return;
            
            Renderer rend;
            for (int i = 0; i < mfs.Count; i++)
            {
                rend = mfs[i].GetComponent<Renderer>();
                if (rend != null)
                {
                    if (rend.sharedMaterial != null)
                    {
                        debrisMaterial = rend.sharedMaterial;
                        return;
                    }
                }
            }

            // Set original object material
            if (debrisMaterial == null)
                debrisMaterial = GetComponent<Renderer>().sharedMaterial;
        }
        
        public bool HasChildren { get { return children != null && children.Count > 0; } }
    }
}
