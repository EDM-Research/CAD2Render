using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.io.BOP
{
    class BOPDatasetExporterMonoColor : BOPDatasetExporter
    {
        protected override void setupExportPath()
        {
            base.setupExportPath();
            ensureDir(getFullPath() + "monoColor/");
        }
        new public void exportRenderTexture(int fileID)
        {
            imageSaver.Save(renderTexture, getFullPath() + "monoColor/" + fileID.ToString("D6"), dataset.outputExt, true, true);
        }
    }
}
