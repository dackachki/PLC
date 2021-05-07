using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCProject
{
    public class ExtraMethod
    {
        public int GetByteToInt(Byte[] Source, int index, int count)
        {
            int value = 0x00;
            for (int i = 0; i < count; i++)
            {
                value = value | (Source[index + i] << (8 * i));
            }
            return value;
        }
        public int GetByteToIntSwap(Byte[] Source, int index, int count)
        {
            int value = 0x00;
            byte[] TempByte = new byte[count];
            Array.Copy(Source, index, TempByte, 0, count);
            Array.Reverse(TempByte);
            for (int i = 0; i < count; i++)
            {
                value = value | (TempByte[i] << (8 * i));
            }
            return value;
        }
        
       


    }



}
