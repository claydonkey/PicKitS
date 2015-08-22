namespace PICkitS
{
    using System;

    public class SPIM
    {
        public static bool Configure_PICkitSerial_For_SPIMaster()
        {
            return Basic.Configure_PICkitSerial(2, true);
        }

        public static bool Configure_PICkitSerial_For_SPIMaster(bool p_sample_phase, bool p_clock_edge_select, bool p_clock_polarity, bool p_auto_output_disable, bool p_chip_sel_polarity, bool p_supply_5V)
        {
            bool flag = false;
            if (!Basic.Configure_PICkitSerial(2, true))
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
            if (p_sample_phase)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 1);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xfe);
            }
            if (p_clock_edge_select)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 2);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xfd);
            }
            if (p_clock_polarity)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 4);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xfb);
            }
            if (p_auto_output_disable)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 8);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0xf7);
            }
            if (p_chip_sel_polarity)
            {
                buffer2[0x18] = (byte) (buffer2[0x18] | 0x80);
            }
            else
            {
                buffer2[0x18] = (byte) (buffer2[0x18] & 0x7f);
            }
            if (p_supply_5V)
            {
                buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
            }
            else
            {
                buffer2[0x10] = (byte) (buffer2[0x10] & 0xdf);
            }
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, true, ref str);
        }

        private static bool Configure_PICkitSerial_For_SPISlave()
        {
            return Basic.Configure_PICkitSerial(3, true);
        }

        public static int Get_Receive_Wait_Time()
        {
            return Basic.m_spi_receive_wait_time;
        }

        public static double Get_SPI_Bit_Rate()
        {
            byte[] array = new byte[0x41];
            double num = 0.0;
            if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return num;
            }
            Array.Clear(array, 0, array.Length);
            if (!Basic.Get_Status_Packet(ref array))
            {
                return num;
            }
            double num2 = 0.0;
            double num3 = 0.0;
            switch (array[50])
            {
                case 0:
                    num2 = 8.0;
                    break;

                case 1:
                    num2 = 32.0;
                    break;

                case 2:
                    num2 = 128.0;
                    break;

                default:
                    num2 = 0.0;
                    break;
            }
            num3 = array[0x33] + 1;
            if (num2 == 0.0)
            {
                return 0.0;
            }
            return (((20.0 / num2) / num3) * 1000.0);
        }

        public static bool Get_SPI_Status(ref bool p_sample_phase, ref bool p_clock_edge_select, ref bool p_clock_polarity, ref bool p_auto_output_disable, ref bool p_SDI_state, ref bool p_SDO_state, ref bool p_SCK_state, ref bool p_chip_select_state)
        {
            byte[] array = new byte[0x41];
            bool flag = false;
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (Basic.Get_Status_Packet(ref array))
                {
                    p_sample_phase = (array[0x2d] & 1) > 0;
                    p_clock_edge_select = (array[0x2d] & 2) > 0;
                    p_clock_polarity = (array[0x2d] & 4) > 0;
                    p_auto_output_disable = (array[0x2d] & 8) > 0;
                    p_SDI_state = (array[0x2e] & 1) > 0;
                    p_SDO_state = (array[0x2e] & 2) > 0;
                    p_SCK_state = (array[0x2e] & 4) > 0;
                    p_chip_select_state = (array[0x2e] & 8) > 0;
                    flag = true;
                }
            }
            return flag;
        }

        public static bool Receive_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
        {
            return Basic.Send_SPI_Receive_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
        }

        public static bool Send_And_Receive_Data(byte p_byte_count, ref byte[] p_send_data_array, ref byte[] p_receive_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
        {
            bool flag = false;
            byte[] array = new byte[0xff];
            int index = 3;
            int num2 = 0;
            p_script_view = "";
            string str = "";
            if (p_byte_count > 0xf5)
            {
                return false;
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            if (p_assert_cs)
            {
                array[index++] = 0x8b;
                p_script_view = "[CSON]";
            }
            array[index++] = 0x86;
            array[index++] = p_byte_count;
            p_script_view = p_script_view + "[DIO]";
            str = string.Format("[{0:X2}]", p_byte_count);
            p_script_view = p_script_view + str;
            for (num2 = 0; num2 < p_byte_count; num2++)
            {
                array[index++] = p_send_data_array[num2];
                str = string.Format("[{0:X2}]", p_send_data_array[num2]);
                p_script_view = p_script_view + str;
            }
            if (p_de_assert_cs)
            {
                array[index++] = 140;
                p_script_view = p_script_view + "[CSOF]";
            }
            array[2] = (byte) (index - 1);
            array[index++] = 0x1f;
            array[index++] = 0x77;
            array[index] = 0;
            USBRead.Clear_Data_Array(p_byte_count);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_spi_receive_wait_time, false)) && USBRead.Retrieve_Data(ref p_receive_data_array, p_byte_count))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
        {
            return Basic.Send_SPI_Send_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
        }

        public static void Set_Receive_Wait_Time(int p_time)
        {
            Basic.m_spi_receive_wait_time = p_time;
        }

        public static bool Set_SPI_BitRate(double p_Bit_Rate)
        {
            if ((p_Bit_Rate < 0.61) || (p_Bit_Rate > 1250.0))
            {
                return false;
            }
            double num = 200000.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            double a = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            ushort num8 = 0;
            ushort num9 = 0;
            ushort num10 = 0;
            byte num11 = 0;
            byte num12 = 0;
            byte[] array = new byte[0x10];
            a = (2.5 / p_Bit_Rate) * 1000.0;
            num6 = (0.625 / p_Bit_Rate) * 1000.0;
            num7 = (0.15625 / p_Bit_Rate) * 1000.0;
            if ((a > 0.0) && (a <= 256.0))
            {
                num8 = (ushort) Math.Round(a);
            }
            else
            {
                num8 = 0;
            }
            if ((num6 > 0.0) && (num6 <= 256.0))
            {
                num9 = (ushort) Math.Round(num6);
            }
            else
            {
                num9 = 0;
            }
            if ((num7 > 0.0) && (num7 <= 256.0))
            {
                num10 = (ushort) Math.Round(num7);
            }
            else
            {
                num10 = 0;
            }
            if (num8 != 0)
            {
                num2 = (2.5 / ((double) num8)) * 1000.0;
            }
            else
            {
                num2 = num;
            }
            if (num9 != 0)
            {
                num3 = (0.625 / ((double) num9)) * 1000.0;
            }
            else
            {
                num3 = num;
            }
            if (num10 != 0)
            {
                num4 = (0.15625 / ((double) num10)) * 1000.0;
            }
            else
            {
                num4 = num;
            }
            if (Math.Abs((double) (num2 - p_Bit_Rate)) < Math.Abs((double) (num3 - p_Bit_Rate)))
            {
                if (Math.Abs((double) (num2 - p_Bit_Rate)) < Math.Abs((double) (num4 - p_Bit_Rate)))
                {
                    num12 = (byte) (num8 - 1);
                    num11 = 0;
                }
                else
                {
                    num12 = (byte) (num10 - 1);
                    num11 = 2;
                }
            }
            else if (Math.Abs((double) (num3 - p_Bit_Rate)) < Math.Abs((double) (num4 - p_Bit_Rate)))
            {
                num12 = (byte) (num9 - 1);
                num11 = 1;
            }
            else
            {
                num12 = (byte) (num10 - 1);
                num11 = 2;
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 5;
            array[3] = 0x83;
            array[4] = num12;
            array[5] = num11;
            array[6] = 0x1f;
            array[7] = 0x77;
            array[8] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Tell_PKSA_To_Power_My_Device()
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
            buffer2[0x10] = (byte) (buffer2[0x10] | 0x20);
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

