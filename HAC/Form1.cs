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
        private string m_Symbol;
        private string m_InstrType;
        private string m_Expiry;
        List<Form2> m_KD;
        List<Form3> m_RSI;
        List<Form4> m_MFI;
        List<Form5> m_FI;

        public Form1()
        {
            InitializeComponent();
            m_KD = new List<Form2>();
            m_RSI = new List<Form3>();
            m_MFI = new List<Form4>();
            m_FI = new List<Form5>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_Symbol = textBox1.Text;
            m_InstrType = textBox2.Text;
            m_Expiry = textBox3.Text;

            switch (listBox1.SelectedIndex)
            {
                // Stochastic Oscillator
                case 0:
                    m_KD.Add(new Form2());
                    m_KD.Last().Text = m_Symbol;
                    m_KD.Last().Show();
                    break;
                
                // Relative Strength Index
                case 1:
                    m_RSI.Add(new Form3());
                    m_RSI.Last().Text = m_Symbol;
                    m_RSI.Last().Show();
                    break;
                
                // Money Flow Index
                case 2:
                    m_MFI.Add(new Form4());
                    m_MFI.Last().Text = m_Symbol;
                    m_MFI.Last().Show();
                    break;
                
                // Force Index
                case 3:
                    m_FI.Add(new Form5());
                    m_FI.Last().Text = m_Symbol;
                    m_FI.Last().Show();
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public string Symbol
        {
            get { return m_Symbol; }
            set { m_Symbol = value; }
        }

        public string InstrType
        {
            get { return m_InstrType; }
            set { m_InstrType = value; }
        }

        public string Expiry
        {
            get { return m_Expiry; }
            set { m_Expiry = value; }
        }
    }
}
