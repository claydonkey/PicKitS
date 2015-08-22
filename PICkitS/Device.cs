namespace PICkitS
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class Device
    {
        public static void Cleanup()
        {
            Basic.Cleanup();
        }

        public static bool Clear_Comm_Errors()
        {
            bool flag = false;
            I2CS.reset_buffers();
            if (USBWrite.Send_CommReset_Cmd() && USBWrite.Send_Warm_Reset_Cmd())
            {
                flag = true;
            }
            return flag;
        }

        public static void Clear_Status_Errors()
        {
            USBWrite.Clear_Status_Errors();
        }

        public static bool Find_ThisDevice(ushort VendorID, ushort ProductID)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0;
            bool flag = USB.Get_This_Device(VendorID, ProductID, 0, ref zero, ref ptr2, ref str, false, ref empty, ref num);
            if (flag)
            {
                flag = USBRead.Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                }
            }
            return flag;
        }

        public static void Flash_LED1_For_2_Seconds()
        {
            Basic.Flash_LED1_For_2_Seconds();
        }

        public static bool Get_Buffer_Flush_Parameters(ref bool p_flush_on_count, ref bool p_flush_on_time, ref byte p_flush_byte_count, ref double p_flush_interval)
        {
            bool flag = false;
            byte[] array = new byte[0x41];
            if (!(Utilities.m_flags.HID_read_handle != IntPtr.Zero))
            {
                return flag;
            }
            Array.Clear(array, 0, array.Length);
            if (!Basic.Get_Status_Packet(ref array))
            {
                return flag;
            }
            p_flush_on_count = (array[7] & 0x40) > 0;
            p_flush_on_time = (array[7] & 0x80) > 0;
            p_flush_byte_count = array[10];
            p_flush_interval = array[11] * 0.409;
            if (p_flush_interval == 0.0)
            {
                p_flush_interval = 0.409;
            }
            return true;
        }

        public static double Get_Buffer_Flush_Time()
        {
            double num = 9999.0;
            byte[] array = new byte[0x41];
            if (Utilities.m_flags.HID_read_handle != IntPtr.Zero)
            {
                Array.Clear(array, 0, array.Length);
                if (!Basic.Get_Status_Packet(ref array))
                {
                    return 9999.0;
                }
                num = array[11] * 0.409;
                if (num == 0.0)
                {
                    num = 0.409;
                }
            }
            return num;
        }

        public static bool Get_PickitS_DLL_Version(ref string p_version)
        {
            bool flag = false;
            string path = Directory.GetCurrentDirectory() + @"\PICkitS.dll";
            if (File.Exists(path))
            {
                flag = true;
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
                p_version = versionInfo.FileVersion;
                versionInfo = null;
            }
            return flag;
        }

        public static bool Get_PKSA_FW_Version(ref ushort p_version, ref string p_str_fw_ver)
        {
            bool flag = false;
            byte[] buffer = new byte[0x41];
            if (Basic.Get_Status_Packet(ref buffer))
            {
                p_version = (ushort) ((buffer[4] << 8) + buffer[3]);
                p_str_fw_ver = string.Format("0x{0:X4}", (ushort) p_version);
                flag = true;
            }
            return flag;
        }

        public static int Get_Script_Timeout()
        {
            return Basic.Get_Script_Timeout();
        }

        public static bool Get_Script_Timeout_Option()
        {
            return USBWrite.m_use_script_timeout;
        }

        public static bool Get_Status_Packet(ref byte[] p_status_packet)
        {
            return Basic.Get_Status_Packet(ref p_status_packet);
        }

        public static ushort How_Many_Of_MyDevices_Are_Attached(ushort ProductID)
        {
            return Basic.How_Many_Of_MyDevices_Are_Attached(ProductID);
        }

        public static ushort How_Many_PICkitSerials_Are_Attached()
        {
            return Basic.How_Many_PICkitSerials_Are_Attached();
        }

        public static bool Initialize_MyDevice(ushort USBIndex, ushort ProductID)
        {
            return Basic.Initialize_MyDevice(USBIndex, ProductID);
        }

        public static bool Initialize_PICkitSerial()
        {
            return Basic.Initialize_PICkitSerial();
        }

        public static bool Initialize_PICkitSerial(ushort USBIndex)
        {
            return Basic.Initialize_PICkitSerial(USBIndex);
        }

        public static bool ReEstablish_Comm_Threads()
        {
            return Basic.ReEstablish_Comm_Threads();
        }

        public static void Reset_Control_Block()
        {
            Basic.Reset_Control_Block();
        }

        public static bool Set_Buffer_Flush_Parameters(bool p_flush_on_count, bool p_flush_on_time, byte p_flush_byte_count, double p_flush_interval)
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
            if (p_flush_on_count)
            {
                buffer2[7] = (byte) (buffer2[7] | 0x40);
            }
            else
            {
                buffer2[7] = (byte) (buffer2[7] & 0xbf);
            }
            if (p_flush_on_time)
            {
                buffer2[7] = (byte) (buffer2[7] | 0x80);
            }
            else
            {
                buffer2[7] = (byte) (buffer2[7] & 0x7f);
            }
            buffer2[10] = p_flush_byte_count;
            byte num = (byte) Math.Round((double) (p_flush_interval / 0.409));
            buffer2[11] = num;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, false, ref str);
        }

        public static bool Set_Buffer_Flush_Time(double p_time)
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
            byte num = (byte) Math.Round((double) (p_time / 0.409));
            buffer2[11] = num;
            USBWrite.configure_outbound_control_block_packet(ref array, ref str, ref buffer2);
            return USBWrite.write_and_verify_config_block(ref array, ref str2, false, ref str);
        }

        public static void Set_Script_Timeout(int p_time)
        {
            Basic.Set_Script_Timeout(p_time);
        }

        public static void Set_Script_Timeout_Option(bool p_use_timeout)
        {
            USBWrite.m_use_script_timeout = p_use_timeout;
        }

        public static void Terminate_Comm_Threads()
        {
            Basic.Terminate_Comm_Threads();
        }

        public static bool There_Is_A_Status_Error(ref uint p_error)
        {
            return Basic.There_Is_A_Status_Error(ref p_error);
        }
    }
}

