using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if WINDOWS_UWP
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
#endif


namespace UnityBLE
{

    public sealed class BLE
    {
        public event EventHandler<String> DeviceAdded;
        public event EventHandler<String> DeviceRemoved;
        public event EventHandler<String> CharacteristicReceived;

#if WINDOWS_UWP
        private DeviceWatcher deviceWatcher;
#endif

        public void Start()
        {
#if WINDOWS_UWP
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            deviceWatcher = DeviceInformation.CreateWatcher(
                                    "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                                    requestedProperties,
                                    DeviceInformationKind.AssociationEndpoint);

            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Start();
#endif
        }

        public void Stop()
        {
#if WINDOWS_UWP
            deviceWatcher.Stop();
#endif
        }

        public void Listen(string deviceId, string serviceUUID, string characteristicUUID)
        {
#if WINDOWS_UWP
            listen(deviceId, serviceUUID, characteristicUUID);
#endif
        }

#if WINDOWS_UWP

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if (sender == deviceWatcher)
            {
                if (deviceInfo.Name != string.Empty)
                {
                    DeviceAdded(this, deviceInfo.Id);
                }
            }
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender == deviceWatcher)
            {
                DeviceRemoved(this, deviceInfoUpdate.Id);
            }
        }

        private async void listen(string deviceId, string serviceUUID, string characteristicUUID)
        {
#if WINDOWS_UWP
            var device = await BluetoothLEDevice.FromIdAsync(deviceId);

            var services = await device.GetGattServicesForUuidAsync(new Guid(serviceUUID));

            var characteristics = await services.Services[0].GetCharacteristicsForUuidAsync(new Guid(characteristicUUID));

            characteristics.Characteristics[0].ValueChanged += characteristicChanged;

            await characteristics.Characteristics[0].WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify
            );
#endif
        }


        private void characteristicChanged(
            GattCharacteristic sender,
            GattValueChangedEventArgs eventArgs
        )
        {
            byte[] data = new byte[eventArgs.CharacteristicValue.Length];
            Windows.Storage.Streams.DataReader.FromBuffer(eventArgs.CharacteristicValue).ReadBytes(data);

            // Make data a string for now
            CharacteristicReceived(this, Encoding.ASCII.GetString(data));
        }
#endif
    }
}
