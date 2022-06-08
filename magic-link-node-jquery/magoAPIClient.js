const axios = require('axios').default;


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

class MagoAPIClient {
    constructor(gwamUrlOrInstanceName, producerID, appID) {
        this.gwamUrlOrInstanceName = gwamUrlOrInstanceName;
        this.producerID = producerID;
        this.appID = appID;
        this.accountManager = new AccountManager(this);
    }

}

module.exports = function magoAPIClient(gwamUrlOrInstanceName, producerID, appID) {
    return new MagoAPIClient(gwamUrlOrInstanceName, producerID, appID);
}

//==============================================================================

//------------------------------------------------------------------------------
async function retrieveServiceURL(gwamUrlOrInstanceName, subscription, service) {
    // var url = `https://${this.gwamUrlOrInstanceName}/gwam_mapper/api/services/url/${subscription}/ACCOUNTMANAGER`;
    var url = `https://${gwamUrlOrInstanceName}/gwam_mapper/api/snapshotcontainer/I-RELEASE?subscriptionKey=${subscription}`;
    return new Promise((resolve, reject) => {
        axios.get(url)
            .then(response => {
                resolve(response);
            })
            .catch(error => {
                reject({
                    status: error.response.status,
                    statusText: error.response.statusText,
                    message: error.response.data.Message
                });
                
            });
    });
}
