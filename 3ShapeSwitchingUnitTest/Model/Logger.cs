/// <copyright>3Shape A/S</copyright>
using System;
using System.IO;
using System.Windows;

namespace ThreeShapeSwitchingUnitTest.Loggers
{
    public class Logger
    {
        string _currentDirectory;
        string _fileName;
        string _filePath;
        public Logger()
        {
            this._currentDirectory = Directory.GetCurrentDirectory();
            this._fileName = "Log.txt";
            this._filePath = this._currentDirectory + "/" + this._fileName;
            if (File.Exists(this._filePath))
            {
                try
                {
                    File.Delete(this._filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void log(string message)
        {
            try
            {
                using (System.IO.StreamWriter writer = System.IO.File.AppendText(this._filePath))
                {
                    writer.Write("\r\nLog Entry : ");
                    writer.Write("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                    writer.WriteLine(":   {0}", message);
                    writer.WriteLine("---------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void logMultiple(string[] message, string optionalMessage = null)
        {
            try
            {
                using (System.IO.StreamWriter writer = System.IO.File.AppendText(this._filePath))
                {
                    writer.Write("\r\nLog Entry: ");
                    writer.Write("{0} {1}: ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                    if (optionalMessage != null)
                    {
                        writer.WriteLine(optionalMessage);
                    }
                    for (int i = 0; i < message.Length; i++)
                    {
                        writer.WriteLine(message[i]);
                    }
                    writer.WriteLine("");
                    writer.WriteLine("---------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}