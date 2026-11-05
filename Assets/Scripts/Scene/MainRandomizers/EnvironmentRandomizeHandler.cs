using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[AddComponentMenu("Cad2Render/Environment Randomize Handler")]
public class EnvironmentRandomizeHandler : RandomizerInterface
{
    public EnvironmentRandomizeData dataset;
    [InspectorButton("TriggerCloneClicked")]
    public bool clone;
    private void TriggerCloneClicked()
    {
        RandomizerInterface.CloneDataset(ref dataset);
    }

    private Object[] cubeMaps = new Object[0];
    private int environmentIndex = 0;

    public void Start()
    {
        randomizerType = MainRandomizerData.RandomizerTypes.Light;
        this.LinkGui();

        if (dataset != null && dataset.environmentsPath != "")
        {
            cubeMaps = Resources.LoadAll(dataset.environmentsPath, typeof(Cubemap));
            if (cubeMaps.Length == 0)
                Debug.LogWarning("No environment maps found in " + dataset.environmentsPath);
        }
    }

    public override ScriptableObject getDataset()
    {
        return dataset;
    }

    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        RandomizeEnvironment(ref rng);
        resetFrameAccumulation();
    }

    private void RandomizeEnvironment(ref RandomNumberGenerator rng)
    {
        HDRISky sky = null;
        if (MainRandomizer.renderSettings != null)
            MainRandomizer.renderSettings.profile.TryGet<HDRISky>(out sky);
        if (sky == null)
        {
            Debug.LogWarning("No sky found in the environment randomizer");
            return;
        }

        if (cubeMaps.Length > 0)
        {
            Cubemap texture;
            if (!dataset.pickRandomEnvironment)
            {
                texture = (Cubemap)cubeMaps[environmentIndex];
                environmentIndex = (environmentIndex + 1) % cubeMaps.Length;
            }
            else
                texture = (Cubemap)cubeMaps[rng.IntRange(0, cubeMaps.Length)];

            sky.hdriSky.value = texture;
        }

        if (dataset.randomEnvironmentRotations)
            sky.rotation.value = rng.Angle(dataset.minEnvironmentAngle, dataset.maxEnvironmentAngle);

        if (dataset.randomExposuresEnvironment)
        {
            Exposure exposureCorrection = null;
            if (MainRandomizer.postProcesingSettings != null)
                MainRandomizer.postProcesingSettings.profile.TryGet<Exposure>(out exposureCorrection);
            if (exposureCorrection == null)
            {
                Debug.LogWarning("No exposure found in the post processing gameobject");
                return;
            }
            exposureCorrection.fixedExposure.value = rng.Range(dataset.minCameraExposure, dataset.maxCameraExposure);

            sky.exposure.value = rng.Range(dataset.minEnvironmentExposure, dataset.maxEnvironmentExposure);
        }
    }
}
