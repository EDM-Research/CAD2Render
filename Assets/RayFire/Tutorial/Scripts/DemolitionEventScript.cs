using UnityEngine;

// IMPORTANT! You should add RayFire namespace to use RayFire component's event.
using RayFire;

// Tutorial script. Allows to subscribe to Rigid component demolition.
public class DemolitionEventScript : MonoBehaviour
{
    // Define if script should subscribe to global demolition event
    public bool globalSubscription = false;
    
    // Local Rigid component which will be checked for demolition.
    // You can get RayfireRigid component which you want to check for demolition in any way you want.
    // This is just a tutorial way to define it.
    public bool localSubscription = false;
    public RayfireRigid localRigidComponent;
    
    // /////////////////////////////////////////////////////////
    // Subscribe/Unsubscribe
    // /////////////////////////////////////////////////////////
    
    // Subscribe to event
    void OnEnable()
    {
        // Subscribe to global demolition event. Every demolition will invoke subscribed methods. 
        if (globalSubscription == true)
            RFDemolitionEvent.GlobalEvent += GlobalMethod;
        
        // Subscribe to local demolition event. Demolition of specific Rigid component will invoke subscribed methods. 
        if (localSubscription == true && localRigidComponent != null)
            localRigidComponent.demolitionEvent.LocalEvent += LocalMethod;
    }
    
    // Unsubscribe from event
    void OnDisable()
    {
        // Unsubscribe from global demolition event.
        if (globalSubscription == true)
            RFActivationEvent.GlobalEvent -= GlobalMethod;
        
        // Unsubscribe from local demolition event.
        if (localSubscription == true && localRigidComponent != null)
            localRigidComponent.demolitionEvent.LocalEvent -= LocalMethod;
    }

    // /////////////////////////////////////////////////////////
    // Subscription Methods
    // /////////////////////////////////////////////////////////
    
    // IMPORTANT!. Subscribed method should has following signature.
    // Void return type and one RayfireRigid input parameter.
    // RayfireRigid input parameter is Rigid component which was demolished.
    // In this way you can get demolition data.
   
    // Method for local demolition subscription
    void LocalMethod(RayfireRigid rigid)
    {
        // Show amount of fragments
        Debug.Log("Local demolition: " + rigid.name + " was just demolished and created " + rigid.fragments.Count.ToString() + " fragments");
        
        // Show contact point
        Debug.Log("Contact point: " + rigid.limitations.contactVector3.ToString());
        
        transform.position = rigid.limitations.contactVector3;
    }
    
    // Method for global demolition subscription
    void GlobalMethod(RayfireRigid rigid)
    {
        // Show amount of fragments
        Debug.Log("Global demolition: " + rigid.name + " was just demolished and created " + rigid.fragments.Count.ToString() + " fragments");
        
        // Show contact point
        Debug.Log("Contact point: " + rigid.limitations.contactVector3.ToString());

        transform.position = rigid.limitations.contactVector3;
    }
}
