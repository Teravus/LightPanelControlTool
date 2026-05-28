using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using InTheHand.Net.Sockets;
using InTheHand.Bluetooth;
using System.Threading;
using System.Globalization;

namespace NeewerLightControlBT
{

    /// <summary>
    /// Represents one Neewer Bluetooth LE light and translates high-level instructions into BLE writes.
    /// </summary>
    /// <remarks>
    /// The packet layouts used by this type are reverse engineered from observed Neewer light behavior.
    /// They are not official protocol definitions. Model capability detection is incomplete because the
    /// lights do not appear to advertise a reliable feature matrix through the BLE endpoints used here.
    /// </remarks>
    public class NeewerLight
    {
        public NeewerLight()
        {

        }
        public void LookupLightDataByNameID (string name, string ID)
        {


            var lightnames = NeewerLightConstant.getLightNames(name, ID);
            var rgbSupport = NeewerLightConstant.isRGBOther(name);
            var lighttype = NeewerLightConstant.getLightType(lightnames.Item1, name, lightnames.Item2);
            var fx = NeewerLightConstant.getLightFX(lighttype);
            _lighttype = lighttype;
            _nickName = lightnames.Item1;
            _projectName = lightnames.Item2;
            var fakeconfigs = NeewerLightConstant.getFakeLightConfigs();
            foreach( var config in fakeconfigs)
            {
                if (config.cfg["rawname"] == name)
                {
                    _macAddress = config.cfg["mac"];
                }
            }
            this.LightType = lighttype;
        }

        private static readonly Guid ServiceGuid = Guid.Parse("69400001-B5A3-F393-E0A9-E50E24DCCA99");

        private bool isOn = false;
        private int channel = 1;
        private bool supportGMRange = false;
        private int brrValue = 50;
        private int cctValue = 53;
        private int hueValue = 0;
        private int satValue = 0;
        private int gmmValue = -50;

        private string _rawName = string.Empty;
        private string _identifier = string.Empty;
        private string _nickName = string.Empty;
        private string _projectName = string.Empty;
        private string _macAddress = string.Empty;
        private byte _lighttype = 0x00;
        private nLightMode _lightmode = nLightMode.OFF;

        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private const int DelayBetweenWrites = 15;
        private NeewerLightFX lastActiveEffect = null;
        public bool IsDisposed { get { return Device == null; } }
        private byte maxChannel
        {
            get
            {
                if (supportedFX.Count == 0)
                    return 0x00;
                return (byte)supportedFX.Count;
            }
        }

