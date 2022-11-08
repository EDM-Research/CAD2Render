using System;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFMaterial
    {
        private string name;
        public  bool   destructible;
        public int solidity;
        public float density;
        public float drag;
        public float angularDrag;
        public PhysicMaterial material;
        public float dynamicFriction;
        public float staticFriction;
        public float bounciness;
        
        public RFMaterial(
            string Name, 
            float Density, 
            float Drag, 
            float AngularDrag, 
            int Solidity, 
            bool Destructible, 
            float DynFriction,
            float StFriction, 
            float Bounce)
        {
            name            = Name;
            density         = Density;
            drag            = Drag;
            angularDrag     = AngularDrag;
            solidity        = Solidity;
            destructible    = Destructible;
            dynamicFriction = DynFriction;
            staticFriction  = StFriction;
            bounciness      = Bounce;
        }

        // Get Physic material
        public PhysicMaterial Material
        {
            get
            {
                PhysicMaterial physMat = new PhysicMaterial();
                physMat.name = name;
                physMat.dynamicFriction = dynamicFriction;
                physMat.staticFriction = staticFriction;
                physMat.bounciness = bounciness;
                physMat.frictionCombine = PhysicMaterialCombine.Minimum;
                return physMat;
            }
        }
    }

    // Material presets
    [Serializable]
    public class RFMaterialPresets
    {
        public RFMaterial heavyMetal;
        public RFMaterial lightMetal;
        public RFMaterial denseRock;
        public RFMaterial porousRock;
        public RFMaterial concrete;
        public RFMaterial brick;
        public RFMaterial glass;
        public RFMaterial rubber;
        public RFMaterial ice;
        public RFMaterial wood;
        
        public RFMaterialPresets()
        {
            heavyMetal = new RFMaterial ("HeavyMetal", 11f,  0f, 0.05f, 80, false, 0.75f, 0.7f,  0.17f);
            lightMetal = new RFMaterial ("LightMetal", 8f,   0f, 0.05f, 50, false, 0.71f, 0.72f, 0.14f);
            denseRock  = new RFMaterial ("DenseRock",  4f,   0f, 0.05f, 22, true,  0.88f, 0.87f, 0.14f);
            porousRock = new RFMaterial ("PorousRock", 2.5f, 0f, 0.05f, 12, true,  0.84f, 0.82f, 0.16f);
            concrete   = new RFMaterial ("Concrete",   3f,   0f, 0.05f, 18, true,  0.81f, 0.83f, 0.15f);
            brick      = new RFMaterial ("Brick",      2.3f, 0f, 0.05f, 10, true,  0.76f, 0.75f, 0.13f);
            glass      = new RFMaterial ("Glass",      1.8f, 0f, 0.05f, 3,  true,  0.53f, 0.53f, 0.2f);
            rubber     = new RFMaterial ("Rubber",     1.4f, 0f, 0.05f, 1,  false, 0.95f, 0.98f, 0.93f);
            ice        = new RFMaterial ("Ice",        1f,   0f, 0.05f, 2,  true,  0.07f, 0.07f, 0f);
            wood       = new RFMaterial ("Wood",       0.7f, 0f, 0.05f, 4,  true,  0.75f, 0.73f, 0.22f);
        }

        // Create physic material if it was not applied by user
        public void SetMaterials()
        {
            if (heavyMetal.material == null)
                heavyMetal.material = heavyMetal.Material;
            if (lightMetal.material == null)
                lightMetal.material = lightMetal.Material;
            if (denseRock.material == null)
                denseRock.material = denseRock.Material;
            if (porousRock.material == null)
                porousRock.material = porousRock.Material;
            if (concrete.material == null)
                concrete.material = concrete.Material;
            if (brick.material == null)
                brick.material = brick.Material;
            if (glass.material == null)
                glass.material = glass.Material;
            if (rubber.material == null)
                rubber.material = rubber.Material;
            if (ice.material == null)
                ice.material = ice.Material;
            if (wood.material == null)
                wood.material = wood.Material;
        }

        // Get density by material Type
        public float Density (MaterialType materialType)
        {
            if (materialType == MaterialType.Concrete)
                return concrete.density;
            if (materialType == MaterialType.Brick)
                return brick.density;
            if (materialType == MaterialType.Glass)
                return glass.density;
            if (materialType == MaterialType.Rubber)
                return rubber.density;
            if (materialType == MaterialType.Ice)
                return ice.density;
            if (materialType == MaterialType.Wood)
                return wood.density;
            
            if (materialType == MaterialType.HeavyMetal)
                return heavyMetal.density;
            if (materialType == MaterialType.LightMetal)
                return lightMetal.density;
            if (materialType == MaterialType.DenseRock)
                return denseRock.density;
            if (materialType == MaterialType.PorousRock)
                return porousRock.density;
            return 2f;
        }
        
        // Get Drag by material Type
        public float Drag (MaterialType materialType)
        {
            if (materialType == MaterialType.Concrete)
                return concrete.drag;
            if (materialType == MaterialType.Brick)
                return brick.drag;
            if (materialType == MaterialType.Glass)
                return glass.drag;
            if (materialType == MaterialType.Rubber)
                return rubber.drag;
            if (materialType == MaterialType.Ice)
                return ice.drag;
            if (materialType == MaterialType.Wood)
                return wood.drag;
            if (materialType == MaterialType.HeavyMetal)
                return heavyMetal.drag;
            if (materialType == MaterialType.LightMetal)
                return lightMetal.drag;
            if (materialType == MaterialType.DenseRock)
                return denseRock.drag;
            if (materialType == MaterialType.PorousRock)
                return porousRock.drag;
            return 0f;
        }
        
        // Get AngularDrag by material Type
        public float AngularDrag (MaterialType materialType)
        {
            if (materialType == MaterialType.Concrete)
                return concrete.angularDrag;
            if (materialType == MaterialType.Brick)
                return brick.angularDrag;
            if (materialType == MaterialType.Glass)
                return glass.angularDrag;
            if (materialType == MaterialType.Rubber)
                return rubber.angularDrag;
            if (materialType == MaterialType.Ice)
                return ice.angularDrag;
            if (materialType == MaterialType.Wood)
                return wood.angularDrag;
            if (materialType == MaterialType.HeavyMetal)
                return heavyMetal.angularDrag;
            if (materialType == MaterialType.LightMetal)
                return lightMetal.angularDrag;
            if (materialType == MaterialType.DenseRock)
                return denseRock.angularDrag;
            if (materialType == MaterialType.PorousRock)
                return porousRock.angularDrag;
            return 0.05f;
        }

        // Get solidity by material type
        public int Solidity (MaterialType materialType)
        {
            int solid = 1;
            if (materialType == MaterialType.Concrete)
                return concrete.solidity;
            if (materialType == MaterialType.Brick)
                return brick.solidity;
            if (materialType == MaterialType.Glass)
                return glass.solidity;
            if (materialType == MaterialType.Rubber)
                return rubber.solidity;
            if (materialType == MaterialType.Ice)
                return ice.solidity;
            if (materialType == MaterialType.Wood)
                return wood.solidity;
            if (materialType == MaterialType.HeavyMetal)
                return heavyMetal.solidity;
            if (materialType == MaterialType.LightMetal)
                return lightMetal.solidity;
            if (materialType == MaterialType.DenseRock)
                return denseRock.solidity;
            if (materialType == MaterialType.PorousRock)
                return porousRock.solidity;
            return solid;
        }
        
        // Get destructible by material type
        public bool Destructible (MaterialType materialType)
        {
            if (materialType == MaterialType.HeavyMetal)
                return heavyMetal.destructible;
            if (materialType == MaterialType.LightMetal)
                return lightMetal.destructible;
            if (materialType == MaterialType.DenseRock)
                return denseRock.destructible;
            if (materialType == MaterialType.PorousRock)
                return porousRock.destructible;
            if (materialType == MaterialType.Concrete)
                return concrete.destructible;
            if (materialType == MaterialType.Brick)
                return brick.destructible;
            if (materialType == MaterialType.Glass)
                return glass.destructible;
            if (materialType == MaterialType.Rubber)
                return rubber.destructible;
            if (materialType == MaterialType.Ice)
                return ice.destructible;
            if (materialType == MaterialType.Wood)
                return wood.destructible;
            return true;
        }
        
        // Create material by material type
        public static PhysicMaterial Material (MaterialType materialType)
        {
            // Crete new material
            if (materialType == MaterialType.HeavyMetal)
                return RayfireMan.inst.materialPresets.heavyMetal.material;
            if (materialType == MaterialType.LightMetal)
                return RayfireMan.inst.materialPresets.lightMetal.material;
            if (materialType == MaterialType.DenseRock)
                return RayfireMan.inst.materialPresets.denseRock.material;
            if (materialType == MaterialType.PorousRock)
                return RayfireMan.inst.materialPresets.porousRock.material;
            if (materialType == MaterialType.Concrete)
                return RayfireMan.inst.materialPresets.concrete.material;
            if (materialType == MaterialType.Brick)
                return RayfireMan.inst.materialPresets.brick.material;
            if (materialType == MaterialType.Glass)
                return RayfireMan.inst.materialPresets.glass.material;
            if (materialType == MaterialType.Rubber)
                return RayfireMan.inst.materialPresets.rubber.material;
            if (materialType == MaterialType.Ice)
                return RayfireMan.inst.materialPresets.ice.material;
            if (materialType == MaterialType.Wood)
                return RayfireMan.inst.materialPresets.wood.material;
            return RayfireMan.inst.materialPresets.concrete.material;
        }
    }
}