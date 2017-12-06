using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
namespace SetNextStageWorkflows
{
    public class send_EMAIL : CodeActivity
    {

        [Input("Booking Resource")]
        [ReferenceTarget("bookableresourcebooking")]
        public InArgument<EntityReference> BookResource { get; set; }

        [Input("Portal Comment")]
        [ReferenceTarget("adx_portalcomment")]
        public InArgument<EntityReference> PortalComment { get; set; }

        [Input("contact")]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> ContactRef { get; set; }

        [Input("systemuser")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> Systemuser { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
    {
        //Create the context
        IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
        IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId); 
        //Entity bookresource = (Entity)service.Retrieve("bookableresourcebooking", context.PrimaryEntityId, new ColumnSet(true));
        //string campaignName = (string)regCampaign.Attributes["name"];
        //EntityReference ownerid = (EntityReference)regCampaign.Attributes["ownerid"];

        Entity emailEntity = new Entity("email");

        Entity fromParty = new Entity("activityparty");
            fromParty.Attributes["partyid"] = Systemuser.Get<EntityReference>(executionContext);
        EntityCollection from = new EntityCollection();
        from.Entities.Add(fromParty);

        Entity toParty = new Entity("activityparty");
        toParty.Attributes["partyid"] = ContactRef.Get<EntityReference>(executionContext);
        EntityCollection to = new EntityCollection();
        to.Entities.Add(toParty);

        EntityReference regarding = new EntityReference("bookableresourcebooking", context.PrimaryEntityId);

        emailEntity["to"] = to;
        emailEntity["from"] = from;
        emailEntity["subject"] = "Technical Report from your WorkOrder";
        emailEntity["regardingobjectid"] = regarding;
        Guid EmailID = service.Create(emailEntity);

        //EntityReference abc = SourceCompaign.Get<EntityReference>(executionContext);
        AddAttachmentToEmailRecord(service, EmailID, BookResource.Get<EntityReference>(executionContext), PortalComment.Get<EntityReference>(executionContext));
    }


        private void CreateNoteAttachment(Entity NotesAttachment, EntityReference portalcomment, IOrganizationService service)
        {


            Guid attachmentId = Guid.Empty;


            Entity note = new Entity("annotation");

            note["subject"] = NotesAttachment.GetAttributeValue<string>("subject");

            note["filename"] = NotesAttachment.GetAttributeValue<string>("filename");

            note["documentbody"] = NotesAttachment.GetAttributeValue<string>("documentbody");

            note["mimetype"] = NotesAttachment.GetAttributeValue<string>("mimetype");

            note["objectid"] = portalcomment;

            service.Create(note);


        }


        private void AddAttachmentToEmailRecord(IOrganizationService service, Guid SourceEmailID, EntityReference CompaignID, EntityReference Comment)
    {

        //create email object
        Entity emailCreated = service.Retrieve("email", SourceEmailID, new ColumnSet(true));
        QueryExpression QueryNotes = new QueryExpression("annotation");
        QueryNotes.ColumnSet = new ColumnSet(new string[] { "subject", "mimetype", "filename", "documentbody" });
        QueryNotes.Criteria = new FilterExpression();
        QueryNotes.Criteria.FilterOperator = LogicalOperator.And;
        QueryNotes.Criteria.AddCondition(new ConditionExpression("objectid", ConditionOperator.Equal, CompaignID.Id));
        EntityCollection MimeCollection = service.RetrieveMultiple(QueryNotes);
        if (MimeCollection.Entities.Count > 0)
{ //we need to fetch first attachment
            Entity NotesAttachment = MimeCollection.Entities.First();
            //Create email attachment
            Entity EmailAttachment = new Entity("activitymimeattachment");
            if (NotesAttachment.Contains("subject"))
                EmailAttachment["subject"] = NotesAttachment.GetAttributeValue<string>("subject");
            EmailAttachment["objectid"] = new EntityReference("email", emailCreated.Id);
            EmailAttachment["objecttypecode"] = "email";
            if (NotesAttachment.Contains("filename"))
                EmailAttachment["filename"] = NotesAttachment.GetAttributeValue<string>("filename");
            if (NotesAttachment.Contains("documentbody"))
                EmailAttachment["body"] = NotesAttachment.GetAttributeValue<string>("documentbody");
            if (NotesAttachment.Contains("mimetype"))
                EmailAttachment["mimetype"] = NotesAttachment.GetAttributeValue<string>("mimetype");
            service.Create(EmailAttachment);
             CreateNoteAttachment(NotesAttachment, Comment, service);
        }
        // Sending email
        SendEmailRequest SendEmail = new SendEmailRequest();
        SendEmail.EmailId = emailCreated.Id;
        SendEmail.TrackingToken = "";
        SendEmail.IssueSend = true;
        SendEmailResponse res = (SendEmailResponse)service.Execute(SendEmail);
    }

}
}
