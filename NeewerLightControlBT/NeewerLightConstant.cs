using System;
using System.Collections.Generic;
using System.Linq;

namespace NeewerLightControlBT
{
    public static class NeewerLightConstant
    {
        public static byte[] getRGBLightTypes()
        {
            return new byte[] { 3, 5, 8, 9, 11, 12, 15, 16, 18, 19, 20, 21, 22, 26, 29, 32, 34, 39, 40, 42, 43, 56, 57, 59 };
        }
        public static byte[] getMusicSupportLightTypes()
        {
            return new byte[] { 8, 18, 43, 20, 21, 40, 14, 34, 25, 30, 38, 28, 19, 26, 42, 16, 27, 32, 37, 31, 22, 44, 46, 45, 47, 49, 50, 51, 52, 53, 54, 55, 39, 58, 56, 57, 59, 60, 61, 62, 63 };
        }
        public static byte[] getRGBLightTypesThatSupport17FX()
        {
            return new byte[] { 8, 16, 20, 22, 34, 40 };
        }
        public static byte[] getRGBLightTypesThatSupport9FX()
        {
            return new byte[] { 3, 5 };
        }
        public static string getProjectName(int idx)
        {
            switch (idx)
            {
                case 8:
                    return "RGB1";
                case 14:
                    return "SL90";
                case 18:
                    return "RGB1200";
                case 21:
                    return "RGB C80";
                case 22:
                    return "CB60 RGB";
                case 24:
                    return "Apollo 150D";
                case 25:
                    return "MS60C";
                case 26:
                    return "BH-30S RGB";
                case 28:
                    return "CB200B";
                case 30:
                    return "MS60B";
                case 31:
                    return "CB60B";
                case 32:
                    return "TL60 RGB";
                case 34:
                    return "SL90 Pro";
                case 40:
                    return "RGB62";
                case 42:
                    return "BH-30S RGB";
                case 43:
                    return "RGB1200";
                case 47:
                    return "CB300B";
                case 49:
                    return "CB100C";
                case 50:
                    return "TL120C";
                case 53:
                    return "FS230 5600K";
                case 54:
                    return "FS150 5600K";
                case 55:
                    return "FS230B";
                case 58:
                    return "AS600B";
                case 59:
                    return "TL60 RGB";
                case 60:
                    return "PL60C";
                case 63:
                    return "RP19C";
                default:
                    return "";
            }
        }
        public static string getProjectName(string str)
        {
            switch (str)
            {
                case "20200015":
                    return "RGB1";
                case "20200037":
                    return "SL90";
                case "20200049":
                    return "RGB1200";
                case "20210006":
                    return "Apollo 150D";
                case "20210007":
                    return "RGB C80";
                case "20210012":
                    return "CB60 RGB";
                case "20210018":
                    return "BH-30S RGB";
                case "20210034":
                    return "MS60B";
                case "20210035":
                    return "MS60C";
                case "20210036":
                    return "TL60 RGB";
                case "20210037":
                    return "CB200B";
                case "20220014":
                    return "CB60B";
                case "20220016":
                    return "PL60C";
                case "20220035":
                    return "MS150B";
                case "20220041":
                    return "AS600B";
                case "20220043":
                    return "FS150B";
                case "20220046":
                    return "RP19C";
                case "20220051":
                    return "CB100C";
                case "20220055":
                    return "CB300B";
                case "20220057":
                    return "SL90 Pro";
                case "20230021":
                    return "BH-30S RGB";
                case "20230025":
                    return "RGB1200";
                case "20230031":
                    return "TL120C";
                case "20230050":
                    return "FS230 5600K";
                case "20230051":
                    return "FS230B";
                case "20230052":
                    return "FS150 5600K";
                case "20230064":
                    return "TL60 RGB";
                default:
                    return "";
            }
        }
        public static bool isRGBOther(string str)
        {
            string STR = str.ToUpperInvariant();
            return "RGB480" == STR
            || "RGB530" == STR
            || "RGB660" == STR
            || "RGB530 PRO" == STR
            || "RGB660 PRO" == STR
            || "RGB-P200" == STR
            || "RGB450" == STR
            || "RGB650" == STR
            ;
        }
        public static Tuple<string, string> getLightNames(string rawname, string identifier)
        {
            string nickName = "";
            string projectName = "";
            string name = rawname;
            string suffix = (string.IsNullOrEmpty(identifier) || identifier.Length < 6) ? identifier : "-" + identifier.Substring(identifier.Length - 6, 6);
            //string suffix = identifier == "" ? "" : "-\(identifier.suffix(6))";


            if (name.StartsWith("NWR")) {
                projectName = name.Substring(4, name.Length - 4);
            }
            else if (name.StartsWith("NEEWER")) {
                projectName = name.Substring(7, name.Length - 7);
            }
            else if (!name.StartsWith("NW") || !name.Contains("&")) {
                projectName = name.StartsWith("NW") ? name.Substring(3, name.Length - 3) : name;
            }
            else
            {
                uint garbo;
                string substring = name.Substring(3, name.Length - 3).Substring(name.LastIndexOf('&'));
                if (uint.TryParse(substring, out garbo)) {
                    string result = NeewerLightConstant.getProjectName(substring);
                    projectName = result;
                }
                else
                {
                    projectName = substring;
                }
            }

            nickName = $"{projectName}{suffix}";
            return new Tuple<string, string>(nickName, projectName);
        }
        public static List<NeewerLightFX> getLightFX(byte lightType)
        {
            List<NeewerLightFX> fxs = new List<NeewerLightFX>();
            if (getRGBLightTypesThatSupport17FX().Contains(lightType))
            {
                fxs.Add(NeewerLightFX.lightingScene());
                fxs.Add(NeewerLightFX.paparazziScene());
                fxs.Add(NeewerLightFX.defectiveBulbScene());
                fxs.Add(NeewerLightFX.explosionScene());
                fxs.Add(NeewerLightFX.weldingScene());
                fxs.Add(NeewerLightFX.cctFlashScene());
                fxs.Add(NeewerLightFX.hueFlashScene());
                fxs.Add(NeewerLightFX.cctPulseScene());
                fxs.Add(NeewerLightFX.huePulseScene());
                fxs.Add(NeewerLightFX.copCarScene());
                fxs.Add(NeewerLightFX.candlelightScene());
                fxs.Add(NeewerLightFX.hueLoopScene());
                fxs.Add(NeewerLightFX.cctLoopScene());
                fxs.Add(NeewerLightFX.intLoopScene());
                fxs.Add(NeewerLightFX.tvScreenScene());
                fxs.Add(NeewerLightFX.fireworkScene());
                fxs.Add(NeewerLightFX.partyScene());
            }
            else if (getRGBLightTypesThatSupport9FX().Contains(lightType))
            {
                fxs.Add(new NeewerLightFX(pid: 0x01, pname: "Squard Car") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x02, pname: "Ambulance") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x03, pname: "Fire Engine") { brrValue = 1f });

