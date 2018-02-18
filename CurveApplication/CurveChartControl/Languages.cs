using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Xml.Linq;

namespace CurveChartControl
{
    public class Languages : INotifyPropertyChanged
    {
        private List<Language> languages = null;
        private Language currentLanguage = null;

        public List<Language> GetAvailableLanguages()
        {
            return languages;
        }

        public Languages()
        {
            ResetToDefault();
        }

        ///reset to default language
        private void ResetToDefault()
        {
            InitializeLanguages();
            currentLanguage = languages.FirstOrDefault(l => l.IsDefault == true);
            if (currentLanguage == null) currentLanguage = languages[0];
            SetDefaultLanguage(currentLanguage.Id);
        }

        private Translations textTranslated;

        public Translations TextTranslated
        {
            get
            {
                return textTranslated;
            }
            set
            {
                textTranslated = value;
                PropertyDidChange("TextTranslated");
            }
        }

        private void InitializeLanguages()
        {
            languages = new List<Language>();
            String path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path += "\\Languages\\Languages.xml";
            var elementLanguages = XElement.Load(path);
            foreach (XElement element in elementLanguages.Elements("language"))
            {
                languages.Add(new Language(element));
            }

        }

        public void SetDefaultLanguage(string languageId)
        {
            //Do not change language if languageId is empty
            if (!string.IsNullOrEmpty(languageId))
            {
                try
                {
                    Translations texts = new Translations();

                    //Load dictionary with labels
                    String path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    path += "\\Languages\\" + languageId + ".xml";
                    var elementLanguages = XElement.Load(path);
                    foreach (XElement element in elementLanguages.Elements("label"))
                    {
                        texts.Add(element.Attribute("key").Value, element.Value);
                    }

                    TextTranslated = texts;
                    texts = null;
                }
                catch (Exception ex) 
                {
                    System.Diagnostics.Debug.WriteLine("Error while setting default language. Language provided: " + languageId + "\n\rError details: " + ex.ToString());
                    //Reset to default language
                    ResetToDefault();
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void PropertyDidChange(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class Language
        {
            public string Id { get; private set; }
            public string Description { get; private set; }
            public bool IsEnabled { get; private set; }
            public bool IsDefault { get; private set; }

            public Language(XElement item)
            {
                if (item != null)
                {
                    Id = item.Attribute("id").Value;
                    Description = item.Attribute("text").Value;
                    IsEnabled = bool.Parse(item.Attribute("enabled").Value);
                    IsDefault = bool.Parse(item.Attribute("default").Value);
                }
            }
        }
        
        #region Attached properties
        
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached("Label", typeof(string), typeof(Languages), new PropertyMetadata("", OnLabelChanged));

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d is TextBlock)
            {
                ((TextBlock)d).Text = e.NewValue.ToString();
                return;
            }
            if (d is MenuItem)
            {
                ((MenuItem)d).Header = e.NewValue.ToString();
                return;
            }

            if (d is ToolTip)
            {
                ((ToolTip)d).Content = e.NewValue.ToString();
                return;
            }

            if (d is RadioButton)
            {
                ((RadioButton)d).Content = e.NewValue.ToString();
                return;
            }

            if (d is Button)
            {
                ((Button)d).Content = e.NewValue.ToString();
                return;
            }

            if (d is C1.WPF.C1Chart.Axis)
            {
                ((C1.WPF.C1Chart.Axis)d).Title = e.NewValue.ToString();
                return;
            }

            return;
        }

        public static string GetLabel(DependencyObject obj)
        {
            return (string)obj.GetValue(LabelProperty);
        }

        public static void SetLabel(DependencyObject obj, String value)
        {
            obj.SetValue(LabelProperty, value);
        }
        
        #endregion
    }
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors"), Serializable]
    public class Translations : Dictionary<String, String> 
    {
        new public string this[String key] 
        { 
            get
            {
                if (this.ContainsKey(key)) return base[key];
                return String.Empty;
            }         
        }        
    }
}
