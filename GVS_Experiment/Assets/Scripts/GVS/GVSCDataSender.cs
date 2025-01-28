using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO.Ports;
using UnityEngine.Rendering;

public enum EGVSMode
{
    NONE,
    BILATERAL_BIPOLAR, // Kathode am linken Ohr, Anode am rechten Ohr, dabei ist ein Ohr Ground und das andere Elektrode 1
    BILATERAL_UNIPOLAR, // Elektroden 1 und 2 hinter den Ohren, Ground am Nacken
    UNIPOLAR, // Ground am Nacken, Elektrode 1 hinter einem Ohr
    ODAS, // Elektrode 1 und 2 hinter den Ohren, 3 und 4 an den Schl�fen
    OVR // Elektrode 1 linkes Ohr, 2 Stirn, 3 rechtes Ohr, 4 Nacken, Ground weiter unten am Nacken
}
public class GVSCDataSender : MonoBehaviour
{
    [SerializeField]
    private GVSReporterBase[] gvsInfluencers;
    [SerializeField]
    private SmoothRotation rotator;

    public bool rotationIsSending;
    public bool accIsSending;

    //Serial Port Fields
    [SerializeField]
    private string ComPort = "COM5";
    [SerializeField]
    private int BaudRate = 9600;
    [SerializeField]
    private Parity Parity = Parity.None;
    [SerializeField]
    private StopBits StopBits = StopBits.One;
    [SerializeField]
    private int DataBits = 8;
    [SerializeField]
    private int WriteTimeout = 50;
    private int ReadTimeout = 50;
    private bool DtrEnable = true;
    private bool RtsEnable = false;
    private char Separator = '-';

    private JObject _serialParams;
    private JObject _timeouts = new JObject();

    //Message management
    private int _bytesWrittenCount = 0;
    private int _bytesReadCount = 0;
    private byte[] _lastMessage = new byte[20];

    // Device mode
    private EGVSMode GVSmode = EGVSMode.BILATERAL_BIPOLAR;
    private bool deviceInitialized = false;
    public bool isLine;

    void Start()
    {
        UnitySerialPort.Instance.ComPort = ComPort;
        UnitySerialPort.Instance.BaudRate = BaudRate;
        UnitySerialPort.Instance.Parity = Parity;
        UnitySerialPort.Instance.StopBits = StopBits;
        UnitySerialPort.Instance.DataBits = DataBits;
        UnitySerialPort.Instance.WriteTimeout = WriteTimeout;
        UnitySerialPort.Instance.ReadTimeout = ReadTimeout;
        UnitySerialPort.Instance.DtrEnable = DtrEnable;
        UnitySerialPort.Instance.RtsEnable = RtsEnable;
        UnitySerialPort.Instance.ReadDataMethod = UnitySerialPort.ReadMethod.ReadHex;
        UnitySerialPort.Instance.Separator = Separator;
        _serialParams = new JObject
        {
            ["BaudRate"] = BaudRate.ToString(),
            ["ComPort"] = ComPort,
            ["Parity"] = Parity.ToString(),
            ["StopBits"] = StopBits.ToString(),
            ["DataBits"] = DataBits
        };
        _timeouts = new JObject
        {
            ["ReadTimeout"] = ReadTimeout,
            ["WriteTimeout"] = WriteTimeout
        };
        UnitySerialPort.SerialPortOpenEvent += OnSerialPortOpen;
        // Subscribe to the SerialDataParseEvent
        UnitySerialPort.SerialDataParseEvent += ParseMessage;


    }

    private void OnSerialPortOpen()
    {
        if (UnitySerialPort.Instance.IsPortInitalized)
            Debug.Log("GVS Cotnacted successfully.");
    }

