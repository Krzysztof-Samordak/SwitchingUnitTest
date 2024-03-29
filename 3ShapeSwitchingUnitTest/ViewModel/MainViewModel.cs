﻿/// <copyright>3Shape A/S</copyright>
///
using ThreeShapeSwitchingUnitTest.Commands;
using ThreeShapeSwitchingUnitTest.Loggers;
using ThreeShapeSwitchingUnitTest.Tests;
using ThreeShapeSwitchingUnitTest.Counters;
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
using System.Text.Json.Serialization;
using Newtonsoft.Json;


namespace ThreeShapeSwitchingUnitTest.MainViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //Logging mechanism initialization
        //        Logger _logger = new Logger();

        //Total numbers of tests achived from python script
        int _totalNumberOfTests = 9;

        //Test Setup variables
        string _currentDirectory = Directory.GetCurrentDirectory();
        string _image1Path = empty;
        string _image2Path = empty;
        string _image3Path = empty;
        string _image4Path = empty;
        string _image5Path = empty;
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
        const string visible = "Visible";
        const string hid = "Hidden";
        const string connectingToTester = "Connecting to the tester";


        //Test procedure variables
        string _pythonConsolePath = empty;
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
        private Counter _pcbUsageCounter;
        private int _testsNumber;
        private DateTime _pcbReplaceDate;
        private string _loadingGifVisibility = hid;
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

        public int testsNumber
        { get { return _testsNumber; } set { _testsNumber = value; OnPropertyChanged(); } }

        public DateTime PCBReplaceDate
        { get { return _pcbReplaceDate; } set { _pcbReplaceDate = value; OnPropertyChanged(); } }

        public string loadingGifVisibility
        {
            get { return _loadingGifVisibility; }
            set
            {
                if(value == visible || value == hid)
                {
                    _loadingGifVisibility = value;
                    OnPropertyChanged();
                }
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
                if (value == ready || value == testStop || value == testFinished || value == empty)
                {
                    testStageColor = "Azure";
                    _testStage = value;
                    OnPropertyChanged();
                }
                else if (value == testInProgress || value == connectingToTester)
                {
                    testStageColor = "Orange";
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
                    loadingGifVisibility = hid;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            _pcbUsageCounter = new Counter();
            _testProcess = new Process();

            _pcbUsageCounter = ReadJSON<Counter>(nameof(Counter));

            PCBReplaceDate = _pcbUsageCounter.date;
            testsNumber = _pcbUsageCounter.number;

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

            _pcbUsageCounter.DailyCheck();
            CounterUpdate(_pcbUsageCounter, true);
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void StartProcess(object obj)
        {
            int numberOfTests = 0;
            var result = fail;
            startButton = dis;
            try
            {
                _testProcess.Start();
//                _logger.log("Process Started");

                _testProcess.BeginOutputReadLine();
                _testProcess.BeginErrorReadLine();
                _testProcess.WaitForExit();
//                _logger.log("Process exited");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start the process! Make sure that Scan-os Production Tools are installed");
//                _logger.log("Cannot start the process! Make sure that Scan-os Production Tools are installed");
                testResult = empty;
            }
            startButton = en;
            stopTest();
//            _logger.log("Tester connection status: " + _testerStatus);

            if (testStage == testInProgress && _testProcess.StartInfo.Arguments == _pythonTestScriptName)
            {
                CounterUpdate(_pcbUsageCounter, false);
                numberOfTests = tests.Count;
                testStage = testFinished;
                if (numberOfTests == _totalNumberOfTests && tests.All(test => test.result == en))
                {
                    result = pass;
                }
                else if (numberOfTests == 0)
                {
                    MessageBox.Show("Something went wrong... Not all tests have been performed! There should be " + _totalNumberOfTests
                        + " tests!");
                    result = empty;
                }
                testResult = result;
//                _logger.log("Test result: " + testResult);
            }
        }

        public void CheckTesterConnection()
        {
            //Setup test
            _testProcess.StartInfo.Arguments = _pythonTestConnectionScriptName;
            testStage = connectingToTester;
            loadingGifVisibility = visible;
//            _logger.log("Checking tester connection");

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
                    ConfigurationManager.AppSettings.Get("Image4Name") is not null &&
                    ConfigurationManager.AppSettings.Get("Image5Name") is not null &&
                    ConfigurationManager.AppSettings.Get("PCBUsageCounterLimit") is not null)
                {
                    _pythonConsolePath = ConfigurationManager.AppSettings.Get("PythonConsolePath");
                    _pythonTestScriptName = ConfigurationManager.AppSettings.Get("PythonTestScriptName");
                    _pythonTestConnectionScriptName = ConfigurationManager.AppSettings.Get("PythonTestConnectionScriptName");
                    _testerSerialNumber = ConfigurationManager.AppSettings.Get("BS_MBSerialNumber");
                    _image1Path = _currentDirectory + @"\Images\" +  ConfigurationManager.AppSettings.Get("Image1Name");
                    _image2Path = _currentDirectory + @"\Images\" + ConfigurationManager.AppSettings.Get("Image2Name");
                    _image3Path = _currentDirectory + @"\Images\" + ConfigurationManager.AppSettings.Get("Image3Name");
                    _image4Path = _currentDirectory + @"\Images\" + ConfigurationManager.AppSettings.Get("Image4Name");
                    _image5Path = _currentDirectory + @"\Images\" + ConfigurationManager.AppSettings.Get("Image5Name");
                    _pcbUsageCounter.limit = Convert.ToInt16(ConfigurationManager.AppSettings.Get("PCBUsageCounterLimit"));
                }
                else
                {
                    throw new Exception("Trying to read config value, but it is null! The app might work incorrectly!!");
                }
            }
            catch (Exception ex)
            {
//                _logger.log(ex.ToString());
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
 //               _logger.log("Cannot find image file: " + _image1Path);
                MessageBox.Show("Cannot find image file: " + _image1Path);
                returnValue = false;
            }
            if (!File.Exists(_image2Path))
            {
//                _logger.log("Cannot find image file: " + _image2Path);
                MessageBox.Show("Cannot find image file: " + _image2Path);
                returnValue = false;
            }
            if (!File.Exists(_image3Path))
            {
 //               _logger.log("Cannot find image file: " + _image3Path);
                MessageBox.Show("Cannot find image file: " + _image3Path);
                returnValue = false;
            }
            if (!File.Exists(_image4Path))
            {
 //               _logger.log("Cannot find image file: " + _image4Path);
                MessageBox.Show("Cannot find image file: " + _image4Path);
                returnValue = false;
            }
            if (!File.Exists(_image5Path))
            {
                //               _logger.log("Cannot find image file: " + _image5Path);
                MessageBox.Show("Cannot find image file: " + _image5Path);
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
//                _logger.log(ex.Message);
                MessageBox.Show(ex.Message);
            }
            testResult = empty;
//            _logger.log("Test stopped");
        }

        void SendCommand(string command = empty)
        {
            _testProcess.StandardInput.WriteLine(command);
//            _logger.log("Command sent: " + command);
        }

        public void PrepareTestSetup()
        {
            string[] input;

            // DataReceive from tester EventHandler
            _testProcess.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                string message;
                if (e.Data != null)
                {
                    message = e.Data.ToString();
                    Debug.WriteLine(message);
//                    _logger.log("message recived: " + message);

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
                        case "TimeoutError":  // Handle TimeoutError
                            MessageBox.Show("Cannot connect to tester!");
                            testerStatusColor = "Red";
                            testerStatus = "Disconnected";
                            testStage = empty;
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

                        case "Place Articulator Plate on Rotation1 and press OK": //Handle showing notification window during SwitchingUnit endstop1_switch test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Place Articulator Plate on Rotation1 and press OK",
                                    _image5Path, false)));
                            SendCommand();
                            break;

                        case "Remove Articulator Plate from Rotation1 and press OK": //Handle showing notification window during SwitchingUnit endstop1_switch test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Remove Articulator Plate from Rotation1 and press OK")));
                            SendCommand();
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
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Rotate the SwitchingUnit right and press 'OK'",
                                    _image3Path, false)));
                            SendCommand();
                            break;

                        case "Rotate the SwitchingUnit left!": //Handle showing notification window during SwitchingUnit endstop2_switch test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification(@"Rotate the SwitchingUnit left and press 'OK'",
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
                        case "EndStop2 activated! The SwitchingUnitMotor probably is rotating in wrong direction!": // Handle showing error notification window during SwitchingUnitMotor Test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("EndStop2 activated!" +
                                   " The Switching Unit Motor probably is" + " rotating in wrong direction!")));
                            break;

                        case "Cannot perform SwitchingUnitMotor because of defective sensors!": //Handle showing error notification window during SwitchingUnitMotor Test
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Cannot perform Switching Unit Motor " +
                                "Test because of defective sensors!")));
                            break;
                        case "Cannot perform Rotation1Switch test because of defective motor": // Handle showing error notification window caused by Rotation1Motor Failure
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Cannot perform" +
                                " Rotation1Switch test because of defective motor!")));
                            SendCommand();
                            break;
                        case "Cannot perform Rotation2Switch test because of defective motor": // Handle showing error notification window caused by Rotation1Motor Failure
                            App.Current.Dispatcher.Invoke(new Action(() => ShowNotification("Cannot perform" +
                                " Rotation2Switch test because of defective motor!")));
                            SendCommand();
                            break;
                        case var tmp when message.Contains("Err_"): // Catch commands errors
                            MessageBox.Show(message);
                            break;
                    }
                }
            });

            // DataReceive from tester Error EventHandler
            _testProcess.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                string message;
                if (e.Data != null)
                {
                    message = e.Data.ToString();
                    Debug.WriteLine(message);
//                    _logger.log("message recived: " + message);

                    switch (message)
                    {
                        case var tmp when message.Contains("TimeoutError"): // Handle TimeoutError
                            MessageBox.Show("Cannot connect to tester!");
                            testerStatusColor = "Red";
                            testerStatus = "Disconnected";
                            testStage = empty;
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
                        case var tmp when  message.Contains("OVERCURRENT"): // Handle overcurrent  state error
                            MessageBox.Show("Overcurrent protection mechanism activated! Please turn off the tester, disconnect switching" +
                                " unit under test and check the IST_CB PCBA(10004517) for any visible damage sights.");
                            testStage = testStop;
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

        public bool SaveToJSON(object obj, string name)
        {
            bool returnValue = true;
            string serializedObject = JsonConvert.SerializeObject(obj);
            try
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + @"\" + name + ".json", serializedObject);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                returnValue = false;
            }
            return returnValue;
        }

        public T ReadJSON<T>(string name)
        {
            string serializedObject = string.Empty;
            T returnValue;
            try
            {
                serializedObject = File.ReadAllText(Directory.GetCurrentDirectory() + @"\" + name + ".json");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            returnValue = JsonConvert.DeserializeObject<T>(serializedObject);
            return returnValue;
        }

        public void CounterUpdate(Counter counter, bool daily)
        {
            counter.check(daily);
            testsNumber = counter.number;
            PCBReplaceDate = counter.date;
            SaveToJSON(counter, nameof(counter));
        }
    }
}



