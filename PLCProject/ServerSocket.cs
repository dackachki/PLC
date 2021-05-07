using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using PLCProject;
using System.Windows.Forms;
using System.Drawing;

namespace SFA_UTIL
{
    public class CSocketServer      // 서버 기능 소켓
    {
        const int MAX_CLIENTS = 1;      // 최대 클라이언트
        public AsyncCallback pfnWorkerCallBack;
        private Socket m_mainSocket = null;                         // Listen 소켓
        public Socket[] m_workerSocket = new Socket[MAX_CLIENTS];  // 통신하는 소켓
        ArrayList ArrayBuffer = new ArrayList();                    // 수신 데이터 넣어 놓을 배열
        Byte m_Specificvalue = 0;
        public byte PCNodeNumber;
        public byte PlcNodeNumber;
       

        public Byte GetSpecificValue()
        {
            return m_Specificvalue++;   // 0 = > 1
        }

        public CSocketServer()
        {

        }

        public CSocketServer(string sip, int PortNo)
        {
            //StartListen(PortNo);    // 대기 시작(포트 번호)
        }

        void AddReceiveData(CDataObject obj)    // 수신 데이터 추가(오브젝트)
        {
            lock (this)
            {
                ArrayBuffer.Add(obj);   // 배열에 오브젝트를 추가
            }
        }

        public CDataObject GetReceiveData()
        {
            lock (this)
            {
                if (ArrayBuffer.Count != 0)     // ArrayBuffer가 0이 아니면
                {
                    CDataObject obj = (CDataObject)ArrayBuffer[0];  // ArrayBuffer 첫번째 배열 요소를 obj에 태운다
                    ArrayBuffer.RemoveAt(0);    // ArrayBuffer 첫번째 배열 요소를 지움
                    return obj;                 // 오브젝트 반환
                }
            }
            return null;    // ArrayBuffer에 아무것도 없으면 Null 반환
        }

        public String GetCurrectIPAddres()
        {
            return GetIP();
        }

        public bool StartListen(string ip, int PortNo)     // 대기 시작
        {
            try
            {
                //string IPNumber = PLC_Simulator.PLC_Form.IPNum;

                int port = PortNo;
                // Create the listening socket...
                if(m_mainSocket != null)
                {
                    MessageBox.Show("이미 서버가 실행중입니다.");
                    return false;
                }
                m_mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);     // 메인 소켓 = (사용할 주소 체계, 데이터 전송 방식, 프로토콜 타입)                
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Parse(ip), port);       // 모든 클라이언트 요청 받음, Port                                
                // Bind to local IP Address...
                m_mainSocket.Bind(ipLocal);     // 시뮬레이터 IP
                // Start listening...
                m_mainSocket.Listen(4);         // Server 소켓이 연결을 대기할 수 있는 최대 대기자 목록 == 4
                // Create the call back for any client connections...

                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null); // 클라이언트 연결 요청 시 OnClientConnect 실행

