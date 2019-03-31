using System.Collections.Generic;
using System.Text;

namespace WebResourcesImporter
{
    public class ImportInfo
    {
        private string solutionName;
        private List<string> fileNamesSuccessful;
        private Dictionary<string, string> fileNamesError;

        public ImportInfo(string solutionName)
        {
            this.solutionName = solutionName;
            fileNamesSuccessful = new List<string>();
            fileNamesError = new Dictionary<string, string>();
        }

        public void AddNameSuccessful(string fileName)
        {
            fileNamesSuccessful.Add(fileName);
        }

        public void AddError(string fileName, string message)
        {
            fileNamesError.Add(fileName, message);
        }

        public string GetInfo()
        {
            var sb = new StringBuilder();
            if (fileNamesSuccessful.Count > 0)
            {
                int i = 1;
                sb.AppendLine($"The following web resources were created in the solution {solutionName}:");
                foreach (var fnsucc in fileNamesSuccessful)
                {
                    sb.AppendLine($"{i} - {fnsucc}");
                    i++;
                }
            }
            else
            {
                sb.AppendLine($"No web resource was created in the solution {solutionName}.");
            }
            if (fileNamesError.Count > 0)
            {
                int i = 1;
                sb.AppendLine("The following files could not be imported:");
                foreach (var fnerr in fileNamesError)
                {
                    sb.AppendLine($"{i} - {fnerr.Key} - {fnerr.Value}");
                    i++;
                }
            }
            else
            {
                sb.AppendLine($"No errors. All files imported successfully.");
            }
            return sb.ToString();
        }
    }
}
