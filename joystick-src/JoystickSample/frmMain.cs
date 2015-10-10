using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using System.Linq;
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


namespace JoystickSample
{
    public partial class frmMain : Form
    {
        private JoystickInterface.Joystick jst;
        private byte[] sendByte = new byte[5] { 0xaa, 0, 0, 0, 0 };
        public int f=0;

        private bool firstActive;                  // 第一次激活窗口
        private ArrayList capDevices;              // 视频设备列表
        private string fileName;                   // 保存视频avi文件
        private prp.IBaseFilter capFilter;             // 视频设备的 filterinfo
        private prp.IGraphBuilder graphBuilder;        // graphBuilder接口
        private prp.ICaptureGraphBuilder2 capGraph;    // 捕捉Graph接口
        private prp.IVideoWindow videoWin;             // video window interface. 
        private prp.IMediaControl mediaCtrl;           // Media control interface
        private prp.IMediaEventEx mediaEvt;            // Media event interface

        private const int WM_GRAPHNOTIFY = 0x00008001;	// message from graph
        private const int WS_CHILD = 0x40000000;	// attributes for video window
        private const int WS_CLIPCHILDREN = 0x02000000;
        private const int WS_CLIPSIBLINGS = 0x04000000;


        string data;



        public frmMain() 
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            serialPort1.PortName = "COM7";
            serialPort1.BaudRate = 9600;
            serialPort1.Open();
            

            // grab the joystick
            jst = new JoystickInterface.Joystick(this.Handle);
            string[] sticks = jst.FindJoysticks();
            jst.AcquireJoystick(sticks[0]);

            // add the axis controls to the axis container
            for (int i = 0; i < jst.AxisCount; i++)
            {
                Axis ax = new Axis();
                ax.AxisId = i + 1;
                flpAxes.Controls.Add(ax);
            }

            // add the button controls to the button container
            for (int i = 0; i < jst.Buttons.Length; i++)
            {
                JoystickSample.Button btn = new Button();
                btn.ButtonId = i + 1;
                btn.ButtonStatus = jst.Buttons[i];
                flpButtons.Controls.Add(btn);
            }

            // start updating positions
            tmrUpdateStick.Enabled = true;
        }

        private void tmrUpdateStick_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString();
            
            // get status
            jst.UpdateStatus();
           
            //*********************************摇杆状态检测************************************//
            
            if (f==0&&jst.AxisC == 32511 && jst.AxisD == 32511 && jst.Buttons[6] == false && jst.Buttons[7] == false && jst.Buttons[4] == false && jst.Buttons[5] == false)
            {
                stop(); f = 1;
            }

            if (jst.AxisC == 65535 && jst.AxisD==32511)
            {
                //data = jst.AxisC.ToString();
                //serialPort1.Write(data);
                left(); f = 0;
            }

            if (jst.AxisC == 0 && jst.AxisD==32511)
            {

                right(); f = 0;
            }

            


            if (jst.AxisD == 65535)
            {
                //data = jst.AxisC.ToString();
                //serialPort1.Write(data);
                if (jst.Buttons[4] == true)
                {
                    accelerate_b(); f = 0;
                }

                else if (jst.Buttons[5] == true)
                {
                    shiftdown_b(); f = 0;
                }
                else if (jst.AxisC == 0)
                {
                    left_backward(); f = 0;
                
                }

                else if (jst.AxisC == 65535)
                {
                    right_backward(); f = 0;

                }

                else
                    backward(); f = 0;

                
            }


            else if (jst.AxisD == 0)
            {

                if (jst.Buttons[4] == true)
                {
                    accelerate_f(); f = 0;
                }

                else if (jst.Buttons[5] == true)
                {
                    shiftdown_f(); f = 0;
                }

                else if (jst.AxisC == 0)
                {

                    left_forward(); f = 0;
                }


                else if (jst.AxisC == 65535)
                {

                    right_forward(); f = 0;
                }


                else forward(); f = 0;

            }

            
            
//*********************************按键状态检测************************************//


