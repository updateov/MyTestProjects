using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CustomItemsPanel
{
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
		}
	}

	public static class RobotImageLoader
	{
		public static List<BitmapImage> LoadImages()
		{
			List<BitmapImage> robotImages = new List<BitmapImage>();
			DirectoryInfo robotImageDir = new DirectoryInfo( @"..\..\Robots" );
			foreach( FileInfo robotImageFile in robotImageDir.GetFiles( "*.jpg" ) )
			{
				Uri uri = new Uri( robotImageFile.FullName );
				robotImages.Add( new BitmapImage( uri ) );
			}
			return robotImages;
		}
	}
}