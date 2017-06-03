using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Use
{
    public sealed partial class MainPage : Page
    {

        UnityBLE.BLE ble;
        List<UnityBLE.DeviceArgs> devices;

        public MainPage()
        {
            this.InitializeComponent();
            ble = new UnityBLE.BLE();
            devices = new List<UnityBLE.DeviceArgs>();

            ble.DeviceAdded += deviceAddedAsync;
            ble.DeviceRemoved += deviceRemovedAsync;
            ble.CharacteristicReceived += characteristicReceivedAsync;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ble.Start();
        }

        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ble.Stop();
            ble.Listen(devices[deviceList.SelectedIndex].DeviceID, serviceUUID.Text, characteristicUUID.Text);
        }

        private async void deviceAddedAsync(object sender, UnityBLE.DeviceArgs args)
        {
            if (sender == ble)
            {
                devices.Add(args);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => deviceList.Items.Add(args.Name + " : " + args.DeviceID));
            }
        }

        private async void deviceRemovedAsync(object sender, UnityBLE.DeviceArgs args)
        {
            if (sender == ble)
            {
                devices.Remove(args);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => deviceList.Items.Remove(args.Name + " : " + args.DeviceID));
            }
        }

        private async void characteristicReceivedAsync(object sender, UnityBLE.CharacteristicArgs args)
        {
            if (sender == ble)
            {
                if (args.ex == null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => receivedData.Text = System.Text.Encoding.UTF8.GetString(args.Value));
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => receivedData.Text = args.ex.Message);
                }
            }
        }
    }
}
