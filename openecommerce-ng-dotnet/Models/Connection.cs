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

    public class GetDataRequest {
        public string? xmlParams {get; set;}
        public TbUserDataWrapper? userData {get; set;}
    }

    public class SetDataRequest {
        public string? xmlData {get; set;}
        public TbUserDataWrapper? userData {get; set;}
    }

    public class SetDataResponse {
        public string? xmlData {get; set;}
        public string? warnings {get; set;}
    }

    public interface IMagoAPIClientWrapper {
        public MagoAPIClient? Client {get; set; }
    }

    public class MagoAPIClientWrapper : IMagoAPIClientWrapper {
        public MagoAPIClient? _client = null;
        public MagoAPIClient? Client {get { return _client; }  set { _client = value; } }
    }

    public class TbUserDataWrapper : TbUserData
    {
        public TbUserDataWrapper() : base("","","","") {}
    }

}
