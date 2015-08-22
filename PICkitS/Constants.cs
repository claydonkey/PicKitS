namespace PICkitS
{
    using System;

    public class Constants
    {
        public const byte BIT_MASK_0 = 1;
        public const byte BIT_MASK_1 = 2;
        public const byte BIT_MASK_2 = 4;
        public const byte BIT_MASK_3 = 8;
        public const byte BIT_MASK_4 = 0x10;
        public const byte BIT_MASK_5 = 0x20;
        public const byte BIT_MASK_6 = 0x40;
        public const byte BIT_MASK_7 = 0x80;
        public const int CB_START_INDEX = 7;
        public const int CBUF_START_INDEX = 0x35;
        public const byte CBUF1_WRITE = 1;
        public const double FOSC = 20.0;
        public const ushort LIN_PRODUCT_ID = 0xa04;
        public const uint MAX_ARRAY_SIZE = 0x5000;
        public const uint MAX_NUM_BYTES_IN_CBUF1 = 0xff;
        public const uint PACKET_SIZE = 0x41;
        public const int SCRIPT_COMPLETE_MARKER = 0x77;
        public const int START_OF_STATUS_BLOCK = 0x20;
        public static byte[] STATUS_PACKET_DATA;
    }
}

