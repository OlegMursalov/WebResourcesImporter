using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebResourcesImporter
{
    public class Importer
    {
        public Entity GetSolutionByName(IOrganizationService service, string solutionName)
        {
            var query = new QueryExpression()
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);
            try
            {
                var result = service.RetrieveMultiple(query);
                if (result != null && result.Entities != null && result.Entities.Count > 0)
                {
                    return result.Entities.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                
            }
            return null;
        }

        public Entity GetPublisherById(IOrganizationService service, Guid id)
        {
            try
            {
                return service.Retrieve("publisher", id, new ColumnSet(true));
            }
            catch (Exception)
            {

            }
            return null;
        }

        public ImportInfo Process(IOrganizationService service, string solutionName, string prefix, string[] fileNames)
        {
            var importInfo = new ImportInfo(solutionName);
            for (int i = 0; i < fileNames.Length; i++)
            {
                var fileInfo = new FileInfo(fileNames[i]);
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        var bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        var base64 = Convert.ToBase64String(bytes, 0, bytes.Length);
                        var createRequest = new CreateRequest
                        {
                            Target = CreateWebResourceEntity(fileInfo.Name, prefix, base64)
                        };
                        createRequest.Parameters.Add("SolutionUniqueName", solutionName);
                        var result = service.Execute(createRequest);
                        importInfo.AddNameSuccessful($"{prefix}_{fileInfo.Name}");
                    }
                    catch (Exception ex)
                    {
                        importInfo.AddError($"{prefix}_{fileInfo.Name}", ex.Message);
                    }
                }
            }
            return importInfo;
        }

        private Entity CreateWebResourceEntity(string fileName, string prefix, string base64)
        {
            var fileExt = Path.GetExtension(fileName);
            var webResourceType = new OptionSetValue(GetWebResourceTypeValue(fileExt));
            var webResource = new Entity("webresource");
            webResource["content"] = base64;
            webResource["displayname"] = $"{prefix}_{fileName}";
            webResource["name"] = $"{prefix}_{fileName}";
            webResource["webresourcetype"] = webResourceType;
            return webResource;
        }

        private int GetWebResourceTypeValue(string fileExt)
        {
            switch (fileExt.ToLower())
            {
                case ".html":
                    return 1;
                case ".css":
                    return 2;
                case ".js":
                    return 3;
                case ".xml":
                    return 4;
                case ".png":
                    return 5;
                case ".jpg":
                    return 6;
                case ".gif":
                    return 7;
                case ".xap":
                    return 8;
                case ".xsl":
                    return 9;
                case ".ico":
                    return 10;
                case ".svg":
                    return 11;
                case ".resx":
                    return 12;
                default:
                    return 0;
            }
        }
    }
}
