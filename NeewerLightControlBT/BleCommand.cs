using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public struct BleCommand
    {
        public static byte prefixTag = 0x78;
        public static byte setRGBLightTag = 0x86;
        public static byte setCCTLightTag = 0x87;
        public static byte setLongCCTLightBrightnessTag = 0x82;
        public static byte setLongCCTLightCCTTag = 0x83;
        public static byte setSceneTag = 0x88;

        public static byte[] powerOn = new byte[] { 0x78, 0x81, 0x01, 0x01, 0xFB };
        public static byte[] powerOff = new byte[] { 0x78, 0x81, 0x01, 0x02, 0xFC };
        public static byte[] readRequest = new byte[] { 0x78, 0x84, 0x00, 0xFC };
        public static byte[] readPower = new byte[] { 0x78, 120, 133, 0, 253 };
        public static byte[] readChan = new byte[] { 0x78, 120, 132, 0, 252 };

    }
}
