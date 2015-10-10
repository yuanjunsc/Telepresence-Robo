using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace prp
{
    public partial class DeviceSelector : Form
    {
      

        /// <summary> Required designer variable. </summary>


        public DeviceSelector(ArrayList devs)
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            ListViewItem item = null;
            foreach (DsDevice d in devs)
            {
                item = new ListViewItem(d.Name);
                item.Tag = d;
                deviceListVw.Items.Add(item);
            }
        }

        private void DeviceSelector_Load(object sender, EventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (deviceListVw.SelectedItems.Count != 1)
                return;
            ListViewItem selitem = deviceListVw.SelectedItems[0];
            SelectedDevice = selitem.Tag as DsDevice;
            Close();
        }
        public DsDevice SelectedDevice;
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void deviceListVw_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
