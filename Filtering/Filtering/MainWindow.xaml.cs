using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Filtering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Personne> persons;
        ICollectionView cvPersonnes;


        public MainWindow()
        {
            InitializeComponent();
            persons = new ObservableCollection<Personne>();

            persons.Add(new Personne() { Id = 1, Nom = "Jean-Michel", Prenom = "BADANHAR" });
            persons.Add(new Personne() { Id = 1, Nom = "Gerard", Prenom = "DEPARDIEU" });
            persons.Add(new Personne() { Id = 1, Nom = "Garfild", Prenom = "THECAT" });
            persons.Add(new Personne() { Id = 1, Nom = "Jean-Paul", Prenom = "BELMONDO" });

            cvPersonnes = CollectionViewSource.GetDefaultView(persons);

            if (cvPersonnes != null)
            {
                dataGrid.ItemsSource = cvPersonnes;
                cvPersonnes.Filter = TextFilter;
            }
        }

        public bool TextFilter(object o)
        {
            bool bNom = true;
            bool bPrenom = true;

            if (textBoxNom.Text.Equals(String.Empty) && textBoxPrenom.Text.Equals(String.Empty))
                return true;

            Personne p = (o as Personne);
            if (p == null)
                return false;

            if (p.Nom.Contains(textBoxNom.Text))
                if (textBoxPrenom.Text.Equals(String.Empty))
                    return true;
                else
                    bNom = true;
            else
                if (textBoxPrenom.Text.Equals(String.Empty))
                    return false;
                else
                    bNom = false;

            if (p.Prenom.Contains(textBoxPrenom.Text))
                if (textBoxNom.Text.Equals(String.Empty))
                    return true;
                else
                    bPrenom = true;
            else
                if (textBoxNom.Text.Equals(String.Empty))
                    return false;
                else
                    bPrenom = false;

            return bPrenom & bNom;
        }

        private void textBoxNom_TextChanged(object sender, TextChangedEventArgs e)
        {
            cvPersonnes.Filter = TextFilter;
        }

        private void textBoxPrenom_TextChanged(object sender, TextChangedEventArgs e)
        {
            cvPersonnes.Filter = TextFilter;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            persons.Add(new Personne() { Id = 2, Nom = "Adf", Prenom = "Rew" });
        }
    }

    public class AnnotationFilterTagViewModel : NotificationObject
    {
        public AnnotationFilterTagViewModel(String name, int atomId)
        {
            Name = name;
            AtomID = atomId;
            CloseButtonVisibility = Visibility.Visible;
        }

        public String Name { get; private set; }
        public int AtomID { get; private set; }
        private Visibility m_closeButtonVisibility;
        public Visibility CloseButtonVisibility
        {
            get
            {
                return m_closeButtonVisibility;
            }
            set
            {
                m_closeButtonVisibility = value;
                this.RaisePropertyChanged(() => CloseButtonVisibility);
            }
        }
    }

    public class Personne
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
    }
}
