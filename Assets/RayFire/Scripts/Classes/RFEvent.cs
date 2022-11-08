
using System.Collections.Generic;

namespace RayFire
{
    // Event
    public class RFEvent
    {
        // Rigid Delegate & events
        public delegate void     EventAction(RayfireRigid rigid);
        public event EventAction LocalEvent;
        
        
        // MeshRoot Rigid Delegate & events
        public delegate void         EventActionMeshRoot(RayfireRigid rigid, RayfireRigid meshRoot);
        public event EventActionMeshRoot LocalEventMeshRoot;
        
        
        // RigidRoot Delegate & events
        public delegate void         EventActionRoot(RFShard shard, RayfireRigidRoot root);
        public event EventActionRoot LocalEventRoot;

        // Local Rigid
        public void InvokeLocalEvent(RayfireRigid rigid)
        {
            if (LocalEvent != null)
                LocalEvent.Invoke(rigid);
        }
        
        // Local MeshRoot Rigid
        public void InvokeLocalEventMeshRoot(RayfireRigid rigid, RayfireRigid meshRoot)
        {
            if (LocalEventMeshRoot != null)
                LocalEventMeshRoot.Invoke(rigid, meshRoot);
        }
        
        // Local RigidRoot Shard
        public void InvokeLocalEventRoot(RFShard shard, RayfireRigidRoot rigidRoot)
        {
            if (LocalEventRoot != null)
                LocalEventRoot.Invoke(shard, rigidRoot);
        }
    }
    
    // Demolition Event
    public class RFDemolitionEvent : RFEvent
    {
        // Delegate & events
        public static event EventAction GlobalEvent;
        
        // Demolition event
        public static void InvokeGlobalEvent(RayfireRigid rigid)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(rigid);
        }
    }
    
    // Activation Event
    public class RFActivationEvent : RFEvent
    {
        // Delegate & events
        public static event EventAction     GlobalEvent;
        public static event EventActionRoot GlobalEventRoot;
        
        // Activation event
        public static void InvokeGlobalEvent(RayfireRigid rigid)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(rigid);
        }
        
        // Activation event
        public static void InvokeGlobalEventRoot(RFShard shard, RayfireRigidRoot rigidRoot)
        {
            if (GlobalEventRoot != null)
                GlobalEventRoot.Invoke(shard, rigidRoot);
        }
    }
    
    // Restriction Event
    public class RFRestrictionEvent : RFEvent
    {
        // Delegate & events
        public static event EventAction GlobalEvent;

        // Restriction event
        public static void InvokeGlobalEvent(RayfireRigid rigid)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(rigid);
        }
    }
    
    // Shot Event
    public class RFShotEvent
    {
        // Delegate & events
        public delegate void EventAction(RayfireGun gun);
        public static event EventAction GlobalEvent;
        public event EventAction LocalEvent;
       
        // Global
        public static void InvokeGlobalEvent(RayfireGun gun)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(gun);
        }
        
        // Local
        public void InvokeLocalEvent(RayfireGun gun)
        {
            if (LocalEvent != null)
                LocalEvent.Invoke(gun);
        }
    }

    // Explosion Event
    public class RFExplosionEvent
    {
        // Delegate & events
        public delegate void EventAction(RayfireBomb bomb);
        public static event EventAction GlobalEvent;
        public event EventAction LocalEvent;
       
        // Global
        public static void InvokeGlobalEvent(RayfireBomb bomb)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(bomb);
        }
        
        // Local
        public void InvokeLocalEvent(RayfireBomb bomb)
        {
            if (LocalEvent != null)
                LocalEvent.Invoke(bomb);
        }
    }
    
    // Slice Event
    public class RFSliceEvent
    {
        // Delegate & events
        public delegate void EventAction(RayfireBlade blade);
        public static event EventAction GlobalEvent;
        public event EventAction LocalEvent;
       
        // Global
        public static void InvokeGlobalEvent(RayfireBlade blade)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(blade);
        }
        
        // Local
        public void InvokeLocalEvent(RayfireBlade blade)
        {
            if (LocalEvent != null)
                LocalEvent.Invoke(blade);
        }
    }
    
    // Connectivity Event
    public class RFConnectivityEvent
    {
        // Delegate & events
        public delegate void            EventAction(RayfireConnectivity connectivity, List<RFShard> shards, List<RFCluster> clusters);
        public static event EventAction GlobalEvent;
        public event        EventAction LocalEvent;
       
        // Global
        public static void InvokeGlobalEvent(RayfireConnectivity connectivity, List<RFShard> shards, List<RFCluster> clusters)
        {
            if (GlobalEvent != null)
                GlobalEvent.Invoke(connectivity, shards, clusters);
        }
        
        // Local
        public void InvokeLocalEvent(RayfireConnectivity connectivity, List<RFShard> shards, List<RFCluster> clusters)
        {
            if (LocalEvent != null)
                LocalEvent.Invoke(connectivity, shards, clusters);
        }
    }
}