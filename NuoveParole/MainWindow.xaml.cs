using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NuoveParole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string filenameTxt;
        Dictionary<string, int> dizionario = new Dictionary<string, int>();

        private void btDizionarioAnalizza_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".txt";
            dlg.Filter = "TXT Files (*.txt)|*.txt";
            dlg.Multiselect = true;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                filenameTxt = dlg.FileName;
            }
            else
            {
                return;
            }

            foreach (String filenameTxt in dlg.FileNames) 
            {
                string linea;

                System.IO.StreamReader file = new System.IO.StreamReader(filenameTxt);
                while ((linea = file.ReadLine()) != null)
                {
                    var punteggiatura = linea.Where(Char.IsPunctuation).Distinct().ToArray();
                    var parole = linea.Split().Select(x => x.Trim(punteggiatura));

                    string[] caratteriPermessi = new[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "é", "è",
                                                     "a", "s", "d", "f", "g", "h", "j", "k", "l", "à", "ù",
                                                     "z", "x", "c", "v", "b", "n", "m", "ì"
                                                   };

                    string[] caratteriNonPermessi = new[] { "\\", "!", "|", "-", "_", ",", ".", ";", ":", "§", "°", "#",
                                                        "'", "£", "$", "%", "&", "/", "(", ")", "=", "?", "^",
                                                        "[", "]", "+", "*", "’"
                                                   };

                    foreach (string parola in parole)
                    {
                        string par = parola.ToLower();

                        if (!par.Equals(string.Empty) && caratteriPermessi.Any(par.Contains) && !caratteriNonPermessi.Any(par.Contains) && !par.Any(char.IsDigit) && !par.Any(char.IsSymbol))
                        {
                            if (dizionario.ContainsKey(par))
                            {
                                // cerco il valore del conteggio della parola
                                // e lo incremento di uno
                                dizionario[par] += 1;
                            }
                            else
                            { dizionario.Add(par, 1); }
                        }
                    }

                }
                file.Close();
            }

            MessageBox.Show("Ordino il dizionario");

            var dizionarioOrdinato = from p in dizionario
                                     orderby p.Key ascending
                                     select p;

            lbParole.Items.Clear();

            foreach (var p in dizionarioOrdinato)
            {
                lbParole.Items.Add(p.Key + " : " + p.Value);
            }

            MessageBox.Show("Inserimento tutti file completato");

                      
        }

        private void btAggiungiDB2_Click(object sender, RoutedEventArgs e)
        {
            var DS = new dizionarioDataSet();
            var tbDiz = new dizionarioDataSetTableAdapters.dizionarioTableAdapter();

            var dizionarioOrdinato = from p in dizionario
                                     orderby p.Key ascending
                                     select p;

            foreach (var p in dizionarioOrdinato)
            {
                tbDiz.InsertQuery(p.Key, p.Value);
            }

            MessageBox.Show("OK");
        }

        Dictionary<char, int> conteggio = new Dictionary<char, int>();

        private void btAnalizzaLettere_Click(object sender, RoutedEventArgs e)
        {
            //CARICA DIZIONARIO DA DATABASE

            var DS = new dizionarioDataSet();
            var tbDiz = new dizionarioDataSetTableAdapters.dizionarioTableAdapter();

            tbDiz.Fill(DS.dizionario);
            lbParole.Items.Clear();

            List<string> diz = new List<string>();

            foreach (DataRow parola in DS.Tables["dizionario"].Rows)
            {
                diz.Add(parola["parola"].ToString());
                lbParole.Items.Add(parola["parola"].ToString());
            }

            string sequenzaConVuoto = "aàbcdeèéfghiìjklmnoòpqrstuùvwxyz0";

            Lettere lettere = new Lettere();

            int pos = 0;
            AggiungiConteggio();

            using (StreamWriter sw = new StreamWriter("Analisi.csv"))
            {
                string intestazione = String.Format("{0,7}", "Lettera");
                foreach (char temp in sequenzaConVuoto)
                {
                    intestazione += String.Format(", {0,7}", temp);
                }
                sw.WriteLine(intestazione);

                foreach (char c in sequenzaConVuoto)
                {
                    for (int parola_x = 0; parola_x <= diz.Count - 1; parola_x++)
                    {
                        pos = 0;
                        
                        if (c == '0')
                        {
                            // calcolo il conteggio della prima lettera parola
                            if (conteggio.ContainsKey(diz[parola_x].Substring(0, 1)[0]))
                            {
                                conteggio[diz[parola_x].Substring(0, 1)[0]] += 1;
                            }    
                        }
                        else
                        {
                            // se lettera normale calcolo con questa procedura
                            while ((pos = diz[parola_x].IndexOf(c, pos)) != -1)
                            {
                                if (diz[parola_x].IndexOf(c, pos) != -1)
                                {
                                    pos = diz[parola_x].IndexOf(c, pos);
                                    if (pos == diz[parola_x].Length - 1)
                                    {
                                        conteggio['0'] += 1;
                                    }
                                    else
                                    {
                                        if (conteggio.ContainsKey(diz[parola_x].Substring(pos + 1, 1)[0]))
                                            conteggio[diz[parola_x].Substring(pos + 1, 1)[0]] += 1;
                                    }
                                }
                                pos += 1;
                            }
                        }

                    }

                    // Salva i conteggi sulla Lettere
                    string s = String.Format("{0,7}", c);
                    foreach (char temp in sequenzaConVuoto)
                    {
                        lettere.Add(new Lettera { carattereSelezionato = c, carattereSeguente = temp, conteggio = conteggio[temp] });
                        // SCRIVO IL FILE CON IL CONTEGGIO TOTALE
                        s += String.Format(", {0,7}", conteggio[temp]);    
                    }
                    sw.WriteLine(s);

                    AzzeraConteggio();
                }
            }

            
            

            // FINE ANALISI
        }


        public void AggiungiConteggio()
        {
            conteggio.Add('a', 0);
            conteggio.Add('à', 0);
            conteggio.Add('b', 0);
            conteggio.Add('c', 0);
            conteggio.Add('d', 0);
            conteggio.Add('e', 0);
            conteggio.Add('è', 0);
            conteggio.Add('é', 0);
            conteggio.Add('f', 0);
            conteggio.Add('g', 0);
            conteggio.Add('h', 0);
            conteggio.Add('i', 0);
            conteggio.Add('ì', 0);
            conteggio.Add('j', 0);
            conteggio.Add('k', 0);
            conteggio.Add('l', 0);
            conteggio.Add('m', 0);
            conteggio.Add('n', 0);
            conteggio.Add('o', 0);
            conteggio.Add('ò', 0);
            conteggio.Add('p', 0);
            conteggio.Add('q', 0);
            conteggio.Add('r', 0);
            conteggio.Add('s', 0);
            conteggio.Add('t', 0);
            conteggio.Add('u', 0);
            conteggio.Add('ù', 0);
            conteggio.Add('v', 0);
            conteggio.Add('w', 0);
            conteggio.Add('x', 0);
            conteggio.Add('y', 0);
            conteggio.Add('z', 0);
            conteggio.Add('0', 0);
        }

        public void AzzeraConteggio()
        {
            conteggio['a'] = 0;
            conteggio['à'] = 0;
            conteggio['b'] = 0;
            conteggio['c'] = 0;
            conteggio['d'] = 0;
            conteggio['e'] = 0;
            conteggio['è'] = 0;
            conteggio['é'] = 0;
            conteggio['f'] = 0;
            conteggio['g'] = 0;
            conteggio['h'] = 0;
            conteggio['i'] = 0;
            conteggio['ì'] = 0;
            conteggio['j'] = 0;
            conteggio['k'] = 0;
            conteggio['l'] = 0;
            conteggio['m'] = 0;
            conteggio['n'] = 0;
            conteggio['o'] = 0;
            conteggio['ò'] = 0;
            conteggio['p'] = 0;
            conteggio['q'] = 0;
            conteggio['r'] = 0;
            conteggio['s'] = 0;
            conteggio['t'] = 0;
            conteggio['u'] = 0;
            conteggio['ù'] = 0;
            conteggio['v'] = 0;
            conteggio['w'] = 0;
            conteggio['x'] = 0;
            conteggio['y'] = 0;
            conteggio['z'] = 0;
            conteggio['0'] = 0;
        }
    }
}
