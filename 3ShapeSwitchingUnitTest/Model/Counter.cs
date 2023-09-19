using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeShapeSwitchingUnitTest.Controls.MessageBox.Views;


namespace ThreeShapeSwitchingUnitTest.Counters
{
    public class Counter
    {

        public int number;
        public DateTime date;
        public int limit = 0;
        public string name = string.Empty;

        public void increase()
        {
            number++;
        }
        public void reset()
        {
            number = 0;
        }

        public bool check(bool daily)
        {
            bool returnValue = false;
            if (number >= limit)
            {
                bool? result = false;
                App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Exceeded the limit of tests performed, please replace the IST_CB PCBA(10004517). Press 'Yes' to confirm replacement or 'No' to skip.", null, true)));
                if (result == true)
                {
                    // User accepted the dialog box
                    number = 0;
                    date = DateTime.Now;
                    name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    returnValue = true;
                }
                else
                {
                    // User cancelled the dialog box
                    if (!daily) number++;
                }
            }
            else
            {
                if(!daily) number++;
            }
            return returnValue;

        }
        public bool DailyCheck()
        {
            bool returnValue = false;
            bool? result = false;

            if (date.Day == DateTime.Today.Day)
            {
                returnValue = true;
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() => result = ShowNotification("Check IST_CB PCBA(10004517) for any visible" +
                    " sights of damage or dirt on connector. Press 'Yes' to confirm good condition of connector / press 'No' to indicate" +
                    " the need of connector replacement", null, true)));
                if (result == false)
                {
                    number = limit;
                }
            }
            return returnValue;
        }

            public bool? ShowNotification(string description, string imagePath = null, bool selection = false)
        {
            NotificationWindow dialog = new NotificationWindow(description, imagePath, selection);
            var result = dialog.ShowDialog();
            return result;
        }
    }
}
