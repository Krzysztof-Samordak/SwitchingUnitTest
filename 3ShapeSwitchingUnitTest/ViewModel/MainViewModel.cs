/// <copyright>3Shape A/S</copyright>
///
using ThreeShapeSwitchingUnitTest.Commands;
using ThreeShapeSwitchingUnitTest.Loggers;
using ThreeShapeSwitchingUnitTest.Tests;
using ThreeShapeSwitchingUnitTest.Controls.MessageBox.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ThreeShapeSwitchingUnitTest.MainViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //Logging mechanism initialization
        Logger _logger = new Logger();

        //Total numbers of tests achived from python script
        int _totalNumberOfTests = 0;

        //Test Setup variables
        string _currentDirectory = Directory.GetCurrentDirectory();
        string _image1Path = empty;
        string _image2Path = empty;
        string _image3Path = empty;
        string _image4Path = empty;
        string _testerSerialNumber = empty;

        //Auxiliary variables
        const string en = "True";
        const string dis = "False";
        const string ready = "Ready";
        const string testInProgress = "Test in progress!";
        const string testFinished = "Test finished!";
        const string testStop = "Test Stopped!";
        const string pass = "PASS";
        const string fail = "FAIL";
        const string empty = "";
        const string connected = "Connected";
        const string disconnected = "Disconnected";
        const string clear = "clear";

        //Test procedure variables
        string _pythonConsolePath = empty;
        string _pythonTestScriptPath = empty;
        string _pythonTestScriptName = empty;
        string _pythonTestConnectionScriptName = empty;
        Process _testProcess;
        ProcessStartInfo _testStartInfo = new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        //View variables inicialization
        private string _testerStatus = disconnected;
        private string _testerStatusColor;
        private string _startButton = en;
        private string _testStage;
        private string _testStageColor;
        private string _testResult;
        private string _testResultColor;
        private ObservableCollection<Test> _tests;

        //Create ICommand object
        public ICommand startTestCommand { get; set; }

        //Create Property Changed Event Handler
        public event PropertyChangedEventHandler PropertyChanged;

        //Vriables required for View update setup
        public ObservableCollection<Test> tests
        {
            get { return _tests; }

            set
            {
                _tests = value;
                OnPropertyChanged();
            }
        }
        public string testStageColor
        {
            get { return _testStageColor; }
            set
            {
                Color c = Color.FromName(value);
                if (c.IsKnownColor)
                {
                    _testStageColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public string startButton
        {
            get { return _startButton; }
            set
            {
                if (value == en || value == dis)
                {
                    _startButton = value;
                    OnPropertyChanged();
                }
            }
        }
        public string testStage
        {
            get { return _testStage; }
            set
            {
                if (value == ready)
                {
                    testStageColor = "Azure";
                    _testStage = value;
                    OnPropertyChanged();
                }
                else if (value == testInProgress)
                {
                    testStageColor = "Orange";
                    _testStage = value;
                    OnPropertyChanged();
                }
                else if (value == testFinished)
                {
                    testStageColor = "Azure";
                    _testStage = value;
                    OnPropertyChanged();
                }
                else if (value == testStop)
                {
                    testStageColor = "Azure";
                    _testStage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string testResult
        {
            get { return _testResult; }
            set
            {
                if (value == pass)
                {
                    testResultColor = "Green";
                    _testResult = value;
                    OnPropertyChanged();
                }
                else if (value == fail)
                {
                    testResultColor = "Red";
                    _testResult = value;
                    OnPropertyChanged();
                }
                else if (value == clear)
                {
                    _testResult = "";
                    OnPropertyChanged();
                }
            }
        }
        public string testResultColor
        {
            get { return _testResultColor; }
            set
            {
                Color c = Color.FromName(value);
                if (c.IsKnownColor)
                {
                    _testResultColor = value;
                    OnPropertyChanged();
                }
            }
        }
        public string testerStatus
        {
            get { return _testerStatus; }
            set
            {
                if (value == connected)
                {
                    _testerStatusColor = "Green";
                    _testerStatus = value;
                    OnPropertyChanged();
                }
                else if (value == disconnected)
                {
                    _testerStatusColor = "Red"; ;
                    _testerStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public string testerStatusColor
        {
            get { return _testerStatusColor; }
            set
            {
                Color c = Color.FromName(value);
                if (c.IsKnownColor)
                {
                    _testerStatusColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            _testProcess = new Process();

            //Read Configuration file and attach values to adequate variables
            ReadConfig();

            //Setup Test Process
            _testStartInfo.FileName = _pythonConsolePath;
            _testProcess.StartInfo = _testStartInfo;
            PrepareTestSetup();

            startTestCommand = new RelayCommand(StartTestClick);

            //Create tests list
            tests = new ObservableCollection<Test>();

            //Check connection with the tester
            CheckTesterConnection();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartProcess(object obj)
        {
            var result = fail;
            startButton = dis;
            try
            {
                _testProcess.Start();
                _logger.log("Process Started");

                _testProcess.BeginOutputReadLine();
                _testProcess.BeginErrorReadLine();
                _testProcess.WaitForExit();
                _logger.log("Process exited");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start the process! Make sure that Scan-os Production Tools are installed");
                _logger.log("Cannot start the process! Make sure that Scan-os Production Tools are installed");
                testResult = empty;
            }
            startButton = en;
            stopTest();
            _logger.log("Tester connection status: " + _testerStatus);

            if (testStage == testInProgress && _testProcess.StartInfo.Arguments == _pythonTestScriptName)
            {
                testStage = testFinished;
                if (tests.Count == _totalNumberOfTests && tests.All(test => test.result == en))
                {
                    result = pass;
                }
                testResult = result;
                _logger.log("Test result: " + testResult);
            }
        }

        public void CheckTesterConnection()
        {
            //Setup test
            _testProcess.StartInfo.Arguments = _pythonTestConnectionScriptName;
            _logger.log("Checking tester connection");

            //Call Start Test process
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartProcess));
        }

        public void ReadConfig()
        {
            try
            {
                if (
                    ConfigurationManager.AppSettings.Get("PythonConsolePath") is not null &&
                    ConfigurationManager.AppSettings.Get("PythonTestScriptName") is not null &&
                    ConfigurationManager.AppSettings.Get("PythonTestConnectionScriptName") is not null &&
                    ConfigurationManager.AppSettings.Get("BS_MBSerialNumber") is not null &&
                    ConfigurationManager.AppSettings.Get("Image1Name") is not null &&
                    ConfigurationManager.AppSettings.Get("Image2Name") is not null &&
                    ConfigurationManager.AppSettings.Get("Image3Name") is not null &&
                    ConfigurationManager.AppSettings.Get("Image4Name") is not null)
                {
                    _pythonConsolePath = ConfigurationManager.AppSettings.Get("PythonConsolePath");
                    _pythonTestScriptName = ConfigurationManager.AppSettings.Get("PythonTestScriptName");
                    _pythonTestConnectionScriptName = ConfigurationManager.AppSettings.Get("PythonTestConnectionScriptName");
                    _testerSerialNumber = ConfigurationManager.AppSettings.Get("BS_MBSerialNumber");
                    _image1Path = _currentDirectory + @"\" + ConfigurationManager.AppSettings.Get("Image1Name");
                    _image2Path = _currentDirectory + @"\" + ConfigurationManager.AppSettings.Get("Image2Name");
                    _image3Path = _currentDirectory + @"\" + ConfigurationManager.AppSettings.Get("Image3Name");
                    _image4Path = _currentDirectory + @"\" + ConfigurationManager.AppSettings.Get("Image4Name");
                }
                else
                {
                    throw new Exception("Trying to read config value, but it is null! The app might work incorrectly!!");
                }
            }
            catch (Exception ex)
            {
                _logger.log(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }

        public bool? ShowNotification(string description, string imagePath = null, bool selection = false)
        {
            NotificationWindow dialog = new NotificationWindow(description, imagePath, selection);
            var result = dialog.ShowDialog();
            return result;
        }

        public void StartTestClick(object obj)
        {
            if (testStage != testInProgress)
            {
                //Clear view before start another test
                testStage = testInProgress;
                testResult = clear;
                tests.Clear();

                //Setup test
                _testProcess.StartInfo.Arguments = _pythonTestScriptName;

                //Start test
                ThreadPool.QueueUserWorkItem(new WaitCallback(StartProcess));
            }
            else
            {
                stopTest();
            }
        }

        public bool CheckIfImagesExists()
        {
            bool returnValue = true;
            if (!File.Exists(_image1Path))
            {
                _logger.log("Cannot find image file: " + _image1Path);
                MessageBox.Show("Cannot find image file: " + _image1Path);
                returnValue = false;
            }
            if (!File.Exists(_image2Path))
            {
                _logger.log("Cannot find image file: " + _image2Path);
                MessageBox.Show("Cannot find image file: " + _image2Path);
                returnValue = false;
            }
            if (!File.Exists(_image3Path))
            {
                _logger.log("Cannot find image file: " + _image3Path);
                MessageBox.Show("Cannot find image file: " + _image3Path);
                returnValue = false;
            }
            if (!File.Exists(_image4Path))
            {
                _logger.log("Cannot find image file: " + _image4Path);
                MessageBox.Show("Cannot find image file: " + _image4Path);
                returnValue = false;
            }
            return returnValue;
        }

        public void stopTest()
        {
            try
            {
                _testProcess.CancelOutputRead();
                _testProcess.CancelErrorRead();
                _testProcess.Close();
            }
            catch (Exception ex)
            {
                _logger.log(ex.Message);
                MessageBox.Show(ex.Message);
            }
            testResult = empty;
            _logger.log("Test stopped");
        }

        void SendCommand(string command = empty)
        {
            _testProcess.StandardInput.WriteLine(command);
            _logger.log("Command sent: " + command);
        }

        public void PrepareTestSetup()
        {
            string[] input;
            this._pythonTestScriptPath = _currentDirectory;

            // DataReceive from tester EventHandler
            _testProcess.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                string message;
                if (e.Data != null)
                {
                    message = e.Data.ToString();
                    _logger.log("message recived: " + message);

                    switch (message)
                    {
                        case "Connected": // Handle connecting to tester
                            if (_testProcess.StartInfo.Arguments == _pythonTestConnectionScriptName)
                            {
                                testerStatusColor = "Green";
                                testerStatus = "Connected";
                                testStage = empty;
                                startButton = en;
                            }
                            else
                            {
                                testerStatusColor = "Green";
                                testerStatus = "Connected";
                                SendCommand();
                            }
                            break;

                        case var tmp when message.Contains("Number of tests -"): // Check how many tests should be performed
                            _totalNumberOfTests = Convert.ToInt16(e.Data.ToString().Split("-").Last());
                            break;

                        case "disconnected": // Handle disconneting of the tester
                            MessageBox.Show("Tester disconnected!");
                            testerStatusColor = "Red";
                            testerStatus = "Disconnected";
                            testStage = testStop;
                            break;

                        case "Provide tester serial number!": // Handle serial number transmission
                            SendCommand(_testerSerialNumber);
                            break;

                        case var tmp when message.Contains("Test"): // Handle displaying test result
                            Test test;
                            input = message.Split("-").Skip(1).ToArray();
                            test = new Test(input);
                            App.Current.Dispatcher.Invoke(new Action(() => tests.Add(test)));
                            Thread.Sleep(1000);
                            break;

                        case "Check if Rotation1 is moving right": // Handle showing notification window during Rotation1 Motor Test
                            bool? result1 = false;
                            App.Current.Dispatcher.Invoke(new Action(() => result1 = ShowNotification("Check if Rotation1 is moving Right",
                                _image1Path, true)));
                            if (result1 == true)
                            {
                                // User accepted the dialog box
                                SendCommand("True");
                            }
                            else
                            {
                                // User cancelled the dialog box
                                SendCommand("False");
                            }
                            break;

                        case "Check if Rotation2 is moving right": // Handle showing notification window during Rotation2 Motor Test
                            bool? result2 = false;
                            App.Current.Dispatcher.Invoke(new Action(() => result2 = ShowNotification("Check if Rotation2 is moving Right",
                                _image2Path, true)));
                            if (result2 == true)
                            {
                                // User accepted the dialog box
                                SendCommand("True");
                            }
                            else
                            {
                                // User cancelled the dialog box
                                SendCommand("False");
                            }
                            break;

                        case "Rotate the SwitchingUnit right!": //Handle showing notification window during SwitchingUnit endstop1_switch test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Rotate the SwitchingUnit right!",
                                    _image3Path, false)));
                            SendCommand();
                            break;

                        case "Rotate the SwitchingUnit left!": //Handle showing notification window during SwitchingUnit endstop2_switch test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Rotate the SwitchingUnit left!",
                                _image4Path, false)));
                            SendCommand();
                            break;

                        case "Check if Switching Unit is Rotating Right!": //Handle showing notification window during SwitchingUnitMotor Test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Press 'OK' button and observe Switching Unit rotation!")));
                            SendCommand();
                            break;

                        case "Has Switching Unit rotated right?": //Handle showing notification window during SwitchingUnitMotor Test
                            bool? result = false;
                            App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Has Switching Unit rotated right?",
                                _image3Path, true)));
                            if (result == true)
                            {
                                // User accepted the dialog box
                                SendCommand("True");
                            }
                            else
                            {
                                // User cancelled the dialog box
                                SendCommand("False");
                            }
                            break;
                        case "EndStop2 activated! The Switching Unit Motor probably is rotating in wrong direction!": // Handle showing error notification window during SwitchingUnitMotor Test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("EndStop2 activated!" +
                                   " The Switching Unit Motor probably is" + " rotating in wrong direction!")));
                            break;

                        case "Cannot perform SwitchingUnitMotor because of defective sensors!": //Handle showing error notification window during SwitchingUnitMotor Test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Cannot perform Switching Unit Motor" +
                                    " Test because of defective sensors!")));
                            break;
                    }
                }
            });

            // DataReceive from tester EventHandler
            _testProcess.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                string message;
                if (e.Data != null)
                {
                    message = e.Data.ToString();
                    _logger.log("message recived: " + message);

                    switch (message)
                    {
                        case var tmp when message.Contains("TimeoutError"): // Handle TimeoutError
                            MessageBox.Show("Cannot connect to tester!");
                            testerStatusColor = "Red";
                            testerStatus = "Disconnected";
                            testStage = testStop;
                            break;

                        case var tmp when message.Contains("disconnected"): //Handle DisconnectedError
                            MessageBox.Show("Tester disconnected!");
                            testerStatusColor = "Red";
                            testerStatus = "Disconnected";
                            testStage = testStop;
                            break;

                        case var tmp when message.Contains("python.exe: can't open file"): //Handle missing file error
                            MessageBox.Show("Cannot open python script!");
                            break;

                        case var tmp when message.Contains("No such file or directory"): //Handle missing file error
                            MessageBox.Show("Cannot open python script!");
                            break;
                    }
                }
            });

            //Check if images required by notifications are in programm folder
            if (!CheckIfImagesExists())
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}

    // Handle connecting to tester
    //Debug.WriteLine(e.Data.ToString());
    //if (e.Data.ToString().Contains("Connected"))
    //{
    //    if (_testProcess.StartInfo.Arguments == _pythonTestConnectionScriptName)
    //    {
    //        testerStatusColor = "Green";
    //        testerStatus = "Connected";
    //        testStage = empty;
    //        startButton = en;
    //    }
    //    else
    //    {
    //        testerStatusColor = "Green";
    //        testerStatus = "Connected";
    //        SendCommand();
    //    }
    //}

    //// Check how many tests should be performed
    //else if (e.Data.ToString().Contains("Number of tests -"))
    //{
    //    _totalNumberOfTests = Convert.ToInt16(e.Data.ToString().Split("-").Last());
    //}

    //// Handle disconneting of the tester
    //else if (e.Data.ToString().Contains("disconnected"))
    //{
    //    MessageBox.Show("Tester disconnected!");
    //    testerStatusColor = "Red";
    //    testerStatus = "Disconnected";
    //    testStage = testStop;
    //}

    // Handle serial number transmission
    //else if (e.Data.ToString().Contains("Provide tester serial number!"))
    //{
    //    SendCommand(_testerSerialNumber);
    //}

    //// Handle displaying test result
    //else if (e.Data.ToString().Contains("Test"))
    //{
    //    input = e.Data.ToString().Split("-").Skip(1).ToArray();
    //    Test tmp = new Test(input);
    //    Debug.WriteLine(tmp);
    //    App.Current.Dispatcher.Invoke(new Action(() => tests.Add(tmp)));
    //    Thread.Sleep(1000);
    //}

    //// Handle showing notification window during Rotation1 Motor Test
    //else if (e.Data.ToString().Equals("Check if Rotation1 is moving right"))
    //{
    //    bool? result = false;

    //    App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Check if Rotation1 is moving Right",
    //        _image1Path, true)));
    //    if (result == true)
    //    {
    //        // User accepted the dialog box
    //        SendCommand("True");
    //    }
    //    else
    //    {
    //        // User cancelled the dialog box
    //        SendCommand("False");
    //    }
    //}

    // Handle showing notification window during Rotation2 Motor Test
    //else if (e.Data.ToString().Equals("Check if Rotation2 is moving right"))
    //{
    //    bool? result = false;
    //    App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Check if Rotation2 is moving Right",
    //        _image2Path, true)));
    //    if (result == true)
    //    {
    //        // User accepted the dialog box
    //        SendCommand("True");
    //    }
    //    else
    //    {
    //        // User cancelled the dialog box
    //        SendCommand("False");
    //    }
    //}

    //// Handle showing notification window during SwitchingUnit endstop1_switch test
    //else if (e.Data.ToString().Equals("Rotate the SwitchingUnit right!"))
    //{
    //    App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Rotate the SwitchingUnit right!",
    //        _image2Path, false)));
    //    SendCommand();
    //}

    //// Handle showing notification window during SwitchingUnit endstop2_switch test
    //else if (e.Data.ToString().Equals("Rotate the SwitchingUnit left!"))
    //{
    //    App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Rotate the SwitchingUnit left!",
    //        _image2Path, false)));
    //    SendCommand();
    //}

    //// Handle showing notification window during SwitchingUnitMotor Test
    //else if (e.Data.ToString().Equals("Check if Switching Unit is Rotating Right!"))
    //{
    //    App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Press 'OK' button and observe " +
    //        "Switching Unit rotation!")));
    //    SendCommand();
    //}


    //// Handle showing notification window during SwitchingUnitMotor Test
    //else if (e.Data.ToString().Equals("Has Switching Unit rotated right?"))
    //{
    //    bool? result = false;
    //    App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Has Switching Unit rotated right?",
    //        _image2Path, true)));
    //    if (result == true)
    //    {
    //        // User accepted the dialog box
    //        SendCommand("True");
    //    }
    //    else
    //    {
    //        // User cancelled the dialog box
    //        SendCommand("False");
    //    }
    //}

    //    // Handle showing notification window during SwitchingUnitMotor Test
    //    else if (e.Data.ToString().Equals("EndStop2 activated! The Switching Unit Motor probably is" +
    //        " rotating in wrong direction!"))
    //    {
    //        App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("EndStop2 activated!" +
    //            " The Switching Unit Motor probably is" + " rotating in wrong direction!")));
    //    }

    //    // Handle showing notification window during SwitchingUnitMotor Test
    //    else if (e.Data.ToString().Equals("Cannot perform SwitchingUnitMotor because of defective sensors!"))
    //    {
    //        App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Cannot perform Switching Unit Motor" +
    //            " Test because of defective sensors!")));
    //    }
    //    }
    //});

    // Add ErrorDataReceive from tester EventHandler
    //    _testProcess.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
    //    {
    //        if (e.Data != null)
    //        {
    //            Debug.WriteLine(e.Data);

    //            // Handle TimeoutError
    //            if (e.Data.ToString().Contains("TimeoutError"))
    //            {
    //                MessageBox.Show("Cannot connect to tester!");
    //                testerStatusColor = "Red";
    //                testerStatus = "Disconnected";
    //                testStage = testStop;
    //            }

    //            //Handle DisconnectedError
    //            else if (e.Data.ToString().Contains("disconnected"))
    //            {
    //                MessageBox.Show("Tester disconnected!");
    //                testerStatusColor = "Red";
    //                testerStatus = "Disconnected";
    //                testStage = testStop;
    //            }

    //            // Handle missing file error
    //            else if (e.Data.ToString().Contains("python.exe: can't open file") || e.Data.ToString().Contains("No such file or directory"))
    //            {
    //                MessageBox.Show("Cannot open python script!");
    //            }
    //        }
    //    });

    //    //Check if images required by notifications are in programm folder
    //    if (!CheckIfImagesExists())
    //    {
    //        System.Windows.Application.Current.Shutdown();
    //    }
    //}