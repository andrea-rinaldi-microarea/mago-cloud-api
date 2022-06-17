const axios = require('axios').default;

class MagoAPIClient {
    constructor(gwamUrl, producerKey, appKey) {
        this.gwamUrl = gwamUrl;
        this.producerKey = producerKey;
        this.appKey = appKey;
    }

    login(accountName, password, subscriptionKey) {
        var url = `https://${this.gwamUrl}/gwam_login/api/login`;
        var loginData = {
            accountName: `${accountName}`,
            appId: "MagoAPI",
            overwrite: false,
            password: `${password}`,
            subscriptionKey: `${subscriptionKey}`
        }
        var headers = Object.assign({}, 
            this.#ProducerInfoHeader()
        );
        return new Promise((resolve, reject) => {
            axios.post(url, loginData, { headers: headers })
                .then(response => {
                    resolve(response.data);
                })
                .catch(error => {
                    if (error.response) {
                        reject({
                            status: error.response.status,
                            statusText: error.response.statusText,
                            message: error.response.data.Message ? error.response.data.Message : error.response.data
                        });
                    } else {
                        reject({
                            status: 400,
                            statusText: error.code,
                            message: error.message
                        });
                    }
                });
        });
    }

    #ProducerInfoHeader() {
        return {
            'Producer-Info': {
                'ProducerKey':`${this.producerKey}`,  
                'AppKey': `${this.appKey}`
            }
        }
    }

}

module.exports = function magoAPIClient(gwamUrl, producerKey, appKey) {
    return new MagoAPIClient(gwamUrl, producerKey, appKey);
}

//==============================================================================

// //------------------------------------------------------------------------------
// function retrieveServiceURL(gwamUrl, subscription, service) {
//     // var url = `https://${gwamUrlOrInstanceName}/gwam_mapper/api/services/url/${subscription}/ACCOUNTMANAGER`;
//     var url = `https://${gwamUrl}/gwam_mapper/api/snapshotcontainer?subscriptionKey=${subscription}`;
//     return new Promise((resolve, reject) => {
//         axios.get(url)
//             .then(response => {
//                 resolve(response);
//             })
//             .catch(error => {
//                 reject({
//                     status: error.response.status,
//                     statusText: error.response.statusText,
//                     message: error.response.data.Message ? error.response.data.Message : error.response.data
//                 });
                
//             });
//     });
// }


/*
class AccountManager {

    constructor(magoAPI) {
        this.magoAPI = magoAPI;
    }

    async login(accountName, password, subscriptionKey) {
        try {
            const accountManagerURL = await retrieveServiceURL(this.magoAPI.gwamUrlOrInstanceName, subscriptionKey, "ACCOUNTMANAGER");
            return accountManagerURL;
        } catch (error) {
            throw error;            
        }
    }
}
*/