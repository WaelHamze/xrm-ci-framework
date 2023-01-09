using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Xrm.Framework.CI.Common.Entities;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;

using Xrm.Framework.CI.Common;

namespace Xrm.Framework.CI.Common
{
    public class PluginRegistrationHelper
    {
        private readonly IOrganizationService organizationService;
        private readonly PluginRepository pluginRepository;
        private readonly Action<string> logVerbose;
        private readonly Action<string> logWarning;
        private readonly IReflectionLoader reflectionLoader;
        private readonly IPluginRegistrationObjectFactory pluginRegistrationObjectFactory;

        public PluginRegistrationHelper(IOrganizationService service, CIContext xrmContext, Action<string> logVerbose, Action<string> logWarning)
        {
            this.logVerbose = logVerbose;
            this.logWarning = logWarning;
            organizationService = service;
            pluginRepository = new PluginRepository(xrmContext);
            reflectionLoader = new ReflectionLoader();
            pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
        }

        public PluginRegistrationHelper(Action<string> logVerbose, Action<string> logWarning)
        {
            this.logVerbose = logVerbose;
            this.logWarning = logWarning;
            reflectionLoader = new ReflectionLoader();
            pluginRegistrationObjectFactory = new PluginRegistrationObjectFactory();
        }

        public PluginRegistrationHelper(Action<string> logVerbose, Action<string> logWarning,
            IReflectionLoader reflectionLoader, IPluginRegistrationObjectFactory pluginRegistrationObjectFactory)
        {
            this.logVerbose = logVerbose;
            this.logWarning = logWarning;
            this.reflectionLoader = reflectionLoader;
            this.pluginRegistrationObjectFactory = pluginRegistrationObjectFactory;
        }

        public Assembly GetAssemblyRegistration(string assemblyName, string version) => pluginRepository.GetAssemblyRegistration(assemblyName, version);

        public void RemoveComponentsNotInMapping(Assembly assemblyMapping)
        {
            var assemblyInCrm = pluginRepository.GetAssemblyRegistration(assemblyMapping.Name, assemblyMapping.Version);
            if (assemblyInCrm == null)
            {
                logVerbose?.Invoke($"Assembly {assemblyMapping.Name} not found in CRM");
                return;
            }

            var stepsInMapping = new HashSet<string>(assemblyMapping.PluginTypes.SelectMany(t => t.Steps, (t, s) => $"{t.Name}#{s.Name}#{s.GetHashCode()}"));
            var pluginStepsToDelete = assemblyInCrm.PluginTypes.SelectMany(t => t.Steps, (t, s) => new
            {
                Key = $"{t.Name}#{s.Name}#{s.GetHashCode()}",
                Name = s.Name,
                Id = s.Id
            })
                .Where(x => !stepsInMapping.Contains(x.Key)).
                ToList();
            foreach (var pluginStep in pluginStepsToDelete)
            {
                logVerbose?.Invoke($"Trying to delete step {pluginStep.Id} / {pluginStep.Name}");
                organizationService.Delete(SdkMessageProcessingStep.EntityLogicalName, pluginStep.Id.Value);
            }

            var typesInMapping = new HashSet<string>(assemblyMapping.PluginTypes.Select(t => t.Name));
            var pluginTypesToDelete = assemblyInCrm.PluginTypes
                .Where(t => !typesInMapping.Contains(t.Name))
                .ToList();
            foreach (var pluginType in pluginTypesToDelete)
            {
                logVerbose?.Invoke($"Trying to delete type {pluginType.Id} / {pluginType.Name}");
                DeleteObjectWithDependencies(pluginType.Id.Value, ComponentType.PluginType);
            }
        }

