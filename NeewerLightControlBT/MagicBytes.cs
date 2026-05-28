namespace NeewerLightControlBT
{
    /// <summary>
    /// Known command bytes used by the reverse-engineered Neewer Bluetooth LE light protocol.
    /// </summary>
    /// <remarks>
    /// These values are not official Neewer documentation. They are protocol constants inferred from
    /// observed device behavior and community reverse-engineering work. Many values are therefore
    /// magic bytes or magic numbers: their meaning is known only by how compatible lights respond
    /// to them. Unsupported light models may ignore some commands, especially RGB or scene commands.
    /// </remarks>
    internal static class MagicBytes
    {

        /// <summary>
        /// First byte observed at the start of Neewer BLE command packets.
        /// </summary>
        public const byte FIRST_BYTE = 0x78;
        public const byte DEVICE_ID = 0x01;
        public const byte SET_POWER = 0x81;
        public const byte SET_MODE_CCT_BRIGHTNESS = 0x82;
        public const byte SET_MODE_CCT_TONE = 0x83;
        public const byte REQUEST_DATA = 0x84;
        public const byte SET_MODE_RGB = 0x86;
        public const byte SET_MODE_CCT = 0x87;
        public const byte SET_MODE_SCENE = 0x88;
        public const byte SET_MODE_SCENE_SUB = 0x8B;

        public const byte SET_MODE_RGB_HSV = 0x89;
        public const byte SET_MODE_RGB_CONTINUITY = 0x90;
        public const byte SET_MODE_SCENE_DATA = 0x91;
        public static readonly byte[] SET_MODE_SCENE_UPDATE_PREFIX = new byte[] { FIRST_BYTE, 0x01, 0x01 };

        public const byte prefixTag = 0x78;         // 120 Every bluettooth cmd start with 120
        public const byte setLongCCTLightBrightnessTag = 0x82;   // 130 Set long CCT Light brightness.
        public const byte setLongCCTLightCCTTag = 0x83;         // 131 Set long CCT Light CCT.

        public const byte setRGBLightTag = 0x86;  // 134 Set RGB Light Mode.
        public const byte setCCTLightTag = 0x87;  // 135 Set CCT Light Mode.

        public const byte setSceneTag = 0x88;      // 136 Set Scene Light Mode.
        public const byte setSCESubTag = 0x8B;  //

        public const byte setHSVDataTag = 0x89;  // 143 Set Continuity RGB Light HSV data.
        public const byte setCCTDataTag = 0x90;  // 144 Set Continuity RGB Light Mode.
        public const byte setSCEDataTag = 0x91;  //


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
