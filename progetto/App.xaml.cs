using System;
using System.Windows;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace progetto
{
   
    public partial class App : System.Windows.Application
    {
        public Thread workerThread3;
        public Thread workerThread;
        internal static progetto.App app;
        public static string[] Args;
        bool trovato;

        // avvia l applicazione per la prima volta   

        // funzione che parte e decide ci sono o non ci sono argomenti quindi si o no file

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            app = this;


            if (e.Args.Length < 1)
            {
                 trovato = false;
                 /* Process[] processlist = Process.GetProcesses();

                   foreach (Process theprocess in processlist)
                   {
                       // Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                       if (theprocess.ProcessName == "progetto")
                       {
                           theprocess.
                           trovato = true;
                       }
                   }*/
                if (trovato == false)
                {
                    MainWindow m = new progetto.MainWindow();
                    // server che consente di scaricare l immagine 
                    m.Show();
                    m.Activate();
                    invia_immagine();

                    if (progetto.Properties.Settings.Default.online == true)
                    {
                        // thread che lancia pacchetti udp per indica che sei online
                        avviainvio();
                    }
                }

            }
            else
            {

                Window1 c = new progetto.Window1();
                c.Hide();


            }
          
        }

        public void avviainvio()
        {
           
            workerThread = new Thread(() => invia());
            workerThread.Start();



        }

      
        public void invia()
        {
            UdpClient client2 = new UdpClient();
            int port = 15000;


            try
            {
                while (true)
                {
                    string message = "";
                    var call = "CALL";
                    int copiati = 0;
                    byte[] pacchetto = new byte[1024];

                    message = message + call;
                    byte[] nomeuser = Encoding.ASCII.GetBytes(progetto.Properties.Settings.Default.User);
                    string si_no = progetto.Properties.Settings.Default.modificafoto;
                    byte[] foto_si_no = Encoding.ASCII.GetBytes(progetto.Properties.Settings.Default.modificafoto);
                    byte[] lunghezzanome_convertito = BitConverter.GetBytes(nomeuser.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(call), 0, pacchetto, 0, Encoding.UTF8.GetBytes(call).Length);
                    copiati += Encoding.UTF8.GetBytes(call).Length;
                    Array.Copy(lunghezzanome_convertito, 0, pacchetto, copiati, call.Length);
                    copiati += lunghezzanome_convertito.Length;
                    Array.Copy(nomeuser, 0, pacchetto, copiati, nomeuser.Length);
                    copiati += nomeuser.Length;
                    Array.Copy(foto_si_no, 0, pacchetto, copiati, foto_si_no.Length);
                    copiati += foto_si_no.Length;
                    client2.Send(pacchetto, pacchetto.Length, IPAddress.Broadcast.ToString(), port);
                    Thread.Sleep(10000);
                   


                }
            }
            catch (Exception)
            {
                progetto.Properties.Settings.Default.online = false;
                var offline = "OFFF";
                byte[] pacchetto = new byte[1024];
                Array.Copy(Encoding.UTF8.GetBytes(offline), 0, pacchetto, 0, Encoding.UTF8.GetBytes(offline).Length);
                client2.Send(pacchetto, pacchetto.Length, IPAddress.Broadcast.ToString(), port);
                client2.Close();

            }






        }



        public void inviaimmagine()
        {


            TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 12000);
            server.Start();
            byte[] data;
            byte[] tmp = new byte[1024];


            while (true)
            {
                var acceptSocket = server.AcceptTcpClient();

                  var Uri = new Uri(progetto.Properties.Settings.Default.Foto, UriKind.RelativeOrAbsolute);
               
                var namefile = GetFileName(Uri.ToString());
                //JpegBitmapEncoder Definisce un codificatore usato per codificare le immagini in formato Joint Photographics Experts Group(JPEG).
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //  how to initialize a T:System.Windows.Media.Imaging.JpegBitmapEncoder to encode a new bitmap image.
                encoder.Frames.Add(BitmapFrame.Create(getBitmap(Uri)));

                /*
                 la classe memory stream rappresenta uno stream che utilizza un array in memoria
                 come archivio di memorizzazione di dati
                 Avere i byte in memoria è megio xk aumenta le performance ed è possibile manipolare i byte

                 */
                using (MemoryStream mymemorystream = new MemoryStream())
                {
                    var sw = new StreamWriter(mymemorystream);

                 
                    // salvo file
                    encoder.Save(mymemorystream);
                    /*
                     * metodo.

Questo metodo restituisce una copia del contenuto di MemoryStream come una matrice di byte. Se l'istanza 
corrente è stato creato in una matrice di byte specificata, viene restituita una copia della sezione della matrice a cui questa istanza ha accesso
                     * */
                    data = mymemorystream.ToArray();
                }
                //  intBytes = BitConverter.GetBytes(data.Length);
                // in ns ce il flusso di dati stream
                NetworkStream flusso = acceptSocket.GetStream();
                try
                {

                    MemoryStream mymemorystream2 = new MemoryStream(data);
                    // scrivo i dati ms nel flusso ns
                    mymemorystream2.CopyTo(flusso);

                }
                catch (Exception)
                { }
                //   System.Windows.MessageBox.Show("immagine inviata");
                progetto.Properties.Settings.Default.modificafoto = "no";
                acceptSocket.Close();


            }

        }

        private string GetFileName(string hrefLink)
        {
            string[] parts = hrefLink.Split('/');
            string fileName = "";

            if (parts.Length > 0)
                fileName = parts[parts.Length - 1];
            else
                fileName = hrefLink;

            return fileName;
        }
        public BitmapImage getBitmap(Uri uri)
        {

            BitmapImage bi = new BitmapImage();

            BitmapImage source = new BitmapImage(uri);
            // Begin initialization.
            bi.BeginInit();

            // Set properties.
            bi.CacheOption = BitmapCacheOption.OnDemand;
            bi.CreateOptions = BitmapCreateOptions.DelayCreation;
            //double divider = (source.Height / 350)+1;
            double divider = (source.Width / 350) + 1;
            bi.DecodePixelHeight = (int)(source.Height / divider);
            //bi.DecodePixelWidth = 10;
            bi.DecodePixelWidth = (int)(source.Width / divider);
            //bi.DecodePixelHeight = 10;
            bi.UriSource = uri;
            bi.EndInit();
            return bi;
        }
        public void stop_workerthread()
        {
      
            if (workerThread != null)
            {
                workerThread.Abort();
            }
       
        }

        public void invia_immagine()
        {
            workerThread3 = new Thread(() => inviaimmagine());
            workerThread3.Start();
        }

        

      

        

       
  







    }
}
