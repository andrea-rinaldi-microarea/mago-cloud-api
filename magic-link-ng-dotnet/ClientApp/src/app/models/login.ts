export class ConnectionRequest {
    public rootURL: string = "release-v112.mago.cloud";
    public isDebugEnv: boolean;

    public accountName: string;
    public password: string;
    public subscriptionKey: string;

}

export class LoginResponse {
    constructor(
    ) {}

    public jwtToken: string;
    public language: string;
    public message: string;
    public regionalSettings:string;
    public result: string;
    public resultCode: number;
    public accountName: string;
    public subscriptionKey: string;
    public subscriptionDesc: string;
    public subscriptionCountry: string;
}
