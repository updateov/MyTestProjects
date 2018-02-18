using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

namespace TextBlockInlineListBindingTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CustomText
        {
            get { return @"Windows Phone<br/>is<br/>really cool!"; }
        }

   

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new VM();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as VM).Items.First().Size = 30;
            (this.DataContext as VM).Items.First().Text = "FKEJASFKJ";
            (this.DataContext as VM).Raise();
        }
    }

    public class TextBlockInlineConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var inlines = new List<Inline>();
            if (value != null)
            {
                // add inlines and linebreaks
                foreach (Model line in value as ObservableCollection<Model>)
                {
                    inlines.Add(new Run() { Text = line.Text + " ", FontSize = line.Size });
                }
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    //public static class TextBlockBindingSamplePage
    //{
    //    public static string GetInlineList(TextBlock element)
    //    {
    //        if (element != null)
    //            return element.GetValue(ArticleContentProperty) as string;
    //        return string.Empty;
    //    }

    //    public static void SetInlineList(TextBlock element, string value)
    //    {
    //        if (element != null)
    //            element.SetValue(ArticleContentProperty, value);
    //    }

    //    public static readonly DependencyProperty ArticleContentProperty =
    //        DependencyProperty.RegisterAttached(
    //            "InlineList",
    //            typeof(List<Inline>),
    //            typeof(TextBlockBindingSamplePage),
    //            new PropertyMetadata(null, OnInlineListPropertyChanged));

    //    private static void OnInlineListPropertyChanged(DependencyObject obj,
    //        DependencyPropertyChangedEventArgs e)
    //    {
    //        var tb = obj as TextBlock;
    //        if (tb != null)
    //        {
    //            // clear previous inlines
    //            tb.Inlines.Clear();

    //            // add new inlines
    //            var inlines = e.NewValue as List<Inline>;
    //            if (inlines != null)
    //            {
    //                inlines.ForEach(inl => tb.Inlines.Add((inl)));
    //            }
    //        }
    //    }
    //}

    public static class TextBlockBindingSamplePage
    {
        public static List<Inline> GetInlineList(TextBlock element)
        {
            if (element != null)
                return element.GetValue(ArticleContentProperty) as List<Inline>;

            return new List<Inline>();
        }

        public static void SetInlineList(TextBlock element, List<Inline> value)
        {
            if (element != null)
                element.SetValue(ArticleContentProperty, value);
        }

        public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "InlineList",
                typeof(List<Inline>),
                typeof(TextBlockBindingSamplePage),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBlock;
            if (tb != null)
            {
                // clear previous inlines
                tb.Inlines.Clear();

                // add new inlines
                var inlines = e.NewValue as List<Inline>;
                if (inlines != null)
                {
                    inlines.ForEach(inl => tb.Inlines.Add((inl)));
                }
            }
        }
    }

    public class VM : NotificationObject
    {
        public VM()
        {
            Items = new ObservableCollection<Model>()
            {
                new Model() { Text="AAA", Size=10},
                new Model() { Text="BBB", Size=15}
            };
        }

        public String Text
        {
            get
            {
                String toRet = String.Empty;
                foreach (var item in Items)
                {
                    toRet = item.Text + " ";
                }

                return toRet;
            }
        }
        public ObservableCollection<Model> Items { get; set; }

        internal void Raise()
        {
            RaisePropertyChanged(() => Items);
        }
    }

    public class Model
    {
        public String Text { get; set; }
        public int Size { get; set; }
    }
}
