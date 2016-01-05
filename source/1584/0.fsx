//smssend.sapx

<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="smssend.aspx.cs" Inherits="_Default" validateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>How to send SMS using ASP.NET through HTTP</title>
</head>
<body>
    <center>
    <form id="smsdata" runat="server">
        <asp:Table id="smstable" runat="server" style="text-align:left; border-width:thin; border-color:Silver;" BorderStyle="Solid">
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2">
                    <b>Compose a message:</b>
                    <br />
                    <br />
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Left" VerticalAlign="Top">
                    <asp:Label ID="labelRecipient" runat="server" Text="Recipient: "></asp:Label>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:TextBox ID="textboxRecipient" runat="server"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell HorizontalAlign="Left" VerticalAlign="Top">
                    <asp:Label ID="labelMessage" runat="server" Text="Message Text: "></asp:Label>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:TextBox ID="textboxMessage" runat="server" TextMode="MultiLine"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2" HorizontalAlign="Center">
                    <asp:Button ID="buttonSend" runat="server" Text="Send Message" OnClick="buttonSendOnClick" />
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2" HorizontalAlign="Center">
                    <asp:TextBox ID="textboxError" runat="server" BorderStyle="None" TextMode="MultiLine"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </form>
    </center>
</body>
</html>

//smssend.aspx.cs

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.Text.RegularExpressions;

public partial class _Default : System.Web.UI.Page 
{
    

    protected void Page_Load(object sender, EventArgs e)
    {
        textboxRecipient.Width = 400;
        textboxMessage.Width = 450;
        textboxMessage.Rows = 10;
        textboxError.Width = 400;
        textboxError.Rows = 5;

        textboxError.ForeColor = System.Drawing.Color.Red;
        textboxError.Visible = false;
        textboxError.Text = "";

        if (!Page.IsPostBack)
        {
            textboxRecipient.Text = "+441234567";
            textboxMessage.Text = "Test message!";
        }
    }

    protected void buttonSendOnClick(object sender, EventArgs e)
    {
        //are required fields filled in:
        if (textboxRecipient.Text == "")
        {
            textboxError.Text += "Recipient(s) field must not be empty!\n";
            textboxError.Visible = true;
            return;
        }

        //we creating the necessary URL string:
        string ozSURL = "http://127.0.0.1"; //where the SMS Gateway is running
        string ozSPort = "9501"; //port number where the SMS Gateway is listening
        string ozUser = HttpUtility.UrlEncode("admin"); //username for successful login
        string ozPassw = HttpUtility.UrlEncode("abc123"); //user's password
        string ozMessageType = "SMS:TEXT"; //type of message
        string ozRecipients = HttpUtility.UrlEncode(textboxRecipient.Text); //who will get the message
        string ozMessageData = HttpUtility.UrlEncode(textboxMessage.Text); //body of message

        string createdURL = ozSURL + ":" + ozSPort + "/httpapi" +
            "?action=sendMessage" +
            "&username=" + ozUser +
            "&password=" + ozPassw +
            "&messageType=" + ozMessageType +
            "&recipient=" + ozRecipients +
            "&messageData=" + ozMessageData;

        try
        {
            //Create the request and send data to the SMS Gateway Server by HTTP connection
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(createdURL);

            //Get response from the SMS Gateway Server and read the answer
            HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
            System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
            string responseString = respStreamReader.ReadToEnd();
            respStreamReader.Close();
            myResp.Close();

            //inform the user
            textboxError.Text = responseString;
            textboxError.Visible = true;
        }
        catch (Exception)
        {
            //if sending request or getting response is not successful the SMS Gateway Server may do not run
            textboxError.Text = "The SMS Gateway Server is not running!";
            textboxError.Visible = true;
        }

    }
}