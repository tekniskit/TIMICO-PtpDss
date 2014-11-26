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
            DDS.Topic delayResponseTopic = participant.create_topic(
                        "Delay response",
                        DDS.StringTypeSupport.get_type_name(),
                        DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);
            DDS.Topic delayRequestTopic = participant.create_topic(
                        "Delay request",
                        DDS.StringTypeSupport.get_type_name(),
                        DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);

            if (syncTopic == null || followUpTopic == null || delayResponseTopic == null || delayRequestTopic == null) {
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
            DDS.StringDataReader delayResponseReader = (DDS.StringDataReader)
                                participant.create_datareader(
                                delayResponseTopic,
                                DDS.Subscriber.DATAREADER_QOS_DEFAULT,
                                new DelayResponseSlaveListener() { SlaveClock = _slaveClock },
                                DDS.StatusMask.STATUS_MASK_ALL);


            if (syncReader == null || followUpReader == null || delayResponseReader == null) {
                Console.WriteLine("! Unable to create DDS Data Reader");
                return;
            }

            // Delay request writer
            DDS.StringDataWriter delayRequestWriter = (DDS.StringDataWriter)participant.create_datawriter(
                            delayRequestTopic,
                            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
                            null, /* Listener */
                            DDS.StatusMask.STATUS_MASK_NONE);
            
            if (delayRequestWriter == null)
            {
                Console.Error.WriteLine("Unable to create DDS data writer");
                return;
            }

            
            for (; ; ) {
                _slaveClock.Oscillate();
                if (_slaveClock.TimeForDelayRequest)
                {
                    // DELAY REQUEST
                    delayRequestWriter.write("Delay request", ref DDS.InstanceHandle_t.HANDLE_NIL);
                    Console.WriteLine("Sent: Delay request");
                    _slaveClock.TimeForDelayRequest = false;
                    _slaveClock.TsDelayStart = _slaveClock.Time;
                }
                Console.WriteLine(_slaveClock.Time.ToString("hh.mm.ss.ffffff"));
            if (shutdown_flag) {
                break;
                }
            }
            Console.WriteLine("Shutting down...");
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(ref participant);
        }
    }
}

