using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebResourcesImporter
{
    public class FInfo
    {
        public string Name { get; set; }
        public string Ext { get; set; }
        public string Body { get; set; }
    }

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
