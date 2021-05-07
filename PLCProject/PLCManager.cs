using SFA_UTIL;
using System;
using System.Threading;
using System.Windows.Forms;


namespace PLCProject
{
    public class PLCManager
    {

        //포트 갯수,상태
        public int[,] SocketConnect = new int[16, 2];
        string PLCName = Device_Def.Device;
        public bool SocketLive = true;
        ExtraMethod ex;
        public static ushort[] D_Reg = new ushort[100000];      // 콤보박스 배열
        public static ushort[] R_Reg = new ushort[100000];
        public static ushort[] W_Reg = new ushort[100000];
        public static ushort[] ZR_Reg = new ushort[100000];
        private static PLCManager PM;
        public static Object SyncObject = new Object();
        public static string[] Portarray = new string[10];

        public PLCManager()
        {
            ex = new ExtraMethod();
        }
        public static PLCManager Get_PM()
        {
            if (PM == null)
            {
                PM = new PLCManager();
            }
            return PM;
        }

        public void SocketInit(object arg)        // 소켓 초기화
        {

            int portNum = Device_Def.Port;

            for(int i = 0; i < 16; i++) { 
                string fulltext;
                fulltext = arg.ToString() + "/" + (portNum + i).ToString() + "/" + i.ToString();
                SocketConnect[0, 0] = portNum ;
                Thread th = new Thread(new ParameterizedThreadStart(ServerAction));
                th.Start(fulltext);
            }

        }
        public void ServerAction(object arg)
        {
            string[] fullText = Convert.ToString(arg).Split('/');
            string ip = fullText[0];
            int portno = Convert.ToInt32(fullText[1]);
            int id = int.Parse(fullText[2]);
            int index = -1;
             CSocketServer server = new CSocketServer();

            bool x = server.StartListen(ip, portno);    // 서버.대기시작(포트번호)            
            if (x == false)
            {
                MessageBox.Show(portno.ToString() + " 통신 Port를 확인하세요");
                Environment.Exit(1);
            }
            

            while (SocketLive && PLCName != null)
            {
                Thread.Sleep(1);
                CDataObject data = server.GetReceiveData();

                                                        //0번 배열 길이
                for (int i = 0; i < SocketConnect.GetLength(0); i++)
                {
                    if (SocketConnect[i, 0] == portno)
                    {
                        index = i;
                        if (server.m_workerSocket[0] != null && SocketConnect[index, 0] != 0)
                        {
                            if (index != -1 && server.m_workerSocket[0].Connected)
                            {
                                SocketConnect[index, 1] = 1;
                            }
                            else
                            {
                                SocketConnect[index, 1] = 0;
                            }

                        }
                        break;
                    }
                }

                if (data != null)               // 서버에서, 수신 데이터가 Null이 아니면
                {
                    PLCMethod(server, data);
                }
               
            }
            server.CloseSockets();
        }
        private void PLCMethod(CSocketServer server, CDataObject data)
        {
            if (data.Count >= 21)
            {
                if (data.m_Data[0] == 0x50
                    && data.m_Data[1] == 0x00
                    && data.m_Data[2] == 0x00
                    && data.m_Data[3] == 0xFF
                    && data.m_Data[4] == 0xFF
                    && data.m_Data[5] == 0x03
                    && (data.m_Data[9] == 0x04 || data.m_Data[9] == 0x08 || data.m_Data[9] == 0x10 || data.m_Data[9] == 0x0A)
                    && data.m_Data[11] == 0x01)
                {
                    if (data.m_Data[12] == 0x14) // write frame
                    {
                        int size = ex.GetByteToInt(data.m_Data, 19, 2);
                        int startaddress = ex.GetByteToInt(data.m_Data, 15, 3);
                        if (data.Count == 21 + size * 2)
                        {
                            if (size < 2000)
                            {
                                if (data.m_Data[18] == Device_Def.DEVICE_CODE_D)
                                {
                                    Byte[] writedata = new Byte[size * 2];
                                    Array.Copy(data.m_Data, 21, writedata, 0, writedata.Length);
                                    WriteForByte(Device_Def.DEVICE_CODE_D, startaddress, writedata);
                                    server.BroadCastingMsg(GetWrtieOKFrame(data));
                                    
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_R)
                                {
                                    Byte[] writedata = new Byte[size * 2];
                                    Array.Copy(data.m_Data, 21, writedata, 0, writedata.Length);
                                    WriteForByte(Device_Def.DEVICE_CODE_R, startaddress, writedata);
                                    server.BroadCastingMsg(GetWrtieOKFrame(data));
                                    
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_W)
                                {
                                    Byte[] writedata = new Byte[size * 2];
                                    Array.Copy(data.m_Data, 21, writedata, 0, writedata.Length);
                                    WriteForByte(Device_Def.DEVICE_CODE_W, startaddress, writedata);
                                    server.BroadCastingMsg(GetWrtieOKFrame(data));
                                    
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_ZR)
                                {
                                    Byte[] writedata = new Byte[size * 2];
                                    Array.Copy(data.m_Data, 21, writedata, 0, writedata.Length);
                                    WriteForByte(Device_Def.DEVICE_CODE_ZR, startaddress, writedata);
                                    server.BroadCastingMsg(GetWrtieOKFrame(data));
                                    
                                }
                                else
                                {
                                    server.BroadCastingMsg(GetErrorFrame(1001));
                                   
                                }
                            }
                            else
                            {
                                server.BroadCastingMsg(GetErrorFrame(1011));
                                
                            }
                        }
                        else
                        {
                            server.BroadCastingMsg(GetErrorFrame(1002));
                            
                        }

                    }
                    else if (data.m_Data[12] == 0x04) // read frame
                    {
                        if (data.Count == 21)
                        {
                            int size = ex.GetByteToInt(data.m_Data, 19, 2);
                            int startaddress = ex.GetByteToInt(data.m_Data, 15, 3);
                            if (size <= 2000)
                            {
                                if (data.m_Data[18] == Device_Def.DEVICE_CODE_D)
                                {
                                    Byte[] readdata = ReadForByte(Device_Def.DEVICE_CODE_D, startaddress, size);
                                    server.BroadCastingMsg(GetReadOKFrame(data, readdata));
                                   
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_R)
                                {
                                    Byte[] readdata = ReadForByte(Device_Def.DEVICE_CODE_R, startaddress, size);
                                    server.BroadCastingMsg(GetReadOKFrame(data, readdata));
                                   
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_W)
                                {
                                    Byte[] readdata = ReadForByte(Device_Def.DEVICE_CODE_W, startaddress, size);
                                    server.BroadCastingMsg(GetReadOKFrame(data, readdata));
                                   
                                }
                                else if (data.m_Data[18] == Device_Def.DEVICE_CODE_ZR)
                                {
                                    Byte[] readdata = ReadForByte(Device_Def.DEVICE_CODE_ZR, startaddress, size);
                                    server.BroadCastingMsg(GetReadOKFrame(data, readdata));
                                   
                                }
                                else
                                {
                                    server.BroadCastingMsg(GetErrorFrame(1003));
                                   
                                }
                            }
                            else
                            {
                                server.BroadCastingMsg(GetErrorFrame(1013));
                                
                            }
                        }
                        else
                        {
                            server.BroadCastingMsg(GetErrorFrame(1004));
                            
                        }
                    }
                    else
                    {
                        server.BroadCastingMsg(GetErrorFrame(1005));
                        
                        Thread.Sleep(200);
                    }
                }
                else
                {
                    server.BroadCastingMsg(GetErrorFrame(1006));
                   
                }
            }
            else
            {
                server.BroadCastingMsg(GetErrorFrame(1007));
                
                Thread.Sleep(15);
            }
        }

