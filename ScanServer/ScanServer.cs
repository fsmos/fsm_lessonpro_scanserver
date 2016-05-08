using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using ScanServer.LessonProDataSetTableAdapters;
using WiaDotNet;
using WIA;
using System.IO;
namespace ScanServer
{
  
    public partial class ScanService : ServiceBase
    {
        public ScanService()
        {
            InitializeComponent();
        }
        Thread ScanServ;
        WiaManager mg;
        Scaner scans;
        FileTableAdapter fl;
        System.Data.SqlClient.SqlConnection scq;
        protected override void OnStart(string[] args)
        {
            StreamReader str =new StreamReader(Environment.GetEnvironmentVariable("SYSTEMDRIVE")+"\\ScanServerSetting\\Setting.scan");
            fl = new FileTableAdapter();
           scq = new System.Data.SqlClient.SqlConnection(string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", str.ReadLine(), str.ReadLine()));

            fl.Connection=scq;
            mg = new WiaManager();
            scans = new Scaner(Environment.MachineName,scq);
            Scaning sc =new Scaning(Scan);
            ScanServ =new Thread(ServerStart);
            ScanServ.Start(sc);
         
                
        }
        delegate void Scaning(int index);
        protected static void ServerStart(object fs)
        {
            StreamReader str = new StreamReader(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\ScanServerSetting\\Setting.scan");
      
            System.Data.SqlClient.SqlConnection scq = new System.Data.SqlClient.SqlConnection(string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", str.ReadLine(), str.ReadLine()));
            Scaner scans = new Scaner(Environment.MachineName,scq);
            Scaning scann = (Scaning)fs;
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Объявляем сервер

            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, scans.Port);

            listenSocket.Bind(ipEndpoint);

            listenSocket.Listen(1);
            while (true)
            {
                byte[] file=new byte[2];
                Socket handler = listenSocket.Accept();
                handler.Receive(file);
                scann((file[1] * 255) + file[0]);
                handler.Close(); 
            }
        }
        protected void Scan(int index)
        {
       
            ScaningFile sc = new ScaningFile(index,scq);
            for (int i = 0; i < sc.count; i++)
            {
                try
                {
                    WIA.ImageFile img = mg.AcquireScan(mg.Devices[scans.ScanerId], DocumentSources.SingleSided, sc.sctype, ScanQualityTypes.None, sc.DPI);
                    string filepatch = "";
                    if (i == 0) filepatch = sc.ScanFile + "." + sc.FileType;
                    else filepatch = sc.ScanFile + i.ToString()  +"." + sc.FileType;
                    img.SaveFile(filepatch);
                    fl.Insert(sc.lessonid, sc.name + i.ToString()+"." + sc.FileType, filepatch);
                }
                catch
                {
                }
            }
            
        }

        void scaner_AcquireCompleted(object sender, EventArgs e)
        {
           
        }
        protected override void OnStop()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
    public class Scaner
    {
        public string ScanerName;
        public int ScanerId;
        public int Port;
        ScanerTableAdapter tl;
        public Scaner(string PCName, System.Data.SqlClient.SqlConnection scq)
        {
            tl = new ScanerTableAdapter();
            tl.Connection = scq;
            LessonProDataSet.ScanerDataTable st = tl.Connect(PCName);
            ScanerName = (string)st.Rows[0].ItemArray[1];
            ScanerId = (int)st.Rows[0].ItemArray[2];
            Port = (int)st.Rows[0].ItemArray[6];
        }
    }
    public class ScaningFile
    {
        public string ScanFile;
            public int DPI;
            public ScanTypes sctype;
            public int count;
            public int lessonid;
            public string FileType;
            public string name;
            ScanTableAdapter sc;
            public ScaningFile(int id, System.Data.SqlClient.SqlConnection scq)
          {
              sc = new ScanTableAdapter();
              sc.Connection = scq;
              LessonProDataSet.ScanDataTable scd= sc.ScanGet(id);
              name = (string)scd.Rows[0].ItemArray[1];
              ScanFile = (string)scd.Rows[0].ItemArray[2];
              DPI = (int)scd.Rows[0].ItemArray[3];
               switch((string)scd.Rows[0].ItemArray[4])
               {
                   case "Color":
                       sctype = ScanTypes.Color;
                       break;
                   case "Grayscale":
                       sctype = ScanTypes.Grayscale;
                       break;
                   case "Text":
                       sctype = ScanTypes.Text;
                       break;
                   default:
                       sctype = ScanTypes.Color;
                       break;
             }
               count = (int)scd.Rows[0].ItemArray[5];
              lessonid = (int)scd.Rows[0].ItemArray[6];
               FileType = (string)scd.Rows[0].ItemArray[7];
           }  
    }
}
