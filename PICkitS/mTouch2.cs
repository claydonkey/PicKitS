namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class mTouch2
    {
        public const ushort AMAD_USB_PRODUCTID = 80;
        internal static ushort[] m_au1_values = new ushort[0x10];
        internal static ushort[] m_au2_values = new ushort[0x10];
        internal static ushort[] m_avg_values = new ushort[0x10];
        internal static volatile byte m_current_sensor_id = 0;
        internal static DATA_STATUS m_data_status = new DATA_STATUS();
        internal static AutoResetEvent m_detect_data_is_ready = new AutoResetEvent(false);
        internal static byte[] m_detect_values = new byte[5];
        internal static AutoResetEvent m_gdb_data_is_ready = new AutoResetEvent(false);
        internal static ushort[] m_gdb_values = new ushort[0x10];
        private static ushort[] m_local_avg_values = new ushort[0x10];
        private static byte[] m_local_detect_values = new byte[5];
        private static ushort[] m_local_gdb_values = new ushort[0x10];
        private static ushort[] m_local_raw_values = new ushort[0x10];
        private static ushort[] m_local_trp_values = new ushort[0x10];
        internal static volatile byte m_num_current_sensors = 0;
        internal static ushort[] m_raw_values = new ushort[0x10];
        internal static Mutex m_sensor_data_mutex = new Mutex(false);
        internal static Mutex m_sensor_status_mutex = new Mutex(false);
        internal static AutoResetEvent m_status_data_is_ready = new AutoResetEvent(false);
        internal static AutoResetEvent m_trip_data_is_ready = new AutoResetEvent(false);
        internal static ushort[] m_trp_values = new ushort[0x10];
        internal static byte[] m_user_sensor_values = new byte[0x11];
        internal static AutoResetEvent m_user_sensor_values_are_ready = new AutoResetEvent(false);
        private static volatile bool m_we_are_broadcasting = false;
        public const byte MAX_NUM_SENSORS = 0x10;
        internal const byte MT2_ARCHIVE = 2;
        internal const byte MT2_COMM_TAG_WR_USE_USB = 0x26;
        internal const byte MT2_DATA_AUX1 = 0x48;
        internal const byte MT2_DATA_AUX2 = 0x49;
        internal const byte MT2_DATA_AVG = 0x47;
        internal const byte MT2_DATA_DETECT = 0x42;
        internal const byte MT2_DATA_GBAND = 0x45;
        internal const byte MT2_DATA_RAW = 70;
        internal const byte MT2_DATA_STATUS = 0x41;
        internal const byte MT2_DATA_TRIP = 0x44;
        internal const byte MT2_DATA_USERGROUP = 0x43;
        internal const byte MT2_END_OF_DATA = 0;
        internal const byte MT2_RD = 20;
        internal const byte MT2_RD_AUTO = 0x15;
        internal const byte MT2_RD_DETECT = 0x12;
        internal const byte MT2_RD_STATUS = 0x11;
        internal const byte MT2_RD_USERGROUP = 0x13;
        internal const byte MT2_RESET = 1;
        internal const byte MT2_WR_AUX1 = 0x24;
        internal const byte MT2_WR_AUX2 = 0x25;
        internal const byte MT2_WR_GBAND = 0x23;
        internal const byte MT2_WR_TRIP = 0x22;
        internal const byte MT2_WR_USERGROUP = 0x21;
        public const byte NUM_DETECT_BYTES = 5;

        public static  event Broadcast_All_Data broadcast_all_data;

        internal static void broadcast_latest_data()
        {
            if (m_we_are_broadcasting)
            {
                m_sensor_data_mutex.WaitOne();
                for (int i = 0; i < m_num_current_sensors; i++)
                {
                    m_local_raw_values[i] = m_raw_values[i];
                    m_local_avg_values[i] = m_avg_values[i];
                    m_local_trp_values[i] = m_trp_values[i];
                    m_local_gdb_values[i] = m_gdb_values[i];
                }
                for (int j = 0; j < 5; j++)
                {
                    m_local_detect_values[j] = m_detect_values[j];
                }
                m_sensor_data_mutex.ReleaseMutex();
                broadcast_all_data(m_current_sensor_id, m_num_current_sensors, ref m_local_raw_values, ref m_local_avg_values, ref m_local_trp_values, ref m_local_gdb_values, ref m_local_detect_values);
            }
        }

        public static bool Configure_PICkitSerial_For_MTouch2()
        {
            return Basic.Configure_PICkitSerial(12, true);
        }

        public static bool Create_User_Defined_Sensor_Group(byte p_sensor_count, ref byte[] p_sensor_array)
        {
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = 0x21;
            buffer[2] = p_sensor_count;
            for (int i = 3; i < (p_sensor_count + 3); i++)
            {
                buffer[i] = p_sensor_array[i - 3];
            }
            buffer[p_sensor_count + 3] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            byte num2 = 0;
            byte[] buffer2 = new byte[0x11];
            flag = Read_User_Defined_Sensor_Group(ref num2, ref buffer2);
            if (flag && (num2 == p_sensor_count))
            {
                for (int j = 0; j < p_sensor_count; j++)
                {
                    if (buffer2[j] != p_sensor_array[j])
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        public static bool Get_MT2_DATA_STATUS(ref ushort p_comm_fw_ver, ref ushort p_touch_fw_ver, ref byte p_hardware_id, ref byte p_max_num_sensors, ref byte p_broadcast_group_id, ref bool p_trip, ref bool p_gdbnd, ref bool p_raw, ref bool p_avg, ref bool p_detect, ref bool p_aux1, ref bool p_aux2, ref bool p_status, ref ushort p_interval)
        {
            bool flag = false;
            if (Send_MT2_RD_STATUS_Command())
            {
                m_sensor_status_mutex.WaitOne();
                p_comm_fw_ver = m_data_status.comm_fw_ver;
                p_touch_fw_ver = m_data_status.touch_fw_ver;
                p_hardware_id = m_data_status.hardware_id;
                p_max_num_sensors = m_data_status.max_num_sensors;
                p_broadcast_group_id = m_data_status.broadcast_group_id;
                p_trip = m_data_status.broadcast_enable_flags.trip;
                p_gdbnd = m_data_status.broadcast_enable_flags.guardband;
                p_raw = m_data_status.broadcast_enable_flags.raw;
                p_avg = m_data_status.broadcast_enable_flags.avg;
                p_detect = m_data_status.broadcast_enable_flags.detect_flags;
                p_aux1 = m_data_status.broadcast_enable_flags.aux1;
                p_aux2 = m_data_status.broadcast_enable_flags.aux2;
                p_status = m_data_status.broadcast_enable_flags.status;
                p_interval = m_data_status.time_interval;
                m_sensor_status_mutex.ReleaseMutex();
                flag = true;
            }
            return flag;
        }

        public static bool Get_Sensor_Data(byte p_sensor_id, byte p_num_sensors, ref ushort[] p_raw, ref ushort[] p_avg, ref ushort[] p_trip, ref ushort[] p_gdbnd, ref byte[] p_detect)
        {
            bool flag = false;
            m_detect_data_is_ready.Reset();
            if ((!Send_MT2_RD_Command(p_sensor_id, true, true, true, true, true, false, false, false) || !m_detect_data_is_ready.WaitOne(500, false)) || ((p_sensor_id != m_current_sensor_id) || (p_num_sensors != m_num_current_sensors)))
            {
                return flag;
            }
            m_sensor_data_mutex.WaitOne();
            for (int i = 0; i < m_num_current_sensors; i++)
            {
                p_raw[i] = m_raw_values[i];
                p_avg[i] = m_avg_values[i];
                p_trip[i] = m_trp_values[i];
                p_gdbnd[i] = m_gdb_values[i];
            }
            for (int j = 0; j < 5; j++)
            {
                p_detect[j] = m_detect_values[j];
            }
            m_sensor_data_mutex.ReleaseMutex();
            return true;
        }

        public static bool Get_Trip_and_Gdbnd_Data(byte p_sensor_id, int p_num_sensors, ref ushort[] p_trip, ref ushort[] p_gdbnd)
        {
            bool flag = false;
            m_gdb_data_is_ready.Reset();
            if (!Send_MT2_RD_Command(p_sensor_id, true, true, false, false, false, false, false, false) || !m_gdb_data_is_ready.WaitOne(500, false))
            {
                return flag;
            }
            m_sensor_data_mutex.WaitOne();
            for (int i = 0; i < p_num_sensors; i++)
            {
                p_trip[i] = m_trp_values[i];
                p_gdbnd[i] = m_gdb_values[i];
            }
            m_sensor_data_mutex.ReleaseMutex();
            return true;
        }

        public static bool Read_User_Defined_Sensor_Group(ref byte p_sensor_count, ref byte[] p_sensor_array)
        {
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = 0x13;
            buffer[2] = 0;
            m_user_sensor_values_are_ready.Reset();
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            if (flag)
            {
                flag = m_user_sensor_values_are_ready.WaitOne(500, false);
                if (!flag)
                {
                    return flag;
                }
                p_sensor_count = m_user_sensor_values[0];
                for (int i = 0; i < p_sensor_count; i++)
                {
                    p_sensor_array[i] = m_user_sensor_values[1 + i];
                }
            }
            return flag;
        }

        public static bool Send_MT2_ARCHIVE_Command()
        {
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = 2;
            buffer[2] = 0;
            return USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
        }

        public static bool Send_MT2_COMM_TAG_WR_USE_USB_Command(bool p_enable)
        {
            byte[] buffer = new byte[0x41];
            byte num = 0;
            if (p_enable)
            {
                num = 1;
            }
            buffer[0] = 0;
            buffer[1] = 0x26;
            buffer[2] = num;
            buffer[3] = 0;
            return USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
        }

        private static bool Send_MT2_RD_AUTO_Command(byte p_sensor_id, bool p_trip, bool p_gdbnd, bool p_raw, bool p_avg, bool p_detect, bool p_aux1, bool p_aux2, bool p_status, ushort p_interval)
        {
            byte[] buffer = new byte[0x41];
            byte num = 0;
            if (p_trip)
            {
                num = (byte) (num | 1);
            }
            if (p_gdbnd)
            {
                num = (byte) (num | 2);
            }
            if (p_raw)
            {
                num = (byte) (num | 4);
            }
            if (p_avg)
            {
                num = (byte) (num | 8);
            }
            if (p_detect)
            {
                num = (byte) (num | 0x10);
            }
            if (p_aux1)
            {
                num = (byte) (num | 0x20);
            }
            if (p_aux2)
            {
                num = (byte) (num | 0x40);
            }
            if (p_status)
            {
                num = (byte) (num | 0x80);
            }
            buffer[0] = 0;
            buffer[1] = 0x15;
            buffer[2] = p_sensor_id;
            buffer[3] = num;
            buffer[4] = (byte) p_interval;
            buffer[5] = (byte) (p_interval >> 8);
            buffer[6] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            if (p_interval == 0)
            {
                m_we_are_broadcasting = false;
                return flag;
            }
            m_we_are_broadcasting = true;
            return flag;
        }

        private static bool Send_MT2_RD_Command(byte p_sensor_id, bool p_trip, bool p_gdbnd, bool p_raw, bool p_avg, bool p_detect, bool p_aux1, bool p_aux2, bool p_status)
        {
            byte[] buffer = new byte[0x41];
            byte num = 0;
            if (p_trip)
            {
                num = (byte) (num | 1);
            }
            if (p_gdbnd)
            {
                num = (byte) (num | 2);
            }
            if (p_raw)
            {
                num = (byte) (num | 4);
            }
            if (p_avg)
            {
                num = (byte) (num | 8);
            }
            if (p_detect)
            {
                num = (byte) (num | 0x10);
            }
            if (p_aux1)
            {
                num = (byte) (num | 0x20);
            }
            if (p_aux2)
            {
                num = (byte) (num | 0x40);
            }
            if (p_status)
            {
                num = (byte) (num | 0x80);
            }
            buffer[0] = 0;
            buffer[1] = 20;
            buffer[2] = p_sensor_id;
            buffer[3] = num;
            buffer[4] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            m_we_are_broadcasting = false;
            return flag;
        }

        internal static bool Send_MT2_RD_STATUS_Command()
        {
            byte[] buffer = new byte[0x41];
            bool flag = false;
            buffer[0] = 0;
            buffer[1] = 0x11;
            buffer[2] = 0;
            m_status_data_is_ready.Reset();
            if (USBWrite.Send_Data_Packet_To_PICkitS(ref buffer))
            {
                flag = m_status_data_is_ready.WaitOne(500, false);
            }
            return flag;
        }

        public static bool Send_MT2_RESET_Command()
        {
            byte[] buffer = new byte[0x41];
            buffer[0] = 0;
            buffer[1] = 1;
            buffer[2] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            m_we_are_broadcasting = false;
            return flag;
        }

        public static bool Write_Gdbnd_Value(byte p_sensor_id, ushort p_gdbnd)
        {
            Send_MT2_RESET_Command();
            byte[] buffer = new byte[0x41];
            ushort num = 0;
            if (m_we_are_broadcasting && Send_MT2_RD_STATUS_Command())
            {
                num = m_data_status.time_interval;
            }
            buffer[0] = 0;
            buffer[1] = 0x23;
            buffer[2] = p_sensor_id;
            buffer[3] = 2;
            buffer[4] = (byte) p_gdbnd;
            buffer[5] = (byte) (p_gdbnd >> 8);
            buffer[6] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            if (flag)
            {
                m_gdb_data_is_ready.Reset();
                if (Send_MT2_RD_Command(p_sensor_id, false, true, false, false, false, false, false, false))
                {
                    flag = m_gdb_data_is_ready.WaitOne(500, false);
                    if (flag)
                    {
                        m_sensor_data_mutex.WaitOne();
                        if (p_gdbnd != m_gdb_values[0])
                        {
                            flag = false;
                        }
                        m_sensor_data_mutex.ReleaseMutex();
                    }
                }
            }
            if (m_we_are_broadcasting && Send_MT2_RD_STATUS_Command())
            {
                m_data_status.time_interval = num;
                Send_MT2_RD_AUTO_Command(m_data_status.broadcast_group_id, m_data_status.broadcast_enable_flags.trip, m_data_status.broadcast_enable_flags.guardband, m_data_status.broadcast_enable_flags.raw, m_data_status.broadcast_enable_flags.avg, m_data_status.broadcast_enable_flags.detect_flags, m_data_status.broadcast_enable_flags.aux1, m_data_status.broadcast_enable_flags.aux2, m_data_status.broadcast_enable_flags.status, m_data_status.time_interval);
            }
            return flag;
        }

        public static bool Write_Trip_Value(byte p_sensor_id, ushort p_trip)
        {
            Send_MT2_RESET_Command();
            byte[] buffer = new byte[0x41];
            ushort num = 0;
            if (m_we_are_broadcasting && Send_MT2_RD_STATUS_Command())
            {
                num = m_data_status.time_interval;
            }
            buffer[0] = 0;
            buffer[1] = 0x22;
            buffer[2] = p_sensor_id;
            buffer[3] = 2;
            buffer[4] = (byte) p_trip;
            buffer[5] = (byte) (p_trip >> 8);
            buffer[6] = 0;
            bool flag = USBWrite.Send_Data_Packet_To_PICkitS(ref buffer);
            if (flag)
            {
                m_trip_data_is_ready.Reset();
                if (Send_MT2_RD_Command(p_sensor_id, true, false, false, false, false, false, false, false))
                {
                    flag = m_trip_data_is_ready.WaitOne(500, false);
                    if (flag)
                    {
                        m_sensor_data_mutex.WaitOne();
                        if (p_trip != m_trp_values[0])
                        {
                            flag = false;
                        }
                        m_sensor_data_mutex.ReleaseMutex();
                    }
                }
            }
            if (m_we_are_broadcasting && Send_MT2_RD_STATUS_Command())
            {
                m_data_status.time_interval = num;
                Send_MT2_RD_AUTO_Command(m_data_status.broadcast_group_id, m_data_status.broadcast_enable_flags.trip, m_data_status.broadcast_enable_flags.guardband, m_data_status.broadcast_enable_flags.raw, m_data_status.broadcast_enable_flags.avg, m_data_status.broadcast_enable_flags.detect_flags, m_data_status.broadcast_enable_flags.aux1, m_data_status.broadcast_enable_flags.aux2, m_data_status.broadcast_enable_flags.status, m_data_status.time_interval);
            }
            return flag;
        }

        public delegate void Broadcast_All_Data(byte sensor_id, byte num_sensors, ref ushort[] raw, ref ushort[] avg, ref ushort[] trip, ref ushort[] gdbnd, ref byte[] detect);

        [StructLayout(LayoutKind.Sequential)]
        internal struct BROADCAST_ENABLE_FLAGS
        {
            internal bool trip;
            internal bool guardband;
            internal bool raw;
            internal bool avg;
            internal bool detect_flags;
            internal bool aux1;
            internal bool aux2;
            internal bool status;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DATA_STATUS
        {
            internal ushort comm_fw_ver;
            internal ushort touch_fw_ver;
            internal byte hardware_id;
            internal byte max_num_sensors;
            internal byte broadcast_group_id;
            internal mTouch2.BROADCAST_ENABLE_FLAGS broadcast_enable_flags;
            internal ushort time_interval;
        }
    }
}

