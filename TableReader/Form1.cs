using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TableReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool InitDone = false;

        private string decode(byte[] data)
        {
            for(int i = 0; i< data.Length;i++)
            {
                data[i] ^= 0x11;
            }
            if(data.Length == 1)
                return Encoding.ASCII.GetString(data);


            byte[] res = new byte[data.Length];


            int indx = 0;
            for (int i = data.Length-1; i > -1 ; i--)
            {
                res[indx] = data[i];
                indx++;
            }
            string sres = Encoding.UTF8.GetString(res);
                return sres;
        }
        private int ReadCount(int num)
        {
            int result;
            switch (num)
            {
                case 0:
                    result = 0;
                    break;
                case 1:
                case 2:
                    result = 1;
                    break;
                case 3:
                case 4:
                    result = 2;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 12:
                    result = 4;
                    break;
                case 9:
                case 10:
                    result = 8;
                    break;
                default:
                    result = -1;
                    break;
            }

            return result;
        }
        tblFile myfile;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "GO Table Files|*.tbl";
            openFileDialog1.Title = "Select a Table File";
            InitDone = false;


            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                label1.Text = openFileDialog1.FileName;

                using (BinaryReader b = new BinaryReader(File.Open(label1.Text, FileMode.Open)))
                {
                    myfile = new tblFile();
                    char[] Head = new char[0x20];
                    b.Read(Head, 0, 0x20);
                    myfile.Header = Encoding.ASCII.GetBytes(Head);
                    //read data any server
                    if(Head[8] == 'G' && Head[9] == 'S' && Head[10] == 'P' || Head[8] == 'C' && Head[9] == 'N' || Head[8] == 'K' && Head[9] == 'R' || Head[8] == 'T' && Head[9] == 'W' || Head[8] == 'T' && Head[9] == 'H')
                    {
                        int TableHeaderCount = b.ReadInt32();
                        myfile.Header_Count = TableHeaderCount;

                        int[] HeaderType = new int[TableHeaderCount];
                        List<string> TableHeader = new List<string>();
                        for(int i=0;i < TableHeaderCount; i++)
                        {
                            HeaderType[i] = b.ReadInt32();
                            myfile.HeadresTypes.Add(HeaderType[i]);
                        }
                        int RowsCount = b.ReadInt32();
                        myfile.Rows_Count = RowsCount;
                        label2.Text = "Rows: " + RowsCount.ToString();
                        //Read Table Header
                        for (int i = 0; i < TableHeaderCount; i++)
                        {
                            int Size = b.ReadInt16();
                            myfile.HeadresNamesSize.Add(BitConverter.GetBytes((short)Size));
                            byte[] TH = b.ReadBytes(Size);
                            myfile.HeadresNames.Add(TH.ToArray());

                            string rrr = decode(TH);
                            TableHeader.Add(rrr);
                          
                            dataGridView1.Columns.Add(rrr, rrr + " "+ HeaderType[i].ToString() + " -> " + ReadCount(HeaderType[i]).ToString());
                        }
                        
                        for(int i=0;i< RowsCount; i++)
                        {
                            dataGridView1.Rows.Add();
                            for(int a =0;a< TableHeaderCount; a++)
                            {

                                //Read Rowas
                                int rc = ReadCount(HeaderType[a]);
                                if(HeaderType[a] == 7)
                                {
                                    int rcount = b.ReadInt16();
                                    byte[] strin = b.ReadBytes(rcount);
                                    myfile.RowValue.Add(strin.ToArray());

                                    string rrr = decode(strin);
                                    dataGridView1[a, i].Value = rrr;
                                }
                                else
                                {
                                    byte[] t = new byte[rc];
                                    t = b.ReadBytes(rc);
                                    myfile.RowValue.Add(t.ToArray());

                                    if (rc == 4)
                                    dataGridView1[a, i].Value = BitConverter.ToUInt32(t,0).ToString();

                                    if (rc == 2)
                                        dataGridView1[a, i].Value = BitConverter.ToInt16(t, 0).ToString();

                                    if (rc == 1)
                                        dataGridView1[a, i].Value = t[0].ToString();
                                }

                            }

                        }

                    }
                }
                InitDone = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            dataGridView1.Width = this.Width - 40;
            dataGridView1.Height = this.Height - 80;
            label2.Top = this.Height - 60;
            button1.Top = this.Height - 65;
            button1.Left = this.Width - 105;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sFileDialog1 = new SaveFileDialog();
            sFileDialog1.Filter = "GO Table Files|*.tbl";
            sFileDialog1.Title = "Select a Table File";



            if (sFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream writeStream;
                try
                {
                    writeStream = new FileStream(sFileDialog1.FileName, FileMode.Create);
                    BinaryWriter writeBinay = new BinaryWriter(writeStream);
                    writeBinay.Write(myfile.Header);
                    writeBinay.Write(myfile.Header_Count);
                    foreach ( int b in myfile.HeadresTypes)
                    {
                        writeBinay.Write(b);
                    }
                    writeBinay.Write(myfile.Rows_Count);
                    
                    foreach (byte[] b in myfile.HeadresNames)
                    {
                        writeBinay.Write((short)b.Length);
                        writeBinay.Write(b);
                    }
                    int tableIndex = 0;
                    foreach (byte[] b in myfile.RowValue)
                    {
                        if(myfile.HeadresTypes[tableIndex] == 7)
                        {
                            writeBinay.Write((short)b.Length);
                        }
                        
                        writeBinay.Write(b);
                        tableIndex++;
                        if (tableIndex == myfile.HeadresTypes.Count)
                            tableIndex = 0;
                    }
                        writeBinay.Close();
                    MessageBox.Show("Done");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (InitDone)
            {
                if(myfile.HeadresTypes[e.ColumnIndex] == 7)
                {
                    int Pos = e.ColumnIndex + (e.RowIndex * myfile.HeadresTypes.Count);
                    //Need Chane the Vale To Encoded
                    string val = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
                    if (val == "")
                        return;
                    byte[] data = Encoding.ASCII.GetBytes(val);
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] ^= 0x11;
                    }
                    myfile.RowValue[Pos] =  data.Reverse().ToArray();
                }
                
            }
            
        }

        private void Button3_Click(object sender, EventArgs e)
        {


            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text File|*.txt";
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            // setup for export
            dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridView1.SelectAll();
            // hiding row headers to avoid extra \t in exported text
            var rowHeaders = dataGridView1.RowHeadersVisible;
            dataGridView1.RowHeadersVisible = false;

            // ! creating text from grid values
            string content = dataGridView1.GetClipboardContent().GetText();

            // restoring grid state
            dataGridView1.ClearSelection();
            dataGridView1.RowHeadersVisible = rowHeaders;

            System.IO.File.WriteAllText(dialog.FileName, content);
            MessageBox.Show(@" Save your tbl file to .txt successfully.");

        }
    }
}
