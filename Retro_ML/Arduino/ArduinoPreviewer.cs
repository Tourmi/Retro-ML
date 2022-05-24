using System.IO.Ports;

namespace Retro_ML.Arduino
{
    internal class ArduinoPreviewer : IDisposable
    {
        private const int SERIAL_BAUD = 9600;

        private SerialPort serial;

        public ArduinoPreviewer(string port)
        {
            serial = new SerialPort(port, SERIAL_BAUD);
            serial.Open();
        }

        /// <summary>
        /// Sends the bytes to the Arduino.                                            <br/>
        /// Must be two bytes long, and the order of the bit flags should be as follow:<br/>
        /// <br>[0]: A</br>
        /// <br>[1]: B</br>
        /// <br>[2]: X</br>
        /// <br>[3]: Y</br>
        /// <br>[4]: Left</br>
        /// <br>[5]: Right</br>
        /// <br>[6]: Up</br>
        /// <br>[7]: Down</br>
        /// <br>[8]: Left Shoulder</br>
        /// <br>[9]: Right Shoulder</br>
        /// <br>[10]: Start</br>
        /// <br>[11]: Select</br>
        /// </summary>
        /// <param name="inputBytes"></param>
        public void SendInput(byte[] inputBytes)
        {
            serial.Write(inputBytes, 0, 2);
        }

        public void Dispose()
        {
            serial.Dispose();
        }

        public static bool ArduinoAvailable(string port) => SerialPort.GetPortNames().Contains(port);
    }
}
