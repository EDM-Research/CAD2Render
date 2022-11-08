# MainRadomizer
The main randomizer triggers the other randomizers that are linked to it as child objects. Only one main randomizer should ever be active in a scene.
A main randomizer can be created by adding an empty game object to the scene and adding the MainRandomizer script to it. This script requires a mainRandomizeData file that stores all the settings. A new mainRandomizeData file can be created by right clicking in the project explorer create->HDRPSyntheticDataGenerator->New Main Dataset.

## Setting up a *mainRandomizeData* asset.
### Input/output paths
| Parameter name | Type | Description |
| --- | --- | --- |
| Output Path | string | The path where the data is saved. Can be a relative or absolute path. |
| Annotations File | string | Name of annotations file. Relative to output path. |
| BOP Input Path | string | Relative or absolure path to the bop dataset that needs to be imported. If you dont want import any data from an existin dataset leave it empty |
| Mm To Unity Distance Scale | float | The scale of unity distance units to mm. (0.01 means 1 unit in unity is the same as 100mm) All prefabs that are imported should use this scale. 0.01 is the default value, use higher values if more precise physics are required. |

### Render settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Resolution Width | int | The amount of pixels in the width of the images that are generated |
| Resolution Height | int | The amount of pixels in the Height of the images that are generated |
| Render Profile | Volume Profile | Needs to Contain the HDRI sky. Other settings render settings like fog or shadows can be added here. |
| Ray Tracing Profile | Volume Profile | Contains the ray tracing settings. Use Recursive rendering instead of path tracing if speed is more important then correct lighting |
| Post Procesing Profile | Volume Profile | Post processing effects like white balancing or tonemapping can be changed here |
| Apply Gamma Correction | bool | Specify if the output image should be in gamma or linear color space |
| Auto Camera Exposure | bool | Should the exposure of the camera automaticly be adjusted to the light conditions |
| Stop Simulation time completly | bool | Stops all objects from moving while rendering the scene. This can produce flying objects but ensures the final image has the specified amount of samples |
| Num Render Frames | int | Number of samples each pixel should take for each image. Higher number will reduce the amount of noise but increase the amount of procesing time |
| Num Physics Frames | int | Number of frames that are not rendered but purely used to calculate the physics in the scene. These frames are calculated much faster then render frames. |


### Generation settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Start File Counter | int | Change the start index of the image that is generated. (Warning: this can not be used to extend an existing dataset any annotation files are still overwriten) |
| Number of Samples | int | The ammount of images that should be generated. |
| Seed | int | The seed used to generate the scene. When importing from the bop format this will be added to the id of the imported scene. |
| Seperate Updates | bool | Specify if all child randomizers should be called at the same time. This can for example be used to create multiple views of the same scene, set the default interval on 5 while setting the view interval on 1 wil create 5 images of the smae scene from difrent viewpoints. |
| Update Itervals | <Randomizer Type, uint> | After how many frames the radomizer type should be triggerd. ( 0 = 1 = every frame the update is trigered) |

### Export settings
| Parameter name | Type | Description |
| --- | --- | --- |
| Export models by Tag | bool | If this is true, only objects with the "exportInstanceInfo" tag are exported, otherwise the material randomizer is used to determine export objects. |
| Export to BOP  | bool | Determine if the data should be exported to the BOP format. |
| BOP scene ID | int | Determine the id of the dataset to be saved in the BOP format. |
| Export Depth Texture | bool | Determine if the Depth texture should be exported. |
| Max Detph Distance | int | Determine the distance in mm the depth texture can display. Any object further away will be saturated to white. |
| Export Normal texture | bool | Determine if the Normal texture should be exported. Only works with the BOP format. This can be used to denoise the rgb images |
| Export Albedo texture | bool | Determine if the Albedo texture should be exported. NOT YET IMPLEMENTED. This can be used to denoise the rgb im
| Export to FM format  | bool | Determine if the data should be exported to the FM-format. This format is being discontinued |ages |
| Export Ext  | image type | The output extention that is used in the FM-foramt for the images. |
| Export Sub Models | bool | Determine if sub models should be exported separately. Only works with the FM-format. |
| Export Image position | bool | Determine if the 2d image positions of sub models for keypoint detection should be exported separately. Only works with the FM-format. |
| Export To Mitsuba | boolean | Export Mitsuba Render files |
| Mitsuba Sample Count | int | Number of Mitsuba samples. |
