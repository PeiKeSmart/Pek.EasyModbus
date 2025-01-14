using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Pek.EasyModbus;

public class ModbusClient
{
    private Boolean debug;
    private TcpClient? tcpClient;
    private String ipAddress = "127.0.0.1";
    private Int32 port = 502;
    private UInt32 transactionIdentifierInternal;
    private Byte[] transactionIdentifier = new Byte[2];
    private Byte[] protocolIdentifier = new Byte[2];
    private Byte[] crc = new Byte[2];
    private Byte[] length = new Byte[2];
    private Byte unitIdentifier = 1;
    private Byte functionCode;
    private Byte[] startingAddress = new Byte[2];
    private Byte[] quantity = new Byte[2];
    private Boolean udpFlag;
    private Int32 portOut;
    private Int32 connectTimeout = 1000;
    public Byte[]? receiveData;
    public Byte[]? sendData;
    private NetworkStream? stream;

    public Int32 NumberOfRetries { get; set; } = 3;

    public event ReceiveDataChangedHandler? ReceiveDataChanged;

    public event SendDataChangedHandler? SendDataChanged;

    public event ConnectedChangedHandler? ConnectedChanged;

    public ModbusClient(String ipAddress, Int32 port)
    {
        if (debug)
            StoreLogData.Instance.Store("EasyModbus library initialized for Modbus-TCP, IPAddress: " + ipAddress + ", Port: " + port.ToString(), DateTime.Now);
        this.ipAddress = ipAddress;
        this.port = port;
    }

    public ModbusClient()
    {
        if (!debug)
            return;
        StoreLogData.Instance.Store("EasyModbus library initialized for Modbus-TCP", DateTime.Now);
    }

    public void Connect()
    {
        if (!udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("Open TCP-Socket, IP-Address: " + ipAddress + ", Port: " + port.ToString(), DateTime.Now);
            tcpClient = new TcpClient();
            var asyncResult = tcpClient.BeginConnect(ipAddress, port, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(connectTimeout))
                throw new ConnectionException("connection timed out");
            tcpClient.EndConnect(asyncResult);
            stream = tcpClient.GetStream();
            stream.ReadTimeout = connectTimeout;
        }
        else
            tcpClient = new TcpClient();
        if (ConnectedChanged == null)
            return;
        try
        {
            ConnectedChanged((Object)this);
        }
        catch
        {
        }
    }

    public void Connect(String ipAddress, Int32 port)
    {
        if (!udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("Open TCP-Socket, IP-Address: " + ipAddress + ", Port: " + port.ToString(), DateTime.Now);
            tcpClient = new TcpClient();
            var asyncResult = tcpClient.BeginConnect(ipAddress, port, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(connectTimeout))
                throw new ConnectionException("connection timed out");
            tcpClient.EndConnect(asyncResult);
            stream = tcpClient.GetStream();
            stream.ReadTimeout = connectTimeout;
        }
        else
            tcpClient = new TcpClient();
        if (ConnectedChanged == null)
            return;
        ConnectedChanged(this);
    }

    public static Single ConvertRegistersToFloat(Int32[] registers)
    {
        var num = registers.Length == 2 ? registers[1] : throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
        var register = registers[0];
        var bytes1 = BitConverter.GetBytes(num);
        var bytes2 = BitConverter.GetBytes(register);
        return BitConverter.ToSingle(
        [
        bytes2[0],
        bytes2[1],
        bytes1[0],
        bytes1[1]
        ], 0);
    }

    public static Single ConvertRegistersToFloat(
      Int32[] registers,
      RegisterOrder registerOrder)
    {
        Int32[] registers1 =
        [
        registers[0],
        registers[1]
        ];
        if (registerOrder == RegisterOrder.HighLow)
            registers1 =
            [
          registers[1],
          registers[0]
            ];
        return ConvertRegistersToFloat(registers1);
    }

    public static Int32 ConvertRegistersToInt(Int32[] registers)
    {
        var num = registers.Length == 2 ? registers[1] : throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
        var register = registers[0];
        var bytes1 = BitConverter.GetBytes(num);
        var bytes2 = BitConverter.GetBytes(register);
        return BitConverter.ToInt32(
        [
        bytes2[0],
        bytes2[1],
        bytes1[0],
        bytes1[1]
        ], 0);
    }

    public static Int32 ConvertRegistersToInt(
      Int32[] registers,
      RegisterOrder registerOrder)
    {
        Int32[] registers1 =
        [
        registers[0],
        registers[1]
        ];
        if (registerOrder == RegisterOrder.HighLow)
            registers1 =
            [
          registers[1],
          registers[0]
            ];
        return ConvertRegistersToInt(registers1);
    }

