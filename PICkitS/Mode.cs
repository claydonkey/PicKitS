namespace PICkitS
{
    using System;

    public class Mode
    {
        private static byte[] m_default_i2c_bbm_data = new byte[0x41];
        private static byte[] m_default_i2c_m_data = new byte[0x41];
        private static byte[] m_default_i2c_s_data = new byte[0x41];
        private static byte[] m_default_i2c_sbbm_data = new byte[0x41];
        private static byte[] m_default_idle_data = new byte[0x41];
        private static byte[] m_default_LIN_data = new byte[0x41];
        private static byte[] m_default_LIN_no_autobaud_data = new byte[0x41];
        private static byte[] m_default_mtouch2_data = new byte[0x41];
        private static byte[] m_default_spi_m_data = new byte[0x41];
        private static byte[] m_default_spi_s_data = new byte[0x41];
        private static byte[] m_default_usart_a_data = new byte[0x41];
        private static byte[] m_default_usart_sm_data = new byte[0x41];
        private static byte[] m_default_usart_ss_data = new byte[0x41];
        private static byte[] m_default_uwire_m_data = new byte[0x41];
        private static byte[] m_test_0_volt_src_data = new byte[0x41];
        private static byte[] m_test_2p5_volt_src_data = new byte[0x41];
        private static byte[] m_test_5_volt_src_data = new byte[0x41];
        private static byte[] m_test_5_volt_src_no_pullup_data = new byte[0x41];
        private static byte[] m_test_i2c_axout_a10_a21_data = new byte[0x41];
        private static byte[] m_test_i2c_axout_a11_a20_data = new byte[0x41];
        private static byte[] m_test_i2c_axout_a11_a21_data = new byte[0x41];
        private static byte[] m_test_i2c_test_sw_enable_data = new byte[0x41];

        public static void configure_run_mode_arrays()
        {
            Array.Clear(m_default_idle_data, 0, m_default_idle_data.Length);
            Array.Clear(m_default_i2c_m_data, 0, m_default_i2c_m_data.Length);
            Array.Clear(m_default_i2c_s_data, 0, m_default_i2c_s_data.Length);
            Array.Clear(m_default_i2c_bbm_data, 0, m_default_i2c_bbm_data.Length);
            Array.Clear(m_default_i2c_sbbm_data, 0, m_default_i2c_sbbm_data.Length);
            Array.Clear(m_default_spi_m_data, 0, m_default_spi_m_data.Length);
            Array.Clear(m_default_spi_s_data, 0, m_default_spi_s_data.Length);
            Array.Clear(m_default_usart_a_data, 0, m_default_usart_a_data.Length);
            Array.Clear(m_default_usart_sm_data, 0, m_default_usart_sm_data.Length);
            Array.Clear(m_default_usart_ss_data, 0, m_default_usart_ss_data.Length);
            Array.Clear(m_default_LIN_data, 0, m_default_LIN_data.Length);
            Array.Clear(m_default_LIN_no_autobaud_data, 0, m_default_LIN_no_autobaud_data.Length);
            Array.Clear(m_default_uwire_m_data, 0, m_default_uwire_m_data.Length);
            Array.Clear(m_default_mtouch2_data, 0, m_default_mtouch2_data.Length);
            Array.Clear(m_test_5_volt_src_data, 0, m_test_5_volt_src_data.Length);
            Array.Clear(m_test_2p5_volt_src_data, 0, m_test_2p5_volt_src_data.Length);
            Array.Clear(m_test_0_volt_src_data, 0, m_test_0_volt_src_data.Length);
            Array.Clear(m_test_5_volt_src_no_pullup_data, 0, m_test_5_volt_src_no_pullup_data.Length);
            Array.Clear(m_test_i2c_axout_a11_a20_data, 0, m_test_i2c_axout_a11_a20_data.Length);
            Array.Clear(m_test_i2c_axout_a10_a21_data, 0, m_test_i2c_axout_a10_a21_data.Length);
            Array.Clear(m_test_i2c_axout_a11_a21_data, 0, m_test_i2c_axout_a11_a21_data.Length);
            Array.Clear(m_test_i2c_test_sw_enable_data, 0, m_test_i2c_test_sw_enable_data.Length);
            m_default_idle_data[7] = 0xc0;
            m_default_idle_data[10] = 10;
            m_default_idle_data[11] = 0xff;
            m_default_idle_data[0x10] = 1;
            m_default_idle_data[0x11] = 0x20;
            m_default_idle_data[0x17] = 1;
            m_default_idle_data[0x18] = 7;
            m_default_i2c_m_data[7] = 0xc0;
            m_default_i2c_m_data[10] = 10;
            m_default_i2c_m_data[11] = 0xff;
            m_default_i2c_m_data[15] = 1;
            m_default_i2c_m_data[0x10] = 0x31;
            m_default_i2c_m_data[0x11] = 0x20;
            m_default_i2c_m_data[0x17] = 3;
            m_default_i2c_m_data[0x18] = 6;
            m_default_i2c_m_data[30] = 0x31;
            m_default_spi_m_data[7] = 0xc0;
            m_default_spi_m_data[10] = 10;
            m_default_spi_m_data[11] = 0xff;
            m_default_spi_m_data[15] = 2;
            m_default_spi_m_data[0x10] = 0x21;
            m_default_spi_m_data[0x11] = 0x20;
            m_default_spi_m_data[0x13] = 0xb0;
            m_default_spi_m_data[20] = 0x2c;
            m_default_spi_m_data[0x17] = 0;
            m_default_spi_m_data[0x18] = 3;
            m_default_spi_m_data[0x1d] = 0;
            m_default_spi_m_data[30] = 0xff;
            m_default_spi_s_data[7] = 0xc0;
            m_default_spi_s_data[10] = 10;
            m_default_spi_s_data[11] = 0xff;
            m_default_spi_s_data[15] = 3;
            m_default_spi_s_data[0x10] = 0x21;
            m_default_spi_s_data[0x11] = 0x20;
            m_default_spi_s_data[0x13] = 0xb0;
            m_default_spi_s_data[20] = 0x2c;
            m_default_spi_s_data[0x17] = 0;
            m_default_spi_s_data[0x18] = 3;
            m_default_spi_s_data[0x1d] = 0;
            m_default_spi_s_data[30] = 0xff;
            m_default_uwire_m_data[7] = 0xc0;
            m_default_uwire_m_data[10] = 10;
            m_default_uwire_m_data[11] = 0xff;
            m_default_uwire_m_data[15] = 11;
            m_default_uwire_m_data[0x10] = 0x21;
            m_default_uwire_m_data[0x11] = 0x20;
            m_default_uwire_m_data[0x13] = 0xb0;
            m_default_uwire_m_data[20] = 0x2c;
            m_default_uwire_m_data[0x17] = 0;
            m_default_uwire_m_data[0x18] = 0x83;
            m_default_uwire_m_data[0x1d] = 0;
            m_default_uwire_m_data[30] = 0xff;
            m_default_usart_a_data[7] = 0xc0;
            m_default_usart_a_data[10] = 10;
            m_default_usart_a_data[11] = 0xff;
            m_default_usart_a_data[15] = 4;
            m_default_usart_a_data[0x10] = 0x21;
            m_default_usart_a_data[0x11] = 0x20;
            m_default_usart_a_data[0x17] = 1;
            m_default_usart_a_data[0x18] = 0;
            m_default_usart_a_data[0x19] = 15;
            m_default_usart_a_data[30] = 0x10;
            m_default_usart_sm_data[7] = 0xc0;
            m_default_usart_sm_data[10] = 10;
            m_default_usart_sm_data[11] = 0xff;
            m_default_usart_sm_data[15] = 5;
            m_default_usart_sm_data[0x10] = 0x21;
            m_default_usart_sm_data[0x11] = 0x20;
            m_default_usart_sm_data[0x17] = 1;
            m_default_usart_sm_data[0x18] = 0;
            m_default_usart_sm_data[0x19] = 15;
            m_default_usart_sm_data[30] = 0x10;
            m_default_usart_ss_data[7] = 0xc0;
            m_default_usart_ss_data[10] = 10;
            m_default_usart_ss_data[11] = 0xff;
            m_default_usart_ss_data[15] = 6;
            m_default_usart_ss_data[0x10] = 0x21;
            m_default_usart_ss_data[0x11] = 0x20;
            m_default_usart_ss_data[0x17] = 1;
            m_default_usart_ss_data[0x18] = 0;
            m_default_usart_ss_data[0x19] = 15;
            m_default_usart_ss_data[30] = 0x10;
            m_default_i2c_s_data[7] = 0xc0;
            m_default_i2c_s_data[10] = 10;
            m_default_i2c_s_data[11] = 0xff;
            m_default_i2c_s_data[15] = 7;
            m_default_i2c_s_data[0x10] = 0x21;
            m_default_i2c_s_data[0x11] = 0x20;
            m_default_i2c_s_data[0x17] = 1;
            m_default_i2c_s_data[0x18] = 0xff;
            m_default_i2c_s_data[0x19] = 6;
            m_default_i2c_s_data[0x1b] = 0;
            m_default_i2c_s_data[0x1c] = 0;
            m_default_i2c_s_data[0x1d] = 0;
            m_default_i2c_s_data[30] = 0;
            m_default_i2c_bbm_data[7] = 0xc0;
            m_default_i2c_bbm_data[10] = 10;
            m_default_i2c_bbm_data[11] = 0xff;
            m_default_i2c_bbm_data[15] = 8;
            m_default_i2c_bbm_data[0x10] = 0x21;
            m_default_i2c_bbm_data[0x11] = 0x20;
            m_default_i2c_bbm_data[0x17] = 3;
            m_default_i2c_bbm_data[0x18] = 6;
            m_default_i2c_bbm_data[30] = 0x7f;
            m_default_i2c_sbbm_data[7] = 0xc0;
            m_default_i2c_sbbm_data[10] = 10;
            m_default_i2c_sbbm_data[11] = 0xff;
            m_default_i2c_sbbm_data[15] = 9;
            m_default_i2c_sbbm_data[0x10] = 0x21;
            m_default_i2c_sbbm_data[0x11] = 0x20;
            m_default_i2c_sbbm_data[0x17] = 3;
            m_default_i2c_sbbm_data[0x18] = 6;
            m_default_i2c_sbbm_data[30] = 0x7f;
            m_default_LIN_data[7] = 0xc0;
            m_default_LIN_data[10] = 0x35;
            m_default_LIN_data[11] = 0x7c;
            m_default_LIN_data[15] = 10;
            m_default_LIN_data[0x10] = 0x33;
            m_default_LIN_data[0x11] = 0x20;
            m_default_LIN_data[0x17] = 200;
            m_default_LIN_data[0x18] = 0x98;
            m_default_LIN_data[0x19] = 15;
            m_default_LIN_data[0x1d] = 0xf3;
            m_default_LIN_data[30] = 1;
            m_default_LIN_no_autobaud_data[7] = 0xc0;
            m_default_LIN_no_autobaud_data[10] = 0x35;
            m_default_LIN_no_autobaud_data[11] = 0x7c;
            m_default_LIN_no_autobaud_data[15] = 10;
            m_default_LIN_no_autobaud_data[0x10] = 0x33;
            m_default_LIN_no_autobaud_data[0x11] = 0x20;
            m_default_LIN_no_autobaud_data[0x17] = 0x48;
            m_default_LIN_no_autobaud_data[0x18] = 0x98;
            m_default_LIN_no_autobaud_data[0x19] = 15;
            m_default_LIN_no_autobaud_data[0x1d] = 0xf3;
            m_default_LIN_no_autobaud_data[30] = 1;
            m_default_mtouch2_data[7] = 0xc0;
            m_default_mtouch2_data[10] = 10;
            m_default_mtouch2_data[11] = 0xff;
            m_default_mtouch2_data[15] = 12;
            m_default_mtouch2_data[0x10] = 0x31;
            m_default_mtouch2_data[0x11] = 0x20;
            m_default_mtouch2_data[0x17] = 3;
            m_default_mtouch2_data[0x18] = 6;
            m_default_mtouch2_data[30] = 0x31;
            m_test_5_volt_src_data[7] = 0xc0;
            m_test_5_volt_src_data[10] = 10;
            m_test_5_volt_src_data[11] = 0xff;
            m_test_5_volt_src_data[15] = 1;
            m_test_5_volt_src_data[0x10] = 0x71;
            m_test_5_volt_src_data[0x11] = 0x20;
            m_test_5_volt_src_data[0x13] = 0xff;
            m_test_5_volt_src_data[20] = 0x3f;
            m_test_5_volt_src_data[0x17] = 3;
            m_test_5_volt_src_data[0x18] = 6;
            m_test_5_volt_src_data[30] = 0x7f;
            m_test_2p5_volt_src_data[7] = 0xc0;
            m_test_2p5_volt_src_data[10] = 10;
            m_test_2p5_volt_src_data[11] = 0xff;
            m_test_2p5_volt_src_data[15] = 1;
            m_test_2p5_volt_src_data[0x10] = 0x71;
            m_test_2p5_volt_src_data[0x11] = 0x20;
            m_test_2p5_volt_src_data[0x13] = 120;
            m_test_2p5_volt_src_data[20] = 30;
            m_test_2p5_volt_src_data[0x17] = 3;
            m_test_2p5_volt_src_data[0x18] = 6;
            m_test_2p5_volt_src_data[30] = 0x7f;
            m_test_0_volt_src_data[7] = 0xc0;
            m_test_0_volt_src_data[10] = 10;
            m_test_0_volt_src_data[11] = 0xff;
            m_test_0_volt_src_data[15] = 1;
            m_test_0_volt_src_data[0x10] = 0x71;
            m_test_0_volt_src_data[0x11] = 0x20;
            m_test_0_volt_src_data[0x17] = 3;
            m_test_0_volt_src_data[0x18] = 6;
            m_test_0_volt_src_data[30] = 0x7f;
            m_test_5_volt_src_no_pullup_data[7] = 0xc0;
            m_test_5_volt_src_no_pullup_data[10] = 10;
            m_test_5_volt_src_no_pullup_data[11] = 0xff;
            m_test_5_volt_src_no_pullup_data[15] = 1;
            m_test_5_volt_src_no_pullup_data[0x10] = 0x61;
            m_test_5_volt_src_no_pullup_data[0x11] = 0x20;
            m_test_5_volt_src_no_pullup_data[0x13] = 0xff;
            m_test_5_volt_src_no_pullup_data[20] = 0x3f;
            m_test_5_volt_src_no_pullup_data[0x17] = 3;
            m_test_5_volt_src_no_pullup_data[0x18] = 6;
            m_test_5_volt_src_no_pullup_data[30] = 0x7f;
            m_test_i2c_axout_a11_a20_data[7] = 0xc0;
            m_test_i2c_axout_a11_a20_data[10] = 10;
            m_test_i2c_axout_a11_a20_data[11] = 0xff;
            m_test_i2c_axout_a11_a20_data[15] = 1;
            m_test_i2c_axout_a11_a20_data[0x10] = 0x31;
            m_test_i2c_axout_a11_a20_data[0x11] = 0x20;
            m_test_i2c_axout_a11_a20_data[0x17] = 3;
            m_test_i2c_axout_a11_a20_data[0x18] = 6;
            m_test_i2c_axout_a11_a20_data[0x1c] = 1;
            m_test_i2c_axout_a11_a20_data[30] = 0x7f;
            m_test_i2c_axout_a10_a21_data[7] = 0xc0;
            m_test_i2c_axout_a10_a21_data[10] = 160;
            m_test_i2c_axout_a10_a21_data[11] = 0xff;
            m_test_i2c_axout_a10_a21_data[15] = 1;
            m_test_i2c_axout_a10_a21_data[0x10] = 0x31;
            m_test_i2c_axout_a10_a21_data[0x11] = 0x20;
            m_test_i2c_axout_a10_a21_data[0x17] = 3;
            m_test_i2c_axout_a10_a21_data[0x18] = 6;
            m_test_i2c_axout_a10_a21_data[0x1c] = 2;
            m_test_i2c_axout_a10_a21_data[30] = 0x7f;
            m_test_i2c_axout_a11_a21_data[7] = 0xc0;
            m_test_i2c_axout_a11_a21_data[10] = 160;
            m_test_i2c_axout_a11_a21_data[11] = 0xff;
            m_test_i2c_axout_a11_a21_data[15] = 1;
            m_test_i2c_axout_a11_a21_data[0x10] = 0x31;
            m_test_i2c_axout_a11_a21_data[0x11] = 0x20;
            m_test_i2c_axout_a11_a21_data[0x17] = 3;
            m_test_i2c_axout_a11_a21_data[0x18] = 6;
            m_test_i2c_axout_a11_a21_data[0x1c] = 3;
            m_test_i2c_axout_a11_a21_data[30] = 0x7f;
            m_test_i2c_test_sw_enable_data[7] = 0xc0;
            m_test_i2c_test_sw_enable_data[10] = 160;
            m_test_i2c_test_sw_enable_data[8] = 1;
            m_test_i2c_test_sw_enable_data[10] = 0;
            m_test_i2c_test_sw_enable_data[11] = 0xff;
            m_test_i2c_test_sw_enable_data[15] = 1;
            m_test_i2c_test_sw_enable_data[0x10] = 0x31;
            m_test_i2c_test_sw_enable_data[0x11] = 0x20;
            m_test_i2c_test_sw_enable_data[0x17] = 3;
            m_test_i2c_test_sw_enable_data[0x18] = 6;
            m_test_i2c_test_sw_enable_data[30] = 0x7f;
        }

        public static void update_status_packet_data(int p_index, ref byte[] p_status_packet_data)
        {
            Device.Set_Script_Timeout_Option(true);
            switch (p_index)
            {
                case 0:
                    for (int i = 7; i < m_default_idle_data.Length; i++)
                    {
                        p_status_packet_data[i] = m_default_idle_data[i];
                    }
                    return;

                case 1:
                    for (int j = 7; j < m_default_idle_data.Length; j++)
                    {
                        p_status_packet_data[j] = m_default_i2c_m_data[j];
                    }
                    return;

                case 2:
                    for (int k = 7; k < m_default_idle_data.Length; k++)
                    {
                        p_status_packet_data[k] = m_default_spi_m_data[k];
                    }
                    return;

                case 3:
                    for (int m = 7; m < m_default_idle_data.Length; m++)
                    {
                        p_status_packet_data[m] = m_default_spi_s_data[m];
                    }
                    return;

                case 4:
                    for (int n = 7; n < m_default_idle_data.Length; n++)
                    {
                        p_status_packet_data[n] = m_default_usart_a_data[n];
                    }
                    return;

                case 5:
                    for (int num6 = 7; num6 < m_default_idle_data.Length; num6++)
                    {
                        p_status_packet_data[num6] = m_default_usart_sm_data[num6];
                    }
                    return;

                case 6:
                    for (int num7 = 7; num7 < m_default_idle_data.Length; num7++)
                    {
                        p_status_packet_data[num7] = m_default_usart_ss_data[num7];
                    }
                    return;

                case 7:
                    for (int num8 = 7; num8 < m_default_idle_data.Length; num8++)
                    {
                        p_status_packet_data[num8] = m_default_i2c_s_data[num8];
                    }
                    return;

                case 8:
                    for (int num9 = 7; num9 < m_default_idle_data.Length; num9++)
                    {
                        p_status_packet_data[num9] = m_default_i2c_bbm_data[num9];
                    }
                    return;

                case 9:
                    for (int num10 = 7; num10 < m_default_idle_data.Length; num10++)
                    {
                        p_status_packet_data[num10] = m_default_i2c_sbbm_data[num10];
                    }
                    return;

                case 10:
                    Device.Set_Script_Timeout_Option(false);
                    for (int num11 = 7; num11 < m_default_idle_data.Length; num11++)
                    {
                        p_status_packet_data[num11] = m_default_LIN_data[num11];
                    }
                    return;

                case 11:
                    for (int num12 = 7; num12 < m_default_idle_data.Length; num12++)
                    {
                        p_status_packet_data[num12] = m_default_uwire_m_data[num12];
                    }
                    return;

                case 12:
                case 13:
                case 14:
                case 15:
                case 0x10:
                case 0x11:
                case 0x12:
                    break;

                case 0x13:
                    Device.Set_Script_Timeout_Option(false);
                    for (int num13 = 7; num13 < m_default_idle_data.Length; num13++)
                    {
                        p_status_packet_data[num13] = m_default_LIN_no_autobaud_data[num13];
                    }
                    return;

                case 20:
                    for (int num14 = 7; num14 < m_default_idle_data.Length; num14++)
                    {
                        p_status_packet_data[num14] = m_test_5_volt_src_data[num14];
                    }
                    return;

                case 0x15:
                    for (int num15 = 7; num15 < m_default_idle_data.Length; num15++)
                    {
                        p_status_packet_data[num15] = m_test_2p5_volt_src_data[num15];
                    }
                    return;

                case 0x16:
                    for (int num16 = 7; num16 < m_default_idle_data.Length; num16++)
                    {
                        p_status_packet_data[num16] = m_test_0_volt_src_data[num16];
                    }
                    return;

                case 0x17:
                    for (int num17 = 7; num17 < m_default_idle_data.Length; num17++)
                    {
                        p_status_packet_data[num17] = m_test_5_volt_src_no_pullup_data[num17];
                    }
                    return;

                case 0x18:
                    for (int num18 = 7; num18 < m_default_idle_data.Length; num18++)
                    {
                        p_status_packet_data[num18] = m_test_i2c_axout_a11_a20_data[num18];
                    }
                    return;

                case 0x19:
                    for (int num19 = 7; num19 < m_default_idle_data.Length; num19++)
                    {
                        p_status_packet_data[num19] = m_test_i2c_axout_a10_a21_data[num19];
                    }
                    return;

                case 0x1a:
                    for (int num20 = 7; num20 < m_default_idle_data.Length; num20++)
                    {
                        p_status_packet_data[num20] = m_test_i2c_axout_a11_a21_data[num20];
                    }
                    return;

                case 0x1b:
                    for (int num21 = 7; num21 < m_default_idle_data.Length; num21++)
                    {
                        p_status_packet_data[num21] = m_test_i2c_test_sw_enable_data[num21];
                    }
                    break;

                default:
                    return;
            }
        }
    }
}

