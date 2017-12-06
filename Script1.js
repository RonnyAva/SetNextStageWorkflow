
function change() {
    try { contactId = Xrm.Page.getAttribute('im_commandrequest').getValue()[0].id; } catch (ex) { contactId = null; }
    if (contactId !== null) {
        var req = new XMLHttpRequest();
        var url = Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/im_commandrequestSet(guid'" + contactId + "')?$select=im_jsonrequest";
        req.open("GET", url, true);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (req.readyState == 4) {
                var data = JSON.parse(req.responseText);
                //alert(data.d.im_jsonrequest);
                Xrm.Page.getAttribute('msdyn_message').setValue(data.d.im_jsonrequest);
                // use data.d.EmailAddress1 
            }
        };
        req.send(null);
    }
}