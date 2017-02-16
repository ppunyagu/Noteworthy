using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Android.Widget;
using System.Text;

namespace Noteworthy
{
	public class TranslationService : IDisposable
	{

		public int ConvertAudioToText(string AudioFilePath)
		{
			try
			{
				// Read file data
				string[] filePathSplit = AudioFilePath.Split('/');
				string fileName = filePathSplit[filePathSplit.Length - 1];

				FileStream fs = new FileStream(AudioFilePath, FileMode.Open, FileAccess.Read);
				byte[] data = new byte[fs.Length];
				fs.Read(data, 0, data.Length);
				fs.Close();

				// Generate post objects
				Dictionary<string, object> postParameters = new Dictionary<string, object>();
				postParameters.Add("model", "en-US");
				if (fileName == "zero.wav")			// Testing purposes
				{
					postParameters.Add("data_file", new FormUpload.FileParameter(data, "zero.wav", "audio/wav"));
				}
				else {
					postParameters.Add("data_file", new FormUpload.FileParameter(data, fileName, "audio/mpeg"));
				}

				// Create request and receive response
				string postURL = "https://api.speechmatics.com/v1.0/user/15669/jobs/?auth_token=MGZiODZkYjAtZWZkZS00MTk3LTkyNTQtYjVjMmRlY2Y3Nzdh";
				HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, "", postParameters);

				// Process response
				StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
				string fullResponse = responseReader.ReadToEnd();
				TranslationServiceJobSent res = JsonConvert.DeserializeObject<TranslationServiceJobSent>(fullResponse);
				webResponse.Close();
				return res.id;
				//Response.Write(fullResponse);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("TranslationService", "ConvertAudioToText2", ex);
			}
			return 0;
		}

		public string GetTextFromJobId(int JobId)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create("https://api.speechmatics.com/v1.0/user/15669/jobs/"+ JobId + "/transcript?format=txt&auth_token=MGZiODZkYjAtZWZkZS00MTk3LTkyNTQtYjVjMmRlY2Y3Nzdh");
				request.Method = "GET";

				WebResponse response = request.GetResponse();
				Stream dataStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(dataStream);
				return reader.ReadToEnd();
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("TranslationService", "GetTextFromJobId", ex);
				return "still transcribing";
			}
		}
		
		public void Dispose()
		{
			// Clear all property values that maybe have been set
			// when the class was instantiated

		}

		class TranslationServiceJobSent
		{
			/*
			[JsonProperty("balance")]
			public int balance { get; set; }
			*/

			[JsonProperty("id")]
			public int id { get; set; }
			//public user user { get; set; }
		}
	}
}
