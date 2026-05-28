using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public struct BleUpdate
    {
        public static byte[] channelUpdatePrefix = new byte[] { 0x78, 0x01, 0x01 };
    }
}
