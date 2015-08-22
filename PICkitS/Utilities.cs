namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Utilities
    {
        public static COMM_MODE g_comm_mode;
        internal static I2CS_MODE g_i2cs_mode;
        public static FLAGS m_flags;

        public static int AddTwo(int p_int1, int p_int2)
        {
            return (p_int1 + p_int2);
        }

        public static byte calculate_crc8(byte p_data, byte p_start_crc)
        {
            uint num = (byte) ((p_data ^ p_start_crc) & 0xff);
            for (int i = 0; i < 8; i++)
            {
                if ((num & 0x80) == 0x80)
                {
                    num = (num * 2) ^ 0x107;
                }
                else
                {
                    num *= 2;
                }
            }
            return (byte) (num & 0xff);
        }

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        public static int Convert_Value_To_Int(string p_value)
        {
            if (p_value == "")
            {
                return 0;
            }
            uint[] numArray = new uint[] { 
                0, 0, 0x80000000, 0x40000000, 0x20000000, 0x10000000, 0x8000000, 0x4000000, 0x2000000, 0x1000000, 0x800000, 0x400000, 0x200000, 0x100000, 0x80000, 0x40000, 
                0x20000, 0x10000, 0x8000, 0x4000, 0x2000, 0x1000, 0x800, 0x400, 0x200, 0x100, 0x80, 0x40, 0x20, 0x10, 8, 4, 
                2, 1
             };
            uint[] numArray2 = new uint[] { 0, 0, 0x10000000, 0x1000000, 0x100000, 0x10000, 0x1000, 0x100, 0x10, 1 };
            int result = 0;
            if (p_value[0] == '\0')
            {
                return 0;
            }
            if ((p_value[0] == 'Y') || (p_value[0] == 'y'))
            {
                return 1;
            }
            if ((p_value[0] == 'N') || (p_value[0] == 'n'))
            {
                return 0;
            }
            if (p_value.Length > 1)
            {
                int num2;
                int num3;
                int num5;
                if (((p_value[0] == '0') && ((p_value[1] == 'b') || (p_value[1] == 'B'))) || ((p_value[0] == 'b') || (p_value[0] == 'B')))
                {
                    int num4;
                    if (p_value.Length > 0x24)
                    {
                        return 0;
                    }
                    num3 = p_value.Length - 1;
                    if (p_value[0] == '0')
                    {
                        num4 = 2;
                    }
                    else
                    {
                        num4 = 1;
                    }
                    num5 = num4;
                    while (num5 <= num3)
                    {
                        if (p_value[num5] == '1')
                        {
                            num2 = 1;
                        }
                        else
                        {
                            num2 = 0;
                        }
                        result += (int) (numArray[(num5 + 0x22) - p_value.Length] * num2);
                        num5++;
                    }
                    return result;
                }
                if ((p_value[0] == '0') && ((p_value[1] == 'x') || (p_value[1] == 'X')))
                {
                    if (p_value.Length > 12)
                    {
                        return 0;
                    }
                    num3 = p_value.Length - 1;
                    for (num5 = 2; num5 <= num3; num5++)
                    {
                        switch (p_value[num5])
                        {
                            case 'A':
                            case 'a':
                                num2 = 10;
                                break;

                            case 'B':
                            case 'b':
                                num2 = 11;
                                break;

                            case 'C':
                            case 'c':
                                num2 = 12;
                                break;

                            case 'D':
                            case 'd':
                                num2 = 13;
                                break;

                            case 'E':
                            case 'e':
                                num2 = 14;
                                break;

                            case 'F':
                            case 'f':
                                num2 = 15;
                                break;

                            default:
                            {
                                char ch2 = p_value[num5];
                                if (!int.TryParse(ch2.ToString(), out num2))
                                {
                                    num2 = 0;
                                }
                                break;
                            }
                        }
                        result += (int) (numArray2[(num5 + 10) - p_value.Length] * num2);
                    }
                    return result;
                }
                if (!int.TryParse(p_value, out result))
                {
                    result = 0;
                }
                return result;
            }
            if (!int.TryParse(p_value, out result))
            {
                result = 0;
            }
            return result;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int CreateEvent(ref SECURITY_ATTRIBUTES SecurityAttributes, int bManualReset, int bInitialState, string lpName);
        public static void InitializeParams()
        {
            m_flags.HID_write_handle = IntPtr.Zero;
            m_flags.HID_read_handle = IntPtr.Zero;
            m_flags.write_buffer = new byte[0x41];
            m_flags.read_buffer = new byte[0x41];
            m_flags.bl_buffer = new byte[0x41];
            m_flags.orbl = 0x41;
            m_flags.irbl = 0x41;
            m_flags.g_status_packet_mutex = new Mutex(false);
            g_comm_mode = COMM_MODE.IDLE;
            g_i2cs_mode = I2CS_MODE.DEFAULT;
            Constants.STATUS_PACKET_DATA = new byte[0x41];
            Mode.configure_run_mode_arrays();
            m_flags.g_status_packet_data_update_event = new AutoResetEvent(false);
            m_flags.g_data_arrived_event = new AutoResetEvent(false);
            m_flags.g_bl_data_arrived_event = new AutoResetEvent(false);
            m_flags.g_PKSA_has_completed_script = new AutoResetEvent(false);
            m_flags.g_special_status_request_event = new AutoResetEvent(false);
            USBWrite.Initialize_Write_Objects();
            USBRead.Initialize_Read_Objects();
        }

        [DllImport("User32.dll")]
        public static extern int MessageBox(int h, string m, string c, int type);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32", SetLastError=true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] Buffer, int NumberOfBytesToRead, ref int pNumberOfBytesRead, int Overlapped);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern int ReadFileEx(IntPtr hFile, byte[] lpBuffer, int nNumberOfBytesToRead, ref OVERLAPPED lpOverlapped, int lpCompletionRoutine);
        public static void Set_Comm_Mode(byte p_comm_mode, byte p_i2cs_mode)
        {
            switch (p_comm_mode)
            {
                case 0:
                    g_comm_mode = COMM_MODE.IDLE;
                    return;

                case 1:
                    g_comm_mode = COMM_MODE.I2C_M;
                    return;

                case 2:
                    g_comm_mode = COMM_MODE.SPI_M;
                    return;

                case 3:
                    g_comm_mode = COMM_MODE.SPI_S;
                    return;

                case 4:
                    g_comm_mode = COMM_MODE.USART_A;
                    return;

                case 5:
                    g_comm_mode = COMM_MODE.USART_SM;
                    return;

                case 6:
                    g_comm_mode = COMM_MODE.USART_SS;
                    return;

                case 7:
                    g_comm_mode = COMM_MODE.I2C_S;
                    g_i2cs_mode = (I2CS_MODE) p_i2cs_mode;
                    return;

                case 8:
                    g_comm_mode = COMM_MODE.I2C_BBM;
                    return;

                case 9:
                    g_comm_mode = COMM_MODE.I2C_SBBM;
                    return;

                case 10:
                    g_comm_mode = COMM_MODE.LIN;
                    return;

                case 11:
                    g_comm_mode = COMM_MODE.UWIRE;
                    return;

                case 12:
                    g_comm_mode = COMM_MODE.MTOUCH2;
                    return;
            }
            g_comm_mode = COMM_MODE.CM_ERROR;
        }

        [DllImport("kernel32.dll")]
        public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);
        public static bool This_Is_A_Valid_Number(string p_text)
        {
            bool flag = false;
            if (p_text.Length > 0)
            {
                char[] anyOf = "0123456789aAbBcCdDeEfFxX".ToCharArray();
                flag = true;
                for (int i = 0; i < p_text.Length; i++)
                {
                    if (p_text.LastIndexOfAny(anyOf, i, 1) < 0)
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        [DllImport("kernel32.dll")]
        public static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] Buffer, int numBytesToWrite, ref int numBytesWritten, int Overlapped);

        public enum COMM_MODE
        {
            IDLE,
            I2C_M,
            SPI_M,
            SPI_S,
            USART_A,
            USART_SM,
            USART_SS,
            I2C_S,
            I2C_BBM,
            I2C_SBBM,
            LIN,
            UWIRE,
            MTOUCH2,
            CM_ERROR
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FLAGS
        {
            public ushort orbl;
            public ushort irbl;
            public IntPtr HID_write_handle;
            public IntPtr HID_read_handle;
            public byte[] write_buffer;
            public byte[] read_buffer;
            public byte[] bl_buffer;
            public Mutex g_status_packet_mutex;
            public AutoResetEvent g_status_packet_data_update_event;
            public AutoResetEvent g_data_arrived_event;
            public AutoResetEvent g_bl_data_arrived_event;
            public AutoResetEvent g_special_status_request_event;
            internal AutoResetEvent g_PKSA_has_completed_script;
            public volatile bool g_need_to_copy_bl_data;
        }

        internal enum I2CS_MODE
        {
            DEFAULT,
            INTERACTIVE,
            AUTO
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public int bInheritHandle;
        }

        public enum ThreadAccess
        {
            DIRECT_IMPERSONATION = 0x200,
            GET_CONTEXT = 8,
            IMPERSONATE = 0x100,
            QUERY_INFORMATION = 0x40,
            SET_CONTEXT = 0x10,
            SET_INFORMATION = 0x20,
            SET_THREAD_TOKEN = 0x80,
            SUSPEND_RESUME = 2,
            TERMINATE = 1
        }
    }
}

