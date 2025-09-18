using Assets.Scripts.io.BOP;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.io.MISC
{
    public class MetadataExporter : ExportDatasetInterface
    {
        private MainRandomizer generator;

        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return 0;
            if (fileID == 0)
            {
                generator = GameObject.FindGameObjectWithTag("Generator")?.GetComponent<MainRandomizer>();
                ExportDatasetInfo();
            }
        }

        protected override void setupExportPath()
        {
            datasetPrefixPath = "metadata/";
            ensureDir(getFullPath());
        }

        private void ExportDatasetInfo()
        {
            StreamWriter writer = new StreamWriter(getFullPath() + "versionInfo.json", false);

            var version = PlanetaGameLabo.UnityGitVersion.GitVersion.version;
            writer.WriteLine(JsonUtility.ToJson(version, true));
            writer.WriteLine();
            writer.Flush();
            writer.Close();

            if (generator == null)
                return;

            var dataset = generator.dataset;
            if (dataset.renderProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.renderProfile), getFullPath() + dataset.renderProfile.name + ".asset", true);
            if (dataset.rayTracingProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.rayTracingProfile), getFullPath() + dataset.rayTracingProfile.name + ".asset", true);
            if (dataset.postProcesingProfile)
                System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset.postProcesingProfile), getFullPath() + dataset.postProcesingProfile.name + ".asset", true);
            System.IO.File.Copy(AssetDatabase.GetAssetPath(dataset), getFullPath() + dataset.name + ".asset", true);

            foreach (RandomizerInterface randomizer in generator.GetComponentsInChildren<RandomizerInterface>())
                if (randomizer.getDataset() != null)
                    System.IO.File.Copy(AssetDatabase.GetAssetPath(randomizer.getDataset()), getFullPath() + randomizer.getDataset().name + ".asset", true);

            foreach (MaterialRandomizerInterface randomizer in generator.GetComponentsInChildren<MaterialRandomizerInterface>())
                if (randomizer.getDataset() != null)
                    System.IO.File.Copy(AssetDatabase.GetAssetPath(randomizer.getDataset()), getFullPath() + randomizer.getDataset().name + ".asset", true);
        }
    }
}