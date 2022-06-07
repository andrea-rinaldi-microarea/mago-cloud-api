var magicLink = {
    login: function() {}
};

( magicLink => {

    const CONNECTION_INFO_TAG = "connectionInfo";

    var current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));
    if (current) {
        document.login["rootURL"].value = current.rootURL;
        document.login["isDebugEnv"].checked = current.isDebugEnv;
        document.login["accountName"].value = current.accountName;
        document.login["password"].value = current.password;
        document.login["subscriptionKey"].value = current.subscriptionKey;
    } else {
        document.login["rootURL"].value = "localhost:5000";
    }

    function composeURL(path) {
        if (document.login["isDebugEnv"].checked) {
            return `http://${document.login["rootURL"].value}/${path}`;
        } else {
            return `https://${document.login["rootURL"].value}/be/${path}`;
        }
    }

    function retrieveAccountManagerURL() {
        if (document.login["isDebugEnv"].checked) {
            console.log("Debug environment not yet supported");
        } else {
            // var url = `https://${document.login["rootURL"].value}/gwam_mapper/api/services/url/${document.login["subscriptionKey"].value}/ACCOUNTMANAGER`;
            var url = `https://${document.login["rootURL"].value}/gwam_mapper/api/snapshotcontainer/I-RELEASE?subscriptionKey=${document.login["subscriptionKey"].value}`;
            $.ajax({
                url: url,
                type: 'GET',
                dataType: 'json',
                headers: {
                    'Content-Type' : 'application/json'
                },
                success: function(data, textStatus, xhr) {
                    console.log("Successful call, textStatus: '" + textStatus + "'");
                    console.log(data);
                },
                error: function(xhr, textStatus, errorThrown){
                    console.log("Error on call, textStatus: '" + textStatus + "'\n errorThrown: '" + errorThrown + "'");
                }
            });        
        }
    }

    magicLink.login = function() {
        retrieveAccountManagerURL();
        // var loginRequest = {
        //     accountName: document.login["accountName"].value,
        //     password:  document.login["password"].value,
        //     overwrite: false,
        //     subscriptionKey: document.login["subscriptionKey"].value,
        //     appId: "M4"
        //   }; 

        // var url = composeURL("account-manager/login");
        // $.ajax({
        //     url: composeURL("account-manager/login"),
        //     type: 'POST',
        //     dataType: 'json',
        //     data: JSON.stringify(loginRequest),
        //     headers: {
        //         'Content-Type' : 'application/json'
        //     },
        //     success: function(data, textStatus, xhr) {
        //         console.log("Successful call, textStatus: '" + textStatus + "'");
        //         console.log(data);
        //     },
        //     error: function(xhr, textStatus, errorThrown){
        //         console.log("Error on call, textStatus: '" + textStatus + "'\n errorThrown: '" + errorThrown + "'");
        //     }
        // });

        var current = {
            rootURL: document.login["rootURL"].value,
            isDebugEnv: document.login["isDebugEnv"].checked,
            accountName: document.login["accountName"].value,
            password: document.login["password"].value,
            subscriptionKey: document.login["subscriptionKey"].value
        }
        localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(current));
    }

})(magicLink);
