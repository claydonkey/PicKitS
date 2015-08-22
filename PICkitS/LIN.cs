namespace PICkitS
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class LIN
    {
        internal const int FRAME_ARRAY_COUNT = 0x100;
        internal static int FRAME_TIMEOUT = 100;
        internal static volatile bool m_autobaud_is_on = true;
        internal static FRAMEINFO[] m_Frames = new FRAMEINFO[0x100];
        internal static Timer m_FrameStartTimer;
        internal static double m_interbyte_timeout = 0.01;
        internal static ushort m_last_master_baud_rate = 0;
        internal static bool m_next_frame_is_first_frame = true;
        private static byte m_OnReceive_error = 0;
        internal static OPMODE m_opmode;
        internal static Thread m_reset_timer;
        internal static SLAVE_PROFILE_ID m_slave_profile_id;
        internal static AutoResetEvent m_slave_profile_id_read;
        public static Stopwatch m_stopwatch;
        private static long[] m_time = new long[11];
        internal static bool m_use_baud_rate_timeout = false;
        private static volatile bool m_we_are_finishing_a_frame = false;
        internal static WORKING_FRAME m_working_frame = new WORKING_FRAME();
        private static AutoResetEvent m_working_frame_is_done;
        internal const int MAX_NUM_DATA_BYTES = 9;

        public static  event GUINotifierOA OnAnswer;

        public static  event GUINotifierOR OnReceive;

        public static bool Add_LIN_Slave_Profile_To_PKS(byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str, ref int p_error_code)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0xff];
            byte[] buffer3 = new byte[0x41];
            bool flag2 = false;
            byte num = 0;
            byte num2 = 0;
            string str = "";
            int index = 0;
            p_error_code = 0;
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer3, 0, buffer3.Length);
            byte num4 = Number_Of_Bytes_In_CBUF3(ref num, ref num2);
            if (p_array_byte_count > num4)
            {
                p_result_str = string.Format("Byte count of {0} greater than allowed value of {1}.", p_array_byte_count, num4);
                p_error_code = 1;
                return flag;
            }
            USBWrite.Clear_CBUF(3);
            if (!Basic.Get_Status_Packet(ref buffer3))
            {
                p_result_str = string.Format("Error reading status packet.", new object[0]);
                p_error_code = 2;
                return false;
            }
            buffer3[0x17] = (byte) (buffer3[0x17] | 0x20);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer3);
            flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
            Array.Clear(buffer2, 0, buffer2.Length);
            buffer2[0] = 0;
            buffer2[1] = 5;
            buffer2[2] = p_array_byte_count;
            for (int i = 3; i < (p_array_byte_count + 3); i++)
            {
                buffer2[i] = p_profile_array[i - 3];
            }
            bool flag3 = USBWrite.Send_Script_To_PICkitS(ref buffer2);
            if (!(flag & flag3))
            {
                p_error_code = 2;
                p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            if (!USBWrite.Update_Status_Packet())
            {
                p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            index = 7;
            while (index < 0x1f)
            {
                if (Constants.STATUS_PACKET_DATA[index] != array[index - 5])
                {
                    p_error_code = 3;
                    p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", index - 7, Constants.STATUS_PACKET_DATA[index], array[index - 5]);
                    break;
                }
                index++;
            }
            if (index == 0x1f)
            {
                flag2 = true;
                p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
            }
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            return flag2;
        }

        private static ushort calculate_baud_rate(ushort p_baud)
        {
            double num = 20.0 / (4.0 * (p_baud + 1.0));
            int num2 = (int) Math.Round((double) (num * 1000000.0));
            return (ushort) num2;
        }

        internal static void calculate_new_baud_dependent_onreceive_timeout(int p_baud)
        {
            double num = ((11.0 / ((double) p_baud)) * 10.0) * 1.5;
            FRAME_TIMEOUT = (int) (num * 1000.0);
        }

        public static bool Change_LIN_BAUD_Rate(ushort Baud)
        {
            bool flag = false;
            int num = 0;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                if (!Basic.Get_Status_Packet(ref buffer2))
                {
                    return false;
                }
                num = ((int) Math.Round((double) ((20000000.0 / ((double) Baud)) / 4.0))) - 1;
                buffer2[0x1d] = (byte) num;
                buffer2[30] = (byte) (num >> 8);
                USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                flag = USBWrite.write_and_verify_LIN_config_block(ref array, ref str2, true, ref str);
            }
            if (flag)
            {
                Get_LIN_BAUD_Rate();
            }
            return flag;
        }

        public static bool Configure_PICkitSerial_For_LIN()
        {
            bool flag = false;
            if (Basic.Configure_PICkitSerial(10, true))
            {
                Get_LIN_BAUD_Rate();
                flag = true;
            }
            return flag;
        }

        public static bool Configure_PICkitSerial_For_LIN(bool p_chip_select_hi, bool p_receive_enable, bool p_autobaud)
        {
            bool flag = false;
            if ((Utilities.m_flags.HID_read_handle != IntPtr.Zero) && Basic.Configure_PICkitSerial(10, true))
            {
                string str = "";
                string str2 = "";
                byte[] array = new byte[0x41];
                byte[] buffer2 = new byte[0x41];
                if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
                {
                    return flag;
                }
                Array.Clear(array, 0, array.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                if (!Basic.Get_Status_Packet(ref buffer2))
                {
                    return false;
                }
                if (p_chip_select_hi)
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] | 8);
                }
                else
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] & 0xf7);
                }
                if (p_receive_enable)
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] | 0x40);
                }
                else
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] & 0xbf);
                }
                if (p_autobaud)
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] | 0x80);
                }
                else
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] & 0x7f);
                }
                USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                flag = USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
                if (flag)
                {
                    Get_LIN_BAUD_Rate();
                }
            }
            return flag;
        }

        public static bool Configure_PICkitSerial_For_LIN_No_Autobaud()
        {
            bool flag = false;
            if (Basic.Configure_PICkitSerial(0x13, true))
            {
                Get_LIN_BAUD_Rate();
                flag = true;
            }
            return flag;
        }

        public static bool Configure_PICkitSerial_For_LINSlave_Mode(byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str, bool p_autobaud, ref int p_error_code)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0xff];
            byte[] buffer3 = new byte[0x41];
            bool flag2 = false;
            byte num = 0;
            byte num2 = 0;
            string str = "";
            int index = 0;
            p_error_code = 0;
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer3, 0, buffer3.Length);
            byte num4 = Number_Of_Bytes_In_CBUF3(ref num, ref num2);
            if (p_array_byte_count > num4)
            {
                p_result_str = string.Format("Byte count of {0} greater than allowed value of {1}.", p_array_byte_count, num4);
                p_error_code = 1;
                return flag;
            }
            USBWrite.Clear_CBUF(3);
            if (p_autobaud)
            {
                Mode.update_status_packet_data(10, ref buffer3);
            }
            else
            {
                Mode.update_status_packet_data(0x13, ref buffer3);
            }
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer3);
            USBWrite.Send_Cold_Reset_Cmd();
            flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
            USBWrite.Send_Warm_Reset_Cmd();
            Array.Clear(buffer2, 0, buffer2.Length);
            buffer2[0] = 0;
            buffer2[1] = 5;
            buffer2[2] = p_array_byte_count;
            for (int i = 3; i < (p_array_byte_count + 3); i++)
            {
                buffer2[i] = p_profile_array[i - 3];
            }
            USBWrite.Send_Script_To_PICkitS(ref buffer2);
            buffer3[0x17] = (byte) (buffer3[0x17] | 0x20);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer3);
            if (!USBWrite.Send_Data_Packet_To_PICkitS(ref array))
            {
                p_error_code = 2;
                p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            if (!USBWrite.Update_Status_Packet())
            {
                p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            index = 7;
            while (index < 0x1f)
            {
                if (Constants.STATUS_PACKET_DATA[index] != array[index - 5])
                {
                    p_error_code = 3;
                    p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", index - 7, Constants.STATUS_PACKET_DATA[index], array[index - 5]);
                    break;
                }
                index++;
            }
            if (index == 0x1f)
            {
                flag2 = true;
                p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
            }
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            return flag2;
        }

        private static void copy_this_frame_into_array()
        {
            m_Frames[m_working_frame.FrameInfo.FrameID].bytecount = m_working_frame.FrameInfo.bytecount;
            m_Frames[m_working_frame.FrameInfo.FrameID].baud = m_working_frame.FrameInfo.baud;
            m_Frames[m_working_frame.FrameInfo.FrameID].time = m_working_frame.FrameInfo.time;
            for (int i = 0; i < m_working_frame.FrameInfo.FrameData.Length; i++)
            {
                m_Frames[m_working_frame.FrameInfo.FrameID].FrameData[i] = m_working_frame.FrameInfo.FrameData[i];
            }
        }

        public static bool DisplayAll_mode_Is_Set()
        {
            return (m_opmode == OPMODE.DISPLAY_ALL);
        }

        internal static void finalize_working_frame()
        {
            if (!m_we_are_finishing_a_frame)
            {
                finish_this_frame();
            }
        }

        private static void finish_this_frame()
        {
            m_we_are_finishing_a_frame = true;
            m_working_frame.BuildState.we_are_building_a_frame = false;
            m_FrameStartTimer.Change(-1, -1);
            bool flag = false;
            if (this_is_a_valid_frame())
            {
                if (m_next_frame_is_first_frame)
                {
                    m_working_frame.FrameInfo.time = 0.0;
                    m_next_frame_is_first_frame = false;
                    flag = !USBRead.reset_timer_params();
                }
                if (m_working_frame.FrameInfo.baud != null)
                {
                    m_working_frame.FrameInfo.baud = calculate_baud_rate(m_working_frame.FrameInfo.baud);
                }
                else
                {
                    m_working_frame.FrameInfo.baud = m_last_master_baud_rate;
                }
                m_OnReceive_error = 0;
                uint num = 0;
                if (Status.There_Is_A_Status_Error(ref num))
                {
                    m_OnReceive_error = 4;
                    Device.Clear_Status_Errors();
                }
                if ((m_OnReceive_error == 0) && m_working_frame.BuildState.we_had_a_status_error)
                {
                    m_OnReceive_error = 5;
                }
                else if (m_working_frame.BuildState.we_timed_out)
                {
                    m_OnReceive_error = 1;
                }
                else if (flag)
                {
                    m_OnReceive_error = 3;
                }
                else if (m_working_frame.BuildState.next_frame_header_received)
                {
                    m_OnReceive_error = 6;
                }
                if ((this_frame_is_different_than_last() && (m_opmode == OPMODE.LISTEN)) || (m_opmode == OPMODE.DISPLAY_ALL))
                {
                    if (m_working_frame.BuildState.we_have_transmitted && m_working_frame.BuildState.transmit_data_byte_count_zero)
                    {
                        OnAnswer(m_working_frame.FrameInfo.FrameID, m_working_frame.FrameInfo.FrameData, m_working_frame.FrameInfo.bytecount, m_OnReceive_error, m_working_frame.FrameInfo.baud, m_working_frame.FrameInfo.time);
                    }
                    else if ((!m_working_frame.BuildState.we_have_transmitted || !m_working_frame.BuildState.transmit_data_byte_count_zero) && (OnReceive != null))
                    {
                        if (m_OnReceive_error == 6)
                        {
                            m_OnReceive_error = 0;
                        }
                        OnReceive(m_working_frame.FrameInfo.FrameID, m_working_frame.FrameInfo.FrameData, m_working_frame.FrameInfo.bytecount, m_OnReceive_error, m_working_frame.FrameInfo.baud, m_working_frame.FrameInfo.time);
                    }
                }
                copy_this_frame_into_array();
            }
            reset_working_frame();
            m_working_frame_is_done.Set();
            m_we_are_finishing_a_frame = false;
        }

        private static void frame_has_timed_out(object state)
        {
            while (m_stopwatch.ElapsedMilliseconds < FRAME_TIMEOUT)
            {
                Thread.Sleep(10);
            }
            m_working_frame.FrameInfo.frame_timeout_time = m_stopwatch.ElapsedMilliseconds;
            if (!m_we_are_finishing_a_frame)
            {
                if (m_working_frame.FrameInfo.FrameID == 0)
                {
                    byte num1 = USBRead.m_raw_cbuf2_data_array[USBRead.m_cb2_array_tag_index];
                }
                m_working_frame.BuildState.we_timed_out = true;
                finish_this_frame();
            }
        }

        public static ushort Get_LIN_BAUD_Rate()
        {
            byte[] array = new byte[0x41];
            ushort num = 0;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    num = calculate_baud_rate((ushort) (array[50] + (array[0x33] << 8)));
                    m_last_master_baud_rate = num;
                }
            }
            return num;
        }

        public static bool Get_LIN_Options(ref bool p_chip_select_hi, ref bool p_receive_enable, ref bool p_autobaud)
        {
            byte[] array = new byte[0x41];
            bool flag = false;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    p_chip_select_hi = (array[0x17] & 8) > 0;
                    p_receive_enable = (array[0x17] & 0x40) > 0;
                    p_autobaud = (array[0x17] & 0x80) > 0;
                    flag = true;
                }
            }
            return flag;
        }

        public static int Get_OnReceive_Timeout()
        {
            return FRAME_TIMEOUT;
        }

        internal static void initialize_LIN_frames()
        {
            m_stopwatch = new Stopwatch();
            m_opmode = OPMODE.LISTEN;
            m_working_frame.FrameInfo.FrameData = new byte[9];
            reset_working_frame();
            Reset_LIN_Frame_Buffers();
            m_FrameStartTimer = new Timer(new TimerCallback(LIN.frame_has_timed_out), null, -1, -1);
            m_working_frame.BuildState.we_have_transmitted = false;
            m_working_frame.BuildState.transmit_data_byte_count_zero = false;
            m_working_frame_is_done = new AutoResetEvent(false);
            m_slave_profile_id_read = new AutoResetEvent(false);
            m_slave_profile_id.ByteCount = 0;
            m_slave_profile_id.FrameID = 0;
            m_slave_profile_id.Data = new byte[0xff];
        }

        public static bool Listen_mode_Is_Set()
        {
            return (m_opmode == OPMODE.LISTEN);
        }

        public static byte Number_Of_Bytes_In_CBUF3(ref byte p_used_bytes, ref byte p_unused_bytes)
        {
            byte num = 0;
            if (USBWrite.Update_Special_Status_Packet())
            {
                Utilities.m_flags.g_status_packet_mutex.WaitOne();
                num = (byte) (Constants.STATUS_PACKET_DATA[0x39] + Constants.STATUS_PACKET_DATA[0x3a]);
                p_used_bytes = Constants.STATUS_PACKET_DATA[0x39];
                p_unused_bytes = Constants.STATUS_PACKET_DATA[0x3a];
                Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            }
            return num;
        }

        public static bool OnReceive_Timeout_Is_Baud_Dependent()
        {
            return m_use_baud_rate_timeout;
        }

        public static bool Read_Slave_Profile(byte p_masterid, ref byte[] p_data, byte p_expected_byte_count, ref byte p_actual_byte_count, ref byte p_error_code)
        {
            bool flag = false;
            p_error_code = 0;
            byte[] array = new byte[30];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 4;
            array[3] = 0x87;
            array[4] = p_masterid;
            array[5] = 0x1f;
            array[6] = 0x77;
            array[7] = 0;
            Array.Clear(m_slave_profile_id.Data, 0, m_slave_profile_id.Data.Length);
            if (USBWrite.Send_Script_To_PICkitS(ref array))
            {
                if (m_slave_profile_id_read.WaitOne(0x7d0, false))
                {
                    if (p_masterid == m_slave_profile_id.FrameID)
                    {
                        p_actual_byte_count = m_slave_profile_id.ByteCount;
                        if (p_expected_byte_count >= m_slave_profile_id.ByteCount)
                        {
                            for (int i = 0; i < m_slave_profile_id.ByteCount; i++)
                            {
                                p_data[i] = m_slave_profile_id.Data[i];
                            }
                            return true;
                        }
                        p_error_code = 3;
                        return flag;
                    }
                    p_error_code = 4;
                    return flag;
                }
                p_error_code = 1;
                return flag;
            }
            p_error_code = 2;
            return flag;
        }

        public static void Reset_LIN_Frame_Buffers()
        {
            for (int i = 0; i < m_Frames.Length; i++)
            {
                m_Frames[i].FrameID = (byte) i;
                m_Frames[i].FrameData = new byte[9];
                m_Frames[i].bytecount = 0;
                m_Frames[i].baud = 0;
                m_Frames[i].time = 0.0;
                m_Frames[i].frame_timeout_time = 0L;
                for (int j = 0; j < m_Frames[i].FrameData.Length; j++)
                {
                    m_Frames[i].FrameData[j] = 0;
                }
            }
        }

        internal static void reset_LIN_timeout()
        {
            m_reset_timer = new Thread(new ThreadStart(LIN.reset_lin_timer));
            m_reset_timer.Start();
            m_reset_timer.Join();
        }

        private static void reset_lin_timer()
        {
            m_FrameStartTimer.Change(FRAME_TIMEOUT, -1);
            m_stopwatch.Reset();
            m_stopwatch.Start();
        }

        public static void Reset_Timer()
        {
            m_next_frame_is_first_frame = true;
        }

        internal static void reset_working_frame()
        {
            for (int i = 0; i < m_working_frame.FrameInfo.FrameData.Length; i++)
            {
                m_working_frame.FrameInfo.FrameData[i] = 0;
            }
            m_working_frame.FrameInfo.FrameID = 0;
            m_working_frame.FrameInfo.bytecount = 0;
            m_working_frame.FrameInfo.baud = 0;
            m_working_frame.FrameInfo.time = 0.0;
            m_working_frame.FrameInfo.frame_timeout_time = 0L;
            m_working_frame.BuildState.we_had_a_status_error = false;
            m_working_frame.BuildState.we_are_building_a_frame = false;
            m_working_frame.BuildState.we_have_an_id = false;
            m_working_frame.BuildState.we_timed_out = false;
            m_working_frame.BuildState.next_frame_header_received = false;
            m_working_frame.BuildState.we_have_transmitted = false;
            m_working_frame.BuildState.transmit_data_byte_count_zero = false;
        }

        internal static void send_on_answer(byte p_id, double p_time, ref byte[] p_data)
        {
            if (OnAnswer != null)
            {
                OnAnswer(p_id, p_data, 9, 0, 0, p_time);
            }
        }

        public static bool Set_LIN_Options(bool p_chip_select_hi, bool p_receive_enable, bool p_autobaud)
        {
            bool flag = false;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return flag;
            }
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer2, 0, buffer2.Length);
            if (!Basic.Get_Status_Packet(ref buffer2))
            {
                return false;
            }
            if (p_chip_select_hi)
            {
                buffer2[0x17] = (byte) (buffer2[0x17] | 8);
            }
            else
            {
                buffer2[0x17] = (byte) (buffer2[0x17] & 0xf7);
            }
            if (p_receive_enable)
            {
                buffer2[0x17] = (byte) (buffer2[0x17] | 0x40);
            }
            else
            {
                buffer2[0x17] = (byte) (buffer2[0x17] & 0xbf);
            }
            if (p_autobaud)
            {
                buffer2[0x17] = (byte) (buffer2[0x17] | 0x80);
            }
            else
            {
                buffer2[0x17] = (byte) (buffer2[0x17] & 0x7f);
            }
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_LIN_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Set_OnReceive_Timeout(int Timeout)
        {
            if (Timeout == 0xffff)
            {
                if (set_OnReceive_timeout_from_baud_rate())
                {
                    m_use_baud_rate_timeout = true;
                    return true;
                }
                Timeout = 0;
                return false;
            }
            m_use_baud_rate_timeout = false;
            FRAME_TIMEOUT = Timeout;
            return true;
        }

        private static bool set_OnReceive_timeout_from_baud_rate()
        {
            byte[] array = new byte[0x41];
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (!Basic.Get_Status_Packet(ref array))
                {
                    return false;
                }
                calculate_new_baud_dependent_onreceive_timeout(calculate_baud_rate((ushort) (array[50] + (array[0x33] << 8))));
            }
            return true;
        }

        public static bool SetModeDisplayAll()
        {
            m_opmode = OPMODE.DISPLAY_ALL;
            return true;
        }

        public static bool SetModeListen()
        {
            m_opmode = OPMODE.LISTEN;
            return true;
        }

        public static bool SetModeTransmit()
        {
            m_opmode = OPMODE.TRANSMIT;
            return true;
        }

        private static bool this_frame_is_different_than_last()
        {
            if (m_Frames[m_working_frame.FrameInfo.FrameID].bytecount != m_working_frame.FrameInfo.bytecount)
            {
                return true;
            }
            if (m_Frames[m_working_frame.FrameInfo.FrameID].baud != m_working_frame.FrameInfo.baud)
            {
                return true;
            }
            for (int i = 0; i < m_working_frame.FrameInfo.FrameData.Length; i++)
            {
                if (m_Frames[m_working_frame.FrameInfo.FrameID].FrameData[i] != m_working_frame.FrameInfo.FrameData[i])
                {
                    return true;
                }
            }
            return false;
        }

        private static bool this_is_a_valid_frame()
        {
            return true;
        }

        private static bool Toggle_AutoBaud_Set(bool p_turn_autobaudset_on, ref ushort p_baud, ref string p_error_detail)
        {
            bool flag = false;
            int num = 0;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            p_error_detail = "";
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                if (!Basic.Get_Status_Packet(ref buffer2))
                {
                    p_error_detail = "Could not poll PKSA for status.";
                    return false;
                }
                p_baud = calculate_baud_rate((ushort) (buffer2[0x1d] + (buffer2[30] << 8)));
                if (p_turn_autobaudset_on)
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] | 0x80);
                }
                else
                {
                    buffer2[0x17] = (byte) (buffer2[0x17] & 0x7f);
                }
                USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                while (!flag && (num < 3))
                {
                    num++;
                    flag = USBWrite.write_and_verify_LIN_config_block(ref array, ref str2, false, ref str);
                    p_error_detail = p_error_detail + str2;
                }
            }
            return flag;
        }

        public static bool Transmit(byte MasterID, byte[] Data, byte DataByteCount, ref string ErrorString)
        {
            int index = 0;
            bool flag = false;
            byte[] array = new byte[30];
            Array.Clear(array, 0, array.Length);
            if (DataByteCount > 9)
            {
                ErrorString = "DataByteCount cannot exceed 9.";
                return flag;
            }
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (DataByteCount + 5);
            array[3] = 0x84;
            array[4] = (byte) (DataByteCount + 1);
            array[5] = MasterID;
            array[6] = 0x1f;
            array[7] = 0x77;
            array[8] = 0;
            if (DataByteCount == 0)
            {
                m_working_frame.BuildState.transmit_data_byte_count_zero = true;
                m_working_frame.BuildState.we_have_transmitted = true;
                m_working_frame_is_done.Reset();
                flag = USBWrite.Send_Script_To_PICkitS(ref array);
                if (!flag)
                {
                    ErrorString = "Error sending script.";
                    return false;
                }
                if (m_working_frame_is_done.WaitOne(0x1770, false))
                {
                    return flag;
                }
                ErrorString = "No data returned";
                return false;
            }
            index = 0;
            while (index < DataByteCount)
            {
                array[index + 6] = Data[index];
                index++;
            }
            array[index + 6] = 0x1f;
            array[index + 7] = 0x77;
            array[index + 8] = 0;
            m_working_frame.BuildState.transmit_data_byte_count_zero = false;
            m_working_frame.BuildState.we_have_transmitted = true;
            m_working_frame_is_done.Reset();
            flag = USBWrite.Send_Script_To_PICkitS(ref array);
            if (!flag)
            {
                ErrorString = "Error sending script.";
                return false;
            }
            return flag;
        }

        public static bool Transmit_mode_Is_Set()
        {
            return (m_opmode == OPMODE.TRANSMIT);
        }

        public static bool Write_Slave_Profile(byte p_masterid, ref byte[] p_data, byte p_byte_count, ref byte p_error_code)
        {
            bool flag = false;
            p_error_code = 0;
            byte[] array = new byte[0xff];
            byte[] buffer2 = new byte[0xff];
            byte num = 0;
            byte num2 = 0;
            byte index = 0;
            if (p_byte_count > 0xf4)
            {
                p_error_code = 3;
                return false;
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (p_byte_count + 5);
            array[3] = 0x86;
            array[4] = p_masterid;
            array[5] = p_byte_count;
            index = 0;
            while (index < p_byte_count)
            {
                array[6 + index] = p_data[index];
                index = (byte) (index + 1);
            }
            array[index + 6] = 0x1f;
            array[index + 7] = 0x77;
            array[index + 8] = 0;
            if (USBWrite.Send_Script_To_PICkitS(ref array))
            {
                if (!Read_Slave_Profile(p_masterid, ref buffer2, p_byte_count, ref num2, ref num))
                {
                    p_error_code = 1;
                    return flag;
                }
                for (index = 0; index < p_byte_count; index = (byte) (index + 1))
                {
                    if (buffer2[index] != p_data[index])
                    {
                        p_error_code = 1;
                        return false;
                    }
                }
                return true;
            }
            p_error_code = 2;
            return flag;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BUILD_STATE
        {
            internal volatile bool we_have_transmitted;
            internal volatile bool we_are_building_a_frame;
            internal volatile bool we_have_an_id;
            internal volatile bool we_had_a_status_error;
            internal volatile bool we_timed_out;
            internal volatile bool next_frame_header_received;
            internal volatile bool transmit_data_byte_count_zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FRAMEINFO
        {
            internal volatile byte FrameID;
            internal volatile byte[] FrameData;
            internal volatile byte bytecount;
            internal volatile ushort baud;
            internal double time;
            internal long frame_timeout_time;
        }

        public delegate void GUINotifierOA(byte masterid, byte[] data, byte length, byte error, ushort baud, double time);

        public delegate void GUINotifierOR(byte masterid, byte[] data, byte length, byte error, ushort baud, double time);

        internal enum OPMODE
        {
            LISTEN,
            TRANSMIT,
            DISPLAY_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SLAVE_PROFILE_ID
        {
            internal byte FrameID;
            internal byte ByteCount;
            internal byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WORKING_FRAME
        {
            internal LIN.FRAMEINFO FrameInfo;
            internal LIN.BUILD_STATE BuildState;
        }
    }
}

