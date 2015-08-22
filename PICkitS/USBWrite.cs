namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class USBWrite
    {
        private const byte EVENT_TIMER_RESET_V = 0x20;
        private static byte m_cbuf1_avail_bytes;
        private static Mutex m_cbuf1_avail_bytes_mutex = new Mutex(false);
        private static Thread m_clear_status_errors;
        private static byte[] m_command_byte_array;
        private static volatile bool m_data_buffer_is_empty;
        private static AutoResetEvent m_have_written_event;
        private static byte[] m_last_script_array;
        private static byte m_last_script_array_byte_count;
        private static Mutex m_last_script_array_mutex;
        private static object m_priv_write_buffer_lock = new object();
        private static object m_priv_write_script_lock = new object();
        private static AutoResetEvent m_ready_to_write_event;
        public static int m_universal_timeout = 0xbb8;
        internal static volatile bool m_use_script_timeout = true;
        private static volatile bool m_we_are_in_write_loop;
        private static volatile bool m_we_had_an_error_writing;
        private static Thread m_write_thread;
        private static AutoResetEvent m_write_thread_has_started_up_event;

        public static  event GUINotifierLockUp Tell_Host_PKSA_Needs_Reset;

        public static bool Clear_CBUF(byte p_buffer)
        {
            if ((p_buffer < 1) || (p_buffer > 3))
            {
                return false;
            }
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = (byte) (p_buffer + 7);
            return Send_Data_Packet_To_PICkitS(ref buffer);
        }

        public static void Clear_Status_Errors()
        {
            m_clear_status_errors = new Thread(new ThreadStart(USBWrite.Send_CommClear_Cmd));
            m_clear_status_errors.IsBackground = true;
            m_clear_status_errors.Start();
        }

        public static void configure_outbound_control_block_packet(ref byte[] p_data, ref string p_str, ref byte[] p_status_packet_data)
        {
            int index = 0;
            string str = "";
            p_str = "";
            p_data[0] = 0;
            p_data[1] = 2;
            index = 2;
            while (index < 0x1f)
            {
                str = string.Format("{0:X2} ", p_status_packet_data[(7 + index) - 2]);
                p_str = p_str + str;
                p_data[index] = p_status_packet_data[(7 + index) - 2];
                index++;
            }
            p_data[index] = 0;
            if ((p_status_packet_data[0x17] & 0x80) == 0x80)
            {
                LIN.m_autobaud_is_on = true;
            }
            else
            {
                LIN.m_autobaud_is_on = false;
            }
        }

        public static void Dispose_Of_Write_Objects()
        {
            m_write_thread_has_started_up_event.Close();
            m_ready_to_write_event.Close();
            m_have_written_event.Close();
            m_cbuf1_avail_bytes_mutex.Close();
        }

        public static byte Get_Last_Script_ByteCount()
        {
            return m_last_script_array_byte_count;
        }

        public static void Get_Last_Script_Sent(ref byte[] p_array, byte p_byte_count)
        {
            m_last_script_array_mutex.WaitOne();
            for (int i = 0; i < p_byte_count; i++)
            {
                p_array[i] = m_last_script_array[i];
            }
            m_last_script_array_mutex.ReleaseMutex();
        }

        public static void Initialize_Write_Objects()
        {
            m_we_are_in_write_loop = false;
            m_we_had_an_error_writing = false;
            m_cbuf1_avail_bytes = 0;
            m_data_buffer_is_empty = true;
            m_command_byte_array = new byte[0x41];
            m_last_script_array = new byte[0x5000];
            Array.Clear(m_last_script_array, 0, m_last_script_array.Length);
            Array.Clear(m_command_byte_array, 0, m_command_byte_array.Length);
            m_last_script_array_byte_count = 0;
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 0;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            m_write_thread_has_started_up_event = new AutoResetEvent(false);
            m_ready_to_write_event = new AutoResetEvent(false);
            m_have_written_event = new AutoResetEvent(false);
            m_priv_write_buffer_lock = new object();
            m_priv_write_script_lock = new object();
            m_last_script_array_mutex = new Mutex(false);
        }

        private static void issue_Tell_Host_PKSA_Needs_Reset()
        {
            if (Tell_Host_PKSA_Needs_Reset != null)
            {
                Tell_Host_PKSA_Needs_Reset();
            }
        }

        public static bool kick_off_write_thread()
        {
            if (!m_we_are_in_write_loop)
            {
                m_write_thread = new Thread(new ThreadStart(USBWrite.Write_USB_Thread));
                m_write_thread.IsBackground = true;
                m_write_thread.Start();
                return m_write_thread_has_started_up_event.WaitOne(0x1388, false);
            }
            return false;
        }

        public static void Kill_Write_Thread()
        {
            if ((m_write_thread != null) && m_write_thread.IsAlive)
            {
                m_we_are_in_write_loop = false;
                m_write_thread.Join();
            }
        }

        public static bool Send_Cold_Reset_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 0;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        public static void Send_CommClear_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 7;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
        }

        public static bool Send_CommReset_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 6;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_CtrlBlk2EE_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 3;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_CtrlBlkWrite_Cmd(ref byte[] p_data_array)
        {
            byte[] array = new byte[0x41];
            int index = 0;
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 2;
            index = 0;
            while (index < 0x18)
            {
                array[index + 2] = p_data_array[index];
                index++;
            }
            array[index] = 0;
            Send_Cold_Reset_Cmd();
            bool flag = Send_Data_Packet_To_PICkitS(ref array);
            Send_Warm_Reset_Cmd();
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_Data_Packet_To_PICkitS(ref byte[] p_data)
        {
            int index = 0;
            bool flag = true;
            lock (m_priv_write_buffer_lock)
            {
                if (m_we_are_in_write_loop && (Utilities.m_flags.HID_read_handle != IntPtr.Zero))
                {
                    for (index = 0; index < Utilities.m_flags.write_buffer.Length; index++)
                    {
                        Utilities.m_flags.write_buffer[index] = p_data[index];
                    }
                    m_ready_to_write_event.Set();
                    if (!m_have_written_event.WaitOne(0xbb8, false))
                    {
                        issue_Tell_Host_PKSA_Needs_Reset();
                    }
                    if (There_Was_A_Write_Error())
                    {
                        flag = false;
                    }
                    return flag;
                }
                string.Format("Error writing to USB device", new object[0]);
                return false;
            }
        }

        public static bool Send_EE2CtrlBlk_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 4;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_Event_Timer_Reset_Cmd()
        {
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = 3;
            buffer[2] = 1;
            buffer[3] = 0x20;
            return Send_Data_Packet_To_PICkitS(ref buffer);
        }

        public static bool Send_FlushCbuf2_Cmd()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 5;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_LED_State_Cmd(int p_LED_num, byte p_value)
        {
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[1] = 3;
            array[2] = 2;
            switch (p_LED_num)
            {
                case 1:
                    array[3] = 0x12;
                    break;

                case 2:
                    array[3] = 0x13;
                    break;

                default:
                    return false;
            }
            array[4] = p_value;
            array[5] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref array);
            Update_Status_Packet();
            return flag;
        }

        public static bool Send_Script_To_PICkitS(ref byte[] p_send_byte_array)
        {
            lock (m_priv_write_script_lock)
            {
                m_data_buffer_is_empty = false;
                bool flag = false;
                int num = 0;
                uint num2 = 0;
                uint index = 0;
                byte num4 = 0;
                uint num5 = 0;
                uint num6 = p_send_byte_array[2];
                uint num7 = 0;
                uint num8 = 0;
                byte[] array = new byte[0x41];
                m_last_script_array_mutex.WaitOne();
                m_last_script_array_byte_count = (byte) (num6 + 2);
                Array.Clear(m_last_script_array, 0, m_last_script_array.Length);
                for (int i = 0; i < m_last_script_array_byte_count; i++)
                {
                    m_last_script_array[i] = p_send_byte_array[i + 1];
                }
                Utilities.m_flags.g_PKSA_has_completed_script.Reset();
                m_last_script_array_mutex.ReleaseMutex();
                num2 = num6 / 0x3e;
                num4 = (byte) (num6 % 0x3e);
                if (num4 != 0)
                {
                    num2++;
                }
                for (num = 0; num < num2; num++)
                {
                    Array.Clear(array, 0, array.Length);
                    array[0] = p_send_byte_array[0];
                    array[1] = p_send_byte_array[1];
                    if (num2 != 1)
                    {
                        num8 = 3;
                        if ((num == (num2 - 1)) && (num4 != 0))
                        {
                            num7 = (uint) (num4 + 4);
                            array[2] = num4;
                        }
                        else
                        {
                            num7 = 0x41;
                            array[2] = 0x3e;
                        }
                    }
                    else
                    {
                        num7 = num6 + 4;
                        num8 = 2;
                    }
                    for (index = num8; index < num7; index++)
                    {
                        array[index] = p_send_byte_array[num5 + index];
                    }
                    if (num2 != 1)
                    {
                        num5 += 0x3e;
                        if (num == (num2 - 1))
                        {
                            num5++;
                        }
                    }
                    if (array[1] == 3)
                    {
                        if (!there_is_room_in_cbuf1(array[2]))
                        {
                            return false;
                        }
                        m_cbuf1_avail_bytes_mutex.WaitOne();
                        m_cbuf1_avail_bytes = (byte) (m_cbuf1_avail_bytes - array[2]);
                        m_cbuf1_avail_bytes_mutex.ReleaseMutex();
                        if (!Send_Data_Packet_To_PICkitS(ref array))
                        {
                            return false;
                        }
                        if (There_Was_A_Write_Error())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Send_Data_Packet_To_PICkitS(ref array))
                        {
                            return false;
                        }
                        if (There_Was_A_Write_Error())
                        {
                            return false;
                        }
                    }
                }
                if (array[1] == 3)
                {
                    if (m_use_script_timeout)
                    {
                        if (Utilities.m_flags.g_PKSA_has_completed_script.WaitOne(m_universal_timeout, false) && Update_Status_Packet())
                        {
                            uint num10 = 0;
                            if (!Status.There_Is_A_Status_Error(ref num10))
                            {
                                flag = true;
                            }
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
                m_data_buffer_is_empty = true;
                return flag;
            }
        }

        public static bool Send_Special_Status_Request()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 11;
            m_command_byte_array[2] = 0xab;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            if (!flag)
            {
                flag = false;
            }
            return flag;
        }

        public static bool Send_Status_Request()
        {
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 2;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            if (!flag)
            {
                flag = false;
            }
            return flag;
        }

        public static bool Send_Warm_Reset_Cmd()
        {
            USBRead.m_EVENT_TIME_ROLLOVER = 0.0;
            USBRead.m_RUNNING_TIME = 0.0;
            m_command_byte_array[0] = 0;
            m_command_byte_array[1] = 1;
            m_command_byte_array[2] = 1;
            m_command_byte_array[3] = 0;
            m_command_byte_array[4] = 0;
            bool flag = Send_Data_Packet_To_PICkitS(ref m_command_byte_array);
            Update_Status_Packet();
            return flag;
        }

        private static bool there_is_room_in_cbuf1(byte p_num_bytes_to_write)
        {
            bool flag = false;
            int num = 0;
            if (p_num_bytes_to_write > 0xff)
            {
                return flag;
            }
            if (p_num_bytes_to_write > m_cbuf1_avail_bytes)
            {
                while ((p_num_bytes_to_write >= m_cbuf1_avail_bytes) && (num++ < 6))
                {
                    m_cbuf1_avail_bytes_mutex.WaitOne();
                    m_cbuf1_avail_bytes = this_many_bytes_are_actually_available_in_cbuf1();
                    m_cbuf1_avail_bytes_mutex.ReleaseMutex();
                    if (p_num_bytes_to_write < m_cbuf1_avail_bytes)
                    {
                        return true;
                    }
                    Thread.Sleep(100);
                }
                return flag;
            }
            return true;
        }

        public static bool There_Was_A_Write_Error()
        {
            return m_we_had_an_error_writing;
        }

        private static byte this_many_bytes_are_actually_available_in_cbuf1()
        {
            byte num = 0;
            if (Update_Special_Status_Packet())
            {
                Utilities.m_flags.g_status_packet_mutex.WaitOne();
                num = Constants.STATUS_PACKET_DATA[0x36];
                Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            }
            return num;
        }

        public static bool Transaction_Is_Complete()
        {
            bool flag = false;
            if ((We_Are_Done_Writing_Data() && !USBRead.m_read_thread_is_processing_a_USB_packet) && Update_Status_Packet())
            {
                Utilities.m_flags.g_status_packet_mutex.WaitOne();
                if (((Constants.STATUS_PACKET_DATA[0x25] & 1) == 0) && (Constants.STATUS_PACKET_DATA[0x37] == 0))
                {
                    flag = true;
                }
                Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            }
            return flag;
        }

        public static bool Update_Special_Status_Packet()
        {
            bool flag = false;
            if ((Utilities.m_flags.HID_read_handle != IntPtr.Zero) && USBRead.Read_Thread_Is_Active())
            {
                Utilities.m_flags.g_special_status_request_event.Reset();
                if (Send_Special_Status_Request() && Utilities.m_flags.g_special_status_request_event.WaitOne(0x7d0, false))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool Update_Status_Packet()
        {
            bool flag = false;
            if ((Utilities.m_flags.HID_read_handle != IntPtr.Zero) && USBRead.Read_Thread_Is_Active())
            {
                Utilities.m_flags.g_status_packet_data_update_event.Reset();
                if (Send_Status_Request() && Utilities.m_flags.g_status_packet_data_update_event.WaitOne(0x7d0, false))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private static bool We_Are_Done_Writing_Data()
        {
            return m_data_buffer_is_empty;
        }

        public static bool write_and_verify_config_block(ref byte[] p_control_block_data, ref string p_result_str, bool p_perform_warm_and_cold_reset, ref string p_cb_data_str)
        {
            bool flag = false;
            bool flag2 = false;
            int index = 0;
            if (p_perform_warm_and_cold_reset)
            {
                Send_Cold_Reset_Cmd();
            }
            flag = Send_Data_Packet_To_PICkitS(ref p_control_block_data);
            if (p_perform_warm_and_cold_reset)
            {
                Send_Warm_Reset_Cmd();
            }
            if (!flag)
            {
                p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            if (!Update_Status_Packet())
            {
                p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            index = 7;
            while (index < 0x1f)
            {
                if (Constants.STATUS_PACKET_DATA[index] != p_control_block_data[index - 5])
                {
                    p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", index - 7, Constants.STATUS_PACKET_DATA[index], p_control_block_data[index - 5]);
                    break;
                }
                index++;
            }
            if (index == 0x1f)
            {
                flag2 = true;
                p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
                p_cb_data_str = "";
                for (index = 7; index < 0x1f; index++)
                {
                    p_cb_data_str = p_cb_data_str + string.Format("{0:X2} ", Constants.STATUS_PACKET_DATA[index]);
                }
            }
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            return flag2;
        }

        public static bool write_and_verify_LIN_config_block(ref byte[] p_control_block_data, ref string p_result_str, bool p_perform_warm_and_cold_reset, ref string p_cb_data_str)
        {
            bool flag = false;
            bool flag2 = false;
            int index = 0;
            flag = Send_Data_Packet_To_PICkitS(ref p_control_block_data);
            Device.Clear_Status_Errors();
            if (!flag)
            {
                p_result_str = string.Format("Error sending config packet - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            if (!Update_Status_Packet())
            {
                p_result_str = string.Format("Error requesting config verification - Config Block may not be updated correctly", new object[0]);
                return flag2;
            }
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            index = 7;
            while (index < 0x1f)
            {
                if (Constants.STATUS_PACKET_DATA[index] != p_control_block_data[index - 5])
                {
                    p_result_str = string.Format("Byte {0} failed verification in config block write.\n Value reads {1:X2}, but should be {2:X2}.", index - 7, Constants.STATUS_PACKET_DATA[index], p_control_block_data[index - 5]);
                    break;
                }
                index++;
            }
            if (index == 0x1f)
            {
                flag2 = true;
                p_result_str = string.Format("PICkit Serial Analyzer correctly updated.", new object[0]);
                p_cb_data_str = "";
                for (index = 7; index < 0x1f; index++)
                {
                    p_cb_data_str = p_cb_data_str + string.Format("{0:X2} ", Constants.STATUS_PACKET_DATA[index]);
                }
            }
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            return flag2;
        }

        public static bool Write_Thread_Is_Active()
        {
            return m_we_are_in_write_loop;
        }

        public static void Write_USB_Thread()
        {
            m_we_are_in_write_loop = true;
            m_write_thread_has_started_up_event.Set();
            bool flag = false;
            int numBytesWritten = 0;
            while (m_we_are_in_write_loop)
            {
                if (m_ready_to_write_event.WaitOne(500, false))
                {
                    flag = Utilities.WriteFile(Utilities.m_flags.HID_write_handle, Utilities.m_flags.write_buffer, Utilities.m_flags.orbl, ref numBytesWritten, 0);
                    m_have_written_event.Set();
                    if (!flag || (numBytesWritten != Utilities.m_flags.orbl))
                    {
                        m_we_had_an_error_writing = true;
                    }
                    else
                    {
                        m_we_had_an_error_writing = false;
                    }
                }
            }
        }

        public delegate void GUINotifierLockUp();
    }
}

