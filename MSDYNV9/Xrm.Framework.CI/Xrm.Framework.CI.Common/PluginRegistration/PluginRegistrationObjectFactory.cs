using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xrm.Framework.CI.Common.Entities;


namespace Xrm.Framework.CI.Common
{
    public interface IPluginRegistrationObjectFactory
    {
        Assembly GetAssembly(IReflectionLoader reflectionLoader);
    }

    public class PluginRegistrationObjectFactory : IPluginRegistrationObjectFactory
    {
        public Assembly GetAssembly(IReflectionLoader reflectionLoader)
        {
            return StaticPluginRegistrationObjectFactory.BuildAssembly(reflectionLoader);
        }

        private static class StaticPluginRegistrationObjectFactory
        {
            public static Assembly BuildAssembly(IReflectionLoader reflectionLoader)
            {
                var isolationModes = reflectionLoader.Constructors
                    .SelectMany(c => c.ArgsList)
                    .Where(d => d.TryGetValue("isolationMode", out _))
                    .Select(d => (PluginAssembly_IsolationMode)Convert.ToInt32(d["isolationMode"]))
                    .Distinct()
                    .ToList();

                var sourceTypes = reflectionLoader.Constructors
                    .SelectMany(c => c.ArgsList)
                    .Where(d => d.TryGetValue("sourceType", out _))
                    .Select(d => (PluginAssembly_SourceType)Convert.ToInt32(d["sourceType"]))
                    .Distinct()
                    .ToList();

                var assembly = new Assembly
                {
                    Id = Guid.NewGuid(),
                    Name = reflectionLoader.AssemblyName,
                    IsolationMode = isolationModes.First(),
                    SourceType = sourceTypes.First()
                };

                foreach (var constructor in reflectionLoader.Constructors)
                {
                    assembly.PluginTypes.Add(BuildType(constructor.ArgsList, constructor.ClassName));
                }

                return assembly;
            }

            private static Type BuildType(List<Dictionary<string, object>> argsList, string className)
            {
                var type = new Type
                {
                    Id = (Guid?)GetMappedObject(argsList.First(), "typeId", Guid.NewGuid()),
                    Description = (string)GetMappedObject(argsList.First(), "description"),
                    FriendlyName = className,
                    Name = className,
                    TypeName = className,
                    WorkflowActivityGroupName = (string)GetMappedObject(argsList.First(), "workflowGroupName", "")
                };

                type.Steps = new List<Step>();
                foreach (var args in argsList)
                {
                    type.Steps.Add(BuildStep(args, className));
                }

                type.Steps.RemoveAll(x => x == null);

                return type;
            }

            private static Step BuildStep(Dictionary<string, object> args, string className)
            {
                if (!args.ContainsKey("executionMode"))
                    return null;

                var step = new Step
                {
                    Id = (Guid?)GetMappedObject(args, "stepId", Guid.NewGuid()),
                    CustomConfiguration = (string)GetMappedObject(args, "customConfiguration"),
                    FilteringAttributes = (string)GetMappedObject(args, "filters"),
                    ImpersonatingUserFullname = (string)GetMappedObject(args, "impersonatingUser"),
                    MessageName = (string)GetMappedObject(args, "message"),
                    PrimaryEntityName = (string)GetMappedObject(args, "entityLogicalName"),
                    Mode = ParseEnum<SdkMessageProcessingStep_Mode>(GetMappedObject(args, "executionMode").ToString()),
                    Name = className,
                    Description = GetStepDescription(args),
                    Rank = (int?)GetMappedObject(args, "order"),
                    Stage = ParseEnum<SdkMessageProcessingStep_Stage>(GetMappedObject(args, "stage").ToString()),
                    SupportedDeployment =
                        ParseEnum<SdkMessageProcessingStep_SupportedDeployment>(
                            GetMappedObject(args, "supportedDeployment").ToString()),
                    Images = new List<Image> { BuildImage(args, 1), BuildImage(args, 2) },
                    AsyncAutoDelete = (bool)GetMappedObject(args, "deleteAsyncOperation"),
                    StateCode = ParseEnum<SdkMessageProcessingStepState>(
                        GetMappedObject(args, "state").ToString())
                };
                step.Images.RemoveAll(x => x == null);
                return step;
            }

            private static Image BuildImage(Dictionary<string, object> args, int imageNumber)
            {

                var imageSection = "image" + imageNumber;
                var image = new Image
                {
                    Attributes = (string)GetMappedObject(args, imageSection + "Attributes", ""),
                    MessagePropertyName = GetMessagePropertyName(args),
                    Id = Guid.NewGuid(),
                    EntityAlias = ParseEnum<SdkMessageProcessingStepImage_ImageType>(
                        GetMappedObject(args, imageSection + "Type")?.ToString()).ToString(),
                    ImageType = ParseEnum<SdkMessageProcessingStepImage_ImageType>(
                        GetMappedObject(args, imageSection + "Type")?.ToString())
                };
                return image.ImageType == null ? null : image;
            }

            private static object GetMappedObject(Dictionary<string, object> args, string mapping, object defaultReturn = null)
            {
                var obj = args.FirstOrDefault(m => m.Key == mapping);
                return obj.Value ?? defaultReturn;
            }

            private static T? ParseEnum<T>(string value) where T : struct, IConvertible
            {
                if (value == "null" || string.IsNullOrEmpty(value))
                    return (T?)null;
                return (T?)Enum.Parse(typeof(T), value, true);
            }

            private static string GetStepDescription(Dictionary<string, object> args)
            {
                var stage = ParseEnum<SdkMessageProcessingStep_Stage>(GetMappedObject(args,
                    "stage").ToString()).ToString();

                var entity = (string)GetMappedObject(args, "entityLogicalName");
                var msg = (string)GetMappedObject(args, "message");

                return $"{stage} {msg} of {entity}";
            }

            private static string GetMessagePropertyName(Dictionary<string, object> args)
            {
                var message = (string)GetMappedObject(args, "message");
                if (message == "SetState" || message == "SetStateDynamicEntity") return "entityMoniker";

                if (message != "Create") return "Target";

                return ParseEnum<SdkMessageProcessingStep_Stage>(GetMappedObject(args, "stage").ToString()) == SdkMessageProcessingStep_Stage.Postoperation ? "id" : "Target";
            }
        }
    }
}