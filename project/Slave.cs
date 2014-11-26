/* ****************************************************************************
*         (c) Copyright, Real-Time Innovations, All rights reserved.       
*                                                                          
*         Permission to modify and use for internal purposes granted.      
* This software is provided "as is", without warranty, express or implied. 
*                                                                          
* ****************************************************************************
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Helpers;

namespace PtpSlave {
    class Slave {
        // For clean shutdown sequence
        private static bool shutdown_flag = false;
        private static SlaveClock _slaveClock = new SlaveClock();
        public static void Main(string[] argv) {
            // Setup clock
            _slaveClock.Time = DateTime.Now;

            // Create the DDS Domain participant on domain ID 0
            DDS.DomainParticipant participant =
                DDS.DomainParticipantFactory.get_instance().create_participant(
                        0,
                        DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);
            if (participant == null) {
                Console.Error.WriteLine("Unable to create DDS domain participant");
                return;
            }

            // Create the topic "Sync" for the String type
            DDS.Topic syncTopic = participant.create_topic(
                        "Sync",
                        DDS.StringTypeSupport.get_type_name(),
                        DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);
            DDS.Topic followUpTopic = participant.create_topic(
                        "Follow-up",
                        DDS.StringTypeSupport.get_type_name(),
                        DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);
            if (syncTopic == null || followUpTopic == null) {
                Console.Error.WriteLine("Unable to create topic");
                return;
            }

            // Create the data reader using the default publisher
            DDS.StringDataReader syncReader = (DDS.StringDataReader)
                                participant.create_datareader(
                                syncTopic,
                                DDS.Subscriber.DATAREADER_QOS_DEFAULT,
                                new SyncSlaveListener() {SlaveClock = _slaveClock},
                                DDS.StatusMask.STATUS_MASK_ALL);
            DDS.StringDataReader followUpReader = (DDS.StringDataReader)
                                participant.create_datareader(
                                followUpTopic,
                                DDS.Subscriber.DATAREADER_QOS_DEFAULT,
                                new FollowUpSlaveListener() { SlaveClock = _slaveClock },
                                DDS.StatusMask.STATUS_MASK_ALL);


            if (syncReader == null || followUpReader == null) {
                Console.WriteLine("! Unable to create DDS Data Reader");
                return;
            }

            Console.WriteLine("Ready to read data.");
            Console.WriteLine("Press CTRL+C to terminate.");
            for (; ; ) {
                //Thread.Sleep(2000);
                _slaveClock.Oscillate();
                Console.WriteLine(_slaveClock.Time.ToString("hh.mm.ss.ffffff"));
            if (shutdown_flag) {
                break;
                }
            }
            Console.WriteLine("Shutting down...");
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(ref participant);
        }

        /*
         * This method gets called back by DDS when one or more data samples have
         * been received.
         */
        //public override void on_data_available(DDS.DataReader reader) {
        //    DDS.StringDataReader stringReader = (DDS.StringDataReader)reader;
        //    DDS.SampleInfo info = new DDS.SampleInfo();
        //    for(;;) {
        //        try {
        //            string sample = stringReader.take_next_sample(info);
        //            Console.WriteLine(sample);
        //            if(sample == ""){
        //                shutdown_flag = true;
        //            }
        //        }
        //        catch(DDS.Retcode_NoData) {
        //            // No more data to read
        //            break;
        //        }
        //        catch(DDS.Exception e) {
        //            // An error occurred in DDS
        //            Console.WriteLine(e.StackTrace);
        //            break;
        //        }
        //    }
        //}
    }
}

