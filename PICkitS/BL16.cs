namespace PICkitS
{
    using System;
    using System.IO;

    public class BL16
    {
        public const uint BLOCK_SIZE = 0x600;

        public static bool BootLoader_Is_Enabled(byte p_slave_addr)
        {
            bool flag = false;
            byte[] buffer = new byte[3];
            if (Read_Status(p_slave_addr, ref buffer) && ((buffer[2] & 1) == 1))
            {
                flag = true;
            }
            return flag;
        }

        public static bool Calculate_Device_Starting_Block_Addr(uint p_start_file_addr, ref uint p_dev_start_addr, ref uint p_end_dev_start_addr)
        {
            uint num = p_start_file_addr / 2;
            int num2 = 0x400;
            int num3 = 0;
            if (num >= 0xc00)
            {
                for (num3 = 2; num3 < 0x3e8; num3++)
                {
                    if ((num3 * num2) == num)
                    {
                        p_dev_start_addr = (uint) (num3 * num2);
                        p_end_dev_start_addr = (uint) (((num3 * num2) + num2) - 1);
                        return true;
                    }
                    if ((num3 * num2) > num)
                    {
                        p_dev_start_addr = (uint) ((num3 - 1) * num2);
                        p_end_dev_start_addr = (uint) ((((num3 - 1) * num2) + num2) - 1);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool Count_Num_Blocks(string p_file_name, ref int p_num_blocks, ref int p_err_code)
        {
            uint num = 0;
            uint num2 = 0;
            uint num3 = 0;
            uint num4 = 0;
            bool flag = true;
            string str = "";
            string str2 = "0x";
            string str3 = "0x";
            string str4 = "0x";
            string str5 = "0x";
            string str6 = "0x";
            int num5 = 0;
            bool flag2 = true;
            byte num6 = 0;
            p_err_code = 0;
            if (File.Exists(p_file_name))
            {
                " ,\t".ToCharArray();
                try
                {
                    StreamReader reader = File.OpenText(p_file_name);
                    p_num_blocks = 0;
                    while (reader.Peek() >= 0)
                    {
                        str = reader.ReadLine();
                        if (str == ":00000001FF")
                        {
                            break;
                        }
                        if ((str != "") && (str[0] == ':'))
                        {
                            str2 = "0x";
                            str3 = "0x";
                            str4 = "0x";
                            str5 = "0x";
                            str6 = "0x";
                            num5 = Utilities.Convert_Value_To_Int(str2 + str[1] + str[2]);
                            str3 = (str3 + str[3] + str[4]) + str[5] + str[6];
                            str4 = str4 + str[7] + str[8];
                            num = (uint) Utilities.Convert_Value_To_Int(str3);
                            num6 = (byte) Utilities.Convert_Value_To_Int(str4);
                            if (num6 == 4)
                            {
                                num4 = (uint) (Utilities.Convert_Value_To_Int((str5 + ((char) str[9]) + ((char) str[10])) + ((char) str[11]) + ((char) str[12])) << 0x10);
                            }
                            else
                            {
                                num += num4;
                                if ((((num / 2) > num3) || ((num / 2) < num2)) && !flag)
                                {
                                    flag = true;
                                }
                                if (flag)
                                {
                                    p_num_blocks++;
                                    flag = false;
                                    if (!Calculate_Device_Starting_Block_Addr(num, ref num2, ref num3))
                                    {
                                        reader.Close();
                                        p_err_code = 1;
                                        return false;
                                    }
                                }
                                for (int i = 0; i < (2 * num5); i += 2)
                                {
                                    str6 = "0x";
                                    str6 = str6 + str[9 + i] + str[10 + i];
                                    if (num2 > (num / 2))
                                    {
                                        reader.Close();
                                        p_err_code = 1;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception)
                {
                    p_err_code = 2;
                    flag2 = false;
                }
                return flag2;
            }
            flag2 = false;
            p_err_code = 2;
            return flag2;
        }

        public static bool Erase_Block(byte p_slave_addr, uint p_mem_addr)
        {
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 11;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 5;
            array[6] = p_slave_addr;
            array[7] = 0;
            array[8] = (byte) (p_mem_addr >> 0x10);
            array[9] = (byte) (p_mem_addr >> 8);
            array[10] = (byte) p_mem_addr;
            array[11] = 130;
            array[12] = 0x1f;
            array[13] = 0x77;
            array[14] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Issue_Password(byte p_slave_addr)
        {
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 12;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 6;
            array[6] = p_slave_addr;
            array[7] = 6;
            array[8] = 0;
            array[9] = 0x11;
            array[10] = 0x22;
            array[11] = 0x33;
            array[12] = 130;
            array[13] = 0x1f;
            array[14] = 0x77;
            array[15] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool LCD_Write(byte p_slave_addr, byte p_LCD, string p_text)
        {
            byte num;
            if (p_text.Length != 0x10)
            {
                return false;
            }
            if (p_LCD == 1)
            {
                num = 0x10;
            }
            else
            {
                num = 0x11;
            }
            int num2 = 0;
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 0x18;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 0x12;
            array[6] = p_slave_addr;
            array[7] = num;
            num2 = 0;
            while (num2 < 0x10)
            {
                array[num2 + 8] = Convert.ToByte(p_text[num2]);
                num2++;
            }
            array[num2 + 8] = 130;
            array[num2 + 9] = 0x1f;
            array[num2 + 10] = 0x77;
            array[num2 + 11] = 0;
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Program(byte p_slave_addr)
        {
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 8;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 2;
            array[6] = p_slave_addr;
            array[7] = 3;
            array[8] = 130;
            array[9] = 0x1f;
            array[10] = 0x77;
            array[11] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Read(byte p_slave_addr, uint p_mem_addr, byte p_num_bytes_to_read, ref byte[] p_data_array)
        {
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 0x11;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 5;
            array[6] = p_slave_addr;
            array[7] = 1;
            array[8] = (byte) (p_mem_addr >> 0x10);
            array[9] = (byte) (p_mem_addr >> 8);
            array[10] = (byte) p_mem_addr;
            array[11] = 0x83;
            array[12] = 0x84;
            array[13] = 1;
            array[14] = (byte) (p_slave_addr + 1);
            array[15] = 0x89;
            array[0x10] = p_num_bytes_to_read;
            array[0x11] = 130;
            array[0x12] = 0x1f;
            array[0x13] = 0x77;
            array[20] = 0;
            USBRead.Clear_Data_Array(p_num_bytes_to_read);
            USBRead.Clear_Raw_Data_Array();
            return (USBWrite.Send_Script_To_PICkitS(ref array) && (Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_read_wait_time, false) && USBRead.Retrieve_Data(ref p_data_array, p_num_bytes_to_read)));
        }

        private bool Read_Block(byte p_slave_addr, uint p_start_addr, ref byte[] p_array)
        {
            byte[] buffer = new byte[0xfc];
            int num = 0;
            uint num2 = p_start_addr;
            for (int i = 0; i < 6; i++)
            {
                if (!Read(p_slave_addr, num2, 0xfc, ref buffer))
                {
                    return false;
                }
                for (int k = 0; k < 0xfc; k++)
                {
                    p_array[num++] = buffer[k];
                }
                num2 += 0xa8;
            }
            if (!Read(p_slave_addr, num2, 0x18, ref buffer))
            {
                return false;
            }
            for (int j = 0; j < 0x18; j++)
            {
                p_array[num++] = buffer[j];
            }
            return true;
        }

        public static bool Read_Status(byte p_slave_addr, ref byte[] p_data_array)
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
            array[7] = 5;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = (byte) (p_slave_addr + 1);
            array[12] = 0x89;
            array[13] = 3;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(3);
            USBRead.Clear_Raw_Data_Array();
            return (USBWrite.Send_Script_To_PICkitS(ref array) && (Utilities.m_flags.g_data_arrived_event.WaitOne(Basic.m_i2cs_read_wait_time, false) && USBRead.Retrieve_Data(ref p_data_array, 3)));
        }

        public static bool Row_Init(byte p_slave_addr)
        {
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 8;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 2;
            array[6] = p_slave_addr;
            array[7] = 4;
            array[8] = 130;
            array[9] = 0x1f;
            array[10] = 0x77;
            array[11] = 0;
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        public static bool Write(byte p_slave_addr, uint p_mem_addr, byte p_num_bytes_to_write, ref byte[] p_data_array)
        {
            int index = 0;
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = (byte) (12 + p_num_bytes_to_write);
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = (byte) (6 + p_num_bytes_to_write);
            array[6] = p_slave_addr;
            array[7] = 2;
            array[8] = (byte) (p_mem_addr >> 0x10);
            array[9] = (byte) (p_mem_addr >> 8);
            array[10] = (byte) p_mem_addr;
            array[11] = p_num_bytes_to_write;
            index = 0;
            while (index < p_num_bytes_to_write)
            {
                array[index + 12] = p_data_array[index];
                index++;
            }
            array[index + 12] = 130;
            array[index + 13] = 0x1f;
            array[index + 14] = 0x77;
            array[index + 15] = 0;
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            return USBWrite.Send_Script_To_PICkitS(ref array);
        }

        private bool Write_Block(byte p_slave_addr, uint p_start_addr, ref byte[] p_array)
        {
            byte[] buffer = new byte[0x30];
            uint num = p_start_addr;
            int num2 = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 0x30; k++)
                    {
                        buffer[k] = p_array[num2++];
                    }
                    if (!Write(p_slave_addr, num, 0x30, ref buffer))
                    {
                        return false;
                    }
                    num += 0x20;
                }
                if (!Program(p_slave_addr))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

