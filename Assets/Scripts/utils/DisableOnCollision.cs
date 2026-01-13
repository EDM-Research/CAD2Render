using UnityEngine;

public class DisableOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(UnityEngine.Collision collision)
    { 
        if(collision.gameObject.tag == "ExportInstanceInfo")
            collision.gameObject.SetActive(false);
    }

}
