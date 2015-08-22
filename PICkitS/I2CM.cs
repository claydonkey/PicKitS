namespace PICkitS
{
    using System;

    public class I2CM
    {
        private static double calculate_baud_rate(ushort p_baud)
        {
            double num = 20.0 / (4.0 * (p_baud + 1.0));
            return (num * 1000.0);
        }

        public static bool Configure_PICkitSerial_For_I2CMaster()
        {
            return Basic.Configure_PICkitSerial(1, true);
        }

        public static bool Configure_PICkitSerial_For_I2CMaster(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_enable_pu, double p_voltage)
        {
            bool flag = false;
            int num = 0;
            if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return flag;
            }
            if ((p_voltage < 0.0) || (p_voltage > 5.0))
            {
                return flag;
            }
            if (!Basic.Configure_PICkitSerial(1, true))
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
            if (p_aux1_def)
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] | 1);
            }
            else
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] & 0xfe);
            }
            if (p_aux2_def)
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] | 2);
            }
            else
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] & 0xfd);
            }
            if (p_aux1_dir)
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] | 4);
            }
            else
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] & 0xfb);
            }
            if (p_aux2_dir)
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] | 8);
            }
            else
            {
                buffer2[0x1c] = (byte) (buffer2[0x1c] & 0xf7);
            }
            if (p_enable_pu)
            {
                buffer2[0x10] = (byte) (buffer2[0x10] | 0x10);
            }
            else
            {
                buffer2[0x10] = (byte) (buffer2[0x10] & 0xef);
            }
            num = (int) Math.Round((double) (((p_voltage * 1000.0) + 43.53) / 21.191));
            buffer2[0x13] = (byte) num;
            buffer2[20] = (byte) (num / 4);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x40);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Get_Aux_Status(ref bool p_aux1_state, ref bool p_aux2_state, ref bool p_aux1_dir, ref bool p_aux2_dir)
        {
            return USART.Get_Aux_Status(ref p_aux1_state, ref p_aux2_state, ref p_aux1_dir, ref p_aux2_dir);
        }

        public static double Get_I2C_Bit_Rate()
        {
            byte[] array = new byte[0x41];
            double num = 0.0;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    num = calculate_baud_rate(array[0x33]);
                }
            }
            return num;
        }

        public static int Get_Read_Wait_Time()
        {
            return Basic.m_i2cs_read_wait_time;
        }

        public static int Get_Receive_Wait_Time()
        {
            return Basic.m_i2cs_receive_wait_time;
        }

        public static bool Get_Source_Voltage(ref double p_voltage, ref bool p_PKSA_power)
        {
            return USART.Get_Source_Voltage(ref p_voltage, ref p_PKSA_power);
        }

        public static bool Read(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            return Basic.Send_I2CRead_Cmd(p_slave_addr, p_start_data_addr, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
        }

        public static bool Read(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            return Basic.Send_I2CRead_Word_Cmd(p_slave_addr, p_command1, p_command2, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
        }

        public static bool Receive(byte p_slave_addr, byte p_num_bytes_to_read, ref byte[] p_data_array, ref string p_script_view)
        {
            return Basic.Send_I2C_SimpleRead_Cmd(p_slave_addr, p_num_bytes_to_read, ref p_data_array, ref p_script_view);
        }

        public static bool Set_Aux1_Direction(bool p_dir)
        {
            return USART.Set_Aux1_Direction(p_dir);
        }

        public static bool Set_Aux1_State(bool p_state)
        {
            return USART.Set_Aux1_State(p_state);
        }

        public static bool Set_Aux2_Direction(bool p_dir)
        {
            return USART.Set_Aux2_Direction(p_dir);
        }

        public static bool Set_Aux2_State(bool p_state)
        {
            return USART.Set_Aux2_State(p_state);
        }

        public static bool Set_I2C_Bit_Rate(double p_Bit_Rate)
        {
            bool flag = false;
            byte num = 0;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if ((p_Bit_Rate < 39.1) || (p_Bit_Rate > 5000.0))
            {
                return flag;
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
            num = (byte) (Math.Round((double) ((20000.0 / p_Bit_Rate) / 4.0)) - 1.0);
            buffer2[30] = num;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Set_Pullup_State(bool p_enable)
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
            if (p_enable)
            {
                buffer2[0x10] = (byte) (buffer2[0x10] | 0x10);
            }
            else
            {
                buffer2[0x10] = (byte) (buffer2[0x10] & 0xef);
            }
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, false, ref str);
        }

        public static void Set_Read_Wait_Time(int p_time)
        {
            Basic.m_i2cs_read_wait_time = p_time;
        }

        public static void Set_Receive_Wait_Time(int p_time)
        {
            Basic.m_i2cs_receive_wait_time = p_time;
        }

        public static bool Set_Source_Voltage(double p_voltage)
        {
            return USART.Set_Source_Voltage(p_voltage);
        }

        public static bool Tell_PKSA_To_Use_External_Voltage_Source()
        {
            return USART.Tell_PKSA_To_Use_External_Voltage_Source();
        }

        public static bool Write(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
        {
            return Basic.Send_I2CWrite_Cmd(p_slave_addr, p_start_data_addr, p_num_bytes_to_write, ref p_data_array, ref p_script_view);
        }

        public static bool Write(byte p_slave_addr, byte p_command1, byte p_command2, byte p_num_bytes_to_write, ref byte[] p_data_array, ref string p_script_view)
        {
            return Basic.Send_I2CWrite_Word_Cmd(p_slave_addr, p_command1, p_command2, p_num_bytes_to_write, ref p_data_array, ref p_script_view);
        }

        public static bool Write_Using_PEC(byte p_slave_addr, byte p_start_data_addr, byte p_num_bytes_to_write, ref byte[] p_data_array, ref byte p_PEC, ref string p_script_view)
        {
            if (p_num_bytes_to_write > 0xfd)
            {
                return false;
            }
            int index = 0;
            byte[] array = new byte[300];
            byte num2 = 0;
            num2 = Utilities.calculate_crc8(p_slave_addr, num2);
            num2 = Utilities.calculate_crc8(p_start_data_addr, num2);
            for (index = 0; index < p_num_bytes_to_write; index++)
            {
                num2 = Utilities.calculate_crc8(p_data_array[index], num2);
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (9 + p_num_bytes_to_write);
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = (byte) (3 + p_num_bytes_to_write);
            array[6] = p_slave_addr;
            array[7] = p_start_data_addr;
            index = 0;
            while (index < p_num_bytes_to_write)
            {
                array[index + 8] = p_data_array[index];
                index++;
            }
            array[index + 8] = num2;
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
            for (index = 0; index < p_num_bytes_to_write; index++)
            {
                str = string.Format("[{0:X2}]", array[index + 8]);
                p_script_view = p_script_view + str;
            }
            str = string.Format("[{0:X2}]", num2);
            p_script_view = p_script_view + str;
            p_script_view = p_script_view + "[P_]";
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }
    }
}

