namespace PICkitS
{
    using System;

    public class Status
    {
        private const int CBUF_START_INDEX = 0x35;

        private static void calculate_and_display_I2CM_bit_rate(ref double p_rate, ref string p_units)
        {
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            p_rate = 20000.0 / (4.0 * (Constants.STATUS_PACKET_DATA[0x33] + 1.0));
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            p_units = " kbps";
        }

        private static void calculate_and_display_SPI_bit_rate(ref double p_rate, ref string p_units)
        {
            double num = 0.0;
            double num2 = 0.0;
            p_units = "Mhz";
            p_rate = 0.0;
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            switch (Constants.STATUS_PACKET_DATA[50])
            {
                case 0:
                    num = 8.0;
                    break;

                case 1:
                    num = 32.0;
                    break;

                case 0x10:
                    num = 128.0;
                    break;

                default:
                    num = 0.0;
                    break;
            }
            num2 = Constants.STATUS_PACKET_DATA[0x33] + 1;
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            if (num == 0.0)
            {
                p_rate = 0.0;
            }
            else
            {
                p_rate = (20.0 / num) / num2;
                if (num2 > 125.0)
                {
                    p_rate *= 1000.0;
                    p_units = "Khz";
                }
            }
        }

        private static void calculate_and_display_USART_baud_rate(ref double p_rate, ref string p_units)
        {
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            int num = Constants.STATUS_PACKET_DATA[50] + (Constants.STATUS_PACKET_DATA[0x33] << 8);
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            p_rate = 20.0 / (4.0 * (num + 1.0));
            p_units = "BAUD";
        }

        public static bool There_Is_A_Status_Error(ref uint p_error)
        {
            p_error = 0;
            Utilities.m_flags.g_status_packet_mutex.WaitOne();
            p_error = Constants.STATUS_PACKET_DATA[0x20];
            p_error += (uint) (Constants.STATUS_PACKET_DATA[0x24] << 8);
            p_error += (uint)(Constants.STATUS_PACKET_DATA[0x2c] << 0x10);
            Utilities.m_flags.g_status_packet_mutex.ReleaseMutex();
            if (p_error == 0)
            {
                return false;
            }
            return true;
        }
    }
}

