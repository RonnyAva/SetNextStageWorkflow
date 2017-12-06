
using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace SetNextStageWorkflows
{
    public sealed class GetRecordId : CodeActivity
    {
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> Case { get; set; }

        [Output("Account Id")]
        public OutArgument<string> CaseId { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            EntityReference accountRef = Case.Get<EntityReference>(executionContext);
            CaseId.Set(executionContext, accountRef.Id.ToString());
        }
    }

}