namespace ModbusTCPServer
{
    internal enum ModbusCommand 
    {
        None = 0,
        Read = 3,
        ResponseException = 3,
        Response = 7,
        StartWriteIndex = 14,
        Write = 16,
        ErrorUnknownCommand = 130,
        InvalidData = 131,
        OverflowData = 132
    }
}
