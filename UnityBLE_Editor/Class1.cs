using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
#endif

namespace UnityBLE
{

    public class DeviceArgs : EventArgs
    {
        public string Name { get; set; }
        public string DeviceID { get; set; }

        public DeviceArgs(string deviceID)
        {
            this.DeviceID = deviceID;
        }

        public DeviceArgs(string deviceID, string name)
        {
            this.Name = name;
            this.DeviceID = deviceID;
        }
    }

    public class CharacteristicArgs : EventArgs
    {
        public byte[] Value { get; set; }
        public Exception ex { get; set; }

        public CharacteristicArgs(byte[] value)
        {
            this.Value = value;
        }

        public CharacteristicArgs(Exception e)
        {
            this.ex = e;
        }
    }

    public sealed class BLE
    {
        public event EventHandler<DeviceArgs> DeviceAdded;
        public event EventHandler<DeviceArgs> DeviceRemoved;
        public event EventHandler<CharacteristicArgs> CharacteristicReceived;

#if WINDOWS_UWP
        private DeviceWatcher deviceWatcher;
#endif

        public void Start()
        {
#if WINDOWS_UWP
            if (deviceWatcher == null) { 

                string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

                deviceWatcher = DeviceInformation.CreateWatcher(
                                        "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                                        requestedProperties,
                                        DeviceInformationKind.AssociationEndpoint);

                deviceWatcher.Added += DeviceWatcher_Added;
                deviceWatcher.Removed += DeviceWatcher_Removed;
            }
            if(deviceWatcher.Status != DeviceWatcherStatus.Started) deviceWatcher.Start();

#endif
        }

        public void Stop()
        {
#if WINDOWS_UWP
            if (deviceWatcher != null && deviceWatcher.Status == DeviceWatcherStatus.Started)
            {
                deviceWatcher.Stop();
            }
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
                if (deviceInfo.Name != string.Empty && deviceInfo.Pairing.IsPaired)
                {
                    DeviceAdded(this, new DeviceArgs(deviceInfo.Id, deviceInfo.Name));
                }
            }
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender == deviceWatcher)
            {
                DeviceRemoved(this, new DeviceArgs(deviceInfoUpdate.Id));
            }
        }

        private async void listen(string deviceId, string serviceUUID, string characteristicUUID)
        {
#if WINDOWS_UWP
            try{
                
                var device = await BluetoothLEDevice.FromIdAsync(deviceId);

                var services = device.GetGattService(new Guid(serviceUUID));


                if (services.GetAllIncludedServices().Count == 0)
                {
                    throw new Exception("no service");
                }


                var characteristics = services.GetAllIncludedServices()[0].GetCharacteristics(new Guid(characteristicUUID));

                if(characteristics.Count == 0)
                {
                    throw new Exception("no characteristic");
                }

                
                characteristics[0].ValueChanged += characteristicChanged;

                await characteristics[0].WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify
                );        
            }
            catch(Exception e)
            {
                CharacteristicReceived(this, new CharacteristicArgs(e));
            }
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
            CharacteristicReceived(this, new CharacteristicArgs(data));
        }
#endif
    }
}