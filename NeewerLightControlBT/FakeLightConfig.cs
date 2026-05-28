using System.Collections.Generic;

namespace NeewerLightControlBT
{
    public class FakeLightConfig
    {
        public Dictionary<string, string> cfg = new Dictionary<string, string>();
    }


    //var adapter = await BluetoothAdapter.GetDefaultAsync();
    //var device = await adapter.StartBleDeviceDiscoveryAsync();

    //// Once device is discovered
    //await device.ConnectAsync();

    //var services = await device.GetGattServicesAsync();
    //foreach (var service in services)
    //{
    //    // Discover the services and characteristics
    //    var characteristics = await service.GetCharacteristicsAsync();
    //    foreach (var characteristic in characteristics)
    //    {
    //        // Do something with the characteristics, e.g., write a value
    //        await characteristic.WriteValueAsync(data);
    //    // Or set up a notification
    //    characteristic.ValueChanged += Characteristic_ValueChanged;
    //    }
    //}

}
