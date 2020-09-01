using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

namespace MagicLinkWinForm
{
    public partial class MainForm : Form
    {
        private string AuthenticationToken = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            tbxRootURL.Text = Properties.Settings.Default.RootURL;
            tbxAccountName.Text = Properties.Settings.Default.AccountName;
            tbxPassword.Text = Properties.Settings.Default.Password;
            tbxSubscription.Text = Properties.Settings.Default.subscriptionkey;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (AuthenticationToken != string.Empty)
            {
                MessageBox.Show("already connected");
                return;
            }

            if (tbxAccountName.Text == string.Empty || tbxRootURL.Text == string.Empty || tbxSubscription.Text == string.Empty)
            {
                MessageBox.Show("missing connection data");
                return;
            }

            using (var msg = new HttpRequestMessage())
            {
                var loginData = JsonConvert.SerializeObject(new
                {
                    AccountName = tbxAccountName.Text,
                    Password = tbxPassword.Text,
                    AppId = "M4",
                    subscriptionkey = tbxSubscription.Text
                });

                msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "account-manager/login");
                msg.Method = HttpMethod.Post;
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                msg.Content = new StringContent(content: loginData, encoding: Encoding.UTF8, mediaType: "application/json");

                HttpClient httpClient = new HttpClient();
                using (var response = httpClient.SendAsync(msg).Result)
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(result))
                    {
                        tbxMessages.Text = "Invalid login";
                        return;
                    }

                    JObject jResult = JsonConvert.DeserializeObject<JObject>(result);

                    int resultCode = int.Parse(jResult["ResultCode"]?.ToString());
                    if (resultCode != 0)
                    {
                        tbxMessages.Text = jResult["Message"]?.ToString();
                        return;
                    }

                    string token = jResult["JwtToken"]?.ToString();
                    if (string.IsNullOrEmpty(token))
                    {
                        tbxMessages.Text = "Invalid login";
                        return;
                    }

                    AuthenticationToken = token.ToString();

                    tbxMessages.Text = "Connected succesfully!";
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.RootURL = tbxRootURL.Text;
            Properties.Settings.Default.AccountName = tbxAccountName.Text;
            Properties.Settings.Default.Password = tbxPassword.Text;
            Properties.Settings.Default.subscriptionkey = tbxSubscription.Text;

