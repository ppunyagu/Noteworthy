﻿using System;
using System.IO;
using System.Net;
using Android.Util;

namespace Noteworthy
{
	public class GetWebPulseService : IDisposable
	{
		public bool IsPulseStressed()
		{
			WebRequest request = WebRequest.Create("http://192.168.1.41:5000");
			request.Method = "GET";

			WebResponse response = request.GetResponse();
			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			string serverResponse = reader.ReadToEnd();
			Log.Debug("GetWebPulseService", string.Format("Pulse: {0}", serverResponse));
			int pulse = Convert.ToInt32(serverResponse);
			if (pulse > 130)
			{
				return true;
			}
			else {
				return false;
			}
		}
		public void Dispose()
		{
			// Clear all property values that maybe have been set
			// when the class was instantiated

		}
	}
}
