using System.Collections.Generic;
using UnityEngine;

// IMPORTANT! You should use RayFire namespace to use RayFire component's event.
using RayFire;

// Tutorial script. Allows to subscribe to Connectivity component activation.
public class ConnectivityEventScript : MonoBehaviour
{
    // Define if script should subscribe to global Connectivity activation event
    public bool globalSubscription = false;
    
    // Local Connectivity component which will be checked for activation.
    // You can get RayfireConnectivity component which you want to check for activation in any way you want.
    // This is just a tutorial way to define it.
    public bool                localSubscription = false;
    public RayfireConnectivity  localConnectivityComponent;
    
    // /////////////////////////////////////////////////////////
    // Subscribe/Unsubscribe
    // /////////////////////////////////////////////////////////
    
    // Subscribe to event
    void OnEnable()
    {
        // Subscribe to global Connectivity activation event. Every activation will invoke subscribed methods. 
        if (globalSubscription == true)
            RFConnectivityEvent.GlobalEvent += GlobalMethodConnectivity;
        
        // Subscribe to local activation event. Activation in specific Connectivity component will invoke subscribed methods. 
        if (localSubscription == true && localConnectivityComponent != null)
            localConnectivityComponent.connectivityEvent.LocalEvent += LocalMethodConnectivity;
    }
    
    // Unsubscribe from event
    void OnDisable()
    {
        // Unsubscribe from global Connectivity activation event.
        if (globalSubscription == true)
            RFConnectivityEvent.GlobalEvent -= GlobalMethodConnectivity;
        
        // Unsubscribe from local Connectivity activation event.
        if (localSubscription == true && localConnectivityComponent != null)
            localConnectivityComponent.connectivityEvent.LocalEvent -= LocalMethodConnectivity;
    }

    // /////////////////////////////////////////////////////////
    // Subscription Methods
    // /////////////////////////////////////////////////////////
    
    // IMPORTANT!. Subscribed method should has following signature.
    // Void return type and one RayfireConnectivity input parameter.
    // RayfireConnectivity input parameter is Connectivity component which was activated.
   
    // Method for local activation subscription
    void LocalMethodConnectivity(RayfireConnectivity connectivity, List<RFShard> shards, List<RFCluster> clusters)
    {
        Debug.Log("Local Connectivity activation: " + connectivity.name + " has " + connectivity.AmountIntegrity + " % Integrity");

        if (shards != null & shards.Count > 0)
            Debug.Log("Local Connectivity activation: " + shards.Count + " shards were activated.");
        if (clusters != null & clusters.Count > 0)
            Debug.Log("Local Connectivity activation: " + clusters.Count + " clusters were activated.");
    }
    
    // Method for global activation subscription
    void GlobalMethodConnectivity(RayfireConnectivity connectivity, List<RFShard> shards, List<RFCluster> clusters)
    {
        Debug.Log("Global Connectivity activation: "+ connectivity.name + " has " + connectivity.AmountIntegrity + " % Integrity");
        
        if (shards != null & shards.Count > 0)
            Debug.Log("Global Connectivity activation: " + shards.Count + " shards were activated.");
        if (clusters != null & clusters.Count > 0)
            Debug.Log("Global Connectivity activation: " + clusters.Count + " clusters were activated.");
    }
}
