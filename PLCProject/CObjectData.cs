using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PLCProject
{
    public class CDataObject
    {
        public Byte[] m_Data = new Byte[4097];
        public Socket SendTo = null;
        public int Count = 0;
        public String ExtraInfo;
        public void MakeReadFrame(Byte nUnitType, Byte DeviceCode, int nStartAddr, int nReadLen)
        {
            m_Data[0] = 0x50;
            m_Data[1] = 0x00;	// Subheader	

            m_Data[2] = 0x00;	// Network No. (Self station)
            m_Data[3] = 0xFF;	// PC No.

            m_Data[4] = 0xFF;
            m_Data[5] = 0x03;	// Specific value
            m_Data[6] = 0x00;	// Specific value 2

            m_Data[7] = 0x0C;
            m_Data[8] = 0x00;	// Request data length

            m_Data[9] = 0x08;  // 0x04;
            m_Data[10] = 0x00;	// CPU monitoring timer (unit is 250ms)

            m_Data[11] = 0x01;
            m_Data[12] = 0x04;  //Command
            m_Data[13] = nUnitType;
            m_Data[14] = 0x00; 	// Subcommand

            m_Data[15] = (byte)(nStartAddr & 0xFF);
            m_Data[16] = (byte)((nStartAddr >> 8) & 0xFF);
            m_Data[17] = (byte)((nStartAddr >> 16) & 0xFF);		// Head device (Start Address)

            m_Data[18] = DeviceCode;					// Device code (Memory Type)

            m_Data[19] = (byte)(nReadLen & 0x00FF);				//WORD단위:읽을 워드수, BIT단위:읽을 비트수
            m_Data[20] = (byte)((nReadLen >> 8) & 0x00FF);		// Number of device points

            Count = 21;
        }
        public void MakeReadFrame(Byte SpecificValue, Byte nUnitType, Byte DeviceCode, int nStartAddr, int nReadLen)
        {
            m_Data[0] = 0x50;
            m_Data[1] = 0x00;	// Subheader	

            m_Data[2] = 0x00;	// Network No. (Self station)
            m_Data[3] = 0xFF;	// PC No.

            m_Data[4] = 0xFF;

            m_Data[5] = 0x03;	// Specific value
            m_Data[6] = SpecificValue;

            m_Data[7] = 0x0C;
            m_Data[8] = 0x00;	// Request data length

            m_Data[9] = 0x08;  // 0x04;
            m_Data[10] = 0x00;	// CPU monitoring timer (unit is 250ms)

            m_Data[11] = 0x01;
            m_Data[12] = 0x04;  //Command
            m_Data[13] = nUnitType;
            m_Data[14] = 0x00; 	// Subcommand

            m_Data[15] = (byte)(nStartAddr & 0xFF);
            m_Data[16] = (byte)((nStartAddr >> 8) & 0xFF);
            m_Data[17] = (byte)((nStartAddr >> 16) & 0xFF);		// Head device (Start Address)

            m_Data[18] = DeviceCode;					// Device code (Memory Type)

            m_Data[19] = (byte)(nReadLen & 0x00FF);				//WORD단위:읽을 워드수, BIT단위:읽을 비트수
            m_Data[20] = (byte)((nReadLen >> 8) & 0x00FF);		// Number of device points

            Count = 21;
        }
        int GetDataLength(int nUnitType, int wlen)
        {
            if (nUnitType == 0x00)
            {
                return wlen * 2;
            }
            else
            {
                return (wlen / 2) + (wlen % 2);
            }
        }
        public void MakeWriteFrame(Byte nUnitType, Byte DeviceCode, Byte[] Data, int nStartAddr, int wWriteLen)
        {
            int wReqLen = 12 + GetDataLength(nUnitType, wWriteLen);

            m_Data[0] = 0x50;
            m_Data[1] = 0x00;	// Subheader	

            m_Data[2] = 0x00;	// Network No. (Self station)
            m_Data[3] = 0xFF;	// PC No.

            m_Data[4] = 0xFF;
            m_Data[5] = 0x03;	// Specific value
            m_Data[6] = 0x00;	// Specific value 2

            m_Data[7] = (byte)(wReqLen & 0x00FF);
            m_Data[8] = (byte)((wReqLen >> 8) & 0x00FF);  	// Request data length

            m_Data[9] = 0x08;  // 0x04;
            m_Data[10] = 0x00;	// CPU monitoring timer (unit is 250ms)

            m_Data[11] = 0x01;
            m_Data[12] = 0x14;  //Command
            m_Data[13] = nUnitType;
            m_Data[14] = 0x00; 	// Subcommand

            m_Data[15] = (byte)((nStartAddr & 0xFF)); //address 주소
            m_Data[16] = (byte)((nStartAddr >> 8) & 0xFF);
            m_Data[17] = (byte)((nStartAddr >> 16) & 0xFF);		// Head device (Start Address)

            m_Data[18] = DeviceCode;					// Device code (Memory Type)

            m_Data[19] = (byte)(wWriteLen & 0x00FF);            //WORD단위:쓸 워드수, BIT단위:쓸 비트수
            m_Data[20] = (byte)((wWriteLen >> 8) & 0x00FF);		// Number of device points
            Array.Copy(Data, 0, m_Data, 21, wWriteLen * 2);
            Count = 21 + GetDataLength(nUnitType, wWriteLen);
        }
        public void MakeWriteFrame(Byte SpecificValue, Byte nUnitType, Byte DeviceCode, Byte[] Data, int nStartAddr, int wWriteLen)
        {
            int wReqLen = 12 + GetDataLength(nUnitType, wWriteLen);

            m_Data[0] = 0x50;
            m_Data[1] = 0x00;	// Subheader	

            m_Data[2] = 0x00;	// Network No. (Self station)
            m_Data[3] = 0xFF;	// PC No.

            m_Data[4] = 0xFF;

            m_Data[5] = 0x03;	// Specific value
            m_Data[6] = SpecificValue;	// Specific value 2

            m_Data[7] = (byte)(wReqLen & 0x00FF);
            m_Data[8] = (byte)((wReqLen >> 8) & 0x00FF);  	// Request data length

            m_Data[9] = 0x08;  // 0x04;
            m_Data[10] = 0x00;	// CPU monitoring timer (unit is 250ms)

            m_Data[11] = 0x01;
            m_Data[12] = 0x14;  //Command
            m_Data[13] = nUnitType;
            m_Data[14] = 0x00; 	// Subcommand

            m_Data[15] = (byte)((nStartAddr & 0xFF));
            m_Data[16] = (byte)((nStartAddr >> 8) & 0xFF);
            m_Data[17] = (byte)((nStartAddr >> 16) & 0xFF);		// Head device (Start Address)

            m_Data[18] = DeviceCode;					// Device code (Memory Type)

            m_Data[19] = (byte)(wWriteLen & 0x00FF);            //WORD단위:쓸 워드수, BIT단위:쓸 비트수
            m_Data[20] = (byte)((wWriteLen >> 8) & 0x00FF);		// Number of device points
            Array.Copy(Data, 0, m_Data, 21, wWriteLen * 2);
            Count = 21 + GetDataLength(nUnitType, wWriteLen);
        }
        public CDataObject()
        {

        }
        public bool ArrayEquals(Byte[] comp, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (m_Data[i] != comp[i]) return false;
            }
            return true;
        }
        public void ClearByteData()
        {
            for (int i = 0; i < m_Data.Count(); i++)
            {
                m_Data[i] = 0x00;
            }
        }
        public CDataObject(String str, String str2)
        {
            System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
            Byte[] BytesMessage = ASCII.GetBytes(str);
            if (str.Length < 4097)
            {
                Count = str.Length;
                Array.Copy(BytesMessage, m_Data, Count);
            }
            else
            {
                Count = 4096;
                Array.Copy(BytesMessage, m_Data, Count);
            }
            ExtraInfo = str2;
        }
        public CDataObject(int scount)
        {
            Count = scount;
        }
        public CDataObject(bool b)
        {
            for (int i = 0; i < 4096; i++)
            {
                m_Data[i] = 0xff;
            }
        }
        public CDataObject(String str)
        {
            System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
            Byte[] BytesMessage = ASCII.GetBytes(str);
            if (str.Length < 4097)
            {
                Count = str.Length;
                Array.Copy(BytesMessage, m_Data, Count);
            }
            else
            {
                Count = 4096;
                Array.Copy(BytesMessage, m_Data, Count);
            }
        }
        public CDataObject(String str, bool binary)
        {
            if (binary)
            {
                if (str.Length < 8192)
                {
                    for (int i = 0; i < str.Length / 2; i++)
                    {
                        String hex = str.Substring(i * 2, 2);
                        m_Data[i] = Convert.ToByte(hex, 16);
                    }
                    Count = str.Length / 2;
                }
                else
                {
                    for (int i = 0; i < 8192 / 2; i++)
                    {
                        String hex = str.Substring(i * 2, 2);
                        m_Data[i] = Convert.ToByte(hex, 16);
                    }
                    Count = str.Length / 2;
                }
            }
            else
            {
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
                Byte[] BytesMessage = ASCII.GetBytes(str);
                if (str.Length < 4097)
                {
                    Count = str.Length;
                    Array.Copy(BytesMessage, m_Data, Count);
                }
                else
                {
                    Count = 4096;
                    Array.Copy(BytesMessage, m_Data, Count);
                }
            }
        }
        public void Copy(Byte[] source, int count)
        {
            Array.Copy(source, m_Data, count);
            Count = count;
            SendTo = null;
        }
        public void Copy(Byte[] source, int index, int count)
        {
            Array.Copy(source, index, m_Data, 0, count);
            Count = count;
            SendTo = null;
        }
        public void Copy(CDataObject ob)
        {
            Array.Copy(ob.m_Data, m_Data, 4097);
            Count = ob.Count;
            ExtraInfo = ob.ExtraInfo;
            SendTo = ob.SendTo;
        }
        public void Copy(Byte[] source, int count, Socket soc)
        {
            Array.Copy(source, m_Data, count);
            Count = count;
            SendTo = soc;
        }

        public String GetRowString()
        {
            String str = "";
            for (int i = 0; i < Count; i++)
            {
                str = str + String.Format("{0:X2} ", m_Data[i]);
            }
            return str;
        }

        public String GetString()
        {
            System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();
            String StringMessage = ASCII.GetString(m_Data);
            return StringMessage;
        }
        public void HiRowChange()
        {
            for (int i = 0; i < Count; i = i + 2)
            {
                Byte b = m_Data[i];
                m_Data[i] = m_Data[i + 1];
                m_Data[i + 1] = b;
            }
        }
        public CDataObject GetErrorFrame(UInt16 errorcode)
        {

            CDataObject ret = new CDataObject(11);
            ret.m_Data[0] = 0x50;
            ret.m_Data[1] = 0x00;   // Subheader	

            ret.m_Data[2] = 0xFF;   // Network No. (Self station)
            ret.m_Data[3] = 0x00;   // PC No.

            ret.m_Data[4] = 0xFF;
            ret.m_Data[5] = 0x03;   // Specific value
            ret.m_Data[6] = 0x00;   // Specific value 2

            ret.m_Data[7] = 0x02;
            ret.m_Data[8] = 0x00;   // Request data length

            ret.m_Data[9] = (byte)(errorcode & 0x00FF);  // 0x04;
            ret.m_Data[10] = (byte)((errorcode >> 8) & 0x00FF); // CPU monitoring timer (unit is 250ms)
            return ret;
        }


        //view sourceprint?1.Byte[] BytesMessage; // Your message 
        //2.System.Text.ASCIIEncoding ASCII  = new System.Text.ASCIIEncoding(); 
        //3.String StringMessage = ASCII.GetString( BytesMessage );
        //Solution #2 – Convert the Unicode string to a 

    }
}

