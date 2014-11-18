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

namespace PtpMaster {
    class Master {
        public static void Main(string[] argv) {
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

            // Create the topic "Follow-up" for the String type
            DDS.Topic followUpTopic = participant.create_topic(
                        "Follow-up",
                        DDS.StringTypeSupport.get_type_name(),
                        DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                        null, /* Listener */
                        DDS.StatusMask.STATUS_MASK_NONE);
            if (syncTopic == null || followUpTopic == null)
            {
                Console.Error.WriteLine("Unable to create topic");
                return;
            }

            // Create the data writer using the default publisher
            DDS.StringDataWriter syncWriter = (DDS.StringDataWriter)participant.create_datawriter(
                            syncTopic,
                            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
                            null, /* Listener */
                            DDS.StatusMask.STATUS_MASK_NONE);
            DDS.StringDataWriter followUpWriter = (DDS.StringDataWriter)participant.create_datawriter(
                            followUpTopic,
                            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
                            null, /* Listener */
                            DDS.StatusMask.STATUS_MASK_NONE);
            if (syncWriter == null || followUpWriter == null) {
                Console.Error.WriteLine("Unable to create DDS data writer");
                return;
            }

            for (; ; ) {
                Thread.Sleep(5000);
                string currentTime = DateTime.Now.ToString("hh.mm.ss.ffffff");
                Console.WriteLine("Sync sent: " + currentTime);
                try {
                    syncWriter.write(currentTime, ref DDS.InstanceHandle_t.HANDLE_NIL);
                }
                catch (DDS.Retcode_Error e) {
                    Console.Error.WriteLine("Write error: " + e.Message);
                    break;
                }

                try
                {
                    followUpWriter.write(currentTime, ref DDS.InstanceHandle_t.HANDLE_NIL);
                }
                catch (DDS.Retcode_Error e)
                {
                    Console.Error.WriteLine("Write error: " + e.Message);
                    break;
                }

            }
            Console.WriteLine("Shutting down...");
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(ref participant);
        }
    }
}
