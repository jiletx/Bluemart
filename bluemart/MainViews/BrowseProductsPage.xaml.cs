﻿using System;
using System.Collections.Generic;
using bluemart.Common.Utilities;
using bluemart.Common.Objects;
using bluemart.Common.ViewCells;
using Xamarin.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace bluemart.MainViews
{
	public partial class BrowseProductsPage : ContentPage
	{
		private Dictionary<string,List<Product>> mProductDictionary;
		private List<BoxView> mBoxViewList;
		private List<Label> mButtonList;
		private BoxView mEnabledBoxView;
		private List<int> mCategoryIndexList;
		private double mPreviousScrollPositionY = 0;
		private int mActiveButtonIndex = 0;
		public RootPage mParent;
		public string mCategoryID;
		public Common.SearchBar mSearchBar;
		private List<Product> mProductList = new List<Product> ();
		private int mLoadSize;
		private int mLastLoadedIndex = 0;
		private int mEndIndex = 0;
		private double mScrollDownY = 0f;
		private double mScrollUpY = 0f;
		private List<ProductCell> mProductCellList = new List<ProductCell> ();

		private List<ProductCell> mTempProductCellList = new List<ProductCell> ();
		static readonly Object _ListLock = new Object();

		public BrowseProductsPage (Dictionary<string, List<Product>> productDictionary, Category category,RootPage parent)
		{					
			InitializeComponent ();
			mParent = parent;
			CreationInitialization ();
			PopulationOfNewProductPage (productDictionary, category);
		}

		public void CreationInitialization()
		{
			NavigationPage.SetHasNavigationBar (this, false);
			mSearchBar = SearchBar;
			SearchBar.mParent = mParent;
			mBoxViewList = new List<BoxView> ();
			mButtonList = new List<Label> ();
			mCategoryIndexList = new List<int> ();

			SetGrid1Definitions ();
		}


		public void PopulationOfNewProductPage(Dictionary<string,List<Product>> productDictionary,Category category)
		{	
			mParent.mTopNavigationBar.NavigationText.Text = category.Name;
			mCategoryID = category.CategoryID;
			mProductDictionary = productDictionary;

			if (mProductDictionary.Count <= 1) {
				ScrollView1.IsEnabled = false;
				Grid1.RowDefinitions [1].Height = 0;
				ScrollView1.IsVisible = false;
			}


			int count = 0;
			foreach (var product in productDictionary) {
				count += product.Value.Count;
			}


			PopulateGrid ();
			UpdatePriceLabel ();


		}

		public void ClearContainers()
		{			
			/*mPreviousScrollPositionY = 0;
			mActiveButtonIndex = 0;
			mLoadSize = 20;
			mLastLoadedIndex = 0;
			mTopOfElement = 1000f;*/

			//Clear Product Grid

			//Grid2.
			//Grid2.Children.Clear ();
			//Grid2.RowDefinitions.Clear ();
			//Grid2.ColumnDefinitions.Clear ();
			//Grid2 = null;
			//GC.Collect();
			SubCategoryStackLayout.Children.Clear ();
			mProductDictionary.Clear ();
			mBoxViewList.Clear ();
			mButtonList.Clear ();
			mCategoryIndexList.Clear ();
			foreach (var productCell in mProductCellList) {
				productCell.ClearStreamsAndImages ();
				//GC.Collect ();
			}	


			mProductCellList.Clear ();
			mProductList.Clear ();
		}

		public void  UpdatePriceLabel()
		{
			mParent.mRootHeader.mPriceLabel.Text = "AED:" + Cart.ProductTotalPrice.ToString();
			mParent.mTopNavigationBar.mPriceLabel.Text = "AED:" + Cart.ProductTotalPrice.ToString();
		}

		protected override void OnAppearing()
		{			
			
			//UpdatePriceLabel ();
			//mTopOfElement = Grid2.Children.ElementAt (mLastLoadedIndex-5).Bounds.Top;
		}

		private void SetGrid1Definitions()
		{
			Grid1.RowDefinitions [0].Height = GridLength.Auto;
			Grid1.RowDefinitions [1].Height = GridLength.Auto;//MyDevice.ScreenHeight / 25;
			Grid1.RowDefinitions [2].Height = GridLength.Auto;
			Grid1.ColumnDefinitions [0].Width = MyDevice.ScreenWidth;
			Grid1.BackgroundColor = MyDevice.BlueColor;
		}

		private void PopulateSubCategoryButtons()
		{
			mBoxViewList.Clear ();
			foreach (var productPair in mProductDictionary) {

				var relativeLayout = new RelativeLayout(){					
					VerticalOptions = LayoutOptions.Fill,
					BackgroundColor = Color.Blue,
					Padding = 0
				};

				Label label = new Label () {
					VerticalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = Color.White,
					Text = productPair.Key,
					TextColor = MyDevice.RedColor,
					FontSize = Device.GetNamedSize(NamedSize.Small,typeof(Label))
				};

				var tapRecognizer = new TapGestureRecognizer ();
				tapRecognizer.Tapped += (sender, e) => {
					if( mSearchBar.mSearchEntry.IsFocused )
						return;
					FocusSelectedButton(sender as Label);
				};

				label.GestureRecognizers.Add (tapRecognizer);

				mButtonList.Add (label);
				BoxView boxView = new BoxView (){
					HeightRequest = 3,
					Color = MyDevice.RedColor,
					IsVisible = false
				};
				mBoxViewList.Add (boxView);

				relativeLayout.Children.Add(label, Constraint.RelativeToParent(parent => {
					return 0;	
				}));

				relativeLayout.Children.Add (boxView, 
					Constraint.RelativeToView (label, (parent, sibling) => {
						return sibling.Bounds.Left + 5;
					}),
					Constraint.RelativeToView (label, (parent, sibling) => {
						return sibling.Bounds.Bottom - 3;
					}),
					Constraint.RelativeToView (label, (parent, sibling) => {
						return sibling.Width - 10;
					}));

				//relativeLayout.WidthRequest = my

				SubCategoryStackLayout.Children.Add (relativeLayout);
			}

			mEnabledBoxView = mBoxViewList [mActiveButtonIndex];
			mBoxViewList [mActiveButtonIndex].IsVisible = true;
		}

		private void OnScrolled( Object sender, ScrolledEventArgs e)
		{
			if( mScrollDownY == 0 )
				mScrollDownY = Grid2.Children.ElementAt (0).Bounds.Height * (mLastLoadedIndex/2 - 3);

			if (DecideIfIsUpOrDown (sender as ScrollView) == "Down") {
				if (mActiveButtonIndex + 1 != mCategoryIndexList.Count) {
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex + 1];
					try{
					double top = Grid2.Children.ElementAt (productCellIndex).Bounds.Top;					
					if (ProductScrollView.ScrollY > top) {
						mActiveButtonIndex += 1;
						mEnabledBoxView.IsVisible = false;
						mEnabledBoxView = mBoxViewList [mActiveButtonIndex];
						mEnabledBoxView.IsVisible = true;
						}
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");
					}
				}

				if ( ProductScrollView.ScrollY >= mScrollDownY ) {	
					LoadLimitedNumberOfProductCells (15);
					mScrollDownY = Grid2.Children.ElementAt (0).Bounds.Height * (mLastLoadedIndex/2 - 3);
					mScrollUpY = mScrollDownY - (mLoadSize/2) * Grid2.Children.ElementAt (0).Bounds.Height;

					mEndIndex += mLoadSize;
					ClearBackwards ();
				}


			}else {
				if (mActiveButtonIndex != 0) {					
					int productCellIndex = mCategoryIndexList [mActiveButtonIndex];
					try{
						double top = Grid2.Children.ElementAt (productCellIndex).Bounds.Top;
						if (ProductScrollView.ScrollY < top) {
							mActiveButtonIndex -= 1;
							mEnabledBoxView.IsVisible = false;
							mEnabledBoxView = mBoxViewList [mActiveButtonIndex];
							mEnabledBoxView.IsVisible = true;
						}
					}
					catch{
						System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");	
					}
				
				}

				if ( mEndIndex != 0 && ProductScrollView.ScrollY <= mScrollUpY ) {
					mLastLoadedIndex -= mLoadSize;
					mScrollDownY = Grid2.Children.ElementAt (0).Bounds.Height * (mLastLoadedIndex/2 -3);
					mScrollUpY = mScrollDownY - mLoadSize * Grid2.Children.ElementAt (0).Bounds.Height;
					PopulateBackwards ();
					//await Task.Delay (10);
					mEndIndex -= mLoadSize;
					ClearForward ();
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

		private void FocusSelectedButton(Label selectedButton)
		{
			mEnabledBoxView.IsVisible = false;
			mActiveButtonIndex = mButtonList.IndexOf (selectedButton);
			int productCellIndex = mCategoryIndexList [mActiveButtonIndex];
			try
			{
				ProductScrollView.ScrollToAsync (Grid2.Children.ElementAt (productCellIndex), ScrollToPosition.Start, true);
			}
			catch{
				System.Diagnostics.Debug.WriteLine ("Something is wrong with Product Number in Grid");
			}
				mEnabledBoxView = mBoxViewList [mActiveButtonIndex];
				mEnabledBoxView.IsVisible = true;
		}

		private void PopulateGrid()
		{
			SetGrid2Definitions ();
			PopulateSubCategoryButtons ();

			//Populate a list with all products 
			//To be able to define product index
			var valueList = mProductDictionary.Values.Cast<List<Product>> ().ToList();
			//var tempProductList = new List<Product> ();

			foreach (var products in valueList) {
				mCategoryIndexList.Add (mProductList.Count);
				foreach (var tempProduct in products) {
					mProductList.Add (tempProduct);	
				}
			}

			LoadLimitedNumberOfProductCells (40);			
		}

		private async void ClearForward()
		{
			int endIndex = mLastLoadedIndex+mLoadSize;

			int startIndex = mLastLoadedIndex;

			for (int i = startIndex; i < endIndex; i++) {
				ProductCell productCell;
				//thread safe list
				lock (_ListLock) {
					if (mProductCellList.Count > i)
						productCell = mProductCellList [i];
					else
						break;
				}
				productCell.ClearStreamsAndImages ();
				productCell.mProductImage.Source = null;
				//await Task.Delay (10);
				productCell.mBorderImage.Source = null;		
				await Task.Delay (10);
				/*productCell.mFavoriteImage.Source = null;
				await Task.Delay (5);
				productCell.mAddImage.Source = null;
				await Task.Delay (5);
				productCell.mRemoveImage.Source = null;
				await Task.Delay (5);*/
			}
		}

		private async void ClearBackwards()
		{						
			int startIndex = mEndIndex - mLoadSize;

			for (int i = startIndex; i < mEndIndex; i++) {
				ProductCell productCell;
				//thread safe list
				lock (_ListLock) {
				productCell = mProductCellList [i];
				}
				productCell.ClearStreamsAndImages ();
				productCell.mProductImage.Source = null;
				//await Task.Delay (10);
				productCell.mBorderImage.Source = null;		
				await Task.Delay (10);
				/*productCell.mFavoriteImage.Source = null;
				await Task.Delay (1);
				productCell.mAddImage.Source = null;
				await Task.Delay (1);
				productCell.mRemoveImage.Source = null;
				await Task.Delay (5);*/
			}
		}

		private async void PopulateBackwards()
		{
			
			int startIndex = mEndIndex - mLoadSize;

			for (int i = startIndex; i < mEndIndex; i++) {
				System.Diagnostics.Debug.WriteLine ("Index:" + i);
				ProductCell productCell;
				//thread safe list
				lock (_ListLock) {
					productCell = mProductCellList [i];
				}

				productCell.ReproduceStreamsAndImages ();
				await Task.Delay (10).ConfigureAwait(false);
			}
		}



		private async void LoadLimitedNumberOfProductCellsAndClearForward()
		{			
			/*await Task.Run(() => PopulateBackwards());
			await Task.Run(() => ClearForward());*/
		}



		private async void LoadLimitedNumberOfProductCells(int loadSize)
		{	
			mLoadSize = loadSize;
			
			int tempLastLoadedIndex = mLastLoadedIndex;
			mLastLoadedIndex += loadSize;
			
			foreach (var product in mProductList.Skip(tempLastLoadedIndex)) {

				int productIndex = mProductList.IndexOf (product);

				if (productIndex == tempLastLoadedIndex + loadSize)
					break;

				ProductCell productCell = new ProductCell (Grid2, product, this);
				
				lock (_ListLock) {
					mProductCellList.Add (productCell);					
				}

				Grid2.Children.Add (productCell.View, productIndex % 2, productIndex / 2);					
				await Task.Delay (10);
				productCell.ProduceStreamsAndImages ();
				await Task.Delay (10);
			}
			
		}
			

		private void SetGrid2Definitions()
		{
			SubCategoryStackLayout.Spacing = MyDevice.ViewPadding*3;
			ScrollView1.Padding = MyDevice.ViewPadding/2;
			/*for (int i = 0; i < mRowCount; i++) 
			{
				Grid2.RowDefinitions.Add (new RowDefinition ());
			}*/
			Grid2.Padding = new Thickness (MyDevice.ViewPadding / 2, 0, 0, 0);
			Grid2.ColumnDefinitions.Add (new ColumnDefinition(){Width = (MyDevice.ScreenWidth-Grid2.ColumnSpacing-MyDevice.ViewPadding)/2});
			Grid2.ColumnDefinitions.Add (new ColumnDefinition(){Width = (MyDevice.ScreenWidth-Grid2.ColumnSpacing-MyDevice.ViewPadding)/2}); 
		}			
	}
}