        public void DeleteObjectWithDependencies(Guid objectId, ComponentType? componentType, HashSet<string> deletingHashSet = null)
        {
            if (deletingHashSet == null)
            {
                deletingHashSet = new HashSet<string>();
            }
            var objectkey = $"{componentType}{objectId}";
            if (deletingHashSet.Contains(objectkey))
            {
                return;
            }
            deletingHashSet.Add(objectkey);

            logVerbose?.Invoke($"Checking dependencies for {componentType} / {objectId}");
            foreach (var objectToDelete in GetDependeciesForDelete(objectId, componentType))
            {
                DeleteObjectWithDependencies(objectToDelete.DependentComponentObjectId.Value, objectToDelete.DependentComponentTypeEnum, deletingHashSet);
            }

            switch (componentType)
            {
                case ComponentType.Workflow:
                    var workflow = pluginRepository.GetWorkflowById(objectId);
                    if (workflow.StateCode == WorkflowState.Activated)
                    {
                        logVerbose?.Invoke($"Unpublishing workflow {workflow.Name}");
                        organizationService.Execute(new SetStateRequest
                        {
                            EntityMoniker = workflow.ToEntityReference(),
                            State = new OptionSetValue((int)WorkflowState.Draft),
                            Status = new OptionSetValue((int)Workflow_StatusCode.Draft)
                        });
                    }
                    if (workflow.CategoryEnum == Workflow_Category.BusinessProcessFlow)
                    {
                        var entityMetadata = organizationService.GetEntityMetadata(workflow.UniqueName);
                        logVerbose?.Invoke($"Checking dependencies for BPF entity: {workflow.UniqueName}");
                        DeleteObjectWithDependencies(entityMetadata.MetadataId.Value, ComponentType.Entity, deletingHashSet);
                    }

                    if (workflow.CategoryEnum == Workflow_Category.BusinessProcessFlow)
                    {
                        RemoveAllWorkflowsFromBpf(workflow);
                        logVerbose?.Invoke($"Preserving BPF {workflow.Name}");
                        return;
                    }

                    logVerbose?.Invoke($"Trying to delete {componentType} {workflow.Name}");
                    organizationService.Delete(Workflow.EntityLogicalName, objectId);
                    break;
                case ComponentType.SDKMessageProcessingStep:
                    var step = pluginRepository.GetSdkMessageProcessingStepById(objectId);
                    if (step.IsHidden.Value == true)
                    {
                        logVerbose?.Invoke($"Preserving hidden SdkMessageProcessingStep {step.Name}");
                        return;
                    }
                    logVerbose?.Invoke($"Trying to delete {componentType} {step.Name} / {objectId}");
                    organizationService.Delete(SdkMessageProcessingStep.EntityLogicalName, objectId);
                    break;
                case ComponentType.PluginType:
                    var type = pluginRepository.GetPluginTypeById(objectId);
                    logVerbose?.Invoke($"Trying to delete {componentType} {type.Name} / {objectId}");
                    organizationService.Delete(PluginType.EntityLogicalName, objectId);
                    break;
                case ComponentType.PluginAssembly:
                    logVerbose?.Invoke($"Trying to delete {componentType} {objectId}");
                    organizationService.Delete(PluginAssembly.EntityLogicalName, objectId);
                    break;
                case ComponentType.ServiceEndpoint:
                    logVerbose?.Invoke($"Trying to delete {componentType} {objectId}");
                    organizationService.Delete(ServiceEndpoint.EntityLogicalName, objectId);
                    break;
            }
        }

        private const string ActionComposieClassWithAssemblyQualifiedName = "Microsoft.Crm.Workflow.Activities.ActionComposite, Microsoft.Crm.Workflow, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string mxswaNamespace = "clr-namespace:Microsoft.Xrm.Sdk.Workflow.Activities;assembly=Microsoft.Xrm.Sdk.Workflow, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        private void RemoveAllWorkflowsFromBpf(Workflow bpf)
        {
            var xaml = XDocument.Parse(bpf.Xaml);
            var nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("mxswa", mxswaNamespace);
            var actionsElements = xaml.XPathSelectElements($"//mxswa:ActivityReference[@AssemblyQualifiedName='{ActionComposieClassWithAssemblyQualifiedName}']", nsmgr).ToList();
            foreach (var element in actionsElements)
            {
                element.Remove();
            }
            organizationService.Update(new Workflow
            {
                Xaml = xaml.ToString(SaveOptions.DisableFormatting),
                Id = bpf.Id
            });
        }

