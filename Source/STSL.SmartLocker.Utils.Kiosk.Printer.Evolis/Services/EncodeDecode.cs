using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;


namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Services;

public class EncodeDecode : IEncodeDecode
{
    private SerialPort? TWNPort;
    private bool IsRun = false;
    bool IsInit = false;
    public EncodeDecode(ILogger<EncodeDecode> logger)
    {
        Logger = logger;
    }
    private void threadSearch()
    {
        IsInit = false;
        // Polling device loop
        while (IsRun)
        {
            try
            {
                // Is there a TWN4?
                string PortName = GetTWNPortName(0);
                // Is TWNPort open?
                if (!IsInit)
                {
                    if (PortName == "")
                        // There is no TWN4 connected. Do a silent execption.
                        throw new ApplicationException("");

                    // Ensure, that COM port becomes available. Sleep a while.
                    System.Threading.Thread.Sleep(100);
                    // A TWN4 was connected. Initialize it.
                    ConnectTWN4(PortName);

                    // Do log ErrorMessage
                    Logger.LogDebug("TWN4 connected on Port: " + PortName);
                    IsInit = true;
                }
                // TWN4 is initialized
                if (PortName == "")
                    throw new ApplicationException("TWN4 disconnected");

            }
            catch (Exception ex)
            {
                try
                {
                    if (IsInit)
                        Logger.LogDebug("TWN4 disconnected");
                    IsInit = false;
                    TWNPort.Close();
                }
                catch { }
            }
        }
        try
        {
            // Try to close TWNPort
            TWNPort.Close();
        }
        catch { }
    }// End of threadSearch


    #region Print Message Tools
    private bool PrintData(byte[] Data, int WindowSize)
    {
        // This function formats the message and print it to a listbox
        int i = 0;
        if (Data == null)
            return false;
        OnMessageRecieved?.Invoke(this, "[" + i.ToString("x8") + "]  ");
        while (i < Data.Length)
        {
            if (i % WindowSize == 0 && i / WindowSize > 0)
            {
                OnMessageRecieved?.Invoke(this, "  |" + GetMyASCIIString(Data, i - WindowSize, WindowSize) + "|");
                OnMessageRecieved?.Invoke(this, "[" + i.ToString("x8") + "]  ");
            }
            OnMessageRecieved?.Invoke(this, " " + Data[i].ToString("X2"));
            i++;
        }
        if (i % WindowSize == 0)
            OnMessageRecieved?.Invoke(this, "  |" + GetMyASCIIString(Data, i - WindowSize, WindowSize) + "|");
        else
        {
            for (int x = 0; x < WindowSize - (i % WindowSize); x++)
                OnMessageRecieved?.Invoke(this, "   ");
            OnMessageRecieved?.Invoke(this, "  |" + GetMyASCIIString(Data, i - (i % WindowSize), i % WindowSize));
            for (int x = 0; x < WindowSize - (i % WindowSize); x++)
                OnMessageRecieved?.Invoke(this, " ");
            OnMessageRecieved?.Invoke(this, "|");
        }
        return true;
    }// End of PrintData
    private string GetMyASCIIString(byte[] Data, int index, int count)
    {
        // This function replace all non chars by •
        if (index + count > Data.Length)
            return null;
        string buffer = null;
        for (int i = 0; i < count; i++)
        {
            if (Data[index + i] > 0x21 && Data[index + i] < 0x7F)
                buffer = buffer + System.Text.ASCIIEncoding.UTF8.GetString(Data, index + i, 1);
            else
                buffer = buffer + "•";
        }
        return buffer;
    }// End of GetMyASCIIString
    #endregion

    #region Simple Protocol
    // **************************************************************************************************
    // A Simple Protocol command starts always with two bytes. The first one reflect the API and the 
    // second one the function number.
    // For example the SearchTag function is include in API RF. The related command is 0x18 0x00 0x10.
    // 0x18: API RF
    // 0x00: SearchTag function
    // 0x10: MaxIDBytes
    // **************************************************************************************************
    #endregion

