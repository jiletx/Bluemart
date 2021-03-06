﻿using System;
using System.Collections.Generic;
using bluemart.Common.Utilities;
using bluemart.Common.Objects;
using bluemart.Common.ViewCells;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using XLabs.Forms.Controls;
using bluemart.Models.Local;

namespace bluemart.MainViews
{
	public partial class BrowseProductsPage : ContentPage
	{		
		private int mActiveButtonIndex = 0;
		public RootPage mParent;
		public string mCategoryID;

		private int mLoadSize = 50;
		private int mLastLoadedIndex = 0;
		private int mLastScrollIndex = 0;
		private bool bIsImagesProduced = false;
		//Containers
		public Dictionary<string,List<Product>> mProductDictionary;
		public Category mCategory;
		private List<Label> mButtonList;
		public List<Product> mProductList = new List<Product> ();
		private List<int> mCategoryIndexList;
		//Queues
		public List<ProductCell> mProductCellList = new List<ProductCell> ();
		private Queue<ProductCell> mTrashProductCellQueue = new Queue<ProductCell>();
		private Queue<ProductCell> mPopulaterProductCellQueue = new Queue<ProductCell> ();
		private Queue<ProductCell> mManagerProductCellQueue = new Queue<ProductCell> ();
		CancellationTokenSource mLoadProductsToken = new CancellationTokenSource();

		static readonly Object _ListLock = new Object();

		List<Product> mTopSellingProductList = new List<Product>();
		List<ProductCell> mTopSellingProductCellList = new List<ProductCell> ();

		//ScrollView ProductScrollView;
		GridView2 ProductGrid;

		private RelativeLayout SubcategoryLayout;
		private ScrollView SubcategoryScrollView;
		private StackLayout SubCategoryStackLayout;
		private List<BoxView> mBoxViewList;
		private BoxView mEnabledBoxView;

		private RelativeLayout InputBlocker;

		private RelativeLayout mTopLayout;
		private RelativeLayout mMenuLayout;
		private RelativeLayout mMidLayout;
		private RelativeLayout mSearchLayout;
		private Entry SearchEntry;
		private Label ProductCountLabel;
		private Label PriceLabel;
		private bool IsMenuOpen = false;
		private double mMenuWidth = 517.0;
		private Label categoriesLabel;

		public bool IsCartOpen = false;
		UserClass mUserModel = new UserClass();
		AddressClass mAddressModel = new AddressClass();
		private RelativeLayout mCartLayout;
		private double mCartWidth = 552.0;
		//public StackLayout CartStackLayout;
		public ListView cartListView;

		public Label subtotalPriceLabel;
		public Label checkoutPriceLabel;

		private RelativeLayout InputBlockerForSwipeMenu;
		private RelativeLayout InputBlockerForSwipeCart;

		public Image mCartImage;
		private readonly uint mCartFadeInTimer = 200;
		private readonly int mCartFlashTimer = 2000;
		private readonly double mCartScaleSize = 1.25f;
		private bool bIsCartFlashing = false;

		public BrowseProductsPage (Dictionary<string, List<Product>> productDictionary, Category category,RootPage parent)
		{					
			InitializeComponent ();
			mParent = parent;
			mCategory = category;
			PopulationOfNewProductPage (productDictionary, category);
			CreationInitialization ();
			MyDevice.currentPage = this;

		}
			
		public async void FlashCartButton()
		{
			if (!bIsCartFlashing) {
				bIsCartFlashing = true;
				mCartImage.Scale = mCartScaleSize;
				//await mCartImage.ScaleTo (mCartScaleSize, 0);
				double flashTimer = mCartFlashTimer;
				while (flashTimer >= 0) {
					flashTimer -= mCartFadeInTimer;
					await mCartImage.FadeTo (0, mCartFadeInTimer, Easing.Linear);
					flashTimer -= mCartFadeInTimer;
					await mCartImage.FadeTo (1, mCartFadeInTimer, Easing.Linear);
				}
				mCartImage.Scale = 1f;
				bIsCartFlashing = false;
			}
		}

		public void CreationInitialization()
		{
			NavigationPage.SetHasNavigationBar (this, false);
			mBoxViewList = new List<BoxView> ();
			mButtonList = new List<Label> ();
			mCategoryIndexList = new List<int> ();
			InitializeLayout ();
			//SetGrid1Definitions ();
		}

		private void InitializeLayout()
		{	
			//mainRelativeLayout.BackgroundColor = Color.FromRgb (236, 240, 241);
			mMidLayout = new RelativeLayout ();

			Point totalDistance = new Point(0,0);

			mainRelativeLayout.Panning += (object sender, MR.Gestures.PanEventArgs e) => {
				totalDistance = e.TotalDistance;
			};

			mainRelativeLayout.Swiped += (object sender, MR.Gestures.SwipeEventArgs e) => {
				if(e.Direction == MR.Gestures.Direction.Left)
				{
					if(!IsCartOpen&&!IsMenuOpen)
						ActivateOrDeactivateCart();
					else if(IsMenuOpen&&!IsCartOpen)
						ActivateOrDeactivateMenu();					
				}
				else if( e.Direction == MR.Gestures.Direction.Right)
				{
					if(IsCartOpen&&!IsMenuOpen)
						ActivateOrDeactivateCart();
					else if(!IsMenuOpen&&!IsCartOpen)
						ActivateOrDeactivateMenu();
				}
				else if( totalDistance.X != 0 && e.Direction == MR.Gestures.Direction.NotClear)
				{
					if( totalDistance.X < - MyDevice.SwipeDistance )
					{
						if(!IsCartOpen&&!IsMenuOpen)
							ActivateOrDeactivateCart();
						else if(IsMenuOpen&&!IsCartOpen)
							ActivateOrDeactivateMenu();
					}
					else if( totalDistance.X > MyDevice.SwipeDistance )
					{
						if(IsCartOpen&&!IsMenuOpen)
							ActivateOrDeactivateCart();
						else if(!IsMenuOpen&&!IsCartOpen)
							ActivateOrDeactivateMenu();
					}
				}
			};

			mainRelativeLayout.BackgroundColor = Color.FromRgb (236, 240, 241);
			mainRelativeLayout.Children.Add (mMidLayout,
				Constraint.Constant (0),
				Constraint.Constant (0)
			);

			InputBlocker = new RelativeLayout () {
				WidthRequest = MyDevice.ScreenWidth,
				HeightRequest = MyDevice.ScreenHeight
			};

			var inputBlockerTapRecogniser = new TapGestureRecognizer ();
			inputBlockerTapRecogniser.Tapped += (sender, e) => {				
				SearchEntry.Unfocus();
			};
			InputBlocker.GestureRecognizers.Add(inputBlockerTapRecogniser);

			InputBlockerForSwipeMenu = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(123),
				HeightRequest = MyDevice.ScreenHeight,
				Padding = 0
			};

