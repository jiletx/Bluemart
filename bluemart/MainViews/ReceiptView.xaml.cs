﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;
using bluemart.Common.Utilities;
using bluemart.Common.Objects;
using bluemart.Models.Local;
using bluemart.MainViews;
using bluemart.Models.Remote;
using bluemart.Common.ViewCells;
using FFImageLoading.Forms;

namespace bluemart
{
	public partial class ReceiptView : ContentPage
	{
		private UserClass mUserModel = new UserClass();
		private AddressClass mAddressModel = new AddressClass();
		private RootPage mParent;
		private Object mObject = null;

		private RelativeLayout mTopLayout;
		private RelativeLayout mMenuLayout;
		private RelativeLayout mMidLayout;
		private Label categoriesLabel;
		private Image menuIcon;
		private bool IsMenuOpen = false;
		private double mMenuWidth = 517.0;

		private Image mAddressLayout;
		private Image mReceiptLayout;

		private RelativeLayout InputBlockerForSwipeMenu;

		public ReceiptView (RootPage parent)
		{			
			InitializeComponent ();

			mParent = parent;
			mUserModel = mUserModel.GetUser ();
			NavigationPage.SetHasNavigationBar (this, false);

			InitializeLayout ();
		}



		public ReceiptView (RootPage parent, Object obj)
		{					
			InitializeComponent ();
			mParent = parent;
			mObject = obj;
			mUserModel = mUserModel.GetUser ();
			NavigationPage.SetHasNavigationBar (this, false);
			InitializeLayout (obj);
		}

		private void InitializeLayout(Object obj=null)
		{	
			mainRelativeLayout.BackgroundColor = Color.FromRgb (236, 240, 241);
			mMidLayout = new RelativeLayout ();

			Point totalDistance = new Point(0,0);

			mainRelativeLayout.Panning += (object sender, MR.Gestures.PanEventArgs e) => {
				totalDistance = e.TotalDistance;
			};

			mainRelativeLayout.Swiped += (object sender, MR.Gestures.SwipeEventArgs e) => {
				if(e.Direction == MR.Gestures.Direction.Left)
				{
					if(IsMenuOpen)
						ActivateOrDeactivateMenu();					
				}
				else if( e.Direction == MR.Gestures.Direction.Right)
				{
					if(!IsMenuOpen)
						ActivateOrDeactivateMenu();
				}
				else if( totalDistance.X != 0 && e.Direction == MR.Gestures.Direction.NotClear)
				{
					if( totalDistance.X < - MyDevice.SwipeDistance )
					{
						if(IsMenuOpen)
							ActivateOrDeactivateMenu();
					}
					else if( totalDistance.X > MyDevice.SwipeDistance )
					{
						if(!IsMenuOpen)
							ActivateOrDeactivateMenu();
					}
				}
			};

			mainRelativeLayout.Children.Add (mMidLayout,
				Constraint.Constant (0),
				Constraint.Constant (0)
			);

			InputBlockerForSwipeMenu = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(123),
				HeightRequest = MyDevice.ScreenHeight,
				Padding = 0
			};

			InitializeHeaderLayout ();
			InitializeMenuLayout ();
			InitializeAddressLayout (obj);
			InitializeReceiptLayout (obj);
		}

		private void InitializeHeaderLayout ()
		{
			mTopLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(640),
				HeightRequest = MyDevice.GetScaledSize(87),
				BackgroundColor = Color.White
			};

			menuIcon = new Image () {
				WidthRequest = MyDevice.GetScaledSize(36),
				HeightRequest = MyDevice.GetScaledSize(37),
				Source = "ReceiptPage_MenuIcon.png"
			};

			var logo = new Image () {
				WidthRequest = MyDevice.GetScaledSize(217),
				HeightRequest = MyDevice.GetScaledSize(39),
				Source = "ReceiptPage_Logo.png"
			};
												
