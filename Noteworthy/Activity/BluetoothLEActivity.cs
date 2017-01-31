
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
		public UUID RX_SERVICE_UUID = UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
    	public UUID RX_CHAR_UUID = UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
    	public UUID TX_CHAR_UUID = UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID CCCD = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		public Button sendButton;

		public BluetoothGatt mBluetoothGatt;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.MainRecording);

			sendButton = FindViewById<Button>(Resource.Id.btnRecord);

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
					BluetoothLEGattCallback mGattCallback = new BluetoothLEGattCallback();
					mGattCallback.OnDeviceReadyWrite += (sendFunction, gatt) =>
					{
						try
						{
							byte[] bufferWrite = ASCIIEncoding.Default.GetBytes("master");
							BluetoothGattService RxService = gatt.GetService(RX_SERVICE_UUID);
							BluetoothGattCharacteristic RxChar = RxService.GetCharacteristic(RX_CHAR_UUID);
							RxChar.SetValue(bufferWrite);
							if (gatt.WriteCharacteristic(RxChar))
							{
								Log.Debug("OnDeviceReadyWrite", "Write Successfull!");
							}
							else {
								Log.Debug("OnDeviceReadyWrite", "Write Unsuccessful... :(");
							}
						}
						catch (Exception ex)
						{
							Utility.ExceptionHandler("BluetoothLEActivity", "OnDeviceReadyWrite", ex);
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
		}
	}

	public class BluetoothLEGattCallback : BluetoothGattCallback
	{
		public UUID RX_SERVICE_UUID = UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID RX_CHAR_UUID = UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID TX_CHAR_UUID = UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID CCCD = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		public delegate void DeviceReadyWrite(object sendFunction, BluetoothGatt gatt);

		public event DeviceReadyWrite OnDeviceReadyWrite;

		public bool isWrite = false;

		public bool isRead = false;

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
		{
			if (status == GattStatus.Success)
			{
				gatt.DiscoverServices();
			}
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
		{
			var handler = OnDeviceReadyWrite;
			if (status == GattStatus.Success)
			{
				BluetoothGattService RxService = gatt.GetService(RX_SERVICE_UUID);
				BluetoothGattCharacteristic TxChar = RxService.GetCharacteristic(TX_CHAR_UUID);
				gatt.SetCharacteristicNotification(TxChar, true);

				BluetoothGattDescriptor descriptor = TxChar.GetDescriptor(CCCD);
				descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
				gatt.WriteDescriptor(descriptor);
			}
		}

		public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
		{
			if (status == GattStatus.Success)
			{
				var handler = OnDeviceReadyWrite;
				if (!isWrite)
				{
					Log.Debug("OnServicesDiscovered", "Is now ready to write!");
					handler(this, gatt);
					isWrite = true;
				}
			}
		}

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
		{
			byte[] value = characteristic.GetValue();
			var result = ASCIIEncoding.Default.GetString(value);
			Log.Debug("BluetoothLEGattCallback", string.Format("Result: {0}", result));
		}

		public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
		{
			try
			{
				if (status == GattStatus.Success)
				{
					Log.Debug("OnCharacteristicRead", "Is read successfully!");
					byte[] value = characteristic.GetValue();
					Log.Debug("BluetoothLEGattCallback", string.Format("String Value: {0}", characteristic.GetStringValue(0)));
					var result = System.Text.Encoding.UTF8.GetString(value);
					Log.Debug("BluetoothLEGattCallback", string.Format("Result: {0}", result));
				}
				base.OnCharacteristicRead(gatt, characteristic, status);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BluetoothLEGattCallback", "OnCharacteristicRead", ex);
			}
		}
	}
}