			InputBlockerForSwipeCart = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(86),
				HeightRequest = MyDevice.ScreenHeight,
				Padding = 0
			};

			InitializeHeaderLayout ();
			InitializeSearchLayout ();
			InitializeSubCategoriesLayout ();
			InitializeCartLayout ();
			InitializeBottomLayout ();
			InitializeMenuLayout ();
			EventHandlers ();
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
				Source = "MenuPage_Open.png",
				Aspect = Aspect.Fill
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

			var settingsImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Settings.png",
				Aspect = Aspect.Fill
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

			var favoritesImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Favorites.png",
				Aspect = Aspect.Fill
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

			var trackImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(40),
				HeightRequest = MyDevice.GetScaledSize(35),
				Source = "MenuPage_Track.png",
				Aspect = Aspect.Fill
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
					tapRecog.Tapped += async (sender, e) => {
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

		private void InitializeCartLayout()
		{
			mCartLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(mCartWidth),
				HeightRequest = MyDevice.ScreenHeight,
				BackgroundColor = Color.FromRgb(51,51,51),
				Padding = 0
			};

			UserClass user = mUserModel.GetUser ();
			AddressClass activeAdress = mAddressModel.GetActiveAddress (user.ActiveRegion);
			string userName = "";
			if (activeAdress != null)
				userName = activeAdress.Name;

			var userNameLabel = new Label () {				
				Text = userName.Split(' ')[0],
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(190),
				HeightRequest = MyDevice.GetScaledSize(85)
			};

			var titleLabel = new Label () {
				Text = "'s Basket (AED)",
				TextColor = Color.FromRgb(152,152,152),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(250),
				HeightRequest = MyDevice.GetScaledSize(85)
			};

			var profilePic = new Image () {
				WidthRequest = MyDevice.GetScaledSize(33),
				HeightRequest = MyDevice.GetScaledSize(37),
				Aspect = Aspect.Fill,
				Source = "CartPage_ProfilePic.png"
			};

			var firstLine = new BoxView (){
				HeightRequest = 1,
				WidthRequest = MyDevice.GetScaledSize(mCartWidth),
				Color = Color.FromRgb(129,129,129)
			};

			/*CartStackLayout = new StackLayout {
				Orientation = StackOrientation.Vertical,
				Padding = 0,
				Spacing = 0
			};	*/				

			cartListView = new ListView()
			{
				RowHeight = (int)MyDevice.GetScaledSize(150)
			};
			cartListView.ItemTemplate = new DataTemplate (typeof(CartCellNew));

			/*var cartScrollView = new ScrollView {
				Orientation = ScrollOrientation.Vertical,
				Content = CartStackLayout
			};*/

			var bottomLayout = new RelativeLayout () {
				BackgroundColor = Color.Black,
				WidthRequest = MyDevice.GetScaledSize(mCartWidth),
				HeightRequest = MyDevice.GetScaledSize(239)
			};

			var subtotalLabel = new Label () {
				Text = "Subtotal",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(140),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			subtotalPriceLabel = new Label () {
				Text = "1055,85",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(130),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			var deliveryFeeLabel = new Label () {
				Text = "Delivery Fee",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(213),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			var deliveryFeePriceLabel = new Label () {
				Text = "FREE",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(130),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			var checkoutLabel = new Label () {
				Text = "Checkout Now",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeMedium,
				WidthRequest = MyDevice.GetScaledSize(251),
				HeightRequest = MyDevice.GetScaledSize(60)
			};

			checkoutPriceLabel = new Label () {
				Text = "1055,85",
				TextColor = Color.White,
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = MyDevice.FontSizeSmall,
				WidthRequest = MyDevice.GetScaledSize(200),
				HeightRequest = MyDevice.GetScaledSize(60)
			};

			var checkoutButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(493),
				HeightRequest = MyDevice.GetScaledSize(60),
				BackgroundColor = Color.FromRgb(253,59,47)
			};

			var checkoutTapRecogniser = new TapGestureRecognizer ();
			checkoutTapRecogniser.Tapped += async (sender, e) => {
				var hour = Time.GetTime();

				if( hour < 8 || hour > 23 ) 
				{
					await DisplayAlert("Sorry","Market is closed between 8 AM and 11 PM ","OK");
				}
				else if( Cart.ProductTotalPrice == 0 )
					await DisplayAlert("Sorry","Please add products to your basket","OK");
				else if( mAddressModel.GetActiveAddress(mUserModel.GetUser().ActiveRegion) == null )
				{					
					await DisplayAlert("Sorry","Please Enter Your Address On Settings Page","OK");
					mParent.LoadSettingsPage();
				}
				else if( Cart.ProductTotalPrice < 50 )
				{
					await DisplayAlert("Sorry","Your orders must exceed 50 AED, as it is the minimum amount.","OK");
				}
				else
					mParent.LoadReceiptPage();
			};
			checkoutButton.GestureRecognizers.Add (checkoutTapRecogniser);

			mainRelativeLayout.Children.Add (mCartLayout,
				Constraint.Constant (MyDevice.ScreenWidth),
				Constraint.Constant (0)
			);

			mCartLayout.Children.Add (profilePic,
				Constraint.Constant (MyDevice.GetScaledSize(mCartWidth-43.5f)),
				Constraint.Constant (MyDevice.GetScaledSize(25f))
			);

			mCartLayout.Children.Add (titleLabel,
				Constraint.RelativeToView (profilePic, (parent,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(268);	
				} ),
				Constraint.Constant (0)
			);

			mCartLayout.Children.Add (userNameLabel,
				Constraint.RelativeToView (titleLabel, (parent,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(190);	
				} ),
				Constraint.Constant (0)
			);

			mCartLayout.Children.Add (firstLine,
				Constraint.Constant(0),
				Constraint.RelativeToView (userNameLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(2);
				})
			);

			mCartLayout.Children.Add (cartListView,
				Constraint.Constant(MyDevice.GetScaledSize(0)),
				Constraint.RelativeToView (firstLine, (parent,sibling) => {
					return sibling.Bounds.Bottom;
				}),
				Constraint.Constant(MyDevice.GetScaledSize(mCartWidth)),
				Constraint.Constant(MyDevice.ScreenHeight - MyDevice.GetScaledSize(324))
			);

			mCartLayout.Children.Add (bottomLayout,
				Constraint.Constant(0),
				Constraint.RelativeToView( mCartLayout, (parent,sibling) =>
					{
						return sibling.Bounds.Bottom - MyDevice.GetScaledSize(239);
					})
			);

			bottomLayout.Children.Add (subtotalLabel,
				Constraint.Constant(MyDevice.GetScaledSize(55)),	
				Constraint.Constant(MyDevice.GetScaledSize(25))
			);

			bottomLayout.Children.Add (subtotalPriceLabel,
				Constraint.Constant(MyDevice.GetScaledSize(mCartWidth-179)),	
				Constraint.Constant(MyDevice.GetScaledSize(25))
			);

			bottomLayout.Children.Add (deliveryFeeLabel,
				Constraint.RelativeToView( subtotalLabel, (parent,sibling) => {
					return sibling.Bounds.Left;	
				}),	
				Constraint.RelativeToView( subtotalLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom;	
				})
			);

			bottomLayout.Children.Add (deliveryFeePriceLabel,
				Constraint.RelativeToView( subtotalPriceLabel, (parent,sibling) => {
					return sibling.Bounds.Left;	
				}),	
				Constraint.RelativeToView( subtotalPriceLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom;	
				})
			);

			bottomLayout.Children.Add (checkoutButton,
				Constraint.RelativeToView (deliveryFeeLabel, (parent, sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(22);	
				}),	
				Constraint.RelativeToView (deliveryFeeLabel, (parent, sibling) => {
					return sibling.Bounds.Bottom;	
				})
			);

			bottomLayout.Children.Add (checkoutLabel,
				Constraint.RelativeToView( deliveryFeeLabel, (parent,sibling) => {
					return sibling.Bounds.Left;	
				}),	
				Constraint.RelativeToView( deliveryFeeLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom;	
				})
			);

