using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PLCProject.Device_Def;
using PLCProject;
using SFA_UTIL;
using System.Threading;
using System.Collections;
using PLC_Simulator;

namespace PLCProject
{
    public partial class Form1 : Form
    {
        private int[] Grid_Array;
        delegate void TimerEvent();
        public int state;
        public static Form1 MainForm;
        CSocketServer server;
        public ClientSocket CL;
        int readL = 15;



        PLCManager PL = new PLCManager();
        int iSelectedAddress = 0;
        int iSelectedRegister = Device_Def.DEVICE_CODE_D;
        bool clientConnected = false;
        private int m_nReadLength = 15;
        public string DeviceName = null;
        CDataObject SendFrame = new CDataObject();
        CDataObject wSendFrame = new CDataObject();
        private ushort[] AddrData;

        public Form1()
        {

            InitializeComponent();

            state = 0;
            MainForm = this;
            comboBox1.Text = "D";

            SetupGrid();

            //Gridset(Device_Def.Start_Address, Device_Def.Device);


        }

        private void callback(object state)
        {
            BeginInvoke(new TimerEvent(ClientStatusCheck));

        }

        private void ClientStatusCheck()
        {

            // Client 실행 시 서버측이 종료될떄
            if (CL.m_clientSocket != null)
            {
                if (CL.m_clientSocket.Connected)
                {
                    Control_Change(state);

                    DisplayGridData();
                }
                else
                {
                    CL.CloseSocket();
                    Control_Change(3);
                }

            }
            else
            {
                CL.CloseSocket();
                Control_Change(3);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {/*
            IPHostEntry aa = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress addr in aa.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    Device_Def.IP = addr.ToString();
                    break;
                }
            }
            */

            MainForm.Text = "Waiting";
            Device_Def.Device = comboBox1.Text;
            txtIP.Text = Device_Def.IP;
            //디버깅용 임시
            txtPort.Text = "8192";
            Device_Def.Port = int.Parse(txtPort.Text);
            DeviceName = comboBox1.Text;
            Device_Def.Start_Address = 0;

            /* 포트 라벨 만들기
            for (int i = 0; i <= portnum; i++)
            {
                int AllowPort = Device_Def.Port + i;
                string lbname =AllowPort.ToString();
                Label lb = createLabel(lbname);
                tablepanel.Controls.Add(lb, i, 0);
                
            }
            */

        }
        // ~ 116 클라이언트에서 input 폼을 통해 데이터 보낼때 CL을 사용하기 위해 끌어옴 
        internal void InputUIIntegerSend(byte[] values, int AddressNo)
        {
            SendFrame.MakeWriteFrame(Device_Def.CMD_WORD_UNIT, Device_Def.SearchDeviceCode(Device), values, AddressNo, 1);
            CL.SendMessage(SendFrame);
        }

        internal void InputUISend(byte[] values, int AddressNo)
        {
            SendFrame.MakeWriteFrame(Device_Def.CMD_WORD_UNIT, Device_Def.SearchDeviceCode(Device), values, AddressNo, 1);
            CL.SendMessage(SendFrame);
        }
        internal void SendStringValue(byte[] values, int AddressNo)
        {
            SendFrame.MakeWriteFrame(Device_Def.CMD_WORD_UNIT, Device_Def.SearchDeviceCode(Device), values, AddressNo, 1);
            CL.SendMessage(SendFrame);
        }

        void SetupGrid()    // 그리드뷰 설정
        {
            Grid.ColumnCount = 21;  // 컬럼 수
            Grid.RowCount = 16;     // Row 수

            Grid.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            // 편집 막기 (읽기 전용)
            for (int i = 0; i < 21; i++)    // 17, 22
            {
                Grid.Columns[i].ReadOnly = true;
            }
        }

        public Label createLabel(string name)
        {
            Label lb = new Label();
            lb.Text = name;
            lb.Name = name;
            lb.Anchor = AnchorStyles.None;
            return lb;
        }


        public void Gridset(int Start_ad, String Device)
        {
            byte[] tmpdata = new byte[readL * 2];



            Grid_Array = new int[UI_set.GridLowCount];
            int[] A = Device_Def.Search_Device_Array(Device);

            for (int i = 0; i < Grid_Array.Length; i++)
            {
                Grid_Array[i] = A[Start_ad + i];
            }

            String[] DGV_Data = new String[Grid.ColumnCount];
            int value_index = DGV_Data.Length - 4;


            for (int j = Start_ad; j < UI_set.GridLowCount + Start_ad; j++)
            {
                int i = 1;
                int Address = Start_ad + j;
                //Address값 출력 D0,D1..
                DGV_Data[0] = Device_Def.Device + Address.ToString();

                //디바이스의 배열 j번쨰를 2진법으로
                String bin = Convert.ToString(A[j], 2);

                //17 - 1 
                for (; i < value_index - bin.Length; i++)
                {
                    DGV_Data[i] = "0";
                }
                // 1
                for (int k = 0; k < bin.Length; k++)
                {
                    String msg = bin.Substring(k, 1);
                    DGV_Data[i] = msg;
                    i++;
                }

                byte[] b = BitConverter.GetBytes(A[j]);
                DGV_Data[value_index] = b[0].ToString();
                //high 컬럼 값
                DGV_Data[value_index + 1] = ""; //b[1].ToString();
                //low 컬럼 값
                DGV_Data[value_index + 2] = "";// A[j].ToString();
                try
                {

                    //All 컬럼 값
                    if (Convert.ToChar(b[0].ToString()).ToString().Equals(""))
                    {
                        DGV_Data[value_index + 3] = "";
                    }
                    else
                    {
                        DGV_Data[value_index + 3] = Convert.ToChar(b[0].ToString()).ToString();
                    }
                }
                catch (Exception e)
                {
                    DGV_Data[value_index + 3] += "";
                }


                try
                {
                    if (Convert.ToChar(b[1].ToString()).ToString().Equals(""))
                    {
                        DGV_Data[value_index + 3] += "";
                    }
                    else
                    {
                        DGV_Data[value_index + 3] += Convert.ToChar(b[1].ToString()).ToString();
                    }
                }
                catch (Exception e)
                {
                    DGV_Data[value_index + 3] += "";
                }


                Grid.Rows.Add(DGV_Data);

            }

        }



        /* 콤보박스로 디바이스 변경 필요시
private void CB_SelectedIndexChanged(object sender, EventArgs e)
{
 lock (obj)
{

    Device_Def.Device = CB.SelectedItem.ToString();
 }
}
*/

        private void BtServer_Click(object sender, EventArgs e)
        {


            if (BtSrv.Text == "Server")
            {
                try
                {
                    IP = txtIP.Text;
                    UI_set.Connection_Live = true;
                    server = new CSocketServer();
                    PL = new PLCManager();

                    Thread t = new Thread(new ParameterizedThreadStart(PL.SocketInit));
                    Thread gr = new Thread(RefreshDisplayPLCData_Except_AddressColumn);

                    if (!int.TryParse(txtPort.Text, out Port))
                    {
                        MessageBox.Show("유효한 포트번호가 아닙니다.");
                    }

                    state = 1;
                    PL.SocketLive = true;
                    Control_Change(state);

                    t.IsBackground = true;
                    t.Start(IP);
                    gr.Start();


                }
                catch (Exception ev)
                {
                    MessageBox.Show(ev.Message);
                }
            }

            else if (BtSrv.Text == "Server On")
            {
                Thread t = new Thread(new ParameterizedThreadStart(PL.SocketInit));
                Thread gr = new Thread(RefreshDisplayPLCData_Except_AddressColumn);
                BtSrv.Text = "Server";
                state = 0;
                server.CloseSockets();
                txtPort.Enabled = true;
                txtIP.Enabled = true;
                MainForm.Text = "Waiting";
                BtSrv.BackColor = SystemColors.Control;
                BtClient.Enabled = true;
                dataReset();
                t.Abort();
                gr.Abort();
                PL.SocketLive = false;
                Gridreset();


            }
        }

        private void Gridreset()
        {
            Grid.Rows.Clear();
            Grid.Refresh();
            SetupGrid();
        }

        private void BtClient_Click(object sender, EventArgs e)
        {
            if (BtClient.Text == "Client")
                try
                {

                    Device_Def.IP = txtIP.Text;
                    CL = new ClientSocket();
                    //Thread gr2 = new Thread(DisplayGridData);
                    if (int.TryParse(txtPort.Text, out Device_Def.Port))

                    {
                        state = 2;
                        clientConnected = CL.ConnectSocket(IP, Port);
                        //gr2.Start();
                        System.Threading.Timer timer = new System.Threading.Timer(callback);
                        timer.Change(0, 100);
                        Control_Change(state);
                    }

                }
                catch (Exception ce)
                {
                    MessageBox.Show(ce.Message);
                }

            else if (BtClient.Text == "Client On")
            {

                txtPort.Enabled = true;
                txtIP.Enabled = true;
                MainForm.Text = "Waiting";
                BtSrv.Enabled = true;
                BtClient.BackColor = SystemColors.Control;
                BtClient.Text = "Client";
                CL.CloseSocket();
                state = 0;
                dataReset();
            }

        }
        private void ClientContuinue()
        {
            while (clientConnected)
            {
                DisplayGridData();
            }
        }

        private void Control_Change(int state)
        {
            if (state == 1)
            {

                txtPort.Enabled = false;
                txtIP.Enabled = false;
                MainForm.Text = "Server ON";
                BtSrv.Text = "Server On";
                BtSrv.BackColor = Color.SkyBlue;
                BtClient.Enabled = false;

            }

            if (state == 2)
            {
                txtPort.Enabled = false;
                txtIP.Enabled = false;
                MainForm.Text = "Client ON";
                BtSrv.Enabled = false;
                BtClient.BackColor = Color.SkyBlue;
                BtClient.Text = "Client On";

            }
            if (state == 3)
            {
                txtPort.Enabled = true;
                txtIP.Enabled = true;
                MainForm.Text = "Waiting";
                BtSrv.Enabled = true;
                BtClient.BackColor = SystemColors.Control;
                BtClient.Text = "Client";

                state = -1;
                state++;


            }

        }

        private void gridControl_View_CellDoubleClick(object sender, DataGridViewCellEventArgs e)   // 그리드뷰 더블클릭 쓰기
        {
            int row = Grid.CurrentCell.RowIndex;
            int col = Grid.CurrentCell.ColumnIndex;

            if (state == 1)
            {

                if (e.ColumnIndex > 0)         // column : 15 ~ 0
                {
                    //18~21열
                    if (e.ColumnIndex >= 17)     // Column :  HIGH ~ ASCII
                    {
                        string selectAddr = Grid[e.ColumnIndex, 0].Value.ToString();
                        string dataType = string.Empty;
                        if (string.IsNullOrEmpty(selectAddr)) return;

                        int addressNo = row;

                        if (e.ColumnIndex == 17) dataType = PLCSetDataType.HIGHT_BYTE;      // HIGH    (08 ~ 15 비트)
                        else if (e.ColumnIndex == 18) dataType = PLCSetDataType.LOW_BYTE;   // LOW     (00 ~ 07 비트)
                        else if (e.ColumnIndex == 19) dataType = PLCSetDataType.INTEGER;    // ALL
                        else if (e.ColumnIndex == 20) dataType = PLCSetDataType.ASCII;       // ASCII
                        InputUI form = new InputUI(dataType, iSelectedRegister, iSelectedAddress + addressNo, this);
                        form.ShowDialog();

                    }
                    //1~16열
                    else
                    {
                        Grid[col, row].Value = 1;       // 선택한 셀 데이터 값 1

                        int tmp = 0;
                        try
                        {
                            tmp = int.Parse(Grid[e.ColumnIndex, e.RowIndex].Value.ToString());  // 문자열을 숫자로 변환 함수
                        }


                        catch
                        {
                            return;
                        }

                        CDataSet ds = new CDataSet(2);
                        byte[] tmpbyte = new byte[2];
                        tmpbyte = PLCManager.ReadForByte(Device_Def.SearchDeviceCode(Device_Def.Device), iSelectedAddress + (e.RowIndex), 2);     // 바이트로 읽기

                        ds.SetSourceByteArray(0, tmpbyte);
                        ds.SetSource(16 - e.ColumnIndex, (ds.GetSource(16 - e.ColumnIndex) == true) ? false : true);

                        PLCManager.WriteForByte(Device_Def.SearchDeviceCode(Device_Def.Device), iSelectedAddress + (e.RowIndex), ds.GetSource());  // 바이트로 쓰기
                    }
                }




                RefreshDisplayPLCData();

            }

            if (state == 2)
            {
                if (e.ColumnIndex > 0)         // column : 15 ~ 0
                {
                    //18~21열
                    if (e.ColumnIndex >= 17)     // Column :  HIGH ~ ASCII
                    {
                        string selectAddr = Grid[e.ColumnIndex, 0].Value.ToString();
                        string dataType = string.Empty;
                        if (string.IsNullOrEmpty(selectAddr)) return;

                        int addressNo = row;

                        if (e.ColumnIndex == 17) dataType = PLCSetDataType.HIGHT_BYTE;      // HIGH    (08 ~ 15 비트)
                        else if (e.ColumnIndex == 18) dataType = PLCSetDataType.LOW_BYTE;   // LOW     (00 ~ 07 비트)
                        else if (e.ColumnIndex == 19) dataType = PLCSetDataType.INTEGER;    // ALL

                        else if (e.ColumnIndex == 20) dataType = PLCSetDataType.ASCII;       // ASCII

                        InputUI form = new InputUI(dataType, iSelectedRegister, iSelectedAddress + addressNo, this);
                        form.ShowDialog();

                        //RefreshDisplayPLCData();

                    }
                    else
                    {
                        Grid[col, row].Value = 1;
                        CDataSet ds = new CDataSet(2);
                        byte[] tmpbyte = new byte[2];

                        tmpbyte = PLCManager.ReadForByte(Device_Def.SearchDeviceCode(Device_Def.Device), iSelectedAddress + (e.RowIndex), 2);     // 바이트로 읽기

                        ds.SetSourceByteArray(0, tmpbyte);
                        ds.SetSource(16 - e.ColumnIndex, (ds.GetSource(16 - e.ColumnIndex) == true) ? false : true);

                        PLCManager.WriteForByte(Device_Def.SearchDeviceCode(Device_Def.Device), iSelectedAddress + (e.RowIndex), ds.GetSource());
                        wSendFrame.MakeWriteFrame(Device_Def.CMD_WORD_UNIT, Device_Def.SearchDeviceCode(Device), ds.GetSource(), iSelectedAddress + (e.RowIndex), 1);

                        if (CL.SendMessage(wSendFrame) == false)
                        {

                        }

                    }

                }
                DisplayGridData();

            }
        }
        private int GetValueByColumn(int columnIndex)
        {

            switch (columnIndex)
            {
                case (1):
                    columnIndex = 15;
                    break;
                case (2):
                    columnIndex = 14;
                    break;
                case (3):
                    columnIndex = 13;
                    break;
                case (4):
                    columnIndex = 12;
                    break;
                case (5):
                    columnIndex = 11;
                    break;
                case (6):
                    columnIndex = 10;
                    break;
                case (7):
                    columnIndex = 9;
                    break;
                case (8):
                    columnIndex = 8;
                    break;
                case (9):
                    columnIndex = 7;
                    break;
                case (10):
                    columnIndex = 6;
                    break;
                case (11):
                    columnIndex = 5;
                    break;
                case (12):
                    columnIndex = 4;
                    break;
                case (13):
                    columnIndex = 3;
                    break;
                case (14):
                    columnIndex = 2;
                    break;
                case (15):
                    columnIndex = 1;
                    break;
                case (16):
                    columnIndex = 0;
                    break;

            }
            int value = Convert.ToInt16(Math.Pow(2, columnIndex));


            return value;
        }

        internal void RefreshDisplayPLCData()        // 그리드뷰 전체 셀 새로고침
        {

            UInt16[] vData = new UInt16[16];
            UInt16[] TempByte = new UInt16[1];

            for (int i = 0; i < vData.Length; i++)
            {
                TempByte = PLCManager.Read(iSelectedRegister, iSelectedAddress + i, 1);
                ushort a = TempByte[0];
                byte[] bb = BitConverter.GetBytes(a);
                for (int j = 0; j < bb.Length; j++)
                {
                    BitArray b = new BitArray(new byte[] { bb[j] });
                    //BitArrayReverse(b);
                    bb[j] = BitConvertToByte(b);
                }
                //Array.Reverse(b);
                TempByte[0] = BitConverter.ToUInt16(bb, 0);
                Array.Copy(TempByte, 0, vData, i, 1);
            }



            vData = PLCManager.Read(iSelectedRegister, iSelectedAddress, 15);   // 16 ==> 100

            for (int i = 0; i < vData.Length; i++)
            {
                Grid[0, i + 0].Value = Device_Def.Device + (iSelectedAddress + i).ToString(); // Address Column 관리                    
                for (int j = 15; j >= 0; j--)
                {
                    Grid[16 - j, i].Value = (vData[i] & GetBitValue(j)) == GetBitValue(j) ? "1" : "0";      // 16개만 찍음 (D0 ~ D15)

                    if (Grid[16 - j, i].Value.ToString() == ((vData[i] & GetBitValue(j)) == GetBitValue(j) ? "1" : "0"))
                    {
                        if (Grid[16 - j, i].Value.ToString() == "1")      // 비트 데이터 1이면
                        {
                            Grid[16 - j, i].Style.BackColor = Color.Aqua;   // 활성화 색
                        }
                        else
                        {
                            Grid[16 - j, i].Style.BackColor = Color.White;  // 비 활성화 색
                        }
                    }


                    //if (m_VirtualPLC.SocketConnect != null)
                    //{
                    //    if (m_VirtualPLC.SocketConnect[i, 1] == 1)
                    //    {
                    //        gridControl_View[i + 1, 16].Style.BackColor = Color.Aqua;
                    //    }
                    //    else
                    //    {
                    //        gridControl_View[i + 1, 16].Style.BackColor = Color.White;
                    //    }
                    //}

                }

                Grid[17, i].Value = ((vData[i] & 0xff00) / 256).ToString();     // HIGH
                Grid[18, i].Value = ((vData[i] & 0x00ff)).ToString();           // LOW
                Grid[19, i].Value = vData[i].ToString();                        // ALL
                Grid[20, i].Value = ((char)((vData[i] & 0xff00) / 256)).ToString() + ((char)(vData[i] & 0x00ff)).ToString();        // ASCII

            }


        }
        public byte BitConvertToByte(BitArray bits)
        {
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
        UInt16 GetBitValue(double index)
        {
            return UInt16.Parse(Math.Pow(2, index).ToString());             // 2^n 비트 계산
        }

        internal void RefreshDisplayPLCData_Except_AddressColumn()        // 그리드뷰 Address Column 뺀, 나머지 새로고침
        {
            while (state > 0)
            {

                UInt16[] vData = new UInt16[16];
                UInt16[] TempByte = new UInt16[1];

                //vdata[16]에 주소 ~ +16까지 배열값 읽어오기
                vData = PLCManager.Read(iSelectedRegister, iSelectedAddress, 15);   // 16 ==> 100

                //row
                for (int i = 0; i < vData.Length; i++)

                {
                    Grid[0, i + 0].Value = Device_Def.Device + (iSelectedAddress + i).ToString();
                    //column
                    for (int j = 15; j >= 0; j--)
                    {    //첫번째줄 처음셀 부터                 //읽어온 배열 내부값 & 쉘의 비트 변환값 == 같으면 1
                        Grid[16 - j, i].Value = (vData[i] & GetBitValue(j)) == GetBitValue(j) ? "1" : "0";      // 16개만 찍음 (D0 ~ D15)


                        if (Grid[16 - j, i].Value.ToString() == ((vData[i] & GetBitValue(j)) == GetBitValue(j) ? "1" : "0"))
                        {
                            if (Grid[16 - j, i].Value.ToString() == "1")      // 비트 데이터 1이면
                            {
                                Grid[16 - j, i].Style.BackColor = Color.Aqua;   // 활성화 색
                            }
                            else
                            {
                                Grid[16 - j, i].Style.BackColor = Color.White;  // 비 활성화 색
                            }
                        }
                        //if (m_VirtualPLC.SocketConnect != null)
                        //{
                        //    if (m_VirtualPLC.SocketConnect[i, 1] == 1)
                        //    {
                        //        gridControl_View[i + 1, 16].Style.BackColor = Color.Aqua;
                        //    }
                        //    else
                        //    {
                        //        gridControl_View[i + 1, 16].Style.BackColor = Color.White;
                        //    }
                        //}
                    }
                    // j 바깥쪽 17~21 값 변환 
                    //포트 번호 나열

                    Grid[0, 15].Value = "접속정보";
                    Grid[17, i].Value = ((vData[i] & 0xff00) / 256).ToString();     // HIGH
                    Grid[18, i].Value = ((vData[i] & 0x00ff)).ToString();           // LOW
                    Grid[19, i].Value = vData[i].ToString();                        // ALL
                    Grid[20, i].Value = ((char)((vData[i] & 0xff00) / 256)).ToString() + ((char)(vData[i] & 0x00ff)).ToString();        // ASCII
                }

                //서버일때 포트 번호 나열
                for (int i = 0; i < UI_set.Enable_Client; i++)
                {
                    Grid[i + 1, 15].Value = Port + i;

                    Device_Def.ablePorts[i] = Port + i;

                }
                
                    int a = 0;
                    while (a < UI_set.Enable_Client)
                    {
                    
                        }
                    }

                }
            
        




        private void btAddr_Click(object sender, EventArgs e)
        {
            try
            {
                int addrlength = Device_Def.Device_Array_D.Length;
                if (!int.TryParse(txtAddr.Text, out iSelectedAddress))
                {
                    MessageBox.Show("숫자를 입력해주세요");
                    return;
                }

                if (iSelectedAddress.GetType() != typeof(int))
                {
                    if (iSelectedAddress > addrlength)
                    {

                        MessageBox.Show("어드레스의 값이 유효하지 않습니다.");
                        return;
                    }
                }

            }
            catch
            {

            }

            RefreshDisplayPLCData();
        }




        private void FormClose(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            this.Close();
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            dataReset();
        }
        private void dataReset()
        {
            switch (DeviceName)
            {
                case ("D"):
                    PLCManager.D_Reg = new ushort[100000];
                    break;
                case ("W"):
                    PLCManager.W_Reg = new ushort[100000];
                    break;
                case ("R"):
                    PLCManager.R_Reg = new ushort[100000];
                    break;
                case ("ZR"):
                    PLCManager.ZR_Reg = new ushort[100000];
                    break;

            }
            RefreshDisplayPLCData_Except_AddressColumn();
        }

        void comboBox_Changed()
        {
            DeviceName = comboBox1.Text;
            Device_Def.Device = comboBox1.Text;
        }


        void DisplayGridData()
        {


            byte[] tmpdata = new byte[m_nReadLength * 3];

            SendFrame.MakeReadFrame(Device_Def.CMD_WORD_UNIT, Device_Def.SearchDeviceCode(DeviceName), iSelectedAddress, m_nReadLength);

            if (CL.SendMessage(SendFrame) == false)
            {
                return;
            }
            else
            {
                CDataObject data = CL.GetReceiveData();

                if (data != null)
                {
                    if (data.Count == (m_nReadLength * 2 + 11))
                    {
                        if (data.m_Data[9] == 0x00 && data.m_Data[10] == 0x00)
                        {

                            Array.Copy(data.m_Data, 11, tmpdata, 0, m_nReadLength * 2);

                        }
                    }
                    else
                    {

                    }
                }
            }
            //row만큼 반복
            for (int nRow = 0; nRow < Grid.RowCount - 1; nRow++)
            {
                int nColumnIndex = 0;
                // Address
                string szDeviceName = String.Format("{0}{1}", DeviceName, iSelectedAddress + nRow);
                Grid.Rows[nRow].Cells[nColumnIndex].Value = szDeviceName;
                nColumnIndex++;

                //low
                for (int nCol = 0; nCol < 8; nCol++)
                {
                    //데이터 배열값과 값이 같으면0 
                    int v = (1 << (nCol % 8));
                    if ((tmpdata[nRow * 2 + 1] & (1 << (nCol % 8))) == 0)
                    {
                        Grid.Rows[nRow].Cells[8 - nCol].Value = 0;
                        Grid.Rows[nRow].Cells[8 - nCol].Style.BackColor = Color.White;
                    }
                    //다르면 1
                    else
                    {
                        Grid.Rows[nRow].Cells[8 - nCol].Value = 1;
                        Grid.Rows[nRow].Cells[8 - nCol].Style.BackColor = Color.Cyan;
                    }

                    nColumnIndex++;
                }
                //high
                for (int nCol = 0; nCol < 8; nCol++)
                {
                    if ((tmpdata[nRow * 2] & (1 << (nCol % 8))) == 0)
                    {
                        Grid.Rows[nRow].Cells[16 - nCol].Value = 0;
                        Grid.Rows[nRow].Cells[16 - nCol].Style.BackColor = Color.White;
                    }
                    else
                    {
                        Grid.Rows[nRow].Cells[16 - nCol].Value = 1;
                        Grid.Rows[nRow].Cells[16 - nCol].Style.BackColor = Color.Cyan;
                    }


                    nColumnIndex++;
                }
                //17
                Grid.Rows[nRow].Cells[nColumnIndex].Value = tmpdata[nRow * 2 + 1];
                nColumnIndex++;
                //18
                Grid.Rows[nRow].Cells[nColumnIndex].Value = tmpdata[nRow * 2];
                nColumnIndex++;

                // All
                Grid.Rows[nRow].Cells[nColumnIndex].Value = GetIntLengthWord(tmpdata, nRow * 2);
                nColumnIndex++;

                // ASCII
                Grid.Rows[nRow].Cells[nColumnIndex].Value = Encoding.Default.GetString(tmpdata, nRow * 2 + 1, 1) + Encoding.Default.GetString(tmpdata, nRow * 2, 1);

            }

        }
        private int GetIntLengthWord(byte[] data, int byteindex)
        {
            uint temp = 0;
            temp = (uint)data[byteindex];
            temp = temp + (uint)data[byteindex + 1] * 256;
            return (int)temp;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            iSelectedRegister = Device_Def.SearchDeviceCode(comboBox1.Text);
            DeviceName = comboBox1.Text;
        }
    }
}
