using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


public class Person
{

        public string Name { get; set; }
        public string Address { get; set; }
        public BitmapSource foto { get; set; }
        public  bool isCheckedState { get; set; }
        public bool v{ get; set; }
        public bool da_rimuovere { get; set; }
    public Ellipse da_inviare { get; set; }
        public Thread thread { get; set; }
        public  int numero_file_da_inviare { get; set; }
        public  int numero_file_inviati { get; set; }
        public TextBlock Tempo { get; set; }
        public string modificafoto { get; set; }
        public Grid progressBarGrid { get; set; }
        public Ellipse Immagine_v { get; set; }
        public BitmapSource Immagine { get; set; }
        public ProgressBar progressBar { get; set; }
        public TextBlock Percentuale { get; set; }
        public TextBlock compressione { get; set; }
        public bool preso_in_carico { get; set; }
        public Button delete { get; set; }
       



}
