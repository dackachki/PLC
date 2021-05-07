using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;


namespace SFA_UTIL
{
    public class CDataSet
    {
        public int ArraySize = 100;
        public CDataSet(int sze)
        {
            ArraySize = sze;
        }
        public bool ArrayEquals(Byte[] comp)
        {
            for (int i = 0; i < ArraySize; i++)
            {
                if (Result[i] != comp[i])
                {
                    Array.Copy(comp, Result, ArraySize);
                    return false;
                }
            }
            return true;
        }

        public bool ArrayEquals(Byte[] comp, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (Result[i] != comp[i]) return false;
            }
            return true;
        }

        public bool ArrayEquals(Byte[] comp, int startindex, int count)
        {
            if (count > ArraySize)
            {
                count = ArraySize;
            }
            for (int i = startindex; i < count; i++)
            {
                if (Result[i] != comp[i])
                {
                    Array.Copy(comp, Result, ArraySize);
                    return false;
                }
            }
            return true;
        }

        public void ArrayCopy(Byte[] comp, int index, int length)
        {
            Array.Copy(comp, index, Result, index, length);
        }

        Byte[] Source = new Byte[1000];
        Byte[] ForceOn = new Byte[1000];
        Byte[] ForceOff = new Byte[1000];
        Byte[] Result = new Byte[1000];



        public void SetSourceByte(int index, Byte by)
        {
            Source[index] = by;
        }
        public void SetSourceByteArray(int index, Byte[] by)
        {
            lock (this)
            {
                Array.Copy(by, 0, Source, index, by.Length);
            }
        }
        public Byte[] GetResult()
        {
            lock (this)
            {
                Byte[] ret = new Byte[ArraySize];
                Array.Copy(Result, ret, ArraySize);
                return ret;
            }
        }
        public Byte GetResultForByte(int byteIndex)
        {
            lock (this)
            {
                return Result[byteIndex];
            }
        }
        public byte[] GetResultForByteArray(int index, int size)
        {
            lock (this)
            {
                Byte[] ret = new Byte[size];
                Array.Copy(Result, index, ret, 0, size);
                return ret;
            }
        }
        public Byte[] GetSource()
        {
            lock (this)
            {
                Byte[] ret = new Byte[ArraySize];
                Array.Copy(Source, ret, ArraySize);
                return ret;
            }
        }
        public Byte[] GetForceOn()
        {
            lock (this)
            {
                Byte[] ret = new Byte[ArraySize];
                Array.Copy(ForceOn, ret, ArraySize);
                return ret;
            }
        }
        public Byte[] GetForceOff()
        {
            lock (this)
            {
                Byte[] ret = new Byte[ArraySize];
                Array.Copy(ForceOff, ret, ArraySize);
                return ret;
            }
        }
        public void SetToSource(Byte[] data, int count)
        {
            lock (this)
            {
                if (count > 1000) count = 1000;
                Array.Copy(data, Source, count);
            }
        }

        public void GetFromArray(Byte[] data)
        {
            lock (this)
            {
                Array.Copy(Result, data, data.Length);
            }
        }
        public void SetWordToSource(int index, String str)
        {
            lock (this)
            {
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                Byte[] by = ASCII.GetBytes(str);
                for (int i = 0; i < (Source.Length - index) && i < str.Length; i++)
                {
                    Source[i + index] = by[i];
                }
            }
        }
        public void SetWordToSource(int index, String str, int count)
        {
            lock (this)
            {
                ResetSource(index, count);
                if (str.Length > count)
                {
                    str = str.Substring(0, count);
                }
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                Byte[] by = ASCII.GetBytes(str);
                for (int i = 0; i < (Source.Length - index) && i < str.Length; i++)
                {
                    Source[i + index] = by[i];
                }
            }
        }
        public String GetWordFromResult(int index)
        {
            lock (this)
            {
                Byte[] by = new Byte[ArraySize];
                for (int i = 0; i < ArraySize; i++)
                {
                    by[i] = Result[i + index];
                }
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                String StringMessage = ASCII.GetString(by);
                return StringMessage;
            }
        }
        public String GetWordFromResult(int index, int count)
        {
            lock (this)
            {
                Byte[] by = new Byte[count];
                for (int i = 0; i < count; i++)
                {
                    if (('0' <= Result[i + index] && '9' >= Result[i + index]) ||
                        ('a' <= Result[i + index] && 'z' >= Result[i + index]) ||
                        ('A' <= Result[i + index] && 'Z' >= Result[i + index]) ||
                         ('-' == Result[i + index]) || ('_' == Result[i + index]))
                    {
                        by[i] = Result[i + index];
                    }
                    else
                    {
                        by[i] = (Byte)' ';
                    }
                }
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                String StringMessage = ASCII.GetString(by);
                StringMessage = StringMessage.Trim();
                StringMessage = StringMessage.Trim('\0');
                return StringMessage;
            }
        }
        public String GetWordFromResultNoTrim(int index, int count)
        {
            lock (this)
            {
                Byte[] by = new Byte[count];
                for (int i = 0; i < count; i++)
                {
                    if (('0' <= Result[i + index] && '9' >= Result[i + index]) ||
                        ('a' <= Result[i + index] && 'z' >= Result[i + index]) ||
                        ('A' <= Result[i + index] && 'Z' >= Result[i + index]) ||
                         ('-' == Result[i + index]) || ('_' == Result[i + index]))
                    {
                        by[i] = Result[i + index];
                    }
                    else
                    {
                        by[i] = (Byte)' ';
                    }
                }
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                String StringMessage = ASCII.GetString(by);
                return StringMessage;
            }
        }
        public void Refresh()
        {
            lock (this)
            {
                for (int i = 0; i < ArraySize; i++)
                {
                    Result[i] = (Byte)((Source[i] | ForceOn[i]) & (~ForceOff[i]));
                }
            }
        }

