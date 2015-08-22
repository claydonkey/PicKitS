namespace PICkitS
{
    using System;

    public class mTouchCap
    {
        private const byte CHANGE_CSM_DAUGHTER_BOARD_CMD = 150;
        private const byte READ_ALL_DATA_CMD = 12;
        private const byte READ_DEVTOOL_DATA_CMD = 0x55;
        private const byte READ_FIRMWARE_OPTIONS_CMD = 0x94;
        private const byte READ_NUM_SENSORS_CMD = 0;
        private const byte READ_RAW_AVG_CMD = 3;
        private const byte READ_TRIGGER_DATA = 0xee;
        private const byte WRITE_SENSOR_DATA = 0x30;

        public static bool ChangeCSMDaughterBoard(byte p_slave_addr, byte p_board)
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
            array[6] = p_slave_addr;
            array[7] = 150;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = (byte) (p_slave_addr + 1);
            array[12] = 0x89;
            array[13] = num;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && (USBRead.Retrieve_Data(ref buffer2, num) && (buffer2[0] == p_board)))
            {
                flag = true;
            }
            return flag;
        }

        public static bool ReadAllData(byte p_slave_addr, byte p_index, byte p_num_sensors, ref byte[] p_data_array)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = (byte) (p_num_sensors * 8);
            if ((p_num_sensors * 8) > 0xff)
            {
                return false;
            }
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 0x10;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 4;
            array[6] = p_slave_addr;
            array[7] = 12;
            array[8] = p_index;
            array[9] = p_num_sensors;
            array[10] = 0x83;
            array[11] = 0x84;
            array[12] = 1;
            array[13] = (byte) (p_slave_addr + 1);
            array[14] = 0x89;
            array[15] = num;
            array[0x10] = 130;
            array[0x11] = 0x1f;
            array[0x12] = 0x77;
            array[0x13] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(200, false)) && USBRead.Retrieve_Data(ref p_data_array, num))
            {
                flag = true;
            }
            return flag;
        }

        public static bool ReadDevToolData(byte p_slave_addr, byte p_num_bytes, ref byte[] p_data_array)
        {
            string str = "";
            return I2CM.Read(p_slave_addr, 0x55, p_num_bytes, ref p_data_array, ref str);
        }

        public static bool ReadFirmwareOptions(byte p_slave_addr, ref byte p_options)
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
            array[6] = p_slave_addr;
            array[7] = 0x94;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = (byte) (p_slave_addr + 1);
            array[12] = 0x89;
            array[13] = num;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && USBRead.Retrieve_Data(ref buffer2, num))
            {
                flag = true;
                p_options = buffer2[0];
            }
            return flag;
        }

        public static bool ReadNumSensors(byte p_slave_addr, ref byte p_num_sensors)
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
            array[6] = p_slave_addr;
            array[7] = 0;
            array[8] = 0x83;
            array[9] = 0x84;
            array[10] = 1;
            array[11] = (byte) (p_slave_addr + 1);
            array[12] = 0x89;
            array[13] = num;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(50, false)) && USBRead.Retrieve_Data(ref buffer2, num))
            {
                flag = true;
                p_num_sensors = buffer2[0];
            }
            return flag;
        }

        public static bool ReadRawAvg(byte p_slave_addr, byte p_index, byte p_num_sensors, ref byte[] p_data_array)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = (byte) (p_num_sensors * 4);
            if ((p_num_sensors * 4) <= 0xff)
            {
                Array.Clear(array, 0, array.Length);
                array[0] = 0;
                array[1] = 3;
                array[2] = 0x10;
                array[3] = 0x81;
                array[4] = 0x84;
                array[5] = 4;
                array[6] = p_slave_addr;
                array[7] = 3;
                array[8] = p_index;
                array[9] = p_num_sensors;
                array[10] = 0x83;
                array[11] = 0x84;
                array[12] = 1;
                array[13] = (byte) (p_slave_addr + 1);
                array[14] = 0x89;
                array[15] = num;
                array[0x10] = 130;
                array[0x11] = 0x1f;
                array[0x12] = 0x77;
                array[0x13] = 0;
                USBRead.Clear_Data_Array(num);
                USBRead.Clear_Raw_Data_Array();
                if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(200, false)) && USBRead.Retrieve_Data(ref p_data_array, num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public static bool ReadTriggerData(byte p_slave_addr, ref byte[] p_data_array)
        {
            bool flag = false;
            byte[] array = new byte[300];
            byte num = 3;
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 0x10;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 4;
            array[6] = p_slave_addr;
            array[7] = 0xee;
            array[8] = 0x11;
            array[9] = 0x12;
            array[10] = 0x83;
            array[11] = 0x84;
            array[12] = 1;
            array[13] = (byte) (p_slave_addr + 1);
            array[14] = 0x89;
            array[15] = num;
            array[0x10] = 130;
            array[0x11] = 0x1f;
            array[0x12] = 0x77;
            array[0x13] = 0;
            USBRead.Clear_Data_Array(num);
            USBRead.Clear_Raw_Data_Array();
            if ((USBWrite.Send_Script_To_PICkitS(ref array) && Utilities.m_flags.g_data_arrived_event.WaitOne(200, false)) && USBRead.Retrieve_Data(ref p_data_array, num))
            {
                flag = true;
            }
            return flag;
        }

        public static bool WriteTripGuardband(byte p_slave_addr, byte p_index, ushort p_trip, ushort p_guardband)
        {
            bool flag = false;
            byte[] array = new byte[300];
            Array.Clear(array, 0, array.Length);
            array[0] = 0;
            array[1] = 3;
            array[2] = 14;
            array[3] = 0x81;
            array[4] = 0x84;
            array[5] = 8;
            array[6] = p_slave_addr;
            array[7] = 0x30;
            array[8] = p_index;
            array[9] = 1;
            array[10] = (byte) (p_trip >> 8);
            array[11] = (byte) p_trip;
            array[12] = (byte) (p_guardband >> 8);
            array[13] = (byte) p_guardband;
            array[14] = 130;
            array[15] = 0x1f;
            array[0x10] = 0x77;
            array[0x11] = 0;
            USBRead.Clear_Data_Array(0);
            USBRead.Clear_Raw_Data_Array();
            if (USBWrite.Send_Script_To_PICkitS(ref array))
            {
                flag = true;
            }
            return flag;
        }
    }
}

