var express = require('express');
var path = require('path');
var opn = require('opn');
const bodyParser = require('body-parser');
var MagoAPI = require('./magoAPIClient');

var app = express();
app.use(bodyParser.urlencoded({ extended: true }));

app.use(express.static(path.join(__dirname,'public')));

app.get('/', function (req, res) {
    res.sendFile(path.join(__dirname, 'public', 'home.html'));
 });

var magoAPI = null;

app.post('/login', function (req, res) {
    var data = req.body;
    magoAPI = MagoAPI(data.gwamUrl, "MyProdKey", "MyAppKey");
    magoAPI.login(data.accountName, data.password, data.subscriptionKey)
        .then( response => {
            res.send(response);
        })
        .catch(error => {
            res.status(error.status).send(error);
        });
});


const cmdArgs = process.argv.slice(2);
var port = 5001;
if (cmdArgs.length == 1) {
    port = cmdArgs[0];
}


 var server = app.listen(port, "localhost", function () {
    var host = server.address().address
    var port = server.address().port
    
    console.log(`MagicLink sample listening at http://${host}:${port}`);
    opn(`http://${host}:${port}`);
 })
 