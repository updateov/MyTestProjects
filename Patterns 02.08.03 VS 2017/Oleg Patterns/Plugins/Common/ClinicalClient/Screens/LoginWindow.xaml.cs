using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private string m_PwdInvalid = "The system could not log you on. Make sure your user ID is correct, then type your password again. Letters in passwords are case sensitive. Make sure the Caps Lock is not accidentally on.";
        private string m_NoPermissions = "You do not belong to any user group that allows documentation.";
        
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
        private Point m_LastPoint;
        private int m_function;
        private int m_action;

        public bool m_isEnableOK;
        public bool IsEnableOK
        {
            set
            {
                if (m_isEnableOK != value)
                {
                    m_isEnableOK = value;
                    RaisePropertyChanged("IsEnableOK");
                }
            }
            get
            {
                return m_isEnableOK;
            }
        }
        private DispatcherTimer m_activityTimer = null;

        public string m_errorDescr = String.Empty;
        public string ErrorDescr
        {
            set
            {
                if (m_errorDescr != value)
                {
                    m_errorDescr = value;
                    RaisePropertyChanged("ErrorDescr");
                }
            }
            get
            {
                return m_errorDescr;
            }
        }

        public LoginWindow(Rect dimensions, int function, int action, double timeout = 0)
        {
            InitializeComponent();

            m_function = function;
            m_action = action;

            this.Top = dimensions.Top;
            this.Left = dimensions.Left;
            this.Width = dimensions.Width;
            this.Height = dimensions.Height;

            Color color = Color.FromArgb(130, 63, 63, 63);
            this.Background = new SolidColorBrush(color);

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            txtUserName.Focus();
            this.KeyUp += OnKeyUp;

            if (timeout > 0)
            {
                m_activityTimer = new DispatcherTimer
                (
                    TimeSpan.FromSeconds(timeout),
                    DispatcherPriority.ApplicationIdle,
                    Inactivity,
                    Application.Current.Dispatcher
                );

                this.MouseMove += OnMouseMove;
                this.KeyDown += OnKeyDown;
                this.MouseLeftButtonUp += OnMouseLeftButtonUp;
            }
        }

        void Activity()
        {
            //if (this.IsLoaded == true &&
            //    this.Visibility != System.Windows.Visibility.Hidden && 
            //    this.WindowState != System.Windows.WindowState.Minimized)
            //{
            if (m_activityTimer != null)
            {
                m_activityTimer.Stop();
                m_activityTimer.Start();
            }
            //}
            //else
            //{
            //    m_activityTimer.Stop();
            //}        
        }

        void Inactivity(object sender, EventArgs e)
        {
            if (m_activityTimer != null)
            {
                m_activityTimer.Stop();
            }

            this.DialogResult = false;
            this.Close();
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (m_activityTimer != null)
                {
                    m_activityTimer.Stop();
                }

                m_gridFadeOutStoryBoard.Begin();
                this.DialogResult = false;
                this.Close();
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Login();
            }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (m_activityTimer != null)
            {
                Activity();
            }
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (m_activityTimer != null)
            {
                if (m_LastPoint != e.GetPosition(this))
                {
                    m_LastPoint = e.GetPosition(this);

                    Activity();
                }
            }
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_activityTimer != null)
            {
                Activity();
            }
        }

        private void Validate()
        {
            if (String.IsNullOrEmpty(txtUserName.Text.Trim()) == false &&
                String.IsNullOrEmpty(txtPassword.Password.Trim()) == false)
            {
                this.IsEnableOK = true;
            }
            else
            {
                this.IsEnableOK = false;
            }
        }

        private void loginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();

            if (m_activityTimer != null)
            {
                m_LastPoint = Mouse.GetPosition(this);

                Activity();
            }
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_activityTimer != null)
            {
                m_activityTimer.Stop();
            }

            m_gridFadeOutStoryBoard.Begin();
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            if (this.IsEnableOK == false)
                return;

            if (App.ClientManager.Login(txtUserName.Text, txtPassword.Password))
            {
                if (App.ClientManager.CheckUserRights(m_function, m_action))
                {
                    if (m_activityTimer != null)
                    {
                        m_activityTimer.Stop();
                    }

                    this.DialogResult = true;
                    m_gridFadeOutStoryBoard.Begin();
                    this.Close();
                }
                else
                {
                    ErrorDescr = this.m_NoPermissions;
                    App.ClientManager.Logout();
                }
            }
            else
            {
                ErrorDescr = this.m_PwdInvalid;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (m_activityTimer != null)
            {
                m_activityTimer.Stop();
            }

            m_gridFadeOutStoryBoard.Begin();
            this.DialogResult = false;
            this.Close();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion     

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }
    }
}
