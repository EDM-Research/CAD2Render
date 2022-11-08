using UnityEngine;

// IMPORTANT! You should use RayFire namespace to use RayFire component's event.
using RayFire;

// Tutorial script. Allows to subscribe to Rigid component activation.
public class ActivationEventScript : MonoBehaviour
{
    
    [Header ("  Rigid Activation")]
    
    // Define if script should subscribe to global activation event
    public bool globalSubscriptionRigid = false;
    
    // Local Rigid component which will be checked for activation.
    // You can get RayfireRigid component which you want to check for activation in any way you want.
    // This is just a tutorial way to define it.
    public bool localSubscriptionRigid = false;
    public RayfireRigid localRigidComponent;
    
    
    
    [Header ("  MeshRoot Activation")]
    
    // Local Rigid component which will be checked for activation.
    // You can get RayfireRigid component which you want to check for activation in any way you want.
    // This is just a tutorial way to define it.
    public bool         localSubscriptionMeshRoot = false;
    public RayfireRigid localMeshRootComponent;
    
    
    
    [Header ("  RigidRoot Activation")]
    
    // Define if script should subscribe to global activation event
    public bool globalSubscriptionRoot = false;
    
    // Local RigidRoot component which will be checked for shard activation.
    // You can get RayfireRigidRoot component which you want to check for activation in any way you want.
    // This is just a tutorial way to define it.
    public bool         localSubscriptionRoot = false;
    public RayfireRigidRoot localRigidRootComponentRoot;
    
    // /////////////////////////////////////////////////////////
    // Subscribe/Unsubscribe
    // /////////////////////////////////////////////////////////
    
    // Subscribe to event
    void OnEnable()
    {
        //// Rigid subscription methods
        
        // Subscribe to global activation event. Every activation will invoke subscribed methods. 
        if (globalSubscriptionRigid == true)
            RFActivationEvent.GlobalEvent += GlobalMethodRigid;
        
        // Subscribe to local activation event. Activation of specific Rigid component will invoke subscribed methods. 
        if (localSubscriptionRigid == true && localRigidComponent != null)
            localRigidComponent.activationEvent.LocalEvent += LocalMethodRigid;
        
        //// MeshRoot Rigid subscription methods
        
        // Subscribe to local activation event. Activation of specific Rigid component will invoke subscribed methods. 
        if (localSubscriptionMeshRoot == true && localMeshRootComponent != null)
            localMeshRootComponent.activationEvent.LocalEventMeshRoot += LocalMethodMeshRoot;
        
        //// RigidRoot subscription methods
        
        // Subscribe to global activation event. Every activation will invoke subscribed methods. 
        if (globalSubscriptionRoot == true)
            RFActivationEvent.GlobalEventRoot += GlobalMethodRoot;
        
        // Subscribe to local activation event. Activation of specific Rigid component will invoke subscribed methods. 
        if (localSubscriptionRoot == true && localRigidRootComponentRoot != null)
            localRigidRootComponentRoot.activationEvent.LocalEventRoot += LocalMethodRoot;
    }
    
    // Unsubscribe from event
    void OnDisable()
    {
        //// Rigid unsubscribe methods
        
        // Unsubscribe from global activation event.
        if (globalSubscriptionRigid == true)
            RFActivationEvent.GlobalEvent -= GlobalMethodRigid;
        
        // Unsubscribe from local activation event.
        if (localSubscriptionRigid == true && localRigidComponent != null)
            localRigidComponent.activationEvent.LocalEvent -= LocalMethodRigid;
        
        //// MeshRoot Rigid subscription methods
        
        // Unsubscribe from local activation event.
        if (localSubscriptionMeshRoot == true && localMeshRootComponent != null)
            localMeshRootComponent.activationEvent.LocalEventMeshRoot -= LocalMethodMeshRoot;
        
        //// RigidRoot unsubscribe methods
        
        // Unsubscribe from global activation event.
        if (globalSubscriptionRoot == true)
            RFActivationEvent.GlobalEventRoot -= GlobalMethodRoot;
        
        // Unsubscribe from local activation event.
        if (localSubscriptionRoot == true && localRigidRootComponentRoot != null)
            localRigidRootComponentRoot.activationEvent.LocalEventRoot -= LocalMethodRoot;
    }

    // /////////////////////////////////////////////////////////
    // Subscription Methods
    // /////////////////////////////////////////////////////////
    
    // IMPORTANT!. Subscribed method should has following signature.
    // Void return type and one RayfireRigid/RayfireRigidRoot input parameter.
    // RayfireRigid/RayfireRigidRoot input parameter is Rigid/RigidRoot component which was activated.
    // In this way you can get activation data.
   
    //// Rigid methods
    
    // Method for local activation subscription
    void LocalMethodRigid(RayfireRigid rigid)
    {
        Debug.Log("Local Rigid activation: " + rigid.name + " was just activated");
    }
    
    // Method for global activation subscription
    void GlobalMethodRigid(RayfireRigid rigid)
    {
        Debug.Log("Global Rigid activation: " + rigid.name + " was just activated");
    }
    
    //// MeshRoot methods
    
    // Method for local activation subscription
    void LocalMethodMeshRoot(RayfireRigid rigid, RayfireRigid meshRoot)
    {
        Debug.Log("Local MeshRoot activation: " + rigid.name + " was just activated");
        Debug.Log("Local MeshRoot activation: " + meshRoot.name + " is its MeshRoot parent");
    }
    
    //// RigidRoot methods
    
    // Method for local activation subscription
    void LocalMethodRoot(RFShard shard, RayfireRigidRoot root)
    {
        Debug.Log("Local RigidRoot activation: " + shard.tm.name + " was just activated");
        Debug.Log("Local RigidRoot activation: " + root.name + " is its RigidRoot parent");
       
    }
    
    // Method for global activation subscription
    void GlobalMethodRoot(RFShard shard, RayfireRigidRoot root)
    {
        Debug.Log("Local RigidRoot activation: " + shard.tm.name + " was just activated");
        Debug.Log("Local RigidRoot activation: " + root.name + " is its RigidRoot parent");
    }
    
    // Method for local activation subscription
    void LocalMethodRoot(RayfireRigid rigid, RayfireRigidRoot root)
    {
        Debug.Log("Local RigidRoot activation: " + rigid.name + " was just activated");
        Debug.Log("Local RigidRoot activation: " + root.name + " is its RigidRoot parent");
       
    }
    
    // Method for global activation subscription
    void GlobalMethodRoot(RayfireRigid rigid, RayfireRigidRoot root)
    {
        Debug.Log("Local RigidRoot activation: " + rigid.name + " was just activated");
        Debug.Log("Local RigidRoot activation: " + root.name + " is its RigidRoot parent");
    }
    
    
}