    public static Int64 ConvertRegistersToLong(Int32[] registers)
    {
        var num = registers.Length == 4 ? registers[3] : throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        var register1 = registers[2];
        var register2 = registers[1];
        var register3 = registers[0];
        var bytes1 = BitConverter.GetBytes(num);
        var bytes2 = BitConverter.GetBytes(register1);
        var bytes3 = BitConverter.GetBytes(register2);
        var bytes4 = BitConverter.GetBytes(register3);
        return BitConverter.ToInt64(
        [
        bytes4[0],
        bytes4[1],
        bytes3[0],
        bytes3[1],
        bytes2[0],
        bytes2[1],
        bytes1[0],
        bytes1[1]
        ], 0);
    }

    public static Int64 ConvertRegistersToLong(
      Int32[] registers,
      RegisterOrder registerOrder)
    {
        Int32[] registers1 = registers.Length == 4 ?
        [
        registers[0],
        registers[1],
        registers[2],
        registers[3]
        ] : throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        if (registerOrder == RegisterOrder.HighLow)
            registers1 =
            [
          registers[3],
          registers[2],
          registers[1],
          registers[0]
            ];
        return ConvertRegistersToLong(registers1);
    }

    public static Double ConvertRegistersToDouble(Int32[] registers)
    {
        var num = registers.Length == 4 ? registers[3] : throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        var register1 = registers[2];
        var register2 = registers[1];
        var register3 = registers[0];
        var bytes1 = BitConverter.GetBytes(num);
        var bytes2 = BitConverter.GetBytes(register1);
        var bytes3 = BitConverter.GetBytes(register2);
        var bytes4 = BitConverter.GetBytes(register3);
        return BitConverter.ToDouble(
        [
        bytes4[0],
        bytes4[1],
        bytes3[0],
        bytes3[1],
        bytes2[0],
        bytes2[1],
        bytes1[0],
        bytes1[1]
        ], 0);
    }

    public static Double ConvertRegistersToDouble(
      Int32[] registers,
      RegisterOrder registerOrder)
    {
        Int32[] registers1 = registers.Length == 4 ?
        [
        registers[0],
        registers[1],
        registers[2],
        registers[3]
        ] : throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
        if (registerOrder == RegisterOrder.HighLow)
            registers1 =
            [
          registers[3],
          registers[2],
          registers[1],
          registers[0]
            ];
        return ConvertRegistersToDouble(registers1);
    }

    public static Int32[] ConvertFloatToRegisters(Single floatValue)
    {
        var bytes = BitConverter.GetBytes(floatValue);
        Byte[] numArray =
        [
        bytes[2],
        bytes[3],
         0,
         0
        ];
        return
        [
        BitConverter.ToInt32(
        [
          bytes[0],
          bytes[1],
           0,
           0
        ], 0),
        BitConverter.ToInt32(numArray, 0)
        ];
    }

    public static Int32[] ConvertFloatToRegisters(
      Single floatValue,
      RegisterOrder registerOrder)
    {
        var registers1 = ConvertFloatToRegisters(floatValue);
        var registers2 = registers1;
        if (registerOrder == RegisterOrder.HighLow)
            registers2 =
            [
          registers1[1],
          registers1[0]
            ];
        return registers2;
    }

    public static Int32[] ConvertIntToRegisters(Int32 intValue)
    {
        var bytes = BitConverter.GetBytes(intValue);
        Byte[] numArray =
        [
        bytes[2],
        bytes[3],
         0,
         0
        ];
        return
        [
        BitConverter.ToInt32(
        [
          bytes[0],
          bytes[1],
           0,
           0
        ], 0),
        BitConverter.ToInt32(numArray, 0)
        ];
    }

    public static Int32[] ConvertIntToRegisters(
      Int32 intValue,
      RegisterOrder registerOrder)
    {
        var registers1 = ConvertIntToRegisters(intValue);
        var registers2 = registers1;
        if (registerOrder == RegisterOrder.HighLow)
            registers2 =
            [
          registers1[1],
          registers1[0]
            ];
        return registers2;
    }

