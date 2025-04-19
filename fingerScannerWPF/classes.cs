csharp
using System;
using System.Collections.Generic;

namespace fingerScannerWPF
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string FingerPrintId { get; set; }
        public DateTime LastCheckIn { get; set; }
        public List<DateTime> CheckInHistory { get; set; }

        public Student()
        {
            CheckInHistory = new List<DateTime>();
        }
    }

    public class AttendanceRecord
    {
        public int StudentId { get; set; }
        public DateTime CheckInTime { get; set; }
    }

    public class FingerprintScanner
    {
        public bool IsConnected { get; set; }
        
        public FingerprintScanner()
        {
          IsConnected = false;
        }
        public bool Connect()
        {
            //Simulate connection to the scanner
            IsConnected = true;
            return true;
        }
        public void Disconnect()
        {
            //Simulate disconnection from the scanner
            IsConnected = false;
        }
        public string CaptureFingerprint()
        {
           //Simulate capturing fingerprint data
            return "simulated_fingerprint_data";
        }
       
    }
}