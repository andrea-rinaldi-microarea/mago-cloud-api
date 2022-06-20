const axios = require('axios').default;

class MagoAPIClient {
    constructor(gwamUrl, producerKey, appKey) {
        this.gwamUrl = gwamUrl;
        this.producerKey = producerKey;
        this.appKey = appKey;
        this.tbServer = new TBServer(this);
    }

    //------------------------------------------------------------------------------
    login(accountName, password, subscriptionKey) {
        return Post(
            `https://${this.gwamUrl}/gwam_login/api/login`,
            {
                accountName: `${accountName}`,
                appId: "MagoAPI",
                overwrite: false,
                password: `${password}`,
                subscriptionKey: `${subscriptionKey}`
            },
            Object.assign({}, 
                this.ProducerInfoHeader()
            )
        )
    }

    //------------------------------------------------------------------------------
    logoff(token) {
        return Post(
            `https://${this.gwamUrl}/gwam_login/api/logoff`, 
            {
                token: token
            },
            Object.assign({}, 
                this.ProducerInfoHeader(),
                this.AuthorizationHeader(token)
            )
        );
    }

    //------------------------------------------------------------------------------
    retrieveServiceURL(token, subscription, service) {
        return Get(
            `https://${this.gwamUrl}/gwam_mapper/api/services/url/${subscription}/${service}`,
            Object.assign({}, 
                this.ProducerInfoHeader(),
                this.AuthorizationHeader(token)
            )
        );
    }

    //------------------------------------------------------------------------------
    ProducerInfoHeader() {
        return {
            'Producer-Info': JSON.stringify({
                ProducerKey: this.producerKey,  
                AppKey: this.appKey
            })
        }
    }

    //------------------------------------------------------------------------------
    AuthorizationHeader(token) {
        return {
            'Authorization': JSON.stringify({
                type: 'jwt', 
                appId: 'MagoAPI', 
                securityValue: token
            })
        }
    }

    //------------------------------------------------------------------------------
    SnapshotHeader(subscriptionKey) {
        return {
            'Snapshot': JSON.stringify({
                SubscriptionKey: subscriptionKey, 
                Token: ""
            })
        }
    }

    //------------------------------------------------------------------------------
    ServerInfoHeader(subscriptionKey, date) {
        return {
            'Server-Info': JSON.stringify({
                subscription: subscriptionKey, 
                gmtOffset: -60,
                date: {
                    day: date.getDate(),
                    month: date.getMonth(),
                    year: date.getFullYear()
                }
            })
        }
    }
}

class TBServer {

    constructor(magoAPI) {
        this.magoAPI = magoAPI;
    }

    async GetXmlData(token, userName, subscription, params) {
        var tbServerURL = "";
        try {
            const serviceData = await this.magoAPI.retrieveServiceURL(token, subscription, "TBSERVER");
            tbServerURL = serviceData.Content;
        } catch (error) {
            throw error;            
        }

        return new Promise((resolve, reject) => {
            Post(
                `${tbServerURL}tbserver/api/tb/document/runRestFunction/`,
                {
                    ns: "Extensions.XEngine.TBXmlTransfer.GetDataRest",
                    args: {
                        param: Buffer.from(params).toString('base64'),
                        useApproximation: true,
                        loginName: userName,
                        result: "data"
                    }
                },
                Object.assign({}, 
                    this.magoAPI.ProducerInfoHeader(),
                    this.magoAPI.AuthorizationHeader(token),
                    this.magoAPI.SnapshotHeader(subscription),
                    this.magoAPI.ServerInfoHeader(subscription, new Date())
                ))
                .then( response => {
                    if (response.Result == false) {
                        reject(response);
                    } else {
                        if (Array.isArray(response.result)) {
                            var resultB64 = response.result;
                            response.result = [];
                            resultB64.forEach(record => {
                                response.result.push(Buffer.from(record, 'base64').toString('ascii'));
                            });
                        }
                        resolve(response);
                    }
                })
                .catch(error => {
                    reject(error);
                })
        });
    }
}

module.exports = function magoAPIClient(gwamUrl, producerKey, appKey) {
    return new MagoAPIClient(gwamUrl, producerKey, appKey);
}

//==============================================================================

//------------------------------------------------------------------------------
function Post(url, payload, headers) {
    return new Promise((resolve, reject) => {
        axios.post(url, payload, { headers: headers })
            .then(response => {
                if (response.data.success == false) {
                    resolve({
                        Result: false,
                        Message: response.data.message.text
                    })
                } else {
                    resolve(response.data);
                }
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

//------------------------------------------------------------------------------
function Get(url, headers) {
    return new Promise((resolve, reject) => {
        axios.get(url, { headers: headers })
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


