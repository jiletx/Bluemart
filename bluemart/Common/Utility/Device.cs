﻿using System;
using XLabs.Platform.Device;
using XLabs.Ioc;
using XLabs.Platform.Services;
using Xamarin.Forms;

namespace bluemart.Common.Utilities
{
	public static class MyDevice
	{
		public static double DeviceHeight = Resolver.Resolve<IDevice> ().Display.Height;
		public static double DeviceWidth = Resolver.Resolve<IDevice> ().Display.Width;
		public static double DeviceHeightInInches = Resolver.Resolve<IDevice> ().Display.HeightRequestInInches(1);
		public static double DeviceWidthInInches = Resolver.Resolve<IDevice> ().Display.WidthRequestInInches(1);
		public static double ScreenWidthInches = Resolver.Resolve<IDevice> ().Display.ScreenWidthInches ();
		public static double ScreenHeightInches = Resolver.Resolve<IDevice> ().Display.ScreenHeightInches ();
		public static double ScreenWidth = Resolver.Resolve<IDevice> ().Display.WidthRequestInInches (1) * Resolver.Resolve<IDevice> ().Display.ScreenWidthInches ();
		public static double ScreenHeight = Resolver.Resolve<IDevice> ().Display.HeightRequestInInches (1) * Resolver.Resolve<IDevice> ().Display.ScreenHeightInches ();
		public static string NetworkStatus = Resolver.Resolve<IDevice> ().Network.InternetConnectionStatus ().ToString();
		public static Color BlueColor =	Color.FromRgb (12,92,169);
		public static Color RedColor = Color.FromRgb (204,27,39);
	}
}

