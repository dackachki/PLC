using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using PLCProject;

namespace PLCProject
{
    public class ClientSocket
    {
        byte[] m_dataBuffer = new byte[10];
        IAsyncResult m_result;
        public AsyncCallback m_pfnCallBack;
        public Socket m_clientSocket;
        public String TargetIP = "";
        public String strHostName;

        ArrayList ArrayBuffer = new ArrayList();
        Byte m_Specificvalue = 0;

        public Byte GetSpecificValue()
        {
            return m_Specificvalue++;
        }

        public ClientSocket()
        {

        }

        void AddReceiveData(CDataObject obj)
        {
            lock (this)
            {
                ArrayBuffer.Add(obj);
            }
        }

        public CDataObject GetReceiveData()
        {
            lock (this)
            {
                if (ArrayBuffer.Count != 0)
                {
                    CDataObject obj = (CDataObject)ArrayBuffer[0];
                    ArrayBuffer.RemoveAt(0);
                    return obj;
                }
            }
            return null;
        }

        public void CloseSocket()
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
            }
        }


        public bool ConnectSocket(String sip, int port)
        {
            // See if we have text on the IP and Port text fields
            if (sip == "" || port <= 0)
            {
                //Debug.WriteLine("IP Address and Port Number are required to connect to the Server\n");
            }
            try
            {
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(sip);
                int iPortNo = port;
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);
                // Connect to the remote host
                m_clientSocket.Connect(ipEnd);

                if (m_clientSocket.Connected)
                {
                    WaitForData();
                }
                return true;
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                //Debug.WriteLine(str);
            }
            return false;
        }

        public bool SendMessage(CDataObject sendobj)
        {
            try
            {
                if (m_clientSocket != null && m_clientSocket.Connected)
                {

                    m_clientSocket.Send(sendobj.m_Data, sendobj.Count, SocketFlags.None);
                    return true;
                }
            }
            catch (SocketException se)
            {
                //Debug.WriteLine(se.Message);
            }
            return false;
        }
        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = m_clientSocket;
                // Start listening to the data asynchronously
                m_result = m_clientSocket.BeginReceive(theSocPkt.dataBuffer,0, theSocPkt.dataBuffer.Length,SocketFlags.None, m_pfnCallBack,theSocPkt);
                
            }
            catch (SocketException se)
            {
                //Debug.WriteLine(se.Message);
            }
            
                }
        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[4096];
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);

                CDataObject obj = new CDataObject();
                obj.Copy(theSockId.dataBuffer, iRx);
                AddReceiveData(obj);
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                //System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
                //Debug.WriteLine("\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                //Debug.WriteLine(se.Message);
            }
        }
        public byte[] Write_byt_adr(int value, int start_adr)
        {                           //string형 파라미터를 인트로 변환시키고 4짜리 배열에 바이트형으로 변환시켜 넣는다
            byte[] Start_ADR = Get_Start_Adr(start_adr.ToString());
            bool Ark = true;
            int Data = 21;
            byte[] byt2 = new byte[2];

            byte[] byt = new byte[Data + 2];
            if (byt[00] != Device_Def.SubHeader1 || byt[01] != Device_Def.SubHeader2) Ark = false;   // check Sub header 
            else if (byt[02] != Device_Def.NetworkNumber) Ark = false;   // check Network Number 

            byt[00] = Device_Def.SubHeader1;
            byt[01] = Device_Def.SubHeader2;
            byt[02] = 0x00;
            byt[03] = 0xff;   // PC Number 
            byt[04] = Device_Def.Start_IO1;   // Start I/O - 1 (선두 I/O) - Gx Developer에 의해 설정된 번호
            byt[05] = Device_Def.Start_IO2;   // Start I/0 - 2
            byt[06] = Device_Def.Station_Number;   // Station Number (국번) - Gx Developer에 의해 설정된 번호 
            byt[07] = 0x0C;   // 데이터 부의 길이 - 1 (data length 이후부터 길이)
            byt[08] = 0x00;   // 데이터 부의 길이 - 2 (data length 이후부터 길이)
            byt[09] = Device_Def.Monitor_timer1;   // 모니터 타이머 1
            byt[10] = Device_Def.Monitor_timer2;   // 모니터 타이머 2

            byt[11] = 0x01;   // Command 1 : 0401 
            byt[12] = 0x14;   // Command 2 : 1401 = 쓰기
            byt[13] = 0x00;   // Sub Command 1 : 0000 (Word 단위 처리)
            byt[14] = 0x00;   // Sub Command 2 : 0000 (Word 단위 처리)


            // 아래 [15]번지 ~ [17]번지까지는 D 변수의 시작번지 -

            byt[15] = Start_ADR[0];
            byt[16] = Start_ADR[1];
            byt[17] = Start_ADR[2];


            byt[18] = Device_Def.SearchDeviceCode(Device_Def.Device);

            byt[19] = 0x01; // 어드레스 길이 1
            byt[20] = 0x00; // 어드레스 길이 2
            byt2 = BitConverter.GetBytes(value);
            byt[21] = byt2[0]; // 홀수 하이
            byt[22] = byt2[1]; // 홀수 하이
            return byt;
        }
        public byte[] Get_Start_Adr(String msg)
        {
            int i = 0;
            byte[] result = new byte[4];
            if (msg == "")
            {

            }
            else
            {
                i = int.Parse(msg);
            }
            result = BitConverter.GetBytes(i);  // 아주 중요 이거 꼭 필요
            return result;
        }
        public void Send_Byte(byte[] byt)
        {

            if (m_clientSocket != null)
            {
                if (m_clientSocket.Connected)
                {
                    m_clientSocket.Send(byt);
                }

            }
        }

    }

    
    }

