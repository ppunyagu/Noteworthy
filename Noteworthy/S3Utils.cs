using System;
using Amazon.CognitoIdentity;
using Amazon.S3;
//using ObjCRuntime;
using Amazon.S3.Model;
using System.Threading.Tasks;
using Amazon;
using System.Net;
using System.Collections.Generic;

namespace Noteworthy
{
	public class Constants
	{
		// You should replace these values with your own
		public const string COGNITO_POOL_ID = "us-east-1:64666f4d-df10-474c-b281-3b284739e5c2";

		// Note, the bucket will be created in all lower case letters
		// If you don't enter an all lower case title, any references you add
		// will need to be sanitized
		public const string BUCKET_NAME = "snapbag";

		public const string ImagePath = "https://s3-ap-southeast-1.amazonaws.com/snapbag/";

		public static RegionEndpoint REGION = RegionEndpoint.USEast1;

		public const HttpStatusCode NO_SUCH_BUCKET_STATUS_CODE = HttpStatusCode.NotFound;
		public const HttpStatusCode BUCKET_ACCESS_FORBIDDEN_STATUS_CODE = HttpStatusCode.Forbidden;
		public const HttpStatusCode BUCKET_REDIRECT_STATUS_CODE = HttpStatusCode.Redirect;

	}


	public class S3Utils
	{
		private static CognitoAWSCredentials cognitoCredentials;
		private static IAmazonS3 s3Client;

		public static CognitoAWSCredentials Credentials
		{
			get
			{
				if (cognitoCredentials == null)
				{
					cognitoCredentials = new CognitoAWSCredentials(Constants.COGNITO_POOL_ID, Constants.REGION);
				}
				return cognitoCredentials;
			}
		}

		public static IAmazonS3 S3Client
		{
			get
			{
				if (s3Client == null)
				{
					s3Client = new AmazonS3Client(Credentials, Constants.REGION);
				}
				return s3Client;
			}
		}

		public static async Task<bool> BucketExist()
		{
			try
			{
				await S3Client.ListObjectsAsync(new ListObjectsRequest()
				{
					BucketName = Constants.BUCKET_NAME.ToLowerInvariant(),
					MaxKeys = 0
				}).ConfigureAwait(false);
				return true;
			}
			catch (Exception e)
			{
				Utility.ExceptionHandler("S3Utils", "BucketExist", e);
				return false;
			}
		}

		/* Dangerous on Company's bucket
		public static async Task CreateBucket()
		{
			try
			{
				string name = Constants.BUCKET_NAME.ToLowerInvariant();

				await S3Client.PutBucketAsync(new PutBucketRequest()
				{
					BucketName = name,
					BucketRegion = S3Region.US
				});
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
			}
		}

		public static async Task DeleteBucket()
		{
			try
			{
				string name = Constants.BUCKET_NAME.ToLowerInvariant();
				await S3Client.DeleteBucketAsync(new DeleteBucketRequest()
				{
					BucketName = name,
					BucketRegion = S3Region.US
				});
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message);
			}
		}
		*/

		public static async Task<string> UploadS3Audios(string filePath, string bucketname = "")
		{
			try
			{
				var objS3Client = S3Client;

				if (string.IsNullOrWhiteSpace(bucketname))
				{
					bucketname = "Audio";
				}

				string keyValue = string.Format("{0}_{1}.{2}", bucketname, Guid.NewGuid(), Utility.audio_file_format);

				PutObjectRequest objRequest = new PutObjectRequest()
				{
					BucketName = (bucketname.Equals("Image")) ? Constants.BUCKET_NAME.ToLowerInvariant() : string.Format("{0}/{1}", Constants.BUCKET_NAME.ToLowerInvariant(), bucketname),
					FilePath = filePath,
					Key = keyValue
				};

				await objS3Client.PutObjectAsync(objRequest);

				return string.Format("{0}{1}/{2}", Constants.ImagePath, bucketname, keyValue);
			}
			catch (Exception s3Exception)
			{
				Utility.ExceptionHandler("S3Utils", "UploadS3Audios", s3Exception);
				return string.Empty;
			}
		}
	}
}

