using RayFire;
using UnityEngine;

public class ApplyDamageScript : MonoBehaviour
{
	public RayfireRigid rigid;
	public float        damageValue = 50f;
	public Transform    damagePoint;
	public float        damageRadius = 2f;
	public Collider     coll; // Optional Connected cluster collider to apply damage to shard
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown ("space") == true)
		{
			if (rigid != null)
			{
				// Get damage position
				Vector3 worldPosition = Vector3.zero;
				if (damagePoint != null)
					worldPosition = damagePoint.position;
				
				// Apply damage
				rigid.ApplyDamage (damageValue, worldPosition, damageRadius, coll);
			}
		}
	}
}