        public void SetForceOn(int index, bool flag)
        {
            lock (this)
            {
                if (flag)
                {
                    ForceOn[index / 8] = (Byte)(ForceOn[index / 8] | (1 << (index % 8)));
                }
                else
                {
                    ForceOn[index / 8] = (Byte)(ForceOn[index / 8] & (~(1 << (index % 8))));
                }

            }
        }

        public void SetForceOff(int index, bool flag)
        {
            lock (this)
            {
                if (flag)
                {
                    ForceOff[index / 8] = (Byte)(ForceOff[index / 8] | (1 << (index % 8)));
                }
                else
                {
                    ForceOff[index / 8] = (Byte)(ForceOff[index / 8] & (~(1 << (index % 8))));
                }

            }
        }

        public void SetSource(int index, bool flag)
        {
            lock (this)
            {
                if (flag)
                {       // or연산 0 -> 1
                    Source[index / 8] = (Byte)(Source[index / 8] | (1 << (index % 8)));
                }
                else
                {   //and연산 1 -> 0
                    Source[index / 8] = (Byte)(Source[index / 8] & (~(1 << (index % 8))));
                }
            }
        }

        public bool GetForceOn(int index)
        {
            if ((ForceOn[index / 8] & (1 << (index % 8))) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool GetForceOff(int index)
        {
            if ((ForceOff[index / 8] & (1 << (index % 8))) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool GetResult(int index)
        {
            if ((Result[index / 8] & (1 << (index % 8))) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool GetSource(int index)
        {
            if ((Source[index / 8] & (1 << (index % 8))) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void ResetAll()
        {
            Byte[] reset = new Byte[1000];
            for (int i = 0; i < 1000; i++)
            {
                Array.Copy(reset, Source, 1000);
                Array.Copy(reset, ForceOn, 1000);
                Array.Copy(reset, ForceOff, 1000);
                Array.Copy(reset, Result, 1000);
            }
        }

        public void ResetSource(int index, int count)
        {
            lock (this)
            {
                Array.Clear(Source, index, count);
            }
        }

        public void ResetSourceBit(int index, int count)
        {
            lock (this)
            {
                for (int i = index; i < index + count; i++)
                {
                    Source[i / 8] = (Byte)(Source[i / 8] & (~(1 << (i % 8))));
                }
            }
        }

        public void ResetSource()
        {
            Array.Clear(Source, 0, 1000);
        }
        public void ResetForceOn()
        {
            Array.Clear(ForceOn, 0, 1000);
        }
        public void ResetForceOff()
        {
            Array.Clear(ForceOff, 0, 1000);
        }
        public Byte GetResultByte(int index)
        {
            return Result[index];
        }
        public int GetResultWord(int index)
        {
            return (Result[index + 1] << 8) | Result[index];
        }
        public int GetSourceWord(int index)
        {
            return (Result[index + 1] << 8) | Result[index];
        }
        public void SetSourceWord(int index, int value)
        {
            value = value & 0xFFFF;
            Source[index] = (Byte)(value & 0xff);
            Source[index + 1] = (Byte)(value >> 8);
        }
        public String GetResultStringForMonitoring()
        {
            String str = "";
            for (int i = 0; i < ArraySize; i++)
            {
                String str2 = String.Format("{0:X2}", Result[i]);
                str = str + str2;
            }
            return str;
        }
    }

  
}
