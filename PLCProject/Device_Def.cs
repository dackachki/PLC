using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCProject
{
   public class Device_Def
    {
       
        public static int[] Device_D = new int[100000];
        public static int[] Device_R = new int[100000];

        public static Array Device_Array_D = Device_D;
        public static Array Device_Array_R = Device_R;
        public static int[] ablePorts = new int[UI_set.Enable_Client];

        public static string Date_info = "yyyy-MM-dd-HH-mm-ss";
        public static String IP = "127.0.0.1";
        public static int Port;
        public static String Device;
        public static int Start_Address;
        public static int Client_Send_Head_Length = 11;
        public static int Server_Send_Head_Length = 11;
        public static int Server_Return_Error_Length = 11;
        public static readonly byte SubHeader1 = 0x50;
        public static readonly byte SubHeader2 = 0x00;
        public static readonly byte Server_Return_SubHeader1 = 0xD0;
        public static readonly byte Server_Return_SubHeader2 = 0x00;
        public static readonly byte NetworkNumber = 0x00;
        public static readonly byte PLC_Number = 0xff;   // PLC Number 
        public static readonly byte Start_IO1 = 0xff;   // Start I/O - 1 (선두 I/O) - Gx Developer에 의해 설정된 번호
        public static readonly byte Start_IO2 = 0x03;   // Start I/0 - 2
        public static readonly byte Station_Number = 0x00;   // Station Number (국번) - Gx Developer에 의해 설정된 번호 
        public static readonly byte Monitor_timer1 = 0x10;   // 모니터 타이머 1
        public static readonly byte Monitor_timer2 = 0x00;   // 모니터 타이머 2
        public static readonly byte Write_Commend1 = 0x01;
        public static readonly byte Write_Commend2 = 0x14;
        public static readonly byte Read_Commend1 = 0x01;
        public static readonly byte Read_Commend2 = 0x04;
        public static readonly byte Sub_Commend1 = 0x00;
        public static readonly byte Sub_Commend2 = 0x00;
        public static readonly byte End_Code1 = 0x00;
        public static readonly byte End_Code2 = 0x00;
        public static readonly byte Error_Code = 0x51;

        public static readonly byte CMD_WORD_UNIT = 0x00;
        public static readonly byte CMD_BIT_UNIT = 0x01;
        public static readonly byte DEVICE_CODE_SM = 0x91;
        public static readonly byte DEVICE_CODE_SD = 0xA9;
        public static readonly byte DEVICE_CODE_X = 0x9C;
        public static readonly byte DEVICE_CODE_Y = 0x9D;
        public static readonly byte DEVICE_CODE_M = 0x90;
        public static readonly byte DEVICE_CODE_L = 0x92;
        public static readonly byte DEVICE_CODE_F = 0x93;
        public static readonly byte DEVICE_CODE_V = 0x94;
        public static readonly byte DEVICE_CODE_B = 0xA0;
        public static readonly byte DEVICE_CODE_D = 0xA8;
        public static readonly byte DEVICE_CODE_W = 0xB4;
        public static readonly byte DEVICE_CODE_TS = 0xC1;
        public static readonly byte DEVICE_CODE_TC = 0xC0;
        public static readonly byte DEVICE_CODE_TN = 0xC2;
        public static readonly byte DEVICE_CODE_SS = 0xC7;
        public static readonly byte DEVICE_CODE_SC = 0xC6;
        public static readonly byte DEVICE_CODE_SN = 0xC8;
        public static readonly byte DEVICE_CODE_CS = 0xC4;
        public static readonly byte DEVICE_CODE_CC = 0xC3;
        public static readonly byte DEVICE_CODE_CN = 0xC5;
        public static readonly byte DEVICE_CODE_SB = 0xA1;
        public static readonly byte DEVICE_CODE_SW = 0xB5;
        public static readonly byte DEVICE_CODE_S = 0x98;
        public static readonly byte DEVICE_CODE_DX = 0xA2;
        public static readonly byte DEVICE_CODE_DY = 0xA3;
        public static readonly byte DEVICE_CODE_Z = 0xCC;
        public static readonly byte DEVICE_CODE_R = 0xAF;
        public static readonly byte DEVICE_CODE_ZR = 0xB0;
        
      

        public static byte SearchDeviceCode(String device)
        {
            byte Device_byte = 0;
            Device = device;
            switch (device)
            {
                case "B":
                    Device_byte = Device_Def.DEVICE_CODE_B;
                    break;
                case "CC":
                    Device_byte = Device_Def.DEVICE_CODE_CC;
                    break;
                case "CN":
                    Device_byte = Device_Def.DEVICE_CODE_CN;
                    break;
                case "CS":
                    Device_byte = Device_Def.DEVICE_CODE_CS;
                    break;
                case "D":
                    Device_byte = Device_Def.DEVICE_CODE_D;
                    break;
                case "DX":
                    Device_byte = Device_Def.DEVICE_CODE_DX;
                    break;
                case "DV":
                    Device_byte = Device_Def.DEVICE_CODE_DY;
                    break;
                case "F":
                    Device_byte = Device_Def.DEVICE_CODE_F;
                    break;
                case "L":
                    Device_byte = Device_Def.DEVICE_CODE_L;
                    break;
                case "M":
                    Device_byte = Device_Def.DEVICE_CODE_M;
                    break;
                case "R":
                    Device_byte = Device_Def.DEVICE_CODE_R;
                    break;
                case "S":
                    Device_byte = Device_Def.DEVICE_CODE_S;
                    break;
                case "SB":
                    Device_byte = Device_Def.DEVICE_CODE_SB;
                    break;
                case "SC":
                    Device_byte = Device_Def.DEVICE_CODE_SC;
                    break;
                case "SD":
                    Device_byte = Device_Def.DEVICE_CODE_SD;
                    break;
                case "SM":
                    Device_byte = Device_Def.DEVICE_CODE_SM;
                    break;
                case "SN":
                    Device_byte = Device_Def.DEVICE_CODE_SN;
                    break;
                case "SS":
                    Device_byte = Device_Def.DEVICE_CODE_SS;
                    break;
                case "SW":
                    Device_byte = Device_Def.DEVICE_CODE_SW;
                    break;
                case "TC":
                    Device_byte = Device_Def.DEVICE_CODE_TC;
                    break;
                case "TN":
                    Device_byte = Device_Def.DEVICE_CODE_TN;
                    break;
                case "TS":
                    Device_byte = Device_Def.DEVICE_CODE_TS;
                    break;
                case "V":
                    Device_byte = Device_Def.DEVICE_CODE_V;
                    break;
                case "W":
                    Device_byte = Device_Def.DEVICE_CODE_W;
                    break;
                case "X":
                    Device_byte = Device_Def.DEVICE_CODE_X;
                    break;
                case "Y":
                    Device_byte = Device_Def.DEVICE_CODE_Y;
                    break;
                case "Z":
                    Device_byte = Device_Def.DEVICE_CODE_Z;
                    break;
                case "ZR":
                    Device_byte = Device_Def.DEVICE_CODE_ZR;
                    break;
            }
            return Device_byte;
        }

        public static int[] Search_Device_Array(String Device)
        {
            if (Device.Equals("D")) return Device_D;
            else if (Device.Equals("R")) return Device_R;
            else return null;

        }
       public class PLCSetDataType
        {
            public static readonly string HIGHT_BYTE = "HIGHT_BYTE";
            public static readonly string LOW_BYTE = "LOW_BYTE";
            public static readonly string INTEGER = "INTEGER";
            public static readonly string ASCII = "ASCII";
        }
       


        public class UI_set
        {
            public static readonly int GridLowCount = 14; // 그리드 행 수
            
            public static bool Server_Mode = true;
            public static int Enable_Client = 10; // 포트 숫자
            public static int[,] Socket_Connection = new int[Enable_Client, 2]; // 서버 모드시 포트, 통신상태
            public static int MaxClient = 1; // 포트별 클라이언트 수
            public static bool Connection_Live = false; // 모든 연결 관련 반복문 핸들러
            public static string Device_Selected;
            
        }
    }

}
