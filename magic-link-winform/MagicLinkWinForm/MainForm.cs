using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

using Microarea.Tbf.Model.API;
using Microarea.Tbf.Model.Interfaces.API;

namespace MagicLinkWinForm
{
    public partial class MainForm : Form
    {
        private ITbUserData userData = null;
        MagoAPIClient magocloudClient = null;

        public MainForm()
        {
            InitializeComponent();
            tbxRootURL.Text = Properties.Settings.Default.RootURL;
            tbxAccountName.Text = Properties.Settings.Default.AccountName;
            tbxPassword.Text = Properties.Settings.Default.Password;
            tbxSubscription.Text = Properties.Settings.Default.subscriptionkey;
            tbxProfileName.Text = Properties.Settings.Default.ProfileName;
            cbxProfileType.Text = Properties.Settings.Default.ProfileType;
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (userData != null)
            {
                MessageBox.Show("already connected");
                return;
            }

            if (tbxAccountName.Text == string.Empty || tbxRootURL.Text == string.Empty || tbxSubscription.Text == string.Empty)
            {
                MessageBox.Show("missing connection data");
                return;
            }
            try
            {
                magocloudClient = new MagoAPIClient(tbxRootURL.Text, new ProducerInfo("MyProdKey", "MyAppId"));
                
                IAccountManagerResult result = await magocloudClient.AccountManager?.Login(tbxAccountName.Text, tbxPassword.Text, tbxSubscription.Text);

                if (!result.Success || result.UserData == null || !result.UserData.IsLogged)
                {
                    tbxMessages.Text = "Invalid login\r\n" + result.ReturnValue;
                    return;
                }

                userData = result.UserData;
                tbxMessages.Text = "Connected succesfully!";
            }
            catch (Exception ex)
            {
                tbxMessages.Text = ex.Message;
            }
        }

        private async Task doLogout()
        {
            try
            {
                IAccountManagerResult isValid = await magocloudClient.AccountManager.IsValid(userData);
                if (isValid.Success)
                {
                    IAccountManagerResult result = await magocloudClient.AccountManager.Logout(userData);
                    if (result.Success)
                    {
                        tbxMessages.Text = "User successfully logged out";
                    }
                    else
                    {
                        tbxMessages.Text = "Logout failed";
                    }
                }
                else
                {
                    tbxMessages.Text = "Login no more valid, logout failed";
                }
            }
            catch (Exception exception)
            {
                tbxMessages.Text = $"Error during logout {exception.Message}";
            }

            userData = null;
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            if (userData == null)
            {
                MessageBox.Show("not yet connected");
                return;
            }
            
            await doLogout();
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.RootURL = tbxRootURL.Text;
            Properties.Settings.Default.AccountName = tbxAccountName.Text;
            Properties.Settings.Default.Password = tbxPassword.Text;
            Properties.Settings.Default.subscriptionkey = tbxSubscription.Text;
            Properties.Settings.Default.ProfileName = tbxProfileName.Text;
            Properties.Settings.Default.ProfileType = cbxProfileType.Text;

            Properties.Settings.Default.Save();

            await doLogout();
        }

        private string extractXMLMessages(string xmlResult, out bool hasErrors)
        {
            hasErrors = false;
            if (xmlResult == string.Empty)
                return string.Empty;

            XDocument xmlMessages = XDocument.Parse(xmlResult);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("maxs", $"http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/{cbxProfileType.Text}/{tbxProfileName.Text}.xsd");

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
                hasErrors = true;
                resultText += "\r\nErrors:\r\n";
                foreach (XElement elem in elems)
                {
                    resultText += elem.XPathSelectElement("maxs:Message", mgr).Value;
                }
            }
            return resultText;
        }

