namespace PICkitS
{
    using System;
    using System.Diagnostics;

    public class mTouchLV
    {
        public const byte KEYPAD_SELECTOR = 130;
        public const byte MODE_SELECTOR = 0x86;
        public const byte NUM_SENSORS = 11;
        public const byte READ_BUFFER = 0x90;
        public const byte READ_BUFFER_IS_READY = 0x88;
        public const byte SLAVE_ADDR = 0x42;
        public const byte WRITE_DEFAULTSETTINGS = 0x92;

        public static void Cleanup()
        {
            Device.Cleanup();
        }

        public static bool InitializePksaForMtouchLV()
        {
            return ((Device.Initialize_PICkitSerial() && I2CM.Configure_PICkitSerial_For_I2CMaster()) && Device.Set_Buffer_Flush_Parameters(true, true, 10, 5.0));
        }

        public static bool ReadBuffer(ref byte[] p_data_array)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = 0x45;
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 14;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 2;
            array[6] = 0x42;
            array[7] = 0x90;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = 0x43;
            array[12] = 0x89;
            array[13] = num;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(200, false)) && USBRead.Retrieve_Data(ref p_data_array, num))
            {
                flag = true;
            }
            return flag;
        }

        public static bool ReadBufferIsReady()
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = 1;
            byte[] buffer2 = new byte[1];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 14;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 2;
            array[6] = 0x42;
            array[7] = 0x88;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = 0x43;
            array[12] = 0x89;
            array[13] = num;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && (USBRead.Retrieve_Data(ref buffer2, num) && (buffer2[0] == 1)))
            {
                flag = true;
            }
            return flag;
        }

        public static bool ReadRawSensorData(int p_num_reads, ref string[] p_data_array, ref string p_status_str)
        {
            ushort[] numArray = new ushort[80];
            byte[] array = new byte[80];
            string str = "";
            string str2 = "";
            bool flag = true;
            int num = 0;
            p_status_str = "Successful read";
            int index = 0;
            int num3 = 0;
            if (p_num_reads == 0)
            {
                if (WriteDefaultSettings())
                {
                    p_status_str = "PASS";
                    return true;
                }
                p_status_str = "FAIL";
                return false;
            }
            int num4 = (p_num_reads / 3) + 1;
            for (int i = 0; i < num4; i++)
            {
                num = 0;
                while ((num3 < 100) && !ReadBufferIsReady())
                {
                    num3++;
                }
                if (ReadBufferIsReady())
                {
                    num3 = 0;
                    Array.Clear(array, 0, array.Length);
                    if (ReadBuffer(ref array))
                    {
                        if (array[0] == 1)
                        {
                            for (int j = 0; j < 0x42; j += 2)
                            {
                                numArray[j / 2] = (ushort) (array[j + 3] + (array[j + 4] << 8));
                            }
                            int num7 = array[1] + (array[2] << 8);
                            for (int k = 0; k < 3; k++)
                            {
                                index = (i * 3) + k;
                                if (index >= p_num_reads)
                                {
                                    break;
                                }
                                str = string.Format("{0},", num7);
                                for (int m = 0; m < 11; m++)
                                {
                                    str2 = string.Format("{0},", numArray[m + num]);
                                    str = str + str2;
                                }
                                p_data_array[index] = str;
                                num7++;
                                num += 11;
                            }
                        }
                        else
                        {
                            p_status_str = "1st byte not=1\n";
                            flag = false;
                        }
                    }
                    else
                    {
                        p_status_str = "ReadBuffer function failed\n";
                        flag = false;
                    }
                }
                else
                {
                    p_status_str = "ReadBuffer not ready\n";
                    flag = false;
                }
            }
            return flag;
        }

        internal static bool ReadRawSensorData2(int p_num_reads, ref string[] p_data_array)
        {
            int index = 0;
            string str = "";
            string str2 = "";
            bool flag = true;
            ushort[] numArray = new ushort[11 * p_num_reads];
            long[] numArray2 = new long[p_num_reads];
            if (!ReadRawSensorData2(p_num_reads, ref numArray, ref numArray2))
            {
                return flag;
            }
            for (index = 0; index < p_num_reads; index++)
            {
                str = "";
                for (int i = 0; i < 11; i++)
                {
                    str2 = string.Format("{0}, ", numArray[i + (index * 11)]);
                    str = str + str2;
                }
                p_data_array[index] = string.Format("{0}, {1}", numArray2[index], str);
            }
            return true;
        }

        internal static bool ReadRawSensorData2(int p_num_reads, ref ushort[] p_raw_data_array, ref long[] p_time_array)
        {
            int index = 0;
            bool flag = true;
            byte[] buffer = new byte[0x2c];
            Stopwatch stopwatch = new Stopwatch();
            long[] numArray = new long[p_num_reads];
            stopwatch.Reset();
            stopwatch.Start();
            for (index = 0; index < p_num_reads; index++)
            {
                if (!mTouchCap.ReadRawAvg(0x42, 0, 11, ref buffer))
                {
                    flag = false;
                    break;
                }
                numArray[index] = stopwatch.ElapsedMilliseconds;
                if (index == 0)
                {
                    p_time_array[index] = 0L;
                }
                else
                {
                    p_time_array[index] = numArray[index] - numArray[index - 1];
                }
                for (int i = 0; i < 0x16; i += 2)
                {
                    p_raw_data_array[(i / 2) + (index * 11)] = (ushort) (buffer[i + 1] + (buffer[i] << 8));
                }
            }
            stopwatch.Stop();
            return flag;
        }

        public static bool SelectKeypad(byte p_selection)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = 1;
            byte[] buffer2 = new byte[1];
            Array.Clear(array, 0, array.Length);
            if (p_selection <= 1)
            {
                array[0] = 0;
                array[1] = 3;
                array[2] = 15;
                array[3] = 0x81;
                array[4] = 0x84;
                array[5] = 3;
                array[6] = 0x42;
                array[7] = 130;
                array[8] = p_selection;
                array[9] = 0x83;
                array[10] = 0x84;
                array[11] = 1;
                array[12] = 0x43;
                array[13] = 0x89;
                array[14] = num;
                array[15] = 130;
                array[0x10] = 0x1f;
                array[0x11] = 0x77;
                array[0x12] = 0;
                USBRead.Clear_Data_Array(num);
                USBRead.Clear_Raw_Data_Array();
                if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && (USBRead.Retrieve_Data(ref buffer2, num) && (buffer2[0] == p_selection)))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool SelectMode(byte p_mode)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = 1;
            byte[] buffer2 = new byte[1];
            Array.Clear(array, 0, array.Length);
            if (p_mode <= 2)
            {
                array[0] = 0;
                array[1] = 3;
                array[2] = 15;
                array[3] = 0x81;
                array[4] = 0x84;
                array[5] = 3;
                array[6] = 0x42;
                array[7] = 0x86;
                array[8] = p_mode;
                array[9] = 0x83;
                array[10] = 0x84;
                array[11] = 1;
                array[12] = 0x43;
                array[13] = 0x89;
                array[14] = num;
                array[15] = 130;
                array[0x10] = 0x1f;
                array[0x11] = 0x77;
                array[0x12] = 0;
                USBRead.Clear_Data_Array(num);
                USBRead.Clear_Raw_Data_Array();
                if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && (USBRead.Retrieve_Data(ref buffer2, num) && (buffer2[0] == p_mode)))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool WriteDefaultSettings()
        {
            bool flag = false;
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 8;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 2;
            array[6] = 0x42;
            array[7] = 0x92;
            array[8] = 130;
            array[9] = 0x1f;
            array[10] = 0x77;
            array[11] = 0;
            if (USBWrite.Send_Script_To_PICkitS(ref array))
            {
                flag = true;
            }
            return flag;
        }
    }
}

