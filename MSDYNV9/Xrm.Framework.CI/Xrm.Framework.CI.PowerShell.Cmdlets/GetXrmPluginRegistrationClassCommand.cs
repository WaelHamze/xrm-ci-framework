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
        public string Message { get; private set; }
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

        //Plugin - no images
        public XrmCiPluginRegistration(
            string entityLogicalName,
            string description,
            PluginIsolationMode isolationMode,
            PluginSourceType sourceType,
            string filters,
            int order,
            PluginStage stage,
            PluginExecutionMode executionMode,
            string message,
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
            State = state;
            DeleteAsyncOperation = deleteAsyncOperation;
            CustomConfiguration = customConfiguration;
            ImpersonatingUser = impersonatingUser;
            Offline = offline;
            SupportedDeployment = supportedDeployment;
        }

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
            string message,
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
            string message,
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

        #region MessageConsts
        public const string AddItem = ""AddItem"";
        public const string AddListMembers = ""AddListMembers"";
        public const string AddMember = ""AddMember"";
        public const string AddMembers = ""AddMembers"";
        public const string AddPrincipalToQueue = ""AddPrincipalToQueue"";
        public const string AddPrivileges = ""AddPrivileges"";
        public const string AddProductToKit = ""AddProductToKit"";
        public const string AddRecurrence = ""AddRecurrence"";
        public const string AddToQueue = ""AddToQueue"";
        public const string AddUserToRecordTeam = ""AddUserToRecordTeam"";
        public const string ApplyRecordCreationAndUpdateRule = ""ApplyRecordCreationAndUpdateRule"";
        public const string Assign = ""Assign"";
        public const string Associate = ""Associate"";
        public const string BackgroundSend = ""BackgroundSend"";
        public const string Book = ""Book"";
        public const string CalculatePrice = ""CalculatePrice"";
        public const string Cancel = ""Cancel"";
        public const string CheckIncoming = ""CheckIncoming"";
        public const string CheckPromote = ""CheckPromote"";
        public const string Clone = ""Clone"";
        public const string CloneMobileOfflineProfile = ""CloneMobileOfflineProfile"";
        public const string CloneProduct = ""CloneProduct"";
        public const string Close = ""Close"";
        public const string CopyDynamicListToStatic = ""CopyDynamicListToStatic"";
        public const string CopySystemForm = ""CopySystemForm"";
        public const string Create = ""Create"";
        public const string CreateException = ""CreateException"";
        public const string CreateInstance = ""CreateInstance"";
        public const string CreateKnowledgeArticleTranslation = ""CreateKnowledgeArticleTranslation"";
        public const string CreateKnowledgeArticleVersion = ""CreateKnowledgeArticleVersion"";
        public const string Delete = ""Delete"";
        public const string DeleteOpenInstances = ""DeleteOpenInstances"";
        public const string DeliverIncoming = ""DeliverIncoming"";
        public const string DeliverPromote = ""DeliverPromote"";
        public const string Disassociate = ""Disassociate"";
        public const string Execute = ""Execute"";
        public const string ExecuteById = ""ExecuteById"";
        public const string Export = ""Export"";
        public const string GenerateSocialProfile = ""GenerateSocialProfile"";
        public const string GetDefaultPriceLevel = ""GetDefaultPriceLevel"";
        public const string GrantAccess = ""GrantAccess"";
        public const string Import = ""Import"";
        public const string LockInvoicePricing = ""LockInvoicePricing"";
        public const string LockSalesOrderPricing = ""LockSalesOrderPricing"";
        public const string Lose = ""Lose"";
        public const string Merge = ""Merge"";
        public const string ModifyAccess = ""ModifyAccess"";
        public const string PickFromQueue = ""PickFromQueue"";
        public const string Publish = ""Publish"";
        public const string PublishAll = ""PublishAll"";
        public const string PublishTheme = ""PublishTheme"";
        public const string QualifyLead = ""QualifyLead"";
        public const string Recalculate = ""Recalculate"";
        public const string ReleaseToQueue = ""ReleaseToQueue"";
        public const string RemoveFromQueue = ""RemoveFromQueue"";
        public const string RemoveItem = ""RemoveItem"";
        public const string RemoveMember = ""RemoveMember"";
        public const string RemoveMembers = ""RemoveMembers"";
        public const string RemovePrivilege = ""RemovePrivilege"";
        public const string RemoveProductFromKit = ""RemoveProductFromKit"";
        public const string RemoveRelated = ""RemoveRelated"";
        public const string RemoveUserFromRecordTeam = ""RemoveUserFromRecordTeam"";
        public const string ReplacePrivileges = ""ReplacePrivileges"";
        public const string Reschedule = ""Reschedule"";
        public const string Retrieve = ""Retrieve"";
        public const string RetrieveExchangeRate = ""RetrieveExchangeRate"";
        public const string RetrieveFilteredForms = ""RetrieveFilteredForms"";
        public const string RetrieveMultiple = ""RetrieveMultiple"";
        public const string RetrievePersonalWall = ""RetrievePersonalWall"";
        public const string RetrievePrincipalAccess = ""RetrievePrincipalAccess"";
        public const string RetrieveRecordWall = ""RetrieveRecordWall"";
        public const string RetrieveSharedPrincipalsAndAccess = ""RetrieveSharedPrincipalsAndAccess"";
        public const string RetrieveUnpublished = ""RetrieveUnpublished"";
        public const string RetrieveUnpublishedMultiple = ""RetrieveUnpublishedMultiple"";
        public const string RetrieveUserQueues = ""RetrieveUserQueues"";
        public const string RevokeAccess = ""RevokeAccess"";
        public const string RouteTo = ""RouteTo"";
        public const string Send = ""Send"";
        public const string SendFromTemplate = ""SendFromTemplate"";
        public const string SetLocLabels = ""SetLocLabels"";
        public const string SetRelated = ""SetRelated"";
        public const string SetState = ""SetState"";
        public const string SetStateDynamicEntity = ""SetStateDynamicEntity"";
        public const string TriggerServiceEndpointCheck = ""TriggerServiceEndpointCheck"";
        public const string UnlockInvoicePricing = ""UnlockInvoicePricing"";
        public const string UnlockSalesOrderPricing = ""UnlockSalesOrderPricing"";
        public const string Update = ""Update"";
        public const string ValidateRecurrenceRule = ""ValidateRecurrenceRule"";
        public const string Win = ""Win"";
        #endregion
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
        None = 1,
        Sandbox = 2,
        External = 3
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

    #endregion
}";

        #endregion
    }
}

