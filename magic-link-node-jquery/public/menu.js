var menu = {
};

( menu => {

    var token = localStorage.getItem(TOKEN_TAG);

    if (!token) {
        window.location.href = 'home.html'
    }

})(menu);