        public Guid UpsertPluginAssembly(Assembly pluginAssembly, AssemblyInfo assemblyInfo, string solutionName, RegistrationTypeEnum registrationType)
        {
            Guid Id = pluginAssembly?.Id ?? Guid.Empty;
            if (Id == Guid.Empty)
            {
                Id = pluginRepository.GetPluginAssemblyId(assemblyInfo.AssemblyName);
                logWarning?.Invoke($"Extracted id using plugin assembly name {assemblyInfo.AssemblyName}");
            }

            var assembly = new PluginAssembly()
            {
                Version = assemblyInfo.Version,
                Content = assemblyInfo.Content,
                Name = assemblyInfo.AssemblyName,
                SourceTypeEnum = PluginAssembly_SourceType.Database,
                IsolationModeEnum = PluginAssembly_IsolationMode.Sandbox,
            };

            if (pluginAssembly != null)
            {
                assembly.SourceTypeEnum = pluginAssembly.SourceType;
                assembly.IsolationModeEnum = pluginAssembly.IsolationMode;
            }

            if (!Id.Equals(Guid.Empty) && registrationType == RegistrationTypeEnum.Reset)
            {
                DeleteObjectWithDependencies(Id, ComponentType.PluginAssembly);
            }

            logVerbose?.Invoke($"Trying to upsert {assemblyInfo.AssemblyName} / {Id}");
            Id = ExecuteRequest(registrationType, Id, assembly);

            AddComponentToSolution(Id, ComponentType.PluginAssembly, solutionName);

            return Id;
        }

        public void UpsertPluginTypeAndSteps(Guid parentId, Type pluginType, string solutionName, RegistrationTypeEnum registrationType)
        {
            Guid Id = pluginType.Id ?? Guid.Empty;
            if (Id == Guid.Empty)
            {
                Id = pluginRepository.GetPluginTypeId(parentId, pluginType.Name);
                logWarning?.Invoke($"Extracted id using plugin type name {pluginType.Name}");
            }

            var type = new PluginType()
            {
                Name = pluginType.Name,
                Description = pluginType.Description,
                FriendlyName = pluginType.FriendlyName,
                TypeName = pluginType.TypeName,
                WorkflowActivityGroupName = pluginType.WorkflowActivityGroupName,
                PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, parentId)
            };

            Id = ExecuteRequest(registrationType, Id, type);
            // AddComponentToSolution(Id, ComponentType.PluginType, solutionName);
            logVerbose?.Invoke($"UpsertPluginType {Id} completed");

            var typeRef = new EntityReference(PluginType.EntityLogicalName, Id);

            foreach (var step in pluginType.Steps)
            {
                var sdkMessageProcessingStepId = UpsertSdkMessageProcessingStep(typeRef, step, solutionName, registrationType);
                logVerbose?.Invoke($"Upsert SdkMessageProcessingStep {sdkMessageProcessingStepId} completed");
                foreach (var image in step.Images)
                {
                    var sdkMessageProcessingStepImageId = UpsertSdkMessageProcessingStepImage(sdkMessageProcessingStepId, image, solutionName, registrationType);
                    logVerbose?.Invoke($"Upsert SdkMessageProcessingStepImage {sdkMessageProcessingStepImageId} completed");
                }
            }
        }

        public List<ServiceEndpt> GetServiceEndpoints(string solutionName, string endPointName) => pluginRepository.GetServiceEndpoints(Guid.Empty, endPointName);

        public void SerializerObjectToFile(string mappingFile, object obj)
        {
            var fileInfo = new FileInfo(mappingFile);
            switch (fileInfo.Extension.ToLower())
            {
                case ".json":
                    Serializers.SaveJson(mappingFile, obj);
                    break;
                case ".xml":
                    Serializers.SaveXml(mappingFile, obj);
                    break;
                default:
                    throw new ArgumentException("Only .json and .xml mapping files are supported", nameof(mappingFile));
            }
        }

