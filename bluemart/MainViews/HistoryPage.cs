﻿using System;
using System.Collections.Generic;
using bluemart.Common.Objects;
using bluemart.Common.Utilities;
using bluemart.Common.ViewCells;
using Xamarin.Forms;
using bluemart.Models.Remote;
using bluemart.Models.Local;

namespace bluemart.MainViews
{
	public partial class HistoryPage : ContentPage
	{				
		public RootPage mParent;
		public HistoryCell mActiveHistoryCell;

		public HistoryPage (RootPage parent)
		{						
			InitializeComponent ();
			mParent = parent;
			MainStackLayout.Spacing = MyDevice.ViewPadding;
			SetGrid1Definitions ();
		}

		public void PopulateListView()
		{
			MainStackLayout.Children.Clear ();
			var orderHistoryList = OrderModel.GetOrdersForHistory ();
			foreach (var history in orderHistoryList) {
				//MainStackLayout.Children.Add( new HistoryCell(history,this ).View );
			}
		}

		private void SetGrid1Definitions()
		{
			Grid1.RowDefinitions [0].Height = GridLength.Auto;
			Grid1.ColumnDefinitions [0].Width = MyDevice.ScreenWidth;
			Grid1.BackgroundColor = MyDevice.BlueColor;
		}
			
	}
}