    public static Int32[] ConvertLongToRegisters(Int64 longValue)
    {
        var bytes = BitConverter.GetBytes(longValue);
        Byte[] numArray1 =
        [
        bytes[6],
        bytes[7],
         0,
         0
        ];
        Byte[] numArray2 =
        [
        bytes[4],
        bytes[5],
         0,
         0
        ];
        Byte[] numArray3 =
        [
        bytes[2],
        bytes[3],
        0,
        0
        ];
        return
        [
        BitConverter.ToInt32(
        [
          bytes[0],
          bytes[1],
           0,
           0
        ], 0),
        BitConverter.ToInt32(numArray3, 0),
        BitConverter.ToInt32(numArray2, 0),
        BitConverter.ToInt32(numArray1, 0)
        ];
    }

    public static Int32[] ConvertLongToRegisters(
      Int64 longValue,
      RegisterOrder registerOrder)
    {
        var registers1 = ConvertLongToRegisters(longValue);
        var registers2 = registers1;
        if (registerOrder == RegisterOrder.HighLow)
            registers2 =
            [
          registers1[3],
          registers1[2],
          registers1[1],
          registers1[0]
            ];
        return registers2;
    }

    public static Int32[] ConvertDoubleToRegisters(Double doubleValue)
    {
        var bytes = BitConverter.GetBytes(doubleValue);
        Byte[] numArray1 =
        [
        bytes[6],
        bytes[7],
         0,
         0
        ];
        Byte[] numArray2 =
        [
        bytes[4],
        bytes[5],
         0,
         0
        ];
        Byte[] numArray3 =
        [
        bytes[2],
        bytes[3],
         0,
         0
        ];
        return
        [
        BitConverter.ToInt32(
        [
          bytes[0],
          bytes[1],
           0,
           0
        ], 0),
        BitConverter.ToInt32(numArray3, 0),
        BitConverter.ToInt32(numArray2, 0),
        BitConverter.ToInt32(numArray1, 0)
        ];
    }

    public static Int32[] ConvertDoubleToRegisters(
      Double doubleValue,
      RegisterOrder registerOrder)
    {
        var registers1 = ConvertDoubleToRegisters(doubleValue);
        var registers2 = registers1;
        if (registerOrder == RegisterOrder.HighLow)
            registers2 =
            [
          registers1[3],
          registers1[2],
          registers1[1],
          registers1[0]
            ];
        return registers2;
    }

    public static String ConvertRegistersToString(Int32[] registers, Int32 offset, Int32 stringLength)
    {
        var bytes1 = new Byte[stringLength];
        for (var index = 0; index < stringLength / 2; ++index)
        {
            var bytes2 = BitConverter.GetBytes(registers[offset + index]);
            bytes1[index * 2] = bytes2[0];
            bytes1[index * 2 + 1] = bytes2[1];
        }
        return Encoding.Default.GetString(bytes1);
    }

    public static Int32[] ConvertStringToRegisters(String stringToConvert)
    {
        var bytes = Encoding.ASCII.GetBytes(stringToConvert);
        var registers = new Int32[stringToConvert.Length / 2 + stringToConvert.Length % 2];
        for (var index = 0; index < registers.Length; ++index)
        {
            registers[index] = bytes[index * 2];
            if (index * 2 + 1 < bytes.Length)
                registers[index] = registers[index] | bytes[index * 2 + 1] << 8;
        }
        return registers;
    }

