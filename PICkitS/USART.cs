namespace PICkitS
{
    using System;

    public class USART
    {
        private static ushort calculate_baud_rate(ushort p_baud)
        {
            double num = 20.0 / (4.0 * (p_baud + 1.0));
            int num2 = (int) Math.Round((double) (num * 1000000.0));
            return (ushort) num2;
        }

        public static bool Configure_PICkitSerial_For_USARTAsync()
        {
            return Basic.Configure_PICkitSerial(4, true);
        }

        public static bool Configure_PICkitSerial_For_USARTAsync(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_rcv_dis, double p_voltage)
        {
            bool flag = false;
            int num = 0;
            if (!Basic.Configure_PICkitSerial(4, true))
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
            if ((p_voltage < 0.0) || (p_voltage > 5.0))
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
            if (p_rcv_dis)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 4);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xfb);
            }
            num = (int) Math.Round((double) (((p_voltage * 1000.0) + 43.53) / 21.191));
            buffer2[0x13] = (byte) num;
            buffer2[20] = (byte) (num / 4);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x40);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Configure_PICkitSerial_For_USARTSyncMaster()
        {
            return Basic.Configure_PICkitSerial(5, true);
        }

        public static bool Configure_PICkitSerial_For_USARTSyncMaster(bool p_aux1_def, bool p_aux2_def, bool p_aux1_dir, bool p_aux2_dir, bool p_clock_pol, double p_voltage)
        {
            bool flag = false;
            int num = 0;
            if (!Basic.Configure_PICkitSerial(5, true))
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
            if (p_clock_pol)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 1);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xfe);
            }
            num = (int) Math.Round((double) (((p_voltage * 1000.0) + 43.53) / 21.191));
            buffer2[0x13] = (byte) num;
            buffer2[20] = (byte) (num / 4);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x40);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Configure_PICkitSerial_For_USARTSyncSlave()
        {
            return Basic.Configure_PICkitSerial(6, true);
        }

        public static bool Get_Aux_Status(ref bool p_aux1_state, ref bool p_aux2_state, ref bool p_aux1_dir, ref bool p_aux2_dir)
        {
            byte[] array = new byte[0x41];
            bool flag = false;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    p_aux1_state = (array[0x30] & 1) > 0;
                    p_aux2_state = (array[0x30] & 2) > 0;
                    p_aux1_dir = (array[0x30] & 4) > 0;
                    p_aux2_dir = (array[0x30] & 8) > 0;
                    flag = true;
                }
            }
            return flag;
        }

        public static ushort Get_Baud_Rate()
        {
            byte[] array = new byte[0x41];
            ushort num = 0;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    num = calculate_baud_rate((ushort) (array[50] + (array[0x33] << 8)));
                }
            }
            return num;
        }

        public static bool Get_Source_Voltage(ref double p_voltage, ref bool p_PKSA_power)
        {
            byte[] array = new byte[0x41];
            bool flag = false;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    p_voltage = (array[0x27] * 5.0) / 255.0;
                    p_PKSA_power = (array[0x10] & 0x20) > 0;
                    flag = true;
                }
            }
            return flag;
        }

        public static bool Retrieve_Data(uint p_byte_count, ref byte[] p_data_array)
        {
            bool flag = false;
            if (USBRead.Retrieve_Data(ref p_data_array, p_byte_count))
            {
                flag = true;
            }
            return flag;
        }

        public static uint Retrieve_Data_Byte_Count()
        {
            return USBRead.Retrieve_Data_Byte_Count();
        }

        public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, ref string p_script_view)
        {
            byte[] array = new byte[310];
            int index = 5;
            int num2 = 0;
            string str = "";
            p_script_view = "";
            if (p_byte_count > 0xfb)
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
                array[index++] = p_data_array[num2];
                str = string.Format("[{0:X2}]", p_data_array[num2]);
                p_script_view = p_script_view + str;
            }
            array[index++] = 0x1f;
            array[index++] = 0x77;
            array[index] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_Aux1_Direction(bool p_dir)
        {
            byte[] array = new byte[0x10];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 3;
            if (p_dir)
            {
                array[3] = 0x93;
            }
            else
            {
                array[3] = 0x92;
            }
            array[4] = 0x1f;
            array[5] = 0x77;
            array[6] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_Aux1_State(bool p_state)
        {
            byte[] array = new byte[0x10];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 3;
            if (p_state)
            {
                array[3] = 0x91;
            }
            else
            {
                array[3] = 0x90;
            }
            array[4] = 0x1f;
            array[5] = 0x77;
            array[6] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_Aux2_Direction(bool p_dir)
        {
            byte[] array = new byte[0x10];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 3;
            if (p_dir)
            {
                array[3] = 0x99;
            }
            else
            {
                array[3] = 0x98;
            }
            array[4] = 0x1f;
            array[5] = 0x77;
            array[6] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_Aux2_State(bool p_state)
        {
            byte[] array = new byte[0x10];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 3;
            if (p_state)
            {
                array[3] = 0x97;
            }
            else
            {
                array[3] = 150;
            }
            array[4] = 0x1f;
            array[5] = 0x77;
            array[6] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Set_Baud_Rate(ushort p_baud)
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
            int num = ((int) Math.Round((double) ((20000000.0 / ((double) p_baud)) / 4.0))) - 1;
            buffer2[0x1d] = (byte) num;
            buffer2[30] = (byte) (num >> 8);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        public static bool Set_Source_Voltage(double p_voltage)
        {
            bool flag = false;
            int num = 0;
            string str = "";
            string str2 = "";
            byte[] array = new byte[0x41];
            byte[] buffer2 = new byte[0x41];
            if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return flag;
            }
            if ((p_voltage < 0.0) || (p_voltage > 5.0))
            {
                return flag;
            }
            Array.Clear(array, 0, array.Length);
            Array.Clear(buffer2, 0, buffer2.Length);
            if (!Basic.Get_Status_Packet(ref buffer2))
            {
                return false;
            }
            num = (int) Math.Round((double) (((p_voltage * 1000.0) + 43.53) / 21.191));
            buffer2[0x13] = (byte) num;
            buffer2[20] = (byte) (num / 4);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x40);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, false, ref str);
        }

        public static bool Tell_PKSA_To_Use_External_Voltage_Source()
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
            buffer2[0x10] = (byte) (buffer2[0x10] & 0xdf);
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, false, ref str);
        }
    }
}

