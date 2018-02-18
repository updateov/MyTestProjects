// -----------------------------------------------------------------------
// <copyright file="ViewItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace WPF_Triggers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Media;
	using WPF_Triggers.ViewModel;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class ViewItem : ViewModelBase
	{
		private string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				RaisePropertyChanged("Title");
			}
		}

		private ImageSource _picture;
		public ImageSource Picture
		{
			get { return _picture; }
			set
			{
				_picture = value;
				RaisePropertyChanged("Picture");
			}
		}		
	}
}
