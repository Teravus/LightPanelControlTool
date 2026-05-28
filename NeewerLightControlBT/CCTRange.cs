namespace NeewerLightControlBT
{

    public class CCTRange
    {
        public int MinCCT { get; set; }
        public int MaxCCT { get; set; }

        public CCTRange()
        {
            // Initialize with actual min and max CCT values
            MinCCT = 32; // For 3200K
            MaxCCT = 56; // For 5600K
        }
        public CCTRange(int mincct, int maxcct)
        {
            MinCCT = mincct;
            MaxCCT = maxcct;
        }
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