            if (jst.Buttons[0] ==true)
            {
                button1(); f = 1;

            }


            if (jst.Buttons[1] == true)
            {
                button2(); f = 0;

            }


            if (jst.Buttons[2] == true)
            {
                button3(); f = 0;

            }


            if (jst.Buttons[3] == true)
            {
                button4(); f = 0;

            }

            if (jst.Buttons[6] == true)
            {
                button7(); f = 0;

            }

            if (jst.Buttons[7] == true)
            {
                button8(); f = 0;

            }

            if (jst.Buttons[9] == true)
            {
                flag_continue(); f = 0;

            }






            // update the axes positions
            foreach (Control ax in flpAxes.Controls)
            {
                if (ax is Axis)
                {
                    switch (((Axis)ax).AxisId)                        //对应六个滑动条
                    {
                        case 1:
                            ((Axis)ax).AxisPos = jst.AxisA;
                            break;
                        case 2:
                            ((Axis)ax).AxisPos = jst.AxisB;
                            break;
                        case 3:
                            ((Axis)ax).AxisPos = jst.AxisC;          //只使用3号和4号滑动条
                       

                               /* 
                            if (jst.AxisC==65535)
                            {
                                //data = jst.AxisC.ToString();
                                //serialPort1.Write(data);
                                right();
                            }

                            else if (jst.AxisC==0)
                            {

                                left();
                            }

                          
                            */


                            break;
                    
                        case 4:
                            ((Axis)ax).AxisPos = jst.AxisD;
                            
                            /*
                            if (jst.AxisD ==65535)
                            {
                                //data = jst.AxisC.ToString();
                                //serialPort1.Write(data);
                                backward();
                            }

                            else if (jst.AxisD==0)
                            {

                                forward();
                            }                        
                            */
                            break;
                             

                        case 5:
                            ((Axis)ax).AxisPos = jst.AxisE;
                            break;
                        case 6:
                            ((Axis)ax).AxisPos = jst.AxisF;
                            break;

                    }
                }
            }



