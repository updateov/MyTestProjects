// -----------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace WPF_Triggers.ViewModel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Collections.ObjectModel;
	using System.Windows.Media;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			ViewItemList = new ObservableCollection<ViewItem>();

			ImageSourceConverter converter = new ImageSourceConverter();
			string commonPath = "pack://application:,,/Images";

			ImageSource imageSource = (ImageSource)converter.ConvertFromString(commonPath + "/Penguins.jpg");
			ViewItemList.Add(new ViewItem() { Title = "Penguins", Picture = imageSource });
			imageSource = (ImageSource)converter.ConvertFromString(commonPath + "/Tulips.jpg");
			ViewItemList.Add(new ViewItem() { Title = "Tulips", Picture = imageSource });
			ViewItemList.Add(new ViewItem() { Title = "Waterfall" });
		}

		private ObservableCollection<ViewItem> _viewItemList;
		public ObservableCollection<ViewItem> ViewItemList
		{
			get { return _viewItemList; }
			set
			{
				_viewItemList = value;
				RaisePropertyChanged("ViewItemList");
			}
		}

	}
}