        //그리드에서 값이 변경되면 해당device startaddress값을 count만큼 복사해서 ret16에 붙혀넣는다
        public static Byte[] ReadForByte(int reg, int start_address, int count)     // 메모리 데이터 값 바이트로 읽기
        {
            lock (SyncObject)
            {
                Byte[] ret = new Byte[count * 2];
                UInt16[] ret16 = new UInt16[count];
                try
                {
                    if (reg == Device_Def.DEVICE_CODE_D)
                    {
                        Array.Copy(D_Reg, start_address, ret16, 0, count);
                    }
                    else if (reg == Device_Def.DEVICE_CODE_R) Array.Copy(R_Reg, start_address, ret16, 0, count);
                    else if (reg == Device_Def.DEVICE_CODE_W) Array.Copy(W_Reg, start_address, ret16, 0, count);
                    else if (reg == Device_Def.DEVICE_CODE_ZR) Array.Copy(ZR_Reg, start_address, ret16, 0, count);
        


                    //else if (reg == DEF_MELSEC.DEVICE_CODE_W) Array.Copy(W_Reg, start_address, ret16, 0, count);

                    for (int i = 0; i < count; i++)
                    {   //257 짝 258 홀
                        ret[i * 2] = (Byte)(ret16[i] & 0xff);
                        ret[i * 2 + 1] = (Byte)(ret16[i] >> 8);
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return ret;
            }

        }
        private CDataObject GetErrorFrame(UInt16 errorcode)
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
        
        private CDataObject GetWrtieOKFrame(CDataObject data)
        {
            CDataObject ret = new CDataObject(11);
            if (data.Count > 11)
            {
                ret.m_Data[0] = 0x50;
                ret.m_Data[1] = 0x00;   // Subheader	

                ret.m_Data[2] = 0xFF;   // Network No. (Self station)
                ret.m_Data[3] = 0x00;   // PC No.

                ret.m_Data[4] = 0xFF;
                ret.m_Data[5] = 0x03;   // Specific value
                ret.m_Data[6] = data.m_Data[6]; // Specific value 2

                ret.m_Data[7] = 0x02;
                ret.m_Data[8] = 0x00;   // Request data length

                ret.m_Data[9] = 0x00;  // 0x04;
                ret.m_Data[10] = 0x00;  // CPU monitoring timer (unit is 250ms)
            }
            return ret;
        }
        private CDataObject GetReadOKFrame(CDataObject data, byte[] readdata)
        {
            CDataObject ret = new CDataObject(11 + readdata.Length);
            if (data.Count == 21)
            {
                ret.m_Data[0] = 0x50;
                ret.m_Data[1] = 0x00;   // Subheader	

                ret.m_Data[2] = 0xFF;   // Network No. (Self station)
                ret.m_Data[3] = 0x00;   // PC No.

                ret.m_Data[4] = 0xFF;
                ret.m_Data[5] = 0x03;   // Specific value
                ret.m_Data[6] = data.m_Data[6]; // Specific value 2

                int wReqLen = readdata.Length + 2;
                ret.m_Data[7] = (byte)(wReqLen & 0x00FF);
                ret.m_Data[8] = (byte)((wReqLen >> 8) & 0x00FF);    // Request data length

                ret.m_Data[9] = 0x00;  // 0x04;
                ret.m_Data[10] = 0x00;  // CPU monitoring timer (unit is 250ms)
                Array.Copy(readdata, 0, ret.m_Data, 11, readdata.Length);
            }
            return ret;
        }

        internal static ushort[] SendReadData(int iSelectedRegister, int dataAdd, int v)
        {
            throw new NotImplementedException();
        }

        //
        public static bool WriteForByte(int reg, int start_address, byte[] data)        // 메모리 데이터 값 바이트로 쓰기
        {
            lock (SyncObject)
            {
                string regstr = string.Empty;
                UInt16[] ret16 = new UInt16[data.Length / 2];


                for (int i = 0; i < data.Length / 2; i++)
                {
                    ret16[i] = (UInt16)((((UInt16)data[i * 2]) << 0) | (((UInt16)data[i * 2 + 1]) << 8));
                }
                bool ret = false;

                try
                {
                    if (reg == Device_Def.DEVICE_CODE_D) { Array.Copy(ret16, 0, D_Reg, start_address, ret16.Length); regstr = "D"; }
                    else if (reg == Device_Def.DEVICE_CODE_R) { Array.Copy(ret16, 0, R_Reg, start_address, ret16.Length); regstr = "R"; }
                    else if (reg == Device_Def.DEVICE_CODE_W) { Array.Copy(ret16, 0, W_Reg, start_address, ret16.Length); regstr = "W"; }
                    else if (reg == Device_Def.DEVICE_CODE_ZR) { Array.Copy(ret16, 0, ZR_Reg, start_address, ret16.Length); regstr = "ZR"; }
             
                    //else if (reg == DEF_MELSEC.DEVICE_CODE_W)
                    //{
                    //    Array.Copy(ret16, 0, W_Reg, start_address, ret16.Length);
                    //}
                    ret = true;


                    //Array.Reverse(intBytes);
                    //byte[] result = intBytes;

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());  // 에러 메시지 출력
                }
                return ret;
            }
        }
        public static UInt16[] Read(int reg, int start_address, int count)      // 메모리 데이터 값 읽기 (레지 == MELSEC.디바이스 코드 D, 시작 주소, 카운트)
        {
            lock (SyncObject)   // 특정 블럭의 코드를 한번에 하나의 쓰레드만 실행할 수 있도록 해준다.
            {
                UInt16[] ret = new UInt16[count];
                try
                {
                    if (reg == Device_Def.DEVICE_CODE_D) Array.Copy(D_Reg, start_address, ret, 0, count);
                    else if (reg == Device_Def.DEVICE_CODE_R) Array.Copy(R_Reg, start_address, ret, 0, count);
                    else if (reg == Device_Def.DEVICE_CODE_W) Array.Copy(W_Reg, start_address, ret, 0, count);
                    else if (reg == Device_Def.DEVICE_CODE_ZR) Array.Copy(ZR_Reg, start_address, ret, 0, count);
            
                    //else if (reg == DEF_MELSEC.DEVICE_CODE_W) Array.Copy(W_Reg, start_address, ret, 0, count);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return ret;
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
        public static bool Write(int reg, int start_address, UInt16[] data)     // 메모리 데이터 값 쓰기 (레지, 시작 주소, 데이터 (배열))
        {
            lock (SyncObject)
            {
                bool ret = false;
                string regstr = string.Empty;
                try
                {
                    if (reg == Device_Def.DEVICE_CODE_D) { Array.Copy(data, 0, D_Reg, start_address, data.Length); regstr = "D"; }
                    else if (reg == Device_Def.DEVICE_CODE_R) { Array.Copy(data, 0, R_Reg, start_address, data.Length); regstr = "R"; }
                    else if (reg == Device_Def.DEVICE_CODE_W) { Array.Copy(data, 0, W_Reg, start_address, data.Length); regstr = "W"; }
                    else if (reg == Device_Def.DEVICE_CODE_ZR) { Array.Copy(data, 0, ZR_Reg, start_address, data.Length); regstr = "ZR"; }

                    //else if (reg == DEF_MELSEC.DEVICE_CODE_W) Array.Copy(data, 0, W_Reg, start_address, data.Length);
                    ret = true;
                    //Array.Reverse(intBytes);
                    //byte[] result = intBytes;
                }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return ret;
            }
        }


    }


}