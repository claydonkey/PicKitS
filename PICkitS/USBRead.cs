namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class USBRead
    {
        internal static uint m_cb2_array_tag_index;
        internal static byte[] m_cbuf2_data_array = new byte[0x5000];
        internal static volatile uint m_cbuf2_data_array_index;
        internal static Mutex m_cbuf2_data_array_mutex = new Mutex(false);
        private static volatile uint m_cbuf2_packet_byte_count;
        private static double m_EVENT_TIME_CONSTANT = 0.000409;
        internal static double m_EVENT_TIME_ROLLOVER = 0.0;
        private static double m_EVENT_TIME_ROLLOVER_CONSTANT = 26.8;
        private static bool m_grab_next_time_marker = false;
        public static bool m_OK_to_send_data = true;
        public static AutoResetEvent m_packet_is_copied = null;
        public static EventWaitHandle m_packet_is_ready = null;
        private static volatile bool m_process_data;
        internal static byte[] m_raw_cbuf2_data_array = new byte[0x5000];
        private static volatile uint m_raw_cbuf2_data_array_index;
        private static Thread m_read_thread;
        public static volatile bool m_read_thread_is_processing_a_USB_packet;
        public static bool m_ready_to_notify = false;
        private static volatile uint m_required_byte_count;
        internal static double m_RUNNING_TIME = 0.0;
        private static bool m_special_status_requested = false;
        internal static Mutex m_usb_packet_mutex = new Mutex(false);
        private static bool m_user_has_created_synchro_objects = false;
        private static volatile bool m_we_are_in_read_loop = false;
        private static bool m_we_need_next_packet_to_continue;
        private const byte TAG_CBUF_1_READ = 0x85;
        private const byte TAG_CBUF_2_READ = 0x86;
        private const byte TAG_CBUF_3_READ = 0x87;
        public const byte TAG_COMMON_DATA_BYTE_V = 0x10;
        public const byte TAG_COMMON_DATA_BYTES_V = 0x11;
        public const byte TAG_COMMON_EVENT_END_OF_SCRIPT_V = 0x1b;
        public const byte TAG_COMMON_EVENT_MACRO_ABORT_V = 0x17;
        public const byte TAG_COMMON_EVENT_MACRO_DONE_V = 0x15;
        public const byte TAG_COMMON_EVENT_MACRO_LOOP_V = 0x12;
        public const byte TAG_COMMON_EVENT_MACRO_ROLLOVER_V = 0x16;
        public const byte TAG_COMMON_EVENT_STATUS_ERROR_V = 0x1a;
        public const byte TAG_COMMON_EVENT_TIME_ROLLOVER_V = 20;
        public const byte TAG_COMMON_EVENT_TIME_V = 0x13;
        public const byte TAG_COMMON_EVENT_TIMEOUT_AB0_V = 0x18;
        public const byte TAG_COMMON_EVENT_TIMEOUT_AB1_V = 0x19;
        public const byte TAG_COMMON_MARKER_V = 0x1c;
        private const byte TAG_CTRL_BLK_READ = 130;
        private const byte TAG_EOD_READ = 0x80;
        private const byte TAG_FW_VER = 0x81;
        public const byte TAG_I2C_EVENT_ACK_RX_V = 0x85;
        public const byte TAG_I2C_EVENT_ACK_TX_V = 0x83;
        public const byte TAG_I2C_EVENT_BYTE_RX_V = 0x88;
        public const byte TAG_I2C_EVENT_BYTE_TX_V = 0x87;
        public const byte TAG_I2C_EVENT_NACK_RX_V = 0x86;
        public const byte TAG_I2C_EVENT_NACK_TX_V = 0x84;
        public const byte TAG_I2C_EVENT_RESTART_TX_V = 130;
        public const byte TAG_I2C_EVENT_START_TX_V = 0x80;
        public const byte TAG_I2C_EVENT_STATUS_ERR_V = 0x8a;
        public const byte TAG_I2C_EVENT_STOP_TX_V = 0x81;
        public const byte TAG_I2C_EVENT_XACT_ERR_V = 0x89;
        public const byte TAG_I2CS_EVENT_ACK_RCV_V = 0xc3;
        public const byte TAG_I2CS_EVENT_ADDR_RX_V = 0xc0;
        public const byte TAG_I2CS_EVENT_DATA_RQ_V = 200;
        public const byte TAG_I2CS_EVENT_DATA_RX_V = 0xc1;
        public const byte TAG_I2CS_EVENT_DATA_TX_V = 0xc2;
        public const byte TAG_I2CS_EVENT_NACK_RCV_V = 0xc4;
        public const byte TAG_I2CS_EVENT_REG_DATA_V = 0xcb;
        public const byte TAG_I2CS_EVENT_REG_READ_V = 0xc9;
        public const byte TAG_I2CS_EVENT_REG_WRITE_V = 0xca;
        public const byte TAG_I2CS_EVENT_STATUS_ERROR_V = 0xc7;
        public const byte TAG_I2CS_EVENT_STOP_V = 0xc6;
        public const byte TAG_LIN_EVENT_AUTO_BAUD_V = 0x85;
        public const byte TAG_LIN_EVENT_BREAK_RX_V = 0x83;
        public const byte TAG_LIN_EVENT_BREAK_TX_V = 0x84;
        public const byte TAG_LIN_EVENT_BYTE_RX_V = 0x80;
        public const byte TAG_LIN_EVENT_BYTE_TX_V = 0x81;
        public const byte TAG_LIN_EVENT_CHECKSUM_ERR_V = 0x86;
        public const byte TAG_LIN_EVENT_ID_PARITY_ERR_V = 0x87;
        public const byte TAG_LIN_EVENT_SLAVE_PROFILE_ID_DATA_V = 0x88;
        public const byte TAG_LIN_EVENT_STATUS_ERR_V = 130;
        private const byte TAG_PACKET_ID = 0x88;
        public const byte TAG_SPI_EVENT_BYTE_RX_V = 0x81;
        public const byte TAG_SPI_EVENT_BYTE_TX_V = 0x80;
        public const byte TAG_SPI_EVENT_STATUS_ERROR_V = 130;
        private const byte TAG_STATUS_BLOCK = 0x83;
        private const byte TAG_STATUS_CBUF = 0x84;
        public const byte TAG_USART_EVENT_BREAK_TX_V = 0x83;
        public const byte TAG_USART_EVENT_BYTE_RX_V = 0x81;
        public const byte TAG_USART_EVENT_BYTE_TX_V = 0x80;
        public const byte TAG_USART_EVENT_STATUS_ERROR_V = 130;

        public static  event DataNotifier DataAvailable;

        public static  event USBNotifier USBDataAvailable;

        private static void add_data_to_cbuf2_data_array(ref byte[] p_source, uint p_index, int p_num_bytes)
        {
            int num = 0;
            m_cbuf2_data_array_mutex.WaitOne();
            while ((m_cbuf2_data_array_index < m_cbuf2_data_array.Length) && (num < p_num_bytes))
            {
                m_cbuf2_data_array[m_cbuf2_data_array_index] = p_source[(int) ((IntPtr) (p_index + num))];
                num++;
                m_cbuf2_data_array_index++;
            }
            m_cbuf2_data_array_mutex.ReleaseMutex();
            if ((m_cbuf2_data_array_index >= m_required_byte_count) && (m_required_byte_count != 0))
            {
                Utilities.m_flags.g_data_arrived_event.Set();
            }
            m_ready_to_notify = true;
        }

        public static void Clear_Data_Array(uint p_required_byte_count)
        {
            m_cbuf2_data_array_mutex.WaitOne();
            Array.Clear(m_cbuf2_data_array, 0, m_cbuf2_data_array.Length);
            m_cbuf2_data_array_index = 0;
            m_required_byte_count = p_required_byte_count;
            m_cbuf2_data_array_mutex.ReleaseMutex();
            Utilities.m_flags.g_data_arrived_event.Reset();
        }

        public static void Clear_Raw_Data_Array()
        {
            Array.Clear(m_raw_cbuf2_data_array, 0, m_raw_cbuf2_data_array.Length);
            m_raw_cbuf2_data_array_index = 0;
            m_cb2_array_tag_index = 0;
        }

        public static void Create_Single_Sync_object()
        {
            m_packet_is_copied = new AutoResetEvent(false);
        }

        public static void Create_Synchronization_Object(ref string p_ready_to_copy)
        {
            string str = DateTime.Now.ToLongTimeString();
            string name = "PacketReady" + str;
            m_packet_is_ready = new EventWaitHandle(false, EventResetMode.AutoReset, name);
            m_packet_is_copied = new AutoResetEvent(false);
            p_ready_to_copy = name;
            m_user_has_created_synchro_objects = true;
        }

        public static void DataAvailable_Should_Fire(bool p_value)
        {
            m_OK_to_send_data = p_value;
        }

        public static void Dispose_Of_Read_Objects()
        {
            m_cbuf2_data_array_mutex.Close();
            m_usb_packet_mutex.Close();
            m_packet_is_ready.Close();
        }

        public static void DLL_Should_Not_Process_Data()
        {
            m_process_data = false;
        }

        public static void DLL_Should_Process_Data()
        {
            m_process_data = true;
        }

        public static void Get_USB_Data_Packet(ref byte[] p_data)
        {
            if (Utilities.m_flags.read_buffer[0x40] == 0)
            {
                Utilities.m_flags.read_buffer[0x40] = 0;
            }
            m_usb_packet_mutex.WaitOne();
            for (int i = 0; i < Utilities.m_flags.read_buffer.Length; i++)
            {
                p_data[i] = Utilities.m_flags.read_buffer[i];
            }
            m_usb_packet_mutex.ReleaseMutex();
            m_packet_is_copied.Set();
        }

        public static ushort Get_USB_IRBL()
        {
            return Utilities.m_flags.irbl;
        }

        public static void Initialize_Read_Objects()
        {
            m_we_are_in_read_loop = false;
            Array.Clear(m_raw_cbuf2_data_array, 0, m_raw_cbuf2_data_array.Length);
            Array.Clear(m_cbuf2_data_array, 0, m_cbuf2_data_array.Length);
            m_cbuf2_data_array_index = 0;
            m_raw_cbuf2_data_array_index = 0;
            m_cb2_array_tag_index = 0;
            m_cbuf2_packet_byte_count = 0;
            m_required_byte_count = 0;
            m_process_data = true;
            m_we_need_next_packet_to_continue = false;
        }

        public static bool Kick_Off_Read_Thread()
        {
            bool flag = true;
            if (!m_we_are_in_read_loop)
            {
                m_read_thread = new Thread(new ThreadStart(USBRead.Read_USB_Thread));
                m_read_thread.IsBackground = true;
                m_read_thread.Start();
                return flag;
            }
            return false;
        }

        public static void Kill_Read_Thread()
        {
            m_we_are_in_read_loop = false;
            if (Utilities.g_comm_mode == Utilities.COMM_MODE.MTOUCH2)
            {
                mTouch2.Send_MT2_RD_STATUS_Command();
            }
            else
            {
                USBWrite.Send_Status_Request();
            }
        }

        private static void process_cbuf2_data(ref byte[] p_data, int p_index)
        {
            m_cbuf2_packet_byte_count = p_data[p_index + 1];
            if (m_cbuf2_packet_byte_count != null)
            {
                if (!m_we_need_next_packet_to_continue)
                {
                    Clear_Raw_Data_Array();
                    if (Utilities.g_comm_mode == Utilities.COMM_MODE.LIN)
                    {
                        Clear_Data_Array(0);
                    }
                }
                for (int i = p_index + 2; i < ((p_index + 2) + m_cbuf2_packet_byte_count); i++)
                {
                    if (m_raw_cbuf2_data_array_index >= m_raw_cbuf2_data_array.Length)
                    {
                        break;
                    }
                    m_raw_cbuf2_data_array_index++;
                    m_raw_cbuf2_data_array[m_raw_cbuf2_data_array_index] = p_data[i];
                }
                process_m_cbuf2_array_data();
            }
        }

        private static void process_common_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x10:
                case 0x12:
                case 0x1a:
                case 0x1c:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
                    {
                        case 0x10:
                            add_data_to_cbuf2_data_array(ref m_raw_cbuf2_data_array, m_cb2_array_tag_index + 1, 1);
                            if (((Utilities.g_comm_mode == Utilities.COMM_MODE.LIN) && !LIN.m_working_frame.BuildState.we_timed_out) && LIN.m_working_frame.BuildState.we_are_building_a_frame)
                            {
                                if (!LIN.m_working_frame.BuildState.we_have_an_id)
                                {
                                    LIN.m_working_frame.FrameInfo.FrameID = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))];
                                    LIN.m_working_frame.BuildState.we_have_an_id = true;
                                }
                                else
                                {
                                    if (LIN.m_working_frame.FrameInfo.bytecount < 9)
                                    {
                                        byte num4;
                                        LIN.m_working_frame.FrameInfo.bytecount = (byte) ((num4 = LIN.m_working_frame.FrameInfo.bytecount) + 1);
                                        LIN.m_working_frame.FrameInfo.FrameData[num4] = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))];
                                    }
                                    if (LIN.m_working_frame.FrameInfo.bytecount == 9)
                                    {
                                        LIN.finalize_working_frame();
                                    }
                                }
                            }
                            goto Label_01C3;

                        case 0x1c:
                            if (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))] == 0x77)
                            {
                                Utilities.m_flags.g_PKSA_has_completed_script.Set();
                            }
                            goto Label_01C3;
                    }
                    break;

                case 0x11:
                    if (((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) - 2) < m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))])
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    add_data_to_cbuf2_data_array(ref m_raw_cbuf2_data_array, m_cb2_array_tag_index + 2, m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))]);
                    m_cb2_array_tag_index += (uint) (2 + m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))]);
                    return;

                case 0x13:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 3)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    if (((Utilities.g_comm_mode == Utilities.COMM_MODE.LIN) && LIN.m_working_frame.BuildState.we_are_building_a_frame) && m_grab_next_time_marker)
                    {
                        double num = ((m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] << 8) + m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))]) * m_EVENT_TIME_CONSTANT;
                        m_RUNNING_TIME = m_EVENT_TIME_ROLLOVER + num;
                        LIN.m_working_frame.FrameInfo.time = m_RUNNING_TIME;
                        m_grab_next_time_marker = false;
                    }
                    m_cb2_array_tag_index += 3;
                    return;

                case 20:
                    m_EVENT_TIME_ROLLOVER += m_EVENT_TIME_ROLLOVER_CONSTANT;
                    m_cb2_array_tag_index++;
                    return;

                case 0x15:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 3)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 3;
                    return;

                case 0x16:
                    m_cb2_array_tag_index++;
                    return;

                case 0x17:
                    m_cb2_array_tag_index++;
                    return;

                case 0x18:
                    m_cb2_array_tag_index++;
                    return;

                case 0x19:
                    m_cb2_array_tag_index++;
                    return;

                case 0x1b:
                    m_cb2_array_tag_index++;
                    return;

                default:
                    p_error = true;
                    return;
            }
        Label_01C3:
            m_cb2_array_tag_index += 2;
        }

        private static void process_i2c_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x80:
                    m_cb2_array_tag_index++;
                    return;

                case 0x81:
                    m_cb2_array_tag_index++;
                    return;

                case 130:
                    m_cb2_array_tag_index++;
                    return;

                case 0x83:
                    m_cb2_array_tag_index++;
                    return;

                case 0x84:
                    m_cb2_array_tag_index++;
                    return;

                case 0x85:
                    m_cb2_array_tag_index++;
                    return;

                case 0x86:
                    m_cb2_array_tag_index++;
                    return;

                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;
            }
            p_error = true;
        }

        private static void process_i2c_slave_data(ref bool p_error)
        {
            I2CS.m_slave_address_was_just_set = false;
            I2CS.m_master_is_waiting_for_data = false;
            I2CS.m_stop_command_issued = false;
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc7:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
                    {
                        case 0xc0:
                            I2CS.m_previous_slave_addr_received = I2CS.m_last_slave_addr_received;
                            I2CS.m_last_slave_addr_received = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))];
                            I2CS.m_slave_address_was_just_set = true;
                            I2CS.issue_event();
                            goto Label_0131;

                        case 0xc1:
                            add_data_to_cbuf2_data_array(ref m_raw_cbuf2_data_array, m_cb2_array_tag_index + 1, 1);
                            goto Label_0131;

                        case 0xc7:
                            I2CS.issue_error();
                            goto Label_0131;
                    }
                    break;

                case 0xc3:
                    m_cb2_array_tag_index++;
                    return;

                case 0xc4:
                    m_cb2_array_tag_index++;
                    return;

                case 0xc6:
                    m_cb2_array_tag_index++;
                    I2CS.m_stop_command_issued = true;
                    I2CS.issue_event();
                    I2CS.reset_buffers();
                    return;

                case 200:
                    m_cb2_array_tag_index++;
                    I2CS.m_master_is_waiting_for_data = true;
                    I2CS.issue_event();
                    return;

                case 0xc9:
                case 0xca:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 3)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
                    {
                    }
                    m_cb2_array_tag_index += 3;
                    return;

                case 0xcb:
                    m_cb2_array_tag_index += (uint) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 3))] + 4);
                    return;

                default:
                    p_error = true;
                    return;
            }
        Label_0131:
            m_cb2_array_tag_index += 2;
        }

        private static void process_lin_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x80:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;

                case 0x81:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;

                case 130:
                    LIN.m_working_frame.BuildState.we_had_a_status_error = true;
                    if (!LIN.m_working_frame.BuildState.we_timed_out)
                    {
                        LIN.finalize_working_frame();
                        Device.Clear_Status_Errors();
                    }
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) >= 2)
                    {
                        m_cb2_array_tag_index += 2;
                        return;
                    }
                    m_we_need_next_packet_to_continue = true;
                    return;

                case 0x83:
                    if (!LIN.m_working_frame.BuildState.we_timed_out)
                    {
                        if (LIN.m_working_frame.BuildState.we_are_building_a_frame)
                        {
                            LIN.m_working_frame.BuildState.next_frame_header_received = true;
                            LIN.finalize_working_frame();
                        }
                        LIN.m_working_frame.BuildState.we_are_building_a_frame = true;
                        m_grab_next_time_marker = true;
                        LIN.reset_LIN_timeout();
                    }
                    m_cb2_array_tag_index++;
                    return;

                case 0x84:
                    m_cb2_array_tag_index++;
                    return;

                case 0x85:
                    if (LIN.m_working_frame.BuildState.we_are_building_a_frame)
                    {
                        LIN.m_working_frame.FrameInfo.baud = (ushort) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] << 8));
                        if (LIN.m_use_baud_rate_timeout)
                        {
                            LIN.calculate_new_baud_dependent_onreceive_timeout(LIN.m_working_frame.FrameInfo.baud);
                        }
                    }
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) >= 3)
                    {
                        m_cb2_array_tag_index += 3;
                        return;
                    }
                    m_we_need_next_packet_to_continue = true;
                    return;

                case 0x86:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;

                case 0x87:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;

                case 0x88:
                    LIN.m_slave_profile_id.FrameID = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))];
                    LIN.m_slave_profile_id.ByteCount = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))];
                    for (int i = 0; i < LIN.m_slave_profile_id.ByteCount; i++)
                    {
                        LIN.m_slave_profile_id.Data[i] = m_raw_cbuf2_data_array[(int) ((IntPtr) ((m_cb2_array_tag_index + 3) + i))];
                    }
                    m_cb2_array_tag_index += (uint) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 3);
                    LIN.m_slave_profile_id_read.Set();
                    return;
            }
            p_error = true;
        }

        private static bool process_m_cbuf2_array_data()
        {
            bool flag2 = false;
            m_we_need_next_packet_to_continue = false;
            while (((m_cb2_array_tag_index < m_raw_cbuf2_data_array_index) && !flag2) && !m_we_need_next_packet_to_continue)
            {
                if ((m_raw_cbuf2_data_array[m_cb2_array_tag_index] & 0x10) == 0x10)
                {
                    process_common_data(ref flag2);
                    continue;
                }
                if (((m_raw_cbuf2_data_array[m_cb2_array_tag_index] & 0x80) != 0x80) && (Utilities.g_comm_mode != Utilities.COMM_MODE.MTOUCH2))
                {
                    flag2 = true;
                    continue;
                }
                switch (Utilities.g_comm_mode)
                {
                    case Utilities.COMM_MODE.I2C_M:
                    {
                        process_i2c_data(ref flag2);
                        continue;
                    }
                    case Utilities.COMM_MODE.SPI_M:
                    case Utilities.COMM_MODE.SPI_S:
                    case Utilities.COMM_MODE.UWIRE:
                    {
                        process_spi_data(ref flag2);
                        continue;
                    }
                    case Utilities.COMM_MODE.USART_A:
                    case Utilities.COMM_MODE.USART_SM:
                    case Utilities.COMM_MODE.USART_SS:
                    {
                        process_usart_data(ref flag2);
                        continue;
                    }
                    case Utilities.COMM_MODE.I2C_S:
                    {
                        process_i2c_slave_data(ref flag2);
                        continue;
                    }
                    case Utilities.COMM_MODE.LIN:
                    {
                        process_lin_data(ref flag2);
                        continue;
                    }
                    case Utilities.COMM_MODE.MTOUCH2:
                    {
                        process_mtouch2_data(ref flag2);
                        continue;
                    }
                }
                flag2 = true;
            }
            if (((DataAvailable != null) && m_ready_to_notify) && m_OK_to_send_data)
            {
                DataAvailable();
                m_ready_to_notify = false;
            }
            return false;
        }

        private static void process_mtouch2_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x41:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 0x10)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    mTouch2.m_sensor_status_mutex.WaitOne();
                    mTouch2.m_data_status.comm_fw_ver = (ushort) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 2))] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 3))] << 8));
                    mTouch2.m_data_status.touch_fw_ver = (ushort) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 4))] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 5))] << 8));
                    mTouch2.m_data_status.hardware_id = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 6))];
                    mTouch2.m_data_status.max_num_sensors = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 7))];
                    mTouch2.m_data_status.broadcast_group_id = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 8))];
                    mTouch2.m_data_status.broadcast_enable_flags.trip = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 1) > 0;
                    mTouch2.m_data_status.broadcast_enable_flags.guardband = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 2) > 0;
                    mTouch2.m_data_status.broadcast_enable_flags.raw = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 4) > 0;
                    mTouch2.m_data_status.broadcast_enable_flags.avg = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 8) > 0;
                    mTouch2.m_data_status.broadcast_enable_flags.detect_flags = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 0x10) > 0;
                    mTouch2.m_data_status.broadcast_enable_flags.status = (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 9))] & 0x80) > 0;
                    mTouch2.m_data_status.time_interval = (ushort) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 10))] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_raw_cbuf2_data_array_index + 11))] << 8));
                    mTouch2.m_sensor_status_mutex.ReleaseMutex();
                    mTouch2.m_status_data_is_ready.Set();
                    m_cb2_array_tag_index += 0x10;
                    return;

                case 0x42:
                {
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 7)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    int num = 0;
                    mTouch2.m_sensor_data_mutex.WaitOne();
                    for (uint i = m_cb2_array_tag_index + 2; i < ((m_cb2_array_tag_index + 2) + 5); i++)
                    {
                        mTouch2.m_detect_values[num++] = m_raw_cbuf2_data_array[i];
                    }
                    mTouch2.m_sensor_data_mutex.ReleaseMutex();
                    mTouch2.m_detect_data_is_ready.Set();
                    m_cb2_array_tag_index += 7;
                    return;
                }
                case 0x43:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 2))
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    for (int j = 0; j < (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 1); j++)
                    {
                        mTouch2.m_user_sensor_values[j] = m_raw_cbuf2_data_array[(int) ((IntPtr) ((m_cb2_array_tag_index + 2) + j))];
                    }
                    mTouch2.m_user_sensor_values_are_ready.Set();
                    m_cb2_array_tag_index += (uint) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 3);
                    return;

                case 0x44:
                case 0x45:
                case 70:
                case 0x47:
                case 0x48:
                case 0x49:
                {
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 3))
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    int num4 = 0;
                    int num5 = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] / 2;
                    mTouch2.m_current_sensor_id = m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 1))];
                    mTouch2.m_num_current_sensors = (byte) num5;
                    switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
                    {
                        case 0x44:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint k = m_cb2_array_tag_index + 3; k < ((m_cb2_array_tag_index + 3) + (num5 * 2)); k += 2)
                            {
                                mTouch2.m_trp_values[num4++] = (ushort) (m_raw_cbuf2_data_array[k] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (k + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            mTouch2.m_trip_data_is_ready.Set();
                            goto Label_0619;

                        case 0x45:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint m = m_cb2_array_tag_index + 3; m < ((m_cb2_array_tag_index + 3) + (num5 * 2)); m += 2)
                            {
                                mTouch2.m_gdb_values[num4++] = (ushort) (m_raw_cbuf2_data_array[m] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (m + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            mTouch2.m_gdb_data_is_ready.Set();
                            goto Label_0619;

                        case 70:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint n = m_cb2_array_tag_index + 3; n < ((m_cb2_array_tag_index + 3) + (num5 * 2)); n += 2)
                            {
                                mTouch2.m_raw_values[num4++] = (ushort) (m_raw_cbuf2_data_array[n] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (n + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            goto Label_0619;

                        case 0x47:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint num9 = m_cb2_array_tag_index + 3; num9 < ((m_cb2_array_tag_index + 3) + (num5 * 2)); num9 += 2)
                            {
                                mTouch2.m_avg_values[num4++] = (ushort) (m_raw_cbuf2_data_array[num9] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (num9 + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            goto Label_0619;

                        case 0x48:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint num10 = m_cb2_array_tag_index + 3; num10 < ((m_cb2_array_tag_index + 3) + (num5 * 2)); num10 += 2)
                            {
                                mTouch2.m_au1_values[num4++] = (ushort) (m_raw_cbuf2_data_array[num10] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (num10 + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            goto Label_0619;

                        case 0x49:
                            mTouch2.m_sensor_data_mutex.WaitOne();
                            for (uint num11 = m_cb2_array_tag_index + 3; num11 < ((m_cb2_array_tag_index + 3) + (num5 * 2)); num11 += 2)
                            {
                                mTouch2.m_au2_values[num4++] = (ushort) (m_raw_cbuf2_data_array[num11] + (m_raw_cbuf2_data_array[(int) ((IntPtr) (num11 + 1))] << 8));
                            }
                            mTouch2.m_sensor_data_mutex.ReleaseMutex();
                            goto Label_0619;
                    }
                    break;
                }
                default:
                    return;
            }
        Label_0619:
            m_cb2_array_tag_index += (uint) (m_raw_cbuf2_data_array[(int) ((IntPtr) (m_cb2_array_tag_index + 2))] + 3);
        }

        private static void process_spi_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x80:
                case 0x81:
                case 130:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;
            }
            p_error = true;
        }

        private static bool process_this_group(ref byte[] p_data, int p_index)
        {
            bool flag = true;
            int num = 0;
            switch (p_data[p_index])
            {
                case 0x80:
                case 0x85:
                case 0x87:
                case 0x88:
                    if (p_data[p_index + 1] != 0)
                    {
                        m_special_status_requested = true;
                    }
                    return flag;

                case 0x81:
                    Utilities.m_flags.g_status_packet_mutex.WaitOne();
                    Constants.STATUS_PACKET_DATA[3] = p_data[p_index + 1];
                    Constants.STATUS_PACKET_DATA[4] = p_data[p_index + 2];
                    Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
                    return flag;

                case 130:
                    Utilities.m_flags.g_status_packet_mutex.WaitOne();
                    for (num = 1; num <= 0x18; num++)
                    {
                        Constants.STATUS_PACKET_DATA[6 + num] = p_data[p_index + num];
                    }
                    Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
                    return flag;

                case 0x83:
                    Utilities.m_flags.g_status_packet_mutex.WaitOne();
                    for (num = 1; num <= 20; num++)
                    {
                        Constants.STATUS_PACKET_DATA[0x1f + num] = p_data[p_index + num];
                    }
                    Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
                    Utilities.Set_Comm_Mode(p_data[0x26], p_data[0x17]);
                    return flag;

                case 0x84:
                    Utilities.m_flags.g_status_packet_mutex.WaitOne();
                    for (num = 1; num <= 6; num++)
                    {
                        Constants.STATUS_PACKET_DATA[0x34 + num] = p_data[p_index + num];
                    }
                    Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
                    Utilities.m_flags.g_status_packet_data_update_event.Set();
                    if (m_special_status_requested)
                    {
                        m_special_status_requested = false;
                        Utilities.m_flags.g_special_status_request_event.Set();
                    }
                    return flag;

                case 0x86:
                    process_cbuf2_data(ref p_data, p_index);
                    return flag;
            }
            return false;
        }

        private static bool process_this_packet(ref byte[] p_packet)
        {
            bool flag = false;
            m_usb_packet_mutex.WaitOne();
            byte index = 1;
            bool flag2 = false;
            byte num1 = p_packet[1];
            while (index < 0x40)
            {
                switch (p_packet[index])
                {
                    case 0x80:
                        flag2 = true;
                        break;

                    case 0x81:
                    {
                        byte num3 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + 3);
                        break;
                    }
                    case 130:
                    {
                        byte num4 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + 0x19);
                        break;
                    }
                    case 0x83:
                    {
                        byte num5 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + 0x15);
                        break;
                    }
                    case 0x84:
                    {
                        byte num6 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + 7);
                        break;
                    }
                    case 0x85:
                    case 0x86:
                    case 0x87:
                    {
                        byte num7 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + ((byte) (2 + p_packet[index + 1])));
                        break;
                    }
                    case 0x88:
                    {
                        byte num8 = p_packet[index];
                        process_this_group(ref p_packet, index);
                        index = (byte) (index + 2);
                        break;
                    }
                    default:
                        flag = true;
                        break;
                }
                if (flag2)
                {
                    break;
                }
                if (flag || (index > 0x42))
                {
                    flag = true;
                    break;
                }
            }
            m_usb_packet_mutex.ReleaseMutex();
            return flag;
        }

        private static void process_usart_data(ref bool p_error)
        {
            switch (m_raw_cbuf2_data_array[m_cb2_array_tag_index])
            {
                case 0x80:
                case 0x81:
                case 130:
                    if ((m_raw_cbuf2_data_array_index - m_cb2_array_tag_index) < 2)
                    {
                        m_we_need_next_packet_to_continue = true;
                        return;
                    }
                    m_cb2_array_tag_index += 2;
                    return;

                case 0x83:
                    m_cb2_array_tag_index++;
                    return;
            }
            p_error = true;
        }

        public static bool Read_Thread_Is_Active()
        {
            return m_we_are_in_read_loop;
        }

        public static void Read_USB_Thread()
        {
            bool flag = true;
            int pNumberOfBytesRead = 0;
            m_we_are_in_read_loop = true;
            while (m_we_are_in_read_loop)
            {
                m_read_thread_is_processing_a_USB_packet = false;
                m_usb_packet_mutex.WaitOne();
                Array.Clear(Utilities.m_flags.read_buffer, 0, Utilities.m_flags.read_buffer.Length);
                flag = Utilities.ReadFile(Utilities.m_flags.HID_read_handle, Utilities.m_flags.read_buffer, Utilities.m_flags.irbl, ref pNumberOfBytesRead, 0);
                if (Utilities.m_flags.g_need_to_copy_bl_data)
                {
                    for (int i = 0; i < Utilities.m_flags.read_buffer.Length; i++)
                    {
                        Utilities.m_flags.bl_buffer[i] = Utilities.m_flags.read_buffer[i];
                    }
                    Utilities.m_flags.g_bl_data_arrived_event.Set();
                    Utilities.m_flags.g_need_to_copy_bl_data = false;
                }
                m_read_thread_is_processing_a_USB_packet = true;
                m_usb_packet_mutex.ReleaseMutex();
                if ((pNumberOfBytesRead != Utilities.m_flags.irbl) || !flag)
                {
                    break;
                }
                if (!m_we_are_in_read_loop)
                {
                    return;
                }
                if (m_user_has_created_synchro_objects)
                {
                    m_packet_is_ready.Set();
                    m_packet_is_copied.WaitOne();
                }
                if ((USBDataAvailable != null) && m_OK_to_send_data)
                {
                    USBDataAvailable();
                    m_packet_is_copied.WaitOne();
                }
                if (m_process_data)
                {
                    m_usb_packet_mutex.WaitOne();
                    process_this_packet(ref Utilities.m_flags.read_buffer);
                    m_usb_packet_mutex.ReleaseMutex();
                }
            }
        }

        public static bool reset_timer_params()
        {
            m_EVENT_TIME_ROLLOVER = 0.0;
            m_RUNNING_TIME = 0.0;
            return USBWrite.Send_Event_Timer_Reset_Cmd();
        }

        public static bool Retrieve_Data(ref byte[] p_data_array, uint p_num_bytes)
        {
            if (p_num_bytes > m_cbuf2_data_array_index)
            {
                return false;
            }
            m_cbuf2_data_array_mutex.WaitOne();
            for (int i = 0; i < p_num_bytes; i++)
            {
                p_data_array[i] = m_cbuf2_data_array[i];
            }
            for (int j = 0; j < (m_cbuf2_data_array_index - p_num_bytes); j++)
            {
                m_cbuf2_data_array[j] = m_cbuf2_data_array[(int) ((IntPtr) (p_num_bytes + j))];
            }
            m_cbuf2_data_array_index -= p_num_bytes;
            Array.Clear(m_cbuf2_data_array, (int)m_cbuf2_data_array_index, m_cbuf2_data_array.Length - ((int)m_cbuf2_data_array_index));
            m_cbuf2_data_array_mutex.ReleaseMutex();
            m_required_byte_count = 0;
            return true;
        }

        public static uint Retrieve_Data_Byte_Count()
        {
            return m_cbuf2_data_array_index;
        }

        public static bool Retrieve_Raw_Data(ref byte[] p_data_array, uint p_num_bytes)
        {
            if (p_num_bytes > m_raw_cbuf2_data_array_index)
            {
                return false;
            }
            for (int i = 0; i < p_num_bytes; i++)
            {
                p_data_array[i] = m_raw_cbuf2_data_array[i];
            }
            for (int j = 0; j < (m_raw_cbuf2_data_array_index - p_num_bytes); j++)
            {
                m_raw_cbuf2_data_array[j] = m_raw_cbuf2_data_array[(int) ((IntPtr) (p_num_bytes + j))];
            }
            m_raw_cbuf2_data_array_index -= p_num_bytes;
            if (m_cb2_array_tag_index < p_num_bytes)
            {
                m_cb2_array_tag_index = 0;
            }
            else
            {
                m_cb2_array_tag_index -= p_num_bytes;
            }
            Array.Clear(m_raw_cbuf2_data_array, (int)m_raw_cbuf2_data_array_index, m_raw_cbuf2_data_array.Length - ((int) m_raw_cbuf2_data_array_index));
            return true;
        }

        public static uint Retrieve_Raw_Data_Byte_Count()
        {
            return m_raw_cbuf2_data_array_index;
        }

        public delegate void DataNotifier();

        public delegate void USBNotifier();
    }
}