            // update each button status
            foreach (Control btn in flpButtons.Controls)
            {
                if (btn is JoystickSample.Button)
                {
                    ((JoystickSample.Button)btn).ButtonStatus =
                        jst.Buttons[((JoystickSample.Button)btn).ButtonId - 1];

                    if (((JoystickSample.Button)btn).ButtonStatus == true)
                    {
                        /*
                        if (((JoystickSample.Button)btn).ButtonId == 1)
                        {
                            button1();

                        }


                        if (((JoystickSample.Button)btn).ButtonId == 2)
                        {
                            button2();

                        }


                        if (((JoystickSample.Button)btn).ButtonId == 3)
                        {
                            button3();

                        }


                        if (((JoystickSample.Button)btn).ButtonId == 4)
                        {
                            button4();

                        }

                        if (((JoystickSample.Button)btn).ButtonId == 7)
                        {
                            button7();

                        }

                        if (((JoystickSample.Button)btn).ButtonId == 8)
                        {
                            button8();

                        }

                        */

                    }

                   


                }
            }

            
                
        }


        private void sender(byte data)
        {
            sendByte[1] = (byte)(data);
            try
            {
                serialPort1.Write(sendByte, 0, 2);
            }
            catch (Exception err)
            {
                //MessageBox.Show(err.Message);
            }
        }

        private void forward()
        {
            sender(0xA1);
        }

        private void backward()
        {
            sender(0xA2);
        }

        private void left()
        {
            sender(0xA3);
        }

        private void right()
        {
            sender(0xA4);
        }

        private void right_forward()
        {
            sender(0xA5);
        }

        private void left_forward()
        {
            sender(0xA6);
        }

        private void right_backward()
        {
            sender(0xA7);
        }


        private void left_backward()
        {
            sender(0xA8);
        }




        private void button1()
        {
            sender(0xB1);
        }
        private void button2()
        {
            sender(0xB2);
        }

        private void button3()
        {
            sender(0xB3);
        }

        private void button4()
        {
            sender(0xB4);
        }

        private void button7()
        {
            sender(0xB7);
        }

        private void button8()
        {
            sender(0xB8);
        }

        private void stop()
        {
            sender(0xB9);
        }


        private void accelerate_f()
        {
            sender(0xBA);
        
        }

        private void shiftdown_f()
        {
            sender(0xBB);

        }

        private void accelerate_b()
        {
            sender(0xBC);

        }

        private void shiftdown_b()
        {
            sender(0xBD);

        }

        private void flag_continue()
        {
            sender(0xBE);

        }

        private void manual_model()
        {
            sender(0xC1);

        }

        private void stance_model()
        {
            sender(0xC2);

        }

        private void flag_send()
        {
            sender(0xC3);

        }

        private void test_model()
        {
            sender(0xC4);

        }

        private void general_model()
        {
            sender(0xC5);

        }



        private void openSP(string baud, string num)
        {
            if (mySP.IsOpen)
            {
                mySP.Close();
                return;
            }
            mySP.BaudRate = System.Convert.ToInt32(baud);
            mySP.PortName = "COM" + num;
            #region 打开串口，如果出现错误显示
            try
            {
                mySP.Open();
            }
            catch (Exception excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            #endregion
            if (mySP.IsOpen)
            {
                openSPBtn.Text = "关闭串口";
            }
            else
            {
                openSPBtn.Text = "打开串口";
            }
        }


        private void flpAxes_Paint(object sender, PaintEventArgs e)
        {

        }
     

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void flpButtons_Paint(object sender, PaintEventArgs e)
        {

        }

        private void openCameraBtn_Click(object sender, EventArgs e)
        {

        }

        private void openSPBtn_Click(object sender, EventArgs e)
        {
            
        }

        private void openSPBtn_Click_1(object sender, EventArgs e)
        {
            openSP(baudTextBox.Text, comTextBox.Text);   //打开串口
        }

        private void flpAxes_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            LL1.Text = trackBar1.Value.ToString();
            
            string p3=(trackBar1.Value).ToString();
            string c3 = "#2P" + p3 + "T1000"+"\r";
            serialPort1.Write(c3);

        }

        private void openCameraBtn_Click_1(object sender, EventArgs e)
        {
            frmCapture_Activated();
        }


        private void frmCapture_Activated() //private void frmCapture_Activated(object sender, EventArgs e)
        {
            if (firstActive) return;
            firstActive = true;

            if (!prp.DsUtils.IsCorrectDirectXVersion())
            {
                MessageBox.Show(this, "DirectX 8.1 NOT installed!", "DirectShow.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                this.Close(); return;
            }

            if (!prp.DsDev.GetDevicesOfCat(prp.FilterCategory.VideoInputDevice, out capDevices))
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

            prp.DsDevice dev = null;
            if (capDevices.Count == 1)
                dev = capDevices[0] as prp.DsDevice;
            else
            {
                prp.DeviceSelector selector = new prp.DeviceSelector(capDevices);
                selector.ShowDialog(this);
                dev = selector.SelectedDevice;
            }

            if (dev == null)
            {
                this.Close(); return;
            }

            if (!StartupVideo(dev.Mon))
                this.Close();
            this.Text = "虚拟现实机器人-北京交通大学";
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
                Guid gbf = typeof(prp.IBaseFilter).GUID;
                mon.BindToObject(null, null, ref gbf, out capObj);
                capFilter = (prp.IBaseFilter)capObj; capObj = null;
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
                comType = Type.GetTypeFromCLSID(prp.Clsid.FilterGraph);
                if (comType == null)
                    throw new NotImplementedException(@"DirectShow FilterGraph not installed/registered!");
                comObj = Activator.CreateInstance(comType);
                graphBuilder = (prp.IGraphBuilder)comObj; comObj = null;

                //Guid clsid = Clsid.CaptureGraphBuilder2;
                //Guid riid = typeof(ICaptureGraphBuilder2).GUID;
                //comObj = DsBugWO.CreateDsInstance(ref clsid, ref riid);
                //capGraph = (ICaptureGraphBuilder2)comObj; comObj = null;

                comType = Type.GetTypeFromCLSID(prp.Clsid.CaptureGraphBuilder2);
                comObj = Activator.CreateInstance(comType);
                capGraph = (prp.ICaptureGraphBuilder2)comObj; comObj = null;

                mediaCtrl = (prp.IMediaControl)graphBuilder;
                videoWin = (prp.IVideoWindow)graphBuilder;
                mediaEvt = (prp.IMediaEventEx)graphBuilder;
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
            prp.IBaseFilter mux = null;
            prp.IFileSinkFilter sink = null;

            try
            {
                hr = capGraph.SetFiltergraph(graphBuilder);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                hr = graphBuilder.AddFilter(capFilter, "Ds.NET Video Capture Device");
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                prp.DsUtils.ShowCapPinDialog(capGraph, capFilter, this.Handle);

                Guid sub = prp.MediaSubType.Avi;
                hr = capGraph.SetOutputFileName(ref sub, fileName, out mux, out sink);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                Guid cat = prp.PinCategory.Capture;
                Guid med = prp.MediaType.Video;
                hr = capGraph.RenderStream(ref cat, ref med, capFilter, null, mux); // stream to file 
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                cat = prp.PinCategory.Preview;
                med = prp.MediaType.Video;
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
                hr = videoWin.SetWindowPosition(panel1.Left, panel1.Top, panel1.Width, panel1.Height);
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
                hr = videoWin.put_Visible(prp.DsHlp.OATRUE);
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

        private void button9_Click(object sender, EventArgs e)
        {
            if (button9.Text == "暂停")
            {
                mediaCtrl.Pause();
                button9.Text = "开始";
                button10.Enabled = true;
                open_video.Enabled = true;


            }
            else
            {

                mediaCtrl.Run();
                button9.Text = "暂停";

                button10.Enabled = false;
                open_video.Enabled = false;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            mediaCtrl.Stop();
        }

        private void upBtn_Click(object sender, EventArgs e)
        {
            forward();
        }

        private void downBtn_Click(object sender, EventArgs e)
        {
            backward();
        }

        private void leftBtn_Click(object sender, EventArgs e)
        {
            left();
        }

        private void rightBtn_Click(object sender, EventArgs e)
        {
            right();
        }

        private void autoBtn_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            button7();
        }

        private void releaseBtn_Click(object sender, EventArgs e)
        {
            button8();
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            label17.Text = trackBar6.Value.ToString();
            string p1 = (trackBar6.Value).ToString();
            string c1 = "#0P" + p1 + "T1000" + "\r";
            serialPort1.Write(c1);
        }

        private void LL1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
           LL5.Text = trackBar2.Value.ToString();
           string p4 = (trackBar2.Value).ToString();
           string c4 = "#3P" + p4 + "T1000" + "\r";
           serialPort1.Write(c4);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            LL9.Text = trackBar3.Value.ToString();
            string p5 = (trackBar3.Value).ToString();
            string c5= "#4P" + p5 + "T1000" + "\r";
            serialPort1.Write(c5);
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            label13.Text = trackBar4.Value.ToString();
            string p6 = (trackBar4.Value).ToString();
            string c6 = "#5P" + p6 + "T1000" + "\r";
            serialPort1.Write(c6);
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            label25.Text = trackBar10.Value.ToString();
            string p7 = (trackBar10.Value).ToString();
            string c7 = "#6P" + p7 + "T1000" + "\r";
            serialPort1.Write(c7);
        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            label27.Text = trackBar11.Value.ToString();
            string p8 = (trackBar11.Value).ToString();
            string c8 = "#7P" + p8 + "T1000" + "\r";
            serialPort1.Write(c8);
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            label29.Text = trackBar12.Value.ToString();
            string p9 = (trackBar12.Value).ToString();
            string c9 = "#8P" + p9 + "T1000" + "\r";
            serialPort1.Write(c9);
        }

        private void trackBar16_Scroll(object sender, EventArgs e)
        {
            label37.Text = trackBar16.Value.ToString();
            string p10 = (trackBar16.Value).ToString();
            string c10 = "#9P" + p10 + "T1000" + "\r";
            serialPort1.Write(c10);
        }

        private void trackBar15_Scroll(object sender, EventArgs e)
        {
            label36.Text = trackBar15.Value.ToString();
            string p11 = (trackBar15.Value).ToString();
            string c11 = "#10P" + p11 + "T1000" + "\r";
            serialPort1.Write(c11);
        }

        private void trackBar14_Scroll(object sender, EventArgs e)
        {
            label35.Text = trackBar14.Value.ToString();
            string p12 = (trackBar14.Value).ToString();
            string c12 = "#11P" + p12 + "T1000" + "\r";
            serialPort1.Write(c12);
        }

        private void trackBar13_Scroll(object sender, EventArgs e)
        {
            label33.Text = trackBar13.Value.ToString();
            string p13 = (trackBar13.Value).ToString();
            string c13 = "#12P" + p13 + "T1000" + "\r";
            serialPort1.Write(c13);
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            label31.Text = trackBar9.Value.ToString();
            string p14 = (trackBar9.Value).ToString();
            string c14 = "#13P" + p14 + "T1000" + "\r";
            serialPort1.Write(c14);
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            label18.Text = trackBar8.Value.ToString();
            string p15 = (trackBar8.Value).ToString();
            string c15 = "#14P" + p15 + "T1000" + "\r";
            serialPort1.Write(c15);
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            label11.Text = trackBar7.Value.ToString();
            string p16 = (trackBar7.Value).ToString();
            string c16 = "#15P" + p16 + "T1000" + "\r";
            serialPort1.Write(c16);
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            label15.Text = trackBar5.Value.ToString();
            string p2 = (trackBar5.Value).ToString();
            string c2 = "#1P" + p2 + "T1000" + "\r";
            serialPort1.Write(c2);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            flag_send();
            richTextBox2.Text = serialPort1.ReadExisting();            
            //richTextBox2.Text += serialPort1.ReadLine();

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            //实例化一个保存文件对话框  

            SaveFileDialog sf = new SaveFileDialog();

            //设置文件保存类型  

            sf.Filter = "txt文件|*.txt|所有文件|*.*";

            //如果用户没有输入扩展名，自动追加后缀  

            sf.AddExtension = true;

            //设置标题  

            sf.Title = "写文件";

            //如果用户点击了保存按钮  

            if (sf.ShowDialog() == DialogResult.OK)
            {
                //实例化一个文件流--->与写入文件相关联  

                FileStream fs = new FileStream(sf.FileName, FileMode.Create);

                //实例化一个StreamWriter-->与fs相关联  

                StreamWriter sw = new StreamWriter(fs);

                //开始写入  

                sw.Write(this.richTextBox2.Text);

                //清空缓冲区  

                sw.Flush();

                //关闭流  

                sw.Close();

                fs.Close();
            } 
        }

        private void button6_Click(object sender, EventArgs e)
        {
            manual_model();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            stance_model();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            test_model();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            general_model();
        }

        private void 说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("      本程序为虚拟现实机器人上位机控制软件，具有两种控制模式和进行机器人调试的功能:\n\r(1)运行程序后首先打开无线摄像头，从而可以进行临场感调试.\n\r(2)选择控制方式为姿态控制或手柄控制:\n\r姿态控制：即使用者通过穿戴本项目的姿态检测系统进行姿态控制.\n\r手柄控制：通过接入上位机的手柄进行控制.\n\r(3)如运行时存在问题，可在机器人调试中，调试本机器人的舵机、直流电机等所有执行机构.", "使用说明");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("      项目的创造性：临场感机器人的概念虽然已经提出多年，但是由于其开发难度巨大且成本极高所以在我国并没有得到很好的发展。而在本项目中，使用常见的STM32作为机器人的主控板，并通过自制的姿态检测系统、传输系统和机器人主体实现了临场感操作的功能，大大降低了临场感技术的开发成本，提高了临场感机器人的可推广性.\n\r", "作品介绍");
        }

        private void panel1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

      




    }
}