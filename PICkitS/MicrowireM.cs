namespace PICkitS
{
    using System;

    public class MicrowireM
    {
        public static bool Configure_PICkitSerial_For_MicrowireMaster()
        {
            return Basic.Configure_PICkitSerial(11, true);
        }

        public static bool Configure_PICkitSerial_For_MicrowireMaster(bool p_sample_phase, bool p_clock_edge_select, bool p_clock_polarity, bool p_auto_output_disable, bool p_chip_sel_polarity, bool p_supply_5V)
        {
            bool flag = false;
            if (!Basic.Configure_PICkitSerial(11, true))
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

        public static double Get_Microwire_Bit_Rate()
        {
            return SPIM.Get_SPI_Bit_Rate();
        }

        public static bool Get_Microwire_Status(ref bool p_sample_phase, ref bool p_clock_edge_select, ref bool p_clock_polarity, ref bool p_auto_output_disable, ref bool p_SDI_state, ref bool p_SDO_state, ref bool p_SCK_state, ref bool p_chip_select_state)
        {
            return SPIM.Get_SPI_Status(ref p_sample_phase, ref p_clock_edge_select, ref p_clock_polarity, ref p_auto_output_disable, ref p_SDI_state, ref p_SDO_state, ref p_SCK_state, ref p_chip_select_state);
        }

        public static int Get_Receive_Wait_Time()
        {
            return Basic.m_spi_receive_wait_time;
        }

        public static bool Receive_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
        {
            return Basic.Send_SPI_Receive_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
        }

        public static bool Send_Data(byte p_byte_count, ref byte[] p_data_array, bool p_assert_cs, bool p_de_assert_cs, ref string p_script_view)
        {
            return Basic.Send_SPI_Send_Cmd(p_byte_count, ref p_data_array, p_assert_cs, p_de_assert_cs, ref p_script_view);
        }

        public static bool Set_Microwire_BitRate(double p_Bit_Rate)
        {
            return SPIM.Set_SPI_BitRate(p_Bit_Rate);
        }

        public static void Set_Receive_Wait_Time(int p_time)
        {
            Basic.m_spi_receive_wait_time = p_time;
        }

        public static bool Tell_PKSA_To_Power_My_Device()
        {
            return SPIM.Tell_PKSA_To_Power_My_Device();
        }

        public static bool Tell_PKSA_To_Use_External_Voltage_Source()
        {
            return SPIM.Tell_PKSA_To_Use_External_Voltage_Source();
        }
    }
}

