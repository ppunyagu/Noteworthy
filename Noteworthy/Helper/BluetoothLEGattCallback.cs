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
	public class BluetoothLEGattCallback : BluetoothGattCallback
	{
		public UUID RX_SERVICE_UUID = UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID RX_CHAR_UUID = UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID TX_CHAR_UUID = UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID CCCD = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		public delegate void DeviceReadyWrite(BluetoothGatt gatt);

		public delegate void DataReceivedFromDevice(string valFromDevice);

		public event DeviceReadyWrite OnDeviceReadyWrite;

		public event DataReceivedFromDevice dataReceivedFromDevice;

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
					if (handler != null)
					{
						handler(gatt);
						isWrite = true;
					}
				}
			}
		}

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
		{
			var handler = dataReceivedFromDevice;
			byte[] value = characteristic.GetValue();
			var result = ASCIIEncoding.Default.GetString(value);
			handler(result);
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
				Log.Debug("LEScanCallBack", string.Format("Results #{0}", results.Count));
				foreach (var result in results)
				{
					Log.Debug("LEScanCallBack", string.Format("Address: {0}, Rssi: {1}", result.Device, result.Rssi));
				}
			}
		}
	}
}
