// -----------------------------------------------------------------------
// <copyright file="ViewModelBase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace WPF_Triggers.ViewModel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.ComponentModel;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void RaisePropertyChanged(string propertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
