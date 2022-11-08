namespace RayFire
{
    // Axis type
    public enum AxisType
    {
        XRed   = 0,
        YGreen = 1,
        ZBlue  = 2
    }

    // Plane Type
    public enum PlaneType
    {
        XY = 0,
        XZ = 1,
        YZ = 2
    }

    // Fragmentation Type
    public enum FragType
    {
        Voronoi   = 0,
        Splinters = 1,
        Slabs     = 2,
        Radial    = 3,
        Custom    = 5,
        //Mirrored  = 6,
        Slices    = 7,
        Bricks    = 9,
        Voxels    = 10,
        Tets      = 11,
        Decompose = 15
    }

    // Demolition Type
    public enum DemolitionType
    {
        None                 = 0, // Object not demolished
        Runtime              = 1, // Demolish during runtime
        AwakePrecache        = 2, // Precalculate mesh and pivot array or calculate them in Awake if they are empty
        AwakePrefragment     = 3, // Prefragment and keep fragments disabled as children or prefragment in awake
        ReferenceDemolition  = 9
    }
    
    // Runtime caching Type
    public enum CachingType
    {
        Disable             = 0,
        ByFrames            = 1,
        ByFragmentsPerFrame = 2
    }
    
    // Fade Type
    public enum FadeType
    {
        None         = 0, // Fragments stay as dynamic objects forever
        SimExclude   = 1, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and stay in scene forever
        FallDown     = 2, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation, fall under ground and then destroyed.
        ScaleDown    = 3, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and scale down during fade time, then destroyed
        MoveDown     = 4, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation, move under ground and then destroyed.
        Destroy      = 5, // Fragments stay as dynamic during lifetime/while moved, then destroyed
        SetStatic    = 8, // Fragments stay as dynamic during lifetime/while moved, then set to static
        SetKinematic = 9  // Fragments stay as dynamic during lifetime/while moved, then set to kinematic
        
        // ,Combine      = 11, // Fragments stay as dynamic during lifetime/while moved, then excluded from simulation and stay in scene forever
    }
    
    // Fade life Type
    public enum RFFadeLifeType
    {
        ByLifeTime              = 4,
        BySimulationAndLifeTime = 8
    }

    // Material Type
    public enum MaterialType
    {
        HeavyMetal = 0,
        LightMetal = 1,
        DenseRock  = 2,
        PorousRock = 3,
        Concrete   = 4,
        Brick      = 5,
        Glass      = 6,
        Rubber     = 7,
        Ice        = 8,
        Wood       = 9
    }

    // Mass Type
    public enum MassType
    {
        MaterialDensity    = 0,
        MassProperty       = 1,
        RigidBodyComponent = 2
    }
    
    // Object Type
    public enum ObjectType
    {
        Mesh             = 0,
        MeshRoot         = 1,
        SkinnedMesh      = 2,
        NestedCluster    = 4,
        ConnectedCluster = 5
    }

    // Sim Type
    public enum SimType
    {
        Dynamic   = 0, // Fall down, get affected by other objects
        Sleeping  = 1,
        Inactive  = 2, // Do not fall down, stop impulse
        Kinematic = 3,
        Static    = 4
    }
    
    // Sim Type
    public enum FragSimType
    {
        Dynamic   = 0, 
        Sleeping  = 1,
        Inactive  = 2, 
        Kinematic = 3,
        Inherit   = 4
    }

    // Cluster Type
    public enum ConnectivityType
    {
        ByBoundingBox             = 0,
        ByTriangles               = 1,
        ByPolygons                = 3,
        ByBoundingBoxAndTriangles = 2,
        ByBoundingBoxAndPolygons  = 4,
    }
    
    // Cluster Type
    public enum FragmentMode
    {
        Runtime = 0,
        Editor  = 1
    }
    
    public enum RFColliderType
    {
        Mesh   = 0,
        Box    = 1,
        Sphere = 2,
        None   = 4
    }
    
        
    public enum RFParenting
    {
        Manager   = 0,
        Parent    = 1,
        Sphere = 2,
        None   = 4
    }
}