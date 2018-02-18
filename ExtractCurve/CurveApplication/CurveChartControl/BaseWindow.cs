using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CurveChartControl
{
    public partial class BaseWindow:Page
    {
        internal Languages LanguageManager { get; private set; }

        public BaseWindow()
        {
            LanguageManager = AppResources.LanguageManager;
            LanguageManager.PropertyChanged += LanguageManager_PropertyChanged;
            TextTranslated = LanguageManager.TextTranslated;
            Languages = AppResources.LanguageManager.GetAvailableLanguages();
            
        }

        void LanguageManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TextTranslated = LanguageManager.TextTranslated;
        }

        internal Dictionary<string, string> TextTranslated
        {
            get { return (Dictionary<string, string>)GetValue(TextTranslatedProperty); }
            set { SetValue(TextTranslatedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Texts.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty TextTranslatedProperty =
            DependencyProperty.Register("TextTranslated", typeof(Dictionary<string, string>), typeof(BaseWindow), new PropertyMetadata(null));

        internal List<Languages.Language> Languages
        {
            get { return (List<Languages.Language>)GetValue(LanguagesProperty); }
            set { SetValue(LanguagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Languages.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty LanguagesProperty =
            DependencyProperty.Register("Languages", typeof(List<Languages.Language>), typeof(BaseWindow), new PropertyMetadata(null));

    }
}