        private async void btnGetContacts_Click(object sender, EventArgs e)
        {
            if (userData == null)
            {
                MessageBox.Show("not yet connected");
                return;
            }
            Cursor.Current = Cursors.WaitCursor;

            var xmlParam =
                $@"<?xml version='1.0' encoding='utf-8'?>
                    <maxs:Contacts tbNamespace='Document.ERP.Contacts.Documents.Contacts' xTechProfile='{tbxProfileName.Text}' xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/{cbxProfileType.Text}/{tbxProfileName.Text}.xsd'>
                        <maxs:Parameters>
                        </maxs:Parameters>
                    </maxs:Contacts>";
                //<maxs:DefaultDialog title='Fields search'>
                //  <maxs:DefaultGroup title='Search group'>
                //    <maxs:Contact type='String'>9999</maxs:Contact>
                //  </maxs:DefaultGroup>
                //</maxs:DefaultDialog>

            ITbServerMagicLinkResult tbResult = await magocloudClient.TbServer?.GetXmlData(userData, xmlParam, DateTime.Now);

            if (!tbResult.Success)
            {
                tbxMessages.Text = "Error in GetData\r\n" + tbResult.ReturnValue;
                Cursor.Current = Cursors.Default;

                return;
            }

            if (tbResult.Xmls.Count == 0)
            {
                tbxMessages.Text = "No data extracted";
                Cursor.Current = Cursors.Default;

                return;
            }

            // just 1 result can be an error or warning message
            if (tbResult.Xmls.Count == 1)
            {
                var strMessages = extractXMLMessages(tbResult.Xmls[0], out bool hasErrors);
                if (hasErrors)
                {
                    tbxMessages.Text = "Request failed:\r\n" + strMessages;
                    Cursor.Current = Cursors.Default;

                    return;
                }
                else if (strMessages != string.Empty)
                {
                    tbxMessages.Text = "Request completed with warnings:\r\n" + strMessages;
                }
            }

            foreach (string xmlContent in tbResult.Xmls)
            {
                tbxData.Text += "\n" + xmlContent;
            }

            Cursor.Current = Cursors.Default;
        }

        private async void btnPost_Click(object sender, EventArgs e)
        {
            if (userData == null)
            {
                MessageBox.Show("not yet connected");
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            string xmlData = $@"<?xml version='1.0'?>
                <maxs:Contacts xmlns:maxs='http://www.microarea.it/Schema/2004/Smart/ERP/Contacts/Contacts/{cbxProfileType.Text}/{tbxProfileName.Text}.xsd' tbNamespace = 'Document.ERP.Contacts.Documents.Contacts' xTechProfile = '{tbxProfileName.Text}' >
                    <maxs:Data>
                        <maxs:Contacts master='true'>
                            <maxs:CompanyName>{txtCompanyName.Text}</maxs:CompanyName>
                            <maxs:Address>{txtAddress.Text}</maxs:Address>
                            <maxs:Telephone1>{txtPhone.Text}</maxs:Telephone1>
                            <maxs:ContactPerson>{txtContactName.Text}</maxs:ContactPerson>
                        </maxs:Contacts>
                    </maxs:Data>
                </maxs:Contacts> ";

            ITbServerMagicLinkResult tbResult = await magocloudClient.TbServer?.SetXmlData(userData, xmlData, 0, DateTime.Now);

            if (!tbResult.Success)
            {
                tbxMessages.Text = "Error in SetData\r\n" + tbResult.ReturnValue;
                Cursor.Current = Cursors.Default;

                return;
            }

            if (tbResult.Xmls.Count == 0)
            {
                tbxMessages.Text = "No postback received";
                Cursor.Current = Cursors.Default;

                return;
            }

            var strMessages = extractXMLMessages(tbResult.Xmls[0], out bool hasErrors);
            if (hasErrors)
            {
                tbxMessages.Text = "Request failed:\r\n" + strMessages;
                Cursor.Current = Cursors.Default;

                return;
            }
            else if (strMessages != string.Empty)
            {
                tbxMessages.Text = "Request completed with warnings:\r\n" + strMessages;
            }

            tbxResponse.Text += tbResult.Xmls[0];

            Cursor.Current = Cursors.Default;

        }
    }
}
