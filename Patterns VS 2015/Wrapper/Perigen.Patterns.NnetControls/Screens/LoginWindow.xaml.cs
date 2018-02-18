using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Perigen.Patterns.NnetControls.Screens
{
    public enum LoginStatus
    {
        Error = -1,
        Ok = 0,
        PwdInvalid,
        NoPermissions
    }

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private string m_PwdInvalid = "The system could not log you on. Make sure your user ID is correct, then type your password again. Letters in passwords are case sensitive. Make sure the Caps Lock is not accidentally on.";
        private string m_NoPermissions = "Cannot perform export because you do not have sufficient privileges.";
        private string m_Error = "Network error.";

        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
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

        public string UserName { get { return txtUserName.Text; } }

        public LoginWindow(int function, int action)
        {
            InitializeComponent();

            m_function = function;
            m_action = action;

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            txtUserName.Focus();
        }

        private void Login()
        {
            if (this.IsEnableOK == false)
                return;

            UserDetails userData = new UserDetails();
            userData.UserName = txtUserName.Text;
            userData.Password = txtPassword.Password;
            userData.Function = m_function;
            userData.Action = m_action;

            LoginStatus status = PatternsUIManager.Instance.Login(userData);

            switch (status)
            {
                case LoginStatus.Ok:
                    {
                        PatternsUIManager.Instance.UserID = txtUserName.Text;
                        this.DialogResult = true;
                        m_gridFadeOutStoryBoard.Begin();
                        this.Close();
                    }
                    break;

                case LoginStatus.NoPermissions:
                    ErrorDescr = this.m_NoPermissions;
                    break;

                case LoginStatus.PwdInvalid:
                    ErrorDescr = this.m_PwdInvalid;
                    break;

                case LoginStatus.Error:
                    ErrorDescr = this.m_Error;
                    break;
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

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {         
            m_gridFadeOutStoryBoard.Begin();
            this.DialogResult = false;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private void loginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();       
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
    }
}
