export class ConnectionInfo {
    constructor(
    ) {}

    public accountName: string;
    public password: string;
    public subscriptionKey: string;
    public jwtToken: string = null;
    public rootURL: string = "public.mago.cloud";
    public ui_culture: string;
    public culture: string;
    public isDebugEnv: boolean;
}

export class LoginRequest {
    public accountName: string;
    public password: string;
    public subscriptionKey: string;
    public appId: string;
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

export class AuthorizationData {
    public Type: string = "JWT";
    public SecurityValue: string;
}

