export class LoginResponse extends Object {
  constructor(
  ) {
    super();
  }

  public JwtToken: string = "";
  public Language: string = "";
  public Message: string = "";
  public RegionalSettings:string = "";
  public Result: string = "";
  public ResultCode: number = 0;
  public AccountName: string = "";
  public SubscriptionKey: string = "";
  public SubscriptionDesc: string = "";
  public SubscriptionCountry: string = "";
}
