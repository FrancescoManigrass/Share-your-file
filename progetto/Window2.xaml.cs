using System.Windows;
using System.Windows.Input;
using Windows.Storage;
using Windows.Foundation.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Windows.Media.Import;
using System.Windows.Automation;
using System.Threading;

using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;
using System.Drawing;

namespace progetto
{
    /// <summary>
    /// Logica di interazione per Window2.xaml
    /// </summary>
    /// 
   
    public partial class Window2 : Window
    {
       
        public StorageFolder SaveFolder { get; }
        public Thread nuovo { get; set; }
        Boolean flagfotocamera = false;
        public List<string> Lista_file = new List<string>();
        public int numero_foto_iniziali = 0;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        internal static Window2 w2;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);




        public Window2()
        {
            InitializeComponent();
            w2 = this;
            color();
           


        }

        public void color()
        {
            System.Windows.Controls.TextBlock text1 = (System.Windows.Controls.TextBlock)progetto.Window2.w2.FindName("online_text");
            if (Properties.Settings.Default.online == true)
            {
                text1.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                text1.Foreground = System.Windows.Media.Brushes.Red;
            }
        }





        private void Border_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            

        
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
            System.Windows.Controls.TextBox text1 = (System.Windows.Controls.TextBox)this.FindName("txt_username");
            System.Windows.Controls.TextBox text2 = (System.Windows.Controls.TextBox)this.FindName("txt_username2");
            System.Windows.Controls.Button tb2 = (System.Windows.Controls.Button)this.FindName("immagine_txt");
            System.Windows.Controls.Button tb3 = (System.Windows.Controls.Button)this.FindName("immagine2_txt");



            if (tb2.IsVisible)
            {
               
                text1.Visibility = System.Windows.Visibility.Hidden;
                text2.Visibility = System.Windows.Visibility.Visible;
                tb2.Visibility= System.Windows.Visibility.Hidden;
                tb3.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {
                
         
                if (text2.Text!="")
                {
                    Properties.Settings.Default.User = text2.Text;
                    Properties.Settings.Default.Save();
                    // this.Close();
                    //Window2 t = new Window2();
                    //t.Show();
                    tb2.Visibility = System.Windows.Visibility.Visible;
                    tb3.Visibility = System.Windows.Visibility.Hidden;
                    text2.Visibility = System.Windows.Visibility.Hidden;
                    text1.Visibility = System.Windows.Visibility.Visible;


                }
                else
                {
                    System.Windows.MessageBox.Show("write your name, please");
                    tb2.Visibility = System.Windows.Visibility.Hidden;
                    tb3.Visibility = System.Windows.Visibility.Visible;

                }
            }

    
        }



        public void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (System.Windows.Controls.Primitives.ToggleButton)sender;
            