            Properties.Settings.Default.Save();
        }

        private string extractXMLMessages(JToken result)
        {
            if (result == null)
                return string.Empty;

            XDocument xmlMessages = XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(result.ToArray()[0].ToString())));
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("maxs", "http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/Standard/DefaultLight.xsd");

            string resultText = string.Empty;

            IEnumerable<XElement> elems = xmlMessages.XPathSelectElements("//maxs:Warning", mgr);
            if (elems.Count() > 0)
            {
                resultText += "\r\nWarnings:\r\n";
                foreach (XElement elem in elems)
                {
                    resultText += elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            elems = xmlMessages.XPathSelectElements("//maxs:Error", mgr);
            if (elems.Count() > 0)
            {
                resultText += "\r\nErrors:\r\n";
                foreach (XElement elem in elems)
                {
                    resultText += elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            return resultText;
        }

        private void btnGetContacts_Click(object sender, EventArgs e)
        {
            if (AuthenticationToken == string.Empty)
            {
                MessageBox.Show("not yet connected");
                return;
            }

            IEnumerable<Cookie> cookies = null;
            var authorizationData = JsonConvert.SerializeObject(new
            {
                Type = "JWT",
                SecurityValue = AuthenticationToken
            });

            Cursor.Current = Cursors.WaitCursor;
            using (var msg = new HttpRequestMessage())
            {

                var server_info = JsonConvert.SerializeObject(new
                {
                    subscription = tbxSubscription.Text,
                    gmtOffset = -60,
                    date = new
                    {
                        day = DateTime.Now.Day,
                        month = DateTime.Now.Month,
                        year = DateTime.Now.Year
                    }
                });
                msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "tbserver/api/tb/document/initTBLogin/");
                msg.Method = HttpMethod.Post;
                msg.Headers.TryAddWithoutValidation("Authorization", authorizationData);
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                msg.Headers.TryAddWithoutValidation("Server-Info", server_info);

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(httpClientHandler);
                using (var response = httpClient.SendAsync(msg).Result)
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(result))
                    {
                        tbxMessages.Text = "Invalid response";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                    JToken ok = jResult["success"];
                    bool bOk = ok == null ? false : ok.Value<bool>();

                    if (bOk)
                    {
                        cookies = httpClientHandler.CookieContainer.GetCookies(msg.RequestUri).Cast<Cookie>().ToList();
                    }
                    else
                    {
                        tbxMessages.Text = "Request failed";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                }
            }

            using (var msg = new HttpRequestMessage())
            {
                var xmlParam =
                    @"<?xml version='1.0' encoding='utf-8'?>
                    <maxs:Contacts tbNamespace='Document.ERP.Contacts.Documents.Contacts' xTechProfile='DefaultLight' xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/Standard/DefaultLight.xsd'>
                        <maxs:Parameters>
                        </maxs:Parameters>
                    </maxs:Contacts>";

                var functionParams = JsonConvert.SerializeObject(new
                {
                    ns = "Extensions.XEngine.TBXmlTransfer.GetDataRest",
                    args = new
                    {
                        param = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xmlParam)),
                        useApproximation = true,
                        loginName = tbxAccountName.Text,
                        result = "data"
                    }
                });

                msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "tbserver/api/tb/document/runRestFunction/");
                msg.Method = HttpMethod.Post;
                msg.Content = new StringContent(content: functionParams, encoding: Encoding.UTF8, mediaType: "application/json");
                msg.Headers.TryAddWithoutValidation("Authorization", authorizationData);
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.CookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    httpClientHandler.CookieContainer.Add(msg.RequestUri, cookie);
                }
                HttpClient httpClient = new HttpClient(httpClientHandler);
                using (var response = httpClient.SendAsync(msg).Result)
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(result))
                    {
                        tbxMessages.Text = "Invalid response";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                    JToken ok = jResult["retVal"];
                    bool bOk = ok == null ? false : ok.Value<bool>();
                    if (bOk)
                    {
                        JToken[] data = jResult["result"].ToArray();
                        tbxData.Text = string.Empty;
                        foreach (JToken item in data)
                        {
                            var bytes = Convert.FromBase64String(item.ToString());
                            var decodedString = Encoding.UTF8.GetString(bytes);
                            tbxData.Text += "\n" + decodedString.ToString();
                        }
                    }
                    else
                    {
                        tbxMessages.Text = "Request failed";
                        if (jResult["result"] != null)
                            tbxMessages.Text += ":\r\n" + extractXMLMessages(jResult["result"]);
                        if (jResult["message"] != null)
                            tbxMessages.Text += ":\r\n" + jResult["message"];

                        Cursor.Current = Cursors.Default;
                        return;
                    }
                }

            }
            Cursor.Current = Cursors.Default;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (AuthenticationToken == string.Empty)
            {
                MessageBox.Show("not yet connected");
                return;
            }
            try
            {
                using (var msg = new HttpRequestMessage())
                {
                    var authorizationData = JsonConvert.SerializeObject(new
                    {
                        Type = "JWT",
                        SecurityValue = AuthenticationToken
                    });

                    msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "account-manager/logoff/");
                    msg.Method = HttpMethod.Post;
                    msg.Headers.TryAddWithoutValidation("Authorization", authorizationData);
                    msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                    HttpClient httpClient = new HttpClient();
                    using (var response = httpClient.SendAsync(msg).Result)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            tbxMessages.Text = "Request failed: " + response.ReasonPhrase;
                            AuthenticationToken = "";
                            return;
                        }

                        string result = response.Content.ReadAsStringAsync().Result;

                        if (string.IsNullOrEmpty(result))
                        {
                            tbxMessages.Text = "Invalid response";
                            AuthenticationToken = "";
                            return;
                        }

                        JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                        JToken ok = jResult["Result"];
                        bool bOk = ok == null ? false : ok.Value<bool>();

                        if (bOk)
                        {
                            tbxMessages.Text = "User successfully logged out";
                        }
                        else
                        {
                            tbxMessages.Text = "Logout failed";
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                tbxMessages.Text = $"Error during logout {exception.Message}";
            }

            AuthenticationToken = "";
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if (AuthenticationToken == string.Empty)
            {
                MessageBox.Show("not yet connected");
                return;
            }

            IEnumerable<Cookie> cookies = null;
            var authorizationData = JsonConvert.SerializeObject(new
            {
                Type = "JWT",
                SecurityValue = AuthenticationToken
            });

            Cursor.Current = Cursors.WaitCursor;
            using (var msg = new HttpRequestMessage())
            {

                var server_info = JsonConvert.SerializeObject(new
                {
                    subscription = tbxSubscription.Text,
                    gmtOffset = -60,
                    date = new
                    {
                        day = DateTime.Now.Day,
                        month = DateTime.Now.Month,
                        year = DateTime.Now.Year
                    }
                });
                msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "tbserver/api/tb/document/initTBLogin/");
                msg.Method = HttpMethod.Post;
                msg.Headers.TryAddWithoutValidation("Authorization", authorizationData);
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                msg.Headers.TryAddWithoutValidation("Server-Info", server_info);

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(httpClientHandler);
                using (var response = httpClient.SendAsync(msg).Result)
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(result))
                    {
                        tbxMessages.Text = "Invalid response";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                    JToken ok = jResult["success"];
                    bool bOk = ok == null ? false : ok.Value<bool>();

                    if (bOk)
                    {
                        cookies = httpClientHandler.CookieContainer.GetCookies(msg.RequestUri).Cast<Cookie>().ToList();
                    }
                    else
                    {
                        tbxMessages.Text = "Request failed";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                }
            }

            using (var msg = new HttpRequestMessage())
            {
                string xmlData = $@"<?xml version='1.0'?>
                <maxs:Contacts xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/Standard/DefaultLight.xsd' tbNamespace = 'Document.ERP.Contacts.Documents.Contacts' xTechProfile = 'DefaultLight' >
                    <maxs:Data>
                        <maxs:Contacts master='true'>
                            <maxs:CompanyName>{txtCompanyName.Text}</maxs:CompanyName>
                            <maxs:Address>{txtAddress.Text}</maxs:Address>
                            <maxs:Telephone1>{txtPhone.Text}</maxs:Telephone1>
                            <maxs:ContactPerson>{txtContactName.Text}</maxs:ContactPerson>
                        </maxs:Contacts>
                    </maxs:Data>
                </maxs:Contacts> ";

                var functionParams = JsonConvert.SerializeObject(new
                {
                    ns = "Extensions.XEngine.TBXmlTransfer.SetDataRest",
                    args = new
                    {
                        data = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xmlData)),
                        saveAction = 0, // insert or update
                        loginName = tbxAccountName.Text,
                        result = "data"
                    }
                });

                msg.RequestUri = new Uri(new Uri(tbxRootURL.Text), "tbserver/api/tb/document/runRestFunction/");
                msg.Method = HttpMethod.Post;
                msg.Content = new StringContent(content: functionParams, encoding: Encoding.UTF8, mediaType: "application/json");
                msg.Headers.TryAddWithoutValidation("Authorization", authorizationData);
                msg.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.CookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    httpClientHandler.CookieContainer.Add(msg.RequestUri, cookie);
                }
                HttpClient httpClient = new HttpClient(httpClientHandler);
                using (var response = httpClient.SendAsync(msg).Result)
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(result))
                    {
                        tbxMessages.Text = "Invalid response";
                        Cursor.Current = Cursors.Default;
                        return;
                    }

                    JObject jResult = JsonConvert.DeserializeObject<JObject>(result);
                    JToken ok = jResult["retVal"];
                    bool bOk = ok == null ? false : ok.Value<bool>();
                    if (bOk)
                    {
                        if (jResult["result"] != null)
                        {
                            tbxResponse.Text = Encoding.UTF8.GetString(System.Convert.FromBase64String(jResult["result"].ToString()));
                        }
                        if (jResult["message"] != null)
                        {
                            string responseMsg = jResult["message"]?.ToString();
                            MessageBox.Show(responseMsg);
                        }
                    }
                    else
                    {
                        tbxMessages.Text = "Request failed";
                        if (jResult["result"] != null)
                            tbxMessages.Text += ":\r\n" + extractXMLMessages(jResult["result"]);
                        Cursor.Current = Cursors.Default;
                        return;
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }
    }
}
