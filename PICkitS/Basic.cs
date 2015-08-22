namespace PICkitS
{
    using System;
    using System.Threading;

    public class Basic
    {
        private static Thread m_flash_led;
        internal static int m_i2cs_read_wait_time = 200;
        internal static int m_i2cs_receive_wait_time = 200;
        private static Mutex m_reset_cb = new Mutex(false);
        private static Thread m_reset_control_block;
        internal static int m_spi_receive_wait_time = 200;

        public static void Cleanup()
        {
            if (USBRead.Read_Thread_Is_Active())
            {
                USBRead.Kill_Read_Thread();
                Thread.Sleep(500);
            }
            USBWrite.Kill_Write_Thread();
            USBWrite.Dispose_Of_Write_Objects();
            Utilities.CloseHandle(Utilities.m_flags.HID_write_handle);
            Utilities.CloseHandle(Utilities.m_flags.HID_read_handle);
            Utilities.m_flags.g_status_packet_data_update_event.Close();
            Utilities.m_flags.g_data_arrived_event.Close();
            Utilities.m_flags.g_bl_data_arrived_event.Close();
            Utilities.m_flags.g_status_packet_mutex.Close();
            Utilities.m_flags.g_PKSA_has_completed_script.Close();
            USBRead.m_usb_packet_mutex.Close();
            USBRead.m_cbuf2_data_array_mutex.Close();
        }

        public static bool Configure_PICkitSerial(int p_mode, bool p_reset)
        {
            bool flag = false;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                Mode.update_status_packet_data(p_mode, ref buffer2);
                USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                flag = USBWrite.write_and_verify_config_block(ref array, ref str2, p_reset, ref str);
            }
            return flag;
        }

        public static bool Configure_PICkitSerial_For_I2C()
        {
            return Configure_PICkitSerial(1, true);
        }

        public static bool Configure_PICkitSerial_For_LIN()
        {
            return Configure_PICkitSerial(10, true);
        }

        public static void Flash_LED1_For_2_Seconds()
        {
            m_flash_led = new Thread(new ThreadStart(Basic.flash_the_busy_led));
            m_flash_led.IsBackground = true;
            m_flash_led.Start();
        }

        private static void flash_the_busy_led()
        {
            byte num = 0xc1;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                Array.Clear(buffer2, 0, buffer2.Length);
                if (Get_Status_Packet(ref buffer2))
                {
                    buffer2[7] = (byte) (buffer2[7] | 0x20);
                    USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                    USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
                    if (USBWrite.Send_LED_State_Cmd(1, num))
                    {
                        buffer2[7] = (byte) (buffer2[7] & 0xdf);
                        Thread.Sleep(0x7d0);
                        USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
                        USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
                    }
                }
            }
        }

        public static int Get_Script_Timeout()
        {
            return USBWrite.m_universal_timeout;
        }

        public static bool Get_Status_Packet(ref byte[] p_status_packet)
        {
            bool flag = false;
            if (!USBWrite.Update_Status_Packet())
            {
                return flag;
            }
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            for (int i = 0; i < Constants.STATUS_PACKET_DATA.Length; i++)
            {
                p_status_packet[i] = Constants.STATUS_PACKET_DATA[i];
            }
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            return true;
        }

        public static ushort How_Many_Of_MyDevices_Are_Attached(ushort ProductID)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0x4d8;
            return USB.Count_Attached_PKSA(num, ProductID, 0x1d, ref zero, ref ptr2, ref str, false, ref empty);
        }

        public static ushort How_Many_PICkitSerials_Are_Attached()
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0x36;
            ushort num2 = 0x4d8;
            return USB.Count_Attached_PKSA(num2, num, 0x1d, ref zero, ref ptr2, ref str, false, ref empty);
        }

        public static bool Initialize_MyDevice(ushort USBIndex, ushort ProductID)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0x4d8;
            ushort num2 = 0;
            bool flag = USB.Get_This_Device(num, ProductID, USBIndex, ref zero, ref ptr2, ref str, false, ref empty, ref num2);
            if (flag)
            {
                flag = USBRead.Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                    if (ProductID == 80)
                    {
                        Utilities.g_comm_mode = Utilities.COMM_MODE.MTOUCH2;
                    }
                }
            }
            return flag;
        }

        public static bool Initialize_PICkitSerial()
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0x36;
            ushort num2 = 0x4d8;
            ushort num3 = 0;
            bool flag = USB.Get_This_Device(num2, num, 0, ref zero, ref ptr2, ref str, false, ref empty, ref num3);
            if (flag)
            {
                flag = USBRead.Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                }
            }
            return flag;
        }

        public static bool Initialize_PICkitSerial(ushort USBIndex)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0x36;
            ushort num2 = 0x4d8;
            ushort num3 = 0;
            bool flag = USB.Get_This_Device(num2, num, USBIndex, ref zero, ref ptr2, ref str, false, ref empty, ref num3);
            if (flag)
            {
                flag = USBRead.Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                }
            }
            return flag;
        }

        public static bool ReEstablish_Comm_Threads()
        {
            return (USBRead.Kick_Off_Read_Thread() && USBWrite.kick_off_write_thread());
        }

        public static void Reset_Control_Block()
        {
            m_reset_control_block = new Thread(new ThreadStart(Basic.Reset_Control_Block_Thread));
            m_reset_control_block.IsBackground = true;
            m_reset_control_block.Start();
        }

        private static void Reset_Control_Block_Thread()
        {
            m_reset_cb.WaitOne();
            string str = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            string str2 = "";
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer2, 0, buffer2.Length);
            I2CS.reset_buffers();
            if (USBWrite.Update_Status_Packet())
            {
                Utilities.m_flags.g_status_packet_mutex.WaitOne();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = Constants.STATUS_PACKET_DATA[i];
                }
                Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
                USBWrite.configure_outbound_control_block_packet(ref buffer2, ref str, ref array);
                USBWrite.write_and_verify_config_block(ref buffer2, ref str, true, ref str2);
            }
            m_reset_cb.ReleaseMutex();
        }

        public static bool Retrieve_USART_Data(uint p_byte_count, ref byte[] p_data_array)
        {
            bool flag = false;
            if (USBRead.Retrieve_Data(ref p_data_array, p_byte_count))
            {
                flag = true;
            }
            return flag;
        }

        public static uint Retrieve_USART_Data_Byte_Count()
        {
            return USBRead.Retrieve_Data_Byte_Count();
        }

        public static bool Send_I2C_SimpleRead_Cmd(byte p_slave_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            bool flag = false;
            if (p_num_bytes_to_read == 0)
            {
                return false;
            }
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 9;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 1;
            array[6] = p_slave_addr;
            array[7] = 0x89;
            array[8] = p_num_bytes_to_read;
            array[9] = 130;
            array[10] = 0x1f;
            array[11] = 0x77;
            array[12] = 0;
            p_script_view = "[S_][W_][01]";
            string str = string.Format("[{0:X2}]", array[6]);
            p_script_view = p_script_view + str;
            p_script_view = p_script_view + "[RN]";
            str = string.Format("[{0:X2}]", array[8]);
            p_script_view = p_script_view + str;
            p_script_view = p_script_view + "[P_]";
            USBRead.Clear_Data_Array(p_num_bytes_to_read);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(m_i2cs_receive_wait_time, false)) && USBRead.Retrieve_Data(ref p_data_array, p_num_bytes_to_read))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Send_I2CRead_Cmd(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            if (p_num_bytes_to_read != 0)
            {
                byte[] array = new byte[300];
                Array.Clear(array, 0, array.Length);
                array[0] = 0;
                array[1] = 3;
                array[2] = 14;
                array[3] = 0x81;
                array[4] = 0x84;
                array[5] = 2;
                array[6] = p_slave_addr;
                array[7] = p_start_data_addr;
                array[8] = 0x83;
                array[9] = 0x84;
                array[10] = 1;
                array[11] = (byte) (p_slave_addr + 1);
                array[12] = 0x89;
                array[13] = p_num_bytes_to_read;
                array[14] = 130;
                array[15] = 0x1f;
                array[0x10] = 0x77;
                array[0x11] = 0;
                p_script_view = "[S_][W_][02]";
                string str = string.Format("[{0:X2}]", array[6]);
                p_script_view = p_script_view + str;
                str = string.Format("[{0:X2}]", array[7]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[RS][W_][01]";
                str = string.Format("[{0:X2}]", array[11]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[RN]";
                str = string.Format("[{0:X2}]", array[13]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[P_]";
                USBRead.Clear_Data_Array(p_num_bytes_to_read);
                USBRead.Clear_Raw_Data_Array();
                if (USBWrite.Send_Script_To_PICkitS(ref array))
                {
                    return (Utilities.m_flags.g_data_arrived_event.WaitOne(m_i2cs_read_wait_time, false) && USBRead.Retrieve_Data(ref p_data_array, p_num_bytes_to_read));
                }
            }
            return false;
        }

        public static bool Send_I2CRead_Word_Cmd(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            if (p_num_bytes_to_read != 0)
            {
                byte[] array = new byte[300];
                Array.Clear(array, 0, array.Length);
                array[0] = 0;
                array[1] = 3;
                array[2] = 15;
                array[3] = 0x81;
                array[4] = 0x84;
                array[5] = 3;
                array[6] = p_slave_addr;
                array[7] = p_command1;
                array[8] = p_command2;
                array[9] = 0x83;
                array[10] = 0x84;
                array[11] = 1;
                array[12] = (byte) (p_slave_addr + 1);
                array[13] = 0x89;
                array[14] = p_num_bytes_to_read;
                array[15] = 130;
                array[0x10] = 0x1f;
                array[0x11] = 0x77;
                array[0x12] = 0;
                p_script_view = "[S_][W_][03]";
                string str = string.Format("[{0:X2}]", array[6]);
                p_script_view = p_script_view + str;
                str = string.Format("[{0:X2}]", array[7]);
                p_script_view = p_script_view + str;
                str = string.Format("[{0:X2}]", array[8]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[RS][W_][01]";
                str = string.Format("[{0:X2}]", array[12]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[RN]";
                str = string.Format("[{0:X2}]", array[14]);
                p_script_view = p_script_view + str;
                p_script_view = p_script_view + "[P_]";
                USBRead.Clear_Data_Array(p_num_bytes_to_read);
                USBRead.Clear_Raw_Data_Array();
                if (USBWrite.Send_Script_To_PICkitS(ref array))
                {
                    return (Utilities.m_flags.g_data_arrived_event.WaitOne(m_i2cs_read_wait_time, false) && USBRead.Retrieve_Data(ref p_data_array, p_num_bytes_to_read));
                }
            }
            return false;
        }

        public static bool Send_I2CWrite_Cmd(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
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
            array[2] = (byte) (8 + p_num_bytes_to_write);
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = (byte) (2 + p_num_bytes_to_write);
            array[6] = p_slave_addr;
            array[7] = p_start_data_addr;
            index = 0;
            while (index < p_num_bytes_to_write)
            {
                array[index + 8] = p_data_array[index];
                index++;
            }
            array[index + 8] = 130;
            array[index + 9] = 0x1f;
            array[index + 10] = 0x77;
            array[index + 11] = 0;
            p_script_view = "[S_][W_]";
            string str = string.Format("[{0:X2}]", array[5]);
            p_script_view = p_script_view + str;
            str = string.Format("[{0:X2}]", array[6]);
            p_script_view = p_script_view + str;
            str = string.Format("[{0:X2}]", array[7]);
            p_script_view = p_script_view + str;
            for (index = 0; index < p_num_bytes_to_write; index++)
            {
                str = string.Format("[{0:X2}]", array[index + 8]);
                p_script_view = p_script_view + str;
            }
            p_script_view = p_script_view + "[P_]";
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Send_I2CWrite_Word_Cmd(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
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
            array[2] = (byte) (9 + p_num_bytes_to_write);
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = (byte) (3 + p_num_bytes_to_write);
            array[6] = p_slave_addr;
            array[7] = p_command1;
            array[8] = p_command2;
            index = 0;
            while (index < p_num_bytes_to_write)
            {
                array[index + 9] = p_data_array[index];
                index++;
            }
            array[index + 9] = 130;
            array[index + 10] = 0x1f;
            array[index + 11] = 0x77;
            array[index + 12] = 0;
            p_script_view = "[S_][W_]";
            string str = string.Format("[{0:X2}]", array[5]);
            p_script_view = p_script_view + str;
            str = string.Format("[{0:X2}]", array[6]);
            p_script_view = p_script_view + str;
            str = string.Format("[{0:X2}]", array[7]);
            p_script_view = p_script_view + str;
            str = string.Format("[{0:X2}]", array[8]);
            p_script_view = p_script_view + str;
            for (index = 0; index < p_num_bytes_to_write; index++)
            {
                str = string.Format("[{0:X2}]", array[index + 9]);
                p_script_view = p_script_view + str;
            }
            p_script_view = p_script_view + "[P_]";
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Send_SPI_Receive_Cmd(byte p_num_bytes_to_read, ref byte[] p_data_array, bool p_first_cmd, bool p_last_cmd, ref string p_script_view)
        {
            bool flag = false;
            byte[] array = new byte[0xff];
            int index = 3;
            p_script_view = "";
            string str = "";
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            if (p_first_cmd)
            {
                array[index++] = 0x8b;
                p_script_view = "[CSON]";
            }
            array[index++] = 0x84;
            array[index++] = p_num_bytes_to_read;
            p_script_view = p_script_view + "[DI]";
            str = string.Format("[{0:X2}]", p_num_bytes_to_read);
            p_script_view = p_script_view + str;
            if (p_last_cmd)
            {
                array[index++] = 140;
                p_script_view = p_script_view + "[CSOF]";
            }
            array[2] = (byte) (index - 1);
            array[index++] = 0x1f;
            array[index++] = 0x77;
            array[index] = 0;
            USBRead.Clear_Data_Array(p_num_bytes_to_read);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(m_spi_receive_wait_time, false)) && USBRead.Retrieve_Data(ref p_data_array, p_num_bytes_to_read))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Send_SPI_Send_Cmd(byte p_byte_count, ref byte[] p_data, bool p_first_cmd, bool p_last_cmd, ref string p_script_view)
        {
            byte[] array = new byte[0xff];
            int index = 3;
            int num2 = 0;
            string str = "";
            if (p_byte_count > 0xf6)
            {
                return false;
            }
            p_script_view = "";
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            if (p_first_cmd)
            {
                array[index++] = 0x8b;
                p_script_view = "[CSON]";
            }
            array[index++] = 0x85;
            array[index++] = p_byte_count;
            p_script_view = p_script_view + "[DO]";
            str = string.Format("[{0:X2}]", p_byte_count);
            p_script_view = p_script_view + str;
            for (num2 = 0; num2 < p_byte_count; num2++)
            {
                array[index++] = p_data[num2];
                str = string.Format("[{0:X2}]", p_data[num2]);
                p_script_view = p_script_view + str;
            }
            if (p_last_cmd)
            {
                array[index++] = 140;
                p_script_view = p_script_view + "[CSOF]";
            }
            array[2] = (byte) (index - 1);
            array[index++] = 0x1f;
            array[index++] = 0x77;
            array[index] = 0;
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Send_USART_Cmd(byte p_byte_count, ref byte[] p_data, ref string p_script_view)
        {
            byte[] array = new byte[0xff];
            int index = 5;
            int num2 = 0;
            string str = "";
            p_script_view = "";
            if (p_byte_count > 0xf7)
            {
                return false;
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (p_byte_count + 4);
            array[3] = 130;
            array[4] = p_byte_count;
            str = string.Format("[{0:X2}]", p_byte_count);
            for (num2 = 0; num2 < p_byte_count; num2++)
            {
                array[index++] = p_data[num2];
                str = string.Format("[{0:X2}]", p_data[num2]);
                p_script_view = p_script_view + str;
            }
            array[index++] = 0x1f;
            array[index++] = 0x77;
            array[index] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static void Set_Script_Timeout(int p_time)
        {
            USBWrite.m_universal_timeout = p_time;
        }

        public static void Terminate_Comm_Threads()
        {
            USBRead.Kill_Read_Thread();
            USBWrite.Kill_Write_Thread();
        }

        public static bool There_Is_A_Status_Error(ref uint p_error)
        {
            return Status.There_Is_A_Status_Error(ref p_error);
        }
    }
}

