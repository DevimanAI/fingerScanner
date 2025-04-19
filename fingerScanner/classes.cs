using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;

namespace fingerScanner
{
    public class Credentials
    {
        public string id { get; set; }
        public string password { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public User User { get; set; }
    }
    public class student
    {

        public string id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string nationCode { get; set; }
        public string picture { get; set; }
        public string fingerprint { get; set; }
    }

    public class postfingerprint
    {
        public string studentId { get; set; }
        public Byte[] fingerprint { get; set; }
    }

    public class FingerPrintManager
    {
        USBCommunication uSBCommunication = new USBCommunication("COM5", 57600);
        public Int32 GenImg()
        {
            try
            {
                uSBCommunication.Close();
                uSBCommunication.Open();
                uSBCommunication.clearUsb();

                byte[] command = { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x03, 0x01, 0x00, 0x05 };
                uSBCommunication.SendData(command);
                Thread.Sleep(800);
                Byte[] Response = USBData.ReciveData(12);
                Int32 ConfirmationCode = ByteArrays.ConvertBytesToInt(Response, 9, 1);
                return ConfirmationCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }
        public Int32 UpImage()
        {
            try
            {
                byte[] command = { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x03, 0x0A, 0x00, 0x0E };
                uSBCommunication.clearUsb();
                uSBCommunication.SendData(command);
                Thread.Sleep(800);
                Byte[] Response = USBData.ReciveData(12);
                Int32 ConfirmationCode = ByteArrays.ConvertBytesToInt(Response, 9, 1);
                return ConfirmationCode;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }
        public Int32 Img2Tz(Byte BufferID)
        {
            try
            {
                byte[] command = { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x02, BufferID, 0x00, Convert.ToByte(0x01 + 0x04 + 0x02 + BufferID) };
                uSBCommunication.clearUsb();
                uSBCommunication.SendData(command);
                Thread.Sleep(800);
                Byte[] Response = USBData.ReciveData(12);
                Int32 ConfirmationCode = ByteArrays.ConvertBytesToInt(Response, 9, 1);
                return ConfirmationCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }
        public Int32 RegModel()
        {
            try
            {
                uSBCommunication.clearUsb();
                byte[] command = { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x03, 0x05, 0x00, 0x09 };
                uSBCommunication.SendData(command);
                Thread.Sleep(800);
                Byte[] Response = USBData.ReciveData(12);
                Int32 ConfirmationCode = ByteArrays.ConvertBytesToInt(Response, 9, 1);
                return ConfirmationCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }
        public Int32 UpChar(Byte bufferID)
        {
            try
            {
                byte[] command = { 0xEF, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x04, 0x08, bufferID, 0x00, Convert.ToByte(0x01 + 0x04 + 0x08 + bufferID) };
                uSBCommunication.SendData(command);
                Thread.Sleep(800);
                Byte[] Response = USBData.ReciveData(12);
                Int32 ConfirmationCode = ByteArrays.ConvertBytesToInt(Response, 9, 1);
                return ConfirmationCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return -1;
        }
    }

    public class USBCommunication
    {
        private SerialPort _serialPort;
        private int baudRate;
        public USBCommunication(string portName, int inbaudRate)
        {
            baudRate = inbaudRate;
            //_serialPort = new SerialPort(portName, inbaudRate);
            //_serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            //_serialPort.Open();
            Open();
        }

        public void Open()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        return;
                    }
                }
                foreach (string port in ports)
                {
                    try
                    {
                    _serialPort = new SerialPort(port, baudRate);
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        break;
                    }
                    }catch(Exception ex)
                    {
                        MessageBox.Show("not found post. searching... ");
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void SendData(byte[] data)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Write(data, 0, data.Length);
            }
            else
            {
                MessageBox.Show("Serial port is not open.");
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                int bufferSize = 4000;

                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                while (sp.BytesToRead > 0)
                {
                    if (sp.BytesToRead < bufferSize)
                    {
                        Array.Resize(ref buffer, sp.BytesToRead);
                        bytesRead = sp.Read(buffer, 0, sp.BytesToRead);
                    }
                    else
                    {
                        Array.Resize(ref buffer, bufferSize);
                        bytesRead = sp.Read(buffer, 0, bufferSize);
                    }

                    //if (bytesRead<bufferSize)
                    //{
                    //   Array.Resize(ref buffer, bytesRead);
                    //}
                    USBData.AddData(buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void clearUsb() {
            SerialPort sp = _serialPort;
            int bufferSize = 4000;

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while (sp.BytesToRead > 0)
            {
                if (sp.BytesToRead < bufferSize)
                {
                    Array.Resize(ref buffer, sp.BytesToRead);
                    bytesRead = sp.Read(buffer, 0, sp.BytesToRead);
                }
                else
                {
                    Array.Resize(ref buffer, bufferSize);
                    bytesRead = sp.Read(buffer, 0, bufferSize);
                }
            }
            USBData.Clear();
        }
    }
    public static class USBData
    {
        private static byte[] Data = new byte[0];
        public static Int32 Length
        {
            get
            {
                return Data.Length;
            }
        }
        public static void Clear()
        {
            Array.Resize(ref Data, 0);
        }
        public static void AddData(byte[] RecivedData)
        {
            try
            {
                Int32 j = Data.Length;
                Array.Resize(ref Data, Data.Length + RecivedData.Length);
                foreach (byte item in RecivedData)
                {
                    Data[j++] = item;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در دریافت داده" + ex.Message);
            }

        }

        public static byte[] ReciveData(Int32 len)
        {
            byte[] receivedData = new byte[len];
            try
            {
                if (len <= 0 || len > Data.Length)
                {
                    throw new ArgumentException("Invalid length specified.");
                }

                Array.Copy(Data, receivedData, len);
                byte[] remainingData = new byte[Data.Length - len];
                Array.Copy(Data, len, remainingData, 0, remainingData.Length);
                Data = remainingData;
            }
            catch (Exception ex) { MessageBox.Show("خطا در دریافت اطلاعات از ماژول!"); }
            return receivedData;

        }
    }
    public static class ByteArrays
    {
        public static Int32 ConvertBytesToInt(byte[] byteArray, int startIndex, int byteCount)
        {
            if (byteArray == null)
            {
                throw new ArgumentNullException(nameof(byteArray));
            }
            if (startIndex < 0 || startIndex >= byteArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (byteCount <= 0 || startIndex + byteCount > byteArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            }

            int result = 0;
            for (int i = 0; i < byteCount; i++)
            {
                result = (result << 8) | byteArray[startIndex + i];
            }

            return result;
        }

        public static byte[] AppendBytes(byte[] array1, byte[] array2, int n, int l)
        {
            if (n < 0 || l < 0 || n + l > array2.Length)
            {
                throw new ArgumentException("Invalid parameters: n or l out of range.");
            }

            byte[] extractedBytes = new byte[l];
            Array.Copy(array2, n, extractedBytes, 0, l);

            byte[] resultArray = new byte[array1.Length + extractedBytes.Length];
            Array.Copy(array1, resultArray, array1.Length);
            Array.Copy(extractedBytes, 0, resultArray, array1.Length, extractedBytes.Length);

            return resultArray;
        }
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

    }
}
