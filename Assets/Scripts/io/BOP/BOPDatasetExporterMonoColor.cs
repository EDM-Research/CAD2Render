using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.io.BOP
{
    class BOPDatasetExporterMonoColor : BOPDatasetExporter
    {
        protected override void setupExportPath()
        {
            base.setupExportPath();
            ensureDir(getFullPath() + "monoColor/");
        }

        public override IEnumerator exportFrame(List<GameObject> instantiated_models, Camera camera, int fileID)
        {
            yield return base.exportFrame(instantiated_models, camera, fileID);
            fileID += 1;
            if (dataset.exportRender)
                imageSaver.Save(renderTexture, getFullPath() + "monoColor/" + fileID.ToString("D6"), dataset.outputExt, true, true);
        }
    }
}
