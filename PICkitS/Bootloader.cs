namespace PICkitS
{
    using System;
    using System.Threading;

    public class Bootloader
    {
        private const int m_lower_fw_addr = 0x2000;
        private const int m_upper_fw_addr = 0x8000;

        public static bool Clear_64_Bytes_of_Flash(ushort p_addr)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 1;
            array[3] = (byte) p_addr;
            array[4] = (byte) (p_addr >> 8);
            array[5] = 0;
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            if (((USBWrite.Send_Data_Packet_To_PICkitS(ref array) && Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) && ((Utilities.m_flags.bl_buffer[1] == 3) && (Utilities.m_flags.bl_buffer[2] == 1))) && (Utilities.m_flags.bl_buffer[3] == ((byte) p_addr)))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Clear_64_Bytes_of_Flash_return_fail_info(ushort p_addr, ref byte[] p_data, ref bool p_write_result, ref bool p_wait_result)
        {
            bool flag = false;
            bool flag2 = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 1;
            array[3] = (byte) p_addr;
            array[4] = (byte) (p_addr >> 8);
            array[5] = 0;
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            bool flag3 = USBWrite.Send_Data_Packet_To_PICkitS(ref array);
            if (flag3)
            {
                flag2 = Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false);
                if (flag2)
                {
                    if (((Utilities.m_flags.bl_buffer[1] == 3) && (Utilities.m_flags.bl_buffer[2] == 1)) && (Utilities.m_flags.bl_buffer[3] == ((byte) p_addr)))
                    {
                        flag = true;
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            p_data[i] = Utilities.m_flags.bl_buffer[i];
                        }
                    }
                }
            }
            p_write_result = flag3;
            p_wait_result = flag2;
            return flag;
        }

        public static bool Clear_BL_Flash()
        {
            byte num = 0x40;
            for (int i = 0x2000; i < 0x8000; i += num)
            {
                if (!Clear_64_Bytes_of_Flash((ushort) i))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Issue_BL_Reset()
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 0xff;
            if (USBWrite.Send_Data_Packet_To_PICkitS(ref array))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Read_BL_Config_Data(ref byte[] p_data)
        {
            byte num = 14;
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 6;
            array[2] = num;
            array[3] = 0;
            array[4] = 0;
            array[5] = 0x30;
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            if ((!USBWrite.Send_Data_Packet_To_PICkitS(ref array) || !Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) || ((Utilities.m_flags.bl_buffer[1] != 6) || (Utilities.m_flags.bl_buffer[2] != num)))
            {
                return flag;
            }
            for (int i = 6; i < (6 + num); i++)
            {
                p_data[i - 6] = Utilities.m_flags.bl_buffer[i];
            }
            return true;
        }

        public static bool Read_BL_Flash(ushort p_start_addr, int p_byte_count, ref byte[] p_data)
        {
            bool flag2 = false;
            int num = 0;
            byte[] buffer = new byte[0x20];
            ushort num2 = p_start_addr;
            int num3 = p_byte_count / 0x20;
            byte num4 = 0;
            if ((p_byte_count % 0x20) != 0)
            {
                flag2 = true;
                num3++;
            }
            for (int i = 0; i < num3; i++)
            {
                if ((i == (num3 - 1)) && flag2)
                {
                    num4 = (byte) (p_byte_count % 0x20);
                }
                else
                {
                    num4 = 0x20;
                }
                if (!Read_One_BL_Flash_USB_Packet(num2, num4, ref buffer))
                {
                    return false;
                }
                Thread.Sleep(15);
                for (int j = 0; j < num4; j++)
                {
                    p_data[num++] = buffer[j];
                }
                num2 = (ushort) (num2 + num4);
            }
            return true;
        }

        public static bool Read_One_BL_Flash_USB_Packet(ushort p_addr, byte p_byte_count, ref byte[] p_data)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 1;
            array[2] = p_byte_count;
            array[3] = (byte) p_addr;
            array[4] = (byte) (p_addr >> 8);
            array[5] = 0;
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            if ((!USBWrite.Send_Data_Packet_To_PICkitS(ref array) || !Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) || ((Utilities.m_flags.bl_buffer[1] != 1) || (Utilities.m_flags.bl_buffer[2] != p_byte_count)))
            {
                return flag;
            }
            for (int i = 6; i < (6 + p_byte_count); i++)
            {
                p_data[i - 6] = Utilities.m_flags.bl_buffer[i];
            }
            return true;
        }

        public static bool Retrieve_BL_FW_Version_Cmd(ref ushort p_bl_ver)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 0x76;
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            if ((USBWrite.Send_Data_Packet_To_PICkitS(ref array) && Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) && (Utilities.m_flags.bl_buffer[1] == 0x76))
            {
                p_bl_ver = (ushort) ((Utilities.m_flags.bl_buffer[7] << 8) + Utilities.m_flags.bl_buffer[8]);
                flag = true;
            }
            return flag;
        }

        public static bool Write_32_Bytes_to_Flash(ushort p_addr, ref byte[] p_data)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 2;
            array[2] = 0x20;
            array[3] = (byte) p_addr;
            array[4] = (byte) (p_addr >> 8);
            array[5] = 0;
            for (int i = 0; i < 0x20; i++)
            {
                array[6 + i] = p_data[i];
            }
            Utilities.m_flags.g_need_to_copy_bl_data = true;
            if (((USBWrite.Send_Data_Packet_To_PICkitS(ref array) && Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) && ((Utilities.m_flags.bl_buffer[1] == 2) && (Utilities.m_flags.bl_buffer[2] == 0x20))) && (Utilities.m_flags.bl_buffer[3] == ((byte) p_addr)))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Write_BL_Config_Bytes(ref byte[] p_config_data, ref bool[] p_config_bool)
        {
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 7;
            array[2] = 1;
            array[3] = 0;
            array[4] = 0;
            array[5] = 0x30;
            for (int i = 0; i < p_config_data.Length; i++)
            {
                if (p_config_bool[i])
                {
                    array[3] = (byte) i;
                    array[6] = p_config_data[i];
                    Utilities.m_flags.g_need_to_copy_bl_data = true;
                    if ((USBWrite.Send_Data_Packet_To_PICkitS(ref array) && Utilities.m_flags.g_bl_data_arrived_event.WaitOne(0x3e8, false)) && (((Utilities.m_flags.bl_buffer[1] != 7) || (Utilities.m_flags.bl_buffer[2] != 1)) || (Utilities.m_flags.bl_buffer[3] != ((byte) i))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool Write_BL_Flash(ref byte[] p_data)
        {
            int num = 0;
            byte num2 = 0x20;
            byte[] buffer = new byte[num2];
            for (int i = 0x2000; i < 0x8000; i += num2)
            {
                for (int j = 0; j < num2; j++)
                {
                    buffer[j] = p_data[num++];
                }
                if (!Write_32_Bytes_to_Flash((ushort) i, ref buffer))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

