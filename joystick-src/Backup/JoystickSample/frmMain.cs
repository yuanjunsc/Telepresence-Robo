using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JoystickSample
{
    public partial class frmMain : Form
    {
        private JoystickInterface.Joystick jst;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
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
            // get status
            jst.UpdateStatus();

            // update the axes positions
            foreach (Control ax in flpAxes.Controls)
            {
                if (ax is Axis)
                {
                    switch (((Axis)ax).AxisId)
                    {
                        case 1:
                            ((Axis)ax).AxisPos = jst.AxisA;
                            break;
                        case 2:
                            ((Axis)ax).AxisPos = jst.AxisB;
                            break;
                        case 3:
                            ((Axis)ax).AxisPos = jst.AxisC;
                            break;
                        case 4:
                            ((Axis)ax).AxisPos = jst.AxisD;
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
                }
            }
        }
    }
}