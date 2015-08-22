namespace PICkitS
{
    using System;
    using System.IO;
    using System.Threading;

    public class Firmware
    {
        private static int m_array_size = 0x6000;
        private static int m_bootloader_delay = 30;
        private static bool[] m_config_bool_array = new bool[14];
        private static byte[] m_config_bytes_array = new byte[14];
        private static DEVICETYPE m_devicetype;
        private static byte[] m_flash_data_array = new byte[0x6000];
        private const int m_lower_fw_addr = 0x2000;
        private const int m_max_upper_fw_value = 0x8000;
        private const int m_NUM_CONFIG_BYTES = 14;
        private static byte[] m_prior_flash_data_array = new byte[0x6000];
        private static int m_upper_fw_addr = 0x8000;

        private static bool Clear_BL_Flash()
        {
            byte num = 0x40;
            for (int i = 0x2000; i < m_upper_fw_addr; i += num)
            {
                Thread.Sleep(m_bootloader_delay);
                if (!Bootloader.Clear_64_Bytes_of_Flash((ushort) i))
                {
                    return false;
                }
            }
            return true;
        }

        private static void copy_current_fw_array_into_prior_array()
        {
            for (int i = 0; i < m_array_size; i++)
            {
                m_prior_flash_data_array[i] = m_flash_data_array[i];
            }
        }

        private static bool Force_PKS_Into_Prog_Mode_Cmd()
        {
            bool flag = false;
            ushort num = 0;
            byte[] array = new byte[0x41];
            Array.Clear(array, 0, array.Length);
            array[1] = 0x42;
            array[2] = 0;
            if (Bootloader.Retrieve_BL_FW_Version_Cmd(ref num))
            {
                return true;
            }
            if (USBWrite.Send_Data_Packet_To_PICkitS(ref array))
            {
                restart_the_device();
                if (Bootloader.Retrieve_BL_FW_Version_Cmd(ref num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool Load_Firmware(string p_file_name, int p_device_type, ref string p_error_str, ref int p_error_code)
        {
            bool flag = false;
            if (p_device_type == 2)
            {
                m_devicetype = DEVICETYPE.LIN;
                m_upper_fw_addr = 0x4000;
            }
            else
            {
                m_devicetype = DEVICETYPE.PKSA;
                m_upper_fw_addr = 0x8000;
            }
            m_array_size = m_upper_fw_addr - 0x2000;
            if (!parse_hex_file_put_in_array(p_file_name))
            {
                p_error_code = 1;
                p_error_str = "Error - Could not read hex file - download aborted";
                return flag;
            }
            if (!Force_PKS_Into_Prog_Mode_Cmd())
            {
                p_error_code = 2;
                p_error_str = "Error - Could not enter programming mode prior to updating firmware - download aborted";
                return flag;
            }
            bool flag2 = false;
            byte[] buffer = new byte[14];
            if (!Clear_BL_Flash())
            {
                flag2 = true;
                p_error_str = "Error - clearing flash prior to writing firmware";
                p_error_code = 3;
            }
            else if (!Write_BL_Flash(ref m_flash_data_array))
            {
                p_error_str = "Error - writing flash";
                flag2 = true;
                p_error_code = 4;
            }
            else if (!Bootloader.Write_BL_Config_Bytes(ref m_config_bytes_array, ref m_config_bool_array))
            {
                flag2 = true;
                p_error_str = "Error - writing config bytes";
                p_error_code = 5;
            }
            else
            {
                copy_current_fw_array_into_prior_array();
                if (!Read_BL_Flash(0x2000, m_upper_fw_addr - 0x2000, ref m_flash_data_array))
                {
                    flag2 = true;
                    p_error_str = "Error - reading flash.";
                    p_error_code = 6;
                }
                else
                {
                    for (int i = 0; i < m_array_size; i++)
                    {
                        if (m_flash_data_array[i] != m_prior_flash_data_array[i])
                        {
                            p_error_str = string.Format("Error - Verification error:  flash byte {0:X2} reads as {1:X2}\nbut should be {2:X2}\n", i, m_flash_data_array[i], m_prior_flash_data_array[i]);
                            flag2 = true;
                            p_error_code = 8;
                            break;
                        }
                    }
                    if (Bootloader.Read_BL_Config_Data(ref buffer))
                    {
                        for (int j = 0; j < buffer.Length; j++)
                        {
                            if (m_config_bool_array[j] && (buffer[j] != m_config_bytes_array[j]))
                            {
                                p_error_str = string.Format("Error - Verification error:  Config byte {0:X2} reads as {1:X2}\nbut should be {2:X2}\n", j, buffer[j], m_config_bytes_array[j]);
                                flag2 = true;
                                p_error_code = 9;
                                break;
                            }
                        }
                    }
                    else
                    {
                        flag2 = true;
                        p_error_str = "Error - reading config bytes";
                        p_error_code = 7;
                    }
                }
            }
            if (!flag2)
            {
                if (m_devicetype == DEVICETYPE.LIN)
                {
                    p_error_str = string.Format("LIN firmware written and verified using hex file {0}.\n", p_file_name);
                }
                else
                {
                    p_error_str = string.Format("PICkit Serial firmware written and verified using hex file {0}.\n", p_file_name);
                }
                p_error_code = 0;
                flag = true;
            }
            if (Bootloader.Issue_BL_Reset())
            {
                restart_the_device();
                return flag;
            }
            p_error_code = 10;
            p_error_str = "Error - sending BL Reset Command.  Firmware may not have been updated correctly.";
            return false;
        }

        private static bool parse_hex_file_put_in_array(string p_file_name)
        {
            string str = "";
            string str2 = "0x";
            string str3 = "0x";
            string str4 = "0x";
            string str5 = "0x";
            int num = 0;
            int num2 = 0;
            bool flag = true;
            byte num3 = 0;
            int num4 = 0;
            if (File.Exists(p_file_name))
            {
                " ,\t".ToCharArray();
                try
                {
                    StreamReader reader = File.OpenText(p_file_name);
                    for (int i = 0; i < m_config_bool_array.Length; i++)
                    {
                        m_config_bool_array[i] = false;
                    }
                    while (reader.Peek() >= 0)
                    {
                        str = reader.ReadLine();
                        if ((str != "") && (str[0] == ':'))
                        {
                            str2 = "0x";
                            str3 = "0x";
                            str4 = "0x";
                            str5 = "0x";
                            num2 = Utilities.Convert_Value_To_Int(str2 + str[1] + str[2]);
                            str4 = (str4 + str[3] + str[4]) + str[5] + str[6];
                            str5 = str5 + str[7] + str[8];
                            num = Utilities.Convert_Value_To_Int(str4);
                            num3 = (byte) Utilities.Convert_Value_To_Int(str5);
                            if (num3 == 4)
                            {
                                num4++;
                            }
                            if (((num >= 0x2000) && (num < m_upper_fw_addr)) && (num3 == 0))
                            {
                                for (int j = 0; j < (2 * num2); j += 2)
                                {
                                    str3 = "0x";
                                    str3 = str3 + str[9 + j] + str[10 + j];
                                    m_flash_data_array[(num + (j / 2)) - 0x2000] = (byte) Utilities.Convert_Value_To_Int(str3);
                                }
                            }
                            else if (((num4 >= 2) && (num2 > 0)) && (num3 == 0))
                            {
                                for (int k = 0; k < (2 * num2); k += 2)
                                {
                                    str3 = "0x";
                                    str3 = str3 + str[9 + k] + str[10 + k];
                                    m_config_bytes_array[num + (k / 2)] = (byte) Utilities.Convert_Value_To_Int(str3);
                                    m_config_bool_array[num + (k / 2)] = true;
                                }
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                    flag = false;
                }
                return flag;
            }
            return false;
        }

        private static bool Read_BL_Flash(ushort p_start_addr, int p_byte_count, ref byte[] p_data)
        {
            bool flag2 = false;
            int num = 0;
            byte[] buffer = new byte[0x3a];
            ushort num2 = p_start_addr;
            int num3 = p_byte_count / 0x3a;
            byte num4 = 0;
            if ((p_byte_count % 0x3a) != 0)
            {
                flag2 = true;
                num3++;
            }
            for (int i = 0; i < num3; i++)
            {
                Thread.Sleep(m_bootloader_delay);
                if ((i == (num3 - 1)) && flag2)
                {
                    num4 = (byte) (p_byte_count % 0x3a);
                }
                else
                {
                    num4 = 0x3a;
                }
                if (!Bootloader.Read_One_BL_Flash_USB_Packet(num2, num4, ref buffer))
                {
                    return false;
                }
                for (int j = 0; j < num4; j++)
                {
                    p_data[num++] = buffer[j];
                }
                num2 = (ushort) (num2 + num4);
            }
            return true;
        }

        private static void restart_the_device()
        {
            Device.Terminate_Comm_Threads();
            Thread.Sleep(0xfa0);
            if (m_devicetype == DEVICETYPE.LIN)
            {
                Device.Initialize_MyDevice(0, 0xa04);
            }
            else
            {
                Device.Initialize_PICkitSerial();
            }
            Thread.Sleep(0x7d0);
        }

        private static bool Write_BL_Flash(ref byte[] p_data)
        {
            int num = 0;
            byte num2 = 0x20;
            byte[] buffer = new byte[num2];
            for (int i = 0x2000; i < m_upper_fw_addr; i += num2)
            {
                Thread.Sleep(m_bootloader_delay);
                for (int j = 0; j < num2; j++)
                {
                    buffer[j] = p_data[num++];
                }
                if (!Bootloader.Write_32_Bytes_to_Flash((ushort) i, ref buffer))
                {
                    return false;
                }
            }
            return true;
        }

        private enum DEVICETYPE
        {
            PKSA,
            LIN
        }
    }
}

