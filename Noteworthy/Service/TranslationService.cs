﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;

namespace Noteworthy
{
	public class TranslationService : IDisposable
	{

		public void ConvertAudioToText(string AudioFilePath)
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
				postParameters.Add("data_file", new FormUpload.FileParameter(data, fileName, "audio/mpeg"));
				//postParameters.Add("data_file", new FormUpload.FileParameter(data, "zero.wav", "audio/wav"));

				// Create request and receive response
				string postURL = "https://api.speechmatics.com/v1.0/user/15669/jobs/?auth_token=MGZiODZkYjAtZWZkZS00MTk3LTkyNTQtYjVjMmRlY2Y3Nzdh";
				HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, "", postParameters);

				// Process response
				StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
				string fullResponse = responseReader.ReadToEnd();
				webResponse.Close();
				//Response.Write(fullResponse);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("TranslationService", "ConvertAudioToText2", ex);
			}
		}
		
		public void Dispose()
		{
			// Clear all property values that maybe have been set
			// when the class was instantiated

		}
	}

	public class FormUpload
	{
		private static readonly Encoding encoding = Encoding.UTF8;

		public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
		{
			string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
			string contentType = "multipart/form-data; boundary=" + formDataBoundary;

			byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

			return PostForm(postUrl, userAgent, contentType, formData);
		}
		private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
		{
			HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

			if (request == null)
			{
				throw new NullReferenceException("request is not a http request");
			}

			// Set up the request properties.
			request.Method = "POST";
			request.ContentType = contentType;
			//request.UserAgent = userAgent;
			request.CookieContainer = new CookieContainer();
			request.ContentLength = formData.Length;

			// You could add authentication here as well if needed:
			// request.PreAuthenticate = true;
			// request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
			// request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

			// Send the form data to the request.
			using (Stream requestStream = request.GetRequestStream())
			{
				requestStream.Write(formData, 0, formData.Length);
				requestStream.Close();
			}

			return request.GetResponse() as HttpWebResponse;
		}

		private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
		{
			Stream formDataStream = new System.IO.MemoryStream();
			bool needsCLRF = false;

			foreach (var param in postParameters)
			{
				// Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
				// Skip it on the first parameter, add it to subsequent parameters.
				if (needsCLRF)
					formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

				needsCLRF = true;

				if (param.Value is FileParameter)
				{
					FileParameter fileToUpload = (FileParameter)param.Value;

					// Add just the first part of this param, since we will write the file data directly to the Stream
					string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
						boundary,
						param.Key,
						fileToUpload.FileName ?? param.Key,
						fileToUpload.ContentType ?? "application/octet-stream");

					formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

					// Write the file data directly to the Stream, rather than serializing it to a string.
					formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
				}
				else
				{
					string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
						boundary,
						param.Key,
						param.Value);
					formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
				}
			}

			// Add the end of the request.  Start with a newline
			string footer = "\r\n--" + boundary + "--\r\n";
			formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

			// Dump the Stream into a byte[]
			formDataStream.Position = 0;
			byte[] formData = new byte[formDataStream.Length];
			formDataStream.Read(formData, 0, formData.Length);
			formDataStream.Close();

			return formData;
		}

		public class FileParameter
		{
			public byte[] File { get; set; }
			public string FileName { get; set; }
			public string ContentType { get; set; }
			public FileParameter(byte[] file) : this(file, null) { }
			public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
			public FileParameter(byte[] file, string filename, string contenttype)
			{
				File = file;
				FileName = filename;
				ContentType = contenttype;
			}
		}
	}
}
