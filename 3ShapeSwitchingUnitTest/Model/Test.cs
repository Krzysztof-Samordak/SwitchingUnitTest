/// <copyright>3Shape A/S</copyright>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeShapeSwitchingUnitTest.Tests
{
    public class Test
    {
        //Auxiliary variables
        const string pass = "PASS";
        const string fail = "FAIL";

        private string _result;
        public int Id { get; set; }
        public string Name { get; set; }
        public string result { get; set; }
        //public string result
        //{
        //    get { return _result; }
        //    set
        //    {
        //        if (value == pass)
        //        {
        //            _result = value;
        //        }
        //        else if (value == fail)
        //        {
        //            _result = value;
        //        }
        //    }
        //}

        public Test(string[] results)
        {
            if(results.Length == 3)
            {
                this.Id = Convert.ToInt16(results[0]);
                this.Name = results[1];
                this.result = results[2];
            }
        }
    }
}