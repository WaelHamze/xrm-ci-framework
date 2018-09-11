﻿using System;
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
             select a.Id).FirstOrDefault();

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

        public Guid GetServiceEndpointId(string name) =>
            (from a in context.ServiceEndpointSet
             where a.Name == name
             select a.Id).FirstOrDefault();
        public List<ServiceEndpt> GetServiceEndpoints(Guid solutionId)
        {
            var webHookList = new List<ServiceEndpt>();
            var pluginAssembliesTypes = (from serviceEndpoint in context.ServiceEndpointSet
                                         join steps in context.SdkMessageProcessingStepSet on serviceEndpoint.ServiceEndpointId equals steps.EventHandler.Id
                                         join message in context.SdkMessageSet on steps.SdkMessageId.Id equals message.SdkMessageId
                                         join filters in context.SdkMessageFilterSet on steps.SdkMessageFilterId.Id equals filters.Id
                                         // where serviceEndpoint.SolutionId == solutionId
                                         select MapWebHookObject(webHookList, serviceEndpoint, steps, message, filters)).ToList();
            return webHookList;
        }

        private static bool MapWebHookObject(List<ServiceEndpt> webHookList, ServiceEndpoint serviceEndpoint, SdkMessageProcessingStep steps, SdkMessage message, SdkMessageFilter filters)
        {
            var webHook = MapWebHook(serviceEndpoint);
            webHook.Steps.Add(MapStep(steps, message, filters));
            webHookList.Add(webHook);

            return true;
        }

        private static ServiceEndpt MapWebHook(ServiceEndpoint serviceEndpoint) => new ServiceEndpt()
        {
            Id = serviceEndpoint.ServiceEndpointId,
            Name = serviceEndpoint.Name,
            NamespaceAddress = serviceEndpoint.NamespaceAddress,
            Contract = serviceEndpoint.ContractEnum,
            Path = serviceEndpoint.Path,
            MessageFormat = serviceEndpoint.MessageFormatEnum,
            SASKey = serviceEndpoint.SASKey,
            SASKeyName = serviceEndpoint.SASKeyName,
            SASToken = serviceEndpoint.SASToken,
            UserClaim = serviceEndpoint.UserClaimEnum,
            Description = serviceEndpoint.Description,
            Url = serviceEndpoint.Url,
            AuthType = serviceEndpoint.AuthTypeEnum,
            AuthValue = serviceEndpoint.AuthValue,
            Steps = new List<Step>(),
        };

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
            var step = MapStep(pluginStep, sdkMessage, filter);
            MapImagesObject(images, pluginStep, step);
            pluginType.Steps.Add(step);
        }

        private static Step MapStep(SdkMessageProcessingStep pluginStep, SdkMessage sdkMessage, SdkMessageFilter filter) => new Step()
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