			var menuButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(72),
				HeightRequest = MyDevice.GetScaledSize(86)
			};

			var menuTapRecognizer= new TapGestureRecognizer ();
			menuTapRecognizer.Tapped += (sender, e) => {				
				ActivateOrDeactivateMenu();
			};
			menuButton.GestureRecognizers.Add(menuTapRecognizer);



			mMidLayout.Children.Add (mTopLayout,
				Constraint.Constant (0),
				Constraint.Constant (0)
			);

			mMidLayout.Children.Add (menuIcon, 
				Constraint.RelativeToParent (parent => {
					return parent.Bounds.Left +  MyDevice.GetScaledSize(20);
				}),
				Constraint.RelativeToParent (parent => {
					return parent.Bounds.Top + MyDevice.GetScaledSize(27);
				})
			);	

			mMidLayout.Children.Add (logo,
				Constraint.RelativeToView (menuIcon, (p, sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (150);	
				}),
				Constraint.RelativeToView (menuIcon, (p, sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(5);	
				})
			);

			mMidLayout.Children.Add (menuButton,
				Constraint.Constant (0),
				Constraint.Constant (0));

		}

		private void InitializeMenuLayout()
		{
			mMenuLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(mMenuWidth),
				HeightRequest = MyDevice.ScreenHeight,
				BackgroundColor = Color.FromRgb(51,51,51)
			};

			var openImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(54),
				HeightRequest = MyDevice.GetScaledSize(44),
				Source = "MenuPage_Open.png"
			};

			categoriesLabel = new Label () {
				Text = "Categories",
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(400),
				HeightRequest = MyDevice.GetScaledSize(44)
			};

			var categoriesButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(512),
				HeightRequest = MyDevice.GetScaledSize(50)
			};

			var firstLine = new BoxView (){
				HeightRequest = 1,
				WidthRequest = MyDevice.GetScaledSize(mMenuWidth),
				Color = Color.FromRgb(129,129,129)
			};

			var settingsImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Settings.png",
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false
			};

			var settingsLabel = new Label () {
				Text = "My Profile",
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(400),
				HeightRequest = MyDevice.GetScaledSize(44)
			};

			var settingsButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(512),
				HeightRequest = MyDevice.GetScaledSize(50)
			};

			var favoritesImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Favorites.png",
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false
			};

			var favoritesLabel = new Label () {
				Text = "Favorites",
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(400),
				HeightRequest = MyDevice.GetScaledSize(44)
			};

			var favoritesButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(512),
				HeightRequest = MyDevice.GetScaledSize(50)
			};

			var trackImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Track.png",
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false
			};

			var trackLabel = new Label () {
				Text = "Track Your Order",
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(400),
				HeightRequest = MyDevice.GetScaledSize(44)
			};

			var trackButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(512),
				HeightRequest = MyDevice.GetScaledSize(50),
				Padding = 0
			};

			var trackTapRecognizer = new TapGestureRecognizer ();
			trackTapRecognizer.Tapped += (sender, e) => {
				mParent.LoadTrackPage();
			};
			trackButton.GestureRecognizers.Add (trackTapRecognizer);

			mMenuLayout.Children.Add (trackButton,
				Constraint.Constant(0),
				Constraint.RelativeToView (trackImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(3);
				})
			);

			var secondLine = new BoxView (){
				HeightRequest = 1,
				WidthRequest = MyDevice.GetScaledSize(mMenuWidth),
				Color = Color.FromRgb(129,129,129)
			};



			var categoryNameStackLayout = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Padding = 0,
				Spacing = 0
			};

			for (int i = 0; i < mParent.mBrowseCategoriesPage.mCategories.Count; i++) {
				if (!mParent.mBrowseCategoriesPage.mCategories [i].IsSubCategory) {
					Label label = new Label () {
						WidthRequest = MyDevice.GetScaledSize(442),	
						HeightRequest = MyDevice.GetScaledSize(78),
						TextColor = Color.White,
						HorizontalTextAlignment = TextAlignment.Start,
						VerticalTextAlignment = TextAlignment.Center,
						Text = mParent.mBrowseCategoriesPage.mCategories [i].Name,
						FontSize = MyDevice.FontSizeMedium
					};

					var tapRecog = new TapGestureRecognizer ();
					tapRecog.Tapped += async(sender, e) => {
						string categoryName = (sender as Label).Text;
						Category category = null;
						foreach(var tempCategory in mParent.mBrowseCategoriesPage.mCategories)
						{
							if(tempCategory.Name == categoryName)
							{
								category = tempCategory;
							}
						}
						if(category.CategoryID == ReleaseConfig.TOBACCO_ID)
						{					
							var isOk = await mParent.DisplayAlert("Warning","I am over 20 years old and I know smoking is bad for my health.","AGREE","DISAGREE");
							if(isOk)
							{
								IsMenuOpen = false;
								MyDevice.rootPage.LoadCategory(category);
							}										
						}else if(category.CategoryID == ReleaseConfig.FRUITS_ID||category.CategoryID == ReleaseConfig.MEAT_ID)
						{					
							await mParent.DisplayAlert("Please Remember","Delivered quantity might differ from the actual ordered quantity by ± 50 grams.","OK");
							IsMenuOpen = false;
							MyDevice.rootPage.LoadCategory(category);										
						}
						else
						{
							IsMenuOpen = false;
							MyDevice.rootPage.LoadCategory(category);
						}
						/*
						foreach(var categoryCell in mParent.mBrowseCategoriesPage.mCategoryCellList)
						{
							if( category != null && categoryCell.mCategory == category )
							{
								IsMenuOpen = false;
								categoryCell.LoadProductsPage(category.CategoryID,mParent);
							}
						}*/

					};

					label.GestureRecognizers.Add (tapRecog);
					categoryNameStackLayout.Children.Add (label);	
				}
			}

			var categoryNameScrollView = new ScrollView {
				Orientation = ScrollOrientation.Vertical,
				Content = categoryNameStackLayout
			};

			var categoriesTapRecognizer = new TapGestureRecognizer ();
			categoriesTapRecognizer.Tapped += (sender, e) => {
				mParent.SwitchTab("BrowseCategories");
			};
			categoriesButton.GestureRecognizers.Add (categoriesTapRecognizer);

			var favoritesTapRecognizer = new TapGestureRecognizer ();
			favoritesTapRecognizer.Tapped += (sender, e) => {
				mParent.LoadFavoritesPage();
			};
			favoritesButton.GestureRecognizers.Add (favoritesTapRecognizer);

			var settingsTapRecognizer = new TapGestureRecognizer ();
			settingsTapRecognizer.Tapped += (sender, e) => {
				mParent.LoadSettingsPage();
			};
			settingsButton.GestureRecognizers.Add (settingsTapRecognizer);

			mainRelativeLayout.Children.Add (mMenuLayout,
				Constraint.Constant (MyDevice.GetScaledSize (mMenuWidth) * -1),
				Constraint.Constant (0)
			);

			mMenuLayout.Children.Add (openImage,				
				Constraint.Constant(MyDevice.GetScaledSize(16)),
				Constraint.RelativeToParent (parent => {
					return parent.Bounds.Top + MyDevice.GetScaledSize (20.59f);
				})
			);

			mMenuLayout.Children.Add (categoriesLabel,
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (10);
				}),
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Top;
				})
			);

			mMenuLayout.Children.Add (firstLine,
				Constraint.Constant(0),
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(22);
				})
			);

			mMenuLayout.Children.Add (settingsImage,
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize (9);
				}),
				Constraint.RelativeToView (firstLine, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (25);
				})
			);

			mMenuLayout.Children.Add (settingsLabel,
				Constraint.RelativeToView (settingsImage, (parent,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (15);
				}),
				Constraint.RelativeToView (settingsImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(4);
				})
			);

			mMenuLayout.Children.Add (favoritesImage,
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize (9);
				}),
				Constraint.RelativeToView (settingsImage, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (46);
				})
			);

			mMenuLayout.Children.Add (favoritesLabel,
				Constraint.RelativeToView (favoritesImage, (parent,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (15);
				}),
				Constraint.RelativeToView (favoritesImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(4);
				})
			);

			mMenuLayout.Children.Add (trackImage,
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize (9);
				}),
				Constraint.RelativeToView (favoritesImage, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (46);
				})
			);

			mMenuLayout.Children.Add (trackLabel,
				Constraint.RelativeToView (trackImage, (parent,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (15);
				}),
				Constraint.RelativeToView (trackImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(4);
				})
			);

			mMenuLayout.Children.Add (secondLine,
				Constraint.Constant(MyDevice.GetScaledSize(0)),
				Constraint.RelativeToView (trackImage, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(22);
				})
			);

			mMenuLayout.Children.Add (categoryNameScrollView,
				Constraint.Constant(MyDevice.GetScaledSize(76)),
				Constraint.RelativeToView (secondLine, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(22);
				}),
				Constraint.Constant(MyDevice.GetScaledSize(440)),
				Constraint.Constant(MyDevice.ScreenHeight - MyDevice.GetScaledSize(445))
			);

			mMenuLayout.Children.Add (categoriesButton,
				Constraint.Constant(0),
				Constraint.RelativeToView (openImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(3);
				})
			);

			mMenuLayout.Children.Add (settingsButton,
				Constraint.Constant(0),
				Constraint.RelativeToView (settingsImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(3);
				})
			);

			mMenuLayout.Children.Add (favoritesButton,
				Constraint.Constant(0),
				Constraint.RelativeToView (favoritesImage, (parent,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(3);
				})
			);
		}

		private void InitializeAddressLayout(Object obj = null){
			mAddressLayout = new Image () {
				WidthRequest = MyDevice.GetScaledSize(600),
				HeightRequest = MyDevice.GetScaledSize(301),
				Source = "ReceiptPage_AddressBackground.png"
			};

			var informationLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (352),
				HeightRequest = MyDevice.GetScaledSize(52),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(228,69,73),
				Text = "Information On System",
				FontSize = MyDevice.FontSizeSmall
			};

			var dateLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (509),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "",
				FontSize = MyDevice.FontSizeSmall
			};

			var nameLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (509),
				HeightRequest = MyDevice.GetScaledSize(28),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "Please Add An Adress For Order",
				FontSize = MyDevice.FontSizeSmall
			};

			var addressLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (509),
				HeightRequest = MyDevice.GetScaledSize(28),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "",
				FontSize = MyDevice.FontSizeSmall
			};

			var phoneLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (509),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "",
				FontSize = MyDevice.FontSizeSmall
			};

			var changeAddressButton = new Label () {
				WidthRequest = MyDevice.GetScaledSize (194),
				HeightRequest = MyDevice.GetScaledSize(54),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				Text = "CHANGE ADDRESS",
				FontSize = MyDevice.FontSizeMicro
			};

			var changeAddressTapRecogniser = new TapGestureRecognizer ();

			changeAddressTapRecogniser.Tapped += (sender, e) => {
				mParent.LoadSettingsPage();
			};
			changeAddressButton.GestureRecognizers.Add (changeAddressTapRecogniser);

			if (obj == null) {
				dateLabel.Text = DateTime.Now.ToString ("MM/dd/yyyy");
				AddressClass address = mAddressModel.GetActiveAddress (mUserModel.ActiveRegion);
				if (address != null) {
					nameLabel.Text = address.Name;
					addressLabel.Text = address.Address;
					phoneLabel.Text = address.PhoneNumber;
				}
			} else {
				if (obj is HistoryClass) {
					HistoryClass history = obj as HistoryClass;
					DateTime historyDate = DateTime.Now;
					DateTime.TryParse(history.Date,out historyDate);					
					dateLabel.Text = historyDate.ToString("MM/dd/yyyy");
					nameLabel.Text = history.Name + " " + history.Surname;
					addressLabel.Text = history.Address;
					phoneLabel.Text = history.Phone;
				} else if (obj is StatusClass) {
					StatusClass status = obj as StatusClass;
					DateTime statusDate = DateTime.Now;
					DateTime.TryParse(status.Date,out statusDate);
					dateLabel.Text = statusDate.ToString("MM/dd/yyyy");
					nameLabel.Text = status.Name + " " + status.Surname;
					addressLabel.Text = status.Address;
					phoneLabel.Text = status.Phone;
				}
			}

			mMidLayout.Children.Add (mAddressLayout,
				Constraint.RelativeToView (menuIcon, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (mTopLayout, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(25);	
				})
			);

			mMidLayout.Children.Add (informationLabel,
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize(67);	
				}),
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Top + MyDevice.GetScaledSize(12);	
				})
			);

			mMidLayout.Children.Add (dateLabel,
				Constraint.RelativeToView (informationLabel, (p, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize(5);	
				}),
				Constraint.RelativeToView (informationLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(18);	
				})
			);

			mMidLayout.Children.Add (nameLabel,
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (dateLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(4);	
				})
			);

			mMidLayout.Children.Add (addressLabel,
				Constraint.RelativeToView (nameLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (nameLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(4);	
				})
			);

			mMidLayout.Children.Add (phoneLabel,
				Constraint.RelativeToView (addressLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (addressLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(4);	
				})
			);

			mMidLayout.Children.Add (changeAddressButton,
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Right - MyDevice.GetScaledSize(222);	
				}),
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Bottom - MyDevice.GetScaledSize(79);	
				})
			);
		}

		private void InitializeReceiptLayout(Object obj = null){
			mReceiptLayout = new Image () {
				WidthRequest = MyDevice.GetScaledSize(600),
				HeightRequest = MyDevice.GetScaledSize(477),
				Source = "ReceiptPage_ReceiptBackground.png"
			};

			var quantityLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (109),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "Quantity",
				FontSize = MyDevice.FontSizeMicro
			};

			var productLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (187),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "Product",
				FontSize = MyDevice.FontSizeMicro
			};

			var descriptionLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (169),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "Description",
				FontSize = MyDevice.FontSizeMicro
			};

			var costLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (117),
				HeightRequest = MyDevice.GetScaledSize(25),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "Cost (AED)",
				FontSize = MyDevice.FontSizeMicro
			};

			var receiptStackLayout = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Padding = 0,
				Spacing = 0
			};	

			var totalAmountLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (259),
				HeightRequest = MyDevice.GetScaledSize(32),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = "TOTAL AMOUNT: ",
				FontSize = MyDevice.FontSizeSmall
			};



			var totalAmountPriceLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (117),
				HeightRequest = MyDevice.GetScaledSize(32),
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(98,98,98),
				Text = Cart.ProductTotalPrice.ToString(),
				FontSize = MyDevice.FontSizeSmall
			};

			if (obj == null) {
				totalAmountPriceLabel.Text = Cart.ProductTotalPrice.ToString ();
			} else {
				if (obj is StatusClass) {
					totalAmountPriceLabel.Text = (obj as StatusClass).TotalPrice.ToString ();
				} else if (obj is HistoryClass) {
					totalAmountPriceLabel.Text = (obj as HistoryClass).TotalPrice.ToString ();
				}
			}

			var agreeLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (168),
				HeightRequest = MyDevice.GetScaledSize(46),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				Text = "AGREE",
				FontSize = MyDevice.FontSizeSmall
			};

			var disagreeLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (173),
				HeightRequest = MyDevice.GetScaledSize(46),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				Text = "CANCEL",
				FontSize = MyDevice.FontSizeSmall
			};

			if (mObject != null) {
				agreeLabel.Text = "OK";
				disagreeLabel.Text = "";
				disagreeLabel.BackgroundColor = Color.White;
			} 

			var thankyouLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (589),
				HeightRequest = MyDevice.GetScaledSize(57),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.FromRgb(192,192,192),
				Text = "THANK YOU\nFOR SHOPPING WITH US",
				FontSize = MyDevice.FontSizeSmall
			};

			var rightsReservedLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize (589),
				HeightRequest = MyDevice.GetScaledSize(24),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb(192,192,192),
				Text = "ALL RIGHTS RESERVED TO BLUEMART",
				FontSize = MyDevice.FontSizeMicro
			};

			var agreeTapRecognizer = new TapGestureRecognizer ();
			agreeTapRecognizer.Tapped += async (sender, e) => {
				if (mObject == null) {
					if (MyDevice.GetNetworkStatus () != "NotReachable") {
						if(mAddressModel.GetActiveAddress (mUserModel.ActiveRegion)==null)
						{
							await DisplayAlert ("No Address", "Please Add An Address.", "OK");
						}
						else
						{
							bool OrderSucceeded = OrderModel.SendOrderToRemote (mUserModel).Result;

							if (OrderSucceeded)
							{
								await DisplayAlert ("Order Sent", "Please check the quality of the fresh produce you receive before you pay to the delivery boy. Your satisfaction is our top priority at all times.", "OK");
								//mParent.mBrowseCategoriesPage.CartStackLayout.Children.Clear();
								Cart.ProductTotalPrice = 0;
								Cart.ProductsInCart.Clear();
								mParent.LoadTrackPage();
							}
							else
							{
								await DisplayAlert ("Connection Error", "Your order couldn't be delivered. Check your internet connection and try again.", "OK");
								mParent.SwitchTab ("BrowseCategories");
							}																				
						}
					} else {
						await DisplayAlert ("Connection Error", "Your order couldn't be delivered. Check your internet connection and try again.", "OK");
					}
				} else {
					if (mObject is HistoryClass) {
						//mParent.mFooter.ChangeColorOfLabel (mParent.mFooter.mCartLabel);
						mParent.LoadTrackPage ();
					} else if (mObject is StatusClass) {
						//mParent.mFooter.ChangeColorOfLabel (mParent.mFooter.mTrackLabel);
						//mParent.SwitchTab ("Track");
						mParent.LoadTrackPage ();
					}
				}
			};
			agreeLabel.GestureRecognizers.Add (agreeTapRecognizer);

			var disagreeTapRecognizer = new TapGestureRecognizer ();
			disagreeTapRecognizer.Tapped += (sender, e) => {
				if (mObject == null) {
					mParent.SwitchTab ("BrowseCategories");
				}
			};
			disagreeLabel.GestureRecognizers.Add (disagreeTapRecognizer);

			int receiptCellCount = 0;

			if (obj == null) {
				foreach (var product in Cart.ProductsInCart) {
					receiptStackLayout.Children.Add (new ReceiptCell (receiptCellCount++, product).View);
				}
			}
			else {
				//Product product = new Product(
				if (obj is HistoryClass) {
					foreach (var productString in (obj as HistoryClass).ProductOrderList) {
						string quantity = productString.Split (';') [0].Split (':') [1];
						string name = productString.Split (';') [1].Split (':') [1];
						string description = productString.Split (';') [2].Split (':') [1];
						string price = productString.Split (';') [3].Split (':') [1];

						var product = new Product ("", name, "", decimal.Parse (price), "", description,true);
						product.ProductNumberInCart = int.Parse (quantity);
						receiptStackLayout.Children.Add (new ReceiptCell (receiptCellCount++, product).View);
					}
				} else if (obj is StatusClass) {
					foreach (var productString in (obj as StatusClass).ProductOrderList) {
						string quantity = productString.Split (';') [0].Split (':') [1];
						string name = productString.Split (';') [1].Split (':') [1];
						string description = productString.Split (';') [2].Split (':') [1];
						string price = productString.Split (';') [3].Split (':') [1];

						var product = new Product ("", name, "", decimal.Parse (price), "", description,true);
						product.ProductNumberInCart = int.Parse (quantity);
						receiptStackLayout.Children.Add (new ReceiptCell (receiptCellCount++, product).View);
					}
				}
					
			}
				
			var receiptScrollView = new ScrollView {
				Orientation = ScrollOrientation.Vertical,
				Content = receiptStackLayout
			};

			mMidLayout.Children.Add (mReceiptLayout,
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (mAddressLayout, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(25);	
				})
			);

			mMidLayout.Children.Add (quantityLabel,
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize(22);	
				}),
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Top + MyDevice.GetScaledSize(37);	
				})
			);

			mMidLayout.Children.Add (productLabel,
				Constraint.RelativeToView (quantityLabel, (p, sibling) => {
					return sibling.Bounds.Right;	
				}),
				Constraint.RelativeToView (quantityLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			mMidLayout.Children.Add (descriptionLabel,
				Constraint.RelativeToView (productLabel, (p, sibling) => {
					return sibling.Bounds.Right;	
				}),
				Constraint.RelativeToView (productLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			mMidLayout.Children.Add (costLabel,
				Constraint.RelativeToView (descriptionLabel, (p, sibling) => {
					return sibling.Bounds.Right;	
				}),
				Constraint.RelativeToView (descriptionLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			double receiptHeight = MyDevice.GetScaledSize (24)*(receiptCellCount);

			if (receiptHeight > MyDevice.GetScaledSize (238))
				receiptHeight = MyDevice.GetScaledSize (238);

			mMidLayout.Children.Add (receiptScrollView,
				Constraint.RelativeToView (quantityLabel, (parent,sibling) => {
					return sibling.Bounds.Left;
				}),
				Constraint.RelativeToView (quantityLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(16);
				}),
				Constraint.Constant(MyDevice.GetScaledSize(571)),
				Constraint.Constant(receiptHeight)
			);

			mMidLayout.Children.Add (totalAmountLabel,
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize(206);	
				}),
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Bottom - MyDevice.GetScaledSize(138);	
				})
			);

			mMidLayout.Children.Add (totalAmountPriceLabel,
				Constraint.RelativeToView (totalAmountLabel, (p, sibling) => {
					return sibling.Bounds.Right;	
				}),
				Constraint.RelativeToView (totalAmountLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			mMidLayout.Children.Add (agreeLabel,
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize(84);	
				}),
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Bottom - MyDevice.GetScaledSize(75);	
				})
			);

			mMidLayout.Children.Add (disagreeLabel,
				Constraint.RelativeToView (agreeLabel, (p, sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize(90);	
				}),
				Constraint.RelativeToView (agreeLabel, (p, sibling) => {
					return sibling.Bounds.Top;	
				})
			);

			mMidLayout.Children.Add (thankyouLabel,
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (mReceiptLayout, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(5);	
				})
			);

			mMidLayout.Children.Add (rightsReservedLabel,
				Constraint.RelativeToView (thankyouLabel, (p, sibling) => {
					return sibling.Bounds.Left;	
				}),
				Constraint.RelativeToView (thankyouLabel, (p, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(5);	
				})
			);
		}
		public bool isMenuAnimationWorking = false;
		private void ActivateOrDeactivateMenu()
		{
			if (isMenuAnimationWorking)
				return;
			else
				isMenuAnimationWorking = true;
			Rectangle menuRectangle;
			Rectangle midRectangle;

			if (!IsMenuOpen) {
				menuRectangle = new Rectangle (new Point (MyDevice.GetScaledSize(mMenuWidth), 0), new Size (mMenuLayout.Bounds.Width, mMenuLayout.Bounds.Height));
				midRectangle = new Rectangle (new Point (MyDevice.GetScaledSize (mMenuWidth), 0), new Size (mMidLayout.Bounds.Width, mMidLayout.Bounds.Height));
				mainRelativeLayout.Children.Add (InputBlockerForSwipeMenu,
					Constraint.Constant (MyDevice.GetScaledSize (mMenuWidth)),
					Constraint.Constant (0)
				);

				var tapRecognizer = new TapGestureRecognizer ();
				if (InputBlockerForSwipeMenu.GestureRecognizers.Count == 0) {
					tapRecognizer.Tapped += (sender, e) => {				 				
						ActivateOrDeactivateMenu();				
					};
				}
				InputBlockerForSwipeMenu.GestureRecognizers.Add(tapRecognizer);
			} else {
				menuRectangle = new Rectangle (new Point (MyDevice.GetScaledSize (0), 0), new Size (mMenuLayout.Bounds.Width, mMenuLayout.Bounds.Height));
				midRectangle = new Rectangle (new Point (0, 0), new Size (mMidLayout.Bounds.Width, mMidLayout.Bounds.Height));
				mainRelativeLayout.Children.Remove (InputBlockerForSwipeMenu);
			}

			mMenuLayout.TranslateTo (menuRectangle.X,menuRectangle.Y, MyDevice.AnimationTimer, Easing.Linear).ContinueWith(antecendent => isMenuAnimationWorking=false);
			mMidLayout.TranslateTo (midRectangle.X,midRectangle.Y, MyDevice.AnimationTimer, Easing.Linear);

			IsMenuOpen = !IsMenuOpen;
		}			
	}
}

