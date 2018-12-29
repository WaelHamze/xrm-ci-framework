using System.Management.Automation;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns C# custom attribute class used for plugin registration</para>
    /// <para type="description">This cmdlet generates C# code for a custom attribute to be used in a
    ///   plugin or workflow project. The class can be used to generate the registration json used in
    ///   the SetXrmPluginRegistration cmdlet
    /// </para>
    /// </summary>
    /// <example>
    ///   <code>C:\PS>Get-XrmPluginRegistrationClass</code>
    ///   <para>Returns code using namespace "XrmCiFramework"</para>
    /// </example>
    /// <example>
    ///   <code>C:\PS>Get-XrmPluginRegistrationClass -Namespace "MySolution.MyNamespace"</code>
    ///   <para>Returns code using namespace "MySolution.MyNamespace"</para>
    /// </example>
    /// <example>
    ///   <code>C:\PS>Get-XrmPluginRegistrationClass > "d:\repos\xrmproj\Plugins\XrmCiPluginRegistration.cs"</code>
    ///   <para>Write the code to file "d:\repos\xrmproj\Plugins\XrmCiPluginRegistration.cs"</para>
    /// </example>
    [Cmdlet("Get", "XrmPluginRegistrationClass")]
    [OutputType(typeof(string))]
    public class GetXrmPluginRegistrationClassCommand : Cmdlet
    {
        #region Parameters

        /// <summary>
        /// <para type="description">The namespace used in the generated class</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Namespace { get; set; }

        #endregion

        #region Process Record

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (string.IsNullOrEmpty(Namespace)) Namespace = "XrmCiFramework";
            base.WriteVerbose($"Generating custom attribute class with namespace: {Namespace}");
            WriteObject(ClassText.Replace("#NAMESPACE#", Namespace));
        }
        #endregion

        #region Class Text
        private static string ClassText => @"using System;

