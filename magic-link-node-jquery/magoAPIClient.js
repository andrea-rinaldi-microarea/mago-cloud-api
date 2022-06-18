const axios = require('axios').default;

class MagoAPIClient {
    constructor(gwamUrl, producerKey, appKey) {
        this.gwamUrl = gwamUrl;
        this.producerKey = producerKey;
        this.appKey = appKey;
    }

    login(accountName, password, subscriptionKey) {
        return this.#Post(
            `https://${this.gwamUrl}/gwam_login/api/login`,
            {
                accountName: `${accountName}`,
                appId: "MagoAPI",
                overwrite: false,
                password: `${password}`,
                subscriptionKey: `${subscriptionKey}`
            },
            Object.assign({}, 
                this.#ProducerInfoHeader()
            )
        )
    }

    logoff(token) {
        return this.#Post(
            `https://${this.gwamUrl}/gwam_login/api/logoff`, 
            {
                token: token
            },
            Object.assign({}, 
                this.#ProducerInfoHeader(),
                this.#AuthorizationHeader(token)
            )
        );
    }

    #ProducerInfoHeader() {
        return {
            'Producer-Info': JSON.stringify({
                ProducerKey: this.producerKey,  
                AppKey: this.appKey
            })
        }
    }

    #AuthorizationHeader(token) {
        return {
            'Authorization': JSON.stringify({
                type: 'jwt', 
                appId: 'MagoAPI', 
                securityValue: token
            })
        }
    }

    #Post(url, data, headers) {
        return new Promise((resolve, reject) => {
            axios.post(url, data, { headers: headers })
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