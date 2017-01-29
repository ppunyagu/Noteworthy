
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Java.Util;

using Android.Util;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Bluetooth.LE;

namespace Noteworthy
{
	[Activity(Label = "BluetoothLEActivity", MainLauncher = true, Icon = "@mipmap/icon")]
	public class BluetoothLEActivity : Activity
	{
		protected async override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.MainRecording);

			// Set this on if want Audio Background: 
			Utility.server_heartRate = "http://157.252.187.36:5000";
			//NoteworthyApplication.StartBackgroundService();

			//DataBase Initalize
			Utility.InitializeDatabase();

			var Label = FindViewById<TextView>(Resource.Id.textYourText);

			BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

			LEScanCallBack _scanCallBack;

			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
			{
				_scanCallBack = new LEScanCallBack();
				_scanCallBack.OnDeviceFound += (sender, device) =>
				{
					adapter.BluetoothLeScanner.StopScan(_scanCallBack);
					BluetoothGatt mBluetoothGatt;
					BluetoothLEGattCallback mGattCallback = new BluetoothLEGattCallback();
					mGattCallback.OnDeviceReady += (sendFunction, gatt) =>
					{
						try
						{
							byte[] bufferWrite = ASCIIEncoding.Default.GetBytes("hi from up");
							BluetoothGattService service = gatt.GetService(UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e"));
							if (service == null)
							{
								Log.Debug("OnDeviceFound", "Service is null");
								return;
							}
							BluetoothGattCharacteristic characteristic = service.GetCharacteristic(UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e"));
							if (characteristic == null)
							{
								Log.Debug("OnDeviceFound", "Characteristic is null");
								return;
							}
							characteristic.SetValue(bufferWrite);
							characteristic.WriteType = GattWriteType.NoResponse;
							if (gatt.WriteCharacteristic(characteristic))
							{
								Log.Debug("OnDeviceFound", "Write Successfull!");
							}
							else {
								Log.Debug("OnDeviceFound", "Write Unsuccessful... :(");
							}
						}
						catch (Exception ex)
						{
							Utility.ExceptionHandler("BluetoothLEActivity", "OnDeviceFound", ex);
						}
					};
					mBluetoothGatt = device.ConnectGatt(this, true, mGattCallback);
				};
			}
			else {
				throw new Exception("Needs to be greated than API level 21.");
			}

			if (adapter != null && adapter.IsEnabled)
			{
				adapter.BluetoothLeScanner.StartScan(_scanCallBack);
			}
			else {
				throw new Exception("No Bluetooth adapter found or is not enabled.");
			}
		}
	}

	public class LEScanCallBack : ScanCallback
	{

		public delegate void DeviceFoundHandler(object sender, BluetoothDevice device);

		public event DeviceFoundHandler OnDeviceFound;

		public bool isConnecting = false;

		public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
		{
			var handler = OnDeviceFound;
			if (result != null)
			{
				if (result.Device.Address == Utility.wearable_device_address)
				{
					if (!isConnecting)
					{
						handler(this, result.Device);
						isConnecting = true;
					}
				}
				Log.Debug("LEScanCallBack", result.Device.Address);
			}
			else {
				Log.Debug("LEScanCallBack", "No device detected!");
			}
			base.OnScanResult(callbackType, result);
		}

		public override void OnBatchScanResults(IList<ScanResult> results)
		{
			if (results != null)
			{
				Log.Debug("LEScanCallBack", string.Format("Results #{0}",results.Count));
				foreach (var result in results)
				{
					Log.Debug("LEScanCallBack", string.Format("Address: {0}, Rssi: {1}", result.Device, result.Rssi));
				}
			}
			base.OnBatchScanResults(results);
		}
	}

	public class BluetoothLEGattCallback : BluetoothGattCallback
	{
		public delegate void DeviceReady(object sendFunction, BluetoothGatt gatt);

		public event DeviceReady OnDeviceReady;

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
		{
			if (status == GattStatus.Success)
			{
				gatt.DiscoverServices();
			}
			base.OnConnectionStateChange(gatt, status, newState);
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
		{
			var handler = OnDeviceReady;
			if (status == GattStatus.Success)
			{
				foreach (var service in gatt.Services)
				{
					foreach (var characteristic in service.Characteristics)
					{
						Log.Debug("OnServicesDiscovered", string.Format("Service @{0}\nCharacteristics @{1}\n Permission: {2}", service.Uuid, characteristic.Uuid, characteristic.Properties));
					}
				}
				handler(this, gatt);
			}
		}
	}
}