    #region API RF

    byte[] UID;

    public event EventHandler<string>? OnMessageRecieved;

    public bool MiFareLogin(byte[] key, byte keyType, byte sector)
    {
        byte[] request = new byte[10];
        request[0] = 0x0B;
        request[1] = 0x00;
        for (int i = 0; i < key.Length; i++)
        {
            request[i + 2] = key[i];
        }
        request[8] = keyType;
        request[9] = sector;
        Logger.LogDebug("TX:" + BitConverter.ToString(request).Replace("-", string.Empty));
        byte[] buffer = DoTXRX(request);
        string response = BitConverter.ToString(buffer).Replace("-", string.Empty);
        Logger.LogDebug("RX:" + BitConverter.ToString(buffer).Replace("-", string.Empty));
        return response == "0001";
    }

    public string MiFareReadBlock(byte block)
    {
        byte[] request = new byte[3];
        request[0] = 0x0B;
        request[1] = 0x01;
        request[2] = block;
        Logger.LogDebug("TX:" + BitConverter.ToString(request).Replace("-", string.Empty));
        byte[] buffer = DoTXRX(request);
        string response = BitConverter.ToString(buffer).Replace("-", string.Empty);
        Logger.LogDebug("RX:" + BitConverter.ToString(buffer).Replace("-", string.Empty));
        return response;
    }

