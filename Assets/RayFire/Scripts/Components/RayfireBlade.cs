using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    public class RFSliceData
    {
        public Vector3 planePos;
        public Vector3 planeDir;
        
        public Vector3 swingDir;
        public float   swingStr;
        
        public float force;
        public float damage;
    }
    
    [AddComponentMenu ("RayFire/Rayfire Blade")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-blade-component/")]
    public class RayfireBlade : MonoBehaviour
    {
        public enum CutType
        {
            Enter     = 0,
            Exit      = 1,
            EnterExit = 2
        }
        
        public enum ActionType
        {
            Slice     = 0,
            Demolish  = 1
        }
        
        public ActionType       actionType = ActionType.Slice;
        public CutType          onTrigger  = CutType.Exit;
        public PlaneType        sliceType  = PlaneType.XY;
        public float            force      = 1f;
        public bool             affectInactive;
        public float            damage;
        public bool             skin;
        public float            cooldown  = 2f;
        public int              mask      = -1;
        public string           tagFilter = "Untagged";
        public List<GameObject> targets;
        public RayfireRigid     rigid;
        public Transform        transForm;
        public Vector3[]        enterPlane;
        public Vector3[]        exitPlane;
        public Collider         colLider;
        public Vector3[]        slicePlanes;
        public bool             coolDownState;
        RFSliceData             sliceData;
        Vector3                 posEnter;
        Vector3                 posExit;
        
        // Event
        public RFSliceEvent sliceEvent = new RFSliceEvent();
        
        // check if one slice creates two halfs in one take
        // do not precap, but slice with cap (precap:true, removeCap:true)
        // plane to bound intersection check first `Plane.GetSide`.
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Set components
            DefineComponents();
        }
        
        // Define components
        void DefineComponents()
        {
            transForm = GetComponent<Transform>();
            
            // Check collider
            colLider = GetComponent<Collider>();

            // No collider. Add own
            if (colLider == null)
                colLider = gameObject.AddComponent<MeshCollider>();
                
            // Set convex for mesh collider
            if (colLider is MeshCollider)
                ((MeshCollider)colLider).convex = true;

            // Set trigger state
            colLider.isTrigger = true;
            
            // Set rigidbody for skinned mesh
            if (skin == true)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb             = gameObject.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity  = false;
                }
            }
            
            coolDownState      = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Triggers
        /// /////////////////////////////////////////////////////////
        
        // Check for trigger
        void OnTriggerEnter (Collider col)
        {
            TriggerEnter (col);
        }

        // Exit trigger
        void OnTriggerExit (Collider col)
        {
            TriggerExit (col);
        }
        
        // Trigger enter
        void TriggerEnter (Collider col)
        {
            // Save enter position
            posEnter = transForm.position;
            
            // Enter
            if (onTrigger == CutType.Enter)
            {
                
                if (actionType == ActionType.Slice)
                    Slice (col.gameObject, GetSlicePlane());
                else
                    Demolish (col.gameObject);
            }

            // Remember enter plane
            else if (onTrigger == CutType.EnterExit)
            {
                // Set enter plane
                if (actionType == ActionType.Slice)
                    enterPlane = GetSlicePlane();
            }
        }
        
        // Trigger exit
        void TriggerExit (Collider col)
        {
            // Save exit position
            posExit = transForm.position;
            
            // Exit
            if (onTrigger == CutType.Exit)
            {
                if (actionType == ActionType.Slice)
                    Slice (col.gameObject, GetSlicePlane());
                else
                    Demolish (col.gameObject);
            }

            // Remember exit plane and calculate average plane
            else if (onTrigger == CutType.EnterExit)
            {
                if (actionType == ActionType.Slice)
                {
                    // Get exit plane
                    exitPlane = GetSlicePlane();
                    
                    // Get slice plane by enter plane and exit plane
                    Vector3[] slicePlane = new Vector3[2];
                    slicePlane[0] = (enterPlane[0] + exitPlane[0]) / 2f;
                    slicePlane[1] = (enterPlane[1] + exitPlane[1]) / 2f;

                    // Slice
                    Slice (col.gameObject, slicePlane);
                }
                else
                    Demolish (col.gameObject);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////
        
        // Demolish
        void Demolish(GameObject targetObject)
        {
            // Filter check
            if (FilterCheck(targetObject) == false)
                return;
            
            // Get RayFire script
            rigid = targetObject.GetComponent<RayfireRigid>();

            // No Rayfire Rigid script
            if (rigid == null)
                return;

            // No demolition allowed
            if (rigid.demolitionType == DemolitionType.None)
                return;
 
            // Available for demolition
            if (rigid.State() == false)
                return;
            
            // Apply damage
            if (ApplyDamage (rigid, damage) == false)
                return;
            
            // Start Cooldown
            StartCoroutine (CooldownCor());
            
            // Demolish
            rigid.limitations.demolitionShould = true;;
        }

        /// /////////////////////////////////////////////////////////
        /// Cooldown
        /// /////////////////////////////////////////////////////////
        
        // Cache physics data for fragments 
        IEnumerator CooldownCor ()
        {
            if (cooldown > 0 && coolDownState == false)
            {
                SetCooldown(true);
                yield return new WaitForSeconds (cooldown);
                SetCooldown(false);
            }
        }

        // Set cooldown state
        void SetCooldown(bool state)
        {
            coolDownState = state;
        }

        // Filter check
        bool FilterCheck(GameObject targetObject)
        {
            // Cooldown check
            if (coolDownState == true)
                return false;

            // Check tag
            if (tagFilter != "Untagged" && !targetObject.CompareTag (tagFilter))
                return false;

            // Check layer
            if (LayerCheck (targetObject.layer) == false)
                return false;
            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Slicing
        /// /////////////////////////////////////////////////////////
        
        // Slice target
        public void SliceTarget()
        {
            if (targets != null && targets.Count > 0)
                for (int i = 0; i < targets.Count; i++)
                    if (targets[i] != null)
                        Slice (targets[i], GetSlicePlane());
        }
        
        // Slice collider by blade
        void Slice (GameObject targetObject, Vector3[] slicePlane)
        {
            // Filter check
            if (FilterCheck(targetObject) == false)
                return;
            
            // Get RayFire script
            rigid = targetObject.GetComponent<RayfireRigid>();
            
            // No Rayfire Rigid script
            if (rigid == null)
                return;
            
            // No demolition allowed
            if (rigid.demolitionType == DemolitionType.None)
                return;
            
            // Object can't be cut
            if (rigid.limitations.sliceByBlade == false)
                return;
            
            // Global demolition state check
            if (rigid.State() == false)
                return;
            
            // Apply damage
            if (damage > 0)
                if (ApplyDamage (rigid, damage) == false)
                    return;

            // Set slice data
            sliceData   = GetSliceData();
            slicePlanes = slicePlane;
            
            
            
            // Slice object
            rigid.AddSlicePlane (slicePlane);
            
            
            
            // Set slice force
            if (force > 0)
            {
                rigid.limitations.sliceForce     = force;
                rigid.limitations.affectInactive = affectInactive;
            }

            // Event
            sliceEvent.InvokeLocalEvent (this);
            RFSliceEvent.InvokeGlobalEvent (this);
            
            // Start Cooldown
            StartCoroutine (CooldownCor());
        }

        // Apply damage and return True if damage limit reached 
        bool ApplyDamage(RayfireRigid scr, float damageValue)
        {
            // Damage collection disabled
            if (scr.damage.enable == false)
                return true;

                // No damage
            if (damageValue == 0)
                return false;

            // Add damage 
            return RFDamage.ApplyToRigid (scr, damageValue);
        }
        
        // Get two points or slice
        Vector3[] GetSlicePlane()
        {
            // Get position and normal
            Vector3[] points = new Vector3[2];
            points[0] = transForm.position;

            // Slice plane direction
            if (sliceType == PlaneType.XY)
                points[1] = transForm.forward;
            else if (sliceType == PlaneType.XZ)
                points[1] = transForm.up;
            else if (sliceType == PlaneType.YZ)
                points[1] = transForm.right;

            return points;
        }
        
        // Get two points or slice
        RFSliceData GetSliceData()
        {
            RFSliceData data = new RFSliceData();

            // Plane position and direction
            data.planePos = transForm.position;
            if (sliceType == PlaneType.XY)
                data.planeDir = transForm.forward;
            else if (sliceType == PlaneType.XZ)
                data.planeDir = transForm.up;
            else if (sliceType == PlaneType.YZ)
                data.planeDir = transForm.right;


            // Swing direction and strength
            data.swingDir = (posExit - posEnter).normalized;
            data.swingStr = (posExit - posEnter).magnitude;
                
            // Blade props
            data.force  = force;
            data.damage = damage;
            
            return data;
        }
        
        public bool HasTargets { get { return targets != null && targets.Count > 0; } }
        
        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Check if object layer is in layer mask
        bool LayerCheck (int layerId)
        {
            //// Layer mask check
            //LayerMask layerMask = new LayerMask();
            //layerMask.value = mask;
            //if (LayerCheck(projectile.rb.gameObject.layer, layerMask) == true)
            //    Debug.Log("In mask " + projectile.rb.name);
            //else
            //    Debug.Log("Not In mask " + projectile.rb.name);
            return mask == (mask | (1 << layerId));
        }

    }
}