        public void UpsertServiceEndpoints(List<ServiceEndpt> serviceEndptLst, string solutionName, RegistrationTypeEnum registrationType)
        {
            foreach (var serviceEndPt in serviceEndptLst)
            {
                logVerbose?.Invoke($"UpsertServiceEndpoint {serviceEndPt.Id} started");
                var serviceEndpointId = UpsertServiceEndpoint(serviceEndPt, solutionName, registrationType);
                logVerbose?.Invoke($"UpsertServiceEndpoint {serviceEndpointId} completed");

                foreach (var step in serviceEndPt.Steps)
                {
                    var serviceEndpointRef = new EntityReference(ServiceEndpoint.EntityLogicalName, serviceEndpointId);
                    logVerbose?.Invoke($"UpsertSdkMessageProcessingStep {step.Id} started");
                    var stepId = UpsertSdkMessageProcessingStep(serviceEndpointRef, step, solutionName, registrationType);
                    logVerbose?.Invoke($"UpsertSdkMessageProcessingStep {stepId} completed");

                    foreach (var image in step.Images)
                    {
                        var stepRef = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, stepId);
                        logVerbose?.Invoke($"UpsertSdkMessageProcessingStepImage {image.Id} started");
                        var imageId = UpsertSdkMessageProcessingStepImage(stepId, image, solutionName, registrationType);
                        logVerbose?.Invoke($"UpsertSdkMessageProcessingStepImage {imageId} completed");
                    }
                }

            }
        }

        private Guid UpsertServiceEndpoint(ServiceEndpt serviceEndpt, string solutionName, RegistrationTypeEnum registrationType)
        {
            Guid Id = serviceEndpt?.Id ?? Guid.Empty;
            if (Id == Guid.Empty)
            {
                Id = pluginRepository.GetServiceEndpointId(serviceEndpt.Name);
                logWarning?.Invoke($"Extracted id using plugin assembly name {serviceEndpt.Name}");
            }

            var serviceEndpoint = new ServiceEndpoint()
            {
                Name = serviceEndpt.Name,
                NamespaceAddress = serviceEndpt.NamespaceAddress,
                ContractEnum = serviceEndpt.Contract,
                Path = serviceEndpt.Path,
                MessageFormatEnum = serviceEndpt.MessageFormat,
                AuthTypeEnum = serviceEndpt.AuthType,
                SASKeyName = serviceEndpt.SASKeyName,
                SASKey = serviceEndpt.SASKey,
                SASToken = serviceEndpt.SASToken,
                UserClaimEnum = serviceEndpt.UserClaim,
                Description = serviceEndpt.Description,
                Url = serviceEndpt.Url,
                AuthValue = serviceEndpt.AuthValue,
            };

            if (!Id.Equals(Guid.Empty) && registrationType == RegistrationTypeEnum.Reset)
            {
                DeleteObjectWithDependencies(Id, ComponentType.ServiceEndpoint);
            }

            logVerbose?.Invoke($"Trying to upsert {serviceEndpt.Name} / {Id}");
            Id = ExecuteRequest(registrationType, Id, serviceEndpoint);

            AddComponentToSolution(Id, ComponentType.ServiceEndpoint, solutionName);

            return Id;
        }

        public Guid UpsertSdkMessageProcessingStep(EntityReference parentRef, Step step, string solutionName, RegistrationTypeEnum registrationType)
        {
            Guid Id = step.Id ?? Guid.Empty;
            if (Id == Guid.Empty)
            {
                Id = pluginRepository.GetSdkMessageProcessingStepId(parentRef.Id, step.Name);
                logWarning?.Invoke($"Extracted id using plugin step name {step.Name}");
            }

            var sdkMessageId = pluginRepository.GetSdkMessageId(step.MessageName);
            var sdkMessageFilterId = pluginRepository.GetSdkMessageFilterId(step.PrimaryEntityName, sdkMessageId);
            var sdkMessageProcessingStep = new SdkMessageProcessingStep()
            {
                Name = step.Name,
                Description = step.Description,
                SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, sdkMessageId),
                Configuration = step.CustomConfiguration,
                FilteringAttributes = step.FilteringAttributes,
                ImpersonatingUserId = new EntityReference(SystemUser.EntityLogicalName, pluginRepository.GetUserId(step.ImpersonatingUserFullname)),
                ModeEnum = step.Mode,
                SdkMessageFilterId = sdkMessageFilterId.Equals(Guid.Empty) ? null : new EntityReference(SdkMessageFilter.EntityLogicalName, sdkMessageFilterId),
                Rank = step.Rank,
                StageEnum = step.Stage,
                SupportedDeploymentEnum = step.SupportedDeployment,
                EventHandler = parentRef,
                AsyncAutoDelete = step.AsyncAutoDelete,
            };

            Id = ExecuteRequest(registrationType, Id, sdkMessageProcessingStep);
            int stateCode = (int)step.StateCode;
            organizationService.Execute(new SetStateRequest
            {
                EntityMoniker = new EntityReference(sdkMessageProcessingStep.LogicalName, Id),
                State = new OptionSetValue(stateCode),
                Status = new OptionSetValue(stateCode + 1)
            });

            AddComponentToSolution(Id, ComponentType.SDKMessageProcessingStep, solutionName);
            return Id;
        }

        private Guid UpsertSdkMessageProcessingStepImage(Guid parentId, Image image, string solutionName, RegistrationTypeEnum registrationType)
        {
            Guid Id = image.Id ?? Guid.Empty;

            if (Id == Guid.Empty)
            {
                Id = pluginRepository.GetSdkMessageProcessingStepImageId(parentId, image.EntityAlias, image.ImageType);
                logWarning?.Invoke($"Extracted id using plugin step image name {image.EntityAlias}");
            }

            var sdkMessageProcessingStepImage = new SdkMessageProcessingStepImage()
            {
                Attributes1 = image.Attributes,
                EntityAlias = image.EntityAlias,
                MessagePropertyName = image.MessagePropertyName,
                ImageTypeEnum = image.ImageType,
                SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, parentId)
            };

            Id = ExecuteRequest(registrationType, Id, sdkMessageProcessingStepImage);

            return Id;
        }

        private Guid ExecuteRequest(RegistrationTypeEnum registrationType, Guid Id, Entity entity)
        {
            if (Id != Guid.Empty)
            {
                entity.Id = Id;
            }

            if (registrationType == RegistrationTypeEnum.Upsert)
            {
                entity.Id = Id;
                var query = new QueryExpression(entity.LogicalName) { Criteria = new FilterExpression(), ColumnSet = new ColumnSet(columns: new[] { entity.LogicalName + "id" }) };
                query.Criteria.AddCondition(entity.LogicalName + "id", ConditionOperator.Equal, Id);
                var ids = organizationService.RetrieveMultiple(query);

                if ((ids?.Entities.FirstOrDefault()?.Id ?? Guid.Empty) != Guid.Empty)
                {
                    organizationService.Update(entity);
                }
                else
                {
                    organizationService.Create(entity);
                }
            }
            else
            {
                Id = organizationService.Create(entity);
            }

            return Id;
        }

        private void AddComponentToSolution(Guid componentId, ComponentType componentType, string solutionName)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                return;
            }

            logVerbose?.Invoke($"Adding {componentType} {componentId} to solution {solutionName}");
            organizationService.Execute(new AddSolutionComponentRequest
            {
                AddRequiredComponents = false,
                ComponentId = componentId,
                ComponentType = (int)componentType,
                SolutionUniqueName = solutionName
            });
        }

        private IEnumerable<Dependency> GetDependeciesForDelete(Guid objectId, ComponentType? componentType) => ((RetrieveDependenciesForDeleteResponse)organizationService.Execute(new RetrieveDependenciesForDeleteRequest()
        {
            ComponentType = (int)componentType,
            ObjectId = objectId
        })).EntityCollection.Entities.Select(x => x.ToEntity<Dependency>());

        public object GetPluginRegistrationObject(string assemblyPath, string customAttributeClass)
        {
            reflectionLoader.Initialise(assemblyPath, customAttributeClass);
            return pluginRegistrationObjectFactory.GetAssembly(reflectionLoader);
        }

        public Assembly ReadMappingFile(string mappingFile)
        {
            var fileInfo = new FileInfo(mappingFile);
            switch (fileInfo.Extension.ToLower())
            {
                case ".json":
                    logVerbose("Reading mapping json file");
                    var pluginAssembly = Serializers.ParseJson<Assembly>(mappingFile);
                    logVerbose("Deserialized mapping json file");
                    return pluginAssembly;
                case ".xml":
                    logVerbose("Reading mapping xml file");
                    pluginAssembly = Serializers.ParseXml<Assembly>(mappingFile);
                    logVerbose("Deserialized mapping xml file");
                    return pluginAssembly;
                default:
                    throw new ArgumentException("Only .json and .xml mapping files are supported", nameof(ReadMappingFile));
            }
        }
    }
}