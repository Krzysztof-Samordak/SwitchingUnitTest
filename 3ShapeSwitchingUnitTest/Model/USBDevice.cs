/// <copyright>3Shape A/S</copyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeShapeSwitchingUnitTest.USBDevices
{
    public class USBDevice
    {
        bool _isInserted;
        string _vidPid;
        public string expectedVidPid;
        public string expectedName;
        public bool lastTestResult;

        public USBDevice()
        {
            _isInserted = false;
            _vidPid = "";
            expectedName = "";
            expectedVidPid = "";
        }
        /// <summary>
        /// /////////
        /// </summary>
        /// <param name="givenVid_Pid"></param>
        /// <returns></returns>
        public bool InsertCheckId(string givenVid_Pid)
        {
            bool returnValue = false;
            string onlyVid = "";
            if (givenVid_Pid.Length > 0 && givenVid_Pid.Contains("VID_"))
            {
                onlyVid = givenVid_Pid.Substring(givenVid_Pid.IndexOf("VID_"), 8);
                if (onlyVid == expectedVidPid)
                {
                    _vidPid = givenVid_Pid;
                    _isInserted = true;
                    returnValue = true;
                }
            }
            return returnValue;
        }

        public bool InsertCheckIdAndName(string givenVid_Pid, string GivenName)
        {
            bool returnValue = false;
            string OnlyVid = "";
            if (givenVid_Pid.Length > 0 && givenVid_Pid.Contains("VID_") && GivenName.Length > 0)
            {
                OnlyVid = givenVid_Pid.Substring(givenVid_Pid.IndexOf("VID_"), 8);
                if (OnlyVid == expectedVidPid && GivenName.Contains(expectedName))
                {
                    _vidPid = givenVid_Pid;
                    _isInserted = true;
                    returnValue = true;
                }
            }
            return returnValue;
        }

        public bool InsertCheck(string givenVid_Pid, string GivenName)
        {
            bool returnValue = false;
            if (givenVid_Pid is not null && GivenName is not null)
            {
                if (givenVid_Pid.Length > 0 && GivenName.Length > 0)
                {
                    if (givenVid_Pid.Contains(expectedVidPid) && GivenName == expectedName)
                    {
                        _vidPid = givenVid_Pid;
                        _isInserted = true;
                        returnValue = true;
                    }
                }
            }
                return returnValue;
        }

        public bool RemoveCheckId(string givenVid_Pid)
        {
            bool returnValue = false;

            if (givenVid_Pid == _vidPid)
            {
                _vidPid = "";
                _isInserted = false;
                returnValue = true;
            }
            return returnValue;
        }

        public bool checkIfInserted()
        {
            bool returnValue = false;
            if (_isInserted)
            {
                returnValue = true;
            }
            return returnValue;
        }
        public void GetExpextedVid(string Vid)
        {
            expectedVidPid = Vid;
        }
        public void GetExpextedName(string Name_tmp)
        {
            expectedName = Name_tmp;
        }
        public string ReturnVid()
        {
            return expectedVidPid;
        }
        public void clearLastTestResult()
        {
            lastTestResult = false;
        }
        public void RemoveDevice()
        {
            _vidPid = "";
            _isInserted = false;
        }
    }
}