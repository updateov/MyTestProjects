using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using C1.WPF.C1Chart;

namespace CurveChartControl
{

    /// <summary>
    /// Interaction logic for PageMain.xaml
    /// </summary>
    public partial class PageMain : UserControl
    {
		private const int BannerPerigenValue = 0;
		private const int BannerPowerByValue = 1;

		//Create model to hold data
		CurveModel dataModel = new CurveModel();

		private BackgroundWorker backWorker;

		//animation
		bool _showAnim = false;

		//Edit Mode
		Boolean isEditMode = false;

		//Review Mode
		bool _isReviewMode = false;
		public bool IsReviewMode
		{
			get
			{
				return _isReviewMode;
			}
			set
			{
				if (value != _isReviewMode)
				{
					_isReviewMode = value;
					IsEditionAllowed = (value) ? false : _editionAllowedBackupForReviewMode;
					UpdateUIPermissions();

				}
			}
		}

		bool FirstCall = true;

		bool _editionAllowedBackupForReviewMode = false;

		//Service URL used to communicate with server
		string ServiceURL = string.Empty;

		//Query String Dictionary
		Dictionary<string, string> QueryString = new Dictionary<string, string>();

		//Query String Constant keys
		const string queryStringDataFeedService = "service";
		const string queryStringUserName = "user_name";
		const string queryStringUserId = "user_id";
		const string queryStringCanModify = "can_modify";
		const string queryStringCanPrint = "can_print";
		const string queryStringVisitKey = "visit_key";
		const string queryStringCurveClientRefresh = "curve_client_refresh";
		const string queryStringPODISendEpidural = "PODI_send_epidural";
		const string queryStringPODISendVBAC = "PODI_send_vbac";
		const string queryStringPODISendVaginalDelivery = "PODI_send_vaginaldelivery";
		const string queryStringReviewModeEnabled = "curve_review_mode_enabled";
		const string queryStringBannerType = "banner";
		string DataCallRequest;
		string currentTheme = "CALM";

		//Possible values are 0 – display, 1 – N/A, 2 – (secure name)" type="integer"
		const string queryStringDisplayPHIForDefaultUser = "default_User_PHI_Option";

		string stringLastRequest = string.Empty;
		DispatcherTimer refreshTimer = new DispatcherTimer();
		ContextMenu myMenu = null;

		//parameters passed to the service
		string param = string.Empty;

        //used to recreate grid if it is necessary
        int flagExamsCounter = 0;

		//Commands
		public static RoutedCommand FirstExamCommand = new RoutedCommand();

		public PageMain()
		{
			//Avoid issues with certificates
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });

			InitializeComponent();

			this.CommandBindings.Add(new CommandBinding(FirstExamCommand, FirstExamCommandExecute, FirstExamCommandCanExecute));

            //Commanding focus
            Focus();

