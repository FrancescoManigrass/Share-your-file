using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace progetto
{
   public  class File
    {
      

        public string Name { get; set; }
        public string Percorso { get; set; }
        public float peso { get; set; }
        public BitmapSource immagine { get; set; }
        public int contatore { get; set; }
        public bool zip { get; set; }

    }
}
