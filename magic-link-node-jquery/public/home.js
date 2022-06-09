var home = {
    login: function() {}
};

( home => {

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

    home.login = function() {

        $.post("/login", {
                rootURL: document.login["rootURL"].value,
                accountName: document.login["accountName"].value,
                password:  document.login["password"].value,
                subscriptionKey: document.login["subscriptionKey"].value
            })
            .done( result => {
                console.log(result);
            })
            .fail( (xhr, status, error) => {
                $("#errorMessage").text(xhr.responseJSON.message);
                $("#error").show();
                console.log(error);    
            });

        // save info in local browser storage to propose next time
        var current = {
            rootURL: document.login["rootURL"].value,
            accountName: document.login["accountName"].value,
            password: document.login["password"].value,
            subscriptionKey: document.login["subscriptionKey"].value
        }
        localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(current));
    }

})(home);
