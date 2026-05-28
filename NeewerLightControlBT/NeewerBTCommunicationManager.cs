using InTheHand.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    /// <summary>
    /// Discovers and controls Neewer Bluetooth LE lights using a reverse-engineered protocol.
    /// </summary>
    /// <remarks>
    /// This library is not based on official Neewer protocol documentation. Neewer does not appear to
    /// expose feature metadata through the Bluetooth LE endpoints used here, so capabilities such as RGB
    /// mode, CCT tone mode, and scene support are discovered practically by sending commands and observing
    /// whether a light responds. The code intentionally allows NEEWER-branded devices to be tried even
    /// when the exact model is untested.
    /// </remarks>
    public class NeewerBTCommunicationManager
    {
        List<NeewerLight> KnownDevices = new List<NeewerLight>();
        Dictionary<string, NeewerLight> KnownDevicesDictionary = new Dictionary<string, NeewerLight>();
        
        public delegate void LogDelegate(string logtype, string logmessage);
        public LogDelegate Log { get; set; } = null;

        /// <summary>
        /// Searches for nearby Bluetooth LE devices that look like Neewer lights and records them for later control.
        /// </summary>
        /// <returns>Display IDs for discovered Neewer devices known to this manager.</returns>
        /// <remarks>
        /// The search relies on observed Neewer naming and service behavior, not official manufacturer
        /// feature descriptors. Some compatible devices may support only a subset of the commands.
        /// </remarks>
        public async Task<List<string>> DoSearch()
        {
            //var peers = 
            //foreach (var peer in peers)
            //{
            //    System.Diagnostics.Debug.Write($"Peer: {peer.DeviceName}, Addr: {peer.DeviceAddress}, DeviceClass: {peer.ClassOfDevice.ToString()}");
            //}

            var options = new RequestDeviceOptions();
            var ScanOption = new BluetoothLEScanFilter();
            ScanOption.Services.Add(BluetoothUuid.FromGuid(new Guid("69400001-B5A3-F393-E0A9-E50E24DCCA99")));
            options.Filters.Add(ScanOption);
            options.AcceptAllDevices = false;
            options.OptionalServices.Add(new Guid("69400001-B5A3-F393-E0A9-E50E24DCCA99"));

            var actionitem = Bluetooth.ScanForDevicesAsync(options);
            //actionitem.RunSynchronously();
            var res = actionitem.Result;
            var resordered = res.OrderBy(xy => xy.Id).ToList();
            string[] Devices = new string[2];
            Devices[0] = "E37D3846235D";
            Devices[1] = "E2F325DCBC28";
            List<NeewerLight> devices = new List<NeewerLight>();
            foreach (var result in resordered)
            {
                if (result.Name.Contains("NEEWER"))
                {
                    //E2F325DCBC28

                    int stringleng = result.Id.Length;
                    string deviceid = $"{result.Name}-{result.Id.Substring(stringleng - 6, 6)}";
                    var BTdevice = new NeewerLight() { Device = result, ID = deviceid, rawName = result.Name, identifier = result.Id };
                    BTdevice.LookupLightDataByNameID(BTdevice.rawName, BTdevice.identifier);

                    devices.Add(BTdevice);
                    if (!KnownDevicesDictionary.ContainsKey(deviceid))
                    {
                        KnownDevicesDictionary.Add(deviceid, BTdevice);
                        KnownDevices.Add(BTdevice);
                    }
                }
                System.Diagnostics.Debug.WriteLine($"id: {result.Id}, name: {result.Name}, paired: {result.IsPaired}, Gatt:{result.Gatt.Device.Id}");
                if (Log != null)
                    Log("info", $"id: {result.Id}, name: {result.Name}, paired: {result.IsPaired}, Gatt:{result.Gatt.Device.Id}\n");

            }

            return devices.Select(xy => xy.ID).ToList();


        }
        public async Task<bool> ConnectToDeviceAsync(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            return await KnownDevicesDictionary[DeviceID].ConnectAsync();
        }

        public async Task ExecuteLightInstruction(string DeviceID, NeewerLightInstruction instruction)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].ExecuteLightInstruction(instruction);
        }
        public async Task TurnOnAsync(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].TurnOnAsync();
        }

        public async Task TurnOffAsync(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].TurnOffAsync();
        }

        public async Task SendReadRequestAsync(string DeviceID, byte experimentalbyte = 0xFC)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].SendReadRequestAsync(experimentalbyte);
        }
        public async Task SendReadRequestOrigAsync(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].SendReadRequestOrigAsync();
        }
        public (string name, string id) GetBluetoothDeviceInfo(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            var item = KnownDevicesDictionary[DeviceID];
            if (item.IsDisposed)
                return (string.Empty, string.Empty);
            return (item.Device.Name, item.Device.Id);
        }
        public async Task DisconnectFromDeviceAsync(string DeviceID)
        {
            if (!KnownDevicesDictionary.ContainsKey(DeviceID))
            {
                throw new ArgumentOutOfRangeException("DeviceID", "We don't have that device. Try scanning again if you think we should have that device");
            }
            await KnownDevicesDictionary[DeviceID].DisconnectAsync();
            var item = KnownDevicesDictionary[DeviceID];
            KnownDevicesDictionary.Remove(DeviceID);
            KnownDevices.Remove(item);
        }
        public async Task<string> ConnectKnownDeviceId(string bluetoothDeviceID)
        {
            foreach (var existingdevice in KnownDevices)
            {
                if (existingdevice.Device == null)
                    return string.Empty;
                if (existingdevice.Device.Id.ToUpperInvariant() == bluetoothDeviceID.ToUpperInvariant())
                {
                    // We know of this device already
                    return existingdevice.ID;
                }
            }
            // Unknown initial device.
            var device = await BluetoothDevice.FromIdAsync(bluetoothDeviceID);
            if (device != null && device.Name.Contains("NEEWER"))
            {
                //E2F325DCBC28

                int stringleng = device.Id.Length;
                string deviceid = $"{device.Name}-{device.Id.Substring(stringleng - 6, 6)}";
                var BTdevice = new NeewerLight() { Device = device, ID = deviceid, rawName = device.Name, identifier = device.Id };
                BTdevice.LookupLightDataByNameID(device.Name, device.Id);

                //devices.Add(BTdevice);
                if (!KnownDevicesDictionary.ContainsKey(deviceid))
                {
                    KnownDevicesDictionary.Add(deviceid, BTdevice);
                    KnownDevices.Add(BTdevice);
                }
                System.Diagnostics.Debug.WriteLine($"id: {device.Id}, name: {device.Name}, paired: {device.IsPaired}, Gatt:{device.Gatt.Device.Id}");
                if(Log != null)
                    Log("info", $"id: {device.Id}, name: {device.Name}, paired: {device.IsPaired}, Gatt:{device.Gatt.Device.Id}\n");
                return BTdevice.ID;
            }

            return string.Empty;
        }
    }
}
