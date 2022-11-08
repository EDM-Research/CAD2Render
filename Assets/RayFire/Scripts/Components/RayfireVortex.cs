using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Vortex")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-vortex-component/")]
    public class RayfireVortex : MonoBehaviour
    {

        [HideInInspector] public Transform       transForm;
        [HideInInspector] public Collider[]      colliders;
        [HideInInspector] public List<Rigidbody> rigidbodies   = new List<Rigidbody>();

        Vector3 bot, top;
        Vector3 normal;
        Vector3 direction;
        Vector3 rbPos;
        Vector3 linePoint;
        Vector3 vectorUp;
        Vector3 centerOutVector;
        Vector3 vectorCenter;
        Vector3 perpend;
        Vector3 vectorSwirl;
        Vector3 forceVector;
        float   distancePerpend;
        float   distanceBottom;
        float   upRateNow;
        float   localRadius;
        float   upRateOwn;
        float   centerRateOwn;
        float   centerRateNow;
        float   upRateDif;
        float   centerRateDif;
        float   maxRadius;
        float   axisDistance;
        Plane   bottomPlane;
        float   torqueVal;
        
        public bool    topHandle        = false;
        public Vector3 topAnchor        = new Vector3 (3, 30, 2);
        public Vector3 bottomAnchor     = new Vector3 (0, 0,  0);
        public bool    showGizmo        = true;
        public float   topRadius        = 15f;
        public float   bottomRadius     = 3f;
        public float   eye              = 0.1f;
        public bool    forceByMass      = true;
        public float   stiffness        = 1f;
        public float   swirlStrength    = 10f;
        public bool    enableTorque     = true;
        public float   torqueStrength   = 0.5f;
        public float   torqueVariation  = 0.5f;
        public bool    enableHeightBias = true;
        public float   biasSpeed        = 0.025f;
        public float   biasSpread       = 1;
        public int     seed             = 0;
        public int     circles          = 3;
        public int     mask             = -1;
        public string  tagFilter        = "Untagged";
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Cache variables
            DefineComponents();
        }

        // Cache variables
        void DefineComponents()
        {
            // Cache transform
            transForm = GetComponent<Transform>();

            // Set base length
            colliders = new Collider[10];
        }

        // Main vortex force coroutine
        IEnumerator VortexForceCor()
        {
            yield return new WaitForSeconds (Random.Range (0f, 0.95f));

            while (enabled == true)
            {
                // Set force to rigid bodies
                SetForce();

                yield return new WaitForSeconds (0.066f);
            }
        }

        // Set colliders coroutine
        IEnumerator SetCollidersCor()
        {
            // Set collider gizmo info
            SetColliderGizmo();

            yield return new WaitForSeconds (Random.Range (0f, 0.95f));

            while (enabled == true)
            {
                // Get all colliders inside gizmo
                SetColliders();

                // Set rigid bodies by colliders
                SetRigidBodies();

                yield return new WaitForSeconds (0.3f);
            }
        }

        // Enabling
        private void OnEnable()
        {
            // Set colliders coroutine
            StartCoroutine (SetCollidersCor());

            // Main wind force coroutine
            StartCoroutine (VortexForceCor());
        }

        /// /////////////////////////////////////////////////////////
        /// Setups
        /// /////////////////////////////////////////////////////////

        // Set colliders by range type
        void SetColliders()
        {
            // Get overlaps
            int colAmount = Physics.OverlapCapsuleNonAlloc (bot, top, maxRadius, colliders, mask);

            // Increase array if not enough
            if (colAmount == colliders.Length)
            {
                colliders = new Collider[colAmount + 30];
                Physics.OverlapCapsuleNonAlloc (bot, top, maxRadius, colliders, mask);
            }

            // Decrease array if too much
            if (colliders.Length > colAmount + 50)
            {
                colliders = new Collider[colAmount - 30];
                Physics.OverlapCapsuleNonAlloc (bot, top, maxRadius, colliders, mask);
            }

            // Get overlapped colliders
            // colliders = Physics.OverlapCapsule(bot, top, maxRadius, mask);


            // Occlude
            //if (occluders.Length > 0)
            //{
            //    occluded = Physics.OverlapBox(occluders[0].bounds.center, occluders[0].bounds.extents, occluders[0].transform.rotation);
            //    if (occluded.Length > 0)
            //    {
            //        Debug.Log(occluded.Length);
            //        for (int i = occluded.Length - 1; i >= 0; i--)
            //        {

            //        }
            //    }
            //}
        }

        // Set collider gizmo info
        void SetColliderGizmo()
        {
            bot          = transForm.TransformPoint (bottomAnchor);
            top          = transForm.TransformPoint (topAnchor);
            direction    = top - bot;
            normal       = transform.up;
            axisDistance = topAnchor.y - bottomAnchor.y;
            maxRadius    = topRadius;
            if (bottomRadius > topRadius)
                maxRadius = bottomRadius;
            bottomPlane = new Plane (transform.up, bot);
        }

        // Set rigid bodies by colliders
        void SetRigidBodies()
        {
            rigidbodies.Clear();

            // Collect all rigid bodies in range
            foreach (Collider col in colliders)
            {
                // Missing collider
                if (col == null)
                    continue;

                // Tag filter
                if (tagFilter != "Untagged" && !col.CompareTag (tagFilter))
                    continue;

                // Get attached rigid body
                Rigidbody rb = col.attachedRigidbody;

                // Create projectile if rigid body new. Could be several colliders on one object.
                if (rb != null && rb.isKinematic == false && rigidbodies.Contains (rb) == false)
                    rigidbodies.Add (rb);
            }
        }

        // Apply explosion force, vector and rotation to projectiles
        void SetForce()
        {
            // Set collider gizmo info
            SetColliderGizmo();

            // Set forceMode by mass state
            ForceMode forceMode = ForceMode.Impulse;
            if (forceByMass == false)
                forceMode = ForceMode.VelocityChange;

            // Get str for each object by expl type with variation
            foreach (Rigidbody rb in rigidbodies)
            {
                // Null check
                if (rb == null)
                    continue;

                // Instance id for same random values
                int instanceId = rb.GetInstanceID();

                // Set same random state
                Random.InitState (instanceId + seed);

                // Get position
                rbPos = rb.transform.position;

                // Closest point on axis
                linePoint = GetClosetLinePoint (rbPos); // TODO get on same plane

                // Get distance perpendicular to axis
                distancePerpend = Vector3.Distance (linePoint, rbPos);

                // Get distance to bottom plane
                distanceBottom = bottomPlane.GetDistanceToPoint (linePoint);

                // Current height rate for up bias
                upRateNow = distanceBottom / axisDistance;

                // Below vortex
                if (bottomPlane.GetSide (linePoint) == false)
                    upRateNow = -upRateNow;

                // Get local radius
                localRadius = Mathf.Lerp (bottomRadius, topRadius, upRateNow);

                // Object not in range
                if (localRadius < distancePerpend)
                    continue;

                // Get random height and depth rates
                upRateOwn     = Random.Range (0.03f, 0.97f);
                centerRateOwn = Random.Range (eye,   0.90f);

                // Height bias for upRateOwn
                if (enableHeightBias == true)
                    upRateOwn = HeightBias (upRateOwn, centerRateOwn);

                // Get current depth rate for center bias
                centerRateNow = distancePerpend / localRadius;

                // Get rate differences to correct current position
                upRateDif     = (upRateOwn - upRateNow) * stiffness;
                centerRateDif = (centerRateOwn - centerRateNow) * stiffness;

                // Up vector
                vectorUp = upRateDif * (stiffness + 2f) * normal; // upStr;

                // Normalized vector from axis to rigid object
                centerOutVector = (rbPos - linePoint).normalized;

                // Vector to center
                vectorCenter = Mathf.Abs (swirlStrength) * centerRateDif * centerOutVector;

                // Swirl vector parallel to plane
                perpend     = Vector3.Cross (normal, centerOutVector);
                vectorSwirl = swirlStrength * perpend.normalized;

                // Get final force vector
                forceVector = vectorUp + vectorCenter + vectorSwirl;

                // Apply force
                rb.velocity = Vector3.zero;
                rb.AddForce (forceVector, forceMode);

                // Set rotation impulse TODO add variation
                if (enableTorque == true)
                {
                    torqueVal = (torqueStrength + Random.Range (-torqueVariation, torqueVariation)) * 10f;
                    rb.AddTorque (torqueVal * swirlStrength * Random.Range (0.0f, 1f) * transForm.up, forceMode);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////

        // Get Closest Point On Vortex Axis
        Vector3 GetClosetLinePoint (Vector3 worldPos)
        {
            Vector3 vectorToPos = worldPos - bot;
            Vector3 projection  = Vector3.Project (vectorToPos, direction);
            return projection + bot;
        }

        // Get Point On Vortex Axis with same Y as worldPos
        Vector3 GetParallelLinePoint (Vector3 worldPos)
        {
            // Plane with direction of Y axis and position of worldPos
            // Plane bottomPlane = new Plane(transform.up, worldPos);
            // Mathf.PingPong();

            Vector3 vectorToPos = worldPos - bot;
            Vector3 projection  = Vector3.Project (vectorToPos, direction);
            return projection + bot;
        }

        //
        public static bool LinePlaneIntersection (out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {
            float   length;
            Vector3 vector;
            intersection = Vector3.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            float dotNumerator   = Vector3.Dot (planePoint - linePoint, planeNormal);
            float dotDenominator = Vector3.Dot (lineVec,                planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = SetVectorLength (lineVec, length);

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }

            //output not valid
            return false;
        }

        // Create a vector of direction "vector" with length "size"
        static Vector3 SetVectorLength (Vector3 vector, float size)
        {
            // normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize (vector);

            // scale the vector
            return vectorNormalized *= size;
        }

        // Get height by height bias
        float HeightBias (float upRateOwnLoc, float centerRateOwnLoc)
        {
            if (biasSpread > 0)
            {
                // Get Perlin noise bias
                float perlinVal = Mathf.PerlinNoise (Time.time * biasSpeed * centerRateOwnLoc, upRateOwnLoc) * biasSpread;

                // Get poitive/negative offset
                int biasMult = 1;
                if (Random.value >= 0.5)
                    biasMult = -1;

                // Get local bias
                float biasLocal = perlinVal * biasMult;

                // Adjust up rate
                upRateOwnLoc += biasLocal;

                // Fix upRate
                if (upRateOwnLoc > 1.0f)
                {
                    if (upRateOwnLoc > 2.0f)
                        upRateOwnLoc -= 2f;
                    else
                        upRateOwnLoc = 1f - (upRateOwnLoc - 1.0f);
                }
                else if (upRateOwnLoc < 0.0f)
                {
                    if (upRateOwnLoc < -1.0f)
                        upRateOwnLoc += 2f;
                    else
                        upRateOwnLoc = -upRateOwnLoc;
                }
            }

            return upRateOwnLoc;
        }
    }
}

 
