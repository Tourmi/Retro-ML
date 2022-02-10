﻿using SMW_ML.Game;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Arduino
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

        public void SendInput(Input input)
        {
            serial.Write(input.GetButtonBytes(), 0, 2);
        }

        public void Dispose()
        {
            serial.Dispose();
        }

        public static bool ArduinoAvailable(string port) => SerialPort.GetPortNames().Contains(port);
    }
}
