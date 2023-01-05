export class ConnectionInfo {
  constructor(
  ) {}

  public accountName: string = "";
  public password: string = "";
  public subscriptionKey: string = "";
  public jwtToken: string | null = null;
  public rootURL: string = "release.mago.cloud/13";
  public isDebugEnv: boolean = false;
}
