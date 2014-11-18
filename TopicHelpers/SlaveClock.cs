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
        public DateTime LastSyncAt { get; set; }
        private Random _random = new Random();

        public void Oscillate()
        {
            var milliSeconds = _random.Next(2); // Time is "ticking"
            Time += new TimeSpan(0, 0, 0, 0, milliSeconds);
        }
    }
}
