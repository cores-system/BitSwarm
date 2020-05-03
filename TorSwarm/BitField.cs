﻿using System;

namespace SuRGeoNix.TorSwarm
{
    public class BitField
    {
        public byte[]   bitfield;
        public int      size;
        public int      setsCounter;

        private static readonly object locker = new object();

        public BitField(int size)
        {
            bitfield        = new byte[((size-1)/8) + 1];
            this.size       = size;
            setsCounter     = 0;
        }
        public BitField(byte[] bitfield, int size)
        {
            if ( size > (bitfield.Length * 8) || size <= ((bitfield.Length-1) * 8) ) throw new Exception("Out of range");

            this.size       = size;
            this.bitfield   = bitfield;
            setsCounter     = 0;
        }

        public void SetBit(int input)
        {
            if ( input >= size ) throw new Exception("Out of range");

            int bytePos = input / 8;
            int bitPos  = input % 8;

            lock ( locker )
            {
                if ( !GetBit(input) ) setsCounter++;
                bitfield[bytePos] |= (byte) (1 << (7-bitPos));
            }
        }
        public void UnSetBit(int input)
        {
            if ( input >= size ) throw new Exception("Out of range");

            int bytePos = input / 8;
            int bitPos  = input % 8;

            lock ( locker )
            {
                if ( GetBit(input) ) setsCounter--;
                bitfield[bytePos] &= (byte) ~(1 << (7-bitPos));
            }
        }
        public bool GetBit(int input)
        {
            if ( input >= size ) throw new Exception("Out of range");

            int bytePos = input / 8;
            int bitPos  = input % 8;

            lock ( locker ) 
                if ( (bitfield[bytePos] & (1 << (7-bitPos))) > 0 ) return true;

            return false;
        }

        public int GetFirst0()
        {
            int bytePos = 0;

            lock ( locker )
            {
                for (;bytePos<bitfield.Length; bytePos++)
                    if ( bitfield[bytePos] != 0xff ) break;

                for (int i=bytePos*8; i<(bytePos*8) + 8; i++)
                    if ( i<size && !GetBit(i) ) return i;
            }

            return -1;
        }
        
        public void PrintBitField()
        {
            Console.WriteLine($"====== BitField ({size}) ======");
            for (int i=0; i<bitfield.Length-1; i++)
            {
                Console.Write(Convert.ToString(bitfield[i], 2).PadLeft(8, '0'));
            }
            Console.Write(Convert.ToString(bitfield[bitfield.Length-1], 2).PadLeft(8, '0').Substring(0,((size-1)%8)+1));
            Console.WriteLine("\n==============================");
        }
        public void SetSize(int size) { this.size = size; }
        public int GetSize() { return size; }
        public void SetBitfield(byte[] bitfield) { this.bitfield = bitfield; }
        public byte[] GetBitfield() { return bitfield; }
    }
}