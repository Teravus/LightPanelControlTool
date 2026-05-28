using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public class NeewerLightFX
    {
        public ushort id;
        public string name;

        public bool needBRR = false;
        public bool needBRRUpperBound = false;
        public bool needCCT = false;
        public bool needCCTUpperBound = false;
        public bool needGM = false;
        public bool needSAT = false;
        public bool needHUE = false;
        public bool needHUEUpperBound = false;
        public bool needSpeed = false;
        public byte speedLevel = 0;
        public bool needSparks = false;
        public byte[] sparkLevel = new byte[0];
        public bool needColor = false;
        public ColorItem[] colors = new ColorItem[0];
        Dictionary<string, float> featureValues = new Dictionary<string, float>();

        public NeewerLightFX(ushort pid, string pname)
        {
            id = pid;
            name = pname;
        }
        public NeewerLightFX(ushort pid, string pname, bool pbrr)
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
        public float brrUpperValue
        {
            get { return featureValues.ContainsKey("brrUpperValue") ? featureValues["brrUpperValue"] : 80.0f; }
            set { if (featureValues.ContainsKey("brrUpperValue")) { featureValues["brrUpperValue"] = value; } else { featureValues.Add("brrUpperValue", value); } }
        }
        public float cctValue
        {
            get { return featureValues.ContainsKey("cctValue") ? featureValues["cctValue"] : 10.0f; }
            set { if (featureValues.ContainsKey("cctValue")) { featureValues["cctValue"] = value; } else { featureValues.Add("cctValue", value); } }
        }
        public float cctUpperValue
        {
            get { return featureValues.ContainsKey("cctUpperValue") ? featureValues["cctUpperValue"] : 20.0f; }
            set { if (featureValues.ContainsKey("cctUpperValue")) { featureValues["cctUpperValue"] = value; } else { featureValues.Add("cctUpperValue", value); } }
        }
        public float gmValue
        {
            get { return featureValues.ContainsKey("gmValue") ? featureValues["gmValue"] : -50.0f; }
            set { if (featureValues.ContainsKey("gmValue")) { featureValues["gmValue"] = value; } else { featureValues.Add("gmValue", value); } }
        }

        public float satValue
        {
            get { return featureValues.ContainsKey("satValue") ? featureValues["satValue"] : 10.0f; }
            set { if (featureValues.ContainsKey("satValue")) { featureValues["satValue"] = value; } else { featureValues.Add("satValue", value); } }
        }

        public float hueValue
        {
            get { return featureValues.ContainsKey("hueValue") ? featureValues["hueValue"] : 10.0f; }
            set { if (featureValues.ContainsKey("hueValue")) { featureValues["hueValue"] = value; } else { featureValues.Add("hueValue", value); } }
        }

        public float hueUpperValue
        {
            get { return featureValues.ContainsKey("hueUpperValue") ? featureValues["hueUpperValue"] : 180.0f; }
            set { if (featureValues.ContainsKey("hueUpperValue")) { featureValues["hueUpperValue"] = value; } else { featureValues.Add("hueUpperValue", value); } }
        }

        public float speedValue
        {
            get { return featureValues.ContainsKey("speedValue") ? featureValues["speedValue"] : 1.0f; }
            set { if (featureValues.ContainsKey("speedValue")) { featureValues["speedValue"] = value; } else { featureValues.Add("speedValue", value); } }
        }

        public float sparksValue
        {
            get { return featureValues.ContainsKey("sparksValue") ? featureValues["sparksValue"] : 1.0f; }
            set { if (featureValues.ContainsKey("sparksValue")) { featureValues["sparksValue"] = value; } else { featureValues.Add("sparksValue", value); } }
        }

        public float colorValue
        {
            get { return featureValues.ContainsKey("colorValue") ? featureValues["colorValue"] : 1.0f; }
            set { if (featureValues.ContainsKey("colorValue")) { featureValues["colorValue"] = value; } else { featureValues.Add("colorValue", value); } } }
        public struct ColorItem
        {
            public string key;
            public int value;

        }
        enum CodingKey
        {
            key,
            value
        }

        public static NeewerLightFX lightingScene()
        {
            var scene = new NeewerLightFX(pid: 0x01, pname: "Lighting");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX paparazziScene()
        {
            var scene = new NeewerLightFX(pid: 0x02, pname: "Paparazzi");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX defectiveBulbScene()
        {
            var scene = new NeewerLightFX(pid: 0x03, pname: "Defective bulb");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX explosionScene()
        {
            var scene = new NeewerLightFX(pid: 0x04, pname: "Explosion");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            scene.needSparks = true;
            scene.sparkLevel = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A };
            return scene;
        }

        public static NeewerLightFX weldingScene()
        {
            var scene = new NeewerLightFX(pid: 0x05, pname: "Welding");
            scene.needBRR = true;
            scene.needBRRUpperBound = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX cctFlashScene()
        {
            var scene = new NeewerLightFX(pid: 0x06, pname: "CCT flash");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX hueFlashScene()
        {
            var scene = new NeewerLightFX(pid: 0x07, pname: "HUE flash");
            scene.needBRR = true;
            scene.needHUE = true;
            scene.needSAT = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX cctPulseScene()
        {
            var scene = new NeewerLightFX(pid: 0x08, pname: "CCT pulse");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX huePulseScene()
        {
            var scene = new NeewerLightFX(pid: 0x09, pname: "HUE pulse");
            scene.needBRR = true;
            scene.needHUE = true;
            scene.needSAT = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX copCarScene()
        {
            var scene = new NeewerLightFX(pid: 0x0A, pname: "Cop Car");
            scene.needBRR = true;
            scene.needColor = true;
            scene.colors = new ColorItem[] { new ColorItem() { key= "Red", value= 0x00 },
                            new ColorItem() { key= "Blue", value= 0x01},
                            new ColorItem() { key= "Red and Blue", value= 0x2},
                            new ColorItem() { key= "White and Blue", value= 0x3},
                            new ColorItem() { key= "Red blue white", value= 0x4} };
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX candlelightScene()
        {
            var scene = new NeewerLightFX(pid: 0x0B, pname: "Candlelight");
            scene.needBRR = true;
            scene.needBRRUpperBound = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            scene.needSparks = true;
            scene.sparkLevel = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A };
            return scene;
        }
        public static NeewerLightFX hueLoopScene()
        {
            var scene = new NeewerLightFX(pid: 0x0C, pname: "HUE Loop");
            scene.needBRR = true;
            scene.needHUE = true;
            scene.needHUEUpperBound = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }

        public static NeewerLightFX cctLoopScene()
        {
            var scene = new NeewerLightFX(pid: 0x0D, pname: "CCT Loop");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needCCTUpperBound = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX intLoopScene()
        {
            var scene = new NeewerLightFX(pid: 0x0E, pname: "INT Loop");
            scene.needBRR = true;
            scene.needBRRUpperBound = true;
            scene.needHUE = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX tvScreenScene()
        {
            var scene = new NeewerLightFX(pid: 0x0F, pname: "TV Screen");
            scene.needBRR = true;
            scene.needCCT = true;
            scene.needGM = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            return scene;
        }
        public static NeewerLightFX fireworkScene()
        {
            var scene = new NeewerLightFX(pid: 0x10, pname: "Firework");
            scene.needBRR = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            scene.needColor = true;
            scene.colors = new ColorItem[] { 
                            new ColorItem() { key= "Single color", value= 0x00 },
                            new ColorItem() { key= "Color", value= 0x01},
                            new ColorItem() { key= "Combined", value= 0x2}
            };
            scene.needSparks = true;
            scene.sparkLevel = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A };
            return scene;
        }
        public static NeewerLightFX partyScene()
        {
            var scene = new NeewerLightFX(pid: 0x11, pname: "Party");
            scene.needBRR = true;
            scene.needSpeed = true;
            scene.speedLevel = 10;
            scene.needColor = true;
            scene.colors = new ColorItem[] {
                            new ColorItem() { key= "Single color", value= 0x00 },
                            new ColorItem() { key= "Color", value= 0x01},
                            new ColorItem() { key= "Combined", value= 0x2}
            };
            
            return scene;
        }
    }
    
}
