using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class SlaveClock
    {
        public DateTime Time { get; set; }
        public TimeSpan Delay { get; set; }
        public DateTime Ts1 { get; set; }
        public DateTime TsDelayStart { get; set; }
        public bool TimeForDelayRequest { get; set; }
        
        private Random _random = new Random();

        public void Oscillate()
        {
            if (_random.Next(2) > 0)
            {
                var milliSeconds = _random.Next(2); // Time is "ticking"
                Time += new TimeSpan(0, 0, 0, 0, milliSeconds);
            }
        }
    }
}
