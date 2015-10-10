using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Microsoft.Win32;
using System.IO.Ports;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Runtime.InteropServices;





namespace prp
{  
    public partial class FormSerial : Form
    {
        private bool firstActive;                  // 第一次激活窗口
        private ArrayList capDevices;              // 视频设备列表
        private string fileName;                   // 保存视频avi文件
        private IBaseFilter capFilter;             // 视频设备的 filterinfo
        private IGraphBuilder graphBuilder;        // graphBuilder接口
        private ICaptureGraphBuilder2 capGraph;    // 捕捉Graph接口
        private IVideoWindow videoWin;             // video window interface. 
        private IMediaControl mediaCtrl;           // Media control interface
        private IMediaEventEx mediaEvt;            // Media event interface

        private const int WM_GRAPHNOTIFY = 0x00008001;	// message from graph
        private const int WS_CHILD = 0x40000000;	// attributes for video window
        private const int WS_CLIPCHILDREN = 0x02000000;
        private const int WS_CLIPSIBLINGS = 0x04000000;
        

        private void getpos(int ID,int position,int velocity)
        {
            byte[] tempbyte=new byte [11]{0,0,0,0,0,0,0,0,0,0,0,}; 
           
            byte[] positionn = new byte[11] { 0xFF, 0xFF, 0x00, 0x07, 0x03, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00 };
///////////////////////////////////////////////////////////////////////////////////////////////////////////
            int temp_velocity = 0;
            int temp_position = 0;
            byte temp_vell = 0;
            byte temp_velh = 0;
            byte posh = 0;
            byte posl = 0;
            int temp_sum = 0;
            if (velocity > 1023)
            {
                temp_velocity = 1023;
            }
            else
            {
                temp_velocity = velocity;
            }
            if (position > 1023)
            {
                temp_position = 1023;
            }
            else
            {
                temp_position = position;
            }
            temp_velh = Convert.ToByte((temp_velocity >> 8)&0x000000ff);
            temp_vell = Convert.ToByte((temp_velocity) & 0x000000ff);
            posh = Convert.ToByte((temp_position >> 8) & 0x000000ff);
            posl = Convert.ToByte((temp_position) & 0x000000ff);
            positionn[2] = Convert.ToByte(ID);
            positionn[6] = posl;
            positionn[7] = posh;
            positionn[8] = temp_vell;
            positionn[9] = temp_velh;
            temp_sum = ID + 7 + 0X03 + 0x1e + ((temp_velocity >> 8) & 0x000000ff) + ((temp_velocity) & 0x000000ff) + ((temp_position >> 8) & 0x000000ff) + ((temp_position) & 0x000000ff);

            temp_sum = temp_sum ^ 0xff;
            positionn[10] = Convert.ToByte(temp_sum & 0x000000ff);
       
            serialPort.Write(positionn, 0, tempbyte.Length);
            //tempbyte =positionn);//发送非打印字符必须转换16进制为字节类型
            //serialPort.Write(tempbyte, 0, tempbyte.Length);
            //////////////////////////////////////////////////////
        
        
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////
        /// //////////////////////////////////////////////////////////////////////////////////////
        
      /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#region 窗口初始化
        private void FormSerial_Load(object sender, EventArgs e)
        {
            iniForm();
            setcom();
           
        }
        public FormSerial()
        {
            InitializeComponent();
        }
#endregion
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void frmCapture_Activated() //private void frmCapture_Activated(object sender, EventArgs e)
        {
            if (firstActive) return;
            firstActive = true;

            if (!DsUtils.IsCorrectDirectXVersion())
            {
                MessageBox.Show(this, "DirectX 8.1 NOT installed!", "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close(); return;
            }

            if (!DsDev.GetDevicesOfCat(FilterCategory.VideoInputDevice, out capDevices))
            {
                MessageBox.Show(this, "No video capture devices found!", "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close(); return;
            }

            SaveFileDialog sd = new SaveFileDialog();
            sd.FileName = @"CaptureNET.avi";
            sd.Title = "Save Video Stream as...";
            sd.Filter = "Video file (*.avi)|*.avi";
            sd.FilterIndex = 1;
            if (sd.ShowDialog() != DialogResult.OK)
            { this.Close(); return; }
            fileName = sd.FileName;

            DsDevice dev = null;
            if (capDevices.Count == 1)
                dev = capDevices[0] as DsDevice;
            else
            {
                DeviceSelector selector = new DeviceSelector(capDevices);
                selector.ShowDialog(this);
                dev = selector.SelectedDevice;
            }

            if (dev == null)
            {
                this.Close(); return;
            }

            if (!StartupVideo(dev.Mon))
                this.Close();
            this.Text = "机器狗调试软件-哈尔滨工程大学";
        }

        /// <summary>
        /// Luoz:start all the interfaces, graphs and preview window
        /// </summary>
        bool StartupVideo(IMoniker mon)
        {
            int hr;
            try
            {
                if (!CreateCaptureDevice(mon))
                    return false;

                if (!GetInterfaces())
                    return false;

                if (!SetupGraph())
                    return false;

                if (!SetupVideoWindow())
                    return false;

#if DEBUG
                //DsROT.AddGraphToRot(graphBuilder, out rotCookie);		// graphBuilder capGraph
#endif

                hr = mediaCtrl.Run();
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not start video stream\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }

        /// <summary>
        /// Luoz: 创建用户选择的设备设备
        /// </summary>
        bool CreateCaptureDevice(IMoniker mon)
        {
            object capObj = null;
            try
            {
                Guid gbf = typeof(IBaseFilter).GUID;
                mon.BindToObject(null, null, ref gbf, out capObj);
                capFilter = (IBaseFilter)capObj; capObj = null;
                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not create capture device\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            finally
            {
                if (capObj != null)
                    Marshal.ReleaseComObject(capObj); capObj = null;
            }
        }

        /// <summary>
        /// Luoz: create the used COM components and get the interfaces
        /// </summary>
        /// <returns></returns>
        bool GetInterfaces()
        {
            Type comType = null;
            object comObj = null;
            try
            {
                comType = Type.GetTypeFromCLSID(Clsid.FilterGraph);
                if (comType == null)
                    throw new NotImplementedException(@"DirectShow FilterGraph not installed/registered!");
                comObj = Activator.CreateInstance(comType);
                graphBuilder = (IGraphBuilder)comObj; comObj = null;

                //Guid clsid = Clsid.CaptureGraphBuilder2;
                //Guid riid = typeof(ICaptureGraphBuilder2).GUID;
                //comObj = DsBugWO.CreateDsInstance(ref clsid, ref riid);
                //capGraph = (ICaptureGraphBuilder2)comObj; comObj = null;

                comType = Type.GetTypeFromCLSID(Clsid.CaptureGraphBuilder2);
                comObj = Activator.CreateInstance(comType);
                capGraph = (ICaptureGraphBuilder2)comObj; comObj = null;

                mediaCtrl = (IMediaControl)graphBuilder;
                videoWin = (IVideoWindow)graphBuilder;
                mediaEvt = (IMediaEventEx)graphBuilder;
                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not get interfaces\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            finally
            {
                if (comObj != null)
                    Marshal.ReleaseComObject(comObj); comObj = null;
            }
        }

        /// <summary>
        /// Luoz: build the capture graph
        /// </summary>
        /// <returns></returns>
        bool SetupGraph()
        {
            int hr;
            IBaseFilter mux = null;
            IFileSinkFilter sink = null;

            try
            {
                hr = capGraph.SetFiltergraph(graphBuilder);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                hr = graphBuilder.AddFilter(capFilter, "Ds.NET Video Capture Device");
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                DsUtils.ShowCapPinDialog(capGraph, capFilter, this.Handle);

                Guid sub = MediaSubType.Avi;
                hr = capGraph.SetOutputFileName(ref sub, fileName, out mux, out sink);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                Guid cat = PinCategory.Capture;
                Guid med = MediaType.Video;
                hr = capGraph.RenderStream(ref cat, ref med, capFilter, null, mux); // stream to file 
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                cat = PinCategory.Preview;
                med = MediaType.Video;
                hr = capGraph.RenderStream(ref cat, ref med, capFilter, null, null); // preview window
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup graph\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            finally
            {
                if (mux != null)
                    Marshal.ReleaseComObject(mux); mux = null;
                if (sink != null)
                    Marshal.ReleaseComObject(sink); sink = null;
            }
        }

        /// <summary>
        /// Luoz： 预览
        /// </summary>
        bool SetupVideoWindow()
        {
            int hr;
            try
            {
                hr=videoWin.SetWindowPosition(panel1.Left,panel1.Top,panel1.Width,panel1.Height); 
               // SetWindowPosition(int left, int top, int width, int height);
                // Set the video window to be a child of the main window
                hr = videoWin.put_Owner(panel1.Handle);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                // Set video window style
                hr = videoWin.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                // Use helper function to position video window in client rect of owner window
                //ResizeVideoWindow();

                // Make the video window visible, now that it is properly positioned
                hr = videoWin.put_Visible(DsHlp.OATRUE);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                hr = mediaEvt.SetNotifyWindow(this.Handle, WM_GRAPHNOTIFY, IntPtr.Zero);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                return true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, "Could not setup video window\r\n" + ee.Message, "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
        }
///////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

#region 全局变量声明
        string InputData = String.Empty;
        string[] cmds = new string[] { "AA00", "AA01", "AA02", "AA03", "AA04", "AA05", "AA06", "AA07", "AA08", "AA09", "AA10", "AA11", "AA12", "AA13", "AA14" };
       // string reStr;
        Boolean cam;
#endregion

#region 自定义函数
        /// 16进制字符串转字节数组    
        private static byte[] HexStrToByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            }
            return returnBytes;
        }

        /// 字节数组转16进制字符串    
        public static string ByteToHexStr(byte[] data)
        {
                   StringBuilder sb = new StringBuilder(data.Length * 2);
                   foreach (byte b in data)//sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' ')); 
                   sb.Append(b.ToString("X").PadLeft(2, '0').PadRight(3, ' '));  
                   return sb.ToString();//.ToUpper();
        }

        //显示jpg格式图片
        private void picShow(string s)
        {
            string tempfile = Path.GetTempFileName();//申请临时文件
            FileStream fs = new FileStream(tempfile, FileMode.OpenOrCreate);
            byte[] dataArray;
            dataArray = HexStrToByte(s);//十六进制字符串转换为二进制字节类型
            for (int i = 0; i < dataArray.Length; ++i)
            {
                fs.WriteByte(dataArray[i]);//把图片数据先保存到临时文件，数据完整后再显示
            }
            fs.Close();
           // pictureBox.Image = Image.FromFile(tempfile);//显示图片
        }

        private void iniForm()
        {
            cam = false; 

            comboBoxCom.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())comboBoxCom.Items.Add(s);
            if (comboBoxCom.Items.Count > 0) comboBoxCom.SelectedIndex = 0;//检测可用串口
            else
            {
                MessageBox.Show(this, "There are no COM Ports detected on this computer.\nPlease install a COM Port and restart this app.", "No COM Ports Installed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
#endregion

#region 串口设置改变
        private void setcom()
        {
            try
            {
                if (serialPort.IsOpen) serialPort.Close();//先关闭串口再设置参数
                if (!serialPort.IsOpen)
                {
                    flag.BackColor = Color.Red;//改变串口是否打开的颜色提示
                    buttonserial.Text = "打开串口";//改变按钮提示
                    serialPort.PortName = comboBoxCom.Text;//串口名
                    serialPort.BaudRate = int.Parse(comboBoxBaud.Text);//波特率
                    serialPort.DataBits = int.Parse(comboBoxData.Text);//数据位
                    serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), comboBoxCheck.SelectedIndex.ToString());
                    serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits),comboBoxStop.Text);//校验位和停止位
                    //MessageBox.Show(serialPort.PortName+":" +serialPort.BaudRate+":" +serialPort.DataBits +":" +serialPort.Parity.ToString() + ":" + serialPort.StopBits.ToString());
                   // serialPort.ReceivedBytesThreshold = 1;//触发字符数
                    serialPort.Open();//打开串口
                    if (serialPort.IsOpen)
                    {
                        flag.BackColor = Color.Green;
                        buttonserial.Text = "关闭串口";
                    }
                   // else MessageBox.Show("串口设置成功！");
                }
              //  else MessageBox.Show("串口设置失败2！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);//异常处理
            }
        }

        //打开串口或关闭串口
        private void buttonserial_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    buttonserial.Text = "关闭串口";
                    flag.BackColor = Color.Green;
                    //   if (serialPort.IsOpen) MessageBox.Show("串口打开");

                }
                else
                {
                    serialPort.Close();
                    buttonserial.Text = "打开串口";
                    flag.BackColor = Color.Red;
                    //     if (!serialPort.IsOpen) MessageBox.Show("串口关闭");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBoxCom_SelectedIndexChanged(object sender, EventArgs e)
        {
            setcom();
        }

        private void comboBoxBaud_SelectedIndexChanged(object sender, EventArgs e)
        {
            setcom();
        }

        private void comboBoxStop_SelectedIndexChanged(object sender, EventArgs e)
        {
            setcom();
        }

        private void comboBoxData_SelectedIndexChanged(object sender, EventArgs e)
        {
            setcom();
        }

        private void comboBoxCheck_SelectedIndexChanged(object sender, EventArgs e)
        {
            setcom();
        }

#endregion

#region 串口发送接收数据
       
                   
        delegate void SetTextCallback(string text);//创建委托(串口)
       
#endregion



        private void button1_Click_2(object sender, EventArgs e)
        {
            frmCapture_Activated();
        }
       

       
        
      


#region 一般函数
     

        private void buttonSavePic_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "jpg(*.jpg)|*.jpg|gif(*.gif)|*.gif|*.*(*.*)|*.*";
           // if (saveFileDialog.ShowDialog() == DialogResult.OK) pictureBox.Image.Save(saveFileDialog.FileName,ImageFormat.Jpeg);  
        }

      

      

#endregion

#region 次要函数
       
#endregion

#region  无关标签函数
        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flag_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
#endregion

       
       
        

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }



        private void saveFileDialog_FileOk_1(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "暂停")
            {
                mediaCtrl.Pause();
                button1.Text = "开始";
                button7.Enabled = true;
                open_video.Enabled = true;


            }
            else
            { 
            
            mediaCtrl.Run();
            button1.Text = "暂停";

            button7.Enabled = false;
            open_video.Enabled = false;
            }
            
            

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            mediaCtrl.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort.Write("U");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            serialPort.Write("D");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            serialPort.Write("L");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            serialPort.Write("R");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void checkBoxStop_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void tabini_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter_1(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = "123";
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter_1(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            getpos(10, trackBar10.Value, 200);
            LL4.Text = trackBar10.Value.ToString();
            
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            getpos(8, trackBar8.Value, 200);
            LL7.Text = trackBar8.Value.ToString();
            
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            getpos(6, trackBar3.Value, 200);
            LL9.Text = trackBar3.Value.ToString();
            
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            getpos(9, trackBar9.Value, 200);
            //serialPort.Write("I" + trackBar9.Value);
            LL11.Text = trackBar9.Value.ToString();
            

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            getpos(4, trackBar1.Value, 200);
            LL1.Text = trackBar1.Value.ToString();
            
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            getpos(1, trackBar4.Value, 200);
            LL2.Text = trackBar4.Value.ToString();
            
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            getpos(7, trackBar7.Value, 200);
            LL3.Text = trackBar7.Value.ToString();
            
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            getpos(5, trackBar2.Value, 200);
           
            LL5.Text = trackBar2.Value.ToString();
            
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            getpos(2, trackBar5.Value, 200);
            LL6.Text = trackBar5.Value.ToString();
            
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            getpos(11, trackBar11.Value, 200);
            LL8.Text = trackBar11.Value.ToString();
            
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            getpos(3, trackBar6.Value, 200);
            LL10.Text = trackBar6.Value.ToString();
            
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            getpos(12, trackBar12.Value, 200);
            LL12.Text = trackBar12.Value.ToString();
            
        }

        private void LL1_Click(object sender, EventArgs e)
        {

        }

        private void LL8_Click(object sender, EventArgs e)
        {

        }


    }
}
