using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sage.SData.Client;
using Sage.SData.Client.Extensions;
using Sage.SData.Client.Core;
using Sage.SData.Client.Atom;

namespace InsertNewContactExistingAccountviaSData
{
    public partial class Form1 : Form
    {
        Sage.SData.Client.Core.SDataService _service;


        private void InitializeSDataForMe()
        {
            //remember _service is global (defined up above)
            _service = new Sage.SData.Client.Core.SDataService();
            //yes the above line could have been shorter if I used a using statement;
            //Doing that makes the code harder to read IMO.

            // set user name to authenticate with
            _service.UserName = "admin";
            // set password to authenticate with
            _service.Password = "";

            //http://localhost:3333/sdata/slx/dynamic/-/clientprojects
            _service.Protocol = "HTTP";
            _service.ServerName = "localhost:3333";
            _service.ApplicationName = "slx";
            _service.VirtualDirectory = "sdata";
            _service.ContractName = "dynamic";
            _service.DataSet = "-";

            //another way of doing this: 
            //mydataService = new SDataService("http://localhost:2001/sdata/slx/dynamic/-/", "admin", "");
        }

        public Form1()
        {
            InitializeComponent();
            InitializeSDataForMe();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // SDataTemplateResourceRequest needs .Core so I have a using.
            SDataTemplateResourceRequest req = new SDataTemplateResourceRequest(_service);
            req.ResourceKind = "contacts";
            //AtomEntry needs Sage.SData.Client.Atom so I added a using
            AtomEntry entry = req.Read();
            var payload = entry.GetSDataPayload();

            //get rid of these guys since we do not want to set them.
            payload.Values.Remove("CreateDate");
            payload.Values.Remove("CreateUser");
            payload.Values.Remove("ModifyDate");
            payload.Values.Remove("ModifyUser");

           payload.Values["Account"] = GetAccountPayload("AA2EK0013031");
           payload.Values["Email"] = "jason.huber@sage.com";
           payload.Values["FirstName"] = "Jason";
           payload.Values["LastName"] = "Huber";

            //removing the address, but you would want it, and it would be another payload like account....
           payload.Values.Remove("Address");
           
            //payload.Values["Description"] = txtProjectDesc.Text;
            //payload.Values["StartDate"] = Convert.ToDateTime(dtStartDate.Text).ToString("yyyy-MM-dd");
            //payload.Values["EndDate"] = Convert.ToDateTime(dtEndDate.Text).ToString("yyyy-MM-dd");

            //now we can send the entry in.
            //need to go and get the task, then update it, then send it back.
            Sage.SData.Client.Core.SDataSingleResourceRequest rcu = new Sage.SData.Client.Core.SDataSingleResourceRequest(_service);

            rcu.ResourceKind = "contacts";
            rcu.Entry = entry;
            try
            {
                AtomEntry result = rcu.Create();
                //the result here should be 201 instead of 200 because it is a create.
                //http://interop.sage.com/daisy/sdata/CreateOperation/ErrorHandling.html

                if (result.GetSDataHttpStatus() == System.Net.HttpStatusCode.Created)
                {
                    MessageBox.Show("I added the contact!");
                }
                else
                {
                    MessageBox.Show("Insert Failed. /n" + result.GetSDataHttpMessage());
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }
        private SDataPayload GetAccountPayload(string accountid)
        {
            Sage.SData.Client.Core.SDataSingleResourceRequest request = new
                   Sage.SData.Client.Core.SDataSingleResourceRequest(_service);


            request.ResourceKind = "accounts";
           

            request.ResourceSelector = string.Format("'{0}'",accountid);
            // Read the feed from the server
   

            //I did this as a seperate so that I could tell when the read comes back
            Sage.SData.Client.Atom.AtomEntry entry = request.Read();
            //and clear the please wait message.
            //first get the payload out for the entry
            return entry.GetSDataPayload();
        
        }
    }
}
