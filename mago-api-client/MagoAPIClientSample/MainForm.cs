using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MagoAPIClientSample
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //    ITbUserData userData = null;
            //    try
            //    {
            //        using (MagoAPIClient magocloudClient = new MagoAPIClient(instance, new ProducerInfo("producer key", "app ID")))
            //        {
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex);
            //        if (userData != null && magocloudClient.AccountManager.IsValid(userData).Result.Success)
            //            return magocloudClient.AccountManager.Logout(userData).Result.Success;
            //    }
        }
    }
}