        public byte LightType
        {
            get
            {
                return _lighttype;
            }
            set
            {
                _lighttype = value;
                var fxs = NeewerLightConstant.getLightFX(_lighttype);
                supportedFX = fxs;
                supportedSource = NeewerLightConstant.getLightSources(_lighttype);

            }
        }
        public async Task DisconnectAsync()
        {
            if (Device == null)
                throw new InvalidOperationException("No Device has been scanned and assigned to this class");
            foreach (var characteristic in subscribedCharacteristics)
            {
                await characteristic.StopNotificationsAsync();
                characteristic.CharacteristicValueChanged -= Characteristic_CharacteristicValueChanged;
            }
            writeCharacteristic = null;
            
            subscribedCharacteristics.Clear();
            this.Device.Gatt.Disconnect();
            this.Device = null;

        }
        public async Task<bool> ConnectAsync()
        {
            if (Device == null)
                return false;// throw new InvalidOperationException("No Device has been scanned and assigned to this class");

            try
            {
                List<GattService> services = await Device.Gatt.GetPrimaryServicesAsync();
                foreach (var service in services)
                {
                    if (service.Uuid.Value == ServiceGuid)
                    {
                        System.Diagnostics.Debug.WriteLine($"UUID: {service.Uuid}, Primary: {service.IsPrimary}");
                        publishedServices.Add(service);
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                return false;
            }

            foreach (var services in publishedServices)
            {

                var characteristics = await services.GetCharacteristicsAsync();

                foreach (var characteristic in characteristics)
                {
                    //byte[] kittyValue = characteristic.Value; NEEWER-RGB660 PRO

                    System.Diagnostics.Debug.WriteLine($"Properties: {(characteristic.Properties)}, Description: {characteristic.UserDescription}, UUID: {characteristic.Uuid}");
                    if (characteristic.Properties == GattCharacteristicProperties.Notify)
                    {
                        subscribedCharacteristics.Add(characteristic);
                        characteristic.CharacteristicValueChanged += Characteristic_CharacteristicValueChanged;
                        await characteristic.StartNotificationsAsync();
                    }
                    if ((characteristic.Properties & GattCharacteristicProperties.WriteWithoutResponse) == GattCharacteristicProperties.WriteWithoutResponse)
                    {
                        writeCharacteristic = characteristic;
                    }

                }
            }
            return writeCharacteristic != null;


        }

        public async Task TurnOffAsync()
        {
            List<byte> TurnOffCommand = new List<byte>()
            {
                MagicBytes.FIRST_BYTE,
                MagicBytes.SET_POWER,
                MagicBytes.DEVICE_ID,
                0x02,
                0xFC
            };
            if (writeCharacteristic != null)
            {
                

                await WriteToCharacteristic(TurnOffCommand.ToArray());
                isOn = false;
            }
        }

        public async Task ExecuteLightInstruction(NeewerLightInstruction instruction)
        {
            switch (instruction.LightMode)
            {
                case nLightMode.CCTMode:
                    if (!isOn)
                    {
                        await TurnOnAsync();
                    }
                    if (_lightmode != nLightMode.CCTMode )//|| instruction.CCT != cctValue
                    {
                        await setCCTLightValues(brr: instruction.brightness / 100.0f, cct: instruction.CCT, gmm: instruction.gmm);
                    }
                    await setBRRLightValues(brr: instruction.brightness / 100.0f, pcctValue: instruction.CCT);
                    break;
                case nLightMode.HSIMode:
                    if (!isOn)
                    {
                        await TurnOnAsync();
                    }
                    var colors = instruction.RGB.ToHSL();
                    await setRGBLightValues(brr: colors.luminosity, hue: colors.hue, sat: colors.saturation, overrideBrightness:instruction.brightness);
                    break;
                case nLightMode.SCEMode:
                    NeewerLightFX effect = lastActiveEffect;
                    foreach (var seffect in supportedFX)
                    {
                        //System.Diagnostics.Debug.WriteLine(seffect.name);
                        if (seffect.name.ToUpperInvariant() == instruction.SceneName.ToUpperInvariant())
                            effect = seffect;
                    }
                    if (effect == null)
                        return;

                    await setScene(scene:(byte)effect.id, brightness: instruction.brightness);
                    break;
                case nLightMode.OFF:
                    await TurnOffAsync();
                    break;
            }
        }

        private async Task WriteToCharacteristic(byte[] command)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await writeCharacteristic.WriteValueWithoutResponseAsync(command);
                // Writing too fast to the device could lead to BLE jam, slow down the request with 15ms delay.
                
                await Task.Delay(DelayBetweenWrites);

            }
            
            catch (Exception ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("disconnected") ||
                    ex.Message.ToLowerInvariant().Contains("out of range") ||
                    ex.Message.ToLowerInvariant().Contains("could not find") ||
                    ex.Message.ToLowerInvariant().Contains("unreachable") ||
                    ex.Message.ToLowerInvariant().Contains("turned off"))
                {
                    // Handle the disconnection or out-of-range case here.
                }
                else
                {
                    throw;
                }
                // Handle other Bluetooth related exceptions.
            }
            finally
            {
                semaphoreSlim.Release(); // Release the semaphore so the next write can proceed
            }
        }
        public async Task TurnOnAsync()
        {
            List<byte> TurnOnCommand = new List<byte>()
            {
                MagicBytes.FIRST_BYTE,
                MagicBytes.SET_POWER,
                MagicBytes.DEVICE_ID,
                0x01,
                0xFB
            };

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(TurnOnCommand.ToArray());
                isOn = true;
            }
        }

        public async Task SendReadRequestOrigAsync()
        {
            List<byte> SendReadCommand = new List<byte>()
            {
                MagicBytes.FIRST_BYTE,
                MagicBytes.REQUEST_DATA,
                0x00,
                0xFC
            };

            //var request = appendCheckSum();

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(SendReadCommand.ToArray());
            }
        }
        public async Task SendReadRequestAsync(byte experimentalbyte = 0x00)
        {
            List<byte> SendReadCommand = new List<byte>()
            {
                MagicBytes.FIRST_BYTE,
                MagicBytes.REQUEST_DATA,
                0x01,
                experimentalbyte
            };

            var request = appendCheckSum(SendReadCommand.ToArray());

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(request);
            }
        }
        public async Task SendReadPowerRequestAsync()
        {

            var request = BleCommand.readPower;

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(request);
            }
        }
        public async Task SendReadChanRequestAsync()
        {

            var request = BleCommand.readChan;

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(request);
            }
        }
        public Dictionary<string,string> getConfig(bool intrinsicOnly = false)
        {
            Dictionary<string, string> vals = new Dictionary<string, string>();
            vals.Add("mac", _macAddress);
            vals.Add("rawname", _rawName);
            vals.Add("identifier", _identifier);
            if (!intrinsicOnly)
            {
                vals.Add("on", isOn.ToString());
                vals.Add("mod", ""); // todo Light mode
                vals.Add("cct", cctValue.ToString());
                vals.Add("brr", brrValue.ToString());
                vals.Add("chn", channel.ToString());
                vals.Add("hue", hueValue.ToString());
                vals.Add("sat", satValue.ToString());
                vals.Add("gmm", gmmValue.ToString());
                vals.Add("nme", "");
                //vals.Add("supportedFX", supportedFX);
                vals.Add("supportedSource", supportedSource.ToString());
                vals.Add("type", LightType.ToString());
                vals.Add("nickname", nickName.ToString());
                vals.Add("projectname", projectName.ToString());
            }
            else
            {

            
                vals.Add("type", LightType.ToString());
                vals.Add("nickname", nickName.ToString());
                vals.Add("projectname", projectName.ToString());
            }
            return vals;
        }
        public CCTRange CCTRangea()
        {
            // Default CCT range from 3200k–5600k
            // some lights support extended CCT range from 3200K–8500K such as
            // https://neewer.com/products/neewer-sl80-10w-rgb-led-video-light-10097903?_pos=1&_sid=dfa97e049&_ss=r&variant=37586440683713
            if (_lighttype == 6) {
                if (_projectName.Contains("SL140")) {
                    // https://neewer.com/products/neewer-sl-140-rgb-led-light-full-color-rechargeable-pocket-size-10097200?_pos=2&_sid=3ff26da17&_ss=r
                    return new CCTRange(25, 90);
                }
                else
                {
                    return new CCTRange(25, 85);
              }
            }
            if (_lighttype == 22) {
                return new CCTRange(27, 65);
            }
            return new CCTRange(32, 56);
        }
        public string deviceName
        {
            get 
            {
                if (_rawName.StartsWith("NW")) 
                { 
                    return $"NW-{_projectName}"; 
                } else { 
                    return _rawName; 
                }
            }
        }

        public string nickNameSuffix
        {
            get
            {
                if (string.IsNullOrEmpty(_macAddress))
                    return _identifier.Substring(_identifier.Length - 6, 6);
                var crunchedMac = _macAddress.Replace(":", "");
                var tmpval = crunchedMac.Substring(crunchedMac.Length - 6, 6);

                return string.IsNullOrEmpty(tmpval) ? _identifier.Substring(_identifier.Length - 6, 6) : tmpval;
            }
        }

        public string nickName
        {
            get
            {
                if (string.IsNullOrEmpty(_nickName))
                {
                    var name = NeewerLightConstant.getLightNames(rawname: rawName, identifier: nickNameSuffix);
                    _nickName = name.Item1;
                }
                return _nickName;
            }
        }

        public string projectName
        {
            get
            {
                if (string.IsNullOrEmpty(_projectName)) 
                {
                    var name = NeewerLightConstant.getLightNames(rawname: rawName, identifier: nickNameSuffix);
                    _projectName = name.Item2;
                }
                //if _projectName == nil {
                //Logger.error("Unable to get projectName")
                //}
                return _projectName;
            }
        }

        public string identifier
        {
            get
            {
                if (!string.IsNullOrEmpty(_identifier)) {
                    return _identifier;
                }
                else
                {
                    _identifier = $"{Device.Id}";
                }
                return !string.IsNullOrEmpty(_identifier) ? _identifier : "";
            }
            internal set
            {
                _identifier = value;
            }
        }
        public string rawName
        {
            get
            {
                if (!string.IsNullOrEmpty(_rawName))
                {
                    return _rawName;
                }
                string name = Device.Name;
                _rawName = name;
                return _rawName;
            }
            internal set
            {
                _rawName = value;
            }
        }
        public string getMAC()
        {
            return _macAddress ?? string.Empty; 
        }
        public byte lightType
        {
            get
            {
                if (_lighttype <=0 )
                {
                    _lighttype = NeewerLightConstant.getLightType(nickName: nickName, str: "", projectName: projectName);
                }
                return _lighttype;
            }
        }
        public async Task sendKeepAlive()
        {
            if (isOn)
            {
                await TurnOnAsync();
            }
            else
            {
                await TurnOffAsync();
            }
        }

        // Helper method to clamp values within a range
        private int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        private int Clamp(float value, int min, int max)
        {
            return (int)((value < min) ? min : (value > max) ? max : value);
        }
        //private int Clamp(byte value, byte min, byte max)
        //{
        //    return Math.Max(min, Math.Min(max, value));
        //}
        private byte[] getCCTDataLightValue(float brightness, float correlatedColorTemperature, float gmm)
        {
            cctValue = (int)correlatedColorTemperature;
            float ratio = 100.0f;
            if (brightness > 1.0f)
            {
                ratio = 1.0f;
            }

            // Assuming CCTRange is a class that holds minCCT and maxCCT values
            CCTRange cctRange = new CCTRange();
            int newCctValue = Clamp((int)correlatedColorTemperature, cctRange.MinCCT, cctRange.MaxCCT);
            int newBrrValue = Clamp((int)(brightness * ratio), 0, 100);
            int newGmValue = Clamp((int)gmm, -50, 50);

            // Assuming gmmValue, cctValue, and brrValue are properties of type that holds a Value field.
            gmmValue = newGmValue;
            //cctValue = newCctValue;
            brrValue = newBrrValue;


            int dimmingCurveType = 0x04;
            byte[] iArr = new byte[] { 2,(byte)brrValue, (byte)cctValue };

            // Assuming composeSingleCommandWithMac is a method that creates a byte array command with a MAC address
            byte[] bArr1 = composeSingleCommand(MagicBytes.setCCTLightTag, iArr);

            // In C#, you can directly return the byte array
            return bArr1;
        }
        private byte[] getCCTLightValue(float brightness, float correlatedColorTemperature)
        {
            float ratio = 100.0f;
            if (brightness > 1.0f)
            {
                ratio = 1.0f;
            }
            cctValue = (int)correlatedColorTemperature;
            CCTRange cctRange = new CCTRange();
            if ((int)correlatedColorTemperature > cctRange.MaxCCT && (int)correlatedColorTemperature < cctRange.MaxCCT * 100 && (int)correlatedColorTemperature > cctRange.MinCCT)
            {
                correlatedColorTemperature *= 0.01f;
            }
            // cct range from 0x20(32) - 0x38(56) 32 stands for 3200K, 56 stands for 5600K
            
            int newCctValue = Clamp((int)correlatedColorTemperature, cctRange.MinCCT, cctRange.MaxCCT);
            // brr range from 0x00 - 0x64
            int newBrrValue = Clamp((int)(brightness * ratio), 0, 100);

            if (newCctValue == 0 || ((int)correlatedColorTemperature * 100) == cctValue)
            {
                // only adjust the brightness and keep the color temp
                if (brrValue == newBrrValue)
                {
                    return Array.Empty<byte>();
                }
                brrValue = newBrrValue;

                byte[] bArr1 = composeSingleCommand(MagicBytes.setCCTLightTag, new byte[] { (byte)brrValue,(byte)newCctValue });
                return bArr1;
            }

            
            brrValue = newBrrValue;

            byte[] bArr2 = composeSingleCommand(MagicBytes.setCCTLightTag, new byte[] { (byte)brrValue, (byte)cctValue });
            return bArr2;
        }
        private byte[] getCCTOnlyLightValue(float brightness, float correlatedColorTemperature)
        {
            CCTRange cctRange = new CCTRange();
            int newCctValue = Clamp((int)correlatedColorTemperature, cctRange.MinCCT, cctRange.MaxCCT);
            int newBrrValue = Clamp((int)brightness, 0, 100);

            if (newCctValue == 0)
            {
                if (brrValue == newBrrValue)
                {
                    return Array.Empty<byte>();
                }
                brrValue = newBrrValue;

                byte[] bArr11 = composeSingleCommand( MagicBytes.setLongCCTLightBrightnessTag, new byte[] { (byte)brrValue });
                return bArr11;
            }

            cctValue = newCctValue;
            brrValue = newBrrValue;

            byte[] bArr1 = composeSingleCommand(MagicBytes.setLongCCTLightBrightnessTag, new byte[] { (byte)brrValue });
            byte[] bArr2 = composeSingleCommand(MagicBytes.setLongCCTLightCCTTag, new byte[] { (byte)cctValue });
            byte[] bArr = bArr1.Concat(bArr2).ToArray();

            return bArr;
        }
        private byte[] getRGBLightValue(float brr, float theHue, float sat, float overridebrightness)
        {
            float ratio = 100.0f;
            if (brr > 1.0f)
            {
                ratio = 1.0f;
            }

            // Clamping the values to their respective ranges
            int newBrrValue = Clamp(((brr * ratio) * (overridebrightness / 100)), 0, 100);
            int newSatValue = Clamp((sat * 100.0f), 0, 100);
            int newHueValue = Clamp((theHue * 360.0f), 0, 360);

            const int byteCount = 4;
            byte[] bArr = new byte[byteCount + 4];

            bArr[0] = MagicBytes.prefixTag; // Assuming this is an int constant
            bArr[1] = MagicBytes.setRGBLightTag; // Assuming this is an int constant
            bArr[2] = byteCount;
            // 4 elements
            bArr[3] = (byte)(newHueValue & 0xFF);
            bArr[4] = (byte)((newHueValue & 0xFF00) >> 8); // Calculated from RGB
            bArr[5] = (byte)(newSatValue); // Saturation 0x00 ~ 0x64
            bArr[6] = (byte)(newBrrValue); // Brightness

            // Assuming brrValue, hueValue, and satValue are properties of some class instance
            brrValue = newBrrValue;
            hueValue = newHueValue;
            satValue = newSatValue;

            // Assuming AppendCheckSum takes an int array and converts it into a byte array with a checksum
            byte[] bArr1 = appendCheckSum(bArr);

            // In C#, byte arrays don't need to be converted to NSData, just return the byte array
            return bArr1;
        }

        public async Task setCCTLightValues(float brr, float cct, float gmm)
        {
            byte[] cmd;
            cmd = getCCTDataLightValue(brightness: brr, correlatedColorTemperature: cct, gmm: gmm);
            _lightmode = nLightMode.CCTMode;
            //brrValue = (int)brr;
            //cctValue = (int)cct;
            await WriteToCharacteristic(cmd);
        }
        public async Task setRGBLightValues(float brr, float hue, float sat, float overrideBrightness = 100f)
        {
            byte[] cmd;
            cmd = getRGBLightValue(brr: brr, theHue: hue, sat: sat, overridebrightness:overrideBrightness);
            _lightmode = nLightMode.HSIMode;
            await WriteToCharacteristic(cmd);
        }
       

        public async Task setBRRLightValues(float brr, float pcctValue)
        {
            byte[] cmd = Array.Empty<byte>();
            if (_lightmode == nLightMode.CCTMode)
            {
                if (supportRGB)
                {
                    cmd = getCCTLightValue(brr, pcctValue); // Assuming this method returns byte[]
                }
                else
                {
                    cmd = getCCTOnlyLightValue(brr, pcctValue); // Assuming this method returns byte[]
                }
            }
            else if (_lightmode == nLightMode.HSIMode)
            {
                cmd = getRGBLightValue(brr, hueValue / 360.0f, satValue / 100.0f, 100); // Assuming this method returns byte[]
            }
            else
            {
                cmd = getSceneValue((byte)channel, brr); // Assuming this method returns byte[]
            }

            if (writeCharacteristic != null)
            {
                await WriteToCharacteristic(cmd); // Writing the command to the characteristic
            }
            brrValue = (int)brr;
            
        }


        public async Task setScene(byte scene, float brightness)
        {
            byte[] cmd;
            cmd = getSceneValue(scene, brightness);
            _lightmode = nLightMode.SCEMode;
            await WriteToCharacteristic(cmd);
        }

        private byte[] getSceneValue(byte scene, float brr)
        {
            // brr range from 0x00 - 0x64
            int newBrrValue = Clamp((int)brr, 0, 100);
            brrValue = newBrrValue;

            // Assuming maxChannel is defined elsewhere and represents the maximum channel value
            channel = Clamp(scene, 1, maxChannel);

            const int byteCount = 2;
            byte[] bArr = new byte[byteCount + 4];

            bArr[0] = MagicBytes.prefixTag; // Assuming this is an int constant
            bArr[1] = MagicBytes.setSceneTag; // Assuming this is an int constant
            bArr[2] = (byte)byteCount;
            // 2 elements
            bArr[3] = (byte)newBrrValue; // Brightness value from 0-100
            bArr[4] = scene;       // Scene value from 1 to maxChannel

            // Assuming AppendCheckSum takes an int array and converts it into a byte array with a checksum
            byte[] bArr1 = appendCheckSum(bArr);

            // In C#, you can directly return the byte array
            return bArr1;
        }
        private byte[] GetSceneCommand(string mac, NeewerLightFX fxx)
        {
            /*
            Oct 25 01:41:40.143  ATT Send         0x004A  00:00:00:00:00:00  Write Command - Handle:0x000E - Value: 7891 0BDF 243A B446 5D8B 1107 0103 4F  SEND
            Oct 25 01:41:42.493  ATT Send         0x004A  00:00:00:00:00:00  Write Command - Handle:0x000E - Value: 7891 0BDF 243A B446 5D8B 1107 0101 4D  SEND

            CMD TAG   SIZE       MAC                     SCE_TAG  SCE_ID(01~0C)     (BRR 00~64)    (COLOR 00~02)      (Speed 00~0A)      (checksum)
            78   91   0B         (DF 24 3A B4 46 5D)     8B       11                 07             01                 03                 4F

            Name               ID
            Lighting           01             BRR   CTT   SPEED
            Paparazzi          02             BRR   CTT   GM       SPEED
            Defective bulb     03             BRR   CTT   GM       SPEED
            Explosion          04             BRR   CTT   GM       SPEED     Sparks(01~0A)
            Welding            05             BRR_low   BRR_high     CTT   GM       SPEED
            CCT flash          06             BRR   CTT   GM       SPEED
            HUE flash          07             BRR   HUE (2Bytes little Endian 0000~6801)   SAT (00~64)   SPEED
            CCT pulse          08             BRR   CCT   GM       SPEED
            HUE pulse          09             BRR   HUE (2Bytes little Endian 0000~6801)   SAT (00~64)   SPEED
            Cop Car            0A             BRR   RED_AND_BLUE(00~05 Red,Blue, Red and Blue, White and Blue, Red blue  white) SPEED
            Candlelight        0B             BRR_low   BRR_high   CTT     GM       SPEED     Sparks
            HUE Loop           0C             BRR   HUE_low  HUE_high      SPEED
            CCT Loop           0D             BRR   CCT_low  CCT_high      SPEED
            INT loop           0E             BRR_low   BRR_high   HUE     SPEED
            TV Screen          0F             BRR   CCT   GM       SPEED
            Firework           10             BRR   COLOR(00 Single color, 01 Color, 02 Combined)   SPEED   Sparks
            Party              11             BRR   COLOR(00 Single color, 01 Color, 02 Combined)   SPEED
            */
            // scene from 1 ~ 9
            channel = Clamp((byte)fxx.id, 1, maxChannel);
            int byteCount = 8;
            if (fxx.needBRR) byteCount += 1;
            if (fxx.needBRRUpperBound) byteCount += 1;
            if (fxx.needHUE) byteCount += 2;
            if (fxx.needHUEUpperBound) byteCount += 2;
            if (fxx.needSAT) byteCount += 1;
            if (fxx.needCCT) byteCount += 1;
            if (fxx.needCCTUpperBound) byteCount += 1;
            if (fxx.needGM) byteCount += 1;
            if (fxx.needColor && fxx.colors.Length > 0) byteCount += 1;
            if (fxx.needSpeed) byteCount += 1;
            if (fxx.needSparks && fxx.sparkLevel.Length > 0) byteCount += 1;
            List<byte> bArr = new List<byte>(byteCount + 4);
            bArr.Add(MagicBytes.prefixTag);
            bArr.Add(MagicBytes.setSCEDataTag);
            bArr.Add((byte)byteCount);
            var macBytes = mac.Split(':').Select(part => Convert.ToByte(part, 16)).ToArray();
            while (macBytes.Length < 6)
            {
                Array.Resize(ref macBytes, 6);
            }
            bArr.AddRange(macBytes);
            bArr.Add(MagicBytes.setSCESubTag);
            bArr.Add((byte)channel);
            int idx = 11;

            if (fxx.needBRR)
            {
                bArr.Add((byte)Clamp(fxx.brrValue, 0, 100));
                idx++;
            }

            if (fxx.needBRRUpperBound)
            {
                bArr.Add((byte)Clamp(fxx.brrUpperValue, 0, 100));
                idx++;
            }

            if (fxx.needHUE)
            {
                int newHueValue = Clamp(fxx.hueValue, 0, 360);
                bArr.Add((byte)(newHueValue & 0xFF)); // LSB
                idx++;
                bArr.Add((byte)((newHueValue >> 8) & 0xFF)); // MSB
                idx++;
            }

            if (fxx.needHUEUpperBound)
            {
                int newHueValue = Clamp(fxx.hueUpperValue, 0, 360);
                bArr.Add((byte)(newHueValue & 0xFF)); // LSB
                idx++;
                bArr.Add((byte)((newHueValue >> 8) & 0xFF)); // MSB
                idx++;
            }
            if (fxx.needSAT)
            {
                bArr.Add((byte)Clamp(fxx.satValue, 0, 100));
                idx++;
            }
            if (fxx.needCCT)
            {
                var cttrange = new CCTRange();
                bArr.Add((byte)Clamp(fxx.cctValue, cttrange.MinCCT, cttrange.MaxCCT));
                idx++;
            }
            if (fxx.needCCTUpperBound)
            {
                var cttrange = new CCTRange();
                bArr.Add((byte)Clamp(fxx.cctUpperValue, cttrange.MinCCT, cttrange.MaxCCT));
                idx++;
            }
            if (fxx.needGM)
            {
                bArr.Add((byte)(Clamp(fxx.gmValue, -50, 50) + 50));
                idx++;
            }
            if (fxx.needColor && fxx.colors.Length > 0)
            {
                bArr.Add((byte)Clamp(fxx.colorValue, 0, fxx.colors.Length));
                idx++;
            }

            if (fxx.needSpeed)
            {
                bArr.Add((byte)Clamp(fxx.speedValue, 1, 10));
                idx++;
            }
            if (fxx.needSparks && fxx.sparkLevel.Length > 0)
            {
                bArr.Add((byte)Clamp(fxx.sparksValue, 0, fxx.sparkLevel.Length));
                idx++;
            }
            // Nov 06 23:52:46.851  ATT Send         0x005B  00:00:00:00:00:00  Write Command - Handle:0x000E - Value: 7891 0BDF 243A B446 5D8B 0132 3706 A3  SEND
            var arr = appendCheckSum(bArr.ToArray());
            return arr;
        }

        

        private byte[] composeSingleCommand( byte tag, byte[] vals)
        {
            int byteCount = vals.Count();
            byte[] bArr = new byte[byteCount + 4];
            bArr[0] = MagicBytes.FIRST_BYTE;
            bArr[1] = tag;
            bArr[2] = (byte)byteCount;
            var idx = 3;
            foreach (var val in vals)
                bArr[idx++] = val;

            return appendCheckSum(bArr);
        }

        private bool validateCheckSum(byte[] data)
        {
            if (data.Length < 2)
            {
                return false;
            }

            int checkSum = 0;
            for (int idx = 0; idx < data.Length - 1; idx++)
            {
                checkSum += data[idx];
            }

            if (data[data.Length - 1] == (byte)(checkSum & 0xFF))
            {
                return true;
            }
            return false;
        }
        private byte[] composeSingleCommandWithMac(byte tag, string mac, byte subtag, byte[] vals)
        {
            int byteCount = vals.Length;
            byte[] bArr = new byte[byteCount + 11];
            bArr[0] = MagicBytes.FIRST_BYTE; // Assuming this is a predefined byte constant
            bArr[1] = tag;
            bArr[2] = (byte)(byteCount + 7);

            // Split the MAC address and convert each part from hexadecimal to integer
            string[] macParts = mac.Split(':');
            byte[] macBytes = macParts.Select(part => Convert.ToByte(part, 16)).ToArray();

            // If the MAC address is shorter than 6 bytes, pad the array with zeroes
            Array.Resize(ref macBytes, 6);

            // Copy MAC address bytes into the command array
            Array.Copy(macBytes, 0, bArr, 3, 6);

            bArr[9] = (byte)subtag;

            // Copy the values into the command array
            Array.Copy(vals, 0, bArr, 10, vals.Length);

            // Append the checksum at the end
            return appendCheckSum(bArr); // Assuming AppendCheckSum is implemented to handle byte arrays
        }

        private byte[] appendCheckSum(byte[] bArr)
        {
            byte[] bArr1 = new byte[bArr.Length];

            int checkSum = 0;
            for (int idx = 0; idx < bArr.Length - 1; idx++)
            {
                // In C#, you don't usually need to handle negative bytes in the same way because bytes are unsigned
                bArr1[idx] = (byte)bArr[idx];
                checkSum += bArr1[idx];
            }

            bArr1[bArr.Length - 1] = (byte)(checkSum & 0xFF);
            return bArr1;
        }
        public bool supportRGB
        {
            get { return NeewerLightConstant.getRGBLightTypes().Contains(_lighttype); }
        }

        private List<NeewerLightFX> supportedFX = new List<NeewerLightFX>();
        private List<NeewerLightSource> supportedSource = new List<NeewerLightSource>();

        private int connectionBreakCounter = 0;


        public string ID { get; set; } = string.Empty;
        public BluetoothDevice Device { get; set; } = null;

        public nLightMode Mode { get; set; } = nLightMode.OFF;

        private List<GattService> publishedServices = new List<GattService>();
        private List<GattCharacteristic> subscribedCharacteristics = new List<GattCharacteristic>();
        private GattCharacteristic writeCharacteristic = null;
        public enum PowerState
        {
            ON,
            OFF
        }
        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }
       
        private void Characteristic_CharacteristicValueChanged(object sender, GattCharacteristicValueChangedEventArgs e)
        {

            //var reader = e
            //byte[] input = e.Value;

            CharactericEventHandlerAsync(sender, e);

        }

        private async Task CharactericEventHandlerAsync(object sender, GattCharacteristicValueChangedEventArgs e)
        {
            byte[] data = e.Value;
            if (data.Take(MagicBytes.SET_MODE_SCENE_UPDATE_PREFIX.Length).SequenceEqual(MagicBytes.SET_MODE_SCENE_UPDATE_PREFIX) && data.Length == MagicBytes.SET_MODE_SCENE_UPDATE_PREFIX.Length + 2)
            {
                // data[3] range in [0,1,2,3,4,5,6,7,8]
                // Assuming 'channel' is an object with a 'Value' property of type byte
                channel = Clamp((byte)(data[3] + 1), 1, maxChannel); // only 1-maxChannel channel allowed.
                System.Diagnostics.Debug.WriteLine($"handleNotifyValueUpdate {BitConverter.ToString(data)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"handleNotifyValueUpdate {BitConverter.ToString(data)}");
            }
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
