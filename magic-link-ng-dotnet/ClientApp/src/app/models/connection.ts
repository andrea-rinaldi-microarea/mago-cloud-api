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
}

export class LoginRequest {
    public url: string;
    public accountName: string;
    public password: string;
    public subscriptionKey: string;
}

export class TbUserData {
    constructor(
    ) {}

    public token: string;
    public userName: string;
    public password: string;
    public subscriptionKey: string;
    public isLogged: boolean;
}

