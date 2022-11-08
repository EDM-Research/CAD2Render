# ObjectRadomizer
The Object randomizer can spawn new objects in the scene. objects spawned in a previous randomization are automaticly removed. Multiple Object randomizer can be added to the scene. Every object randomizer also needs a materialRandomizer see [here](README_materialrandomizer.md) for the parameters description. 
A Object randomizer can be created by adding an empty game object to the scene and adding a (box)collider to it first. Make sure the collider is set as "is trigger". Then the objectRandomizeHandler script can be added to the game object. This script requires a MaterialRandomizeData file and ObjectRandomizeData file that stores all the settings. A new ObjectRandomizeData file can be created by right clicking in the project explorer create->HDRPSyntheticDataGenerator->New Object Dataset.

## Setting up a *ObjectRandomizeData* asset.
### Input/output paths
| Parameter name | Type | Description |
| --- | --- | --- |
| Model Path | string | The path where the prefabs are saved. This is a relative path starting in the Resources directory. (All prefabs should be added to the resources directory or sub directory) |

### Model Variations
| Parameter name | Type | Description |
| --- | --- | --- |
| Import from BOP | BopImportType | Determines which parameters are copied from an imported BOp dataset |
| Unique Objects | bool | When enabled all unique objects will be spawned that where loaded from the input file. Num random objects parameter is ignored when enabled. |
| Num Random Objects | int | Determines how many objects are spawned. |
| Random model Translation | bool | Enable the object to spawn in a random location in the spawn zone. |
| Random model Rotation | bool | Enable the object to spawn with a random rotation. |
| Random Rotation offset | bool | Used to limit the random rotation around one axis. (no effect if Random model Rotation is enabled) |
| Avoid Collisions | bool | When enabled the program tries to spawn objects that dont collide. Use a flat spawn zone and make sure there is enough space for the objects to spawn sperated. |
