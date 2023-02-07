
export class ConnectionInfo {
  constructor(
  ) {}

  public accountName: string = "";
  public password: string = "";
  public subscriptionKey: string = "";
  public jwtToken: string | null = null;
  public rootURL: string = "public.mago.cloud";
  public ui_culture: string = "";
  public culture: string = "";
}

export class LoginRequest {
  public url: string = "";
  public accountName: string = "";
  public password: string = "";
  public subscriptionKey: string = "";
}

export interface TbUserData {
  token: string | null;
  userName: string;
  password: string;
  subscriptionKey: string;
  isLogged: boolean;
}

