using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public enum nLightMode : byte
    {
        OFF = 0
        ,CCTMode = 1    // Bi-color mode
        ,HSIMode        // Color mode
        ,SCEMode        // Scene mode, or animation mode or channel mode
    }
}
