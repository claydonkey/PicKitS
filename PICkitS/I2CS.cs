namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class I2CS
    {
        internal static ushort m_last_slave_addr_received = 0x200;
        internal static bool m_master_is_waiting_for_data = false;
        internal static ushort m_previous_slave_addr_received = 0x200;
        private static Thread m_read;
        private static Mutex m_read_mutex = new Mutex(false);
        private static Thread m_receive;
        private static Mutex m_receive_mutex = new Mutex(false);
        internal static bool m_slave_address_was_just_set = false;
        private static uint m_slave_array_index = 0;
        private const int m_slave_array_size = 0x3e8;
        private static byte[] m_slave_data_array = new byte[0x3e8];
        internal static byte m_start_data_addr_received = 0;
        internal static bool m_stop_command_issued = false;
        private static Thread m_write;
        private static Mutex m_write_mutex = new Mutex(false);
        private const ushort SLAVE_ADDR_RESET = 0x200;

        public static  event GUINotifierError Error;

        public static  event GUINotifierRead Read;

        public static  event GUINotifierReceive Receive;

        public static  event GUINotifierWrite Write;

        public static bool Configure_PICkitSerial_For_I2CSlave_Auto_Mode(byte p_slave_addr, byte p_slave_mask, byte p_array_byte_count, ref byte[] p_profile_array, ref string p_result_str)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0xff];
            byte[] buffer3 = new byte[0x41];
            bool flag2 = false;
            string str = "";
            int index = 0;
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer3, 0, buffer3.Length);
            Mode.update_status_packet_data(7, ref buffer3);
            buffer3[0x17] = 2;
            buffer3[0x1d] = p_slave_addr;
            buffer3[30] = p_slave_mask;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer3);
            USBWrite.Send_Cold_Reset_Cmd();
            flag = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
            Array.Clear(buffer2, 0, buffer2.Length);
            buffer2[0] = 0;
            buffer2[1] = 5;
            buffer2[2] = p_array_byte_count;
            for (int i = 3; i < (p_array_byte_count + 3); i++)
            {
                buffer2[i] = p_profile_array[i - 3];
            }
            USBWrite.Send_Script_To_PICkitS(ref buffer2);
            USBWrite.Send_Warm_Reset_Cmd();
            if (!flag)
            {
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

        public static bool Configure_PICkitSerial_For_I2CSlave_Default_Mode(byte p_slave_addr, byte p_slave_mask, byte p_read_byte_0_data, byte p_read_bytes_1_N_data)
        {
            bool flag = false;
            if (!Basic.Configure_PICkitSerial(7, true))
            {
                return flag;
            }
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
            buffer2[0x17] = 0;
            buffer2[0x1b] = p_read_bytes_1_N_data;
            buffer2[0x1c] = p_read_byte_0_data;
            buffer2[0x1d] = p_slave_addr;
            buffer2[30] = p_slave_mask;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Configure_PICkitSerial_For_I2CSlave_Interactive_Mode(byte p_slave_addr, byte p_slave_mask)
        {
            bool flag = false;
            string str = "";
            string str2 = "";
            byte num = 0;
            byte num2 = 0;
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (!Basic.Get_Status_Packet(ref buffer2))
            {
                return false;
            }
            num = buffer2[0x1c];
            num2 = buffer2[0x1b];
            if (!Basic.Configure_PICkitSerial(7, true) || !(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return flag;
            }
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer2, 0, buffer2.Length);
            if (!Basic.Get_Status_Packet(ref buffer2))
            {
                return false;
            }
            buffer2[0x17] = 1;
            buffer2[0x1b] = num2;
            buffer2[0x1c] = num;
            buffer2[0x1d] = p_slave_addr;
            buffer2[30] = p_slave_mask;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        private static void fire_and_forget_read()
        {
            m_read_mutex.WaitOne();
            if (Read != null)
            {
                Read((byte) m_last_slave_addr_received, (ushort) m_slave_array_index, ref m_slave_data_array);
            }
            m_read_mutex.ReleaseMutex();
        }

        private static void fire_and_forget_receive()
        {
            m_receive_mutex.WaitOne();
            if (Receive != null)
            {
                Receive((byte) m_last_slave_addr_received);
            }
            m_receive_mutex.ReleaseMutex();
        }

        private static void fire_and_forget_write()
        {
            m_write_mutex.WaitOne();
            if (Write != null)
            {
                Write((byte) m_last_slave_addr_received, (ushort) m_slave_array_index, ref m_slave_data_array);
            }
            m_write_mutex.ReleaseMutex();
        }

        public static bool Get_I2CSlave_Address_and_Mask(ref byte p_slave_addr, ref byte p_slave_mask)
        {
            byte[] buffer = new byte[0x41];
            if (!Basic.Get_Status_Packet(ref buffer))
            {
                return false;
            }
            p_slave_addr = buffer[0x1d];
            p_slave_mask = buffer[30];
            return true;
        }

        public static byte Get_Slave_Addresses_That_Will_Acknowledge(byte p_slave_addr, byte p_slave_mask, ref byte[] p_addr_array, ref string p_addr_str)
        {
            byte index = 0;
            p_addr_str = "";
            byte num2 = (byte) (p_slave_mask & 0x3e);
            byte num3 = (byte) (p_slave_addr & ~num2);
            for (int i = 0; i <= 0xff; i++)
            {
                byte num5 = (byte) (i & ~num2);
                if (num5 == num3)
                {
                    index = (byte) (index + 1);
                    p_addr_array[index] = (byte) i;
                    p_addr_str = p_addr_str + string.Format("{0:X2} ", i);
                }
            }
            return index;
        }

        internal static void issue_error()
        {
            Basic.Reset_Control_Block();
            Error();
        }

        internal static void issue_event()
        {
            if (Utilities.g_i2cs_mode == Utilities.I2CS_MODE.INTERACTIVE)
            {
                bool flag = false;
                if (((m_last_slave_addr_received == (m_previous_slave_addr_received + 1)) && m_master_is_waiting_for_data) && !m_stop_command_issued)
                {
                    issue_read_command();
                    flag = true;
                }
                else if (((m_previous_slave_addr_received == 0x200) && ((m_last_slave_addr_received % 2) != 0)) && (m_master_is_waiting_for_data && !m_stop_command_issued))
                {
                    issue_receive_command();
                    flag = true;
                }
                else if (((m_previous_slave_addr_received == 0x200) && ((m_last_slave_addr_received % 2) == 0)) && (!m_master_is_waiting_for_data && m_stop_command_issued))
                {
                    issue_write_command();
                    flag = true;
                }
                else if ((((m_last_slave_addr_received != (m_previous_slave_addr_received + 1)) && !m_master_is_waiting_for_data) && ((m_previous_slave_addr_received != 0x200) && m_slave_address_was_just_set)) && !m_stop_command_issued)
                {
                    issue_write_command();
                    USBRead.Clear_Data_Array(0);
                    USBRead.Clear_Raw_Data_Array();
                    flag = true;
                }
                else if ((((m_previous_slave_addr_received != 0x200) && (m_last_slave_addr_received != (m_previous_slave_addr_received + 1))) && (((m_last_slave_addr_received % 2) != 0) && m_slave_address_was_just_set)) && !m_stop_command_issued)
                {
                    issue_read_command();
                    USBRead.Clear_Data_Array(0);
                    USBRead.Clear_Raw_Data_Array();
                    flag = true;
                }
                else if (((m_previous_slave_addr_received != 0x200) && ((m_last_slave_addr_received % 2) == 0)) && (m_slave_address_was_just_set && !m_stop_command_issued))
                {
                    issue_receive_command();
                    USBRead.Clear_Data_Array(0);
                    USBRead.Clear_Raw_Data_Array();
                    flag = true;
                }
            }
        }

        private static void issue_read_command()
        {
            m_slave_array_index = USBRead.m_cbuf2_data_array_index;
            for (int i = 0; i < m_slave_array_index; i++)
            {
                m_slave_data_array[i] = USBRead.m_cbuf2_data_array[i];
            }
            m_read = new Thread(new ThreadStart(I2CS.fire_and_forget_read));
            m_read.IsBackground = true;
            m_read.Start();
        }

        private static void issue_receive_command()
        {
            m_receive = new Thread(new ThreadStart(I2CS.fire_and_forget_receive));
            m_receive.IsBackground = true;
            m_receive.Start();
        }

        private static void issue_write_command()
        {
            m_slave_array_index = USBRead.m_cbuf2_data_array_index;
            for (int i = 0; i < m_slave_array_index; i++)
            {
                m_slave_data_array[i] = USBRead.m_cbuf2_data_array[i];
            }
            m_write = new Thread(new ThreadStart(I2CS.fire_and_forget_write));
            m_write.IsBackground = true;
            m_write.Start();
        }

        internal static void reset_buffers()
        {
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            m_last_slave_addr_received = 0x200;
            m_previous_slave_addr_received = 0x200;
        }

        public static bool Send_Bytes(byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
        {
            if (p_num_bytes_to_write > 0xfd)
            {
                return false;
            }
            int index = 0;
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (4 + p_num_bytes_to_write);
            array[3] = 0xc1;
            array[4] = p_num_bytes_to_write;
            index = 0;
            while (index < p_num_bytes_to_write)
            {
                array[index + 5] = p_data_array[index];
                index++;
            }
            array[index + 5] = 0x1f;
            array[index + 6] = 0x77;
            array[index + 7] = 0;
            p_script_view = "[SB]";
            string str = string.Format("[{0:X2}]", array[4]);
            p_script_view = p_script_view + str;
            for (index = 0; index < p_num_bytes_to_write; index++)
            {
                str = string.Format("[{0:X2}]", array[index + 5]);
                p_script_view = p_script_view + str;
            }
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_I2CSlave_Address_and_Mask(byte p_slave_addr, byte p_slave_mask)
        {
            bool flag = false;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (!Basic.Get_Status_Packet(ref buffer2))
            {
                return false;
            }
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
            buffer2[0x1d] = p_slave_addr;
            buffer2[30] = p_slave_mask;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public delegate void GUINotifierError();

        public delegate void GUINotifierRead(byte slave_addr, ushort byte_count, ref byte[] data);

        public delegate void GUINotifierReceive(byte slave_addr);

        public delegate void GUINotifierWrite(byte slave_addr, ushort byte_count, ref byte[] data);
    }
}