                fxs.Add(new NeewerLightFX(pid: 0x04, pname: "Fireworks") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x05, pname: "Party") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x06, pname: "Candlelight") { brrValue = 1f });

                fxs.Add(new NeewerLightFX(pid: 0x07, pname: "Lightning") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x08, pname: "Paparazzi") { brrValue = 1f });
                fxs.Add(new NeewerLightFX(pid: 0x09, pname: "Screen") { brrValue = 1f });
             

            }
            return fxs;
        }
        public static List<NeewerLightSource> getLightSources(byte lightType)
        {
            List<NeewerLightSource> fxs = new List<NeewerLightSource>();
            fxs.Add(NeewerLightSource.sunlightSource());
            fxs.Add(NeewerLightSource.whiteHalogenSource());
            fxs.Add(NeewerLightSource.xenonShortarcLampSource());
            fxs.Add(NeewerLightSource.horizonDaylightSource());
            fxs.Add(NeewerLightSource.daylightSource());
            fxs.Add(NeewerLightSource.tungstenSource()); 
            fxs.Add(NeewerLightSource.studioBulbSource());
            fxs.Add(NeewerLightSource.modelingLightsSource());
            fxs.Add(NeewerLightSource.dysprosicLampSource());
            fxs.Add(NeewerLightSource.hmi6000Source());
            return fxs;
        }
        public static byte getLightType(string nickName, string str, string projectName)
        {
            byte lightType = 0;
            if (nickName.Contains("SRP") || nickName.Contains("RP18-P"))
            {
                lightType = 1;
                return lightType;
            }
            if (nickName.Contains("RP18B PRO"))
            {
                lightType = 51;
                return lightType;
            }

            if (nickName.Contains("SNL") || nickName.Contains("NL"))
            {
                if (nickName.Contains("SNL"))
                {
                    if (nickName.Contains("SNL960") || nickName.Contains("SNL1320") || nickName.Contains("SNL1920"))
                    {
                        lightType = 13;
                        return lightType;
                    }
                    lightType = 7;
                    return lightType;
                }
                lightType = 2;
                return lightType;
            }

            if (nickName.Contains("GL1"))
            {
                if (nickName.Contains("GL1 PRO"))
                {
                    lightType = 33;
                }
                else if (nickName.Contains("GL1C"))
                {
                    lightType = 39;
                }
                else
                {
                    lightType = 4;
                }
                return lightType;
            }

            if (nickName.Contains("ZK-RY"))
            {
                lightType = 17;
                return lightType;
            }

            if (!nickName.Contains("RGB") && !nickName.Contains("SL"))
            {
                if (nickName.Contains("ZY") || nickName.Contains("ER1"))
                {
                    lightType = 23;
                    return lightType;
                }
                if (nickName.Contains("DL200"))
                {
                    lightType = 35;
                    return lightType;
                }
                if (nickName.Contains("X2"))
                {
                    lightType = 27;
                    return lightType;
                }
                if (nickName.Contains("CB200B"))
                {
                    lightType = 28;
                    return lightType;
                }
                if (nickName.Contains("Apollo 150D"))
                {
                    lightType = 24;
                    return lightType;
                }
                if (nickName.Contains("MS60C"))
                {
                    lightType = 25;
                    return lightType;
                }
                if (nickName.Contains("MS60B"))
                {
                    lightType = 30;
                    return lightType;
                }
                if (nickName.Contains("CB60B"))
                {
                    lightType = 31;
                    return lightType;
                }
                if (nickName.Contains("RGB62"))
                {
                    lightType = 40;
                    return lightType;
                }
                if (nickName.Contains("GM16"))
                {
                    lightType = 36;
                    return lightType;
                }
                if (nickName.Contains("FS150B"))
                {
                    lightType = 37;
                    return lightType;
                }
                if (nickName.Contains("MS150B"))
                {
                    lightType = 38;
                    return lightType;
                }
                if (nickName.Contains("DL300"))
                {
                    lightType = 41;
                    return lightType;
                }
                if (nickName.Contains("T100C"))
                {
                    lightType = 44;
                    return lightType;
                }
                if (nickName.Contains("A19C 220V"))
                {
                    lightType = 45;
                    return lightType;
                }
                if (nickName.Contains("A19C(E26)"))
                {
                    lightType = 46;
                    return lightType;
                }
                if (nickName.Contains("CB300B"))
                {
                    lightType = 47;
                    return lightType;
                }
                if (nickName.Contains("R360"))
                {
                    lightType = 48;
                    return lightType;
                }
                if (nickName.Contains("CB100C"))
                {
                    lightType = 49;
                    return lightType;
                }
                if (nickName.Contains("TL120C"))
                {
                    lightType = 50;
                    return lightType;
                }
                if (nickName.Contains("RL45B"))
                {
                    lightType = 52;
                    return lightType;
                }
                if (nickName.Contains("FS230 5600K"))
                {
                    lightType = 53;
                    return lightType;
                }
                if (nickName.Contains("FS150 5600K"))
                {
                    lightType = 54;
                    return lightType;
                }
                if (nickName.Contains("FS230B"))
                {
                    lightType = 55;
                    return lightType;
                }
                if (nickName.Contains("20220041"))
                {
                    lightType = 58;
                    return lightType;
                }
                if (nickName.Contains("PL60C"))
                {
                    lightType = 60;
                    return lightType;
                }
                if (nickName.Contains("BH40C"))
                {
                    lightType = 61;
                    return lightType;
                }
                if (nickName.Contains("GR18C"))
                {
                    lightType = 62;
                    return lightType;
                }
                if (nickName.Contains("RP19C"))
                {
                    lightType = 63;
                    return lightType;
                }
                lightType = 0;
                return lightType;
            }

            if (nickName.Contains("RGB"))
            {
                if (projectName == "RGB1" || nickName.Contains("RGB1-A"))
                {
                    lightType = 8;
                }
                else if (nickName.Contains("RGB176"))
                {
                    lightType = nickName.Contains("RGB176 A1") ? (byte)20 : (byte)5;
                }
                else if (nickName.Contains("RGB18(II)"))
                {
                    lightType = 57;
                }
                else
                {
                    if (nickName.Contains("RGB18"))
                    {
                        lightType = 9;
                    }
                    else if (nickName.Contains("RGB190"))
                    {
                        lightType = 11;
                    }
                    else if (nickName.Contains("RGB960") || nickName.Contains("RGB1320") || nickName.Contains("RGB1920"))
                    {
                        lightType = 12;
                    }
                    else if (nickName.Contains("RGB140"))
                    {
                        lightType = 15;
                    }
                    else if (nickName.Contains("RGB168"))
                    {
                        lightType = 16;
                    }
                    if (nickName.Contains("RGB1200"))
                    {
                        lightType = nickName.Contains("20230025") ? (byte)43 : (byte)18;
                    }
                    else if (nickName.Contains("CL124 RGB(II)"))
                    {
                        lightType = 56;
                    }
                    else
                    {
                        if (nickName.Contains("CL124-RGB"))
                        {
                            lightType = 19;
                        }
                        else if (nickName.Contains("RGB C80") || nickName.Contains("RGBC80"))
                        {
                            lightType = 21;
                        }
                        else if (nickName.Contains("CB60 RGB"))
                        {
                            lightType = 22;
                        }
                        else if (nickName.Contains("RGB-P280"))
                        {
                            lightType = 29;
                        }
                        if (nickName.Contains("BH-30S RGB"))
                        {
                            lightType = str.Contains("20230021") ? (byte)42 : (byte)26;
                        }
                        else if (nickName.Contains("TL60 RGB"))
                        {
                            lightType = str.Contains("20230064") ? (byte)59 : (byte)32;
                        }
                        else if (nickName.Contains("RGB62"))
                        {
                            lightType = 40;
                        }
                        else
                        {
                            if (isRGBOther(projectName))
                            {
                                lightType = 3;
                            }
                        }
                    }
                }
            }
            else if (nickName.Contains("SL90 Pro"))
            {
                lightType = 34;
            }
            else if (nickName.Contains("SL90"))
            {
                lightType = 14;
            }
            else
            {
                lightType = 6;
            }
            return lightType;
        }
        public static List<FakeLightConfig> getFakeLightConfigs()
        {
            List<FakeLightConfig> lights = new List<FakeLightConfig>();
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","DF:24:3A:B4:46:5D");
                cfg.cfg.Add("rawname","NW-20210012&FFFFFFFF");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-2531C7E4B053");
                lights.Add(cfg);


            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","ED:86:66:4A:18:74");
                cfg.cfg.Add("rawname","NEEWER-RGB660 PRO");
                cfg.cfg.Add("identifier","EC2907F4-B7DC-ED69-6385-19682E5FE87F");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","F3:74:C6:C5:7C:EF");
                cfg.cfg.Add("rawname","NW-20200015&00000000");
                cfg.cfg.Add("identifier","85D152B3-AC94-3CBB-A475-9A3D2224E88F");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","F3:74:C6:C5:7E:CF");
                cfg.cfg.Add("rawname","NW-RGB176 A1");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-2531C7E4B053");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:74:C6:C5:7E:AB"); 
                cfg.cfg.Add("rawname","NEEWER-SNL530"); 
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-2531DEE0BA8C");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:74:C6:C5:CC:AB");
                cfg.cfg.Add("rawname","NEEWER-RGB168");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-2531DEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:74:C6:C5:AA:AB");
                cfg.cfg.Add("rawname","NEEWER-RGB530 Pro");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-1A3BDEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:74:C6:C5:AA:CC");
                cfg.cfg.Add("rawname","NEEWER-GL1");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-1A3DDEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:74:AA:BB:AA:DD");
                cfg.cfg.Add("rawname","NEEWER-GL1C");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-0FD2-7A8DDEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","FA:58:9A:CC:EE:DD");
                cfg.cfg.Add("rawname","NW-20220057&00000000");
                cfg.cfg.Add("identifier","DEE0BA8C-D9B4-B7DB-012C-7A8DDEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","12:38:9A:CC:EE:DD");
                cfg.cfg.Add("rawname","NW-RGB62");
                cfg.cfg.Add("identifier","FAE0BA8C-D9B4-B7DB-012C-7A8DDEE0BAFA");
                lights.Add(cfg);
            }
            if (true)
            {
                var cfg = new FakeLightConfig();
                cfg.cfg.Add("fake","true");
                cfg.cfg.Add("mac","12:32:9A:AC:EE:DD");
                cfg.cfg.Add("rawname","NEEWER-NL-116AI");
                cfg.cfg.Add("identifier","FAE0BA8C-ABCD-B7DB-012C-7A8DDEE0BAFA");
                lights.Add(cfg);
            }
            return lights;
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
