var getContacts = {
    onGetContacts: function() {}
};

( getContacts => {

    var token = localStorage.getItem(TOKEN_TAG);
    var current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));

    if (!token) {
        window.location.href = 'home.html';
    }

    getContacts.onGetContacts = function() {
        const profileName = "pippo";
        const profileType = "Standard";
        $.post("/getContacts", {
            token: token,
            userName: current.accountName,
            subscriptionKey: current.subscriptionKey,
            params: `<?xml version="1.0" encoding="utf-8"?>
                <maxs:Contacts tbNamespace="Document.ERP.Contacts.Documents.Contacts" xTechProfile="${profileName}" xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/${profileType}/${profileName}.xsd">
                    <maxs:Parameters>
                    </maxs:Parameters>
                </maxs:Contacts>`
        })
        .done( response => {
            console.log(response);
        })
        .fail( (xhr, status, error) => {
            $("#errorMessage").text(xhr.responseJSON.message);
            $("#error").show();
            console.log(error);    
        });
    }

})(getContacts);