    private bool? ResetTagTypes()
    {
        if (!IsInit)
        {
            Logger.LogDebug("TWN4 is not connected");
            return null;
        }

        // Define SetTagType request
        byte[] request = { 0x05, 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        Logger.LogDebug("TX:" + BitConverter.ToString(request).Replace("-", string.Empty));
        byte[] buffer = DoTXRX(request);
        string response = BitConverter.ToString(buffer).Replace("-", string.Empty);
        Logger.LogDebug("RX:" + BitConverter.ToString(buffer).Replace("-", string.Empty));
        return response == "00";
    }
    private string? SearchTAG()
    {
        if (!IsInit)
        {
            Logger.LogDebug("TWN4 is not connected");
            return null;
        }

        UID = new byte[10];

        // Define search request
        byte[] request = { 0x05, 0x00, 0x10 };
        // Define search response
        byte[] response = { 0x00, 0x01 };

        Logger.LogDebug("TX:" + BitConverter.ToString(request).Replace("-", string.Empty));

        // DoTXRX and copy answer to buffer
        byte[] buffer = DoTXRX(request);

        if (buffer == null)
            return null;

        // Is first byte, the expected byte?
        if (!CompareArraysSegments(response, 0, buffer, 0, 2))
        {
            Logger.LogDebug("RX:" + BitConverter.ToString(buffer).Replace("-", string.Empty) + "  (No Tag)");
            return null;
        }
        else
        {
            Logger.LogDebug("RX:" + BitConverter.ToString(buffer).Replace("-", string.Empty));
            return BitConverter.ToString(buffer).Replace("-", string.Empty);
        }
    }// End of SearchTAG
    #endregion

    #region Tools for Simple Protocol
    private byte[] GetByteArrayfromPRS(string PRSString)
    {
        // Is string length = 0?
        if (PRSString.Length < 1)
            // Yes, return null
            return null;
        // Initialize byte array, half string length
        byte[] buffer = new byte[PRSString.Length / 2];
        // Get byte array from PRS string
        for (int i = 0; i < (buffer.Length); i++)
            // Convert PRS Chars to byte array buffer
            buffer[i] = byte.Parse(PRSString.Substring((i * 2), 2), NumberStyles.HexNumber);
        // Return byte array
        return buffer;
    }// End of PRStoByteArray
    private string GetPRSfromByteArray(byte[] PRSStream)
    {
        // Is length of PRS stream = 0
        if (PRSStream.Length < 1)
            // Yes, return null
            return null;
        // Iinitialize PRS buffer
        string buffer = null;
        // Convert byte to PRS string
        for (int i = 0; i < PRSStream.Length; i++)
            // convert byte to characters
            buffer = buffer + PRSStream[i].ToString("X2");
        // return buffer
        return buffer;
    }// End of GetPRSfromByteArray
    #endregion

    #region Reader Communication

    private void ConnectTWN4(string PortName)
    {
        // Initialize serial port
        TWNPort.PortName = PortName;
        TWNPort.BaudRate = 115200;
        TWNPort.DataBits = 8;
        TWNPort.StopBits = System.IO.Ports.StopBits.One;
        TWNPort.Parity = System.IO.Ports.Parity.None;
        // NFC functions are known to take less than 2 second to execute.
        TWNPort.ReadTimeout = 2000;
        TWNPort.WriteTimeout = 2000;
        TWNPort.NewLine = "\r";
        // Open TWN4 com port
        TWNPort.Open();
    }// End of ConnectTWN4
    private byte[] DoTXRX(byte[] CMD)
    {
        byte[] Respond = new byte[2];
        try
        {
            // Discard com port inbuffer
            TWNPort.DiscardInBuffer();
            // Generate simple protocol string and send command
            TWNPort.WriteLine(GetPRSfromByteArray(CMD));
            // Read simple protocoll string and convert to byte array
            Respond = GetByteArrayfromPRS(TWNPort.ReadLine());
        }
        catch (Exception ex)
        {
            return null;
            //Logger.WriteLine(ex.Message);
        }
        return Respond;
    }// End of DoTXRX


    #region Tools for connect TWN4
    private string RegHKLMQuerySZ(string SubKey, string ValueName)
    {
        string Data;

        RegistryKey Key = Registry.LocalMachine.OpenSubKey(SubKey);
        if (Key == null)
            return "";
        if (Key.GetValue(ValueName) != null)
            Data = Key.GetValue(ValueName).ToString();
        else
            return "";
        if (Data == "")
            return "";
        if (Key.GetValueKind(ValueName) != RegistryValueKind.String)
            Data = "";
        Key.Close();
        return Data;
    }// End of RegHKLMQuerySZ
    private string FindUSBDevice(string Driver, string DevicePath)
    {
        int PortIndex = 0;

        while (true)
        {
            string Path = "SYSTEM\\CurrentControlSet\\Services\\" + Driver + "\\Enum";
            string Data = RegHKLMQuerySZ(Path, PortIndex.ToString());
            PortIndex++;
            if (Data == "")
                return "";
            string substr = Data.Substring(0, DevicePath.Length).ToUpper();
            if (substr == DevicePath)
                return Data;
        }
    }// End of FindUSBDevice
    private int GetCOMPortNr(string Device)
    {
        string Path = "SYSTEM\\CurrentControlSet\\Enum\\" + Device + "\\Device Parameters";
        string Data = RegHKLMQuerySZ(Path, "PortName");
        if (Data == "")
            return 0;
        if (Data.Length < 4)
            return 0;
        int PortNr = Convert.ToUInt16(Data.Substring(3));
        if (PortNr < 1 || PortNr > 256)
            return 0;
        return PortNr;
    }// End of GetCOMPortNr
    private string GetCOMPortString(int PortNr)
    {
        if (PortNr < 1 || PortNr > 256)
            return "";
        return "COM" + PortNr.ToString();
    }// End of GetCOMPortString
    private string GetTWNPortName(int PortNr)
    {
        string PortName;
        if (PortNr == 0)
        {
            string Path = FindUSBDevice("usbser", "USB\\VID_09D8&PID_0420\\");

            if (Path == "")
                return "";
            PortName = GetCOMPortString(GetCOMPortNr(Path));
            if (PortName == "")
                return "";
        }
        else
        {
            PortName = GetCOMPortString(PortNr);
            if (PortName == "")
                return "";
        }
        return PortName;
    }// End of GetTWNPortName
    #endregion

    #endregion

    #region Tool for byte arrays
    private byte[] AddByteArray(byte[] Source, byte[] Add)
    {
        // Is Source = null
        if (Source == null)
        {
            // Yes, copy Add in Source
            Source = Add;
            // Return source
            return Source;
        }
        // Initialize buffer array, with the length of Source and Add
        byte[] buffer = new byte[Source.Length + Add.Length];
        // Copy Source in buffer
        for (int i = 0; i < Source.Length; i++)
            // Copy source bytes to buffer
            buffer[i] = Source[i];
        // Add the secound array to buffer
        for (int i = Source.Length; i < buffer.Length; i++)
            // Copy Add bytes after the Source bytes in buffer
            buffer[i] = Add[i - Source.Length];
        // Return the combined array buffer
        return buffer;
    }// End of AddByteArray
    private byte[] AddByte2Array(byte[] Source, byte Add)
    {
        if (Source == null)
        {
            return new byte[] { Add };
        }
        // Initialize buffer with the length of Source + 1
        byte[] buffer = new byte[Source.Length + 1];
        // Copy Source in buffer
        for (int i = 0; i < Source.Length; i++)
            // Copy Source bytes in buffer array
            buffer[i] = Source[i];
        // Add byte behind the Source
        buffer[Source.Length] = Add;
        // Return the buffer
        return buffer;
    }// End of AddByte2Array
    private byte[] GetSegmentFromByteArray(byte[] Source, int index, int count)
    {
        // Initialize buffer with the segment size
        byte[] buffer = new byte[count];
        // Copy bytes from index until count
        for (int i = index; i < (index + count); i++)
            // Copy in segment buffer
            buffer[i - index] = Source[i];
        // Return segment buffer
        return buffer;
    }// End of GetSegmentFromByteArray
    private bool CompareArraysSegments(byte[] Array1, int index1, byte[] Array2, int index2, int count)
    {
        // Plausibility check, is index + count longer than arran
        if (((index1 + count) > Array1.Length) || ((index2 + count) > Array2.Length))
            // Yes, return false
            return false;
        // Compare segments of count
        for (int i = 0; i < count; i++)
            // Is byte in Array1 == byte in Array2?
            if (Array1[i + index1] != Array2[i + index2])
                // No, return flase
                return false;
        // Return true
        return true;
    }

    // End of CompareArraysSegment
    #endregion
    public ILogger<EncodeDecode> Logger { get; }

    public void Run()
    {
        // Initialize TWNPort
        TWNPort = new SerialPort();
        // Set Run true
        IsRun = true;
        // Initialize thread tSearch
        Thread tSearch = new Thread(threadSearch);
        // Start thread tSearch
        tSearch.Start();
        SearchTAG();
    }

    public string? GetCardCSN()
    {
        ResetTagTypes();
        // Search tag and return card serial number
        return SearchTAG();
    }

    public string? GetCardHID()
    {
        byte[] MiFareKeyDefault = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        byte KeyA = 0x00;
        byte KeyB = 0x01;
        byte sector = 0x01;
        byte[] MiFareKey01 = {0x48, 0x49, 0x44, 0x20, 0x49, 0x53};
        bool cardPresent = SearchTAG() != null;
        if (cardPresent)
        {
            bool loginSuccess = MiFareLogin(MiFareKey01, KeyA, sector);
            if (loginSuccess) { 
                string data = MiFareReadBlock(0x05);
                if (!(string.IsNullOrEmpty(data))) {
                    string hex = data.Substring(27);
                    // Convert hex to binary
                    string binaryString = Convert.ToString(Convert.ToInt64(hex, 16), 2).PadLeft(35, '0');

                    // Extract 35-bit binary value
                    string binValue = binaryString.Substring(binaryString.Length - 35);

                    // Extract card number (bits 14 to 34)
                    string cardNumBinary = binValue[14..34];
                    int hidNumber = Convert.ToInt32(cardNumBinary, 2);
                    return hidNumber.ToString();
                }
            }
        }
        return null;
    }

}
