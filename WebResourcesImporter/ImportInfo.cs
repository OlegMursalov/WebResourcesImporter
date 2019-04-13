using System.Collections.Generic;

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

        public List<string> GetInfo()
        {
            List<string> info = new List<string>();
            if (fileNamesSuccessful.Count > 0)
            {
                int i = 1;
                info.Add($"The following web resources were imported in the solution {solutionName}:");
                foreach (var fnsucc in fileNamesSuccessful)
                {
                    info.Add($"{i} - {fnsucc}");
                    i++;
                }
            }
            else
            {
                info.Add($"No web resource was imported in the solution {solutionName}.");
            }
            if (fileNamesError.Count > 0)
            {
                int i = 1;
                info.Add("The following files could not be imported:");
                foreach (var fnerr in fileNamesError)
                {
                    info.Add($"{i} - {fnerr.Key} - {fnerr.Value}");
                    i++;
                }
            }
            else
            {
                info.Add($"No errors. All files imported successfully.");
            }
            return info;
        }
    }
}
