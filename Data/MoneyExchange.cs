using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO.Ports;
using MPOST;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.DataProtection;

namespace MoneyChanger.Data
{
    public class MoneyExchange
    {
        private string comportname = "COM3";
        private PowerUp pupmode = PowerUp.A;
        private MPOST.Acceptor billacceptor = new MPOST.Acceptor();
        private Money money = new Money();


        /*private ConnectedEventHandler ConnectedDelegate;
        private DisconnectedEventHandler DisconnectedDelegate;
        private EscrowEventHandler EscrowedDelegate;
        private RejectedEventHandler RejectedDelegate;
        private ReturnedEventHandler ReturnedDelegate;
        private StackedEventHandler StackedDelegate;*/

        class capabil
        {
            public string Capability { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
        }

        class billset
        {
            public string code { get; set; }
            public string value { get; set; }
            public string atributes { get; set; }
            public string enable { get; set; }
        }

        class billvalue
        {
            public string code { get; set; }
            public string value { get; set; }
            public string atributes { get; set; }
            public string enable { get; set; }
        }

        private List<capabil> listcapabil = new List<capabil>();
        private List<billset> listbillset = new List<billset>();
        private List<billvalue> listbillvalue = new List<billvalue>();

        public void addlistcapabil(string strcapability, string strvalue, string strdescription)
        {
            capabil cap = new capabil();
            cap.Capability = strcapability;
            cap.Value = strvalue;
            cap.Description = strdescription;
            listcapabil.Add(cap);
        }

        public void addlistbillset(string strcode, string strvalue, string stratributes, string strenable)
        {
            billset bill = new billset();
            bill.code = strcode;
            bill.value = strvalue;
            bill.atributes = stratributes;
            bill.enable = strenable;
            listbillset.Add(bill);
        }

        private void addlistbillvalue(string strcode, string strvalue, string stratributes, string strenable)
        {
            billvalue bill = new billvalue();
            bill.code = strcode;
            bill.value = strvalue;
            bill.atributes = stratributes;
            bill.enable = strenable;
            listbillvalue.Add(bill);
        }

        private string tipeuang = string.Empty;
        private string nilaiuang = string.Empty;

        public void initialize()
        {
            ConnectedEventHandler ConnectedDelegate = new ConnectedEventHandler(HandleConnectedEvent);
            DisconnectedEventHandler DisconnectedDelegate = new DisconnectedEventHandler(HandleDisconnectedEvent);
            EscrowEventHandler EscrowedDelegate = new EscrowEventHandler(HandleEscrowedEvent);
            RejectedEventHandler RejectedDelegate = new RejectedEventHandler(HandleRejectedEvent);
            ReturnedEventHandler ReturnedDelegate = new ReturnedEventHandler(HandleReturnedEvent);
            StackedEventHandler StackedDelegate = new StackedEventHandler(HandleStackedEvent);


            billacceptor.OnConnected += ConnectedDelegate;
            billacceptor.OnDisconnected += DisconnectedDelegate;
            billacceptor.OnEscrow += EscrowedDelegate;
            billacceptor.OnRejected += RejectedDelegate;
            billacceptor.OnReturned += ReturnedDelegate;
            billacceptor.OnStacked += StackedDelegate;
        }

        public void openfunction()
        {
            ListEvent("Bill Acceptor Open.");
            try
            {
                billacceptor.Open(comportname, pupmode);
            }
            catch (Exception err)
            {
                ListEvent("Unable to open the bill acceptor on com port <" + comportname + "> " + err.Message);
            }
        }

        public void setportname(string portname)
        {
            comportname = portname;
        }

        public void stackfunction()
        {
            ListEvent("Bill Acceptor Escrow Stack.");
            billacceptor.EscrowStack();
            string convert = convertuang(money._moneytipe, money._moneyvalue);
            ListEvent(convert);
        }

        public void returnfunction()
        {
            ListEvent("Bill Acceptor Escrow Return.");
            billacceptor.EscrowReturn();
        }

        public void closefunction()
        {
            ListEvent("Bill Acceptor Close.");
            billacceptor.Close();
        }

        private void HandleConnectedEvent(object sender, EventArgs e)
        {
            PopulateCapabilities();
            PopulateBillSet();
            PopulateBillValue();
            billacceptor.EnableAcceptance = true;
            billacceptor.AutoStack = false;
            billacceptor.OrientationCtl = OrientationControl.FourWay;
            billacceptor.OrientationCtlExt = OrientationControl.FourWay;
            ListEvent("Bill Acceptor Connected.");
        }

