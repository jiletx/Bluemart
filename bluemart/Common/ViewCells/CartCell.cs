﻿using System;
using Xamarin.Forms;
using bluemart.Common.Utilities;
using bluemart.Common.Objects;
using System.Threading.Tasks;
using bluemart.MainViews;
using bluemart.Models.Local;
using FFImageLoading.Forms;
using bluemart.Models.Remote;

namespace bluemart
{
	public class CartCell : ViewCell
	{
		private CachedImage mAddImage;
		private CachedImage mRemoveImage;
		private CachedImage mFavoriteImage;
		private Label mProductNumberLabel;
		private Label mProductPriceLabel;
		private Product mProduct;
		private Page mParentPage;
		private bool bIsFavorite;
		private bool IsOpen = false;
		//change PriceLabel
		//int mQuantity = 0;
		//string mQuantityLabel;

		private FavoritesClass mFavoriteModel = new FavoritesClass();

		public CartCell (Product product,Page parent)
		{
			//change PriceLabel
			//mQuantity = Convert.ToInt32 (product.Quantity.Split (' ') [0]);
			//mQuantityLabel = product.Quantity.Split (' ') [1];
			mProduct = product; 
			mParentPage = parent;
			bIsFavorite = mFavoriteModel.IsProductFavorite (product.ProductID);

			Grid mainCellView = new Grid (){
				RowSpacing = 0, 
				ColumnSpacing = 0, 
				Padding = 0,
				BackgroundColor=Color.FromRgb(51,51,51),
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = MyDevice.GetScaledSize(150),
				WidthRequest = MyDevice.GetScaledSize(535)
			};

			//mainCellView.BackgroundColor = Color.Red;
			mainCellView.ColumnDefinitions.Add (new ColumnDefinition (){ Width = MyDevice.GetScaledSize(535) } );
			mainCellView.RowDefinitions.Add (new RowDefinition (){ Height = MyDevice.GetScaledSize(148) } );
			mainCellView.RowDefinitions.Add (new RowDefinition (){ Height = MyDevice.GetScaledSize(2) } );

			var mainLayout = new RelativeLayout () {
				HeightRequest = MyDevice.GetScaledSize(148),
				WidthRequest = MyDevice.GetScaledSize(535),
				Padding = 0,
				BackgroundColor = Color.FromRgb(253,59,47)
			};

			var cellLayout = new MR.Gestures.RelativeLayout () {
				HeightRequest = MyDevice.GetScaledSize(148),
				WidthRequest = MyDevice.GetScaledSize(535),
				Padding = 0,
				BackgroundColor=Color.FromRgb(51,51,51)
			};

			var productImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(126),
				HeightRequest = MyDevice.GetScaledSize(122),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false
			};

			/*PCLStorage.IFolder folder=null;

			if (mParentPage is BrowseProductsPage) {
				folder = (mParentPage as BrowseProductsPage).mParent.mFolder;
			} else if (mParentPage is FavoritesPage) {
				folder = (mParentPage as FavoritesPage).mParent.mFolder;
			} else if (mParentPage is SearchPage) {
				folder = (mParentPage as SearchPage).mParent.mFolder;
			}*/
			/*
			if( folder.CheckExistsAsync(ProductModel.mRootFolderPath + "/" + ParseConstants.IMAGE_FOLDER_NAME + "/" + mProduct.ProductImageName).Result != PCLStorage.ExistenceCheckResult.NotFound)
				productImage.Source = ProductModel.mRootFolderPath + "/" + ParseConstants.IMAGE_FOLDER_NAME + "/" + mProduct.ProductImageName;
			else
				productImage.Source = ImageSource.FromResource("bluemart.SavedImages."+mProduct.ProductImageName);
*/
			productImage.Source = mProduct.ProductImagePath;

			var imageMask = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(126),
				HeightRequest = MyDevice.GetScaledSize(122),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false,
				Source = "CartPage_ImageMask.png"
			};

			mFavoriteImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(39),
				HeightRequest = MyDevice.GetScaledSize(32),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false
			};

			var favoriteButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(84),
				HeightRequest = MyDevice.GetScaledSize(71),
				BackgroundColor = Color.Transparent
			};

			if (!bIsFavorite) {
				mFavoriteImage.Source = "CartPage_AddFavorites.png";
			} else {
				mFavoriteImage.Source = "CartPage_RemoveFavorites.png";
			}

			mProductPriceLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize(102),
				HeightRequest = MyDevice.GetScaledSize(40),
				HorizontalTextAlignment = TextAlignment.End,
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMedium
			};

			UpdatePriceLabel();

			var productNameLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize(225),
				HeightRequest = MyDevice.GetScaledSize(40),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeMicro,
				Text = product.Name
			};

			var productQuantityLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize(225),
				HeightRequest = MyDevice.GetScaledSize(26),
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Start,
				TextColor = Color.FromRgb(152,152,152),
				FontSize = MyDevice.FontSizeMicro,
				Text = product.Quantity
			};

			mRemoveImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(27),
				HeightRequest = MyDevice.GetScaledSize(21),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false,
				Source = "CartPage_RemoveProduct.png"
			};

			var removeProductButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(59),
				HeightRequest = MyDevice.GetScaledSize(49),
				BackgroundColor = Color.Transparent
			};

			mProductNumberLabel = new Label () {
				WidthRequest = MyDevice.GetScaledSize(120),
				HeightRequest = MyDevice.GetScaledSize(47),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				BackgroundColor = Color.FromRgb(152,152,152),
				TextColor = Color.FromRgb(51,51,51),
				FontSize = MyDevice.FontSizeSmall
			};
					

			UpdateNumberLabel ();

			mAddImage = new CachedImage () {
				WidthRequest = MyDevice.GetScaledSize(27),
				HeightRequest = MyDevice.GetScaledSize(21),
				CacheDuration = TimeSpan.FromDays(30),
				DownsampleToViewSize = true,
				RetryCount = 10,
				RetryDelay = 250,
				TransparencyEnabled = false,
				FadeAnimationEnabled = false,
				Source = "CartPage_AddProduct.png"
			};

			var addProductButton = new RelativeLayout () {
				WidthRequest = MyDevice.GetScaledSize(59),
				HeightRequest = MyDevice.GetScaledSize(49),
				BackgroundColor = Color.Transparent
			};

			var deleteButton = new Label () {
				WidthRequest = MyDevice.GetScaledSize(167),
				HeightRequest = MyDevice.GetScaledSize(147),
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				BackgroundColor = Color.FromRgb(253,59,47),
				TextColor = Color.White,
				FontSize = MyDevice.FontSizeSmall,
				Text = "DELETE"
			};

			var cartTapRecogniser = new TapGestureRecognizer ();

			cartTapRecogniser.Tapped += (sender, e) => {
				if( !IsOpen )
					cellLayout.TranslateTo( MyDevice.GetScaledSize(-164),0,300,Easing.Linear);
				else
					cellLayout.TranslateTo( MyDevice.GetScaledSize(0),0,300,Easing.Linear);
				IsOpen = !IsOpen;
			};

			cellLayout.GestureRecognizers.Add (cartTapRecogniser);

			cellLayout.Swiped += (object sender, MR.Gestures.SwipeEventArgs e) => 
			{
				if( !IsOpen && e.Direction == MR.Gestures.Direction.Left )
					cellLayout.TranslateTo( MyDevice.GetScaledSize(-164),0,300,Easing.Linear);
				else if( IsOpen && e.Direction == MR.Gestures.Direction.Right )
					cellLayout.TranslateTo( MyDevice.GetScaledSize(0),0,300,Easing.Linear);
				IsOpen = !IsOpen;	
			};

			var deleteButtonTapRecogniser = new TapGestureRecognizer ();
			deleteButtonTapRecogniser.Tapped += (sender, e) => {
				Cart.ProductTotalPrice -= (mProduct.ProductNumberInCart-1)*mProduct.Price;
				mProduct.ProductNumberInCart = 1;

				if (mParentPage is BrowseProductsPage) {	
					foreach (var productCell in (mParentPage as BrowseProductsPage).mProductCellList) {
						if (productCell.mProduct.ProductID == mProduct.ProductID) {
							
							productCell.DeactivateAddMenu ();
						}
					}
				}
				else if (mParentPage is SearchPage) {	
					foreach (var productCell in (mParentPage as SearchPage).mProductCellList) {
						if (productCell.mProduct.ProductID == mProduct.ProductID) {
							productCell.DeactivateAddMenu ();
						}
					}
				}
				else if (mParentPage is FavoritesPage) {				
					foreach (var productCell in (mParentPage as FavoritesPage).mProductCellList) {
						if (productCell.mProduct.ProductID == mProduct.ProductID) {
							productCell.DeactivateAddMenu ();
						}
					}
				}

				RemoveProductFromCart();
			};
			deleteButton.GestureRecognizers.Add (deleteButtonTapRecogniser);

			var removeProductTapRecogniser = new TapGestureRecognizer ();
			removeProductTapRecogniser.Tapped += (sender, e) => {
				RemoveProductFromCart();
			};
			removeProductButton.GestureRecognizers.Add (removeProductTapRecogniser);

			var addProductTapRecogniser = new TapGestureRecognizer ();
			addProductTapRecogniser.Tapped += (sender, e) => {
				AddProductInCart();
			};
			addProductButton.GestureRecognizers.Add (addProductTapRecogniser);

			var favoriteTapRecogniser = new TapGestureRecognizer ();
			favoriteTapRecogniser.Tapped += (sender, e) => {
				if( !bIsFavorite )
				{
					mFavoriteModel.AddProductID(mProduct.ProductID);
					mFavoriteImage.Source = "CartPage_RemoveFavorites.png";
					bIsFavorite = true;
				}
				else
				{					
					mFavoriteModel.RemoveProductID(mProduct.ProductID);
					mFavoriteImage.Source = "CartPage_AddFavorites.png";
					bIsFavorite = false;
				}
			};
			favoriteButton.GestureRecognizers.Add (favoriteTapRecogniser);


			mainLayout.Children.Add (deleteButton,
				Constraint.Constant (MyDevice.GetScaledSize (368)),
				Constraint.Constant (0)
			);

			mainLayout.Children.Add (cellLayout,
				Constraint.Constant (0),
				Constraint.Constant (0)
			);
											
			cellLayout.Children.Add (productImage,
				Constraint.Constant (MyDevice.GetScaledSize(12)),
				Constraint.Constant (MyDevice.GetScaledSize(12))
			);

			cellLayout.Children.Add (imageMask,
				Constraint.RelativeToView (productImage, (p,sibling) => {
					return sibling.Bounds.Left;
				}),
				Constraint.RelativeToView (productImage, (p,sibling) => {
					return sibling.Bounds.Top;
				})
			);

			cellLayout.Children.Add (mFavoriteImage,
				Constraint.Constant (MyDevice.GetScaledSize(479)),
				Constraint.Constant (MyDevice.GetScaledSize(27))
			);

			cellLayout.Children.Add (favoriteButton,
				Constraint.RelativeToView(mFavoriteImage, (p,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(24);
				}),	
				Constraint.Constant(0)
			);


			cellLayout.Children.Add (mProductPriceLabel,
				Constraint.RelativeToView(mFavoriteImage, (p,sibling) => {
					return sibling.Bounds.Right - MyDevice.GetScaledSize(102);
				}),	
				Constraint.RelativeToView(mFavoriteImage, (p,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(30);
				})
			);

			cellLayout.Children.Add (productNameLabel,
				Constraint.RelativeToView(mFavoriteImage, (p,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(278);
				}),	
				Constraint.RelativeToView(mFavoriteImage, (p,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(8);
				})
			);

			cellLayout.Children.Add (productQuantityLabel,
				Constraint.RelativeToView(productNameLabel, (p,sibling) => {
					return sibling.Bounds.Left;
				}),	
				Constraint.RelativeToView(productNameLabel, (p,sibling) => {
					return sibling.Bounds.Bottom;
				})
			);

			cellLayout.Children.Add (mRemoveImage,
				Constraint.RelativeToView(productQuantityLabel, (p,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(7);
				}),	
				Constraint.RelativeToView(productQuantityLabel, (p,sibling) => {
					return sibling.Bounds.Bottom + MyDevice.GetScaledSize(15);
				})
			);

			cellLayout.Children.Add (removeProductButton,
				Constraint.RelativeToView(mRemoveImage, (p,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(11);
				}),	
				Constraint.RelativeToView(mRemoveImage, (p,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(14);
				})
			);

			cellLayout.Children.Add (mProductNumberLabel,
				Constraint.RelativeToView(mRemoveImage, (p,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize(18);
				}),	
				Constraint.RelativeToView(mRemoveImage, (p,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(14);
				})
			);

			cellLayout.Children.Add (mAddImage,
				Constraint.RelativeToView(mProductNumberLabel, (p,sibling) => {
					return sibling.Bounds.Right + MyDevice.GetScaledSize(18);
				}),	
				Constraint.RelativeToView(mRemoveImage, (p,sibling) => {
					return sibling.Bounds.Top;
				})
			);

			cellLayout.Children.Add (addProductButton,
				Constraint.RelativeToView(mAddImage, (p,sibling) => {
					return sibling.Bounds.Left - MyDevice.GetScaledSize(11);
				}),	
				Constraint.RelativeToView(mAddImage, (p,sibling) => {
					return sibling.Bounds.Top - MyDevice.GetScaledSize(14);
				})
			);

			var bottomLayout = new RelativeLayout () {
				HeightRequest = MyDevice.GetScaledSize(2),
				WidthRequest = MyDevice.GetScaledSize(535),
				BackgroundColor = Color.FromRgb(129,129,129),
				Padding = 0
			};

			mainCellView.Children.Add (mainLayout, 0, 0);
			mainCellView.Children.Add (bottomLayout, 0, 1);

			this.View = mainCellView;
		}

		private void UpdateNumberLabel()
		{
			mProductNumberLabel.Text = mProduct.ProductNumberInCart.ToString() + " x " + mProduct.Price;
			//change PriceLabel
			//mProductNumberLabel.Text = mProduct.ProductNumberInCart.ToString()+ " " + mQuantityLabel;
		}

		private void UpdatePriceLabel()
		{
			mProductPriceLabel.Text = (mProduct.Price * mProduct.ProductNumberInCart).ToString();
			//change PriceLabel
			//mProductPriceLabel.Text = "DH " + (mProduct.Price * mProduct.ProductNumberInCart/mQuantity).ToString();
		}

		private void AddTapRecognizers()
		{
			var addButtonTapGestureRecognizer = new TapGestureRecognizer ();
			addButtonTapGestureRecognizer.Tapped += async (sender, e) => {

				mAddImage.Opacity = 0.5f;
				AddProductInCart();
				await Task.Delay(MyDevice.DelayTime);
				mAddImage.Opacity = 1f;
			};
			mAddImage.GestureRecognizers.Add (addButtonTapGestureRecognizer);

			var removeButtonTapGestureRecognizer = new TapGestureRecognizer ();
			removeButtonTapGestureRecognizer.Tapped += async (sender, e) => {

				mRemoveImage.Opacity = 0.5f;
				RemoveProductFromCart();
				await Task.Delay(MyDevice.DelayTime);
				mRemoveImage.Opacity = 1f;
			};
			mRemoveImage.GestureRecognizers.Add (removeButtonTapGestureRecognizer);

			var favoriteButtonTapGestureRecognizer = new TapGestureRecognizer ();
			favoriteButtonTapGestureRecognizer.Tapped += async (sender, e) => {
				if( !bIsFavorite )
				{
					mFavoriteImage.Opacity = 0.5f;
					mFavoriteModel.AddProductID(mProduct.ProductID);
					await Task.Delay(MyDevice.DelayTime);
					mFavoriteImage.Opacity = 1f;
					mFavoriteImage.Source = "bookmark_remove.png";
					bIsFavorite = true;
				}
				else
				{					
					mFavoriteImage.Opacity = 0.5f;
					mFavoriteModel.RemoveProductID(mProduct.ProductID);
					await Task.Delay(MyDevice.DelayTime);
					mFavoriteImage.Opacity = 1f;
					mFavoriteImage.Source = "bookmark_add.png";
					bIsFavorite = false;
				}
			};

			mFavoriteImage.GestureRecognizers.Add (favoriteButtonTapGestureRecognizer);
		}

		private void RemoveProductFromCart()
		{
			if (mProduct.ProductNumberInCart > 0)
				mProduct.ProductNumberInCart--;
				//change PriceLabel
				//mProduct.ProductNumberInCart -= Convert.ToInt32 (mProduct.Quantity.Split (' ') [0]);
			if (mProduct.ProductNumberInCart == 0) {
				//mParentPage.RemoveProductFromCart (this.View);
				/*if (mParentPage is BrowseCategoriesPage) {					
					(mParentPage as BrowseCategoriesPage).CartStackLayout.Children.Remove (this.View);
				}
				else if (mParentPage is BrowseProductsPage) {					
					(mParentPage as BrowseProductsPage).CartStackLayout.Children.Remove (this.View);
				}
				else if (mParentPage is SearchPage) {					
					(mParentPage as SearchPage).CartStackLayout.Children.Remove (this.View);
				}
				else if (mParentPage is FavoritesPage) {					
					(mParentPage as FavoritesPage).CartStackLayout.Children.Remove (this.View);
				}*/
				Cart.ProductsInCart.Remove (mProduct);
			}

			UpdateNumberLabel ();
			Cart.ProductTotalPrice -= mProduct.Price;
			UpdatePriceLabel();


			if (mParentPage is BrowseCategoriesPage) {		
				
				(mParentPage as BrowseCategoriesPage).UpdateProductCountLabel();
				(mParentPage as BrowseCategoriesPage).UpdatePriceLabel();
				(mParentPage as BrowseCategoriesPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as BrowseCategoriesPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is BrowseProductsPage) {
				foreach (var productCell in (mParentPage as BrowseProductsPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						if (mProduct.ProductNumberInCart == 0) {
							productCell.DeactivateAddMenu ();
						}
						productCell.UpdateNumberLabel ();
					}
				}
				(mParentPage as BrowseProductsPage).UpdateProductCountLabel();
				(mParentPage as BrowseProductsPage).UpdatePriceLabel();
				(mParentPage as BrowseProductsPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as BrowseProductsPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is SearchPage) {
				foreach (var productCell in (mParentPage as SearchPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						if (mProduct.ProductNumberInCart == 0) {
							productCell.DeactivateAddMenu ();
						}
						productCell.UpdateNumberLabel ();
					}
				}
				(mParentPage as SearchPage).UpdateProductCountLabel();
				(mParentPage as SearchPage).UpdatePriceLabel();
				(mParentPage as SearchPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as SearchPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is FavoritesPage) {	
				foreach (var productCell in (mParentPage as FavoritesPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						if (mProduct.ProductNumberInCart == 0) {
							productCell.DeactivateAddMenu ();
						}
						productCell.UpdateNumberLabel ();
					}
				}
				(mParentPage as FavoritesPage).UpdateProductCountLabel();
				(mParentPage as FavoritesPage).UpdatePriceLabel();
				(mParentPage as FavoritesPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as FavoritesPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}

		}

		private void AddProductInCart()
		{
			if (!Cart.ProductsInCart.Contains (mProduct)) 
			{
				Cart.ProductsInCart.Add (mProduct);
			}

			mProduct.ProductNumberInCart++;    
			//change PriceLabel
			//mProduct.ProductNumberInCart += Convert.ToInt32 (mProduct.Quantity.Split (' ') [0]);
			UpdateNumberLabel ();
			Cart.ProductTotalPrice += mProduct.Price;
			UpdatePriceLabel();

			if (mParentPage is BrowseCategoriesPage) {					
				(mParentPage as BrowseCategoriesPage).UpdateProductCountLabel();
				(mParentPage as BrowseCategoriesPage).UpdatePriceLabel();
				(mParentPage as BrowseCategoriesPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as BrowseCategoriesPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is BrowseProductsPage) {
				foreach (var productCell in (mParentPage as BrowseProductsPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						productCell.UpdateNumberLabel ();
					}
				}
				(mParentPage as BrowseProductsPage).UpdateProductCountLabel();
				(mParentPage as BrowseProductsPage).UpdatePriceLabel();
				(mParentPage as BrowseProductsPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as BrowseProductsPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is SearchPage) {	
				foreach (var productCell in (mParentPage as SearchPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						productCell.UpdateNumberLabel ();
					}
				}

				(mParentPage as SearchPage).UpdateProductCountLabel();
				(mParentPage as SearchPage).UpdatePriceLabel();
				(mParentPage as SearchPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as SearchPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
			else if (mParentPage is FavoritesPage) {	
				foreach (var productCell in (mParentPage as FavoritesPage).mProductCellList) {
					if (productCell.mProduct.ProductID == mProduct.ProductID) {
						productCell.UpdateNumberLabel ();
					}
				}

				(mParentPage as FavoritesPage).UpdateProductCountLabel();
				(mParentPage as FavoritesPage).UpdatePriceLabel();
				(mParentPage as FavoritesPage).subtotalPriceLabel.Text = Cart.ProductTotalPrice.ToString();
				(mParentPage as FavoritesPage).checkoutPriceLabel.Text = "AED " + Cart.ProductTotalPrice.ToString ();
			}
		}
	}
}

