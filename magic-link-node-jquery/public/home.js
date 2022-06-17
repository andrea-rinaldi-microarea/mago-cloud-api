var home = {
    login: function() {}
};

( home => {

    function init() {
        var token = localStorage.getItem(TOKEN_TAG);
        var current = JSON.parse(localStorage.getItem(CONNECTION_INFO_TAG));
    
        if (token) {
            $('#loginPanel').hide();
            $('#logoutPanel').show();
            if (current) {
                $("#loginInfo").text(`Logged in as ${current.accountName} on ${current.subscriptionKey}`);
            }
        } else {
            $('#loginPanel').show();
            $('#logoutPanel').hide();
            if (current) {
                document.login["gwamUrl"].value = current.gwamUrl;
                document.login["accountName"].value = current.accountName;
                document.login["password"].value = current.password;
                document.login["subscriptionKey"].value = current.subscriptionKey;
            } else {
                document.login["gwamUrl"].value = "gwam.mago.cloud";
            }
        }
    }
    init();

    home.login = function() {

        $.post("/login", {
                gwamUrl: document.login["gwamUrl"].value,
                accountName: document.login["accountName"].value,
                password:  document.login["password"].value,
                subscriptionKey: document.login["subscriptionKey"].value
            })
            .done( response => {
                console.log(response);
                localStorage.setItem(TOKEN_TAG, response.jwtToken);
                window.location.href = 'menu.html'
            })
            .fail( (xhr, status, error) => {
                $("#errorMessage").text(xhr.responseJSON.message);
                $("#error").show();
                console.log(error);    
            });

        // save info in local browser storage to propose next time
        var current = {
            gwamUrl: document.login["gwamUrl"].value,
            accountName: document.login["accountName"].value,
            password: document.login["password"].value,
            subscriptionKey: document.login["subscriptionKey"].value
        }
        localStorage.setItem(CONNECTION_INFO_TAG, JSON.stringify(current));
    }

    home.logout = function() {
        localStorage.removeItem(TOKEN_TAG);
        init();
    }

})(home);
