using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebResourcesImporter
{
    public class ImpExp
    {
        private string prefix;
        private string solutionName;
        private Guid solutionId;

        public ImpExp()
        {
        }

        private void SetPrefix(string prefix)
        {
            this.prefix = prefix;
        }

        public string GetPrefix()
        {
            return prefix;
        }

        private void SetSolutionName(string solutionName)
        {
            this.solutionName = solutionName;
        }

        public string GetSolutionName()
        {
            return solutionName;
        }

        private void SetSolutionId(Guid solutionId)
        {
            this.solutionId = solutionId;
        }

        public Guid GetSolutionId()
        {
            return solutionId;
        }

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
                    var solution = result.Entities.FirstOrDefault();
                    SetSolutionName(solutionName);
                    if (solution.Contains("solutionid"))
                    {
                        SetSolutionId((Guid)solution["solutionid"]);
                    }
                    else
                    {
                        SetSolutionId(Guid.Empty);
                    }
                    return solution;
                }
            }
            catch (Exception)
            {
                
            }
            SetSolutionName(null);
            SetSolutionId(Guid.Empty);
            return null;
        }

        public string GetPrefixFromSolutionPublisher(IOrganizationService service, Entity solution)
        {
            if (solution.Contains("publisherid"))
            {
                var publisherid = solution["publisherid"] as EntityReference;
                if (publisherid != null)
                {
                    var publisher = GetPublisherById(service, publisherid.Id);
                    if (publisher != null)
                    {
                        if (publisher.Contains("customizationprefix"))
                        {
                            var prefix = publisher["customizationprefix"] as string;
                            if (!string.IsNullOrEmpty(prefix))
                            {
                                SetPrefix(prefix);
                                return prefix;
                            }
                        }
                    }
                }
            }
            SetPrefix(null);
            return string.Empty;
        }

        private Entity GetPublisherById(IOrganizationService service, Guid id)
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

        public ExportInfo GetFilesFromSolution(IOrganizationService service)
        {
            var exportInfo = new ExportInfo(solutionName);
            try
            {
                var query = new QueryExpression()
                {
                    EntityName = "webresource",
                    ColumnSet = new ColumnSet(true),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("iscustomizable", ConditionOperator.Equal, true);
                var result = service.RetrieveMultiple(query);
                if (result != null && result.Entities != null && result.Entities.Count > 0)
                {
                    var preStr = prefix + "_";
                    foreach (var item in result.Entities)
                    {
                        if (item.Contains("name") && item.Contains("webresourcetype") && item.Contains("content"))
                        {
                            var name = item["name"] as string;
                            if (name.StartsWith(preStr))
                            {
                                var body = item["content"] as string;
                                var webresourcetype = item["webresourcetype"] as OptionSetValue;
                                if (webresourcetype != null)
                                {
                                    var ext = GetExtByTypeValue(webresourcetype.Value);
                                    if (!string.IsNullOrEmpty(ext))
                                    {
                                        if (!name.EndsWith(ext))
                                        {
                                            name += ext;
                                        }
                                        exportInfo.AddFileInfo(name, ext, body);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return exportInfo;
        }

        public ImportInfo Import(IOrganizationService service, string[] fileNames, bool? overwriteMod, bool? changeTheCharactersMod)
        {
            var importInfo = new ImportInfo(solutionName);
            for (int i = 0; i < fileNames.Length; i++)
            {
                var fileInfo = new FileInfo(fileNames[i]);
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                {
                    var fileName = fileInfo.Name;
                    try
                    {
                        var bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        var base64 = Convert.ToBase64String(bytes, 0, bytes.Length);
                        if (changeTheCharactersMod.HasValue && changeTheCharactersMod.Value)
                        {
                            fileName = GetValidFileName(fileName);
                        }
                        var createRequest = new CreateRequest
                        {
                            Target = CreateWebResourceEntity(fileName, prefix, base64)
                        };
                        createRequest.Parameters.Add("SolutionUniqueName", solutionName);
                        if (overwriteMod.HasValue && overwriteMod.Value)
                        {
                            var query = new QueryExpression()
                            {
                                EntityName = "webresource",
                                ColumnSet = new ColumnSet(true),
                                Criteria = new FilterExpression()
                            };
                            query.Criteria.AddCondition("name", ConditionOperator.Equal, $"{prefix}_{fileName}");
                            var result = service.RetrieveMultiple(query);
                            if (result != null && result.Entities != null && result.Entities.Count > 0)
                            {
                                var webResource = result.Entities.First();
                                webResource["content"] = base64;
                                service.Update(webResource);
                                importInfo.AddNameSuccessful($"{prefix}_{fileName}");
                            }
                            else
                            {
                                service.Execute(createRequest);
                                importInfo.AddNameSuccessful($"{prefix}_{fileName}");
                            }
                        }
                        else
                        {
                            service.Execute(createRequest);
                            importInfo.AddNameSuccessful($"{prefix}_{fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        importInfo.AddError($"{prefix}_{fileName}", ex.Message);
                    }
                }
            }
            return importInfo;
        }

        private string GetValidFileName(string fileName)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < fileName.Length; i++)
            {
                var c = fileName[i].ToString();
                if (Regex.IsMatch(c, "[(a-zA-Z0-9._)]+$"))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("_");
                }
            }
            return sb.ToString();
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

        private string GetExtByTypeValue(int value)
        {
            switch (value)
            {
                case 1:
                    return ".html";
                case 2:
                    return ".css";
                case 3:
                    return ".js";
                case 4:
                    return ".xml";
                case 5:
                    return ".png";
                case 6:
                    return ".jpg";
                case 7:
                    return ".gif";
                case 8:
                    return ".xap";
                case 9:
                    return ".xsl";
                case 10:
                    return ".ico";
                case 11:
                    return ".svg";
                case 12:
                    return ".resx";
                default:
                    return string.Empty;
            }
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
