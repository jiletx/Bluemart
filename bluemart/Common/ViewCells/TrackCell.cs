﻿using System;
using Xamarin.Forms;
using bluemart.Common.Utilities;
using bluemart.Models.Local;
using System.Threading.Tasks;
using bluemart.MainViews;
using FFImageLoading.Forms;
using bluemart.Models.Remote;

namespace bluemart.Common.ViewCells
{
	public class TrackCell : ViewCell
	{
		public Label mTotalPriceLabel;
		public Label mDateLabel;
		public Label mRegionLabel;
		public Label mStatusLabel;
		private TrackPage mRootPage;
		private StatusClass mStatus;

		public TrackCell (StatusClass status, TrackPage rootPage)
		{
			mRootPage = rootPage;
			mStatus = status;



			var mainLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize (600),
				HeightRequest = MyDevice.GetScaledSize (180),
				BackgroundColor = Color.White,
				Padding = 0
			};					

			var backgroundImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize (62),
				HeightRequest = MyDevice.GetScaledSize (82),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false,
				Source = "TrackPage_TrackBackground_Red.png"
			};

			var totalPriceLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (455),
				HeightRequest = MyDevice.GetScaledSize (26),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb (98, 98, 98),
				Text = "Total Price: " + status.TotalPrice + " AED",
				FontSize = MyDevice.FontSizeMicro	
			};

			DateTime statusDate = DateTime.Now;

			DateTime.TryParse(status.Date,out statusDate);				
			DateTime addedStatusDate =statusDate.AddHours (2);
			var dateLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (220),
				HeightRequest = MyDevice.GetScaledSize (26),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb (98, 98, 98),
				Text = "Date: " + addedStatusDate.ToString ("MM/dd/yyyy") + " - Time: ",
				FontSize = MyDevice.FontSizeMicro	
			};

			var timeLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (80),
				HeightRequest = MyDevice.GetScaledSize (26),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = MyDevice.RedColor,
				Text = addedStatusDate.ToString ("hh:mm:ss"),
				FontSize = MyDevice.FontSizeMicro
			};

			var regionLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (455),
				HeightRequest = MyDevice.GetScaledSize (26),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb (98, 98, 98),
				Text = "Region: " + status.Region,
				FontSize = MyDevice.FontSizeMicro	
			};

			var statusLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (455),
				HeightRequest = MyDevice.GetScaledSize (60),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb (98, 98, 98),
				Text = status.Status,
				FontSize = MyDevice.FontSizeMicro	
			};

			var line = new BoxView () {
				WidthRequest = MyDevice.GetScaledSize (600),
				HeightRequest = MyDevice.GetScaledSize (1),
				Color = Color.FromRgb (181, 185, 187)
			};



			mainLayout.Children.Add (totalPriceLabel,
				Constraint.Constant (MyDevice.GetScaledSize (140)),
				Constraint.Constant (MyDevice.GetScaledSize (26))
			);

			mainLayout.Children.Add (dateLabel,
				Constraint.RelativeToView (totalPriceLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (totalPriceLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (2);	
				})
			);

			mainLayout.Children.Add (timeLabel,
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize(2);	
				}),
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			mainLayout.Children.Add (regionLabel,
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (2);	
				})
			);

			mainLayout.Children.Add (statusLabel,
				Constraint.RelativeToView (regionLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (regionLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (2);	
				})
			);

			if (status.OrderStatus == OrderModel.OrderStatus.WAITING_CONFIRMATION) {
				backgroundImage.Source = "TrackPage_TrackBackground_Red.png";
				mainLayout.Children.Add (backgroundImage,
					Constraint.Constant (MyDevice.GetScaledSize (42)),
					Constraint.Constant (MyDevice.GetScaledSize (42))
				);

				mainLayout.Children.Add (line,
					Constraint.Constant (0),
					Constraint.Constant (MyDevice.GetScaledSize (179))
				);
			}
			else if(status.OrderStatus == OrderModel.OrderStatus.CONFIRMED){
				backgroundImage.Source = "TrackPage_TrackBackground_Blue.png";
				mainLayout.Children.Add (backgroundImage,
					Constraint.Constant (MyDevice.GetScaledSize (42)),
					Constraint.Constant (MyDevice.GetScaledSize (42))
				);

				mainLayout.Children.Add (line,
					Constraint.Constant (0),
					Constraint.Constant (MyDevice.GetScaledSize (179))
				);
			}
			else if (status.OrderStatus == OrderModel.OrderStatus.IN_TRANSIT) {				
				mainLayout.HeightRequest = MyDevice.GetScaledSize (232);
				mainLayout.Children.Add (line,
					Constraint.Constant (0),
					Constraint.Constant (MyDevice.GetScaledSize (231))
				);

				var deliveryStaffNameLabel = new Label () {
					WidthRequest = MyDevice.GetScaledSize (455),
					HeightRequest = MyDevice.GetScaledSize (26),
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Start,
					TextColor = Color.FromRgb (98, 98, 98),
					Text = "Staff Name: " + status.DeliveryStaffName,
					FontSize = MyDevice.FontSizeMicro	
				};

				var deliveryStaffPhoneLabel = new Label () {
					WidthRequest = MyDevice.GetScaledSize (455),
					HeightRequest = MyDevice.GetScaledSize (26),
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Start,
					TextColor = Color.FromRgb (98, 98, 98),
					Text = "Staff Phone: " + status.Phone,
					FontSize = MyDevice.FontSizeMicro	
				};

				mainLayout.Children.Add (deliveryStaffNameLabel,
					Constraint.RelativeToView (statusLabel, (p, sibling) => {
						return sibling.Bounds.Left;	
					}),
					Constraint.RelativeToView (statusLabel, (p, sibling) => {
						return sibling.Bounds.Bottom + MyDevice.GetScaledSize (2);	
					})
				);

				mainLayout.Children.Add (deliveryStaffPhoneLabel,
					Constraint.RelativeToView (deliveryStaffNameLabel, (p, sibling) => {
						return sibling.Bounds.Left;	
					}),
					Constraint.RelativeToView (deliveryStaffNameLabel, (p, sibling) => {
						return sibling.Bounds.Bottom + MyDevice.GetScaledSize (2);	
					})
				);

				backgroundImage.Source = "TrackPage_TrackBackground_Green.png";
				mainLayout.Children.Add (backgroundImage,
					Constraint.Constant (MyDevice.GetScaledSize (42)),
					Constraint.Constant (MyDevice.GetScaledSize (70))
				);

			} 

			var tapGestureRecognizer = new TapGestureRecognizer ();
			tapGestureRecognizer.Tapped += (sender, e) => {
				mRootPage.mParent.LoadReceiptPage (mStatus);
			};
			mainLayout.GestureRecognizers.Add (tapGestureRecognizer);

			this.View = mainLayout;
		}
	}
}

