using UnityEngine;

[AddComponentMenu("Cad2Render/MaterialRandomizers/DefectMaterialIndexHandler")]
public class DefectMaterialIndexHandler : MaterialRandomizerInterface
{

    public int defectIndex = 0;
    public override int getPriority() { return 50; }

    public override void RandomizeSingleMaterial(MaterialTextures textures, ref RandomNumberGenerator rng)
    {
        if(textures.materialIndex == defectIndex)
        {
            textures.falseColor.falseColor.a = 0;
        }
    }
}
