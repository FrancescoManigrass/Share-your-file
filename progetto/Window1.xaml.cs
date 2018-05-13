

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.IO.Pipes;


// https://stackoverflow.com/questions/10182751/server-client-send-receive-simple-text


namespace progetto
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>


    public partial class Window1 : Window
    {

        //  public Person PersonOne { get; set; }
        public ObservableCollection<Person> PersonOne { get; set; }
        public ObservableCollection<File> Lista_file { get; set; }
        public ObservableCollection<string> persone_a_cui_inviare { get; set; }
        public string Path { get; set; }
        public Thread workerthread2 = null;


        //Thread workerThread = App.app.workerThread;
        //Thread workerthread2 = App.app.workerthr
        Thread client;

        public Window1()
        {
            string[] args = Environment.GetCommandLineArgs();
            // dichiaro un client che esegue una funzione che invia il nome del file( argomento !=0)
            client = new Thread(() => Client(args));
            // per far partire il thread faccio lo start
            client.Start();
         
            client.Join();
            Environment.Exit(0);

        }


        public void Client(string[] args)
        {

            try {

                // var client = new NamedPipeClientStream("PipesOfPiece");   
                var client = new NamedPipeClientStream(".", "PipesOfPiece", PipeAccessRights.FullControl,
         PipeOptions.WriteThrough, System.Security.Principal.TokenImpersonationLevel.None, HandleInheritability.None);
                client.Connect();
              /*  if (!client.IsConnected)
                {
                    System.Windows.MessageBox.Show("ciao");
                }
                */

                StreamWriter writer = new StreamWriter(client);
                int indice = 0;
          

                foreach (var s in args)
                {
                    if (indice != 0)
                    {
                        string input = s;
                        writer.WriteLine(input);
                        writer.Flush();
                    }
                    indice++;
                }
                client.Close();
                client.Dispose();
            }
            catch (Exception )
            {
                System.Windows.MessageBox.Show("eccezione window1");
               
            }
        }
    }

}


    