    public static UInt16 CalculateCRC(Byte[] data, UInt16 numberOfBytes, Int32 startByte)
    {
        Byte[] numArray1 =
        [
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
         64,
         0,
         193,
         129,
         64,
         1,
         192,
         128,
         65,
         0,
         193,
         129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64,
        1,
        192,
        128,
        65,
        1,
        192,
        128,
        65,
        0,
        193,
        129,
        64
        ];
        Byte[] numArray2 =
        [
        0,
        192,
        193,
        1,
        195,
        3,
        2,
        194,
        198,
        6,
        7,
        199,
        5,
        197,
        196,
        4,
        204,
        12,
        13,
        205,
        15,
        207,
        206,
        14,
        10,
        202,
        203,
        11,
        201,
        9,
        8,
        200,
        216,
        24,
        25,
        217,
        27,
        219,
        218,
        26,
        30,
        222,
        223,
        31,
        221,
        29,
        28,
        220,
        20,
        212,
        213,
        21,
        215,
        23,
        22,
        214,
        210,
        18,
        19,
        211,
        17,
        209,
        208,
        16,
        240,
        48,
        49,
        241,
        51,
        243,
        242,
        50,
        54,
        246,
        247,
        55,
        245,
        53,
        52,
        244,
        60,
        252,
        253,
        61,
        Byte.MaxValue,
        63,
        62,
        254,
        250,
        58,
        59,
        251,
        57,
        249,
        248,
        56,
        40,
        232,
        233,
        41,
        235,
        43,
        42,
        234,
        238,
        46,
        47,
        239,
        45,
        237,
        236,
        44,
        228,
        36,
        37,
        229,
        39,
        231,
        230,
        38,
        34,
        226,
        227,
        35,
        225,
        33,
        32,
        224,
        160,
        96,
        97,
        161,
        99,
        163,
        162,
        98,
        102,
        166,
        167,
        103,
        165,
        101,
        100,
        164,
        108,
        172,
        173,
        109,
        175,
        111,
        110,
        174,
        170,
        106,
        107,
        171,
        105,
        169,
        168,
        104,
        120,
        184,
        185,
        121,
        187,
        123,
        122,
        186,
        190,
        126,
        127,
        191,
        125,
        189,
        188,
        124,
        180,
        116,
        117,
        181,
        119,
        183,
        182,
        118,
        114,
        178,
        179,
        115,
        177,
        113,
        112,
        176,
        80,
        144,
        145,
        81,
        147,
        83,
        82,
        146,
        150,
        86,
        87,
        151,
        85,
        149,
        148,
        84,
        156,
        92,
        93,
        157,
        95,
        159,
        158,
        94,
        90,
        154,
        155,
        91,
        153,
        89,
        88,
        152,
        136,
        72,
        73,
        137,
        75,
        139,
        138,
        74,
        78,
        142,
        143,
        79,
        141,
        77,
        76,
        140,
        68,
        132,
        133,
        69,
        135,
        71,
        70,
        134,
        130,
        66,
        67,
        131,
        65,
        129,
        128,
        64
        ];
        var num1 = numberOfBytes;
        var maxValue = Byte.MaxValue;
        var num2 = Byte.MaxValue;
        var num3 = 0;
        while (num1 > 0)
        {
            --num1;
            if (num3 + startByte < data.Length)
            {
                var index = num2 ^ data[num3 + startByte];
                num2 = (Byte)(maxValue ^ (UInt32)numArray1[index]);
                maxValue = numArray2[index];
            }
            ++num3;
        }
        return (UInt16)((UInt32)maxValue << 8 | num2);
    }

    public static Boolean DetectValidModbusFrame(Byte[] readBuffer, Int32 length)
    {
        if (length < 6 || readBuffer[0] < 1 | readBuffer[0] > 247)
            return false;
        var bytes = BitConverter.GetBytes(CalculateCRC(readBuffer, (UInt16)(length - 2), 0));
        return !(bytes[0] != (Int32)readBuffer[length - 2] | bytes[1] != readBuffer[length - 1]);
    }

