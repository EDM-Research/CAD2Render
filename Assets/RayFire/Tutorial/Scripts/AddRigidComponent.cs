using UnityEngine;
using RayFire;

public class AddRigidComponent : MonoBehaviour
{

	public GameObject targetObject;
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown ("space") == true)
		{
			if (targetObject != null)
			{
				RayfireRigid rigidComponent = targetObject.AddComponent<RayfireRigid>();
				rigidComponent.simulationType = SimType.Dynamic;
				rigidComponent.demolitionType = DemolitionType.Runtime;
				rigidComponent.objectType     = ObjectType.Mesh;
				rigidComponent.Initialize();
			}
		}


		//ConfigurableJoint cj = new ConfigurableJoint();
		//cj.
		

	}
}
