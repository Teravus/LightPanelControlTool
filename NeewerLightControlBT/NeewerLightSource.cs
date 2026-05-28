using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public class NeewerLightSource
    {
        ushort id;
        string name;

        bool needBRR = false;
        bool needCCT = false;
        bool needGM = false;

        Dictionary<string, float> featureValues = new Dictionary<string, float>();
        public NeewerLightSource(ushort pid, string pname)
        {
            id = pid;
            name = pname;
        }
        public NeewerLightSource(ushort pid, string pname, bool pbrr)
        {
            id = pid;
            name = pname;
            needBRR = pbrr;
        }
        public float brrValue
        {
            get { return featureValues.ContainsKey("brrValue") ? featureValues["brrValue"] : 50.0f; }
            set { if (featureValues.ContainsKey("brrValue")) { featureValues["brrValue"] = value; } else { featureValues.Add("brrValue", value); } }
        }
        public float cctValue
        {
            get { return featureValues.ContainsKey("cctValue") ? featureValues["cctValue"] : 10.0f; }
            set { if (featureValues.ContainsKey("cctValue")) { featureValues["cctValue"] = value; } else { featureValues.Add("cctValue", value); } }
        }
        public float gmValue
        {
            get { return featureValues.ContainsKey("gmValue") ? featureValues["gmValue"] : -50.0f; }
            set { if (featureValues.ContainsKey("gmValue")) { featureValues["gmValue"] = value; } else { featureValues.Add("gmValue", value); } }
        }
        public static NeewerLightSource sunlightSource()
        {
            var scene = new NeewerLightSource(pid: 0x01, pname: "Sunlight");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource whiteHalogenSource()
        {
            var scene = new NeewerLightSource(pid: 0x02, pname: "White Halogen light");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource xenonShortarcLampSource()
        {
            var scene = new NeewerLightSource(pid: 0x03, pname: "Xenon short-arc lamp");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource horizonDaylightSource()
        {
            var scene = new NeewerLightSource(pid: 0x04, pname: "Horizon daylight");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource daylightSource()
        {
            var scene = new NeewerLightSource(pid: 0x05, pname: "Daylight");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource tungstenSource()
        {
            var scene = new NeewerLightSource(pid: 0x06, pname: "Tungsten");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource studioBulbSource()
        {
            var scene = new NeewerLightSource(pid: 0x07, pname: "Studio Bulb");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource modelingLightsSource()
        {
            var scene = new NeewerLightSource(pid: 0x08, pname: "Modeling Lights");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource dysprosicLampSource()
        {
            var scene = new NeewerLightSource(pid: 0x09, pname: "Dysprosic lamp");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }
        public static NeewerLightSource hmi6000Source()
        {
            var scene = new NeewerLightSource(pid: 0x0A, pname: "HMI6000");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            return scene;
        }

    }
}
