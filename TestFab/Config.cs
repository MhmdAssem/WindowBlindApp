using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Config
{
    private string xlfilepath = null;
    private string logpath = null;
    private string columns = null;
    private int Counter = 5;
    private string toEmailAddress = null;
    private string SheetName = null;
    private string SavePath = null;
    private string FTPSite = null;
    private string FTPUsername = null;
    private string FTPPassword = null;
    private string Rollwithfile = null;
    private string Lathefile = null;
    private string PrinterName = null;
    private string WidthDeduction = null;


    public string RollWidth
    {
        get
        {
            return Rollwithfile;
        }
        set
        {

            Rollwithfile = value;

        }
    }

    public string Printer
    {
        get
        {
            return PrinterName;
        }
        set
        {

            PrinterName = value;

        }
    }



    public string WidthDeduct
        {
        get
        {
            return WidthDeduction;
        }
        set
        {

            WidthDeduction = value;

        }
    }

    public string Lathe
    {
        get
        {
            return Lathefile;
        }
        set
        {

            Lathefile = value;

        }
    }





    public string FilePath
    {
        get
        {
            return xlfilepath;
        }
        set
        {

            xlfilepath = value;

        }
    }

    public string ftpSiteName
    {
        get
        {
            return FTPSite;
        }
        set
        {

            FTPSite = value;

        }
    }

    public string FTPSiteUsername
    {
        get
        {
            return FTPUsername;
        }
        set
        {

            FTPUsername = value;

        }
    }

    public string FTPSitepassword
    {
        get
        {
            return FTPPassword;
        }
        set
        {

            FTPPassword = value;

        }
    }



    public string SaveFilesPath
    {
        get
        {
            return SavePath;
        }
        set
        {

            SavePath = value;

        }
    }

    public string ToEmailAddress
    {
        get
        {
            return toEmailAddress;
        }
        set
        {

            toEmailAddress = value;

        }
    }


    public string LogPath
    {
        get
        {
            return logpath;
        }
        set
        {
            logpath = value;

        }
    }



    public string SheetNameValue
    {
        get
        {
            return SheetName;
        }
        set
        {
            SheetName = value;

        }
    }

    public int CounterValue
    {
        get
        {
            return Counter;
        }
        set
        {
            Counter = value;

        }
    }

    public string Columns
    {
        get
        {
            return columns;
        }
        set
        {
            columns = value;

        }

    }




}