    void OnDestroy()
    {
        // Unsubscribe from the SerialDataParseEvent
        UnitySerialPort.SerialDataParseEvent -= ParseMessage;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OpenSerialPortToGVS();
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            InitalizeGVSDevice();
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            SendMessageToSerialPort("2");
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SendMessageToSerialPort("3");
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            SendMessageToSerialPort("4");
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            SendMessageToSerialPort("5");
        }
    }
    private void OnEnable()
    {
        SubscribeToTrackers();
    }
    private void OnDisable()
    {
        UnsubscribeFromTrackers();
    }

    //GVS Setup
    private void OpenSerialPortToGVS()
    {
        if (!deviceInitialized)
        {
            UnitySerialPort.Instance.OpenSerialPort();
        }
    }
    public bool IsDeviceInitalized()
    {
        return deviceInitialized;
    }

    //GVS Mode
    public void SetGVSMode(EGVSMode mode)
    {
        GVSmode = mode;
    }

    private void HandleRotation(Vector3 vector, AccelerationTypes types)
    {
        if (!rotationIsSending || vector.y == 0) return;
        SendValueToGVS(vector.y);
    }

    private void HandleAcceleration(Vector3 direction, AccelerationTypes type)
    {
        if (!accIsSending) return;
        SendValueToGVS(direction);
    }

    void SendValueToGVS(float value)
    {
        string valueString = value.ToString();
        SendMessageToSerialPort(valueString);
    }
    void SendValueToGVS(Vector3 value)
    {
        string valueString = value.ToString();
        SendMessageToSerialPort(valueString);
    }
    bool SendMessageToSerialPort(string message)
    {
        if (UnitySerialPort.Instance != null && UnitySerialPort.Instance.SerialPort.IsOpen)
        {
            if (isLine)
            {
                UnitySerialPort.Instance.SendSerialDataAsLine(message);
            }
            else
            {
                UnitySerialPort.Instance.SendSerialData(message);
            }
            return true;
        }
        else
        {
            Debug.LogWarning("Serial port is not open!");
            return false;
        }
    }
    bool SendBytesToSerialPort(byte[] message)
    {
        if (UnitySerialPort.Instance != null && UnitySerialPort.Instance.SerialPort.IsOpen)
        {
            UnitySerialPort.Instance.SendSerialData(message);
            return true;
        }
        else
        {
            Debug.LogWarning("Serial port is not open!");
            return false;
        }
    }

    private void SubscribeToTrackers()
    {
        foreach (var tracker in gvsInfluencers)
        {
            if (tracker != null)
            {
                tracker.Subscribe(HandleAcceleration);
            }
        }
        rotator.Subscribe(HandleRotation);
    }
    private void UnsubscribeFromTrackers()
    {
        foreach (var tracker in gvsInfluencers)
        {
            if (tracker != null)
            {
                tracker.Unsubscribe(HandleAcceleration);
            }
        }
        rotator.Unsubscribe(HandleRotation);
    }

    private void ParseMessage(string[] data, string rawData)
    {
        // Process the data here
        string head = data[0];
        string length = data[1];
        string body = "";
        string tail = "";
        for (int i = 2; i < data.Length - 2; i++)
        {
            body += data[i];
            body += " ";
        }
        for (int i = data.Length - 2; i < data.Length; i++)
        {
            tail += data[i];
            tail += " ";
        }
        Debug.Log("Received data from Serial Port: " + head + " " + length + " " + body + " " + tail);
        string[] messageWithoutHead = body.Split(" ");

        /*
         1. Recieve message
         2. Turn message to string
            1. Check Message
            2. Build string
         3. Print
         */
        string message = GetMessageAsString(messageWithoutHead, Int32.Parse(length));
        Debug.Log(message);
    }

    private string GetMessageAsString(string[] messageWithoutHead, int length)
    {
        string message = CheckMesage(messageWithoutHead[0]);

        if (length > 1)
        {
            if (messageWithoutHead[1] == "09")
            {
                message += " Electrode ";
                message += messageWithoutHead[2];
                message += ": ";
                float value = float.Parse(messageWithoutHead[3]);
                float mA = (value * 0.02f) - 2.56f;
                message += mA.ToString();
                message += "mA";
                return message;
            }
            message += ": ";
            for (int i = 0; i < length; i++)
            {
                message += messageWithoutHead[i];
                message += " ";
            }
        }

        return message;
    }

    private string CheckMesage(string message)
    {
        string translation = "";
        switch (message)
        {
            case "00":
                translation = "mdgCmdAccepted";
                break;
            case "01":
                translation = "mdgCmdRejectedInvalidMode";
                break;
            case "02":
                translation = "mdgCmdRejectedExpectedSOC";
                break;
            case "03":
                translation = "mdgCmdRejectedLengthBad";
                break;
            case "04":
                translation = "mdgCmdRejectedInvalidCdg";
                break;
            case "05":
                translation = "mdgCmdRejectedLengthToCdgBad";
                break;
            case "06":
                translation = "mdgCmdRejectedEOCNotPresent";
                break;
            case "07":
                translation = "mdgCmdRejectedChecksum";
                break;
            case "08":
                translation = "mdgRxCmdTimeout";
                break;
            case "09":
                translation = "mdgCmdExpectedSOC";
                break;
            case "0a":
                translation = "mdgResync";
                break;
            case "0b":
                translation = "mdgExitedModeInit";
                break;
            case "0c":
                translation = "mdgEnteredModeIdle";
                break;
            case "0d":
                translation = "mdgExitedModeIdle";
                break;
            case "0e":
                translation = "mdgEnteredModeDirect";
                break;
            case "0f":
                translation = "mdgExitedModeDirect";
                break;
            case "10":
                translation = "mdgEnteredModePgmScr";
                break;
            case "11":
                translation = "mdgExitedModePgmScr";
                break;
            case "12":
                translation = "mdgEnteredModeRunScr";
                break;
            case "13":
                translation = "mdgExitedModeRunScr";
                break;
            case "14":
                translation = "mdgEnteredModeFault";
                break;
            case "15":
                translation = "mdgExitedModeFault";
                break;
            case "16":
                translation = "mdgModeDirectSelected";
                break;
            case "17":
                translation = "mdgModeDirectDeselected";
                break;
            case "18":
                translation = "mdgModePgmScrSelected";
                break;
            case "19":
                translation = "mdgModePgmScrDeselected";
                break;
            case "1a":
                translation = "mdgModeRunScrSelected";
                break;
            case "1b":
                translation = "mdgModeRunScrDeselected";
                break;
            case "1c":
                translation = "mdgMode";
                break;
            case "1d":
                translation = "mdgAllElectrodesDld";
                break;
            case "1e":
                translation = "mdgCmdRejectedElectrodeRange";
                break;
            case "1f":
                translation = "mdgScrMemCleared";
                break;
            case "20":
                translation = "mdgScrMemUlded";
                break;
            case "21":
                translation = "mdgCmdRejectedUldMemAddrRange";
                break;
            case "22":
                translation = "mdgScrMemDld";
                break;
            case "23":
                translation = "mdgCmdRejectedDldMemAddrRange";
                break;
            case "24":
                translation = "mdgScrArmed";
                break;
            case "25":
                translation = "mdgCmdRejectedScrArmAddr";
                break;
            case "26":
                translation = "mdgScrDisarmed";
                break;
            case "27":
                translation = "mdgScrStarted";
                break;
            case "28":
                translation = "mdgCmdRejectedScrRunNotArmed";
                break;
            case "29":
                translation = "mdgScrStopped";
                break;
            case "2a":
                translation = "mdgScrTrace";
                break;
            case "2b":
                translation = "mdgLclCtrlDisabled";
                break;
            case "2c":
                translation = "mdgLclCtrlEnabled";
                break;
            case "2d":
                translation = "mdgFault";
                break;
            case "2e":
                translation = "mdgFaultStatusCleared";
                break;
            case "2f":
                translation = "mdgRAMDld";
                break;
            case "30":
                translation = "mdgCmdRejectedDldRAMAddrRange";
                break;
            case "31":
                translation = "mdgLclCmdRejectedLclCtrlDisabled";
                break;
            case "32":
                translation = "mdgTilt";
                break;
            case "33":
                translation = "mdgCalibrate";
                break;
            case "34":
                translation = "mdgRxRateTooFast";
                break;
            default:
                translation = "Unknown Message";
                break;
        }
        return translation;
    }

    private bool InitalizeGVSDevice()
    {
        byte[] InitCommandBytes = new byte[] { 0xAA, 0x01, 0x01, 0x01, 0x55 };
        return SendBytesToSerialPort(InitCommandBytes);
    }

}