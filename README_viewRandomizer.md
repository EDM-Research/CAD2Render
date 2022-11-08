# ViewRadomizer
The view randomizer can change the pose of the main camera. Only one view randomizer should ever be active in a scene.
A view randomizer can be created by adding an empty game object to the scene and adding the ViewRandomizeHandler script to it. This script requires a viewRandomizeData file that stores all the settings. A new viewRandomizeData file can be created by right clicking in the project explorer create->HDRPSyntheticDataGenerator->New View Dataset.
The camera will always look towards the origin of the ViewRadomizer and be moved to a point around this origin as determined by the spherical coordinates in the viewRandomizeData file.

## Setting up a *viewRandomizerData* asset.
### Viewpoint Variations
| Parameter name | Type | Description |
| --- | --- | --- |
| Viewpoint Variatons | boolean | Boolean to enable viewpoint variations. |
| Import from BOP | boolean | Boolean to enable importing the exact pose from an existing bop dataset. (ignore all other settings when enabled) |
| Random YUp | boolean | Enable random y up vector between (0,1,0) and (0,-1,0). Required for changing viewpoint over poles, caused by limitations of spherical coordinates. |
| Min Theta | float | Minimum theta angle in range of [-90,90]|
| Max Theta | float | Maximun theta angle in range of [-90,90] |
| Min Phi | float | Minimum phi angle in range of [-360,360] |
| Max Phi | float | Maximun phi angle in range of [-360,360] |
| Min Radius | float | Minimun distance to the ViewRadomizer origin in mm |
| Max Radius | float | Maximun distance to the ViewRadomizer origin in mm |
