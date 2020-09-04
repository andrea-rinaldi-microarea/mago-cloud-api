namespace magic_link_ng_dotnet.Models
{
    public class LoginRequest {
        public string accountName {get; set;}
        public string password {get; set;}
        public string subscriptionKey {get; set;}
        public bool overwrite {get; set;}
        public string appId {get; set;}
    }

    public class LoginResponse {
        public string jwtToken {get; set;}
        public string language {get; set;}
        public string message {get; set;}
        public string regionalSettings {get; set;}
        public string result {get; set;}
        public int resultCode {get; set;}
        public string accountName {get; set;}
        public string subscriptionKey {get; set;}
        public string subscriptionDesc {get; set;}
        public string subscriptionCountry {get; set;}
    }

    public class AuthorizationData {
        public string type {get; set;}
        public string securityValue {get; set;}
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
        public AuthorizationData authorizationData {get; set;}
        public ServerInfo serverInfo {get; set;}
        public string payload {get; set;}
        public string loginName {get; set;}
    }

}
