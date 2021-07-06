using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public class Email
{

    public static string smtpserver = "smtp.gmail.com";
    //"AKLEXCH10.nz-window-shades.co.nz";

    public static void SendMail(string FromAddress, string ToAddress, string MailSubject, string MailBody, bool isHTML = false, string[] Attachments = null)
    {
        SendMail(FromAddress, new string[] { ToAddress }, new string[] { "" }, MailSubject, MailBody, isHTML, Attachments);

    }

    public static void SendMail(string FromAddress, string ToAddress, string ccAddress, string MailSubject, string MailBody, bool isHTML = false, string[] Attachments = null)
    {
        SendMail(FromAddress, new string[] { ToAddress }, new string[] { ccAddress }, MailSubject, MailBody, isHTML, Attachments);

    }

    public static void SendMail(string FromAddress, string[] ToAddress, string[] ccAddress, string MailSubject, string MailBody, bool isHTML = false, string[] Attachments = null)
    {
        System.Net.Mail.MailMessage myMessage = new System.Net.Mail.MailMessage();


        myMessage.From = new System.Net.Mail.MailAddress(FromAddress);

        if (ToAddress.Length == 0)
            throw new Exception("You must supply at least one email recipient!");

        foreach (object address in ToAddress)
        {
            myMessage.To.Add(address.ToString());
        }

        foreach (object address in ccAddress)
        {

            if ((address.ToString() != null) & !string.IsNullOrEmpty(address.ToString()))
                myMessage.CC.Add(address.ToString());
        }

        myMessage.Subject = MailSubject;

        myMessage.BodyEncoding = System.Text.Encoding.UTF8;
        myMessage.IsBodyHtml = isHTML;
        myMessage.Body = MailBody;

        if ((Attachments != null))
        {
            foreach (object Attachment in Attachments)
            {
                if ((Attachment != null) & !string.IsNullOrEmpty(Attachment.ToString()))
                    myMessage.Attachments.Add(new System.Net.Mail.Attachment(Attachment.ToString()));
            }
        }
        //p00dl3m@nia

        System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smtpserver);
        smtp.Credentials = new System.Net.NetworkCredential("softintelnz@gmail.com", "p00dl3m@nia");
        smtp.Port = 587;
        smtp.EnableSsl = true;

        smtp.Send(myMessage);
        myMessage.Dispose();

    }


    public static bool isValid(string emailAddress)
    {
        string pattern = "^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$";
        System.Text.RegularExpressions.Match emailAddressMatch = System.Text.RegularExpressions.Regex.Match(emailAddress.Trim(), pattern);

        if (emailAddressMatch.Success)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public static bool isValid(string[] emailAddresses)
    {
        foreach (object emailaddress in emailAddresses)
        {

            if (!isValid(emailaddress.ToString()))
            {
                return false;
            }
        }

        return true;
    }

    public static string[] RemoveInvalidEmails(string[] EmailAddresses)
    {
        List<string> EmailAddressesClean = new List<string>();
        foreach (object EmailAddress in EmailAddresses)
        {

            if (isValid(EmailAddress.ToString()))
                EmailAddressesClean.Add(EmailAddress.ToString());
        }

        return EmailAddressesClean.ToArray();
    }

}

