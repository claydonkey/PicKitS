namespace PICkitS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class GenericHID
    {
        private const short DIGCF_DEVICEINTERFACE = 0x10;
        private const short DIGCF_PRESENT = 2;
        public const int ERROR_HANDLE_EOF = 0x26;
        public const int ERROR_IO_INCOMPLETE = 0x3e4;
        public const int ERROR_IO_PENDING = 0x3e5;
        private const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const int INVALID_HANDLE_VALUE = -1;
        public static AutoResetEvent m_packet_is_copied = null;
        private static byte[] m_read_buffer = new byte[0x3e8];
        private static Thread m_read_thread;
        internal static Mutex m_usb_packet_mutex = new Mutex(false);
        private static volatile bool m_we_are_in_read_loop = false;
        private const short OPEN_EXISTING = 3;
        public const short WAIT_OBJECT_0 = 0;
        public const int WAIT_TIMEOUT = 0x102;

        public static  event USBNotifier USBDataAvailable;

        public static void Cleanup()
        {
            if (m_read_thread.IsAlive)
            {
                Kill_Read_Thread();
            }
            USBWrite.Kill_Write_Thread();
            USBWrite.Dispose_Of_Write_Objects();
            Utilities.CloseHandle(Utilities.m_flags.HID_write_handle);
            Utilities.CloseHandle(Utilities.m_flags.HID_read_handle);
            m_usb_packet_mutex.Close();
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern int ClearCommError(IntPtr hFile, out uint lpErrors, IntPtr lpStat);
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        public static void Create_Single_Sync_object()
        {
            m_packet_is_copied = new AutoResetEvent(false);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);
        public static bool Find_HID_Device(ushort p_VendorID, ushort p_PoductID)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            Guid empty = Guid.Empty;
            string str = "";
            ushort num = 0;
            bool flag = Get_HID_Device(p_VendorID, p_PoductID, 0, ref zero, ref ptr2, ref str, false, ref empty, ref num);
            if (flag)
            {
                flag = Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                }
            }
            return flag;
        }

        public static bool Find_HID_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, ref Guid p_HidGuid)
        {
            ushort num = 0;
            bool flag = Get_HID_Device(p_VendorID, p_PoductID, p_index, ref p_ReadHandle, ref p_WriteHandle, ref p_devicepath, true, ref p_HidGuid, ref num);
            if (flag)
            {
                flag = Kick_Off_Read_Thread();
                if (flag)
                {
                    flag = USBWrite.kick_off_write_thread();
                }
            }
            return flag;
        }

        public static bool Get_HID_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle, ref string p_devicepath, bool p_pass_ptr_to_handle, ref Guid p_HidGuid, ref ushort p_num_devices_attached)
        {
            SP_DEVICE_INTERFACE_DATA sp_device_interface_data;
            SP_DEVICE_INTERFACE_DETAIL_DATA sp_device_interface_detail_data;
            HIDD_ATTRIBUTES hidd_attributes;
            Utilities.InitializeParams();
            LIN.initialize_LIN_frames();
            IntPtr zero = IntPtr.Zero;
            IntPtr preparsedData = IntPtr.Zero;
            HIDP_CAPS capabilities = new HIDP_CAPS();
            ushort num2 = 0;
            IntPtr hidDeviceObject = IntPtr.Zero;
            int requiredSize = 0;
            SECURITY_ATTRIBUTES structure = new SECURITY_ATTRIBUTES();
            IntPtr ptr4 = new IntPtr(-1);
            structure.lpSecurityDescriptor = 0;
            structure.bInheritHandle = Convert.ToInt32(true);
            structure.nLength = Marshal.SizeOf(structure);
            Guid empty = Guid.Empty;
            sp_device_interface_data.cbSize = 0;
            sp_device_interface_data.Flags = 0;
            sp_device_interface_data.InterfaceClassGuid = Guid.Empty;
            sp_device_interface_data.Reserved = 0;
            sp_device_interface_detail_data.cbSize = 0;
            sp_device_interface_detail_data.DevicePath = "";
            hidd_attributes.ProductID = 0;
            hidd_attributes.Size = 0;
            hidd_attributes.VendorID = 0;
            hidd_attributes.VersionNumber = 0;
            bool flag = false;
            structure.lpSecurityDescriptor = 0;
            structure.bInheritHandle = Convert.ToInt32(true);
            structure.nLength = Marshal.SizeOf(structure);
            HidD_GetHidGuid(ref empty);
            zero = SetupDiGetClassDevs(ref empty, null, 0, 0x12);
            sp_device_interface_data.cbSize = Marshal.SizeOf(sp_device_interface_data);
            for (int i = 0; i < 30; i++)
            {
                if (SetupDiEnumDeviceInterfaces(zero, 0, ref empty, i, ref sp_device_interface_data) != 0)
                {
                    SetupDiGetDeviceInterfaceDetail(zero, ref sp_device_interface_data, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);
                    sp_device_interface_detail_data.cbSize = Marshal.SizeOf(sp_device_interface_detail_data);
                    IntPtr ptr = Marshal.AllocHGlobal(requiredSize);
                    Marshal.WriteInt32(ptr, 4 + Marshal.SystemDefaultCharSize);
                    SetupDiGetDeviceInterfaceDetail(zero, ref sp_device_interface_data, ptr, requiredSize, ref requiredSize, IntPtr.Zero);
                    IntPtr ptr6 = new IntPtr(ptr.ToInt32() + 4);
                    string lpFileName = Marshal.PtrToStringAuto(ptr6);
                    hidDeviceObject = CreateFile(lpFileName, 0xc0000000, 3, ref structure, 3, 0, 0);
                    if (hidDeviceObject != ptr4)
                    {
                        hidd_attributes.Size = Marshal.SizeOf(hidd_attributes);
                        if (HidD_GetAttributes(hidDeviceObject, ref hidd_attributes) != 0)
                        {
                            if ((hidd_attributes.VendorID == p_VendorID) && (hidd_attributes.ProductID == p_PoductID))
                            {
                                if (num2 == p_index)
                                {
                                    flag = true;
                                    if (p_pass_ptr_to_handle)
                                    {
                                        p_WriteHandle = hidDeviceObject;
                                    }
                                    p_devicepath = lpFileName;
                                    p_HidGuid = empty;
                                    Utilities.m_flags.HID_write_handle = hidDeviceObject;
                                    HidD_GetPreparsedData(hidDeviceObject, ref preparsedData);
                                    HidP_GetCaps(preparsedData, ref capabilities);
                                    Utilities.m_flags.irbl = (ushort) capabilities.InputReportByteLength;
                                    Utilities.m_flags.HID_read_handle = CreateFile(lpFileName, 0xc0000000, 3, ref structure, 3, 0x40000000, 0);
                                    if (p_pass_ptr_to_handle)
                                    {
                                        p_ReadHandle = Utilities.m_flags.HID_read_handle;
                                    }
                                    HidD_FreePreparsedData(ref preparsedData);
                                    break;
                                }
                                num2 = (ushort) (num2 + 1);
                            }
                            else
                            {
                                flag = false;
                                CloseHandle(hidDeviceObject);
                            }
                        }
                        else
                        {
                            flag = false;
                            CloseHandle(hidDeviceObject);
                        }
                    }
                }
            }
            SetupDiDestroyDeviceInfoList(zero);
            p_num_devices_attached = num2;
            return flag;
        }

        public static void Get_USB_Data_Packet(ref byte[] p_data)
        {
            for (int i = 0; i < Utilities.m_flags.irbl; i++)
            {
                p_data[i] = m_read_buffer[i];
            }
            m_packet_is_copied.Set();
        }

        public static ushort Get_USB_IRBL()
        {
            return Utilities.m_flags.irbl;
        }

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool GetOverlappedResult(IntPtr hFile, ref Utilities.OVERLAPPED lpOverlapped, ref int lpNumberOfBytesTransferred, int bWait);
        [DllImport("hid.dll")]
        public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);
        [DllImport("hid.dll")]
        public static extern int HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);
        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(ref Guid HidGuid);
        [DllImport("hid.dll")]
        public static extern bool HidD_GetNumInputBuffers(IntPtr HidDeviceObject, ref int NumberBuffers);
        [DllImport("hid.dll")]
        public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);
        [DllImport("hid.dll")]
        public static extern bool HidD_SetNumInputBuffers(IntPtr HidDeviceObject, int NumberBuffers);
        [DllImport("hid.dll")]
        public static extern int HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);
        public static bool Kick_Off_Read_Thread()
        {
            bool flag = true;
            if (!m_we_are_in_read_loop)
            {
                m_read_thread = new Thread(new ThreadStart(GenericHID.Read_USB_Thread));
                m_read_thread.IsBackground = true;
                m_read_thread.Start();
                return flag;
            }
            return false;
        }

        public static void Kill_Read_Thread()
        {
            if ((m_read_thread != null) && m_read_thread.IsAlive)
            {
                m_we_are_in_read_loop = false;
                m_read_thread.Join();
            }
        }

        private static void Read_USB_Thread()
        {
            Utilities.SECURITY_ATTRIBUTES security_attributes;
            int num = 0;
            int pNumberOfBytesRead = 0;
            m_we_are_in_read_loop = true;
            Utilities.OVERLAPPED pOverlapped = new Utilities.OVERLAPPED();
            security_attributes = new Utilities.SECURITY_ATTRIBUTES {
                lpSecurityDescriptor = 0,
                bInheritHandle = Convert.ToInt32(true),
                nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES))
            };
            int num3 = Utilities.CreateEvent(ref security_attributes, Convert.ToInt32(false), Convert.ToInt32(false), "");
            pOverlapped.hEvent = num3;
            while (m_we_are_in_read_loop)
            {
                Array.Clear(m_read_buffer, 0, m_read_buffer.Length);
                pOverlapped.Offset = 0;
                pOverlapped.OffsetHigh = 0;
                if (ReadFile(Utilities.m_flags.HID_read_handle, m_read_buffer, Utilities.m_flags.irbl, ref pNumberOfBytesRead, ref pOverlapped))
                {
                    if (USBDataAvailable != null)
                    {
                        USBDataAvailable();
                        m_packet_is_copied.WaitOne(0x3e8, false);
                    }
                }
                else
                {
                    switch (USB.GetLastError())
                    {
                        case 0x26:
                        {
                            num++;
                            continue;
                        }
                        case 0x3e5:
                        {
                            Utilities.WaitForSingleObject(pOverlapped.hEvent, 0x7d0);
                            if (USB.GetOverlappedResult(Utilities.m_flags.HID_read_handle, ref pOverlapped, ref pNumberOfBytesRead, 0) && (USBDataAvailable != null))
                            {
                                USBDataAvailable();
                                m_packet_is_copied.WaitOne(0x3e8, false);
                            }
                            continue;
                        }
                    }
                    num++;
                }
            }
            num++;
        }

        [DllImport("kernel32", SetLastError=true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] Buffer, int NumberOfBytesToRead, ref int pNumberOfBytesRead, ref Utilities.OVERLAPPED pOverlapped);
        [DllImport("setupapi.dll")]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
        [DllImport("setupapi.dll")]
        public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
        [DllImport("setupapi.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, int hwndParent, int Flags);
        [DllImport("setupapi.dll", CharSet=CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x11)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public int Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public int Reserved;
        }

        public delegate void USBNotifier();
    }
}

