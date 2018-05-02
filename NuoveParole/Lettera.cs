using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NuoveParole
{
    class Lettere : ObservableCollection<Lettera>
    { }

    class Lettera
    {
        public char carattereSelezionato { get; set; }
        public char carattereSeguente { get; set; }
        public int conteggio { get; set; }
    }
}
