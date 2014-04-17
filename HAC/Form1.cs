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
    public partial class Form1 : Form
    {
        List<Form2> m_KD;
        List<Form3> m_RSI;

        public Form1()
        {
            InitializeComponent();
            m_KD = new List<Form2>();
            m_RSI = new List<Form3>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (listBox1.SelectedIndex)
            {
                // Stochastic Oscillator
                case 0:
                    m_KD.Add(new Form2());
                    m_KD.Last().Text = textBox1.Text;
                    m_KD.Last().Show();
                    break;
                
                // Relative Strength Index
                case 1:
                    m_RSI.Add(new Form3());
                    m_RSI.Last().Text = textBox1.Text;
                    m_RSI.Last().Show();
                    break;
                
                // Money Flow Index
                case 2:
                    break;
                
                // Force Index
                case 3:
                    break;
                
                // Donchian Channel
                case 4:
                    break;
                
                // Nope
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