			bottomLayout.Children.Add (checkoutPriceLabel,
				Constraint.RelativeToView( checkoutLabel, (parent,sibling) => {
					return sibling.Bounds.Right;	
				}),	
				Constraint.RelativeToView( deliveryFeePriceLabel, (parent,sibling) => {
					return sibling.Bounds.Bottom;	
				})
			);
		}

		private void InitializeHeaderLayout ()
		{
			mTopLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(640),
				HeightRequest = MyDevice.GetScaledSize(87),
				BackgroundColor = Color.FromRgb(27,184,105)
			};

			var menuIcon = new Image () {
				WidthRequest = MyDevice.GetScaledSize(36),
				HeightRequest = MyDevice.GetScaledSize(37),
				Source = "CategoriesPage_MenuIcon.png"
			};

			var categoryLabel = new Label (){ 
				Text = mCategory.Name,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeLarge,
				LineBreakMode = LineBreakMode.TailTruncation
			};

			PriceLabel = new Label () {
				Text = "0\nAED",	
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeSmall,
				HorizontalTextAlignment = TextAlignment.End,
				HorizontalOptions = LayoutOptions.End,
				WidthRequest = MyDevice.GetScaledSize(130)
			};

			var verticalLine = new Image () {
				WidthRequest = MyDevice.GetScaledSize(1),
				HeightRequest = MyDevice.GetScaledSize(63),
				Aspect = Aspect.Fill,
				Source = "CategoriesPage_VerticalLine.png"
			};

			mCartImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(71),
				HeightRequest = MyDevice.GetScaledSize(57),
				Aspect = Aspect.Fill,
				Source = "ProductsPage_BasketIcon.png"
			};

			var cartButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(90),
				HeightRequest = MyDevice.GetScaledSize(90),
				Padding = 0
			};

			ProductCountLabel = new Label () {					
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMicro,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				WidthRequest = MyDevice.GetScaledSize(37),
				HeightRequest = MyDevice.GetScaledSize(27)
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

			var cartTapRecognizer= new TapGestureRecognizer ();
			cartTapRecognizer.Tapped += (sender, e) => {				
				ActivateOrDeactivateCart();
			};
			cartButton.GestureRecognizers.Add(cartTapRecognizer);

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

			mMidLayout.Children.Add (categoryLabel,
				Constraint.RelativeToView (menuIcon, (parent, sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize (22);
				}),
				Constraint.RelativeToView (menuIcon, (parent, sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize (3);
				}),
				Constraint.RelativeToView (menuIcon, (parent, sibling) => {
					return  MyDevice.GetScaledSize (350);
				}),null

			);

			mMidLayout.Children.Add (mCartImage, 
				Constraint.Constant( MyDevice.GetScaledSize(561) ),
				Constraint.Constant( MyDevice.GetScaledSize(16) )
			);

			mMidLayout.Children.Add (verticalLine,
				Constraint.RelativeToView (mCartImage, (parent, sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize (14);
				}),
				Constraint.RelativeToView (mCartImage, (parent, sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize (5);
				})
			);

			mMidLayout.Children.Add (PriceLabel,
				Constraint.RelativeToView (verticalLine, (parent, sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize (150);
				}),
				Constraint.RelativeToView (mCartImage, (parent, sibling) => {
					return sibling.Bounds.Top;
				})
			);

			mMidLayout.Children.Add (ProductCountLabel,
				Constraint.RelativeToView (mCartImage, (parent, sibling) => {
					return sibling.Bounds.Right - MyDevice.GetScaledSize (37);
				}),
				Constraint.RelativeToView (mCartImage, (parent, sibling) => {
					return sibling.Bounds.Bottom - MyDevice.GetScaledSize (27);
				})
			);	

			mMidLayout.Children.Add (menuButton,
				Constraint.Constant (0),
				Constraint.Constant (0));

			mMidLayout.Children.Add (cartButton,
				Constraint.Constant(MyDevice.GetScaledSize(550)),
				Constraint.Constant (0)
			);

			UpdateProductCountLabel ();
			UpdatePriceLabel ();
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

		public void ActivateOrDeactivateCart()
		{
			if (isMenuAnimationWorking)
				return;
			else
				isMenuAnimationWorking = true;
			Rectangle cartRectangle;
			Rectangle midRectangle;

			CartCellNew.deleteIsOpenDictionary = new Dictionary<string, bool> ();

			if (!IsCartOpen) {
				cartRectangle = new Rectangle (new Point (MyDevice.GetScaledSize (mCartWidth*-1), 0), new Size (mCartLayout.Bounds.Width, mCartLayout.Bounds.Height));
				midRectangle = new Rectangle (new Point (MyDevice.GetScaledSize (mCartWidth*-1), 0), new Size (mMidLayout.Bounds.Width, mMidLayout.Bounds.Height));

				mainRelativeLayout.Children.Add (InputBlockerForSwipeCart,
					Constraint.Constant (0),
					Constraint.Constant (0)
				);

				var tapRecognizer = new TapGestureRecognizer ();
				if (InputBlockerForSwipeCart.GestureRecognizers.Count == 0) {
					tapRecognizer.Tapped += (sender, e) => {				 				
						ActivateOrDeactivateCart();				
					};
				}
				InputBlockerForSwipeCart.GestureRecognizers.Add(tapRecognizer);

				subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();

				cartListView.ItemsSource = Cart.ProductsInCart;

				/*CartStackLayout.Children.Clear ();

				foreach (Product p in Cart.ProductsInCart) {
					var cartCell = new CartCell (p, this);
					//mCartCellList.Add (cartCell);
					CartStackLayout.Children.Add( cartCell.View );
				}*/
			} else {
				cartRectangle = new Rectangle (new Point (0, 0), new Size (mCartLayout.Bounds.Width, mCartLayout.Bounds.Height));
				midRectangle = new Rectangle (new Point (0, 0), new Size (mMidLayout.Bounds.Width, mMidLayout.Bounds.Height));
				mainRelativeLayout.Children.Remove (InputBlockerForSwipeCart);
			}

			mCartLayout.TranslateTo (cartRectangle.X,cartRectangle.Y, MyDevice.AnimationTimer, Easing.Linear).ContinueWith(antecendent => isMenuAnimationWorking=false);
			mMidLayout.TranslateTo (midRectangle.X,midRectangle.Y, MyDevice.AnimationTimer, Easing.Linear);

			IsCartOpen = !IsCartOpen;
		}


		private void InitializeSearchLayout()
		{
			mSearchLayout = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(640),
				HeightRequest = MyDevice.GetScaledSize(73),
				BackgroundColor = Color.FromRgb(27,184,105)
			};

			SearchEntry = new Entry () {
				WidthRequest = MyDevice.GetScaledSize(640),
				HeightRequest = MyDevice.GetScaledSize(73),
				Text = "Search",
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium,
				HorizontalTextAlignment = TextAlignment.Start
			};

			var searchImage = new Image () {
				WidthRequest = MyDevice.GetScaledSize(583),
				HeightRequest = MyDevice.GetScaledSize(52),
				Source = "ProductsPage_SearchBar.png"	
			};

			var searchButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(444),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			var deleteButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(69),
				HeightRequest = MyDevice.GetScaledSize(51)
			};

			var backButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(65),
				HeightRequest = MyDevice.GetScaledSize(72)
			};					


			var searchEntryTapRecognizer= new TapGestureRecognizer ();
			searchEntryTapRecognizer.Tapped += (sender, e) => {				
				SearchEntry.Focus();
			};
			searchButton.GestureRecognizers.Add(searchEntryTapRecognizer);

			var deleteButtonTapRecognizer= new TapGestureRecognizer ();
			deleteButtonTapRecognizer.Tapped += (sender, e) => {				
				SearchEntry.Text = "";
			};
			deleteButton.GestureRecognizers.Add(deleteButtonTapRecognizer);

			var backButtonTapRecognizer= new TapGestureRecognizer ();
			backButtonTapRecognizer.Tapped += (sender, e) => {				
				mParent.SwitchTab ("BrowseCategories");
			};
			backButton.GestureRecognizers.Add(backButtonTapRecognizer);

			mMidLayout.Children.Add (mSearchLayout,
				Constraint.Constant(0),
				Constraint.RelativeToView (mTopLayout, (parent, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize (1);
				})
			);

			mMidLayout.Children.Add (searchImage,
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize (28);
				}),
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Top + MyDevice.GetScaledSize (10);
				})
			);

			mMidLayout.Children.Add (SearchEntry,
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Left + MyDevice.GetScaledSize (136);
				}),
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Top + MyDevice.GetScaledSize (5);
				})
			);

			mMidLayout.Children.Add (searchButton,
				Constraint.RelativeToView (searchImage, (parent, sibling) => {
					return sibling.Bounds.Left;
				}),
				Constraint.RelativeToView (searchImage, (parent, sibling) => {
					return sibling.Bounds.Top;
				})
			);

			mMidLayout.Children.Add (deleteButton,
				Constraint.RelativeToView (searchImage, (parent, sibling) => {
					return sibling.Bounds.Right - MyDevice.GetScaledSize (67);
				}),
				Constraint.RelativeToView (searchImage, (parent, sibling) => {
					return sibling.Bounds.Top;
				})
			);

			mMidLayout.Children.Add (backButton,
				Constraint.Constant (0),
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Top;
				})
			);
		}

		private void InitializeSubCategoriesLayout()
		{
			SubcategoryLayout = new RelativeLayout (){ 
				HeightRequest = MyDevice.GetScaledSize(66),
				BackgroundColor = Color.Red/*,
				BackgroundColor = Color.FromRgb(27,184,105)*/
			};

			SubCategoryStackLayout = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Padding = new Thickness(MyDevice.GetScaledSize(15),0,0,0),
				Spacing = MyDevice.GetScaledSize(15)
			};

			PopulateSubCategoryButtons ();

			SubcategoryScrollView = new ScrollView {
				Orientation = ScrollOrientation.Horizontal,
				Content = SubCategoryStackLayout
			};

			mMidLayout.Children.Add (SubcategoryScrollView,
				Constraint.Constant(0),
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Bottom;
				}),
				Constraint.Constant(MyDevice.ScreenWidth)
			);

			if (mProductDictionary.Count <= 0) {
				SubcategoryScrollView.IsEnabled = false;
				SubcategoryScrollView.IsVisible = false;
			}
		}

		private void PopulateSubCategoryButtons()
		{
			mButtonList.Clear ();
			mBoxViewList.Clear ();
			mTopSellingProductList.Clear ();
			mTopSellingProductCellList.Clear ();

			foreach (var productPair in mProductDictionary) {
				if (productPair.Value.Count > 0) {

					if (productPair.Key == "Top Selling") {
						mTopSellingProductList = productPair.Value;
					}

					var buttonLayout = new RelativeLayout () {
						BackgroundColor = Color.Transparent,
					};

					Label label = new Label () {
						VerticalOptions = LayoutOptions.Center,
						BackgroundColor = Color.Transparent,
						Text = " "+productPair.Key+" ",
						TextColor = Color.FromRgb(136,147,161),
						FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label)),
						HeightRequest = MyDevice.GetScaledSize(60),
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center
					};

					BoxView boxView = new BoxView (){
						HeightRequest = 1,
						Color = MyDevice.RedColor,
						IsVisible = false
					};

					mBoxViewList.Add (boxView);
					mButtonList.Add (label);

					buttonLayout.Children.Add (label,
						Constraint.Constant (0),
						Constraint.Constant (0)
					);

					buttonLayout.Children.Add (boxView,
						Constraint.RelativeToView( label, (parent,sibling) =>{
							return sibling.Bounds.Left + MyDevice.GetScaledSize(0);	
						}),
						Constraint.RelativeToView( label, (parent,sibling) =>{
							return sibling.Bounds.Bottom - MyDevice.GetScaledSize(14);	
						}),
						Constraint.RelativeToView( label, (parent,sibling) =>{
							return sibling.Bounds.Width - MyDevice.GetScaledSize(5);	
						})
					);

					var tapRecognizer = new TapGestureRecognizer ();
					tapRecognizer.Tapped += (sender, e) => {

						if (mParent.mActivityIndicator.IsRunning)
							return;
						FocusSelectedButton (sender as Label);
					};

					label.GestureRecognizers.Add (tapRecognizer);

					SubCategoryStackLayout.Children.Add (buttonLayout);
				}
			}

			if (mButtonList.Count > 0) {
				mEnabledBoxView = mBoxViewList [0];
				mEnabledBoxView.IsVisible = true;
			}
		}


		private void InitializeBottomLayout()
		{
			/*ProductGrid = new Grid () {
				Padding = 0
			};

			PopulateGrid ();

			ProductScrollView = new ScrollView () {
				Orientation = ScrollOrientation.Vertical,
				Content = ProductGrid
			};*/
			ProductGrid = new GridView2(){};
			//ProductGrid.Padding = new Thickness (MyDevice.GetScaledSize(13), 0, 0, MyDevice.GetScaledSize(13)); 
			//ProductGrid.ColumnSpacing = MyDevice.GetScaledSize (13);
			//ProductGrid.ContentPaddingLeft = MyDevice.GetScaledSize (10);
			//ProductGrid.ContentPaddingRight = MyDevice.GetScaledSize (10);
			ProductGrid.WidthRequest = MyDevice.ScreenWidth;
			//ProductGrid.IsContentCentered = true;
			ProductGrid.RowSpacing = MyDevice.GetScaledSize (10);
			//ProductGrid.ColumnSpacing = MyDevice.GetScaledSize (10);
			ProductGrid.ItemWidth = MyDevice.GetScaledSize (298);
			ProductGrid.ItemHeight = MyDevice.GetScaledSize (377);
			ProductGrid.SelectionEnabled = true;
			ProductGrid.ItemTemplate = new DataTemplate (typeof(ProductCellNew));
			ProductGrid.IsScrollEnabled = true;
			ProductGrid.MaxItemsPerRow = 2;
			ProductGrid.CenterAsFilledRow=true;
			var valueList = mProductDictionary.Values.Cast<List<Product>> ().ToList();
			//var tempProductList = new List<Product> ();
			foreach (var products in valueList) {
				if (products.Count > 0) {
					mCategoryIndexList.Add (mProductList.Count);
					foreach (var tempProduct in products) {					
						mProductList.Add (tempProduct);	
					}
				}
			}
			ProductGrid.ItemsSource = mProductList;
			mMidLayout.Children.Add (ProductGrid,
				Constraint.Constant(0),
				Constraint.RelativeToView (mSearchLayout, (parent, sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(64);
				}),
				Constraint.Constant(MyDevice.ScreenWidth),
				Constraint.Constant(MyDevice.ScreenHeight-MyDevice.GetScaledSize(87)-MyDevice.GetScaledSize(73)-MyDevice.GetScaledSize(1)-MyDevice.GetScaledSize(117))
			);
		}

		private void EventHandlers()
		{
			SearchEntry.Focused += (sender, e) => {
				SearchEntry.Text = "";
				mMidLayout.Children.Add( InputBlocker,
					Constraint.Constant(0),
					Constraint.Constant(0)
				);
			};

			SearchEntry.Unfocused += (sender, e) => {
				if( SearchEntry.Text == "" )
					SearchEntry.Text = "Search";
				mMidLayout.Children.Remove(InputBlocker);
			};

			SearchEntry.Completed += (sender, e) => {
				if (SearchEntry.Text.Length >= 3) {				
					mParent.LoadSearchPage (SearchEntry.Text);
				} else {				
					SearchEntry.Text = "Must be longer than 2 characters!";
				}
				mMidLayout.Children.Remove(InputBlocker);
			};
			ProductGrid.OnScroll += OnScrolled;
			//ProductScrollView.Scrolled += OnScrolled;
		}


		public void PopulationOfNewProductPage(Dictionary<string,List<Product>> productDictionary,Category category)
		{	
			//mParent.mTopNavigationBar.NavigationText.Text = category.Name;
			mCategoryID = category.CategoryID;
			mProductDictionary = productDictionary;



			//PopulateGrid ();
		}

		public void ClearContainers()
		{			
			mLoadProductsToken.Cancel ();
			mParent.mActivityIndicator.IsRunning = false;

			SubCategoryStackLayout.Children.Clear ();
			mProductDictionary.Clear ();
			mButtonList.Clear ();
			//mButtonList.Clear ();
			mCategoryIndexList.Clear ();
			foreach (var productCell in mProductCellList) {
				productCell.ClearStreamsAndImages ();
			}	


			//mProductCellList.Clear ();

			mProductCellList.Clear();
			mTrashProductCellQueue.Clear();
			mPopulaterProductCellQueue.Clear();
			mManagerProductCellQueue.Clear();
		}

		public void  UpdatePriceLabel()
		{
			PriceLabel.Text = Cart.ProductTotalPrice.ToString()+"\nAED";
		}

		public void UpdateProductCountLabel()
		{
			int count = 0;

			foreach (var product in Cart.ProductsInCart) {
				count += product.ProductNumberInCart;	
			}

			ProductCountLabel.Text = count.ToString ();
		}

		protected override void OnAppearing()
		{			
			UpdatePriceLabel ();
		}

		private void SetGrid1Definitions()
		{
			/*Grid1.RowDefinitions [0].Height = HeightRequest = MyDevice.ScreenWidth * 0.15f;
			Grid1.RowDefinitions [1].Height = GridLength.Auto;
			Grid1.ColumnDefinitions [0].Width = MyDevice.ScreenWidth;
			Grid1.BackgroundColor = MyDevice.BackgroundColor;*/
		}

		private async Task WaitUntilCorrespondingSubCategoryLoaded(int productCellIndex)
		{
			/*mParent.mActivityIndicator.IsRunning = true;
			ProductScrollView.IsVisible = false;
			while (ProductGrid.Children.Count-2 < productCellIndex) {
				await Task.Delay (100);
			}
			mParent.mActivityIndicator.IsRunning = false;
			ProductScrollView.IsVisible = true;*/
		}

		private void PopulateTopSelling()
		{

		}			
		private void OnScrolled (object sender, TwinTechs.Controls.ControlScrollEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine ("delta:"+e.Delta.ToString()+" current:"+e.CurrentY + " curentRow:" +e.CurrentRow);
			//System.Diagnostics.Debug.WriteLine ("mActiveButtonIndex:"+mActiveButtonIndex.ToString()+" mCategoryIndexList.Count:"+mCategoryIndexList.Count );
			//System.Diagnostics.Debug.WriteLine (e.Delta);
			if (e.Delta > 0) {
				if (mActiveButtonIndex + 1 != mCategoryIndexList.Count) {
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex +1];
					if (e.CurrentRow * 2 >= productCellIndex) {
						mActiveButtonIndex += 1;
						ChangeSelectedButton ();
					}
				}
			} else if(e.Delta < 0)
			{
				if (mActiveButtonIndex  != -0) {
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex  ];
					if (e.CurrentRow * 2 <= productCellIndex) {
						mActiveButtonIndex -= 1;
						ChangeSelectedButton ();
					}
				}
			}
		}
		/*
		private void  OnScrolled( Object sender, ScrolledEventArgs e)
		{
			if (DecideIfIsUpOrDown (sender as ScrollView) == "Down") {
				if (mActiveButtonIndex + 1 != mCategoryIndexList.Count) {
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex + 1];
					try {
						double top = ProductGrid.Children.ElementAt (productCellIndex).Bounds.Top;					
						if (ProductScrollView.ScrollY > top) {
							mActiveButtonIndex += 1;
							ChangeSelectedButton();
						}
					} catch {
						System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");
					}
				}
					

				if ( ProductScrollView.ScrollY >= ProductGrid.Children.ElementAt (mLastLoadedIndex).Bounds.Bottom-50 ) {
					int endIndex = (int)Math.Ceiling (ProductScrollView.ScrollY / (int)Math.Floor(ProductGrid.Children.ElementAt(0).Height-MyDevice.ViewPadding/2)) * 2 - 1;

					if (endIndex >= ProductGrid.Children.Count) {
						endIndex = ProductGrid.Children.Count - 1;
					} 

					mLastLoadedIndex = endIndex;

				}
			}else {				

				if (mActiveButtonIndex != 0) {					
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex];
					try{
						double top = ProductGrid.Children.ElementAt (productCellIndex).Bounds.Top;
						if (ProductScrollView.ScrollY < top) {
							mActiveButtonIndex -= 1;
							ChangeSelectedButton();
						}
					}
					catch{
						System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");	
					}

				}

				if (mLastLoadedIndex >= ProductGrid.Children.Count)
					mLastLoadedIndex = ProductGrid.Children.Count - 1;

				if (ProductScrollView.ScrollY <= ProductGrid.Children.ElementAt (mLastLoadedIndex).Bounds.Top) {
					int endIndex = (int)Math.Floor (ProductScrollView.ScrollY / (int)Math.Floor(ProductGrid.Children.ElementAt(0).Height-MyDevice.ViewPadding/2)) * 2 - 1;

					if (endIndex < 0) {
						endIndex = 0;
					}
					else if( endIndex >= ProductGrid.Children.Count )
						endIndex =	ProductGrid.Children.Count - 1;

					mLastLoadedIndex = endIndex;
				}
			}				
		}				


		private string DecideIfIsUpOrDown(ScrollView scrollView)
		{
			string rotation = "";
			if (scrollView.ScrollY >= mPreviousScrollPositionY)
				rotation = "Down";
			else
				rotation = "Up";

			mPreviousScrollPositionY = scrollView.ScrollY;

			return rotation;
		}

*/	

		private void FocusSelectedButton(Label selectedButton)
		{		
			
			mActiveButtonIndex = mButtonList.IndexOf (selectedButton);			
			int productCellIndex = mCategoryIndexList [mActiveButtonIndex]+2;
			productCellIndex = productCellIndex == 2 ? 0 : productCellIndex;
			System.Diagnostics.Debug.WriteLine ("aa" + mActiveButtonIndex.ToString());
			ChangeSelectedButton ();
			//System.Diagnostics.Debug.WriteLine ("aa" + ProductGrid._initialIndex.ToString());
			//ProductGrid.GridViewProvider.ScrollToItemWithIndex (productCellIndex,true);
			/*if(ProductGrid.GridViewProvider==null)
			{
				IGridViewProvider _gridViewProvider = ProductGrid;
				System.Diagnostics.Debug.WriteLine ("aa");
				ProductGrid.GridViewProvider = _gridViewProvider;
				_gridViewProvider.ScrollToItemWithIndex (productCellIndex,false);
			}*/
			ProductGrid.ScrollToItemWithIndex (productCellIndex,false);
			/*
			lock (_ListLock) {
				mManagerProductCellQueue.Clear ();
			}

			try
			{
				if( productCellIndex < ProductGrid.Children.Count )
					await ProductScrollView.ScrollToAsync (ProductGrid.Children.ElementAt (productCellIndex), ScrollToPosition.Start, true);
				else
				{
					await WaitUntilCorrespondingSubCategoryLoaded(productCellIndex);
					await ProductScrollView.ScrollToAsync (ProductGrid.Children.ElementAt (productCellIndex), ScrollToPosition.Start, true);
				}
			}
			catch{
				System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");
			}	*/			
		}

		private void ChangeSelectedButton()
		{
			mEnabledBoxView.IsVisible = false;
			mEnabledBoxView = mBoxViewList [mActiveButtonIndex];
			mEnabledBoxView.IsVisible = true;
			SubcategoryScrollView.ScrollToAsync(mEnabledBoxView,ScrollToPosition.Center,true);
			System.Diagnostics.Debug.WriteLine ("bb" + mActiveButtonIndex.ToString());
		}
		/*
		private void PopulateGrid()
		{
			SetGrid2Definitions ();


			//Populate a list with all products 
			//To be able to define product index
			var valueList = mProductDictionary.Values.Cast<List<Product>> ().ToList();
			//var tempProductList = new List<Product> ();
			foreach (var products in valueList) {
				if (products.Count > 0) {
					mCategoryIndexList.Add (mProductList.Count);
					foreach (var tempProduct in products) {					
						mProductList.Add (tempProduct);	
					}
				}
			}
			//PopulateSubCategoryButtons ();
			//LoadLimitedNumberOfProducts (1000);
			LoadAllProducts();
			ManageQueuesInBackground ();
			PopulateProductCellInBackground ();
			//EraseProductCellInBackground ();
			CheckIfLastIndexChanged ();

			//LoadInitialImages ();
		}
*/
		private async void CheckIfLastIndexChanged(){

			while (true) {
				if (mLastLoadedIndex != mLastScrollIndex) {
					lock (_ListLock) {
						mManagerProductCellQueue.Clear ();
					}

					mLastScrollIndex = mLastLoadedIndex;
					bIsImagesProduced = false;
				} else {
					if (!bIsImagesProduced) {
						bIsImagesProduced = true;
						for (int i = 0; i < 8; i++) {
							int next = mLastLoadedIndex + i;
							int prev = mLastLoadedIndex - i;
							if (next < mProductCellList.Count /*&& !mTrashProductCellQueue.Contains (mProductCellList [mLastLoadedIndex + i])*/) {							
								lock (_ListLock) {									
									mManagerProductCellQueue.Enqueue (mProductCellList [next]);
								}
							}
							if (prev > 0 /*&& !mTrashProductCellQueue.Contains (mProductCellList [prev])*/) {							
								lock (_ListLock) {									
									mManagerProductCellQueue.Enqueue (mProductCellList [prev]);
								}
							}
						}
					}
				}
				await Task.Delay (100);
			}
		}

		private async void ManageQueuesInBackground()
		{
			while (true) {
				while (mManagerProductCellQueue.Count > mLoadSize) {
					lock (_ListLock) {
						mManagerProductCellQueue.Dequeue ();
					}
				}

				if( mManagerProductCellQueue.Count > 0 ){
					ProductCell productCell;

					lock (_ListLock) {
						productCell = mManagerProductCellQueue.Dequeue ();
					}

					if (!mPopulaterProductCellQueue.Contains (productCell)) {
						mPopulaterProductCellQueue.Enqueue (productCell);	
					}
				}

				await Task.Delay (100);	
			}
		}

		private async void EraseProductCellInBackground()
		{
			while (true) {
				if (mTrashProductCellQueue.Count > mLoadSize) {
					var	productCell = mTrashProductCellQueue.Dequeue ();

					if (productCell.bIsImageSet) {
						productCell.bIsImageSet = false;
						productCell.ClearStreamsAndImages ();
						productCell.mProductImage.Source = null;
					}

				}
				await Task.Delay (100);	
			}
		}

		private async void PopulateProductCellInBackground()
		{
			while (true) {				

				if (mPopulaterProductCellQueue.Count > 0) {
					var productCell = mPopulaterProductCellQueue.Dequeue ();	
					if (!productCell.bIsImageSet) {	
						productCell.bIsImageSet = true;
						mTrashProductCellQueue.Enqueue (productCell);
						productCell.ProduceProductImages ();
					}
				}

				await Task.Delay (100);				

			}
		}

		private async void LoadLimitedNumberOfProducts(int count)
		{	
			/*foreach (var product in mProductList) {
				int productIndex = mProductList.IndexOf (product);
				if (productIndex == count)
					break;

				ProductCell productCell = new ProductCell (ProductGrid, product, this);

				mProductCellList.Add (productCell);					
			
				ProductGrid.Children.Add (productCell.View, productIndex % 2, productIndex / 2);			
				productCell.ProduceStreamsAndImages ();
				 
				await Task.Delay (100);
			}*/
		}

		private int CheckIfProductIsInTopSellingListAndReturnIndex(Product p)
		{			
			int counter = 0;
			foreach (var product in mTopSellingProductList) {				
				if (p.ProductID == product.ProductID)
					return counter;
				counter++;
			}

			return counter;
		}

		private async void LoadAllProducts()
		{	
			/*foreach (var product in mProductList) {
				int productIndex = mProductList.IndexOf (product);
				ProductCell productCell;


					
				if (productIndex < mTopSellingProductList.Count) {
					productCell = new ProductCell (ProductGrid, product, this);
					mTopSellingProductCellList.Add (productCell);
				} else {
					int index = CheckIfProductIsInTopSellingListAndReturnIndex (product);

					if (index != mTopSellingProductList.Count) {
						productCell = new ProductCell (ProductGrid, mTopSellingProductCellList [index].mProduct, this);
						productCell.mPairCell = mTopSellingProductCellList [index];
						mTopSellingProductCellList [index].mPairCell = productCell;
					}
					else
						productCell = new ProductCell (ProductGrid, product, this);					
				}

				mProductCellList.Add (productCell);					

				//productCell.ProduceStreamsAndImages ();	
				ProductGrid.Children.Add (productCell.View, productIndex % 2, productIndex / 2);			



				if( productIndex < mInitialLoadSize )
					mManagerProductCellQueue.Enqueue (productCell);
				
				try{
					await Task.Delay (50,mLoadProductsToken.Token);
				}
				catch {
					mProductList.Clear ();
					break;
				}
			}*/
		}

		private void SetGrid2Definitions()
		{
			//SubCategoryStackLayout.Spacing = MyDevice.ViewPadding;
			//SubcategoryScrollView.Padding = MyDevice.ViewPadding/2;
			/*for (int i = 0; i < mRowCount; i++) 
			{
				Grid2.RowDefinitions.Add (new RowDefinition ());
			}*/
			/*
			ProductGrid.Padding = new Thickness (MyDevice.GetScaledSize(12), 0, 0, 0); 
			ProductGrid.ColumnSpacing = MyDevice.GetScaledSize (10);*/
			/*ProductGrid.ColumnDefinitions.Add (new ColumnDefinition(){Width = (MyDevice.ScreenWidth-ProductGrid.ColumnSpacing-MyDevice.ViewPadding)/2});
			ProductGrid.ColumnDefinitions.Add (new ColumnDefinition(){Width = (MyDevice.ScreenWidth-ProductGrid.ColumnSpacing-MyDevice.ViewPadding)/2}); */
			/*
			ProductGrid.ColumnDefinitions.Add (new ColumnDefinition(){Width = MyDevice.GetScaledSize (300)});
			ProductGrid.ColumnDefinitions.Add (new ColumnDefinition(){Width = MyDevice.GetScaledSize (300)});*/
		}			
	}
}