namespace #NAMESPACE#
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class XrmCiPluginRegistration : Attribute
    {
        #region Members
        public PluginIsolationMode IsolationMode { get; private set; }
        public PluginMessage Message { get; private set; }
        public string EntityLogicalName { get; private set; }
        public string FilteringAttributes { get; private set; }
        public int ExecutionOrder { get; private set; }
        public PluginStage? Stage { get; private set; }
        public PluginExecutionMode ExecutionMode { get; private set; }

        public string FriendlyName { get; set; }
        public string WorkflowGroupName { get; set; }
        public string Image1Name { get; set; }
        public string Image1Attributes { get; set; }
        public string Image2Name { get; set; }
        public string Image2Attributes { get; set; }
        public string Description { get; set; }
        public bool DeleteAsyncOperation { get; set; }
        public string CustomConfiguration { get; set; }
        public bool Offline { get; set; }
        public SdkMessageProcessingStepState State { get; set; }
        public PluginImageType Image1Type { get; set; }
        public PluginImageType Image2Type { get; set; }
        public string Name { get; set; }
        public PluginSourceType SourceType { get; set; }
        public string ImpersonatingUser { get; set; }
        public SdkMessageProcessingStep_SupportedDeployment SupportedDeployment { get; set; }

        #endregion

        //Plugin - one image
        public XrmCiPluginRegistration(
            string entityLogicalName,
            string description,
            PluginIsolationMode isolationMode,
            PluginSourceType sourceType,
            string filters,
            int order,
            PluginStage stage,
            PluginExecutionMode executionMode,
            PluginMessage message,
            string image1Name,
            string image1Attributes,
            PluginImageType image1Type,
            SdkMessageProcessingStepState state = SdkMessageProcessingStepState.Enabled,
            bool deleteAsyncOperation = false,
            string customConfiguration = """",
            string impersonatingUser = """",
            bool offline = false,
            SdkMessageProcessingStep_SupportedDeployment supportedDeployment = SdkMessageProcessingStep_SupportedDeployment.ServerOnly
        )
        {
            EntityLogicalName = entityLogicalName;
            Description = description;
            IsolationMode = isolationMode;
            SourceType = sourceType;
            FilteringAttributes = filters;
            ExecutionOrder = order;
            ExecutionMode = executionMode;
            Stage = stage;
            Message = message;
            Image1Name = image1Name;
            Image1Attributes = image1Attributes;
            Image1Type = image1Type;
            State = state;
            DeleteAsyncOperation = deleteAsyncOperation;
            CustomConfiguration = customConfiguration;
            ImpersonatingUser = impersonatingUser;
            Offline = offline;
            SupportedDeployment = supportedDeployment;
        }

        //Plugin - two images
        public XrmCiPluginRegistration(
            string entityLogicalName,
            string description,
            PluginIsolationMode isolationMode,
            PluginSourceType sourceType,
            string filters,
            int order,
            PluginStage stage,
            PluginExecutionMode executionMode,
            PluginMessage message,
            string image1Name,
            string image1Attributes,
            PluginImageType image1Type,
            string image2Name,
            string image2Attributes,
            PluginImageType image2Type,
            SdkMessageProcessingStepState state = SdkMessageProcessingStepState.Enabled,
            bool deleteAsyncOperation=false,
            string customConfiguration = """",
            string impersonatingUser = """",
            bool offline = false,
            SdkMessageProcessingStep_SupportedDeployment supportedDeployment = SdkMessageProcessingStep_SupportedDeployment.ServerOnly
        )
        {
            EntityLogicalName = entityLogicalName;
            Description = description;
            IsolationMode = isolationMode;
            SourceType = sourceType;
            FilteringAttributes = filters;
            ExecutionOrder = order;
            ExecutionMode = executionMode;
            Stage = stage;
            Message = message;
            Image1Name = image1Name;
            Image1Attributes = image1Attributes;
            Image1Type = image1Type;
            Image2Name = image2Name;
            Image2Attributes = image2Attributes;
            Image2Type = image2Type;
            State = state;
            DeleteAsyncOperation = deleteAsyncOperation;
            CustomConfiguration = customConfiguration;
            ImpersonatingUser = impersonatingUser;
            Offline = offline;
            SupportedDeployment = supportedDeployment;
        }

        //Plugin - no steps
        public XrmCiPluginRegistration(
            string entityLogicalName,
            string description,
            PluginIsolationMode isolationMode,
            PluginSourceType sourceType
        )
        {
            EntityLogicalName = entityLogicalName;
            Description = description;
            IsolationMode = isolationMode;
            SourceType = sourceType;
        }

        //Workflow
        public XrmCiPluginRegistration(
            string description,
            PluginIsolationMode isolationMode,
            PluginSourceType sourceType,
            string friendlyName,
            string workflowGroupName)
        {
            Description = description;
            IsolationMode = isolationMode;
            SourceType = sourceType;
            FriendlyName = friendlyName;
            WorkflowGroupName = workflowGroupName;
        }
    }

    #region Enums
    public enum PluginExecutionMode
    {
        Synchronous = 0,
        Asynchronous = 1
    }

    public enum PluginImageType
    {
        PreImage = 0,
        PostImage = 1
    }

    public enum PluginIsolationMode
    {
        None = 0,
        Sandbox = 1
    }

    public enum PluginStage
    {
        Prevalidation = 10,
        Preoperation = 20,
        Postoperation = 40
    }

    public enum PluginSourceType
    {
        Database = 0,
        Disk = 1,
        Normal = 2,
        AzureWebApp = 3
    }

    public enum SdkMessageProcessingStep_SupportedDeployment
    {

        ServerOnly = 0,
        MicrosoftDynamics365ClientforOutlookOnly = 1,
        Both = 2
    }

    public enum SdkMessageProcessingStepState
    {
        Enabled = 0,
        Disabled = 1,
    }

    public enum PluginMessage
    {
        AddItem,
        AddListMembers,
        AddMember,
        AddMembers,
        AddPrincipalToQueue,
        AddPrivileges,
        AddProductToKit,
        AddRecurrence,
        AddToQueue,
        AddUserToRecordTeam,
        ApplyRecordCreationAndUpdateRule,
        Assign,
        Associate,
        BackgroundSend,
        Book,
        CalculatePrice,
        Cancel,
        CheckIncoming,
        CheckPromote,
        Clone,
        CloneMobileOfflineProfile,
        CloneProduct,
        Close,
        CopyDynamicListToStatic,
        CopySystemForm,
        Create,
        CreateException,
        CreateInstance,
        CreateKnowledgeArticleTranslation,
        CreateKnowledgeArticleVersion,
        Delete,
        DeleteOpenInstances,
        DeliverIncoming,
        DeliverPromote,
        Disassociate,
        Execute,
        ExecuteById,
        Export,
        GenerateSocialProfile,
        GetDefaultPriceLevel,
        GrantAccess,
        Import,
        LockInvoicePricing,
        LockSalesOrderPricing,
        Lose,
        Merge,
        ModifyAccess,
        PickFromQueue,
        Publish,
        PublishAll,
        PublishTheme,
        QualifyLead,
        Recalculate,
        ReleaseToQueue,
        RemoveFromQueue,
        RemoveItem,
        RemoveMember,
        RemoveMembers,
        RemovePrivilege,
        RemoveProductFromKit,
        RemoveRelated,
        RemoveUserFromRecordTeam,
        ReplacePrivileges,
        Reschedule,
        Retrieve,
        RetrieveExchangeRate,
        RetrieveFilteredForms,
        RetrieveMultiple,
        RetrievePersonalWall,
        RetrievePrincipalAccess,
        RetrieveRecordWall,
        RetrieveSharedPrincipalsAndAccess,
        RetrieveUnpublished,
        RetrieveUnpublishedMultiple,
        RetrieveUserQueues,
        RevokeAccess,
        RouteTo,
        Send,
        SendFromTemplate,
        SetLocLabels,
        SetRelated,
        SetState,
        SetStateDynamicEntity,
        TriggerServiceEndpointCheck,
        UnlockInvoicePricing,
        UnlockSalesOrderPricing,
        Update,
        ValidateRecurrenceRule,
        Win
    }
    #endregion
}";

        #endregion
    }
}

