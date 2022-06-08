var client = {
    login: function() {}
};

( client => {

    const CONNECTION_INFO_TAG = "connectionInfo";

    var current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));
    if (current) {
        document.login["rootURL"].value = current.rootURL;
        document.login["accountName"].value = current.accountName;
        document.login["password"].value = current.password;
        document.login["subscriptionKey"].value = current.subscriptionKey;
    } else {
        document.login["rootURL"].value = "localhost:5000";
    }

    client.login = function() {

        $.post("/login", {
                rootURL: document.login["rootURL"].value,
                accountName: document.login["accountName"].value,
                password:  document.login["password"].value,
                subscriptionKey: document.login["subscriptionKey"].value
            }, 
            function(error, result) {
                if (error) {
                    console.log(error);    
                } else {
                    console.log(result);
                }
            }
        );

        // save info in local browser storage to propose next time
        var current = {
            rootURL: document.login["rootURL"].value,
            accountName: document.login["accountName"].value,
            password: document.login["password"].value,
            subscriptionKey: document.login["subscriptionKey"].value
        }
        localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(current));
    }

})(client);