                return true;        // 통신 준비 완료
            }
            catch (SocketException se)
            {
                
            }
            return false;       // 메시지 띄움 "통신 Port를 확인하세요"
        }

        public void OnClientConnect(IAsyncResult asyn)  // Client와 연결 되면
        {
            try
            {
                Socket m_tempWorkerSocket = m_mainSocket.EndAccept(asyn);       // m_tempWorkerSocket == asyn == 클라이언트와 통신하는 소켓
                bool asign = false;
                for (int i = 0; i < MAX_CLIENTS; i++)       // i = 0, 1
                {
                    if (m_workerSocket[i] == null)          // 배열에 값이 Null 이면
                    {
                        m_workerSocket[i] = m_tempWorkerSocket; // m_workerSocket[i] = 통신 소켓;
                        WaitForData(m_workerSocket[i]);         // 
                        asign = true;
                        break;
                    }
                    else
                    {
                        if (m_workerSocket[i].Connected == false)
                        {
                            m_workerSocket[i] = m_tempWorkerSocket;
                            WaitForData(m_workerSocket[i]);
                            asign = true;
                            break;
                        }
                    }
                }
                if (asign == false)
                {
                    if (m_tempWorkerSocket != null)     // 소켓 통신 연결 해제
                    {
                        m_tempWorkerSocket.Shutdown(SocketShutdown.Both);
                        m_tempWorkerSocket.Disconnect(false);
                        m_tempWorkerSocket.Close();
                        m_tempWorkerSocket = null;
                    }
                }
                
                string ms = m_tempWorkerSocket.LocalEndPoint.ToString();
                string[] infos = ms.Split(':');
                int connectedPort = int.Parse(infos[1]);
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("\n OnClientConnection: Socket has been closed");
                //System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                
                
            }
            catch(Exception e)
            {
                
            }
        }
        
       

        public class SocketPacket
        {
            public System.Net.Sockets.Socket m_currentSocket;
            public byte[] dataBuffer = new byte[4096];
        }
        // Start waiting for data from the client       // 클라이언트로부터 데이터를 기다리기 시작
        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = soc;
                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length,SocketFlags.None, pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                
            }
        }
        // This the call back function which will be invoked when the socket
        // detects any client writing of data on the stream
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                int iRx = 0;
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                iRx = socketData.m_currentSocket.EndReceive(asyn);
                CDataObject obj = new CDataObject();
                obj.Copy(socketData.dataBuffer, iRx, socketData.m_currentSocket);
                AddReceiveData(obj);
                WaitForData(socketData.m_currentSocket);
            }
            catch (ObjectDisposedException)
            {
                //System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
                Debug.WriteLine("\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
             
            }
        }
        void BroadCastingMsg(String text)
        {
            try
            {
                Object objData = text;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                for (int i = 0; i < MAX_CLIENTS; i++)
                {
                    if (m_workerSocket[i] != null)
                    {
                        if (m_workerSocket[i].Connected)
                        {
                            m_workerSocket[i].Send(byData);
                        }
                    }
                }

            }
            catch (SocketException se)
            {
                
            }
        }
        public bool BroadCastingMsg(CDataObject sendobj)
        {
            try
            {
                for (int i = 0; i < MAX_CLIENTS; i++)
                {
                    if (m_workerSocket[i] != null)
                    {
                        if (m_workerSocket[i].Connected)
                        {
                            m_workerSocket[i].Send(sendobj.m_Data, sendobj.Count, SocketFlags.None);
                        }
                    }
                }
                return true;
            }
            catch (SocketException se)
            {
                

            }
            return false;
        }
        public bool BroadCastingMsg(byte[] sendobj)
        {
            try
            {
                for (int i = 0; i < MAX_CLIENTS; i++)
                {
                    if (m_workerSocket[i] != null)
                    {
                        if (m_workerSocket[i].Connected)
                        {
                            m_workerSocket[i].Send(sendobj, sendobj.Length, SocketFlags.None);
                        }
                    }
                }
                return true;
            }
            catch (SocketException se)
            {
                

            }
            return false;
        }
        public void StopListen()
        {
            CloseSockets();
        }
        public int ConnectCount()
        {
            int j = 0;
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_workerSocket[i] != null)
                {
                    if (m_workerSocket[i].Connected)
                    {
                        j++;
                    }
                }
            }
            return j;
        }
        String GetIP()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            String IPStr = "";
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                IPStr = ipaddress.ToString();
                return IPStr;
            }
            return IPStr;
        }

        public void CloseSockets()
        {
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_workerSocket[i] != null)
                {
                    if (m_workerSocket[i].Connected)
                    {
                        m_workerSocket[i].Close();
                        m_workerSocket[i] = null;
                    }
                }
            }
            if (m_mainSocket != null)
            {
                m_mainSocket.Close();
            }
        }
    }
}
