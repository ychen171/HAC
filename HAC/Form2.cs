using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HAC
{
    public partial class Form2 : Form
    {
        private SystemManager02 m_Manager;
        private Timer m_Timer;
        System.IO.StreamWriter price_file;

        public Form2()
        {
            InitializeComponent();

            price_file = new System.IO.StreamWriter("C:\\temp\\Data02.csv", true);

            m_Timer = new Timer();
            m_Timer.Interval = 20000;
            m_Timer.Tick += new EventHandler(m_Timer_Tick);
            m_Timer.Enabled = true;

            if (m_Manager == null)
            {
                m_Manager = new SystemManager02();
                m_Manager.OnSystemUpdate += new OnSystemUpdateEventHandler(OnSystemUpdate);

                m_Manager.Qty = Convert.ToDouble(numericUpDown1.Value);
                m_Manager.Ticks = Convert.ToInt32(numericUpDown2.Value);
                m_Manager.TargetTicks = Convert.ToInt32(numericUpDown4.Value);
                m_Manager.StopTicks = Convert.ToInt32(numericUpDown5.Value);
            }

            dataGridView1.DataSource = m_Manager.Matcher.BuyTable;
            dataGridView2.DataSource = m_Manager.Matcher.SellTable;
            dataGridView3.DataSource = m_Manager.Matcher.RoundTurns;
        }

        private void OnSystemUpdate(double price, double qty, double slow, double fast, double target, double stop)
        {
            // Event handler prints the data to the GUI.
            textBox1.Text = (price / 100.00).ToString();
            textBox2.Text = slow.ToString();
            textBox3.Text = fast.ToString();
            textBox4.Text = target.ToString();
            textBox5.Text = stop.ToString();
            textBox6.Text = qty.ToString();
            textBox7.Text = m_Manager.Position.ToString();
            textBox8.Text = m_Manager.NetPos.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.StartStop();
                if (button1.Text == "START")
                    button1.Text = "STOP";
                else
                    button1.Text = "START";
            }
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.ShutDown();

                m_Manager.Matcher.WriteBuys("C:\\temp\\Trade Matching Algos\\buys02.csv");
                m_Manager.Matcher.WriteSells("C:\\temp\\Trade Matching Algos\\sells02.csv");
                m_Manager.Matcher.WriteRoundTurns("C:\\temp\\Trade Matching Algos\\roundturns02.csv");

                m_Timer.Tick -= new EventHandler(m_Timer_Tick);
                m_Timer = null;

                m_Manager = null;
                GC.Collect();
                price_file.Close();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.Qty = Convert.ToDouble(numericUpDown1.Value);
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.Ticks = Convert.ToInt32(numericUpDown2.Value);
            }
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.TargetTicks = Convert.ToInt32(numericUpDown4.Value);
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.StopTicks = Convert.ToInt32(numericUpDown5.Value);
            }
        }

        private void m_Timer_Tick(Object o, EventArgs e)
        {
            price_file.WriteLine(((m_Manager.Bid + m_Manager.Ask) / 2.0).ToString());
            m_Timer.Interval = 5000;
            if (textBox1.Text != "" && Convert.ToDouble(textBox1.Text) != 0.0)
                chart1.Series[0].Points.AddXY(DateTime.Now.ToOADate(), textBox1.Text);
            chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chart1.Series[0].Points.FindMaxByValue().YValues[0]) + 0.01;
            chart1.ChartAreas[0].AxisY.Minimum = Math.Floor(chart1.Series[0].Points.FindMinByValue().YValues[0]) - 0.01;
            chart1.ChartAreas[0].AxisY.Interval = Math.Floor(((chart1.ChartAreas[0].AxisY.Maximum) - (chart1.ChartAreas[0].AxisY.Minimum)) / 5.0);

            double removeBefore = DateTime.Now.AddSeconds(-4900.0).ToOADate();

            while (chart1.Series[0].Points[0].XValue < removeBefore)
            {
                chart1.Series[0].Points.RemoveAt(0);
            }

            chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
            chart1.ChartAreas[0].AxisX.Maximum = DateTime.FromOADate(chart1.Series[0].Points[0].XValue).AddSeconds(5000).ToOADate();
            chart1.Invalidate();
        }
    }
}
