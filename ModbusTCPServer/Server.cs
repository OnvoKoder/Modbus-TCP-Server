using System.Net;
using System.Net.Sockets;

namespace ModbusTCPServer
{
    public class Server
    {
        private static string ipAddress = "";
        private static byte[] register = new byte [0];
        public EventHandler<string> RunningServer;
        public Server(string ip) => ipAddress = ip;
        public Task StartRun()
        {
            IPAddress IP = IPAddress.Parse(ipAddress);
            IPEndPoint endPoint = new IPEndPoint(IP, 502);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            socket.Listen(10);
            while (true)
            {
                try
                {
                    Socket listener = socket.Accept();
                    byte[] buffer = new byte[256];
                    int size = 0;
                    do
                    {
                        size = listener.Receive(buffer);
                        byte[] bufferMessage = buffer;
                        bufferMessage[5] = (byte)ModbusCommand.Response;
                        if (bufferMessage[7] == (byte)ModbusCommand.Read)
                        {
                            if (bufferMessage[9] <= register.Length && (bufferMessage[6] + (bufferMessage[11]/2) - 1) <= register.Length)
                            {
                                bufferMessage[8] = (byte)(2 * buffer[11]);
                                int quntity = bufferMessage[11] / 2;
                                bufferMessage[11] = (byte)ModbusCommand.None;
                                for (int i = bufferMessage[9], addressBuf = 9, counter = 0; counter < quntity; i++, addressBuf += 2, counter++)
                                   bufferMessage[addressBuf] = register[i];
                                OnRunningServer("Success: Read data from registers");
                            }
                            else
                                MakeErrorMessage(ref bufferMessage, (byte)ModbusCommand.InvalidData, "Error: Error: The quntity which needs to be read is bigger than registers.");
                        }
                        else if (bufferMessage[7] == (byte)ModbusCommand.Write)
                        {
                            if(bufferMessage[11] <= register.Length && (bufferMessage[9] + bufferMessage[11] - 1) <= register.Length)
                            {
                                for (int i = bufferMessage[9], addressBuf = (int)ModbusCommand.StartWriteIndex, counter = 0; counter < bufferMessage[11]; i++, addressBuf += 2, counter++)
                                {
                                    if (bufferMessage[addressBuf] > byte.MaxValue || bufferMessage[addressBuf] < byte.MinValue)
                                    {
                                        MakeErrorMessage(ref bufferMessage, (byte)ModbusCommand.OverflowData, "Error: Current data lower/bigger than type data 'byte'");
                                        break;
                                    }
                                    else
                                        register[i] = bufferMessage[addressBuf];
                                }
                                OnRunningServer("Success: Wrote data in registers");
                            }
                            else
                                MakeErrorMessage(ref bufferMessage, (byte)ModbusCommand.InvalidData, "Error: The quntity which needs to be written is bigger than registers.");
                        }
                        else
                            MakeErrorMessage(ref bufferMessage, (byte)ModbusCommand.ErrorUnknownCommand, "Error: Unknown command");
                        
                        listener.SendTo(bufferMessage, endPoint);
                    } while (listener.Available > 0);
                    listener.Shutdown(SocketShutdown.Both);
                    listener.Close();
                }
                catch (SocketException ex)
                {
                    OnRunningServer(ex.Message);
                    StartRun();
                }
            }
        }
        private void MakeErrorMessage(ref byte[] bufferMessage,byte codeErr,string message)
        {
            bufferMessage[5] = (byte)ModbusCommand.ResponseException;
            bufferMessage[7] = codeErr;
            bufferMessage[8] = 2;
            bufferMessage[9] = (byte)ModbusCommand.None;
            bufferMessage[11] = (byte)ModbusCommand.None;
            OnRunningServer(message);
        }
        public virtual void OnRunningServer(string e) => RunningServer?.Invoke(this, e);
        public void SetRegisterData(ref byte[] registerData) => register = registerData;

    }
}