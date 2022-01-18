using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Arduino
{
    public class ArduinoPreviewer : IDisposable
    {
        private const string SERIAL_PORT = "COM3";
        private const int SERIAL_BAUD = 9600;

        private SerialPort serial;

        public ArduinoPreviewer()
        {
            serial = new SerialPort(SERIAL_PORT, SERIAL_BAUD);
            serial.Open();
        }

        public void SendInput(Input input)
        {
            serial.Write(input.GetButtonBytes(), 0, 2);
        }

        public void Dispose()
        {
            serial.Dispose();
        }

        public static bool ArduinoAvailable() => SerialPort.GetPortNames().Contains(SERIAL_PORT);
    }
}
