# MaterialRadomizer
The Material randomizer can change the Material properties of an object. Every object in the scene that needs to change its material properties requires its own material randomizer. The Object randomizer automaticly adds a material randomizer to every object it spawns.
A Material randomizer can be created by adding adding the MatRandomizeHandler script to any game object or prefab that has a renderer component or a child with a renderer component. This script requires a MaterialRandomizeData file that stores all the settings. A new MaterialRandomizeData file can be created by right clicking in the project explorer create->HDRPSyntheticDataGenerator->New Material Dataset.

## Setting up a *MaterialRandomizeData* asset.
### Input/output paths
| Parameter name | Type | Description |
| --- | --- | --- |
| Materials Path | string | The path where the materials are saved. Can be left empty if the original material of the object must be used. This is a relative path starting in the Resources directory. (All materials should be added to the resources directory or sub directory) |
| Texture Path | string | The path where albedo textures are saved. Can be left empty if the albedo texture of the selected material must be used. This is a relative path starting in the Resources directory. (All albedo textures should be added to the resources directory or sub directory) |

### General Material settings
| Parameter name | Type | Description |
| --- | --- | --- |
| False Color Type | FalseColorType | Determines how the false color for the segmentation mask is selected for this object. <ul><li>Global index: sync between all objects</li><li>Local Index: sync between all objects spawned by the same object randomizer</li><li>predetermined: dont change the false color compared to the original</li><li>none: remove the false color</li></ul>|
| Export | bool | Export the pose of this object. IMPORTANT: if this is not enabled no data about this object is exported! |
| Generated Texture Resolution | Vector2Int | Resolution of the textures if they are generated |

### Simple texture variations
| Parameter name | Type | Description |
| --- | --- | --- |
| Apply Random HSV Offset | boolean | Enable random HSV offets. Allows for color variations of H, S or V of the assigned material.. Offset is the maximum change in HSV. The variation is chosen randomly between 0 and offset. |
| H_max Offset | float | Maximum Random Hue offset in the range [0,180] |
| S_max Offset | float | Maximum Random Saturation offset in the range [0,50]|
| V_max Offset | float | Maximum Random Value offset in the range [0,50] |

### Detailmap generation settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Apply Detail Map Variations | boolean | Enable to add variations in detail map, in the form of scratches |


### Rust generation settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Apply Rust | boolean | Enable the addition of rust spots to the material |
| Rust Coeficient | float | Determine the amount of rust 0 = no rust, 1 = fully rusted. |
| Rust Mask Zoom | float | Determine the size of the rust patches. larger numbers mean larger patches. |
| Rust patern Zoom | float | Determine the detail in the rust patches between the difrent posible colors. |
| Rust Color | Color | Determines the colors the rust patches are assigned. |
| nr of Octaves | int | Determines the complexity of the rust patches. |


### Polishing lines generation settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Apply Manufacturing lines | boolean | Enable to add manufacturing lines to the material. This requires the object to be cerrectly UV maped and a line and rust mask must be imported. |
| line spacing | float | Determines the size of the manufacturing lines. |
| line and rust mask | texture | Texture that is used to determine where lines or rust need to be generated. The rust generation can work without this file. |

### Texture resampler settings (preview)
| Parameter name | Type | Description |
| --- | --- | --- |
| Enable texture resampling | boolean | Enable the resampling of the textures of a material to create variations of the same material. This is an expensive opperation. Only the color map is suported at the moment. |
| Nr Resample Samples | int | Determines how many samples are taken to find the optimal sample. |
| Nr Resample Generations | int | Determines how many generations are executed before stopping the algoritme. |