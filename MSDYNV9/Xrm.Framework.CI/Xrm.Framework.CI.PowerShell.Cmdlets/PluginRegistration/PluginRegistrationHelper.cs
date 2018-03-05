using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class PluginRegistrationHelper
    {
        private IOrganizationService OrganizationService;
        private CIContext context;

        public PluginRegistrationHelper(IOrganizationService service, CIContext xrmContext)
        {
            this.OrganizationService = service;
            this.context = xrmContext;
        }

        public Guid UpsertPluginAssembly(Assembly pluginAssembly, string version, string content)
        {
            var lastIndex = pluginAssembly.Name.LastIndexOf(".dll");
            string name = lastIndex > 0 ? pluginAssembly.Name.Remove(lastIndex, 4) : pluginAssembly.Name;
            
            Guid Id = GetPluginAssemblyId(name);

            var assembly = new PluginAssembly()
            {
                Version = version,
                Content = content,
                Name = name,
                SourceType = new OptionSetValue(pluginAssembly.SourceType),
                IsolationMode = new OptionSetValue(pluginAssembly.IsolationMode),
            };

            if (Id.Equals(Guid.Empty))
            {
                Id = OrganizationService.Create(assembly);
            }
            else
            {
                assembly.Id = Id;
                OrganizationService.Update(assembly);
            }

            return Id;
        }

        private Guid GetPluginAssemblyId(string name)
        {   
            var query = from a in context.PluginAssemblySet
                        where a.Name == name
                        select a.Id;

            Guid Id = query.FirstOrDefault();
            
            return Id;
        }

        public Guid UpsertPluginType(Guid parentId, Type pluginType)
        {
            var name = pluginType.Name;
            Guid Id = GetPluginTypeId(parentId, name);

            var type = new PluginType()
            {
                Name = name,
                Description = pluginType.Description,
                FriendlyName = pluginType.FriendlyName,
                TypeName = pluginType.TypeName,
                PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, parentId)
            };

            if (Id.Equals(Guid.Empty))
            {
                Id = OrganizationService.Create(type);
            }
            else
            {
                type.Id = Id;
                OrganizationService.Update(type);
            }

            return Id;
        }

        private Guid GetPluginTypeId(Guid parentId, string name)
        {
            var query = from a in context.PluginTypeSet
                        where a.PluginAssemblyId.Id == parentId && a.Name == name
                        select a.Id;

            Guid Id = query.FirstOrDefault();
            
            return Id;
        }

        public Guid UpsertSdkMessageProcessingStep(Guid parentId, Step step)
        {
            var name = step.Name;
            Guid Id = GetSdkMessageProcessingStepId(parentId, name);
            var sdkMessageId = GetSdkMessageId(step.MessageName);
            var sdkMessageFilterId = GetSdkMessageFilterId(step.PrimaryEntityName, sdkMessageId);
            var sdkMessageProcessingStep = new SdkMessageProcessingStep()
            {
                Name = name,
                Description = step.Description,
                SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, sdkMessageId),
                Configuration = step.CustomConfiguration,
                FilteringAttributes = step.FilteringAttributes,
                ImpersonatingUserId = new EntityReference(SystemUser.EntityLogicalName, step.ImpersonatingUserId),
                Mode = new OptionSetValue(step.Mode),
                SdkMessageFilterId = new EntityReference(SdkMessageFilter.EntityLogicalName, sdkMessageFilterId),
                Rank = step.Rank,
                Stage = new OptionSetValue(step.Stage),
                SupportedDeployment = new OptionSetValue(step.SupportedDeployment),
                EventHandler = new EntityReference(PluginType.EntityLogicalName, parentId)
            };

            if (Id.Equals(Guid.Empty))
            {
                Id = OrganizationService.Create(sdkMessageProcessingStep);
            }
            else
            {
                sdkMessageProcessingStep.Id = Id;
                OrganizationService.Update(sdkMessageProcessingStep);
            }

            return Id;
        }

        private Guid GetSdkMessageProcessingStepId(Guid parentId, string name)
        {
            var query = from steps in context.SdkMessageProcessingStepSet
                        where steps.EventHandler.Id == parentId && steps.Name == name
                        select steps.Id;

            Guid Id = query.FirstOrDefault();
            
            return Id;
        }

        private Guid GetSdkMessageId(string name)
        {
            try
            {
                //GET SDK MESSAGE QUERY
                var query = from a in context.SdkMessageSet
                            where a.Name == name
                            select a.Id;

                Guid Id = query.FirstOrDefault();

                if (Id == null || Id == Guid.Empty)
                {
                    //throw new ItemNotFoundException(string.Format("{0} was not found", assemblyName));
                    throw new Exception(string.Format("{0} was not found", name));
                }

                return Id;
            }
            catch (InvalidPluginExecutionException invalidPluginExecutionException)
            {
                throw invalidPluginExecutionException;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private Guid GetSdkMessageFilterId(string EntityLogicalName, Guid sdkMessageId)
        {
            try
            {
                //GET SDK MESSAGE FILTER QUERY
                var query = from a in context.SdkMessageFilterSet
                            where a.PrimaryObjectTypeCode == EntityLogicalName && a.SdkMessageId.Id == sdkMessageId
                            select a.Id;

                Guid Id = query.FirstOrDefault();

                return Id;
            }
            catch (InvalidPluginExecutionException invalidPluginExecutionException)
            {
                throw invalidPluginExecutionException;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        
        public Guid UpsertSdkMessageProcessingStepImage(Guid parentId, Image image)
        {
            var name = image.EntityAlias;
            var imageType = image.ImageType;
            Guid Id = GetSdkMessageProcessingStepImageId(parentId, name, imageType);

            var sdkMessageProcessingStepImage = new SdkMessageProcessingStepImage()
            {
                Attributes1 = image.Attributes,
                EntityAlias = image.EntityAlias,
                MessagePropertyName = image.MessagePropertyName,
                ImageType = new OptionSetValue((int)image.ImageType),
                SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, parentId)
            };

            if (Id.Equals(Guid.Empty))
            {
                Id = OrganizationService.Create(sdkMessageProcessingStepImage);
            }
            else
            {
                sdkMessageProcessingStepImage.Id = Id;
                OrganizationService.Update(sdkMessageProcessingStepImage);
            }

            return Id;
        }

        private Guid GetSdkMessageProcessingStepImageId(Guid parentId, string name, int? imageType)
        {
            var query = from a in context.SdkMessageProcessingStepImageSet
                        where a.SdkMessageProcessingStepId.Id == parentId && a.ImageType.Value == imageType && a.EntityAlias == name
                        select a.Id;

            Guid Id = query.FirstOrDefault();

            return Id;
        }

        public string GetJsonMappingFromCrm(string assemblyName)
        {
            var lastIndex = assemblyName.LastIndexOf(".dll");
            string name = assemblyName.Remove(lastIndex, 4);
            var pluginAssemblyList = new List<Assembly>();
            var pluginStepImages = (from pluginAssembly in context.PluginAssemblySet
                                    join plugins in context.PluginTypeSet on pluginAssembly.Id equals plugins.PluginAssemblyId.Id
                                    join steps in context.SdkMessageProcessingStepSet on plugins.PluginTypeId equals steps.EventHandler.Id
                                    join images in context.SdkMessageProcessingStepImageSet on steps.SdkMessageProcessingStepId equals images.SdkMessageProcessingStepId.Id
                                    where pluginAssembly.Name == name
                                    select images).ToList();
            var pluginAssembliesTypes = (from pluginAssembly in context.PluginAssemblySet
                                         join plugins in context.PluginTypeSet on pluginAssembly.Id equals plugins.PluginAssemblyId.Id
                                         join steps in context.SdkMessageProcessingStepSet on plugins.PluginTypeId equals steps.EventHandler.Id
                                         join message in context.SdkMessageSet on steps.SdkMessageId.Id equals message.SdkMessageId
                                         join filters in context.SdkMessageFilterSet on steps.SdkMessageFilterId.Id equals filters.Id
                                         where pluginAssembly.Name == name
                                         select MapObject(pluginAssemblyList, pluginAssembly, plugins, steps, message, filters, pluginStepImages)).ToList();
            string json = JsonConvert.SerializeObject(pluginAssemblyList.FirstOrDefault());
            return json;
        }

        private static Assembly MapPluginAssemblyObject(List<Assembly> pluginAssemblyList, PluginAssembly pluginAssemblies)
        {
            var pluginAssemblyTemp = pluginAssemblyList.Find(item => item.Name == pluginAssemblies.Name + ".dll");
            if (pluginAssemblyTemp == null)
            {
                pluginAssemblyTemp = new Assembly()
                {
                    Id = pluginAssemblies.Id,
                    Name = pluginAssemblies.Name + ".dll",
                    IsolationMode = pluginAssemblies.IsolationMode.Value,
                    SourceType = pluginAssemblies.SourceType.Value,
                    PluginTypes = new List<Type>()
                };

                pluginAssemblyList.Add(pluginAssemblyTemp);
            }

            return pluginAssemblyTemp;
        }

        private static bool MapObject(List<Assembly> pluginAssemblyList
            , PluginAssembly pluginAssembly
            , PluginType pluginType
            , SdkMessageProcessingStep pluginStep
            , SdkMessage sdkMessage
            , SdkMessageFilter filter
            , List<SdkMessageProcessingStepImage> images)
        {
            var pluginAssemblyTemp = MapPluginAssemblyObject(pluginAssemblyList, pluginAssembly);
            if (pluginAssemblyTemp == null) { return false; }
            var pluginAssemblyTypeTemp = MapPluginAssemblyTypeObject(pluginType, pluginAssemblyTemp);

            if (pluginStep != null)
            {
                MapPluginAssemblyStepObject(pluginType, pluginStep, sdkMessage, filter, images, pluginAssemblyTemp, pluginAssemblyTypeTemp);
            }

            return true;
        }

        private static void MapPluginAssemblyStepObject(PluginType pluginType
            , SdkMessageProcessingStep pluginStep
            , SdkMessage sdkMessage
            , SdkMessageFilter filter
            , List<SdkMessageProcessingStepImage> images
            , Assembly pluginAssemblyTemp
            , Type pluginAssemblyTypeTemp)
        {
            var pluginAssemblyStepTemp = pluginAssemblyTemp.PluginTypes.FirstOrDefault(item1 => item1.Name == pluginType.Name)
                .Steps.FirstOrDefault<Step>(item => item.Name == pluginStep.Name);
            if (pluginAssemblyStepTemp == null)
            {
                pluginAssemblyStepTemp = new Step()
                {
                    Id = pluginStep.Id,
                    CustomConfiguration = pluginStep.Configuration,
                    Name = pluginStep.Name,
                    Description = pluginStep.Description,
                    FilteringAttributes = pluginStep.FilteringAttributes,
                    ImpersonatingUserId = pluginStep.ImpersonatingUserId == null ? Guid.Empty : pluginStep.ImpersonatingUserId.Id,
                    MessageName = sdkMessage != null ? sdkMessage.CategoryName : null,
                    Mode = pluginStep.Mode.Value,
                    PrimaryEntityName = filter.PrimaryObjectTypeCode,
                    Rank = pluginStep.Rank,
                    Stage = pluginStep.Stage.Value,
                    SupportedDeployment = pluginStep.SupportedDeployment.Value,
                    Images = new List<Image>()
                };
                MapImagesObject(images, pluginStep, pluginAssemblyStepTemp);
                pluginAssemblyTypeTemp.Steps.Add(pluginAssemblyStepTemp);

            }
        }

        private static Type MapPluginAssemblyTypeObject(PluginType pluginType, Assembly pluginAssemblyTemp)
        {
            var pluginAssemblyTypeTemp = pluginAssemblyTemp.PluginTypes.FirstOrDefault(item1 => item1.Name == pluginType.Name);
            if (pluginAssemblyTypeTemp == null)
            {
                pluginAssemblyTypeTemp = new Type()
                {
                    Id = pluginType.Id,
                    Description = pluginType.Description,
                    FriendlyName = pluginType.FriendlyName,
                    Name = pluginType.Name,
                    TypeName = pluginType.TypeName,
                    Steps = new List<Step>()
                };

                pluginAssemblyTemp.PluginTypes.Add(pluginAssemblyTypeTemp);
            }
            return pluginAssemblyTypeTemp;
        }

        private static Image MapImagesObject(List<SdkMessageProcessingStepImage> images, SdkMessageProcessingStep pluginStep, Step step)
        {
            Image imageTemp = null;
            var imagesTemp = images.FindAll(item => item.SdkMessageProcessingStepId.Id == pluginStep.SdkMessageProcessingStepId);
            foreach (var image in imagesTemp)
            {
                imageTemp = new Image()
                {
                    Id = image.Id,
                    Attributes = image.Attributes1,
                    EntityAlias = image.EntityAlias,
                    MessagePropertyName = image.MessagePropertyName,
                    ImageType = image.ImageType != null ? image.ImageType.Value : (int?)null
                };

                step.Images.Add(imageTemp);
            }

            return imageTemp;
        }
    }
}
