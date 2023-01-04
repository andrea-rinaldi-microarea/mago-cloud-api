export class ConnectionInfo {
    constructor(
    ) {}

    public accountName: string;
    public password: string;
    public subscriptionKey: string;
    public jwtToken: string = null;
    public rootURL: string = "release-v112.mago.cloud";
    public isDebugEnv: boolean;
}
