using PLCProject;
using SFA_UTIL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PLCProject.Device_Def;

namespace PLC_Simulator
{

    
    public partial class InputUI : Form
    {
        string sDataType = string.Empty;
        int AddressNo = 0;
        int RegisterNo = 0;
        Form1 f1;
        int state;
        CDataObject cs = new CDataObject();
        

        public InputUI()
        {
            InitializeComponent();
        }

        public InputUI(string dataType, int iRegisterNo, int iAddressNo, Form1 form)
            : this()
        {
            this.sDataType = dataType;
            this.AddressNo = iAddressNo;
            this.RegisterNo = iRegisterNo;
            f1 = form;
        }

        private void SetByteValue(byte[] values)
        {
            try
            {
                UInt16[] x = new UInt16[1];
                x[0] = BitConverter.ToUInt16(values, 0);

                PLCManager.Write(RegisterNo, AddressNo, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void SendByteValue(byte[] values)
        {
            try
            {
                UInt16[] x = new UInt16[1];
                x[0] = BitConverter.ToUInt16(values, 0);

               
                Form1.MainForm.InputUISend(values,AddressNo);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetIntegerValue(UInt16 value)
        {
            try
            {
                UInt16[] x = new UInt16[1];
                x[0] = value;

                PLCManager.Write(RegisterNo, AddressNo, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void SendIntegerValue(UInt16 value)
        {
            try
            {
                byte[] values = BitConverter.GetBytes(value);
                
                Form1.MainForm.InputUIIntegerSend(values, AddressNo);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetIntegerValue(UInt32 value)
        {
            try
            {
                UInt16[] x = new UInt16[2];
                x[0] = (UInt16)(value & 0xFFFF);
                x[1] = (UInt16)((value >> 16) & 0xFFFF);
                PLCManager.Write(RegisterNo, AddressNo, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetIntegerValue(UInt64 value)
        {
            try
            {
                UInt16[] x = new UInt16[4];
                x[0] = (UInt16)(value & 0xFFFF);
                x[1] = (UInt16)((value >> 16) & 0xFFFF);
                x[2] = (UInt16)((value >> 32) & 0xFFFF);
                x[3] = (UInt16)((value >> 48) & 0xFFFF);
                PLCManager.Write(RegisterNo, AddressNo, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetHexStringValue(string value)
        {
            try
            {
                UInt16[] x = new UInt16[1];

                x[0] = Convert.ToUInt16(value, 16);
                PLCManager.Write(RegisterNo, AddressNo, x);
            }
            catch (Exception ex)
            {
                MessageBox.Show("범위를 벗어났습니다.");
                //MessageBox.Show(ex.ToString());
            }
        }

        private void SetStringValue(string value)
        {
            try
            {
                byte[] values = System.Text.Encoding.Default.GetBytes(value);

                PLCManager.WriteForByte(RegisterNo, AddressNo, values);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SendStringValue(string value)
        {
            try
            {
                byte[] values = System.Text.Encoding.Default.GetBytes(value);

                Form1.MainForm.SendStringValue(values,AddressNo); ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //private bool CheckInputInvalidate(string data, string dataType)
        //{
        //    if (dataType == PLCSetDataType.HIGHT_BYTE || data == PLCSetDataType.LOW_BYTE)
        //    {
        //        int iVal = int.Parse(data);

        //        if (iVal > Byte.MaxValue) return false;
        //    }
        //    else if (dataType == PLCSetDataType.INTEGER)
        //    {
        //        UInt16 iVal = UInt16.Parse(data);

        //        if (iVal > UInt16.MaxValue) return false;
        //    }
        //    else if (dataType == PLCSetDataType.HEX)
        //    {
        //        if (data.Length > 4) return false;
        //    }
        //    else if (dataType == PLCSetDataType.ASCII)
        //    {

        //    }

        //    return true;
        //}

        //private void FormChangePLC_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        FormChangePLC_Load_1(null, null);
        //    }
        //}

        private void button1_Click_1(object sender, EventArgs e)        // 입력 버튼
        {
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    MessageBox.Show("입력 된 정보가 없습니다.");
                    return;
                }
                if(state == 1) { 

                string data = textBox1.Text;
                string dataType = string.Empty;

                if (sDataType == PLCSetDataType.HIGHT_BYTE)
                {
                    UInt16[] values = PLCManager.Read(RegisterNo, AddressNo, 1);
                    byte[] byValues = new byte[2];
                    byValues[0] = (Byte)(values[0] & 0x00FF);
                    if (Convert.ToInt32(data) > 255 || Convert.ToInt32(data) < 0)
                    {
                        MessageBox.Show("범위를 벗어났습니다.");
                        return;
                    }
                    byValues[1] = Convert.ToByte(data);

                    SetByteValue(byValues);
                }
                else if (sDataType == PLCSetDataType.LOW_BYTE)
                {
                    UInt16[] values = PLCManager.Read(RegisterNo, AddressNo, 1);
                    byte[] byValues = new byte[2];
                    if (Convert.ToInt32(data) > 255 || Convert.ToInt32(data) < 0)
                    {
                        MessageBox.Show("범위를 벗어났습니다.");
                        return;
                    }
                    byValues[0] = Convert.ToByte(data);
                    byValues[1] = (Byte)((values[0] & 0xFF00) >> 8);

                    SetByteValue(byValues);
                }
                else if (sDataType == PLCSetDataType.INTEGER)
                {
                    if (comboBox1.Text == "UINT16")
                    {
                        if (Convert.ToInt32(data) > 65535 || Convert.ToInt32(data) < 0)
                        {
                            MessageBox.Show("범위를 벗어났습니다.");
                            return;
                        }
                        SetIntegerValue(Convert.ToUInt16(data));
                    }
                    else if (comboBox1.Text == "UINT32")
                    {
                        if (Convert.ToInt64(data) > 4294967295 || Convert.ToInt16(data) < 0)
                        {
                            MessageBox.Show("범위를 벗어났습니다.");
                            return;
                        }
                        SetIntegerValue(Convert.ToUInt32(data));
                    }
                    else if (comboBox1.Text == "UINT64")
                    {
                        SetIntegerValue(Convert.ToUInt64(data));
                    }
                }

                    else
                    {
                        if (data.Length > 2)
                        {
                            MessageBox.Show("2글자만 입력이 가능합니다.");
                            return;
                        }
                        else { SetStringValue(data); }

                    }

                    f1.RefreshDisplayPLCData();
                Close();
            }
                if(state == 2)
                {
                    string data = textBox1.Text;
                    string dataType = string.Empty;

                    if (sDataType == PLCSetDataType.HIGHT_BYTE)
                    {
                        UInt16[] values = PLCManager.Read(RegisterNo, AddressNo, 1);
                        byte[] byValues = new byte[2];
                        byValues[0] = (Byte)(values[0] & 0x00FF);
                        if (Convert.ToInt32(data) > 255 || Convert.ToInt32(data) < 0)
                        {
                            MessageBox.Show("범위를 벗어났습니다.");
                            return;
                        }
                        byValues[1] = Convert.ToByte(data);

                        SendByteValue(byValues);
                    }
                    else if (sDataType == PLCSetDataType.LOW_BYTE)
                    {
                        UInt16[] values = PLCManager.Read(RegisterNo, AddressNo, 1);
                        byte[] byValues = new byte[2];
                        if (Convert.ToInt32(data) > 255 || Convert.ToInt32(data) < 0)
                        {
                            MessageBox.Show("범위를 벗어났습니다.");
                            return;
                        }
                        byValues[0] = Convert.ToByte(data);
                        byValues[1] = (Byte)((values[0] & 0xFF00) >> 8);

                        SendByteValue(byValues);
                    }
                    else if (sDataType == PLCSetDataType.INTEGER)
                    {
                        if (comboBox1.Text == "UINT16")
                        {
                            if (Convert.ToInt32(data) > 65535 || Convert.ToInt32(data) < 0)
                            {
                                MessageBox.Show("범위를 벗어났습니다.");
                                return;
                            }
                            SendIntegerValue(Convert.ToUInt16(data));
                        }
                        else if (comboBox1.Text == "UINT32")
                        {
                            if (Convert.ToInt64(data) > 4294967295 || Convert.ToInt16(data) < 0)
                            {
                                MessageBox.Show("범위를 벗어났습니다.");
                                return;
                            }
                            SetIntegerValue(Convert.ToUInt32(data));
                        }
                        else if (comboBox1.Text == "UINT64")
                        {
                            SetIntegerValue(Convert.ToUInt64(data));
                        }
                    }

                    else
                    {
                        if(data.Length < 2)
                        {
                            MessageBox.Show("2글자만 입력이 가능합니다.");
                                return;
                        }
                        else { SendStringValue(data); }
                        
                    }

                    f1.RefreshDisplayPLCData();
                    Close();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("다시 입력해 주세요");
                //MessageBox.Show(ex.ToString());
            }
        }

        private void FormChangePLC_Load_1(object sender, EventArgs e)           // 폼 디자인
        {
            state = Form1.MainForm.state;
            

            if (sDataType == PLCSetDataType.HIGHT_BYTE || sDataType == PLCSetDataType.LOW_BYTE)
            {
                comboBox1.Text = "BYTE"; ;
                label1.Text = "0~255 사이의 숫자를 입력하세요";
            }
            else if (sDataType == PLCSetDataType.INTEGER)
            {
                comboBox1.Text = "UINT16";
                label1.Text = "0~65535 사이의 숫자를 입력하세요";
            }
           
            else
            {
                comboBox1.Text = "ASCII";
                label1.Text = "ASCII 문자 2자를 입력하세요";
            }

            if (comboBox1.Text == "UINT16")
            {
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else
            {
                comboBox1.Enabled = false;
            }
        }
    }
}