        private void HandleEscrowedEvent(object sender, EventArgs e)
        {
            string value = DocInfoToString(billacceptor.DocType, billacceptor.getDocument());
            ListEvent("Bill Acceptor Escrowed : " + value + ".");
            string[] arrayvalue = value.Split().ToArray();
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                    tipeuang = arrayvalue[i];
                else if (i == 1)
                    nilaiuang = arrayvalue[i];
            }
            money.setmoney(tipeuang, nilaiuang);
            ListEvent("Money Type : " + money._moneytipe);
            ListEvent("Money Value : " + money._moneyvalue);
        }

        private void HandleRejectedEvent(object sender, EventArgs e)
        {
            ListEvent("Bill Acceptor Rejected.");
        }

        private void HandleReturnedEvent(object sender, EventArgs e)
        {
            ListEvent("Bill Acceptor Returned.");
        }

        private void HandleDisconnectedEvent(object sender, EventArgs e)
        {
            ListEvent("Bill Acceptor Disconnected.");
        }

        private void HandleStackedEvent(object sender, EventArgs e)
        {
            ListEvent("Event: Stacked");
            string convert = convertuang(money._moneytipe, money._moneyvalue);
            ListEvent("Convert to IDR : " + convert);
        }

        private void PopulateCapabilities()
        {
            string[] row;
            string capability = string.Empty;
            string value = string.Empty;
            string description = string.Empty;

            row = new string[] { "CapAdvBookmark", "False", "The advanced bookmark feature is available" };
            if (billacceptor.CapAdvBookmark) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);


            row = new string[] { "CapApplicationID", "False", "The application part number is available" };
            if (billacceptor.CapApplicationID) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapApplicationPN", "False", "The application file's part number is available" };
            if (billacceptor.CapApplicationPN) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapAssetNumber", "False", "The asset number may be set." };
            if (billacceptor.CapAssetNumber) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapAudit", "False", "Audit data is available" };
            if (billacceptor.CapAudit) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapBarCodes", "False", "The unit supports bar coded documents" };
            if (billacceptor.CapBarCodes) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapBarCodesExt", "False", "Extended bar codes are supported" };
            if (billacceptor.CapBarCodesExt) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapBNFStatus", "False", "The BNFStatus property is supported" };
            if (billacceptor.CapBNFStatus) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapBookmark", "False", "Bookmark documents are supported" };
            if (billacceptor.CapBookmark) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapBootPN", "False", "The bootloader part number is available" };
            if (billacceptor.CapBootPN) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapCalibrate", "False", "The unit may be calibrated" };
            if (billacceptor.CapCalibrate) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapCashBoxTotal", "False", "The unit supports a cash box total counter" };
            if (billacceptor.CapCashBoxTotal) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapClearAudit", "False", "The unit supports the clear audit command" };
            if (billacceptor.CapClearAudit) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapCouponExt", "False", "The unit supports a extended generic coupons" };
            if (billacceptor.CapCouponExt) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapDevicePaused", "False", "The unit supports the paused state" };
            if (billacceptor.CapDevicePaused) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapDeviceSoftReset", "False", "The unit supports the soft reset command" };
            if (billacceptor.CapDeviceSoftReset) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapDeviceType", "False", "The unit reports its device type" };
            if (billacceptor.CapDeviceType) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapDeviceResets", "False", "The unit reports its reset counter" };
            if (billacceptor.CapDeviceResets) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapDeviceSerialNumber", "False", "The unit reports its serial number" };
            if (billacceptor.CapDeviceSerialNumber) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapEasiTrax", "False", "EasiTrax is supported" };
            if (billacceptor.CapEasitrax) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapEscrowTimeout", "False", "The unit supports the escrow timeout command" };
            if (billacceptor.CapEscrowTimeout) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapFlashDownload", "False", "The unit supports flash download" };
            if (billacceptor.CapFlashDownload) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapNoPush", "False", "The unit supports no_push mode" };
            if (billacceptor.CapNoPush) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapNoteRetrieved", "False", "The unit supports reporting when user takes rejected/returned notes" };
            if (billacceptor.CapNoteRetrieved) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapOrientationExt", "False", "The unit supports extended handling of bill orientation" };
            if (billacceptor.CapOrientationExt) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapPupExt", "False", "The unit supports extended PUP mode" };
            if (billacceptor.CapPupExt) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapSetBezel", "False", "The bezel may be configured" };
            if (billacceptor.CapSetBezel) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapTestDoc", "False", "Special Test Documents are supported" };
            if (billacceptor.CapTestDoc) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapVariantID", "False", "The variant part number is available" };
            if (billacceptor.CapVariantID) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);

