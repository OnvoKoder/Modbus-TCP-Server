# Modbus-TCP-Server
#### It's a cross-platforms library write on C#. This library work on Windows, Linux (Ubuntu) and Mac. 
#### Library in charge off work server(simulation device/microcontrollers) (write and read). 
```ruby
using ModbusTCP;
using ModbusTCP.Enums;
using ModbusTCPServer;

internal class Program
{

    private static void Main(string[] args)
    {
        Server server = new Server("127.0.0.1");
        var sq = new byte[] { 69, 96 };
        server.SetRegisterData(ref sq); //Send array of registers
        server.RunningServer += (sender, e) => Console.WriteLine(e);//Get Server status
        Task.Run(async () => await server.StartRun());//Run Server async
        Modbus device_1 = new ModbusTcp("127.0.0.1");
        while (true)
        {
            //Get zero element of register
            int [] array = device_1.ReadHoldingInt(0, Endians.Endians_0123);
            foreach (int element in array)
                Console.WriteLine(element);
            device_1 = new ModbusTcp("127.0.0.1");
            //Write in regidters (zero, first)
            device_1.WriteHolding(0, new ushort[] { 256, 255 }, Endians.Endians_0123);
            device_1 = new ModbusTcp("127.0.0.1");
            //Get zero element of register
            array = device_1.ReadHoldingInt(0, Endians.Endians_0123);
            foreach (var element in array)
                Console.WriteLine(element);
            break;
        }
    }
}
```
