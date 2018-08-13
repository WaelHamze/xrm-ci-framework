using System;
using System.Collections.Generic;
using System.Linq;
using Xrm.Framework.CI.PowerShell.Cmdlets.Common;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.PluginRegistration
{
    public class PluginRepository
    {
        private CIContext context { get; }

        public PluginRepository(CIContext context)
        {
            this.context = context;
        }

        public Guid GetUserId(string name) =>
            (from users in context.SystemUserSet
             where users.FullName == name
             select users.Id).FirstOrDefault();

        public Guid GetPluginAssemblyId(string name) =>
            (from a in context.PluginAssemblySet
             where a.Name == name
             select a.Id).FirstOrDefault();

        public Guid GetPluginTypeId(Guid parentId, string name) =>
            (from a in context.PluginTypeSet
             where a.PluginAssemblyId.Id == parentId && a.Name == name
             select a.Id).FirstOrDefault();

        public Guid GetSdkMessageProcessingStepId(Guid parentId, string name) =>
            (from steps in context.SdkMessageProcessingStepSet
             where steps.EventHandler.Id == parentId && steps.Name == name
             select steps.Id).FirstOrDefault();

        public Guid GetSdkMessageProcessingStepImageId(Guid parentId, string name, SdkMessageProcessingStepImage_ImageType? imageType) =>
            (from a in context.SdkMessageProcessingStepImageSet
             where a.SdkMessageProcessingStepId.Id == parentId && a.ImageTypeEnum == imageType && a.EntityAlias == name
             select a.Id).FirstOrDefault();

        public Guid GetSdkMessageId(string name) =>
            (from a in context.SdkMessageSet
             where a.Name == name
             select a.Id).First();

        public Guid GetSdkMessageFilterId(string EntityLogicalName, Guid sdkMessageId) =>
            (from a in context.SdkMessageFilterSet
             where a.PrimaryObjectTypeCode == EntityLogicalName && a.SdkMessageId.Id == sdkMessageId
             select a.Id).FirstOrDefault();

        public Workflow GetWorkflowById(Guid id) => context.CreateQuery<Workflow>().Single(x => x.Id == id);

        public Assembly GetAssemblyRegistration(string assemblyName)
        {
            var lastIndex = assemblyName.LastIndexOf(".dll");
            string name = lastIndex > 0 ? assemblyName.Remove(lastIndex, 4) : assemblyName;
            var pluginAssembly = context.PluginAssemblySet.SingleOrDefault(x => x.Name == name);
            if (pluginAssembly == null)
            {
                return null;
            }

            var pluginAssemblyObject = MapPluginAssemblyObject(pluginAssembly);

            var pluginWorkflowTypes = (from plugins in context.PluginTypeSet
                                       where plugins.PluginAssemblyId.Id == pluginAssembly.Id && plugins.IsWorkflowActivity == true
                                       select MapPluginObject(null, null, null, null, pluginAssemblyObject, plugins)).ToList();

            var pluginStepImages = (from plugins in context.PluginTypeSet
                                    join steps in context.SdkMessageProcessingStepSet on plugins.PluginTypeId equals steps.EventHandler.Id
                                    join images in context.SdkMessageProcessingStepImageSet on steps.SdkMessageProcessingStepId equals images.SdkMessageProcessingStepId.Id
                                    where plugins.PluginAssemblyId.Id == pluginAssembly.Id && plugins.IsWorkflowActivity == false
                                    select images).ToList();

            var pluginTypes = (from plugins in context.PluginTypeSet
                               join steps in context.SdkMessageProcessingStepSet on plugins.PluginTypeId equals steps.EventHandler.Id
                               join message in context.SdkMessageSet on steps.SdkMessageId.Id equals message.SdkMessageId
                               join filters in context.SdkMessageFilterSet on steps.SdkMessageFilterId.Id equals filters.Id
                               where plugins.PluginAssemblyId.Id == pluginAssembly.Id && plugins.IsWorkflowActivity == false
                               select MapPluginObject(steps, message, filters, pluginStepImages, pluginAssemblyObject, plugins)).ToList();

            return pluginAssemblyObject;
        }

        private static Assembly MapPluginAssemblyObject(PluginAssembly pluginAssembly) => new Assembly
        {
            Id = pluginAssembly.PluginAssemblyId,
            Name = pluginAssembly.Name + ".dll",
            IsolationMode = pluginAssembly.IsolationModeEnum,
            SourceType = pluginAssembly.SourceTypeEnum
        };

        private static bool MapPluginObject(SdkMessageProcessingStep pluginStep
            , SdkMessage sdkMessage
            , SdkMessageFilter filter
            , List<SdkMessageProcessingStepImage> images
            , Assembly pluginAssemblyObject
            , PluginType pluginType
           )
        {
            var typeObject = MapPluginAssemblyTypeObject(pluginType, pluginAssemblyObject);

            if (pluginStep != null)
            {
                MapPluginAssemblyStepObject(pluginStep, sdkMessage, filter, images, pluginAssemblyObject, typeObject);
            }

            return true;
        }

        private static Type MapPluginAssemblyTypeObject(PluginType pluginType, Assembly pluginAssembly)
        {
            var type = pluginAssembly.PluginTypes.FirstOrDefault(item1 => item1.Name == pluginType.Name);

            if (type == null)
            {
                type = new Type()
                {
                    Id = pluginType.PluginTypeId,
                    Description = pluginType.Description,
                    FriendlyName = pluginType.FriendlyName,
                    Name = pluginType.Name,
                    TypeName = pluginType.TypeName,
                    WorkflowActivityGroupName = pluginType.IsWorkflowActivity == true ? pluginType.WorkflowActivityGroupName ?? pluginAssembly.Name : null,
                    Steps = new List<Step>()
                };

                pluginAssembly.PluginTypes.Add(type);
            }

            return type;
        }

        private static void MapPluginAssemblyStepObject(SdkMessageProcessingStep pluginStep
            , SdkMessage sdkMessage
            , SdkMessageFilter filter
            , List<SdkMessageProcessingStepImage> images
            , Assembly pluginAssembly
            , Type pluginType)
        {
            var step = new Step()
            {
                Id = pluginStep.SdkMessageProcessingStepId,
                CustomConfiguration = pluginStep.Configuration,
                Name = pluginStep.Name,
                Description = pluginStep.Description,
                FilteringAttributes = pluginStep.FilteringAttributes,
                ImpersonatingUserFullname = pluginStep.ImpersonatingUserId?.Name ?? string.Empty,
                MessageName = sdkMessage?.CategoryName,
                Mode = pluginStep.ModeEnum,
                PrimaryEntityName = filter.PrimaryObjectTypeCode,
                Rank = pluginStep.Rank,
                Stage = pluginStep.StageEnum,
                SupportedDeployment = pluginStep.SupportedDeploymentEnum,
                Images = new List<Image>()
            };
            MapImagesObject(images, pluginStep, step);
            pluginType.Steps.Add(step);
        }

        private static void MapImagesObject(List<SdkMessageProcessingStepImage> images, SdkMessageProcessingStep pluginStep, Step step)
        {
            var imagesTemp = images.FindAll(item => item.SdkMessageProcessingStepId.Id == pluginStep.SdkMessageProcessingStepId);
            foreach (var image in imagesTemp)
            {
                var imageTemp = new Image()
                {
                    Id = image.SdkMessageProcessingStepImageId,
                    Attributes = image.Attributes1,
                    EntityAlias = image.EntityAlias,
                    MessagePropertyName = image.MessagePropertyName,
                    ImageType = image.ImageTypeEnum
                };

                step.Images.Add(imageTemp);
            }
        }
    }
}
