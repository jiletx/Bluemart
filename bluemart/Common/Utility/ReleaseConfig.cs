﻿using System;
using XLabs.Platform.Device;
using XLabs.Ioc;
using XLabs.Platform.Services;
using Xamarin.Forms;

namespace bluemart.Common.Utilities
{
	public static class ReleaseConfig
	{
		public static DateTime LAST_UPDATEDATE;
		//public static string FONT_PATH = "Arial-Rounded-MT-Bold";
		public static string FONT_PATH = "monospace";
		public static string TOBACCO_ID = "Omwz701hpB";
		static ReleaseConfig()
		{
			DateTime.TryParse ("2016-01-13T07:29:59.275Z", out LAST_UPDATEDATE);
		}
	}
}