    public Boolean[] ReadDiscreteInputs(Int32 startingAddress, Int32 quantity)
    {
        if (debug)
            StoreLogData.Instance.Store("FC2 (Read Discrete Inputs from Master device), StartingAddress: " + startingAddress.ToString() + ", Quantity: " + quantity.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        if (startingAddress > UInt16.MaxValue | quantity > 2000)
        {
            if (debug)
                StoreLogData.Instance.Store("ArgumentException Throwed", DateTime.Now);
            throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 2;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        this.quantity = BitConverter.GetBytes(quantity);
        Byte[] numArray1 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        this.quantity[1],
        this.quantity[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, 6, 6));
        numArray1[12] = crc[0];
        numArray1[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[2100];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 130 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 130 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 130 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 130 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
            var flagArray = new Boolean[quantity];
            for (var index = 0; index < quantity; ++index)
            {
                var num = numArray2[9 + index / 8];
                var int32 = Convert.ToInt32(Math.Pow(2.0, index % 8));
                flagArray[index] = Convert.ToBoolean((num & int32) / int32);
            }
            return flagArray;
        }
        if (debug)
            StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
        throw new ConnectionException("connection error");
    }

    public Boolean[] ReadCoils(Int32 startingAddress, Int32 quantity)
    {
        if (debug)
            StoreLogData.Instance.Store("FC1 (Read Coils from Master device), StartingAddress: " + startingAddress.ToString() + ", Quantity: " + quantity.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        if (startingAddress > UInt16.MaxValue | quantity > 2000)
        {
            if (debug)
                StoreLogData.Instance.Store("ArgumentException Throwed", DateTime.Now);
            throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 1;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        this.quantity = BitConverter.GetBytes(quantity);
        Byte[] numArray1 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        this.quantity[1],
        this.quantity[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, 6, 6));
        numArray1[12] = crc[0];
        numArray1[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send MocbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[2100];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 129 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 129 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 129 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 129 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
            var flagArray = new Boolean[quantity];
            for (var index = 0; index < quantity; ++index)
            {
                var num = numArray2[9 + index / 8];
                var int32 = Convert.ToInt32(Math.Pow(2.0, index % 8));
                flagArray[index] = Convert.ToBoolean((num & int32) / int32);
            }
            return flagArray;
        }
        if (debug)
            StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
        throw new ConnectionException("connection error");
    }

    public Int32[] ReadHoldingRegisters(Int32 startingAddress, Int32 quantity)
    {
        if (debug)
            StoreLogData.Instance.Store("FC3 (Read Holding Registers from Master device), StartingAddress: " + startingAddress.ToString() + ", Quantity: " + quantity.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        if (startingAddress > UInt16.MaxValue | quantity > 125)
        {
            if (debug)
                StoreLogData.Instance.Store("ArgumentException Throwed", DateTime.Now);
            throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 3;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        this.quantity = BitConverter.GetBytes(quantity);
        Byte[] numArray1 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        this.quantity[1],
        this.quantity[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, 6, 6));
        numArray1[12] = crc[0];
        numArray1[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[256];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 131 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 131 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 131 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 131 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
            var numArray3 = new Int32[quantity];
            for (var index = 0; index < quantity; ++index)
            {
                var num1 = numArray2[9 + index * 2];
                var num2 = numArray2[9 + index * 2 + 1];
                numArray2[9 + index * 2] = num2;
                numArray2[9 + index * 2 + 1] = num1;
                numArray3[index] = BitConverter.ToInt16(numArray2, 9 + index * 2);
            }
            return numArray3;
        }
        if (debug)
            StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
        throw new ConnectionException("connection error");
    }

    public Int32[] ReadInputRegisters(Int32 startingAddress, Int32 quantity)
    {
        if (debug)
            StoreLogData.Instance.Store("FC4 (Read Input Registers from Master device), StartingAddress: " + startingAddress.ToString() + ", Quantity: " + quantity.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        if (startingAddress > UInt16.MaxValue | quantity > 125)
        {
            if (debug)
                StoreLogData.Instance.Store("ArgumentException Throwed", DateTime.Now);
            throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 4;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        this.quantity = BitConverter.GetBytes(quantity);
        Byte[] numArray1 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        this.quantity[1],
        this.quantity[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, 6, 6));
        numArray1[12] = crc[0];
        numArray1[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[2100];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 132 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 132 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 132 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 132 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
            var numArray3 = new Int32[quantity];
            for (var index = 0; index < quantity; ++index)
            {
                var num1 = numArray2[9 + index * 2];
                var num2 = numArray2[9 + index * 2 + 1];
                numArray2[9 + index * 2] = num2;
                numArray2[9 + index * 2 + 1] = num1;
                numArray3[index] = BitConverter.ToInt16(numArray2, 9 + index * 2);
            }
            return numArray3;
        }
        if (debug)
            StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
        throw new ConnectionException("connection error");
    }

    public void WriteSingleCoil(Int32 startingAddress, Boolean value)
    {
        if (debug)
            StoreLogData.Instance.Store("FC5 (Write single coil to Master device), StartingAddress: " + startingAddress.ToString() + ", Value: " + value.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        var numArray1 = new Byte[2];
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 5;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        var numArray2 = !value ? BitConverter.GetBytes(0) : BitConverter.GetBytes(65280);
        Byte[] numArray3 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        numArray2[1],
        numArray2[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray3, 6, 6));
        numArray3[12] = crc[0];
        numArray3[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray4;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray3, numArray3.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray4 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray3, 0, numArray3.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray3.Length - 2];
                    Array.Copy(numArray3, 0, destinationArray, 0, numArray3.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray3.Length - 2];
                    Array.Copy(numArray3, 0, sendData, 0, numArray3.Length - 2);
                    SendDataChanged(this);
                }
                numArray4 = new Byte[2100];
                var length = stream?.Read(numArray4, 0, numArray4.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray4, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray4[7] == 133 & numArray4[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray4[7] == 133 & numArray4[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray4[7] == 133 & numArray4[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray4[7] == 133 & numArray4[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
        }
        else
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
    }

    public void WriteSingleRegister(Int32 startingAddress, Int32 value)
    {
        if (debug)
            StoreLogData.Instance.Store("FC6 (Write single register to Master device), StartingAddress: " + startingAddress.ToString() + ", Value: " + value.ToString(), DateTime.Now);
        ++transactionIdentifierInternal;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        var numArray1 = new Byte[2];
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(6);
        functionCode = 6;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        var bytes = BitConverter.GetBytes(value);
        Byte[] numArray2 =
        [
        transactionIdentifier[1],
        transactionIdentifier[0],
        protocolIdentifier[1],
        protocolIdentifier[0],
        length[1],
        length[0],
        unitIdentifier,
        functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        bytes[1],
        bytes[0],
        crc[0],
        crc[1]
        ];
        crc = BitConverter.GetBytes(CalculateCRC(numArray2, 6, 6));
        numArray2[12] = crc[0];
        numArray2[13] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray3;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray2, numArray2.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray3 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray2, 0, numArray2.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray2.Length - 2];
                    Array.Copy(numArray2, 0, destinationArray, 0, numArray2.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray2.Length - 2];
                    Array.Copy(numArray2, 0, sendData, 0, numArray2.Length - 2);
                    SendDataChanged(this);
                }
                numArray3 = new Byte[2100];
                var length = stream?.Read(numArray3, 0, numArray3.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray3, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray3[7] == 134 & numArray3[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray3[7] == 134 & numArray3[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray3[7] == 134 & numArray3[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray3[7] == 134 & numArray3[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
        }
        else
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
    }

    public void WriteMultipleCoils(Int32 startingAddress, Boolean[] values)
    {
        var str = "";
        for (var index = 0; index < values.Length; ++index)
            str = str + values[index].ToString() + " ";
        if (debug)
            StoreLogData.Instance.Store("FC15 (Write multiple coils to Master device), StartingAddress: " + startingAddress.ToString() + ", Values: " + str, DateTime.Now);
        ++transactionIdentifierInternal;
        var num1 = values.Length % 8 != 0 ? (Byte)(values.Length / 8 + 1) : (Byte)(values.Length / 8);
        var bytes = BitConverter.GetBytes(values.Length);
        Byte num2 = 0;
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(7 + num1);
        functionCode = 15;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        var numArray1 = new Byte[16 + (values.Length % 8 != 0 ? values.Length / 8 : values.Length / 8 - 1)];
        numArray1[0] = transactionIdentifier[1];
        numArray1[1] = transactionIdentifier[0];
        numArray1[2] = protocolIdentifier[1];
        numArray1[3] = protocolIdentifier[0];
        numArray1[4] = length[1];
        numArray1[5] = length[0];
        numArray1[6] = unitIdentifier;
        numArray1[7] = functionCode;
        numArray1[8] = this.startingAddress[1];
        numArray1[9] = this.startingAddress[0];
        numArray1[10] = bytes[1];
        numArray1[11] = bytes[0];
        numArray1[12] = num1;
        for (var index = 0; index < values.Length; ++index)
        {
            if (index % 8 == 0)
                num2 = 0;
            num2 = (Byte)((!values[index] ? 0U : 1U) << index % 8 | num2);
            numArray1[13 + index / 8] = num2;
        }
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, (UInt16)(numArray1.Length - 8), 6));
        numArray1[^2] = crc[0];
        numArray1[^1] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[2100];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 143 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 143 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 143 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 143 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
        }
        else
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
    }

    public void WriteMultipleRegisters(Int32 startingAddress, Int32[] values)
    {
        var str = "";
        for (var index = 0; index < values.Length; ++index)
            str = str + values[index].ToString() + " ";
        if (debug)
            StoreLogData.Instance.Store("FC16 (Write multiple Registers to Server device), StartingAddress: " + startingAddress.ToString() + ", Values: " + str, DateTime.Now);
        ++transactionIdentifierInternal;
        var num = (Byte)(values.Length * 2);
        var bytes1 = BitConverter.GetBytes(values.Length);
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(7 + values.Length * 2);
        functionCode = 16;
        this.startingAddress = BitConverter.GetBytes(startingAddress);
        var numArray1 = new Byte[15 + values.Length * 2];
        numArray1[0] = transactionIdentifier[1];
        numArray1[1] = transactionIdentifier[0];
        numArray1[2] = protocolIdentifier[1];
        numArray1[3] = protocolIdentifier[0];
        numArray1[4] = length[1];
        numArray1[5] = length[0];
        numArray1[6] = unitIdentifier;
        numArray1[7] = functionCode;
        numArray1[8] = this.startingAddress[1];
        numArray1[9] = this.startingAddress[0];
        numArray1[10] = bytes1[1];
        numArray1[11] = bytes1[0];
        numArray1[12] = num;
        for (var index = 0; index < values.Length; ++index)
        {
            var bytes2 = BitConverter.GetBytes(values[index]);
            numArray1[13 + index * 2] = bytes2[1];
            numArray1[14 + index * 2] = bytes2[0];
        }
        crc = BitConverter.GetBytes(CalculateCRC(numArray1, (UInt16)(numArray1.Length - 8), 6));
        numArray1[^2] = crc[0];
        numArray1[^1] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray2;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray1, numArray1.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray2 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray1, 0, numArray1.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, destinationArray, 0, numArray1.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray1.Length - 2];
                    Array.Copy(numArray1, 0, sendData, 0, numArray1.Length - 2);
                    SendDataChanged(this);
                }
                numArray2 = new Byte[2100];
                var length = stream?.Read(numArray2, 0, numArray2.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray2, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray2[7] == 144 & numArray2[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray2[7] == 144 & numArray2[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray2[7] == 144 & numArray2[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray2[7] == 144 & numArray2[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
        }
        else
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
    }

    public Int32[] ReadWriteMultipleRegisters(
      Int32 startingAddressRead,
      Int32 quantityRead,
      Int32 startingAddressWrite,
      Int32[] values)
    {
        var str = "";
        for (var index = 0; index < values.Length; ++index)
            str = str + values[index].ToString() + " ";
        if (debug)
            StoreLogData.Instance.Store("FC23 (Read and Write multiple Registers to Server device), StartingAddress Read: " + startingAddressRead.ToString() + ", Quantity Read: " + quantityRead.ToString() + ", startingAddressWrite: " + startingAddressWrite.ToString() + ", Values: " + str, DateTime.Now);
        ++transactionIdentifierInternal;
        var numArray1 = new Byte[2];
        var numArray2 = new Byte[2];
        var numArray3 = new Byte[2];
        var numArray4 = new Byte[2];
        if (tcpClient == null & !udpFlag)
        {
            if (debug)
                StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
            throw new ConnectionException("connection error");
        }
        if (startingAddressRead > UInt16.MaxValue | quantityRead > 125 | startingAddressWrite > UInt16.MaxValue | values.Length > 121)
        {
            if (debug)
                StoreLogData.Instance.Store("ArgumentException Throwed", DateTime.Now);
            throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
        }
        transactionIdentifier = BitConverter.GetBytes(transactionIdentifierInternal);
        protocolIdentifier = BitConverter.GetBytes(0);
        length = BitConverter.GetBytes(11 + values.Length * 2);
        functionCode = 23;
        var bytes1 = BitConverter.GetBytes(startingAddressRead);
        var bytes2 = BitConverter.GetBytes(quantityRead);
        var bytes3 = BitConverter.GetBytes(startingAddressWrite);
        var bytes4 = BitConverter.GetBytes(values.Length);
        var num1 = Convert.ToByte(values.Length * 2);
        var numArray5 = new Byte[19 + values.Length * 2];
        numArray5[0] = transactionIdentifier[1];
        numArray5[1] = transactionIdentifier[0];
        numArray5[2] = protocolIdentifier[1];
        numArray5[3] = protocolIdentifier[0];
        numArray5[4] = length[1];
        numArray5[5] = length[0];
        numArray5[6] = unitIdentifier;
        numArray5[7] = functionCode;
        numArray5[8] = bytes1[1];
        numArray5[9] = bytes1[0];
        numArray5[10] = bytes2[1];
        numArray5[11] = bytes2[0];
        numArray5[12] = bytes3[1];
        numArray5[13] = bytes3[0];
        numArray5[14] = bytes4[1];
        numArray5[15] = bytes4[0];
        numArray5[16] = num1;
        for (var index = 0; index < values.Length; ++index)
        {
            var bytes5 = BitConverter.GetBytes(values[index]);
            numArray5[17 + index * 2] = bytes5[1];
            numArray5[18 + index * 2] = bytes5[0];
        }
        crc = BitConverter.GetBytes(CalculateCRC(numArray5, (UInt16)(numArray5.Length - 8), 6));
        numArray5[^2] = crc[0];
        numArray5[^1] = crc[1];
        if (tcpClient?.Client.Connected == true | udpFlag)
        {
            Byte[] numArray6;
            if (udpFlag)
            {
                var udpClient = new UdpClient();
                var endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                udpClient.Send(numArray5, numArray5.Length - 2, endPoint);
                portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
                udpClient.Client.ReceiveTimeout = 5000;
                var remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                numArray6 = udpClient.Receive(ref remoteEP);
            }
            else
            {
                stream?.Write(numArray5, 0, numArray5.Length - 2);
                if (debug)
                {
                    var destinationArray = new Byte[numArray5.Length - 2];
                    Array.Copy(numArray5, 0, destinationArray, 0, numArray5.Length - 2);
                    if (debug)
                        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(destinationArray), DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new Byte[numArray5.Length - 2];
                    Array.Copy(numArray5, 0, sendData, 0, numArray5.Length - 2);
                    SendDataChanged(this);
                }
                numArray6 = new Byte[2100];
                var length = stream?.Read(numArray6, 0, numArray6.Length) ?? 0;
                if (ReceiveDataChanged != null)
                {
                    receiveData = new Byte[length];
                    Array.Copy(numArray6, 0, receiveData, 0, length);
                    if (debug)
                        StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), DateTime.Now);
                    ReceiveDataChanged(this);
                }
            }
            if (numArray6[7] == 151 & numArray6[8] == 1)
            {
                if (debug)
                    StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", DateTime.Now);
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (numArray6[7] == 151 & numArray6[8] == 2)
            {
                if (debug)
                    StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", DateTime.Now);
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray6[7] == 151 & numArray6[8] == 3)
            {
                if (debug)
                    StoreLogData.Instance.Store("QuantityInvalidException Throwed", DateTime.Now);
                throw new QuantityInvalidException("quantity invalid");
            }
            if (numArray6[7] == 151 & numArray6[8] == 4)
            {
                if (debug)
                    StoreLogData.Instance.Store("ModbusException Throwed", DateTime.Now);
                throw new ModbusException("error reading");
            }
            var numArray7 = new Int32[quantityRead];
            for (var index = 0; index < quantityRead; ++index)
            {
                var num2 = numArray6[9 + index * 2];
                var num3 = numArray6[9 + index * 2 + 1];
                numArray6[9 + index * 2] = num3;
                numArray6[9 + index * 2 + 1] = num2;
                numArray7[index] = BitConverter.ToInt16(numArray6, 9 + index * 2);
            }
            return numArray7;
        }
        if (debug)
            StoreLogData.Instance.Store("ConnectionException Throwed", DateTime.Now);
        throw new ConnectionException("connection error");
    }

    public void Disconnect()
    {
        if (debug)
            StoreLogData.Instance.Store(nameof(Disconnect), DateTime.Now);
        stream?.Close();
        tcpClient?.Close();
        if (ConnectedChanged == null)
            return;
        ConnectedChanged(this);
    }

    ~ModbusClient()
    {
        if (debug)
            StoreLogData.Instance.Store("Destructor called - automatically disconnect", DateTime.Now);
        if (!(tcpClient != null & !udpFlag))
            return;
        stream?.Close();
        tcpClient?.Close();
    }

    public Boolean Connected
    {
        get
        {
            if (udpFlag & tcpClient != null)
                return true;
            return tcpClient != null && tcpClient.Connected;
        }
    }

    public Boolean Available(Int32 timeout)
    {
        var ping = new Ping();
        var ipAddress = System.Net.IPAddress.Parse(this.ipAddress);
        var bytes = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        var address = ipAddress;
        var timeout1 = timeout;
        var buffer = bytes;
        return ping.Send(address, timeout1, buffer).Status == IPStatus.Success;
    }

    public String IPAddress
    {
        get => ipAddress;
        set => ipAddress = value;
    }

    public Int32 Port
    {
        get => port;
        set => port = value;
    }

    public Boolean UDPFlag
    {
        get => udpFlag;
        set => udpFlag = value;
    }

    public Byte UnitIdentifier
    {
        get => unitIdentifier;
        set => unitIdentifier = value;
    }

    public Int32 ConnectionTimeout
    {
        get => connectTimeout;
        set => connectTimeout = value;
    }

    public String? LogFileFilename
    {
        get => StoreLogData.Instance.Filename;
        set
        {
            StoreLogData.Instance.Filename = value;
            if (StoreLogData.Instance.Filename != null)
                debug = true;
            else
                debug = false;
        }
    }

    public enum RegisterOrder
    {
        LowHigh,
        HighLow,
    }

    public delegate void ReceiveDataChangedHandler(Object sender);

    public delegate void SendDataChangedHandler(Object sender);

    public delegate void ConnectedChangedHandler(Object sender);
}