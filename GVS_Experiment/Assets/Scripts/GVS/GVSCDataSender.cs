using System;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO.Ports;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.IO.LowLevel.Unsafe;
using Assets.Scripts.GVS;

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
    private MovementTracker[] gvsInfluencers;
    [SerializeField]
    private SmoothRotation rotator;
    [SerializeField]
    private bool isSham = false;
    [SerializeField]
    private bool isDirectional = false;
    [SerializeField]
    private bool isNoisy = false;

    [SerializeField]
    private bool rotationIsSending;
    [SerializeField]
    private bool accIsSending;
    [SerializeField]
    private bool isTesting = false;
    [SerializeField]
    private bool isManual = true;
    [SerializeField]
    private bool logEvents = false;


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
    //private int _bytesReadCount = 0;
    private byte[] _lastMessage = new byte[20];
    private Queue<string> messageBuffer = new Queue<string>();

    // Device mode
    [SerializeField]
    private EGVSMode GVSmode = EGVSMode.BILATERAL_BIPOLAR;
    private bool isPortConnected = false;
    public bool isLine;
    [SerializeField]
    private float maxMiliAmpere = 2.54f;

    private float timer = 1;

    private NoisyGVS noisyGVS = new NoisyGVS();
    public float interpolatorValue = 0;

    private void Awake()
    {
        isSham = false;
        isDirectional = false;
        isNoisy = true;
    }
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
        StartCoroutine(SetupCalibrationPhase());
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

    private IEnumerator SetupCalibrationPhase()
    {
        Debug.Log("Setting up calibration...\n");
        ToggleSerialPortState();
        yield return new WaitUntil(() => (isPortConnected));
        InitalizeGVSDevice();
        yield return new WaitForSecondsRealtime(0.5f);
        SetModeDirect();
        yield return new WaitForSecondsRealtime(0.5f);
        SetupNoisyGVS();
        Debug.Log("Calibration mode initalized...");
        Debug.Log($"<color=red> Remember to set Age, Gender and MSSQ</color>");
        Debug.Log($"<color=red> Remember to set Max Mili Ampere</color>");
        Debug.Log($"<color=green> Starting Experiment: To start experiment, move forward or untick isManual</color>");
        
    }


    void Update()
    {
        interpolatorValue = noisyGVS.Interpolator;
        if (isNoisy)
        {
            NoisyGVS.SetMaxValue(MaxMiliAmpere);
        }
        // Important selections
        // Zero all
        if (Input.GetKeyUp(KeyCode.Alpha9))
        {
            TriggerGVSYaw(0f);
            ZeroAllElectrodes();
        }
        // Killswitch
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            KillSwitch();
        }
        if (IsManual)
        {
            // Test inputs
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                TriggerGVSLateral(MaxMiliAmpere);
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                TriggerGVSLateral(-MaxMiliAmpere);
            }
            if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                TriggerGVSRoll(MaxMiliAmpere);
            }
            if (Input.GetKeyUp(KeyCode.Alpha6))
            {
                TriggerGVSRoll(-MaxMiliAmpere);
            }
            if (Input.GetKeyUp(KeyCode.Alpha7))
            {
                TriggerGVSYaw(MaxMiliAmpere);
            }
            if (Input.GetKeyUp(KeyCode.Alpha8))
            {
                TriggerGVSYaw(-MaxMiliAmpere);
            }

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
    private void ToggleSerialPortState()
    {
        if (!isPortConnected)
        {
            UnitySerialPort.Instance.OpenSerialPort();
            isPortConnected = true;
        }
        else
        {
            UnitySerialPort.Instance.CloseSerialPort();
            isPortConnected = false;
        }
    }
    public bool IsPortConnected()
    {
        return isPortConnected;
    }

    //GVS Mode
    public void SetGVSMode(EGVSMode mode)
    {
        GVSmode = mode;
    }

    private bool isRotating = false;
    private bool rotatingLeft = false;
    private bool rotatingRight = false;
    private bool keepRotation = false;
    private void HandleRotation(Vector3 vector, AccelerationTypes types)
    {
        if (isSham || isNoisy)
        {
            return;
        }
        //If not sending, but rotating is true, set all electrodes to 0 once
        if (!rotationIsSending)
        {
            if (isRotating)
            {
                ZeroAllElectrodes();
                isRotating = false;
            }
            return;
        }
        //Check if camera is rotating
        if (vector.y == 0)
        {
            keepRotation = false;
        }
        if (vector.y != 0)
        {
            keepRotation = true;
        }
        // If rotating and direction changed, send signal
        if(isRotating)
        {
            if (rotatingRight && vector.y > 0 ||
                rotatingLeft && vector.y < 0) return; 
            rotatingLeft = vector.y < 0;
            rotatingRight = vector.y > 0;
            timer = 1;
            float currentInMa = vector.y * MaxMiliAmpere;
            currentInMa = Mathf.Clamp(currentInMa, -MaxMiliAmpere, MaxMiliAmpere);
            //TriggerGVSYaw(currentInMa);
            TriggerGVSYawAndLateral(lastLinear, currentInMa);
        }
        isRotating = keepRotation;

    }

    private bool isAccelerating = false;
    private bool acceleratingForward = false;
    private bool acceleratingBackward = false;
    private bool keepAcceleration = false;
    private void HandleAcceleration(Vector3 direction, AccelerationTypes type)
    {
        if (isSham || isNoisy)
        {
            return;
        }
            // If not sending, but accelerating is true, set all electrodes to 0 once
            if (!accIsSending)
        {
            if (isAccelerating)
            {
                ZeroAllElectrodes();
                isAccelerating = false;
            }
            return;
        }

        // Check if acceleration is active (non-zero direction)
        if (direction.z == 0)
        {
            keepAcceleration = false;
        }
        else
        {
            keepAcceleration = true;
        }

        // If accelerating and direction changed, send signal
        if (isAccelerating)
        {
            if ((acceleratingForward && direction.z > 0) ||
                (acceleratingBackward && direction.z < 0))
                return;

            acceleratingForward = direction.z > 0;
            acceleratingBackward = direction.z < 0;
            timer = 1;
            float currentInMa = direction.z * MaxMiliAmpere;
            currentInMa = Mathf.Clamp(currentInMa, -MaxMiliAmpere, MaxMiliAmpere);
            //TriggerGVSLateral(currentInMa);
            TriggerGVSYawAndLateral(currentInMa, lastAngular);
        }

        isAccelerating = keepAcceleration;
    }

    private float lastAngular, lastLinear = 0;

    public NoisyGVS NoisyGVS { get => noisyGVS; set => noisyGVS = value; }
    public bool IsManual { get => isManual; set => isManual = value; }
    public bool LogEvents { get => logEvents; set => logEvents = value; }
    public bool IsTesting { get => isTesting; set => isTesting = value; }
    public float MaxMiliAmpere { get => maxMiliAmpere; set => maxMiliAmpere = value; }

    public float NormalizeValues(float sum)
    {
        float absSum = Mathf.Abs(sum);
        if (absSum > 2.54f)
        {
            float scaleFactor = 2.54f / absSum;
            sum *= scaleFactor;
        }
        return sum;
    }

    private IEnumerator ZeroAllAfterTime()
    {
        yield return new WaitUntil(() => isPortConnected);
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            timer -= 0.1f;
            if(timer <= 0)
            {
                Debug.Log("Stopping all electrodes");
                ZeroAllElectrodes();
                timer = 1;
            }
        }
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
        gvsInfluencers[0].Subscribe(HandleAcceleration);
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
        foreach (string s in data)
        {
            messageBuffer.Enqueue(s);
        }

        // Process the buffer to extract complete messages
        ProcessBuffer();
    }

    private void ProcessBuffer()
    {
        List<string> bufferList = new List<string>(messageBuffer);
        if (bufferList.Count == 0)
            return;

        int startIndex = bufferList.IndexOf("AA");
        if (startIndex == -1)
        {
            // No start marker found, clear the buffer
            messageBuffer.Clear();
            return;
        }

        // Remove any elements before the start of the message
        while (startIndex > 0 && messageBuffer.Count > 0)
        {
            messageBuffer.Dequeue();
            startIndex--;
        }

        // Look for the end marker "55" after the start
        bufferList = new List<string>(messageBuffer);
        int endIndex = -1;
        for (int i = 1; i < bufferList.Count; i++) // Start from index 1 (after AA)
        {
            if (bufferList[i] == "55")
            {
                endIndex = i;
                break;
            }
        }

        if (endIndex == -1)
        {
            // No end marker found, wait for more data
            return;
        }

        // Extract the message from AA to 55 (inclusive)
        string[] messageData = new string[endIndex + 1];
        for (int i = 0; i <= endIndex; i++)
        {
            messageData[i] = bufferList[i];
        }

        // Process the complete message
        ProcessCompleteMessage(messageData);

        // Remove the processed message from the buffer
        for (int i = 0; i <= endIndex; i++)
        {
            if (messageBuffer.Count > 0)
                messageBuffer.Dequeue();
        }
    }

    private void ProcessCompleteMessage(string[] messageData)
    {
        if (messageData.Length < 4)
        {
            Debug.LogError("Invalid message: too short");
            return;
        }
        for (int i = 0; i < messageData.Length; i++)
            messageData[i] = messageData[i].ToLower();

        string head = messageData[0]; //Must be AA
        string lengthStr = messageData[1];
        string body = "";

        string checkedMessage = "";

        for (int i = 2; i < messageData.Length - 2; i++)
        {
            body += messageData[i];
            string partial = CheckMesage(messageData[i]);
            if (partial.Equals("Unknown Message"))
            {
                partial += ": " + messageData[i];
            }
            checkedMessage += partial + "\n";
            if (i < messageData.Length - 2)
                body += " ";
        }
        if(LogEvents) Debug.Log("Message from GVS: " + checkedMessage);

        string[] messageWithoutHead = body.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        int length;
        if (!int.TryParse(lengthStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out length))
        {
            Debug.LogError("Invalid length format: " + lengthStr);
            return;
        }

        //if (messageWithoutHead.Length > 1)
        //{
        //    if (!messageWithoutHead[1].Equals("0A")) return;
        //    Debug.Log("Response from setting Electrodes ");
        //    //string message = GetMessageAsString(messageWithoutHead, length);
        //}
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
    private bool SetModeDirect()
    {
        byte[] ModeCommandBytes = new byte[] { 0xAA, 0x01, 0x02, 0x02, 0x55 };
        return SendBytesToSerialPort(ModeCommandBytes);
    }

    private bool DeselectModeDirect()
    {
        byte[] ModeCommandBytes = new byte[] { 0xAA, 0x01, 0x03, 0x03, 0x55 };
        return SendBytesToSerialPort(ModeCommandBytes);
    }

    private bool SetElectrode(int nr, float CurrentInMilliampere)
    {
        if (nr <= 0 || nr >= 5) return false;
        if(Mathf.Abs(CurrentInMilliampere) > MaxMiliAmpere) return false;
        //Convert mA in Bytes to send to device (see GoodVibrations GVS - Device Manual)
        int CurrentInByte = (int)((CurrentInMilliampere + 2.56f) /0.02f);
        int checkSum = (9 + nr + CurrentInByte) % 256;

        byte[] SetElectrodeBytes = new byte[] { 0xAA, 0x03, 0x09, (byte)nr, (byte)CurrentInByte, (byte)checkSum, 0x55 };
        return SendBytesToSerialPort(SetElectrodeBytes);
    }
    private bool SetAllElectrodes(float CurrentInMilliampereElectrode1, float CurrentInMilliampereElectrode2, float CurrentInMilliampereElectrode3, float CurrentInMilliampereElectrode4)
    {
        if (Mathf.Abs(CurrentInMilliampereElectrode1) > 2.54) return false;
        if (Mathf.Abs(CurrentInMilliampereElectrode2) > 2.54) return false;
        if (Mathf.Abs(CurrentInMilliampereElectrode3) > 2.54) return false;
        if (Mathf.Abs(CurrentInMilliampereElectrode4) > 2.54) return false;
        int CurrentInByte1 = (int)((CurrentInMilliampereElectrode1 + 2.56f) / 0.02f);
        int CurrentInByte2 = (int)((CurrentInMilliampereElectrode2 + 2.56f) / 0.02f);
        int CurrentInByte3 = (int)((CurrentInMilliampereElectrode3 + 2.56f) / 0.02f);
        int CurrentInByte4 = (int)((CurrentInMilliampereElectrode4 + 2.56f) / 0.02f);

        int checkSum = (10 + CurrentInByte1 + CurrentInByte2 + CurrentInByte3 + CurrentInByte4) % 256;

        //Is checksum also sent? In the cpp code, only 0x0a is sent, but in the Vestibulator host, a checksum is recieved
        byte[] SetAllBytes = new byte[] { 0xaa, 0x05, 0x0a, (byte)CurrentInByte1, (byte)CurrentInByte2, (byte)CurrentInByte3, (byte)CurrentInByte4, (byte)checkSum, 0x55 };
        return SendBytesToSerialPort(SetAllBytes);
    }
    public bool Calibrate()
    {
        byte[] CalibrateBytes = new byte[] { 0xaa, 0x01, 0x1d, 0x1d, 0x55 };
        return SendBytesToSerialPort(CalibrateBytes);
    }
    // Stops running scrips, zeros all electrodes
    public bool KillSwitch()
    {
        ZeroAllElectrodes();
        byte[] StopBytes = new byte[] { 0xaa, 0x01, 0x14, 0x14, 0x55 };
        return SendBytesToSerialPort(StopBytes);
    }
    public void ZeroAllElectrodes()
    {
        SetAllElectrodes(0, 0, 0, 0);
    }

    public void TriggerGVSLateral(float CurrentInmA)
    {
        if (!isPortConnected) return;
        if(GVSmode == EGVSMode.BILATERAL_UNIPOLAR || GVSmode == EGVSMode.ODAS)
        {
            SetElectrode(1, CurrentInmA);
            SetElectrode(2, CurrentInmA);
        }
    }
    public void TriggerGVSRoll(float CurrentInmA)
    {
        if (!isPortConnected) return;

        switch (GVSmode)
        {
            case EGVSMode.BILATERAL_BIPOLAR:
                SetElectrode(1, CurrentInmA);
                break;
            case EGVSMode.ODAS:
                SetElectrode(1, CurrentInmA / 2);
                SetElectrode(2, -CurrentInmA / 2);
                break;
            case EGVSMode.OVR:
                SetElectrode(4, CurrentInmA / 2);
                SetElectrode(3, -CurrentInmA / 2);
                break;
            default:
                break;
        }
    }

    public void TriggerGVSPitch(float CurrentInmA)
    {
        if (!isPortConnected) return;

        switch (GVSmode)
        {
            case EGVSMode.BILATERAL_BIPOLAR:
                SetElectrode(1, CurrentInmA);
                SetElectrode(2, CurrentInmA);
                break;
            case EGVSMode.ODAS:
                SetElectrode(1, CurrentInmA / 2);
                SetElectrode(2, CurrentInmA / 2);
                SetElectrode(3, -CurrentInmA / 2);
                SetElectrode(4, -CurrentInmA / 2);
                break;
            case EGVSMode.OVR:
                SetElectrode(3, CurrentInmA / 2);
                SetElectrode(2, -CurrentInmA / 2);
                break;
            default:
                break;
        }
    }
    public void TriggerGVSYaw(float CurrentInmA)
    {
        if (!isPortConnected) return;

        switch (GVSmode)
        {
            case EGVSMode.ODAS:
                SetElectrode(1, CurrentInmA / 2);
                SetElectrode(2, -CurrentInmA / 2);
                SetElectrode(3, CurrentInmA / 2);
                SetElectrode(4, -CurrentInmA / 2);
                break;
            case EGVSMode.OVR:
                SetElectrode(1, CurrentInmA / 2);
                SetElectrode(3, -CurrentInmA / 2);
                break;
            default:
                break;
        }
    }

    private void TriggerGVSYawAndLateral(float currentAngular, float currentLinear)
    {
        //Yaw, electrodes 1-4, current/2
        float yawCurrent = currentAngular / 2;
        //Lateral, electrodes 1-2 current*1
        float lateralCurrent = currentLinear;

        float electrode1 = NormalizeValues(lateralCurrent + yawCurrent);
        float electrode2 = NormalizeValues(lateralCurrent - yawCurrent);
        float electrode3 = yawCurrent;
        float electrode4 = yawCurrent * -1;

        //If values were updated, resend a new signal
        if (currentAngular != lastAngular || currentLinear != lastLinear)
        {
            lastAngular = currentAngular;
            lastLinear = currentLinear;

            SetElectrode(1, electrode1);
            SetElectrode(2, electrode2);
            SetElectrode(3, electrode3);
            SetElectrode(4, electrode4);
        }
    }

    private void SetupNoisyGVS()
    {
        if (isNoisy)
        {
            NoisyGVS.SetMaxValue(MaxMiliAmpere);
            StartCoroutine(SendNoisySignal());
        }
    }
    private void TriggerNoisyGVS()
    {
        if (isNoisy)
        {
            float[] values = NoisyGVS.GetNextCurrents();
            if (!IsTesting && !IsManual)
            {
                SetElectrode(1, values[0]);
                SetElectrode(2, values[1]);
                SetElectrode(3, values[2]);
                SetElectrode(4, values[3]);
            }
        }
    }
    private IEnumerator SendNoisySignal()
    {
        yield return new WaitUntil(() => ((isPortConnected && !IsManual) || IsTesting));
        while (true)
        {
            if (!isNoisy || isManual)
                ZeroAllElectrodes();
            yield return new WaitUntil(() => (isNoisy && !isManual));
            yield return new WaitForSecondsRealtime(0.1f);
            TriggerNoisyGVS();
        }
    }
}