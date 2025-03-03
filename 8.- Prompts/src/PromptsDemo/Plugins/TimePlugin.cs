using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptsDemo.Plugins
{
    public class TimePlugin
    {
        [KernelFunction("get_time")]
        public string GetTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        [KernelFunction("get_days_until_christmas")]
        public string GetDaysUntilChristmas(DateTime dateTime)
        {
            DateTime christmas = new DateTime(dateTime.Year, 12, 25).ToUniversalTime();
            if (dateTime > christmas)
            {
                christmas = christmas.AddYears(1);
            }
            TimeSpan timeUntilChristmas = christmas - dateTime;
            return timeUntilChristmas.Days.ToString();
        }

    }


}
