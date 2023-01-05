var postContact = {
    onPostContact: function() {}
};

( postContact => {

    var token = localStorage.getItem(TOKEN_TAG);
    var current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));

    if (!token) {
        window.location.href = 'home.html';
    }

    postContact.onPostContact = function() {
        const profileName = "DefaultLight";
        const profileType = "Standard";
                
        $.post("/postContact", {
            token: token,
            userName: current.accountName,
            subscriptionKey: current.subscriptionKey,
            params: `<?xml version="1.0" encoding="utf-8"?>
            <maxs:Contacts tbNamespace="Document.ERP.Contacts.Documents.Contacts" xTechProfile="${profileName}" xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/${profileType}/${profileName}.xsd">
              <maxs:Data>
                <maxs:Contacts master='true'>
                  <maxs:CompanyName>${document.postContact["company"].value}</maxs:CompanyName>
                  <maxs:Address>${document.postContact["address"].value}</maxs:Address>
                  <maxs:Telephone1>${document.postContact["phone"].value}</maxs:Telephone1>
                  <maxs:ContactPerson>${document.postContact["contactName"].value}</maxs:ContactPerson>
                </maxs:Contacts>
              </maxs:Data>
            </maxs:Contacts>`
        })
        .done( response => {
            console.log(response);
            var xmls = "";
            response.result.forEach(record => {
                xmls += record + "\n";
            });
            $("#result").text(xmls);
        })
        .fail( (xhr, status, error) => {
            $("#errorMessage").text(xhr.responseJSON.message);
            $("#error").show();
            console.log(error);    
        });
    }

})(postContact);

