using Microarea.Tbf.Model.API;

namespace openecommerce_ng_dotnet.Models
{
    public class LoginRequest {
        public string? url {get; set;}
        public string? accountName {get; set;}
        public string? password {get; set;}
        public string? subscriptionKey {get; set;}
        public bool overwrite {get; set;}
        public string? appId {get; set;}
    }

    public class SetDataResponse {
        public string? xmlData {get; set;}
        public string? warnings {get; set;}
    }

    public interface IMagoConnection {
        public MagoAPIClient? APIClient {get; set; }
        public TbUserData? TbUserData {get; set; }
    }

    public class MagoConnection : IMagoConnection {
        public MagoAPIClient? _APIclient = null;
        public TbUserData? _TbUserData = null;
        public MagoAPIClient? APIClient {get { return _APIclient; }  set { _APIclient = value; } }
        public TbUserData? TbUserData {get { return _TbUserData; } set{ _TbUserData = value; } }
    }

    public class TbUserDataWrapper : TbUserData
    {
        public TbUserDataWrapper() : base("","","","") {}
    }

}
