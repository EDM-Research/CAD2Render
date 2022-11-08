# LightRadomizer
The light randomizer can change the skybox and add extra lights to the scene. Only one light randomizer should control the skybox, but others can be added to have more controll over the extra lights.
A light randomizer can be created by adding an empty game object to the scene and adding the LightRandomizeHandler script to it. This script requires a LightRandomizeData file that stores all the settings. A new LightRandomizeData file can be created by right clicking in the project explorer create->HDRPSyntheticDataGenerator->New Light Dataset.

## Setting up a *LightRandomizeData* asset.
### Input/output paths
| Parameter name | Type | Description |
| --- | --- | --- |
| Environments Path | string | The path where the environment maps are saved. This is a relative path starting in the Resources directory. (All environment maps should be added to the resources directory or sub directory) |
| Light Source prefab | Light | A prefab of a light that is used as extra light. If no extra lights need to be added to the scene this can be set to None|


### Environment Variations
| Parameter name| Type | Description |
| --- | --- | --- |
| Environment Variatons | boolean | Boolean to enable environment map variations. |
| Environment Variatons | boolean | Boolean to enable random selection of an environment map or sequential selection. |
| Random Environment Rotations | boolean | Boolean to enable random environment map rotations. |
| Min Environment Angle | float | Min angle the environment map takes. |
| Max Environment Angle  | float | Max angle the environment map takes. |
| Random Exposures Environment | boolean | Boolean to enable random environment exposures. Scaling of environment map intensity. |
| Min Exposure | float | Min exposure |
| Max exposure | float | Max exposure |


### Light sources Variations
| Parameter name| Type | Description |
| --- | --- | --- |
| Light source Variatons | boolean | Boolean to enable light source variations. |
| num Light sources | int | Number of lights that should be spawned from the light source prefab. |
| Min Intensity Modifier | float | Min modifier with which the light intensity of the light prefab is multiplied. |
| Max Intensity Modifier | float | Max modifier with which the light intensity of the light prefab is multiplied. || Min Theta | float | Minimum theta angle in range of [-90,90]|
| Max Theta Light | float | Maximun theta angle in range of [-90,90] |
| Min Phi Light| float | Minimum phi angle in range of [-360,360] |
| Max Phi Light| float | Maximun phi angle in range of [-360,360] |
| Min Radius Light| float | Minimun distance to the LightRadomizer origin in mm |
| Max Radius Light| float | Maximun distance to the LightRadomizer origin in mm |
| Apply projector Variations| bool | When using a spot light as light prefab a black and white texture can be added to set the intensity of spot light as a projector. |
| Projector Texture Path| string | The path where the projector textures are saved. This is a relative path starting in the Resources directory. (All projector textures should be added to the resources directory or sub directory) |