			//hook load
			this.Loaded += new RoutedEventHandler(PageLoaded);

		}

		/// <summary>
		/// Command for first exam
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FirstExamCommandExecute(object sender, ExecutedRoutedEventArgs e)
		{
            Exam exam = e.Parameter as Exam;
			var date = DateTime.FromOADate((double)exam.TimeOADate).ToLocalTime().ToShortDateString();
			var time = DateTime.FromOADate((double)exam.TimeOADate).ToLocalTime().ToShortTimeString();
            lblTitleSetFirstExamDialog.Text = "First Exam Selection";
            lblMessage.Text = "Exam was charted " + date + " " + time + ". Begin labor evaluation from this exam?";
            
            btnOkFirstExam.Tag = exam;
            btnOkFirstExam.Click+=OKFirstExam;
            btnCancelFirstExam.Click += delegate
            {
                GridFirstExamDialog.Visibility = System.Windows.Visibility.Collapsed;
            };
            GridFirstExamDialog.Visibility = System.Windows.Visibility.Visible;
		}

        private void OKFirstExam(object sender, RoutedEventArgs e) 
        {
            var exam = (Exam)((Control)sender).Tag;
            //update first exam
            GenerateXMLAndSendUpdate(exam);
            GridFirstExamDialog.Visibility = System.Windows.Visibility.Collapsed;
        }

		/// <summary>
		/// can execute first exam
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FirstExamCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = IsEditionAllowed;
		}

		void PageLoaded(object sender, RoutedEventArgs e)
		{
			//Check if it is network deployed, that way we can store initial parameters in the app
			//if (ApplicationDeployment.IsNetworkDeployed)
			//{
//				try
//				{
//					//uri
//					Uri launchUri = ApplicationDeployment.CurrentDeployment.ActivationUri;

//					// Get parameters
//					var parameters = HttpUtility.ParseQueryString(launchUri.Query);
//					if (parameters.Count == 0)
//						throw new InvalidDataException("Error. Parameters are missing.");

//					var parameter = parameters[0];

//					// Decode
//					parameter = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(parameter));
//					if (string.IsNullOrEmpty(parameter)) 
//						throw new InvalidDataException("Error. Parameters are missing.");
					
//					// Extract the parameters value
//					parameters = HttpUtility.ParseQueryString(parameter);
//					if (parameters.Count == 0)
//						throw new InvalidDataException("Error. Parameters are missing.");

//					Debug.WriteLine("XBAP|parameters=" + parameter);

//					foreach (string key in parameters)
//					{
//						Debug.WriteLine(key + "=" + parameters[key]);
//						QueryString.Add(key, parameters[key]);
//					}
//					ServiceURL = QueryString.GetValueWithDefault(queryStringDataFeedService);

//					// Initialize the call string for the data to the server
//					DataCallRequest = new XElement("patients", "{0}", new XAttribute("version", "01.00.00.00"), new XAttribute("user", QueryString[queryStringUserId] ?? string.Empty)).ToString(SaveOptions.DisableFormatting);
//				}
//				catch (Exception ex)
//				{
//					//No parameters			
//					DisplayMainError(ex);
//					return;
//				}
//			}
////#if !DEBUG
////            else
////            {                
////                Exception ex =  new InvalidDeploymentException("Invalid deployment method.\nCannot read application parameters.");    			
////                //No parameters			
////	    		DisplayMainError(ex);
////				return;
////            }
////#else
//            else
//			{
				//debug
                string[] query = "service=http://olegu_win10:7802/PatternsDataFeed/&curve_client_refresh=5&banner=0&curve_review_mode_enabled=True&PODI_send_epidural=False&PODI_send_vbac=False&PODI_send_vaginaldelivery=False&user_name=PeriGen Support&user_id=PeriGen&can_modify=True&can_print=True&visit_key=666-16-1-1".Split('&');
				foreach (var item in query)
				{
					QueryString.Add(item.Split('=')[0], item.Split('=')[1]);
				}
				ServiceURL = QueryString[queryStringDataFeedService];

				// Initialize the call string for the data to the server
				DataCallRequest = new XElement("patients", "{0}", new XAttribute("version", "01.00.00.00"), new XAttribute("user", QueryString[queryStringUserId] ?? string.Empty)).ToString(SaveOptions.DisableFormatting);
			//}
//#endif

			//TO Debug parameters
			//foreach (var item in QueryString)
			//{
			//    MessageBox.Show(item.Key + ":" + item.Value);
			//}

			//Set language
			AppResources.LanguageManager.SetDefaultLanguage("english"); // Default language, to be changed when it will (if it will) support multi language

			////Application title
			//this.Title = AppResources.LanguageManager.TextTranslated["ApplicationTitle"];

			//UI theme
			currentTheme = "CALM";
			SetTheme();

			//clear messages for the moment
			dataModel.Message = string.Empty;

			DataContext = this;
			//assign model to page and curve container
			borderCurve.DataContext = dataModel;

			////////////////////////////////////////////////////////////
			//Allow edition??
			////////////////////////////////////////////////////////////

			IsEditionAllowed = Boolean.Parse(QueryString.GetValueWithDefault(queryStringCanModify));
			_editionAllowedBackupForReviewMode = IsEditionAllowed;

			IsReviewModeVisible = Boolean.Parse(QueryString.GetValueWithDefault(queryStringReviewModeEnabled));

			IsEpiduralEditionVisible = !Boolean.Parse(QueryString.GetValueWithDefault(queryStringPODISendEpidural));
			IsVBACEditionVisible = !Boolean.Parse(QueryString.GetValueWithDefault(queryStringPODISendVBAC));
			IsPreviousVaginalEditionVisible = !Boolean.Parse(QueryString.GetValueWithDefault(queryStringPODISendVaginalDelivery));

			//Check is powered by logo
			IsPoweredByLogo = (int.Parse(QueryString.GetValueWithDefault(queryStringBannerType)) != BannerPerigenValue);

			UpdateUIPermissions();
			btnEditValues.Click += EditItems;

			//hook events
			btnCancel.Click += btnCancel_Click;
			btnOk.Click += btnOk_Click;

			//btnExport.Click += new RoutedEventHandler(btnExport_Click);
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty, typeof(ScrollViewer));
			descriptor.AddValueChanged(ScrollViewerFlowSheet, new EventHandler(HorizontalScrollBarIsChanged));

			backWorker = new BackgroundWorker();
			backWorker.DoWork += WorkerDoWork;
			backWorker.RunWorkerCompleted += WorkerRunCompleted;

			//Flowsheet
			//Always show flowsheet skip validation for now....
			chkFlow.IsChecked = true;
			btnAbout.Click += new RoutedEventHandler(btnAbout_Click);

			myMenu = new ContextMenu();
			btnSnapshots.Click += new RoutedEventHandler(btnSnapshots_Click);

			//Refresh interval
			var interval = QueryString.GetValueWithDefault(queryStringCurveClientRefresh);
			if (string.IsNullOrEmpty(interval)) interval = "20"; //seconds

            //Create chart elements
            SetupChart();
			
            //update positions
            UpdateLayout();			

			//setup timer for autorefresh
			refreshTimer.Interval = TimeSpan.FromSeconds(int.Parse(interval));
			refreshTimer.Tick += TimerRefreshTick;
			refreshTimer.Start();

			//refresh curve
			RefreshCurve(string.Empty);

		}

		/// <summary>
		/// Update UI
		/// </summary>
		private void UpdateUIPermissions()
		{
			btnEditValues.Visibility = IsEditionAllowed &&  (IsEpiduralEditionVisible || IsPreviousVaginalEditionVisible || IsVBACEditionVisible) ? Visibility.Visible : Visibility.Collapsed;
		}

		void btnSnapshots_Click(object sender, RoutedEventArgs e)
		{
			//Stop autorefresh
			refreshTimer.Stop();
			myMenu.Items.Clear();
			myMenu.MaxHeight = 200.0;
			var result = SendRequest(ServiceURL + "snapshots", string.Format(DataCallRequest, stringLastRequest));
			var data = XElement.Parse(result);
			bool liveOne = true;
			foreach (var item in data.Descendants("snapshot"))
			{
				//create snapshots and menuitems from XElement
				Snapshot snapshot = new Snapshot(item);
				var menuItem = new MenuItem() { Header = snapshot.ToString(), Tag = snapshot };
				menuItem.Click += itemMenu_Click;
				myMenu.Items.Add(menuItem);

				//first one?
				if (liveOne)
				{
					liveOne = false;
					snapshot.IsLive = true;
					menuItem.Icon = new Image() { Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"/images/lastSnapshot.png")) };
					myMenu.Items.Add(new Separator());
				}
			}
			myMenu.PlacementTarget = this;
			myMenu.IsOpen = true;

			//restart autorefresh
			refreshTimer.Start();
		}

		//Click on item of review mode
		private void itemMenu_Click(object sender, RoutedEventArgs e)
		{
			IsReviewMode = true;
			Snapshot snap = ((MenuItem)sender).Tag as Snapshot;
			if (myMenu.Items.IndexOf(sender) == 0)
			{
				// go back to live mode
				IsReviewMode = false;

				// Update request data to indicate review mode and Id to retrieve
				XElement request = XElement.Parse(stringLastRequest);

				// Reset Id to get
				request.SetAttributeValue("snapshot", string.Empty);

				//Update string
				stringLastRequest = request.ToString();
			}
			else
			{
				// review mode
				IsReviewMode = true;

				// Update request data to indicate review mode and Id to retrieve
				XElement request = XElement.Parse(stringLastRequest);

				//Id to get
				request.SetAttributeValue("snapshot", snap.Id.ToString());

				//Update string
				stringLastRequest = request.ToString();

				//Get data
				FirstCall = true;
			}
			RefreshCurve(string.Empty);
		}

		void btnAbout_Click(object sender, RoutedEventArgs e)
		{

            //this.Title = Statics.AppResources.LanguageManager.TextTranslated["AboutTitle"];
            btnOkAbout.Content = AppResources.LanguageManager.TextTranslated["AboutButtonOkTitle"];
            btnOkAbout.Click += delegate { GridAbout.Visibility =System.Windows.Visibility.Collapsed; };
            var aboutData = new AboutData();
            aboutData.IsPowerByPeriGen = IsPoweredByLogo; //check if we display the Is Power By PeriGen logo or just the normal PeriGen logo 
            if (IsPoweredByLogo)
            {
                //imgCCMark.Visibility = System.Windows.Visibility.Collapsed;
                imgPerigen.Visibility = System.Windows.Visibility.Collapsed;
                imgPoweredBy.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                imgPerigen.Visibility = System.Windows.Visibility.Visible;
                //imgCCMark.Visibility = System.Windows.Visibility.Visible;
                imgPoweredBy.Visibility = System.Windows.Visibility.Collapsed;
            }
            
            GridAbout.DataContext = aboutData;
            GridAbout.Visibility = System.Windows.Visibility.Visible;
		}

		/// <summary>
		/// Update UI theme
		/// </summary>
		private void SetTheme()
		{
			//for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
			//{
			//	//Get resource dictionary
			//	var dic = Application.Current.Resources.MergedDictionaries[i];
			//	//Is it a theme..?
			//	if ((dic.Source == null) || dic.Source.OriginalString.ToLower().Contains("themes/theme"))
			//	{
			//		//remove it and add the new requested theme
			//		Application.Current.Resources.MergedDictionaries.RemoveAt(i);
			//		//Application.Current.Resources.MergedDictionaries.Insert(i, Application.LoadComponent(new Uri("CurveChartControl;component/Themes/Theme" + currentTheme + ".xaml", UriKind.Relative)) as ResourceDictionary);
			//		break;
			//	}
   //         }
        }

		/// <summary>
		/// Set Review Mode Watermark
		/// </summary>
		private void SetReviewModeText()
		{

			///check review mode status
			if (!IsReviewMode)
			{
				c1ChartCtl.View.PlotBackground = Brushes.White;
				return;
			}

			// Style for the text.
			FormattedText fText = new FormattedText("     IN REVIEW     ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Sans Serif"), 18.0, new SolidColorBrush(Color.FromRgb(192, 192, 192)));

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.White, 1.0), new Rect(0.0, 0.0, fText.WidthIncludingTrailingWhitespace, fText.WidthIncludingTrailingWhitespace));
			drawingContext.PushTransform(new RotateTransform(45.0, 0.5, 0.5));
			drawingContext.DrawText(fText, new Point(0.0, 0.0));
			drawingContext.Pop();
			drawingContext.Close();

			// Render to bitmap.
			int imageWidth = (int)fText.WidthIncludingTrailingWhitespace;
			int imageHeight = (int)fText.WidthIncludingTrailingWhitespace;
			RenderTargetBitmap rtBitmap = new RenderTargetBitmap(imageWidth, imageHeight,
										  96.0, 96.0, PixelFormats.Pbgra32);

			rtBitmap.Render(drawingVisual);

			Image im = new Image();
			im.Source = rtBitmap;

			//apply watermark to chart control
			ImageBrush img = new ImageBrush(im.Source);
			img.TileMode = TileMode.Tile;
			img.ViewportUnits = BrushMappingMode.Absolute;
			img.Viewport = new Rect(0, 0, imageWidth, imageHeight);

			c1ChartCtl.View.PlotBackground = img;

		}

		/// <summary>
		/// handles tick timer to refresh data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void TimerRefreshTick(object sender, EventArgs e)
		{
			if (isEditMode)
			{
				refreshTimer.Stop();
				return;
			}
			else
			{
				RefreshCurve(string.Empty);
			}
		}

		/// <summary>
		/// Thread execution finished. Data comming from server is ready
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void WorkerRunCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

			if (e.Error == null)
			{
				try
				{
					//result from thread...
					string data = e.Result.ToString();

					// Parse data
					XElement result = null;
					try
					{
						result = XElement.Parse(data);
					}
					catch (Exception)
					{
						throw new Exception("Unable to reach the server.");
					}

					if (result != null)
					{
						//update data model and chart
						dataModel.ParseCurveData(result);

						//store last request node
						stringLastRequest = dataModel.LastRequestInfo;

						borderCurve.DataContext = null;
                        borderCurve.DataContext = dataModel;
                        
                        //Review Mode
                        IsReviewMode = dataModel.IsReviewMode;

                        //Demo Mode
                        IsDemoMode = dataModel.DemoMode;

                        //IsEditionAllowed Update based in restored visit.....
                        IsEditionAllowed = Boolean.Parse(QueryString.GetValueWithDefault(queryStringCanModify)) && !dataModel.IsReadOnly;
                        UpdateUIPermissions();

                        //avoid resizing in HPC
                        //Only creat grid when is the first call or the exams count is different
                        if (!FirstCall && dataModel.Exams.Count == flagExamsCounter)
                        {
                            //update points only
                            CreateChart();
                        }
                        else
                        {                            
                            //could create grid?
                            if (SetupChart())
                            {
                                flagExamsCounter = dataModel.Exams.Count;
                                
                                //update points
                                CreateChart();
                            }
                        }
					}
					else
					{
						//No data, clear
						borderCurve.DataContext = null;
					}
				}
				catch (Exception ex)
				{
					///Error  during parsing
					DisplayError(ex);
				}

			}
			else
			{
				///error during refresh data
				DisplayError(e.Error);
			}

			//first time load black curtain
			if (borderLoad.Visibility == System.Windows.Visibility.Visible) borderLoad.Visibility = System.Windows.Visibility.Collapsed;
		}

		/// <summary>
		/// Display error and refresh UI
		/// </summary>
		/// <param name="ex"></param>
		private void DisplayError(Exception ex)
		{
			//show error
			refreshTimer.Stop();

			//remove reviewMode
			IsReviewMode = false;

			//show no data
			borderCurve.DataContext = null;

			//initialize datamodel to zero
			dataModel.Reset();
			FirstCall = true;

			//set error to display message                    
			dataModel.VisitStatus = CurveModel.VisitStatusEnum.Error;

			// To comply with requirement with the message to display if the service is not running...
			var details = ex.Message;
			if (string.CompareOrdinal("Unable to connect to the remote server", details) == 0)
				details = "Unable to reach the server.";
 
			//set error to show
			dataModel.VisitStatusDetail = details;

			//assign datamodel to show error only
			borderCurve.DataContext = dataModel;

			// Clear the chart
			CreateChart();

			//start timer for automatic refresh again
			refreshTimer.Start();
		}

		/// <summary>
		/// Display error and DO NOT refresh UI
		/// </summary>
		/// <param name="ex"></param>
		private void DisplayMainError(Exception ex)
		{

			//remove reviewMode
			IsReviewMode = false;

			//show no data
			borderCurve.DataContext = null;

			//initialize datamodel to zero
			dataModel.Reset();
			FirstCall = true;

			//set error to display message                    
			dataModel.VisitStatus = CurveModel.VisitStatusEnum.Error;

			// To comply with requirement with the message to display if the service is not running...
			var details = ex.Message;
			if (string.CompareOrdinal("Unable to connect to the remote server", details) == 0)
				details = "Unable to reach the server.";

			//set error to show
			dataModel.VisitStatusDetail = details;

			//assign datamodel to show error only
			borderCurve.DataContext = dataModel;

			// Clear the chart
			CreateChart();

		}

		/// <summary>
		/// Connect to server and get/update curve data
		/// Remark: if an exception occurs in that method, the BackgroundWorker class takes care of handling it and put the exception content in the DoWorkEventArgs.Error member
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void WorkerDoWork(object sender, DoWorkEventArgs e)
		{

			///check if url is provided
			if (string.IsNullOrEmpty(ServiceURL)) 
				throw new ArgumentException(AppResources.LanguageManager.TextTranslated["ErrorNOUrl"]);

			try
			{
				var url = ServiceURL;
				//verify parameters: first time or edition
				if (string.IsNullOrEmpty(e.Argument.ToString()))
				{
					//first time or refresh?
					if (string.IsNullOrEmpty(stringLastRequest))
					{
						//first time
						e.Result = SendRequest(ServiceURL + "curve", string.Format(DataCallRequest, "<request key=\"" + QueryString.GetValueWithDefault(queryStringVisitKey) + "\" />"));
					}
					else
					{
						//refresh
						e.Result = SendRequest(ServiceURL + "curve", string.Format(DataCallRequest, stringLastRequest));
					}
				}
				else
				{
					//edition
					e.Result = SendRequest(ServiceURL + "updatefields", (XElement.Parse("<data>" + e.Argument.ToString() + "</data>")).ToString());
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				e.Result = null;
				throw;
			}

		}

		/// <summary>
		/// Create and Send request to server using REST
		/// </summary>
		/// <param name="uri">url</param>
		/// <param name="method">type of method [POST, GET]</param>
		/// <param name="contentType">Content Type Request</param>
		/// <param name="body">data to send</param>
		/// <returns>data from server</returns>
		static string SendRequest(string uri, string body)
		{

			string responseBody = null;

			// Create WebRequest			
			var req = (HttpWebRequest)HttpWebRequest.Create(uri);

			// increase speed avoiding checking proxy....
			req.Proxy = null;

			// Config
			req.Method = "POST";
			req.Accept = "application/xml";
			req.ContentType = "application/xml; charset=utf-8";

			if (body != null)
			{
				byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
				req.GetRequestStream().Write(bodyBytes, 0, bodyBytes.Length);
				req.GetRequestStream().Close();
			}

			///Try to get Response
			var resp = (HttpWebResponse)req.GetResponse();

			Stream respStream = resp.GetResponseStream();
			if (respStream != null)
			{
				responseBody = new StreamReader(respStream).ReadToEnd();
			}
			return responseBody;
		}

		/// <summary>
		/// Handle scrollabar for flowsheet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HorizontalScrollBarIsChanged(object sender, EventArgs e)
		{
			if (ScrollViewerFlowSheet.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
			{
				//Visible is set true 
				GridFlowSheet.Height = 193;

				//Scroll to see the last exam
				ScrollViewerFlowSheet.ScrollToRightEnd();
			}
			else
			{
				//Visible is set false
				GridFlowSheet.Height = 193;//176;
			}
			UpdateLayout();
		}

		#region Export --- code not implemented

		/// <summary>
		/// TODO: Export Code
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnExport_Click(object sender, RoutedEventArgs e)
		{
			//try
			//{
			//    using (var fs = new FileStream(@"c:\chart.jpg", FileMode.Create))
			//    {
			//        c1ChartCtl.SaveImage(fs, ImageFormat.Jpeg);
			//    }
			//    using (XpsDocument doc = new XpsDocument(@"c:\chart.xps", FileAccess.ReadWrite))
			//    {
			//        XpsDocumentWriter xpsdw = XpsDocument.CreateXpsDocumentWriter(doc);
			//        xpsdw.Write(c1ChartCtl);
			//        doc.Close();
			//    }
			//    MessageBox.Show("Chart Exported!");
			//}
			//catch (Exception ex)
			//{
			//    MessageBox.Show("Error exporting image. Details:\n\r" + ex.ToString());
			//}
		}

		#endregion

		/// <summary>
		/// Edit values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void EditItems(object sender, RoutedEventArgs e)
		{
			isEditMode = true;
			refreshTimer.Stop();

			///reset values in editors

			//epidural
			if (IsEpiduralEditionVisible)
			{

				if (dataModel.Epidural.HasValue)
				{
					chkEpidural.IsChecked = true;
					epiduralDateTimeCtl.DateTime = dataModel.Epidural.HasValue ? (Nullable<DateTime>)(dataModel.Epidural.Value.ToLocalTime()) : null;
				}
				else
				{
					chkEpidural.IsChecked = false;
					epiduralDateTimeCtl.DateTime = null;
				}
			}

			//vbac
			if (IsVBACEditionVisible)
			{

				if (dataModel.VBAC.HasValue)
				{
					if (dataModel.VBAC.Value)
					{
						radioVBACYes.IsChecked = true;
						radioVBACNO.IsChecked = false;
					}
					else
					{
						radioVBACYes.IsChecked = false;
						radioVBACNO.IsChecked = true;
					}
				}
				else
				{
					radioVBACNO.IsChecked = false;
					radioVBACYes.IsChecked = false;
				}
			}

			//previous vaginal deliveries
			if (IsPreviousVaginalEditionVisible)
			{

				if (dataModel.VaginalUI.HasValue)
				{
					if (dataModel.VaginalUI.Value)
					{
						radioVaginalDeliveryYes.IsChecked = true;
						radioVaginalDeliveryNO.IsChecked = false;
					}
					else
					{
						radioVaginalDeliveryYes.IsChecked = false;
						radioVaginalDeliveryNO.IsChecked = true;
					}
				}
				else
				{
					radioVaginalDeliveryNO.IsChecked = false;
					radioVaginalDeliveryYes.IsChecked = false;
				}
			}

			GridEdit.Visibility = System.Windows.Visibility.Visible;
		}

		/// <summary>
		/// Refresh data for curve.
		/// It updates datamodel with info from service
		/// </summary>
		/// <param name="parameters"></param>
		private void RefreshCurve(string parameters)
		{
			if (!backWorker.IsBusy) backWorker.RunWorkerAsync(parameters);
		}

		#region Points->animation --- Not used for now...

		private void Symbol_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_showAnim)
				return;

			PlotElement pe = (PlotElement)sender;

			Storyboard stb = new Storyboard();

			DoubleAnimation da = new DoubleAnimation();
			da.From = -Canvas.GetTop(pe); da.To = -Canvas.GetTop(pe); da.Duration = new Duration(TimeSpan.FromSeconds(0.01));
			Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.Children[1].Y"));
			stb.Children.Add(da);

			da = new DoubleAnimation();
			da.From = -Canvas.GetTop(pe); da.To = 0;
			if (pe.DataPoint != null)
			{
				da.BeginTime = TimeSpan.FromSeconds(0.04 * (pe.DataPoint.PointIndex));
				da.Duration = new Duration(TimeSpan.FromSeconds(1));
			}
			else
				da.Duration = new Duration(TimeSpan.FromSeconds(1.5));

			Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.Children[1].Y"));
			stb.Children.Add(da);

			stb.Begin(pe);

		}

		private void WaitAndDisableAnimation()
		{
			Storyboard stb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation();
			da.Duration = new Duration(TimeSpan.FromSeconds(1));
			Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));

			stb.Children.Add(da);
			stb.Completed += (s, a) =>
			  {
				  _showAnim = false;
			  };
			stb.Begin(this);
		}

		#endregion

        //Brushes
		Brush dilatationBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xda, 0x47, 0x00));   
        Brush brushPolygonColor = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xae, 0x01));        
        Brush brushDilatationRange = new SolidColorBrush(Color.FromArgb(0xff, 0xf8, 0x8d, 0x5c));

        /// <summary>
        /// create chart (points of data)
        /// </summary>
		private void CreateChart()
		{

			//Reset chart            
			_showAnim = false;

            //SetupChart();

            c1ChartCtl.BeginUpdate();

            c1ChartCtl.Data.Children.Clear();

            //c1ChartCtl.Background = (SolidColorBrush)Application.Current.FindResource("ChartBackgroundColorBrush");
            c1ChartCtl.Background = (SolidColorBrush)FindResource("ChartBackgroundColorBrush");

            //Review Mode?
            SetReviewModeText();

			if (dataModel.Exams.Count > 0)
			{

				c1ChartCtl.Data.ItemsSource = dataModel.Exams;
				Style ss = (Style)Resources["sstyle"];

                ///Create series for dilatation      
				var dilatationSeries = new C1.WPF.C1Chart.XYDataSeries();
				dilatationSeries.AxisX = "XAxis1";
				dilatationSeries.AxisY = "YDilatation";
				dilatationSeries.ChartType = ChartType.LineSymbols;
				dilatationSeries.ConnectionStroke = dilatationBrush;
				dilatationSeries.SymbolFill = dilatationBrush;
				dilatationSeries.SymbolStroke = dilatationBrush;
				dilatationSeries.SymbolSize = new Size(12, 12);
				dilatationSeries.SymbolStyle = ss; //animation
				dilatationSeries.XValueBinding = new Binding("Time");
				dilatationSeries.ValueBinding = new Binding("Dilatation");
				dilatationSeries.SymbolStroke = Brushes.Transparent;
				dilatationSeries.SymbolStrokeThickness = 0;

				///Create series for station
				var stationSeries = new C1.WPF.C1Chart.XYDataSeries();
				stationSeries.AxisX = "XAxis1";
				stationSeries.AxisY = "YStation";
				stationSeries.ChartType = ChartType.LineSymbols;
				stationSeries.ConnectionStroke = Brushes.Green;
				stationSeries.SymbolSize = new Size(16, 16);
				stationSeries.SymbolStyle = ss; //animation
				stationSeries.SymbolStroke = Brushes.Transparent;
				stationSeries.SymbolStrokeThickness = 0;
				stationSeries.XValueBinding = new Binding("Time");
				stationSeries.ValueBinding = new Binding("Station");
				stationSeries.PlotElementLoaded += (s, e) =>
					{
						//Customize point based in position
						var pe = (PlotElement)s;
						var dp = pe.DataPoint;
						if (!(pe is Lines))
						{
							try
							{
								//Load image from resources
								var exam = dp.DataObject as Exam;
                                pe.Fill = ImageBrushForPosition(exam.Position);
							}
							catch (Exception ex) { Debug.WriteLine(String.Format("Error loading image for position. Details: {0}", ex)); }
						}
					};

				////////////////////////////////////////////////////////////////////////////////
				// Draw the colored area that is defined by lower and upper expected dilation
				var listPopulatedExams = dataModel.Exams.Where(e => e.UpperExpectedDilatation.HasValue && e.LowerExpectedDilatation.HasValue && e.ExpectedDilatation.HasValue).ToList();

				// For the polygon.. (colored area)
				if (listPopulatedExams.Count >= 2) // Need two at least since the area is drawn using a polygon...
				{
					///Create band series to show range
					
					var bandSeries = new C1.WPF.C1Chart.XYDataSeries();
					bandSeries.AxisX = "XAxis1";
					bandSeries.AxisY = "YDilatation";
					bandSeries.ChartType = C1.WPF.C1Chart.ChartType.PolygonFilled;
                    bandSeries.ConnectionStroke = brushPolygonColor;
                    bandSeries.ConnectionStroke.Opacity = 0.10;
					bandSeries.ConnectionFill = brushPolygonColor;
                    bandSeries.ConnectionFill.Opacity=0.15;
					bandSeries.ConnectionStrokeThickness = 0.0;

					// Create values for series
					bandSeries.XValues = new DoubleCollection();
					bandSeries.Values = new DoubleCollection();

					// Populate left to right with lower...
					foreach (var item in listPopulatedExams)
					{
						bandSeries.XValues.Add(item.TimeOADate);
						bandSeries.Values.Add(item.LowerExpectedDilatation.Value);
					}

					// ... then populate from right to left (which explains the reverse) with the upper
					listPopulatedExams.Reverse();
					foreach (var item in listPopulatedExams)
					{
						bandSeries.XValues.Add(item.TimeOADate);
						bandSeries.Values.Add(item.UpperExpectedDilatation.Value);
					}

					// And finally close the polygon
					bandSeries.XValues.Add(listPopulatedExams.Last().TimeOADate);
					bandSeries.Values.Add(listPopulatedExams.Last().LowerExpectedDilatation.Value);					
                    
					// Top line of the "valid" dilatation range
					var topLimitSeries = new C1.WPF.C1Chart.XYDataSeries()
					{
						AxisX = "XAxis1",
						AxisY = "YDilatation",
						ChartType = ChartType.Line,
						ConnectionStroke = brushDilatationRange,                        
						ConnectionStrokeThickness = 1.5,
						ConnectionStrokeDashes = new DoubleCollection() { 5.0, 2.0 },
						XValues = new DoubleCollection(),
						Values = new DoubleCollection()
					};
                    topLimitSeries.ConnectionStroke.Opacity = 0.6;
					foreach (var item in listPopulatedExams)
					{
						topLimitSeries.XValues.Add(item.TimeOADate);
						topLimitSeries.Values.Add(item.UpperExpectedDilatation.Value);
					}

					// Bottom line of the "valid" dilatation range
					var bottomLimitSeries = new C1.WPF.C1Chart.XYDataSeries()
					{
						AxisX = "XAxis1",
						AxisY = "YDilatation",
						ChartType = ChartType.Line,
                        ConnectionStroke = brushDilatationRange,                        
						ConnectionStrokeThickness = 1.5,
						ConnectionStrokeDashes = new DoubleCollection() { 5.0, 2.0 },
						XValues = new DoubleCollection(),
						Values = new DoubleCollection()
					};
                    bottomLimitSeries.ConnectionStroke.Opacity = 0.6;

					foreach (var item in listPopulatedExams)
					{
						bottomLimitSeries.XValues.Add(item.TimeOADate);
						bottomLimitSeries.Values.Add(item.LowerExpectedDilatation.Value);
					}

					c1ChartCtl.Data.Children.Add(bandSeries);
					c1ChartCtl.Data.Children.Add(topLimitSeries);
					c1ChartCtl.Data.Children.Add(bottomLimitSeries);

					// Create series for expected dilatation
					if (dataModel.Display50Percentile)
					{
						var expectedDilatationSeries = new C1.WPF.C1Chart.XYDataSeries()
						{
							AxisX = "XAxis1",
							AxisY = "YDilatation",
							ChartType = ChartType.Line,
							ConnectionStroke = brushDilatationRange,
							ConnectionStrokeDashes = new DoubleCollection() { 5.0, 2.0 },
                            SymbolFill = brushDilatationRange,
							SymbolStroke = Brushes.Transparent,
							SymbolStrokeThickness = 0,
							SymbolSize = new Size(8, 8),
							SymbolStyle = ss, //animation
							XValues = new DoubleCollection(),
							Values = new DoubleCollection()
						};
                        expectedDilatationSeries.ConnectionStroke.Opacity = 0.8;
                        expectedDilatationSeries.SymbolFill.Opacity = 0.8;

						foreach (var item in listPopulatedExams)
						{
							expectedDilatationSeries.XValues.Add(item.TimeOADate);
							expectedDilatationSeries.Values.Add(item.ExpectedDilatation.Value);
						}

						c1ChartCtl.Data.Children.Add(expectedDilatationSeries);
					}
				}

				// add series data to chart
				c1ChartCtl.Data.Children.Add(dilatationSeries);
				c1ChartCtl.Data.Children.Add(stationSeries);

			}

			// Finish Update
			c1ChartCtl.EndUpdate();

			UpdateLayout();
		}
        
        /// <summary>
        /// Create grid with aspect ratio
        /// </summary>
        /// <returns></returns>
		private bool SetupChart()
		{
            try
            {                

                bool notYetInitialized = (double.IsInfinity(c1ChartCtl.View.AxisX.GetAxisRect().Width)
                                            || double.IsInfinity(c1ChartCtl.View.AxisY.GetAxisRect().Height)
                                            || double.IsNaN(c1ChartCtl.View.AxisX.GetAxisRect().Width)
                                            || double.IsNaN(c1ChartCtl.View.AxisY.GetAxisRect().Height));

                if (notYetInitialized) return false;


                // DataTemplate for DateTime in XAxis
                DataTemplate xAxisLabelsTemplate = new DataTemplate();

                // TextBlock
                xAxisLabelsTemplate.DataType = typeof(TextBlock);
                FrameworkElementFactory labelText = new FrameworkElementFactory(typeof(TextBlock));

                // Assign bindings
                System.Windows.Data.Binding converterBinding = new Binding();
                converterBinding.Converter = new TimeConverter();
                labelText.SetBinding(TextBlock.TextProperty, converterBinding);
                labelText.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                labelText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                labelText.SetValue(TextBlock.WidthProperty, 100.0);
                xAxisLabelsTemplate.VisualTree = labelText;
                xAxisLabelsTemplate.Seal();

                // Assign Template
                c1ChartCtl.View.AxisX.AnnoTemplate = xAxisLabelsTemplate;

                // Axis X - Date Formating
                c1ChartCtl.View.AxisX.MinorTickHeight = 0;
                c1ChartCtl.View.AxisX.AutoMin = false;
                c1ChartCtl.View.AxisX.AutoMax = false;

                c1ChartCtl.View.AxisX.MajorUnit = 0.04167; //1 hour -- 1day / 24 hours

                
                
                // There is an aspect ratio rule, the number of pixels for 10 hours is the same number of pixels that's used for the vertical axis (Dilatation from 0 to 10)
                var NumberOfHoursDisplayedToMaintainAspectRatio = notYetInitialized ? 10 : Math.Max(1, Math.Min(240, Math.Max(1.0, c1ChartCtl.View.AxisX.GetAxisRect().Width * 10 / c1ChartCtl.View.AxisY.GetAxisRect().Height)));                               
                Debug.WriteLine(c1ChartCtl.View.AxisX.GetAxisRect().Width + " - " + c1ChartCtl.View.AxisY.GetAxisRect().Height + " - Hours: " + NumberOfHoursDisplayedToMaintainAspectRatio);

                // If we don't have enough data for 'NumberOfHoursDisplayedToMaintainAspectRatio', we need to add a artificial points so that the chart is not zoomed to fill the space
                var dummySeries = new C1.WPF.C1Chart.XYDataSeries()
                {
                    AxisX = "XAxis1",
                    AxisY = "YDilatation",
                    ChartType = ChartType.Line,
                    ConnectionStroke = new SolidColorBrush(Colors.Red) { Opacity = 0.0 },
                    ConnectionStrokeThickness = 0.0,
                    XValues = new DoubleCollection(),
                    Values = new DoubleCollection()
                };

                DateTime startTime = (dataModel.Exams.Count == 0) ? DateTime.UtcNow.AddHours(-NumberOfHoursDisplayedToMaintainAspectRatio) : dataModel.Exams.Min(e => e.Time);                
                dummySeries.XValues.Add(startTime.ToOADate());
                dummySeries.Values.Add(0.0);
                dummySeries.XValues.Add(startTime.AddHours(NumberOfHoursDisplayedToMaintainAspectRatio).ToOADate());
                dummySeries.Values.Add(0.0);

                c1ChartCtl.Data.Children.Add(dummySeries);

                var realHoursOfData = (dataModel.Exams.Count == 0) ? 0 : (dataModel.Exams.Max(e => e.Time) - dataModel.Exams.Min(e => e.Time)).TotalHours;

                c1ChartCtl.View.AxisX.Min = startTime.ToOADate();
                c1ChartCtl.View.AxisX.Max = startTime.AddHours(Math.Max(realHoursOfData, NumberOfHoursDisplayedToMaintainAspectRatio)).ToOADate();
                c1ChartCtl.View.AxisX.Scale = (realHoursOfData <= NumberOfHoursDisplayedToMaintainAspectRatio) ? 1 : (NumberOfHoursDisplayedToMaintainAspectRatio / realHoursOfData);
                if (FirstCall)
                {
                    FirstCall = false;
                    c1ChartCtl.View.AxisX.Value = 1; // Align to the right

                    // Scroll to see the last exam
                    ScrollViewerFlowSheet.ScrollToRightEnd();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // Ignore
                return false;
            }           
		}

		#region Tooltip on points

		private void DataPoint_MouseEnter(object sender, RoutedEventArgs e)
		{
			PlotElement pe = sender as PlotElement;
			if (pe != null)
			{
				ToolTip tt = new ToolTip();
				tt.HasDropShadow = true;
				tt.BorderThickness = new Thickness(0);
				tt.Padding = new Thickness(0);
				tt.Content = pe.DataPoint.DataObject;
				tt.ContentTemplate = (DataTemplate)Resources["TemplateCurveDataToolTip"];
				pe.ToolTip = tt;
			}
		}

		private void DataPoint_MouseLeave(object sender, RoutedEventArgs e)
		{
			PlotElement pe = sender as PlotElement;
			if (pe != null)
				pe.ToolTip = null;
		}

		private void StarData_MouseEnter(object sender, RoutedEventArgs e)
		{
			Image image = sender as Image;
			if (image != null)
			{
				ToolTip tt = new ToolTip();
				tt.HasDropShadow = true;
				tt.BorderThickness = new Thickness(0);
				tt.Padding = new Thickness(0);
				tt.Content = image.Tag;
				tt.ContentTemplate = (DataTemplate)Resources["TemplateTooltipStarData"];
				image.ToolTip = tt;
			}
		}

		private void StarData_MouseLeave(object sender, RoutedEventArgs e)
		{
			Image image = sender as Image;
			if (image != null) image.ToolTip = null;
		}

		#endregion

		/// <summary>
		/// Cancel edition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			GridEdit.Visibility = System.Windows.Visibility.Collapsed;
			epiduralDateTimeCtl.DateTime = DateTime.Now;
			isEditMode = false;
			refreshTimer.Start();
		}

		/// <summary>
		/// Commit edition
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			GridEdit.Visibility = System.Windows.Visibility.Collapsed;
			//update fields
			GenerateXMLAndSendUpdate(null);
			isEditMode = false;
			refreshTimer.Start();
		}

		/// <summary>
		/// Set first exam in flowsheet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SetFirstExam(object sender, RoutedEventArgs e)
		{
			Exam exam = ((Exam)((Control)sender).Tag);

			//update first exam
			GenerateXMLAndSendUpdate(exam);
		}

		#region Edition Permissions  -- Dependency Properties

		public Boolean IsEditionAllowed
		{
			get { return (Boolean)GetValue(IsEditionAllowedProperty); }
			set { SetValue(IsEditionAllowedProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsEditionAllowed.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsEditionAllowedProperty =
			DependencyProperty.Register("IsEditionAllowed", typeof(Boolean), typeof(PageMain), new PropertyMetadata(true));


		public Boolean IsReviewModeVisible
		{
			get { return (Boolean)GetValue(IsReviewModeVisibleProperty); }
			set { SetValue(IsReviewModeVisibleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsReviewModeVisibleProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsReviewModeVisibleProperty =
			DependencyProperty.Register("IsReviewModeVisible", typeof(Boolean), typeof(PageMain), new PropertyMetadata(true));


		public Boolean IsDemoMode
		{
			get { return (Boolean)GetValue(IsDemoModeProperty); }
			set { SetValue(IsDemoModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsDemoModeProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDemoModeProperty =
			DependencyProperty.Register("IsDemoMode", typeof(Boolean), typeof(PageMain), new PropertyMetadata(false));

		public Boolean IsPoweredByLogo
		{
			get { return (Boolean)GetValue(IsPoweredByLogoProperty); }
			set { SetValue(IsPoweredByLogoProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsPoweredByLogoProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsPoweredByLogoProperty =
			DependencyProperty.Register("IsPoweredByLogo", typeof(Boolean), typeof(PageMain), new PropertyMetadata(false));


		public Boolean IsEpiduralEditionVisible
		{
			get { return (Boolean)GetValue(IsEpiduralEditionVisibleProperty); }
			set { SetValue(IsEpiduralEditionVisibleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsEpiduralEditionVisible.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsEpiduralEditionVisibleProperty =
			DependencyProperty.Register("IsEpiduralEditionVisible", typeof(Boolean), typeof(PageMain), new PropertyMetadata(true));


		public Boolean IsPreviousVaginalEditionVisible
		{
			get { return (Boolean)GetValue(IsPreviousVaginalEditionVisibleProperty); }
			set { SetValue(IsPreviousVaginalEditionVisibleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsPreviousVaginalEditionVisible.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsPreviousVaginalEditionVisibleProperty =
			DependencyProperty.Register("IsPreviousVaginalEditionVisible", typeof(Boolean), typeof(PageMain), new PropertyMetadata(true));


		public Boolean IsVBACEditionVisible
		{
			get { return (Boolean)GetValue(IsVBACEditionVisibleProperty); }
			set { SetValue(IsVBACEditionVisibleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsVBACEditionVisible.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsVBACEditionVisibleProperty =
			DependencyProperty.Register("IsVBACEditionVisible", typeof(Boolean), typeof(PageMain), new PropertyMetadata(true));


		#endregion

		/// <summary>
		/// Generate XML for update
		/// It considers if is an update for first exam
		/// or update for many fields
		/// </summary>
		/// <param name="exam">Exam object if you want to update first exam. NULL, if you want to update fields.</param>
		private void GenerateXMLAndSendUpdate(Exam exam)
		{
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				writer.WriteStartDocument();

				writer.WriteStartElement("data");

				//who is updating data and when 
				writer.WriteAttributeString("userId", QueryString[queryStringUserId]);
				writer.WriteAttributeString("userName", QueryString[queryStringUserName]);
				writer.WriteAttributeString("updateTime", DateTime.UtcNow.ToString("s"));

				writer.WriteRaw(stringLastRequest); //last request

				// Update First Exams or field values???
				if (exam != null)
				{
					//Set first exam
					writer.WriteStartElement("exam");
					writer.WriteAttributeString("dateTime", exam.Time.ToString("s"));
					writer.WriteEndElement(); //exam
				}
				else
				{
					writer.WriteStartElement("fields");

					//Epidural Date
					if (IsEpiduralEditionVisible)
					{

						bool epiduralEdited = false;
						string epiduralValue = string.Empty;
						//new values???
						if (!dataModel.Epidural.HasValue && !chkEpidural.IsChecked.Value)
						{
							epiduralEdited = false;
						}
						else
						{
							//we have values but now we remove it
							if (dataModel.Epidural.HasValue && !chkEpidural.IsChecked.Value)
							{
								//put epidural = empty
								epiduralEdited = true;
								epiduralValue = string.Empty;
							}
							else
							{
								//we did not have values and now we have 
								if (!dataModel.Epidural.HasValue && chkEpidural.IsChecked.Value)
								{
									//put epidural current value
									epiduralEdited = true;
									epiduralValue = epiduralDateTimeCtl.DateTime.Value.ToUniversalTime().ToString("s");
								}
								else
								{
									//both have values, check if they are the same
									if (dataModel.Epidural.Value != epiduralDateTimeCtl.DateTime.Value.ToUniversalTime())
									{
										//put new value in epidural
										epiduralEdited = true;
										epiduralValue = epiduralDateTimeCtl.DateTime.Value.ToUniversalTime().ToString("s");
									}
									else
									{
										//Do no edit
										epiduralEdited = false;
									}
								}
							}

						}
						//edited??
						if (epiduralEdited)
						{
							writer.WriteStartElement("field");
							writer.WriteAttributeString("name", Common.XML_epidural_field);
							writer.WriteAttributeString("value", epiduralValue);
							writer.WriteEndElement();
						}
					}


					#region Vaginal Deliveries

					//Vaginal deliveries
					if (IsPreviousVaginalEditionVisible)
					{
						///Verify is edited
						///Load unedited values
						int VDPrevious = 0; //NO Values
						if (dataModel.Vaginal.HasValue)
						{
							if (!dataModel.Vaginal.Value)
							{
								VDPrevious = 1; //Vaginal delivery
							}
							else
							{
								VDPrevious = 2;  //NO Vaginal delivery
							}
						}

						///load Edited values if there are any
						int VDPreviousEdited = 0; //No Values
						if (radioVaginalDeliveryYes.IsChecked.HasValue && radioVaginalDeliveryNO.IsChecked.HasValue)
						{
							if (radioVaginalDeliveryYes.IsChecked.Value == false && radioVaginalDeliveryNO.IsChecked.Value == false)
							{
								VDPreviousEdited = 0; //No Values
							}
							else
							{
								VDPreviousEdited = radioVaginalDeliveryYes.IsChecked.Value ? 1 : 2;
							}
						}

						///Changed??
						if (VDPrevious != VDPreviousEdited)
						{
							writer.WriteStartElement("field");
							writer.WriteAttributeString("name", Common.XML_previous_vaginal_deliveries_field);

							if (VDPreviousEdited == 0)
							{
								writer.WriteAttributeString("value", string.Empty);
							}
							else
							{
								///Original values answer the question: Have previous vaginal deliveries?
								///But the question was changed to: First vaginal delivery?, so the result
								///must be the opposite to keep compatibility with the calculation mode.						
								bool value = !radioVaginalDeliveryYes.IsChecked.Value;
								writer.WriteAttributeString("value", value.ToString());
							}
							writer.WriteEndElement();
						}

					}
					#endregion

					#region VBAC

					//VBAC
					if (IsVBACEditionVisible)
					{

						///Verify is edited
						///Load unedited values
						int VBACPrevious = 0; //NO Values
						if (dataModel.VBAC.HasValue)
						{
							if (dataModel.VBAC.Value)
							{
								VBACPrevious = 1; //VBAC
							}
							else
							{
								VBACPrevious = 2;  //NOVBAC
							}
						}

						///load Edited values if there are any
						int VBACEdited = 0; //No Values
						if (radioVBACYes.IsChecked.HasValue && radioVBACNO.IsChecked.HasValue)
						{
							if (radioVBACYes.IsChecked.Value == false && radioVBACNO.IsChecked.Value == false)
							{
								VBACEdited = 0; //No Values
							}
							else
							{
								VBACEdited = radioVBACYes.IsChecked.Value ? 1 : 2; //1-VBAC---2-NOVBAC
							}
						}

						///Changed??
						if (VBACPrevious != VBACEdited)
						{
							writer.WriteStartElement("field");
							writer.WriteAttributeString("name", Common.XML_vbac_field);

							if (VBACEdited == 0)
							{
								writer.WriteAttributeString("value", string.Empty);
							}
							else
							{
								writer.WriteAttributeString("value", (VBACEdited == 1) ? true.ToString() : false.ToString());
							}
							writer.WriteEndElement();
						}
					}
					#endregion

					writer.WriteEndElement();//fields
				}

				writer.WriteEndElement();//data
				writer.WriteEndDocument();
				writer.Flush();
			}

			//encode data
			var data = System.Web.HttpUtility.UrlEncode(UTF8Encoding.UTF8.GetBytes(Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(sb.ToString()))));

			//send to server and update
			RefreshCurve(data);
		}

		/// <summary>
		/// Handle the event when user do rightclick on header to set first exam and is not allowed editing or in review mode
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void header_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (!IsEditionAllowed) e.Handled = true;
        }

        private void Navigate_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        #region Utils


        Dictionary<Exam.FetalPositionEnum, ImageBrush> imageBrushPositions = null;

        public ImageBrush ImageBrushForPosition(Exam.FetalPositionEnum position)
        {
            //if it is not defined, load types
            if (imageBrushPositions == null)
            {
                String imageName;
                imageBrushPositions = new Dictionary<Exam.FetalPositionEnum, ImageBrush>();
                foreach (string item in Enum.GetNames(typeof(Exam.FetalPositionEnum)))
                {

                    if (item.ToLowerInvariant() == "unknown")
                    {
                        imageName = @"/images/NoStation.png";
                    }
                    else
                    {
                        imageName = @"/images/" + item + ".png";
                    }
                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imageName));
                    imageBrushPositions.Add((Exam.FetalPositionEnum)Enum.Parse(typeof(Exam.FetalPositionEnum), item), imgBrush);
                }
            }

            return imageBrushPositions[position];
        }

        #endregion
    }
}