            if (button.IsChecked.Value)
            {
                Properties.Settings.Default.online = true;
          
              
                Properties.Settings.Default.Save();
                // ((MainWindow)System.Windows.Application.Current.MainWindow).avviainvio();
                // MainWindow.Main.avviainvio();
                App.app.avviainvio();









            }
            else if (!button.IsChecked.Value)
            {
                Properties.Settings.Default.online = false;
                Properties.Settings.Default.Save();
              
      
                App.app.stop_workerthread();






            }

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.download = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
          
                 

        
            MainWindow.main.f = null;
            if (nuovo != null)
            {
                nuovo.Abort();
                flagfotocamera = false;
            }



        }




        public void OnFocusChangedHandler(object src, AutomationFocusChangedEventArgs args)
        {
           // System.Windows.MessageBox.Show("Focus changed!");
            AutomationElement element = src as AutomationElement;

            
                if (element != null)
                {
                    string name = element.Current.Name;
                    string id = element.Current.AutomationId;
                    int processId = element.Current.ProcessId;
                    using (Process process = Process.GetProcessById(processId))
                    {
                        // Console.WriteLine("  Name: {0}, Id: {1}, Process: {2}", name, id, process.ProcessName);
                        System.Windows.MessageBox.Show(name.ToString());
                    }
                }
            
        }
        
        private void Request_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (System.Windows.Controls.Primitives.ToggleButton)sender;
            if (button.IsChecked.Value)
            {
                Properties.Settings.Default.all_file = true;
                Properties.Settings.Default.Save();
               

            }
            else if (!button.IsChecked.Value)
            {
                Properties.Settings.Default.all_file = false;
                Properties.Settings.Default.Save();
            }
        }

        private void ToggleButton_Click_1(object sender, RoutedEventArgs e)
        {

            ToggleButton button = (System.Windows.Controls.Primitives.ToggleButton)sender;
            if (button.IsChecked.Value)
            {
                Properties.Settings.Default.new_folder = true;
                Properties.Settings.Default.Save();
           
        

            }
            else if (!button.IsChecked.Value)
            {
                Properties.Settings.Default.new_folder = false;
                Properties.Settings.Default.Save();
            }

        }

    

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // processo della fotocamera
            Process ciao = System.Diagnostics.Process.Start("microsoft.windows.camera:");
            if (nuovo == null)
            {
                try
                {
                    var picturesLibrary2 = Windows.Storage.KnownFolders.CameraRoll;
                    string[] files = Directory.GetFiles(picturesLibrary2.Path);
                    // var files = picturesLibrary2.GetFilesAsync().GetResults();

                    Lista_file.Clear();


                    Lista_file.AddRange(files);

                    /*foreach (var file in files)
                    {
                        // do something with the music files
                        Lista_file.Add(file);
                    }*/

                    flagfotocamera = true;



                    nuovo = new Thread(() => parti());
                    nuovo.Start();
                }
                catch
                {

                }

            }
        }
        public bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        private void parti()
        {
            while (true)
            {
                while (!ApplicationIsActivated()) ;
                // app non in focus
                var picturesLibrary2 = Windows.Storage.KnownFolders.CameraRoll;
                string[] files2 = Directory.GetFiles(picturesLibrary2.Path);
                if (files2.Count() > Lista_file.Count() && flagfotocamera == true)
                {
                    List<string> lista2 = new List<string>();
                    lista2.AddRange(files2);
                    List<string> not_common = lista2.Except(Lista_file).ToList();
                    if (not_common.Count() != 0)
                    {
                        string max = string.Empty;
                        //var item = not_common.FirstOrDefault(x => x.DateCreated >= i.DateCreated);
                        not_common = not_common.OrderBy(i => i).ToList();
                        // var item = not_common.FirstOrDefault(x => x.DateCreated == max);
                        string extension = "ciao";
                        string item = null;
                        while (extension != ".jpg")
                        {
                            item = not_common.Last();
                            extension = System.IO.Path.GetExtension(item);
                            if (extension == ".jpg")
                                break;
                            not_common.Remove(item);
                        }


                        string filepath = item;
                        Properties.Settings.Default.Foto = item;
                        Properties.Settings.Default.modificafoto = "si";
                        Properties.Settings.Default.Save();
                        Lista_file.AddRange(not_common);
                        // Process.GetProcessesByName("WindowsCamera.exe")[0].CloseMainWindow();
                    }
                }


            }
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Image Files(*.BMP; *.JPG; *.GIF; *.PNG;)| *.BMP; *.JPG; *.GIF; *.PNG;";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                /*  string filepath = ofd.FileName;
                  String cartella = System.IO.Directory.GetCurrentDirectory();
                  cartella = cartella.Replace("\\bin\\Debug", "");
                  cartella = cartella + "\\immagini";
                  String finale = cartella+"\\" + ofd.SafeFileName;
                  File.Copy(cartella, filepath,true);
                  Properties.Settings.Default.Foto =finale;
                  Properties.Settings.Default.Save();*/

                string filepath = ofd.FileName;
                Properties.Settings.Default.Foto = ofd.FileName;
                Properties.Settings.Default.modificafoto = "si";
                Properties.Settings.Default.Save();
                this.Close();


            }
        }

        private void yurself_Click(object sender, RoutedEventArgs e)
        {

            ToggleButton button = (System.Windows.Controls.Primitives.ToggleButton)sender;

            if (button.IsChecked.Value)
            {
                Properties.Settings.Default.yourself = true;
                Properties.Settings.Default.Save();
            }
            else if (!button.IsChecked.Value)
            {
                Properties.Settings.Default.yourself = false;               
                Properties.Settings.Default.Save();
                string myip = progetto.MainWindow.main.GetLocalIPAddress();
                var item = progetto.MainWindow.main.PersonOne.FirstOrDefault(x => x.Address == myip );
                progetto.MainWindow.main.PersonOne.Remove(item);
                
              
               





            }

        }
    }
}
