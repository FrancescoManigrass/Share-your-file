using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

using System.IO;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.Compression;
using System.Windows.Shapes;
using System.Windows.Forms;

using System.Security.AccessControl;
using System.ComponentModel;

namespace progetto
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {

        //  public Person PersonOne { get; set; }
        public ObservableCollection<Person> PersonOne { get; set; }
        public ObservableCollection<File> Lista_file { get; set; }
        public List<string> Lista_file_2 { get; set; }
        public ObservableCollection<string> persone_a_cui_inviare { get; set; }
        public ObservableCollection<Person> persone_a_cui_inviare2 { get; set; }
        public Grid progressBarGrid { get; set; }
        public System.Windows.Controls.ProgressBar progressBar { get; set; }
        private static  Mutex mut = new Mutex();
        public bool IsAcquired { get; private set; }
        public bool zip_pronti { get; private set; }
        private static object syncPrimitive = new object(); // this is legal
        public Thread workerthread2 = null;
        public Thread zippatore = null;
        public Thread workerthread_invia_file = null;
        Thread server_pipe;
        Task task;
        CancellationTokenSource token;

        internal static progetto.MainWindow m;
        public Window2 f;
        NotifyIcon nIcon;
        internal static progetto.MainWindow main;
        int numero_thread;
        public Thread workerthreadricevifile = null;
        public bool avvio = false;

        TcpListener server;
        int totalepersone = 0;
        Socket acceptSocket;
        public MainWindow()
        {

            InitializeComponent();
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) {

                 Environment.Exit(0);
              
            }
            // riceve sulla pipe gli argomenti mandati dalla window1
            server_pipe = new Thread(() => StartServer());
            server_pipe.Start();

            share_button.DataContext = new System.Windows.Controls.Button() { Content = "Share" };            
            lista_vuota_2.DataContext = new System.Windows.Controls.TextBox() { Visibility = Visibility.Visible };
       
       
           
           
            // lista di persone a cui voglio inviare i file -> quelle dove metto la spunta( devo lasciarne solo una delle due )
            persone_a_cui_inviare = new ObservableCollection<string>();
            persone_a_cui_inviare2 = new ObservableCollection<Person>();
            // aggiunge la notifica in basso a destra
            notifica();
            // LISTA DEI File da inviare
            Lista_file = new ObservableCollection<File>();
            Lista_file_2 = new List<string>();
            // lista di persone online
            PersonOne = new ObservableCollection<Person>();

            // aggiunge la chiave di registro per far uscire la spunta in invia con
           // AddOption_ContextMenu();

            // thread per ricevere il file
            workerthreadricevifile = new Thread(() => ricevi_file());
            workerthreadricevifile.Start();
            // riceve il pacchetto udp delle persone prende l'ip nome e scarica la foto tramite ricevi_immagine
            workerthread2 = new Thread(() => ricevi());
            workerthread2.Start();
      

            File.ItemsSource = Lista_file;
            Persone.ItemsSource = PersonOne;
            // mostra la finestra
            //this.Show();
            m = this;
            main = this;
            this.DataContext = this;


        }
        public void ricevi_file()
        {
            server = new TcpListener(IPAddress.Parse("0.0.0.0"), 12001);
            server.Start();
            string nomefile = null;
            int n = 0;
            int lunghezzanome = 0;
            long peso_file = 0;
            int bytericevuti = 0;
            BinaryWriter bwrite = null;
            string percorso = null;
            string fileNameOnly = null;
            string extension = null;


            while (true)
            {
                try
                {
                    // essendo tcp aspetta che qualcuno si connetta
                    acceptSocket = server.AcceptSocket();
                    // qualcuno si è connesso
                    byte[] pacchetto = new byte[3];
                    if (acceptSocket.Connected)
                    {
                        int i = acceptSocket.Receive(pacchetto);
                        if (i == 3)
                        {
                            if (Encoding.ASCII.GetString(pacchetto) == "PUT")
                            {

                                // System.Windows.MessageBox.Show("pacchetto ricevuto");
                                pacchetto = new byte[4];
                                // ricevo la lunghezza del nome del file su 4 byte
                                bytericevuti = acceptSocket.Receive(pacchetto);
                                // traduco la lunghezza da networkbye order a intero
                                lunghezzanome = BitConverter.ToInt32(pacchetto, 0);
                                pacchetto = new byte[lunghezzanome];
                                bytericevuti = acceptSocket.Receive(pacchetto);
                                //leggo il nome del file
                                nomefile = Encoding.ASCII.GetString(pacchetto);
                                pacchetto = new byte[4];
                                bytericevuti = acceptSocket.Receive(pacchetto);
                                // ricevo lunghezza nome utente
                                int lunghezzanomeutente = BitConverter.ToInt32(pacchetto, 0);
                                pacchetto = new byte[lunghezzanomeutente];
                                n = acceptSocket.Receive(pacchetto);
                                // ricevo nome utente
                                string nomeutente = Encoding.ASCII.GetString(pacchetto);
                                percorso = progetto.Properties.Settings.Default.download;
                                if (Properties.Settings.Default.new_folder == true)
                                {
                                    System.Windows.Forms.DialogResult dialogResult2 = System.Windows.Forms.MessageBox.Show(" Change download folder ?", "Send to your friends", System.Windows.Forms.MessageBoxButtons.YesNo);
                                    if (dialogResult2 == System.Windows.Forms.DialogResult.Yes)
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                                        {

                                            using (var fbd = new FolderBrowserDialog())
                                            {
                                                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                                                if (result == System.Windows.Forms.DialogResult.OK)
                                                {
                                                    percorso = fbd.SelectedPath;

                                                }
                                            }
                                        }));
                                    }

                                }
                                percorso = percorso + "\\" + nomefile;
                                fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(percorso);
                                extension = System.IO.Path.GetExtension(percorso);
                                System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.DialogResult.Yes;
                                if (Properties.Settings.Default.all_file == false)
                                {
                                    dialogResult = System.Windows.Forms.MessageBox.Show("Recive " + nomefile + " from " + nomeutente + " ?", "Send to your friends", System.Windows.Forms.MessageBoxButtons.YesNo);
                                }
                                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                                {
                                    //do something
                                    byte[] ok = Encoding.UTF8.GetBytes("+OK");

                                    acceptSocket.Send(ok, 0, 3, 0);


                                    if (System.IO.File.Exists(percorso))
                                    {
                                        int count = 1;
                                        // prendo in nome del file dal percorso
                                        fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(percorso);
                                        // prendo estensione
                                        extension = System.IO.Path.GetExtension(percorso);
                                        string path = System.IO.Path.GetDirectoryName(percorso);
                                        string newFullPath = percorso;
                                        while ((System.IO.File.Exists(newFullPath)))
                                        {
                                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                            newFullPath = System.IO.Path.Combine(path, tempFileName + extension);
                                        }
                                        percorso = newFullPath;
                                    }

                                    bwrite = new BinaryWriter(System.IO.File.Open(percorso, FileMode.Append));
                                    pacchetto = new byte[8];

                                    n = acceptSocket.Receive(pacchetto);

                                    peso_file = BitConverter.ToInt64(pacchetto, 0);
                                    pacchetto = new byte[1024];
                                    // peso_iniziale = tutto
                                    // peso_file quanto è rimasto
                                    // peso_inviato  quello effettivamente inviato
                                    long peso_iniziale = peso_file;
                                    long peso_inviato = 0;
                                    int differenza = -1;
                                    pacchetto = new byte[2048];

                                    while (peso_file > 0 && bytericevuti > 0)
                                    {

                                        bytericevuti = acceptSocket.Receive(pacchetto, 1024, SocketFlags.None);

                                        if (!acceptSocket.Connected)
                                        {
                                            System.Windows.MessageBox.Show("not connect");
                                        }
                                        if (bytericevuti <= 0)
                                        {
                                            // System.Windows.MessageBox.Show(" <0 ricevuti");

                                        }
                                        else
                                        {

                                            // scrive da 0 a 1024
                                            bwrite.Write(pacchetto, 0, bytericevuti);
                                            peso_file -= (int)bytericevuti;
                                            //peso_file -= 1024;
                                            peso_inviato += bytericevuti;
                                            //peso_inviato += 1024;
                                            differenza = (int)(peso_iniziale - peso_inviato);
                                        }


                                    }

                                    bwrite.Close();

                                    if (differenza == 0 && peso_iniziale != 0)
                                    {

                                        nIcon.Visible = nIcon.Visible;
                                        nIcon.ShowBalloonTip(3000, "Download Completed", "The download of File " + fileNameOnly + extension + " is completed", System.Windows.Forms.ToolTipIcon.Info);
                                    }
                                    else
                                    {
                                        nIcon.Visible = true;
                                        nIcon.ShowBalloonTip(3000, "Download Failed", "The download of File " + fileNameOnly + extension + " is failed", System.Windows.Forms.ToolTipIcon.Error);

                                        acceptSocket.Close();
                                        System.IO.File.Delete(percorso);
                                    }


                                }
                                else if (dialogResult == System.Windows.Forms.DialogResult.No)
                                {
                                    //do something else
                                    byte[] ERR = Encoding.UTF8.GetBytes("ERR");
                                    acceptSocket.Send(ERR, 0, 3, 0);
                                    if (acceptSocket.Connected)
                                    {
                                        acceptSocket.Close();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {


                    }

                }
                catch
                {
                    System.Windows.MessageBox.Show("eccezione ricezione file");
                    acceptSocket.Close();

                    if (bwrite != null)
                    {
                        bwrite.Close();
                    }
                    if (peso_file != bytericevuti)
                    {
                        if (System.IO.File.Exists(percorso))
                        {
                            System.IO.File.Delete(percorso);
                            nIcon.ShowBalloonTip(3000, "Download Failed", "The download of File " + fileNameOnly + extension + " is failed", System.Windows.Forms.ToolTipIcon.Error);

                        }

                    }
                }

            }

        }
        private void scambio()
        {

            share_button.Visibility = Visibility.Visible;
                share_button.Content = "Share";
            
        }

        public byte[] riceviimmagine(IPAddress ip)
        {
            TcpClient client = new TcpClient();
            client.Connect(ip, 12000);
            NetworkStream flusso = client.GetStream();
            byte[] data;

            try
            {
                using (var stream = flusso)
                {
                    MemoryStream mystream = new MemoryStream();
                    flusso.CopyTo(mystream);


                    data = mystream.ToArray();

                    return data;

                }
            }

            catch (Exception)
            {

                Console.Write("eccezione immagine");
            }


            return null;

        }
        public void ricevi()
        {

            int port = 15000;
            UdpClient client = null;
            int i = 0;
            int j = 0;

            //  client.Client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

            try
            {
                client = new UdpClient(port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
                System.Windows.MessageBox.Show("You have already started an instance of this program");
               
            }

            IPEndPoint server = new IPEndPoint(IPAddress.Any, 0);
            var sw = new Stopwatch();
            sw.Start();
            while (true)
            {

                try
                {

                    byte[] packet = client.Receive(ref server);
                    string remoteIpEndPoint = server.Address.ToString();
                    string myip = GetLocalIPAddress();
                    string messaggio_ricevuto = Encoding.ASCII.GetString(packet);
                    byte[] array = null;
                    byte[] chiamata = new byte[4];
                    bool trovato2 = false;
                    
                    for (i = 0; i < 4; i++)
                    {
                        chiamata[i] = packet[i];
                    }
                    int lunghezzanome = 0;
                    string nome = null;


                    if ((remoteIpEndPoint != myip || Properties.Settings.Default.yourself == true) && myip.ToString() != IPAddress.Loopback.ToString())
                    {

                        lunghezzanome = 0;
                        Person tmp = new Person();
                        bool trovato = false;
                       

                        Dispatcher.Invoke(new Action(() =>
                        {

                            if (Encoding.ASCII.GetString(chiamata) == "CALL")
                            {
                                
                                array = new byte[8];
                                for (j = i; j < 8; j++)
                                {
                                    array[j - i] = packet[j];
                                }
                                lunghezzanome = BitConverter.ToInt32(array, 0);

                                array = new byte[lunghezzanome];
                                i = j;

                                for (j = i; j < lunghezzanome + i; j++)
                                {

                                    array[j - i] = packet[j];

                                }
                                i = j;
                                nome = Encoding.ASCII.GetString(array);

                                tmp.Name = nome;
                                array = new byte[2];
                                for (j = i; j < i + 2; j++)
                                {
                                    array[j - i] = packet[j];
                                }
                                tmp.modificafoto = Encoding.ASCII.GetString(array);

                                ImageSource imageSource1 = new BitmapImage(new Uri("user.png", UriKind.Relative));
                                tmp.foto = (System.Windows.Media.Imaging.BitmapSource)imageSource1;
                                tmp.Address = remoteIpEndPoint;
                                var data = riceviimmagine(IPAddress.Parse(tmp.Address));
                                tmp.foto = ByteArraytoBitmap(data);
                                trovato2 = false;
                                List<Person> temporanea = PersonOne.ToList();
                                var item = PersonOne.FirstOrDefault(x => x.Address == tmp.Address);
                                if (item != null && !persone_a_cui_inviare.Contains(item.Address))
                                {
                                    

                                    PersonOne.Remove(item);
                                    PersonOne.Add(tmp);
                                }
                                else if (item == null)
                                {
                                    PersonOne.Add(tmp);
                                }



                              
                                    if (sw.ElapsedMilliseconds / 10 == 0)
                                    {
                                  
                                        PersonOne.Clear();
                                    }
                              
                            }
                            else if (Encoding.ASCII.GetString(chiamata) == "OFFF")
                            {
                               
                                List<Person> temporanea = PersonOne.ToList();

                                foreach (Person p in temporanea)
                                {
                                    if (p.Address == remoteIpEndPoint)
                                    {
                                        var item = persone_a_cui_inviare2.FirstOrDefault(x => x.Address == p.Address);
                                        
                                        
                                     if (item==null)
                                        {
                                            Dispatcher.Invoke(new Action(() =>
                                            {
                                                PersonOne.Remove(p);
                                            }));
                                        }
                                        else
                                        {
                                            item.da_rimuovere = true;
                                        }
                                       
                                    }
                                }


                            }

                        }));



                    }




                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // System.Windows.MessageBox.Show("eccezione");
                }



            }


        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        public static BitmapImage ByteArraytoBitmap(Byte[] byteArray)
        {
            MemoryStream stream = new MemoryStream(byteArray);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();

            // Set properties.
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;

            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }
        private async void ButtonBase_OnClickAsync(object sender, RoutedEventArgs e)
        {

            //  invia();

            if (share_button.Content.ToString() == "Share")
            {

                if (share_button.Content.ToString() == "Stop All")
                {
                    share_button.Content = "Share";
                }
                if (persone_a_cui_inviare.Count() != 0 && Lista_file.Count != 0)
                {
                    List<Person> pers = persone_a_cui_inviare2.ToList();
                    numero_thread = 0;
                    foreach (Person s in pers)
                    {
                        if (s.thread == null)
                        {
                            numero_thread++;
                        }
                    }
                    if (numero_thread >= pers.Count())
                    {
                        totalepersone = 0;

                        //zippatore = new Thread(() => zippa_tutto());
                        //zippatore.Start();
                        //await Task.Factory.StartNew(() => zippa_tutto());
                        token= new CancellationTokenSource();
                        task = Task.Run(() => zippa_tutto(token),token.Token);
                       // share_button.Visibility = Visibility.Collapsed;
                        share_button.Content = "Stop All";
                        await task;
                        if (!token.IsCancellationRequested)
                        {
                            foreach (Person s in pers)
                            {

                                s.thread = new Thread(() => invia_file(s));
                                s.numero_file_da_inviare = Lista_file.Count();
                                s.numero_file_inviati = 0;
                                s.thread.Start();
                                share_button.Content = "Stop All";
                            }
                        }
                      
                        
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Before you press share, wait for all files to be sent");

                    }



                }
                else
                {
                    System.Windows.MessageBox.Show("Before you share files, select  file or users");
                }

            }
            else
            {
                Boolean si_no = false;
               
                if (si_no == false)
                {
                    token.Cancel();
                    string current = Directory.GetCurrentDirectory();
                    string current_zip = current + "\\tmp";
                    //task.c
                    //task.Dispose();
                    await task;
                    //Directory.Delete(current_zip, true);

                    foreach (Person t in persone_a_cui_inviare2)
                {
                    if (t.thread != null)
                    {
                       
                            t.thread.Abort();
                       
                    }

                }
                
                    Dispatcher.Invoke(new Action(() =>
                    {
                        scambio();
                        persone_a_cui_inviare.Clear();
                        persone_a_cui_inviare2.Clear();
                        Lista_file_2.Clear();
                        Lista_file.Clear();
                        lista_vuota_2.Visibility = Visibility.Visible;
                    }));


                }

            }


        }

        public void invia_file(Person s)
        {
            int flag = 100;
            string percorso_iniziale = "";
            FileStream file = null;
            IPEndPoint ipEnd_client;
            Socket clientSock_client = null;
            string nome_file = null;
            flag = 3;
            string file_stringa = null;
            List<File> listafile = Lista_file.ToList();
            File item1 = null;


            try
            {



                for (int j = s.numero_file_inviati; j < s.numero_file_da_inviare; j++)
                {
                    item1 = listafile[j];
                
                    file_stringa = Lista_file_2[j];
                    
                    
                    FileInfo f = new FileInfo(file_stringa);
                    string IpAddressString = s.Address;
                    ipEnd_client = new IPEndPoint(IPAddress.Parse(IpAddressString), 12001);
                    clientSock_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    clientSock_client.Connect(ipEnd_client);
                    string fileNameOnly = System.IO.Path.GetFileName(file_stringa);
                    byte[] put = Encoding.UTF8.GetBytes("PUT");
                    byte[] fileNameByte = Encoding.UTF8.GetBytes(fileNameOnly);
                    byte[] lunghezzanome =  BitConverter.GetBytes(fileNameByte.Length);
                    percorso_iniziale = "";
                    byte[] nomemio = Encoding.UTF8.GetBytes(progetto.Properties.Settings.Default.User);
                    byte[] lunghezzanomemio = null;
                    lunghezzanomemio = BitConverter.GetBytes(nomemio.Length);
                   
                   

                       
                       

                
                        nome_file= System.IO.Path.GetFileName(file_stringa);
                        clientSock_client.Send(put, 0, 3, 0);
                        clientSock_client.Send(lunghezzanome, 0, 4, 0);
                        clientSock_client.Send(fileNameByte, 0, fileNameByte.Length, 0);
                        clientSock_client.Send(lunghezzanomemio, 0, 4, 0);
                        clientSock_client.Send(nomemio, 0, nomemio.Length, 0);
                        byte[] pacchetto_rec = new byte[3];
                        clientSock_client.Receive(pacchetto_rec);
                        if (Encoding.ASCII.GetString(pacchetto_rec) == "+OK")
                        {

                        percorso_iniziale = file_stringa;
                            FileInfo tmp = new FileInfo(file_stringa);
                            byte[] peso_tradotto = BitConverter.GetBytes((long)tmp.Length);
                            long peso = (long)tmp.Length;
                            long inviati = 0;
                            clientSock_client.Send(peso_tradotto, 0, 8, 0);
                            // byte[] fileData = System.IO.File.ReadAllBytes(percorso_iniziale);
                            file = System.IO.File.OpenRead(percorso_iniziale);
                            byte[] pezzo = new byte[1024];
                            long byte_inviati = 0;
                            float peso_iniziale = peso;
                            List<Person> lista_tmp = PersonOne.ToList();
                            int finti = 0;
                            DateTime started = DateTime.Now;
                            TimeSpan rimanente1;
                            string rimanente = "30 seconds";
                            int contatore = 0;

                            while (peso > 0 && clientSock_client.Connected)
                            {
                                inviati = 0;

                                //  strSource.CopyTo ( 0, destination, 4, strSource.Length );
                                if (peso > 1024)
                                {
                                    finti += 1024;

                                    //Buffer.BlockCopy(src, 16, dest, 22, 5);
                                    //Buffer.BlockCopy(fileData, byte_inviati, pezzo, 0, 1024);

                                    var dataToSend = file.Read(pezzo, 0, pezzo.Length);
                                    var offset = 0;
                                    while (dataToSend > 0)
                                    {
                                        inviati = clientSock_client.Send(pezzo, offset, 1024, SocketFlags.None);
                                        peso -= (int)inviati;
                                        byte_inviati += inviati;
                                        dataToSend -= (int)inviati;
                                        offset += (int)byte_inviati;
                                        contatore++;
                                        TimeSpan elapsedTime = DateTime.Now - started;
                                        rimanente1 =
                                           TimeSpan.FromSeconds(
                                               (peso) /
                                               ((long)byte_inviati / elapsedTime.TotalSeconds));
                                        int hours = (int)rimanente1.Hours;
                                        int minute = (int)rimanente1.Minutes;
                                        int seconds = (int)rimanente1.Seconds;

                                        if (contatore > 30 || byte_inviati <= 1024)
                                        {
                                            if (hours > 0)
                                                rimanente = hours.ToString() + " hour " + minute.ToString() + " min";
                                            else if (minute > 0)
                                                rimanente = minute.ToString() + " minutes " + seconds.ToString() + " sec";
                                            else
                                                rimanente = seconds.ToString() + " seconds";
                                            contatore = 0;

                                        }

                                    }




                                    Dispatcher.Invoke(new Action(() =>
                                    {

                                        s.progressBarGrid.Visibility = Visibility.Visible;
                                        s.compressione.Visibility = Visibility.Visible;
                                        share_button.Content = "Stop All";
                                        share_button.Visibility= Visibility;
                                        s.da_inviare.Visibility = Visibility.Visible;
                                        if (f.Name.Length >= 10)
                                        {
                                            s.compressione.Text = f.Name.Substring(0, 10) + "..";
                                        }
                                        else
                                        {
                                            s.compressione.Text = f.Name + "..";
                                        }
                                        s.delete.Visibility = Visibility.Visible;
                                        s.Tempo.Visibility = Visibility.Visible;
                                        s.Tempo.Text = rimanente.ToString();
                                        s.Percentuale.Visibility = Visibility.Visible;
                                        s.Percentuale.Text = ((long)((byte_inviati / peso_iniziale) * 100)).ToString() + " %";
                                    }));
                                }
                                else
                                {

                                    var dataToSend = file.Read(pezzo, 0, (int)peso);
                                    var offset = 0;
                                    while (dataToSend > 0)
                                    {
                                        inviati = clientSock_client.Send(pezzo, offset, (int)peso, SocketFlags.None);
                                        peso -= (int)inviati;
                                        byte_inviati += inviati;
                                        dataToSend -= (int)inviati;
                                        offset += (int)byte_inviati;
                                        TimeSpan elapsedTime = DateTime.Now - started;
                                        rimanente1 =
                                           TimeSpan.FromSeconds(
                                               (peso) /
                                               ((long)byte_inviati / elapsedTime.TotalSeconds));
                                        int hours = (int)rimanente1.Hours;
                                        int minute = (int)rimanente1.Minutes;
                                        int seconds = (int)rimanente1.Seconds;
                                        if (hours > 0)
                                            rimanente = hours.ToString() + " hour " + minute.ToString() + " min";
                                        else if (minute > 0)
                                            rimanente = minute.ToString() + " minutes " + seconds.ToString() + " sec";
                                        else
                                            rimanente = seconds.ToString() + " seconds";

                                    }

                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                                    {

                                        s.progressBarGrid.Visibility = Visibility.Visible;
                                        if (f.Name.Length >= 5)
                                        {
                                            s.compressione.Text = "sending " + f.Name.Substring(0, 5) + "..";
                                        }
                                        else
                                        {
                                            s.compressione.Text = "sending " + f.Name + "..";
                                        }
                                        s.delete.Visibility = Visibility.Visible;
                                        s.da_inviare.Visibility = Visibility.Visible;
                                        s.Percentuale.Text = ((long)((byte_inviati / peso_iniziale) * 100)).ToString() + " %";
                                        s.Tempo.Visibility = Visibility.Visible;
                                        s.Tempo.Text = rimanente.ToString();
                                    }));


                                }



                            }


                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                            {

                                s.progressBarGrid.Visibility = Visibility.Visible;
                                if (f.Name.Length >= 5)
                                {
                                    s.compressione.Text = f.Name.Substring(0, 5) + " sent..";
                                }
                                else
                                {
                                    s.compressione.Text = f.Name + " sent..";

                                }
                                s.delete.Visibility = Visibility.Collapsed;
                                s.progressBar.Visibility = Visibility.Collapsed;
                                s.da_inviare.Visibility = Visibility.Visible;
                                // Lista_file.Remove(item);
                                s.Percentuale.Text = ((long)((byte_inviati / peso_iniziale) * 100)).ToString() + " %";

                            }));


                            s.numero_file_inviati++;
                            if (clientSock_client != null)
                            {
                                if (clientSock_client.Connected)
                                    clientSock_client.Close();
                            }
                            if (file != null)
                                file.Close();
                            IsAcquired = mut.WaitOne();
                             item1 = listafile[j];
                            var item = Lista_file.FirstOrDefault(x => x.Name == item1.Name);
                            if (item != null)
                            {
                                if (System.IO.File.Exists(item1.Percorso))
                                {
                                    if (item != null)
                                        item.contatore++;

                                    if (item.contatore >= persone_a_cui_inviare.Count())
                                    {
                                        

                                        Dispatcher.Invoke(new Action(() =>
                                        {

                                            Lista_file.Remove(item);
                                           
                                            if (Lista_file.Count() == 0)
                                            {
                                                Lista_file_2.Clear();
                                                lista_vuota_2.Visibility = Visibility.Visible;
                                                scambio();
                                            }
                                         
                                            
                                        }));
                                        if (s.da_rimuovere == true && s.numero_file_inviati >= s.numero_file_da_inviare)
                                        {
                                            var item2 = PersonOne.FirstOrDefault(x => x.Address == s.Address);
                                            Dispatcher.Invoke( new Action(() =>
                                        {                                           
                                                PersonOne.Remove(item2);
                                        
                                        }));
                                        }
                                    }

                                }
                                if (System.IO.Directory.Exists(item1.Percorso))
                                {
                                    if (item != null)
                                        item.contatore++;
                                    // mut.ReleaseMutex();
                                    if (item.contatore >= persone_a_cui_inviare.Count())
                                    {

                                        System.IO.File.Delete(file_stringa);
                                        item.zip = false;


                                        Dispatcher.Invoke( new Action(() =>
                                        {

                                            Lista_file.Remove(item1);
                                           

                                            if (Lista_file.Count() == 0)
                                            {
                                                Lista_file_2.Clear();
                                                lista_vuota_2.Visibility = Visibility.Visible;
                                                scambio();
                                            }
                                        
                                        }));
                                        if (s.da_rimuovere == true && s.numero_file_inviati >= s.numero_file_da_inviare)
                                        {
                                            var item2 = PersonOne.FirstOrDefault(x => x.Address == s.Address);
                                            Dispatcher.Invoke( new Action(() =>
                                        {

                                           
                                                PersonOne.Remove(item2);
                                           
                                        }));
                                        }

                                    }

                                }

                            }
                            IsAcquired = false;
                            mut.ReleaseMutex();






                        }
                        else
                        {
                            System.Windows.MessageBox.Show(s.Name + " refuse file");
                            if (System.IO.File.Exists(percorso_iniziale))
                            {
                                IsAcquired = mut.WaitOne();
                                var item = Lista_file.FirstOrDefault(x => x.Name == nome_file);
                                //  item.contatore--;
                                if (item.contatore == totalepersone)
                                {
                                    System.IO.File.Delete(percorso_iniziale);
                                    item.zip = false;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
                                    {

                                        Lista_file.Remove(item);
                                      
                                        if (Lista_file.Count() == 0)
                                        {
                                            lista_vuota_2.Visibility = Visibility.Visible;
                                            Lista_file_2.Clear();
                                            scambio();
                                        }

                                    }));
                                }
                                IsAcquired = false;
                                mut.ReleaseMutex();
                            }
                            IsAcquired = mut.WaitOne();

                            if (totalepersone == 0)
                            {
                                persone_a_cui_inviare.Remove(s.Address);
                                persone_a_cui_inviare2.Remove(s);
                            }
                            IsAcquired = false;
                            mut.ReleaseMutex();


                        }   
                if (s.numero_file_inviati >= listafile.Count())
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        s.da_inviare.Visibility = Visibility.Collapsed;
                        persone_a_cui_inviare2.Remove(s);
                        persone_a_cui_inviare.Remove(s.Address);
                    }));
                }





                }


              

                IsAcquired = mut.WaitOne();
                numero_thread--;
                IsAcquired = false;
                mut.ReleaseMutex();

            }
            catch (Exception)
            {
                s.numero_file_inviati++;
                if (clientSock_client != null)
                {
                    if (clientSock_client.Connected)
                        clientSock_client.Close();
                }
                if (file != null)
                    file.Close();
                IsAcquired = mut.WaitOne();
              

                    item1 = listafile[s.numero_file_inviati - 1];
                
                var item = Lista_file.FirstOrDefault(x => x.Name == item1.Name);
                if (item != null)
                {
                    if (System.IO.File.Exists(item1.Percorso))
                    {
                        if (item != null)
                            item.contatore++;

                        if (item.contatore >= persone_a_cui_inviare.Count())
                        {


                            Dispatcher.Invoke(new Action(() =>
                            {

                                Lista_file.Remove(item);
                               
                                if (Lista_file.Count() == 0)
                                {
                                    lista_vuota_2.Visibility = Visibility.Visible;
                                    Lista_file_2.Clear();
                                    scambio();
                                }


                            }));
                            if (s.da_rimuovere == true && s.numero_file_inviati >= s.numero_file_da_inviare)
                            {
                                var item2 = PersonOne.FirstOrDefault(x => x.Address == s.Address);
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    PersonOne.Remove(item2);

                                }));
                            }
                        }

                    }
                    if (System.IO.Directory.Exists(item1.Percorso))
                    {
                        if (item != null)
                            item.contatore++;
                        // mut.ReleaseMutex();
                        if (item.contatore >= persone_a_cui_inviare.Count())
                        {
                            if (Lista_file_2.Count() > 0)
                            {
                                file_stringa = Lista_file_2[s.numero_file_inviati - 1];
                                System.IO.File.Delete(file_stringa);
                                item.zip = false;
                            }


                            Dispatcher.Invoke(new Action(() =>
                            {

                                Lista_file.Remove(item1);
                               

                                if (Lista_file.Count() == 0)
                                {
                                    lista_vuota_2.Visibility = Visibility.Visible;
                                    Lista_file_2.Clear();
                                    scambio();
                                }

                            }));
                            if (s.da_rimuovere == true && s.numero_file_inviati >= s.numero_file_da_inviare)
                            {
                                var item2 = PersonOne.FirstOrDefault(x => x.Address == s.Address);
                                Dispatcher.Invoke(new Action(() =>
                                {


                                    PersonOne.Remove(item2);

                                }));
                            }
                            
                        }

                    }
                    if (s.numero_file_inviati >= listafile.Count())
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            s.da_inviare.Visibility = Visibility.Collapsed;
                            persone_a_cui_inviare2.Remove(s);
                            persone_a_cui_inviare.Remove(s.Address);
                        }));
                    }

                }
                IsAcquired = false;
                if (item != null)
                    item.contatore++;
                mut.ReleaseMutex();


            }


        }


        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            m.Hide();
          //  m.Visibility = Visibility.Collapsed;
           
        }
        private void setting_metod(object sender, RoutedEventArgs e)
        {
            if (f == null)
            {
                // System.Windows.MessageBox.Show(" creo profilo");
                f = new Window2();
                f.Show();
            }
            else
            {
                // System.Windows.MessageBox.Show(" rettifico profilo");
                f.Activate();
                f.Show();
            }




        }
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = (StackPanel)sender;
            bool da_inserire = false;
            Person person = new Person();
            foreach (UIElement i in sp.Children)
            {

                if (i.Uid.Equals("Nome"))
                    person.Name = ((TextBlock)i).Text;
                if (i.Uid.Equals("Tempo"))
                    person.Tempo = ((TextBlock)i);

                else if (i.Uid.Equals("Address"))
                    person.Address = ((TextBlock)i).Text;
                else if (i.Uid.Equals("Immagine"))
                {
                    Grid g = (Grid)i;
                    foreach (UIElement j in g.Children)
                    {
                        if (j.Uid.Equals("Immagine_v"))
                        {
                            if (j.Visibility == Visibility.Collapsed)
                            {
                                j.Visibility = Visibility.Visible;
                                da_inserire = true;
                                person.preso_in_carico = true;
                                person.da_inviare = (Ellipse)j;

                            }

                            else
                            {
                                   j.Visibility = Visibility.Collapsed;
                                   da_inserire = false;
                                   person.preso_in_carico = false;
                                   person.da_inviare = (Ellipse)j;                              


                            }
                            person.da_inviare = (Ellipse)j;
                            /// person.v = (Ellipse)j;
                        }

                    }
                }
                else if (i.Uid.Equals("ProgressBarGrid"))
                {

                    Grid g = (Grid)i;
                    person.progressBarGrid = g;
                    foreach (UIElement j in g.Children)
                    {
                        if (j.Uid.Equals("ProgressBar"))
                        {
                            person.progressBar = (System.Windows.Controls.ProgressBar)j;

                        }
                        if (j.Uid.Equals("percentuale"))
                        {
                            person.Percentuale = ((TextBlock)j);

                        }
                        if (j.Uid.Equals("compressione"))
                        {
                            person.compressione = ((TextBlock)j);
                        }
                        if (j.Uid.Equals("delete"))
                        {
                            person.delete = ((System.Windows.Controls.Button)j);
                        }


                    }



                }



            }
            if (da_inserire == true)
            {
                List<Person> lista_tmp = PersonOne.ToList();
               
                if (!persone_a_cui_inviare.Contains(person.Address))
                {

                    persone_a_cui_inviare.Add(person.Address);
                    persone_a_cui_inviare2.Add(person);



                }
            }
            else
            {
                var item = persone_a_cui_inviare2.FirstOrDefault(x => x.Address == person.Address);
                if (item!=null && item.thread==null)
                {
                 
                        
                    persone_a_cui_inviare.Remove(person.Address);
                        persone_a_cui_inviare2.Remove(person);
                   
                }

            }


        }
        private void Restore_profilo(object sender, System.EventArgs e)
        {
            if (f != null)
            {
                // System.Windows.MessageBox.Show(" rettifico profilo");
                f.Activate();
                f.Show();
            }
            else
            {
                f = new Window2();
                f.Show();
            }

        }
        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            nIcon.Dispose();
           
            if (workerthread2 != null)
            {
                workerthread2.Abort();
            }

            //RemoveOption_ContextMenu();

            for (int intCounter = App.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
            {
                App.Current.Windows[intCounter].Close();
            }
            if (f != null)
            {
                f.Close();
            }
            App.app.workerThread.Abort();
            nIcon.Dispose();
            
            Environment.Exit(0);

        }
        private void RemoveOption_ContextMenu()
        {
            RegistryKey _key = Registry.ClassesRoot.OpenSubKey("*\\shell", true);
            RegistryKey _key1 = Registry.ClassesRoot.OpenSubKey("Folder\\shell", true);
            _key.DeleteSubKeyTree("Send to your friends");
            _key1.DeleteSubKeyTree("Send to your friends");
            _key1.Close();
            _key.Close();
        }
        private void notifica()
        {
            nIcon = new NotifyIcon
            {
                Icon = new Icon("icon.ico"),
                Visible = true
            };
            System.Windows.Forms.MenuItem[] menuList = new System.Windows.Forms.MenuItem[] { new System.Windows.Forms.MenuItem("Open"), new System.Windows.Forms.MenuItem("Exit"), new System.Windows.Forms.MenuItem("Profilo") };
            System.Windows.Forms.ContextMenu clickMenu = new System.Windows.Forms.ContextMenu(menuList);
            nIcon.ContextMenu = clickMenu;
            menuList[1].Click += new EventHandler(CloseMenuItem_Click);
            menuList[0].Click += new EventHandler(Restore_size);
            menuList[2].Click += new EventHandler(Restore_profilo);
            nIcon.Text = "Send to your friends";



        }
        private void Restore_size(object sender, System.EventArgs e)
        {

            m.Activate();
            m.WindowState = WindowState.Normal;
            //m = new MainWindow();
            m.Show();
       
        }
        private void AddOption_ContextMenu()
        {
            RegistryKey _key1 = Registry.ClassesRoot.OpenSubKey("Folder\\shell", true);
            RegistryKey _key = Registry.ClassesRoot.OpenSubKey("*\\shell", true);
            RegistryKey newkey = _key.CreateSubKey("Send to your friends");
            RegistryKey newkey1 = _key1.CreateSubKey("Send to your friends");
            RegistryKey command = newkey.CreateSubKey("Command");
            RegistryKey command1 = newkey1.CreateSubKey("Command");

            string program = System.Windows.Application.ResourceAssembly.Location + " %1";

            command.SetValue("", program);
            command1.SetValue("", program);
            newkey.SetValue("DefaultIcon", System.IO.Path.GetDirectoryName(System.Windows.Application.ResourceAssembly.Location) + "\\icon.ico");

            command.Close();
            command1.Close();

            newkey1.Close();
            newkey.Close();
            _key.Close();
        }
        public void StartServer()
        {
            Task.Factory.StartNew(() =>
            {
                PipeSecurity ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().Name, PipeAccessRights.FullControl, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule("SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
                NamedPipeServerStream server = new NamedPipeServerStream("PipesOfPiece", PipeDirection.InOut, 10,
                                    PipeTransmissionMode.Message, PipeOptions.WriteThrough, 1024, 1024, ps);
                String separatore = "\\";
                char[] separatore2 = separatore.ToCharArray();
                int indice = 0;
                string nomefile;
                string[] vettore;
                string[] parole;


                try
                {
                    while (true)
                    {

                        
                        server.WaitForConnection();

                        StreamReader reader = new StreamReader(server);
                        var line = reader.ReadLine();
                        String[] liste = { ".txt", ".pdf", ".jpeg", ".ppt", ".mp4", "cartella", ".doc", ".png" };
                        if (line != null)
                        {
                            File nuovo = new progetto.File();
                            nuovo.Percorso = line;
                            nuovo.Name = System.IO.Path.GetFileName(line);
                            parole = line.ToString().Split(separatore2);
                            indice = parole.Length;
                            nomefile = parole[indice - 1];
                            nuovo.Name = nomefile;
                            string extension = System.IO.Path.GetExtension(nuovo.Name);
                            if (liste.Contains((extension.ToLower())))
                            {

                                vettore = extension.Split('.');
                                extension = vettore[1] + ".png";
                            }
                            else
                            {
                                extension = "CARTELLA.png";
                            }

                            List<File> lista = Lista_file.ToList();
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() => {

                                var Uri = new Uri(extension, UriKind.Relative);
                                bool trovato = false;
                                nuovo.immagine = BitmapFrame.Create(App.app.getBitmap(Uri));
                                // nuovo.immagine = GetImageFormat(nuovo.Name);
                                foreach (File s in lista)
                                {
                                    if (s.Name == nuovo.Name)
                                    {
                                        trovato = true;
                                    }
                                }

                                if (trovato == false)
                                {
                                    nuovo.contatore = 0;
                                    nuovo.zip = false;
                                    bool trovato24 = false;
                                    foreach(Person c in persone_a_cui_inviare2)
                                    {
                                   if(c.thread != null)
                                        {
                                            trovato24 = true;
                                        }
                                    }
                                    if (trovato24 == false)
                                    {
                                        Lista_file.Add(nuovo);
                                        lista_vuota_2.Visibility = Visibility.Collapsed;

                                    }
                                    else
                                    {
                                        System.Windows.MessageBox.Show("Before you add others files, wait for all files to be sent");
                                    }


                                }
                            }));
                            line = null;
                        }



                        server.WaitForPipeDrain();
                        if (server.IsConnected) { server.Disconnect(); }


                    }
                }
                catch (Exception)
                {
                    if (server.IsConnected) { server.Disconnect(); }
                }
            });
        }
        private void ButtonsDemoChip_OnDeleteClick(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.Chip b = (MaterialDesignThemes.Wpf.Chip)sender;
            List<File> lista = Lista_file.ToList();
            bool trovato = false;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                foreach (File i in lista)
                {
                    if (b.Content.Equals(i.Name))
                    {
                        foreach(Person p in persone_a_cui_inviare2)
                        {
                            if (p.thread != null)
                            {
                                trovato = true;
                            }
                        }
                        if (trovato == true)
                        {
                            System.Windows.MessageBox.Show("you can't remove this file , when sending is in progress");
                        }
                        else
                        {
                            Lista_file.Remove(i);
                            if (Lista_file.Count() == 0)
                                lista_vuota_2.Visibility = Visibility.Visible;
                        }

                    }
                }

            }));

        }
        private void delete_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Controls.Button sp = (System.Windows.Controls.Button)sender;
            var viewModel = (Person)sp.DataContext;
            List<Person> lista = persone_a_cui_inviare2.ToList();
            foreach (Person i in lista)
            {
                if (viewModel.Address == i.Address)
                {

                   
                        var item = persone_a_cui_inviare2.FirstOrDefault(x => x.Address == i.Address);
                    if (item.thread != null)
                    {
                        item.thread.Abort();
                        if (item.numero_file_da_inviare != item.numero_file_inviati)
                        {

                            totalepersone = persone_a_cui_inviare2.Count();
                            item.thread = new Thread(() => invia_file(item));
                            //item.numero_file_inviati++;
                            item.thread.Start();

                        }
                    }
                    else
                    {
                        //item.numero_file_inviati++;
                        if (item.numero_file_da_inviare != item.numero_file_inviati)
                        {
                            totalepersone = persone_a_cui_inviare2.Count();
                            item.thread = new Thread(() => invia_file(item));
                            //item.numero_file_inviati++;
                            item.thread.Start();

                        }
                    }
                   
                   
                }
            }








        }
        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {


            if (Properties.Settings.Default.online == false)
            {
                Properties.Settings.Default.online = true;
                Properties.Settings.Default.online_string = "online";
                Properties.Settings.Default.Save();
                App.app.avviainvio();
            }
            else if (Properties.Settings.Default.online == true)
            {
                Properties.Settings.Default.online = false;
                Properties.Settings.Default.online_string = "offline";

                Properties.Settings.Default.Save();
                App.app.stop_workerthread();
            }
            if (progetto.Window2.w2 != null)
            {
                progetto.Window2.w2.color();
            }

        }
        void Notify()
        {
            lock (syncPrimitive)
            {
                Monitor.PulseAll(syncPrimitive);
            }
        }
        void RunLoop()
        {
            lock (syncPrimitive)
            {
                for (; ; )
                {
                    // do work here...
                    Monitor.Wait(syncPrimitive);
                }
            }
        }

       private void zippa_tutto( CancellationTokenSource token)
        {
            string current = Directory.GetCurrentDirectory();
            string current_zip = current + "\\tmp";
            if(Directory.Exists(current_zip))
            Directory.Delete(current_zip,true);
            DirectoryInfo di = Directory.CreateDirectory(current_zip);
            try
            {
                foreach (Person s in persone_a_cui_inviare2) {
                    Dispatcher.Invoke(new Action(() =>
                    {

                        s.progressBarGrid.Visibility = Visibility.Visible;
                        s.progressBar.Visibility = Visibility.Visible;
                        s.delete.Visibility = Visibility.Hidden;
                        s.da_inviare.Visibility = Visibility.Visible;
                        s.Percentuale.Visibility = Visibility.Collapsed;
                        s.Tempo.Visibility = Visibility.Collapsed;
                        s.compressione.Text = "compression files..";
                        s.compressione.Visibility = Visibility.Visible;
                        s.Tempo.Visibility = Visibility.Collapsed;




                    }));
                }

                foreach (File f in Lista_file.ToList())
                {
                    if (System.IO.Directory.Exists(f.Percorso) && !token.IsCancellationRequested)
                    {
                        string fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(f.Percorso);
                        string extension = System.IO.Path.GetExtension(f.Percorso);
                        string path = System.IO.Path.GetDirectoryName(f.Percorso);
                        string percorso_iniziale = current_zip + "\\" + fileNameOnly + ".zip";
                        if (System.IO.File.Exists(f.Percorso))
                        {
                            int count = 1;
                            fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(percorso_iniziale);
                            extension = System.IO.Path.GetExtension(percorso_iniziale);
                            path = System.IO.Path.GetDirectoryName(percorso_iniziale);
                            string newFullPath = percorso_iniziale;
                            while ((System.IO.File.Exists(newFullPath)))
                            {
                                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                newFullPath = System.IO.Path.Combine(path, tempFileName + extension);
                            }
                            percorso_iniziale = newFullPath;
                        }
                        token.Token.ThrowIfCancellationRequested();
                        ZipFile.CreateFromDirectory(f.Percorso, percorso_iniziale, CompressionLevel.NoCompression, true);
                        Lista_file_2.Add(percorso_iniziale);

                    }
                    else
                    {
                        Lista_file_2.Add(f.Percorso);

                    }
                }
                IsAcquired= mut.WaitOne();
                zip_pronti = true;
                IsAcquired = false;
                mut.ReleaseMutex();
                Notify();
            }
            catch
            {
                foreach(string f in Lista_file_2)
                {
                    string fileNameOnly = System.IO.Path.GetFileNameWithoutExtension(f);
                    string extension = System.IO.Path.GetExtension(f);
                    if (extension == "zip")
                    {
                        System.IO.File.Delete(f);
                    }
                }
                Lista_file_2.Clear();
            }
        }

    }


}

