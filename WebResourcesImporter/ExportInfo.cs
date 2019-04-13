using System.Collections.Generic;

namespace WebResourcesImporter
{
    public class ExportInfo
    {
        private string solutionName;
        public List<FInfo> Files { get; set; }

        public ExportInfo(string solutionName)
        {
            this.solutionName = solutionName;
            Files = new List<FInfo>();
        }

        public void AddFileInfo(string name, string ext, string body)
        {
            Files.Add(new FInfo
            {
                Name = name,
                Ext = ext,
                Body = body
            });
        }
    }
}
