using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace JoystickSample
{
    
    public partial class Axis : UserControl
    {
        string data;


        public Axis()
        {
            InitializeComponent();
            
        }

        private int axisPos = 32767;

        public int AxisPos
        {
            set
            {
                

                lblAxisName.Text = "Axis: " + axisId + "  Value: " + value;          //value值即为摇杆数据
                tbAxisPos.Value = value;
                axisPos = value;

                


                data = value.ToString();
               
                

            }


            

        }

        private int axisId = 0;
        public int AxisId
        {
            set 
            {
                lblAxisName.Text = "Axis: " + value + "  Value: " + axisPos;
                axisId = value; 
            }
            get
            {
                return axisId;
            }
        }

        private void tbAxisPos_Scroll(object sender, EventArgs e)
        {
            

          
        }

        private void lblAxisName_Click(object sender, EventArgs e)
        {

        }
        }


    }

