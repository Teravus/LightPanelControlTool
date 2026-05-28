//using InTheHand.Net.Sockets;

namespace NeewerLightControlBT
{
    /// <summary>
    /// Describes one desired Neewer light operation for the reverse-engineered Bluetooth protocol.
    /// </summary>
    /// <remarks>
    /// Not every Neewer model supports every mode. Some lights may only respond to CCT commands,
    /// some support RGB, and scene availability varies by model. Unsupported commands are commonly
    /// ignored by the device.
    /// </remarks>
    public class NeewerLightInstruction
    {
        public nLightMode LightMode { get; set; } = nLightMode.OFF;
        public float brightness { get; set; } = 100;
        public int CCT { get; set; } = 6500;
        public float gmm { get; set; } = 50;
        public NeewerColor RGB { get; set; } = NeewerColor.Black;
        public float Saturation { get; set; } = 100;


        public string SceneName { get; set; } = "";

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


