using Microarea.Tbf.Model.API;

namespace magic_link_ng_dotnet.Models
{
    public class LoginRequest {
        public string url {get; set;}
        public string accountName {get; set;}
        public string password {get; set;}
        public string subscriptionKey {get; set;}
        public bool overwrite {get; set;}
        public string appId {get; set;}
    }

    public class DateInfo {
        public int day {get; set;}
        public int month {get; set;}
        public int year {get; set;}
    }

    public class ServerInfo {
        public string subscription {get; set;}
        public int gmtOffset {get; set;}
        public string ui_culture {get; set;}
        public string culture {get; set;}
        public DateInfo date {get; set;}
    }

    public class GetDataRequest {
        public ServerInfo serverInfo {get; set;}
        public string payload {get; set;}
        public string loginName {get; set;}
    }

    public class MagoAPIClientWrapper {
        public MagoAPIClient client = null;
    }

    public class TbUserDataWrapper : TbUserData
    {
        public TbUserDataWrapper() : base("","","","") {}
    }

}
