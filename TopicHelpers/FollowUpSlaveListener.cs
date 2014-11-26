using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public class FollowUpSlaveListener : DDS.DataReaderListener
    {
        // For clean shutdown sequence
        private static bool shutdown_flag = false;
        public SlaveClock SlaveClock { get; set; }

        public override void on_data_available(DDS.DataReader reader)
        {
            DDS.StringDataReader stringReader = (DDS.StringDataReader)reader;
            DDS.SampleInfo info = new DDS.SampleInfo();

            try
            {
                string sample = stringReader.take_next_sample(info);
                Console.WriteLine("Follow-up received");
                

                var format = "hh.mm.ss.ffffff";
                CultureInfo provider = CultureInfo.InvariantCulture;
                var Tm1 = DateTime.ParseExact(sample, format, provider);

                var offset = SlaveClock.Ts1 - Tm1; // - 0

                SlaveClock.Time -= offset;
                if (sample == "")
                {
                    shutdown_flag = true;
                }
            }
            catch (DDS.Retcode_NoData)
            {
                // No more data to read
                Console.WriteLine("No more data to read");
            }
            catch (DDS.Exception e)
            {
                // An error occurred in DDS
                Console.WriteLine(e.StackTrace);
            }

        }
    }
}