            row = new string[] { "CapVariantPN", "False", "The variant file's part number is available" };
            if (billacceptor.CapVariantPN) row[1] = "True";
            for (int i = 0; i < row.Length; i++)
            {
                if (i == 0)
                    capability = row[i];
                else if (i == 1)
                {
                    row[i] = "True";
                    value = row[i];
                }
                else if (i == 2)
                    description = row[i];
            }
            addlistcapabil(capability, value, description);
        }

        private void PopulateBillSet()
        {
            MPOST.Bill[] Bills = billacceptor.BillTypes;
            Boolean[] Enables = billacceptor.GetBillTypeEnables();
            string code = string.Empty;
            string value = string.Empty;
            string atributes = string.Empty;
            string enable = string.Empty;

            for (int i = 0; i < Bills.Length; i++)
            {
                code = Bills[i].Country;
                value = Bills[i].Value.ToString();
                atributes = Bills[i].Type.ToString() +
                         " " +
                         Bills[i].Series.ToString() +
                         " " +
                         Bills[i].Compatibility.ToString() +
                         " " +
                         Bills[i].Version.ToString();
                enable = Enables[i] ? "True" : "False";

                addlistbillset(code, value, atributes, enable);
            }
        }

        private void PopulateBillValue()
        {
            MPOST.Bill[] Bills = billacceptor.BillValues;
            Boolean[] Enables = billacceptor.GetBillValueEnables();
            string code = string.Empty;
            string value = string.Empty;
            string atributes = string.Empty;
            string enable = string.Empty;

            for (int i = 0; i < Bills.Length; i++)
            {
                code = Bills[i].Country;
                value = Bills[i].Value.ToString();
                atributes = Bills[i].Type.ToString() +
                         " " +
                         Bills[i].Series.ToString() +
                         " " +
                         Bills[i].Compatibility.ToString() +
                         " " +
                         Bills[i].Version.ToString();
                enable = Enables[i] ? "True" : "False";

                addlistbillvalue(code, value, atributes, enable);
            }
        }
        private String DocInfoToString(DocumentType docType, IDocument doc)
        {
            string result = string.Empty;
            if (docType == DocumentType.None)
                result = "Doc Type: None";
            else if (docType == DocumentType.NoValue)
                result = "Doc Type: No Value";
            else if (docType == DocumentType.Bill)
            {
                if (doc == null)
                    result = "Doc Type Bill = null";
                else
                    result = doc.ToString() + " (" + billacceptor.EscrowOrientation.ToString() + ")";
            }
            return result;
        }

        private void ListEvent(string listevent)
        {
            Console.WriteLine(listevent);
        }

        public String convertuang(string strtipe, string strnilai)
        {
            string result = string.Empty;
            int changer = int.Parse(strnilai);
            switch (strtipe)
            {
                case "USD":
                    {
                        changer = changer * 14000;
                        break;
                    }
                case "EUR":
                    {
                        changer = changer * 16000;
                        break;
                    }
                case "JPY":
                    {
                        changer = changer * 132;
                        break;
                    }
                case "CNY":
                    {
                        changer = changer * 2000;
                        break;
                    }
                case "KRW":
                    {
                        changer = changer * 12;
                        break;
                    }
                case "GBP":
                    {
                        changer = changer * 18000;
                        break;
                    }
                case "AUD":
                    {
                        changer = changer * 10000;
                        break;
                    }
                case "TWD":
                    {
                        changer = changer * 477;
                        break;
                    }
                case "PHP":
                    {
                        changer = changer * 282;
                        break;
                    }
                case "HKD":
                    {
                        changer = changer * 2000;
                        break;
                    }
                case "SGD":
                    {
                        changer = changer * 10000;
                        break;
                    }
                case "MYR":
                    {
                        changer = changer * 3300;
                        break;
                    }
                case "THB":
                    {
                        changer = changer * 453;
                        break;
                    }
            }
            result = changer.ToString("N0");
            return result;
        }
    }
}


