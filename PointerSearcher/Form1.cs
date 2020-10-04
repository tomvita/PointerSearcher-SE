using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Data;

namespace PointerSearcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            int maxDepth = 4;
            int maxOffsetNum = 1;
            long maxOffsetAddress = 0x800;
            textBoxDepth.Text = maxDepth.ToString();
            textBoxOffsetNum.Text = maxOffsetNum.ToString();
            textBoxOffsetAddress.Text = maxOffsetAddress.ToString("X");
            buttonSearch.Enabled = false;
            buttonNarrowDown.Enabled = false;
            buttonCancel.Enabled = false;
            progressBar1.Maximum = 100;

            result = new List<List<IReverseOrderPath>>();
        }
        Socket s;
        private PointerInfo info;
        private int maxDepth;
        private int targetselect = 0;
        private int fileselect = 0;
        private bool user_abort = false;
        private bool user_abort2 = false;
        private bool attached = false;
        private bool command_inprogress = false;
        private int maxOffsetNum;
        private long maxOffsetAddress;
        private List<List<IReverseOrderPath>> result;
        private CancellationTokenSource cancel = null;
        private double progressTotal;

        public BinaryReader FileStream { get; private set; }

        private async void buttonRead_Click(object sender, EventArgs e)
        {
            SetProgressBar(0);
            try
            {
                buttonRead.Enabled = false;


                IDumpDataReader reader = CreateDumpDataReader(dataGridView1.Rows[0], false);
                if (reader == null)
                {
                    throw new Exception("Invalid input" + Environment.NewLine + "Check highlighted cell");
                }
                //reader.readsetup(); // not reading again so the change won't be overwritten by what is in the file
                dataGridView1.Rows[0].Cells[1].Value = "0x" + Convert.ToString(reader.mainStartAddress(), 16);
                dataGridView1.Rows[0].Cells[2].Value = "0x" + Convert.ToString(reader.mainEndAddress(), 16);
                dataGridView1.Rows[0].Cells[3].Value = "0x" + Convert.ToString(reader.heapStartAddress(), 16);
                dataGridView1.Rows[0].Cells[4].Value = "0x" + Convert.ToString(reader.heapEndAddress(), 16);
                //              dataGridView1.Rows[0].Cells[5].Value = "0x" + Convert.ToString(reader.TargetAddress(), 16);
                buttonSearch.Enabled = false;
                buttonNarrowDown.Enabled = false;
                buttonCancel.Enabled = true;

                cancel = new CancellationTokenSource();
                Progress<int> prog = new Progress<int>(SetProgressBar);

                info = await Task.Run(() => reader.Read(cancel.Token, prog));

                SetProgressBar(100);
                System.Media.SystemSounds.Asterisk.Play();

                buttonSearch.Enabled = true;
            }
            catch (System.OperationCanceledException)
            {
                SetProgressBar(0);
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                SetProgressBar(0);
                MessageBox.Show("Read Failed" + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cancel != null)
                {
                    cancel.Dispose();
                }

                buttonCancel.Enabled = false;
                buttonRead.Enabled = true;
            }
        }
        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            result.Clear();
            textBox1.Text = "";
            buttonRead.Enabled = false;
            //buttonSearch.Enabled = false;
            buttonNarrowDown.Enabled = true;
            textBox2.Text = dataGridView1.Rows[fileselect].Cells[0].Value.ToString();
            textBox2.Text = textBox2.Text.Remove(textBox2.Text.Length - 4, 4) + "bmk";
            SetProgressBar(0);
            if (dataGridView1.Rows[fileselect].Cells[5 + targetselect].Value == null)
            { MessageBox.Show("Target not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            try
            {
                maxDepth = Convert.ToInt32(textBoxDepth.Text);
                maxOffsetNum = Convert.ToInt32(textBoxOffsetNum.Text);
                maxOffsetAddress = Convert.ToInt32(textBoxOffsetAddress.Text, 16);
                long heapStart = Convert.ToInt64(dataGridView1.Rows[fileselect].Cells[3].Value.ToString(), 16);
                long targetAddress = Convert.ToInt64(dataGridView1.Rows[fileselect].Cells[5 + targetselect].Value.ToString(), 16);
                Address address = new Address(MemoryType.HEAP, targetAddress - heapStart);

                if (maxOffsetNum <= 0)
                {
                    MessageBox.Show("Offset Num must be greater than 0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (maxOffsetAddress < 0)
                {
                    MessageBox.Show("Offset Range must be greater or equal to 0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    buttonCancel.Enabled = true;

                    cancel = new CancellationTokenSource();
                    Progress<double> prog = new Progress<double>(AddProgressBar);

                    FindPath find = new FindPath(maxOffsetNum, maxOffsetAddress);

                    await Task.Run(() =>
                    {
                        find.Search(cancel.Token, prog, 100.0, info, maxDepth, new List<IReverseOrderPath>(), address, result);
                    });

                    SetProgressBar(100);
                    PrintPath();
                    System.Media.SystemSounds.Asterisk.Play();

                    buttonNarrowDown.Enabled = true;
                }
            }
            catch (System.OperationCanceledException)
            {
                SetProgressBar(0);
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                SetProgressBar(0);
                MessageBox.Show("Read Failed" + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonCancel.Enabled = false;
                cancel.Dispose();
            }

            buttonRead.Enabled = true;
            buttonSearch.Enabled = true;
        }
        private void PrintPath()
        {
            textBox1.Text = "";
            if (result.Count > 1000)
            {
                string str;
                str = result.Count.ToString();
                textBox1.Text = str + " results" + Environment.NewLine;
            }
            else if (result.Count > 0)
            {
                foreach (List<IReverseOrderPath> path in result)
                {
                    String str = "main";
                    for (int i = path.Count - 1; i >= 0; i--)
                    {
                        str = path[i].ToString(str);
                    }
                    textBox1.Text += str + Environment.NewLine;
                }
            }
            else
            {
                textBox1.Text = "not found";
            }
        }

        private void ExportPath()
        {

            if (result.Count > 0)
            {
                textBox1.Text = "Exporting result to file ... " + result.Count.ToString();
                String filepath = textBox2.Text;
                //if ((filepath == "") || System.IO.File.Exists(filepath))
                //{
                //    textBox1.Text = "Book Mark File exist";
                //    return;
                //}
                BinaryWriter BM;
                try
                {
                    BM = new BinaryWriter(new FileStream(filepath, FileMode.Create, FileAccess.Write));
                    BM.BaseStream.Seek(0, SeekOrigin.Begin);
                    int magic = 0x4E5A4445;
                    BM.Write(magic);
                    long fileindex = 0;
                    long depth = 0;
                    long[] chain = new long[13];

                    foreach (List<IReverseOrderPath> path in result)
                    {

                        BM.BaseStream.Seek(134 + fileindex * 8 * 14, SeekOrigin.Begin); // sizeof(pointer_chain_t)  Edizon header size = 134
                        depth = 0;
                        for (int i = path.Count - 1; i >= 0; i--)
                        {
                            if (path[i] is ReverseOrderPathOffset)
                            {
                                chain[depth] = (path[i] as ReverseOrderPathOffset).getOffset();
                            }
                            else
                            {
                                depth++;
                                chain[depth] = 0;
                            }
                        }
                        BM.Write(depth);
                        for (long z = depth; z >= 0; z--)
                            BM.Write(chain[z]);
                        fileindex++;
                    };
                    for (long z = depth + 1; z < 13; z++)
                        BM.Write(chain[z]);
                    BM.BaseStream.Seek(5, SeekOrigin.Begin);
                    BM.Write(result.Count * 8 * 14);
                    BM.BaseStream.Close();
                }
                catch (IOException) { textBox1.Text = "Cannot create file"; }
            }
            else
            {
                textBox1.Text = "not found";
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"";
                ofd.Filter = "EdizonSE DumpFile(*.dmp*)|*.dmp*|All Files(*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.Title = "select EdiZon SE dump file";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ofd.FileName;
                }
                IDumpDataReader reader = CreateDumpDataReader(dataGridView1.Rows[e.RowIndex], true);
                if (reader != null)
                {
                    reader.readsetup();
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = "0x" + Convert.ToString(reader.mainStartAddress(), 16);
                    dataGridView1.Rows[e.RowIndex].Cells[2].Value = "0x" + Convert.ToString(reader.mainEndAddress(), 16);
                    dataGridView1.Rows[e.RowIndex].Cells[3].Value = "0x" + Convert.ToString(reader.heapStartAddress(), 16);
                    dataGridView1.Rows[e.RowIndex].Cells[4].Value = "0x" + Convert.ToString(reader.heapEndAddress(), 16);
                    dataGridView1.Rows[e.RowIndex].Cells[5].Value = "0x" + Convert.ToString(reader.TargetAddress(), 16);
                    // BM1

                }
            }
        }

        private async void buttonNarrowDown_Click(object sender, EventArgs e)
        {

            try
            {
                SetProgressBar(0);
                Dictionary<IDumpDataReader, long> dumps = new Dictionary<IDumpDataReader, long>();
                for (int i = 1; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];
                    ClearRowBackColor(row);
                    if (IsBlankRow(row))
                    {
                        continue;
                    }
                    IDumpDataReader reader = CreateDumpDataReader(row, true);
                    if (reader != null)
                    {
                        reader.readsetup();
                        dataGridView1.Rows[i].Cells[1].Value = "0x" + Convert.ToString(reader.mainStartAddress(), 16);
                        dataGridView1.Rows[i].Cells[2].Value = "0x" + Convert.ToString(reader.mainEndAddress(), 16);
                        dataGridView1.Rows[i].Cells[3].Value = "0x" + Convert.ToString(reader.heapStartAddress(), 16);
                        dataGridView1.Rows[i].Cells[4].Value = "0x" + Convert.ToString(reader.heapEndAddress(), 16);
                        //                     dataGridView1.Rows[i].Cells[5].Value = "0x" + Convert.ToString(reader.TargetAddress(), 16);
                        long target = Convert.ToInt64(row.Cells[5 + targetselect].Value.ToString(), 16);

                        dumps.Add(reader, target);
                    }
                }
                if (dumps.Count == 0)
                {
                    throw new Exception("Fill out 2nd line to narrow down");
                }
                buttonRead.Enabled = false;
                buttonSearch.Enabled = false;
                buttonNarrowDown.Enabled = false;
                buttonCancel.Enabled = true;

                cancel = new CancellationTokenSource();
                Progress<int> prog = new Progress<int>(SetProgressBar);

                List<List<IReverseOrderPath>> copyList = new List<List<IReverseOrderPath>>(result);

                result = await Task.Run(() => FindPath.NarrowDown(cancel.Token, prog, result, dumps));

                SetProgressBar(100);
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (System.OperationCanceledException)
            {
                SetProgressBar(0);
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                SetProgressBar(0);
                MessageBox.Show(Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (cancel != null)
                {
                    cancel.Dispose();
                }
                PrintPath();

                buttonRead.Enabled = true;
                buttonSearch.Enabled = true;
                buttonNarrowDown.Enabled = true;
                buttonCancel.Enabled = false;
            }
        }
        private bool IsBlankRow(DataGridViewRow row)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (row.Cells[i].Value == null)
                {
                    continue;
                }
                if (row.Cells[i].Value.ToString() != "")
                {
                    return false;
                }
            }
            return true;
        }
        private void ClearRowBackColor(DataGridViewRow row)
        {
            for (int i = 0; i <= 5; i++)
            {
                row.Cells[i].Style.BackColor = Color.White;
            }
        }
        private IDumpDataReader CreateDumpDataReader(DataGridViewRow row, bool allowUnknownTarget)
        {
            bool canCreate = true;
            String path = "";
            long mainStart = -1;
            long mainEnd = -1;
            long heapStart = -1;
            long heapEnd = -1;
            long target = -1;

            if (row.Cells[0].Value != null)
            {
                path = row.Cells[0].Value.ToString();
            }
            if ((path == "") || !System.IO.File.Exists(path))
            {
                row.Cells[0].Style.BackColor = Color.Red;
                canCreate = false;
                return null;
            }
            if (row.Cells[1].Value == null) 
                return new NoexsDumpDataReader(path, mainStart, mainEnd, heapStart, heapEnd);

            try
            {
                mainStart = Convert.ToInt64(row.Cells[1].Value.ToString(), 16);
            }
            catch
            {
                row.Cells[1].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                mainEnd = Convert.ToInt64(row.Cells[2].Value.ToString(), 16);
            }
            catch
            {
                row.Cells[2].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                heapStart = Convert.ToInt64(row.Cells[3].Value.ToString(), 16);
            }
            catch
            {
                row.Cells[3].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                heapEnd = Convert.ToInt64(row.Cells[4].Value.ToString(), 16);
            }
            catch
            {
                row.Cells[4].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                target = Convert.ToInt64(row.Cells[5].Value.ToString(), 16);
            }
            catch
            {
                row.Cells[5].Style.BackColor = Color.Red;
                canCreate = false;
            }
            if (!canCreate)
            {
                return null;
            }
            //if (mainEnd <= mainStart)
            //{
            //    row.Cells[1].Style.BackColor = Color.Red;
            //    row.Cells[2].Style.BackColor = Color.Red;
            //    canCreate = false;
            //}
            //if (heapEnd <= heapStart)
            //{
            //    row.Cells[3].Style.BackColor = Color.Red;
            //    row.Cells[4].Style.BackColor = Color.Red;
            //    canCreate = false;
            //}
            //if (allowUnknownTarget && (target == 0))
            //{
            //    //if target address is set to 0,it means unknown address.
            //}
            //else if ((target < heapStart) || (heapEnd <= target))
            //{
            //    //if not unknown,target should be located at heap region
            //    row.Cells[5].Style.BackColor = Color.Red;
            //    canCreate = false;
            //}
            if (!canCreate)
            {
                return null;
            }
            return new NoexsDumpDataReader(path, mainStart, mainEnd, heapStart, heapEnd);
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.White;
            //  dataGridView1.BeginEdit(true);
        }
        private void SetProgressBar(int percent)
        {
            progressBar1.Value = percent;
            progressTotal = percent;
        }
        private void AddProgressBar(double percent)
        {
            progressTotal += percent;
            if (progressTotal > 100)
            {
                progressTotal = 100;
            }
            progressBar1.Value = (int)progressTotal;
        }

        private void buttonCancel_Click_1(object sender, EventArgs e)
        {
            if (cancel != null)
            {
                cancel.Cancel();
            }
        }

        private void textBoxDepth_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.BringToFront();
            //this.tabControl1.SelectedIndex = 1;
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Close();
            ipBox.Text = ConfigurationManager.AppSettings["ipAddress"];
            pictureBox2.BringToFront();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //byte[] msg = { 0x1D }; //_dmnt_resume
            //int a = s.Send(msg);
        }


        private void Export_to_SE_Click(object sender, EventArgs e)
        {
            ExportPath();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintPath();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            targetselect = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            targetselect = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            targetselect = 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (connectBtn.Text == "Disconnect")
            {
                button2_Click_1(sender, e);
                s.Close();
                connectBtn.Text = "Connect";
                ipBox.BackColor = System.Drawing.Color.White;
                ipBox.ReadOnly = false;
                attached = false;
                attachbutton1.BackColor = System.Drawing.Color.White;
                attachbutton2.BackColor = System.Drawing.Color.White;
                command_inprogress = false;
                return;
            }
            string ipPattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";
            if (!Regex.IsMatch(ipBox.Text, ipPattern))
            {
                ipBox.BackColor = System.Drawing.Color.Red;
                return;
            }
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipBox.Text), 7331);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            if (config.AppSettings.Settings["ipAddress"] == null) config.AppSettings.Settings.Add("ipAddress", ipBox.Text);
            else
                config.AppSettings.Settings["ipAddress"].Value = ipBox.Text;
            config.Save(ConfigurationSaveMode.Minimal);
            if (s.Connected == false)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    IAsyncResult result = s.BeginConnect(ep, null, null);
                    bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);
                    if (conSuceded == true)
                    {
                        try
                        {
                            s.EndConnect(result);
                        }
                        catch
                        {
                            this.ipBox.Invoke((MethodInvoker)delegate
                            {
                                ipBox.BackColor = System.Drawing.Color.Red;
                                ipBox.ReadOnly = false;
                            });
                            return;
                        }

                        this.connectBtn.Invoke((MethodInvoker)delegate
                        {
                            this.connectBtn.Text = "Disconnect";
                        });
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {
                            ipBox.BackColor = System.Drawing.Color.LightGreen;
                            ipBox.ReadOnly = true;
                            //this.refreshBtn.Visible = true;
                            //this.Player1Btn.Visible = true;
                            //this.Player2Btn.Visible = true;
                        });

                    }
                    else
                    {
                        s.Close();
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {
                            ipBox.BackColor = System.Drawing.Color.Red;
                        });
                        MessageBox.Show("Could not connect to the SE tools server"); //, Go to https://github.com/ for help."
                    }
                }).Start();
            }
        }
        private bool command_available()
        {
            if (!s.Connected)
            {
                MessageBox.Show("Not connected");
                return false;
            }
            if (command_inprogress)
            {
                MessageBox.Show("command_inprogress");
                return false;
            }
            command_inprogress = true;
            errorBox.Text = "";
            return true;
        }
        private bool is_attached()
        {
            if (!attached)
            {
                MessageBox.Show("not attached");
                return false;
            }
            return true;
        }
        private bool showerror(byte [] b)
        {
            errorBox.Text = Convert.ToString(b[0]) + " . " + Convert.ToString(b[1]) + " . " + Convert.ToString(b[2]) + " . " + Convert.ToString(b[3]);
            if (b[0]==15 && b[1] == 8)
            {
                errorBox.Text = errorBox.Text + "  pminfo not valid";
            }
            if (b[0] == 93 && b[1] == 21)
            {
                errorBox.Text = errorBox.Text + "  already attached";
            }
            if (b[0] == 93 && b[1] == 19)
            {
                errorBox.Text = errorBox.Text + "  invalid cmd";
            }
            if (b[0] == 93 && b[1] == 33)
            {
                errorBox.Text = errorBox.Text + "  user abort";
            }
            if (b[0] == 93 && b[1] == 35)
            {
                errorBox.Text = errorBox.Text + "  file not accessible";
            }
            user_abort = false;
            user_abort2 = false;
            command_inprogress = false;
            int e = BitConverter.ToInt32(b,0);
            return e != 0;
        }

        private void getstatus_Click(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x10 }; // _list_pids
            int a = s.Send(msg);
            byte[] k = new byte[4];
            int c = s.Receive(k);
            int count = BitConverter.ToInt32(k, 0);
            byte[] b = new byte[count * 8];
            int d = s.Receive(b);
            long pid = BitConverter.ToInt64(b, (count-2) *8);
            long pid0 = BitConverter.ToInt64(b, (count - 1) * 8);
            int f = s.Available;
            c = s.Receive(k);

            pidBox.Text = Convert.ToString(pid);
            pid0Box.Text = Convert.ToString(pid0);

            msg[0] = 0x01; // _status
            a = s.Send(msg);
            b = new byte[4];
            while (s.Available < 4) ;
            c = s.Receive(b);
            count = BitConverter.ToInt32(k, 0);
            statusBox.Text = Convert.ToString(b[0]) + " . " + Convert.ToString(b[1]) + " . " + Convert.ToString(b[2]) + " . " + Convert.ToString(b[3]);
            if (b[3] >= 146) statusBox.BackColor = System.Drawing.Color.LightGreen; else statusBox.BackColor = System.Drawing.Color.Red;
            f = s.Available;
            b = new byte[f];
            s.Receive(b);

            msg[0] = 0x0E; //_current_pid
            a = s.Send(msg);
            k = new byte[8];
            while (s.Available < 8) ;
            c = s.Receive(k);
            long curpid = BitConverter.ToInt64(k, 0);
            curpidBox.Text = Convert.ToString(curpid);
            while (s.Available < 4) ;
            b = new byte[s.Available];
            s.Receive(b);
            showerror(b);
            return;

            msg[0] = 0x11; //_get_titleid
            a = s.Send(msg);
            k = new byte[8];
            k = BitConverter.GetBytes(pid);
            a = s.Send(k);
            while (s.Available < 8) ;
            c = s.Receive(k);
            long TID = BitConverter.ToInt64(k, 0);
            TIDBox.Text = "0x" + Convert.ToString(TID,16);
            while (s.Available < 4) ;
            b = new byte[s.Available];
            s.Receive(b);

            msg[0] = 0x11; //_get_titleid
            a = s.Send(msg);
            k = new byte[8];
            k = BitConverter.GetBytes(pid0);
            a = s.Send(k);
            while (s.Available < 8) ;
            c = s.Receive(k);
            long TID0 = BitConverter.ToInt64(k, 0);
            TID0Box.Text = "0x" + Convert.ToString(TID0, 16);
            while (s.Available < 4) ;
            b = new byte[s.Available];
            s.Receive(b);


            msg[0] = 0x0A; //_attach
            a = s.Send(msg);
            k = BitConverter.GetBytes(curpid);
            a = s.Send(k);
            while (s.Available < 4) ;
            b = new byte[s.Available];
            s.Receive(b);


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x0B }; //_detatch
            int a = s.Send(msg);
            //k = BitConverter.GetBytes(curpid);
            //a = s.Send(k);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                attachbutton1.BackColor = System.Drawing.Color.White;
                attachbutton2.BackColor = System.Drawing.Color.White;
                attached = false;
            }
        }
        private int LZ_Uncompress(byte[] inbuf,ref byte[] outbuf, int insize)
        {
            uint inpos, outpos, i;
            uint front, back;
            if (insize < 1)
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                front = (uint)(inbuf[inpos] / 16);
                back = (uint)(inbuf[inpos] & 0xF) * 8 + 8;
                inpos++;
                for (i = 0; i < front; i++)
                    outbuf[outpos + i] = inbuf[inpos + i];
                for (i = front; i < 8; i++)
                    outbuf[outpos + i] = outbuf[outpos - back + i];
                inpos += front;
                outpos += 8;
            } while (inpos < insize);
            return (int) outpos;
        }
        private int receivedata(ref byte[] dataset)
        {
            if (!user_abort)
            {
                byte[] msg = { 0x1 }; // _status; anything other than 0 
                int a = s.Send(msg);
            }
            else
            {
                byte[] msg = { 0x0 }; // 0 to abort
                int a = s.Send(msg);
                user_abort2 = true;
            }
            byte[] k = new byte[4];
            while (s.Available < 4) ;
            int c = s.Receive(k);
            int size = BitConverter.ToInt32(k, 0);
            if (size > 0)
            {
                byte[] datasetc = new byte[size];
                dataset = new byte[2048 * 32];
                while (s.Available < size) ;
                int dc = s.Receive(datasetc);
                size = LZ_Uncompress(datasetc, ref dataset, size);
            }
            else dataset = new byte[8];
            return size;
        }
        private long[,] pointer_candidate;
        private void button3_Click(object sender, EventArgs e)
        {
            pausebutton_Click(sender, e);
            if (!is_attached()) return;
            if (!command_available()) return;
            stopbutton.Enabled = true;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            RecSizeBox.Text = "0";
            byte[] msg = { 0x19 }; //_dump_ptr
            int a = s.Send(msg);
            byte[] b;
            a = s.Send(msg);
            a = s.Send(msg);
            a = s.Send(msg);
            a = s.Send(msg);
            //while (s.Available < 4) ;
            //b = new byte[s.Available];
            //s.Receive(b);

            byte[] k = new byte[8 * 4];
            while (s.Available < 8 * 4) ;
            int c = s.Receive(k);
            long address1 = BitConverter.ToInt64(k, 0);
            long address2 = BitConverter.ToInt64(k, 8);
            long address3 = BitConverter.ToInt64(k, 16);
            long address4 = BitConverter.ToInt64(k, 24);
            MainStartBox.Text = "0x" + Convert.ToString(address1, 16);
            MainEndBox.Text = "0x" + Convert.ToString(address2, 16);
            HeapStartBox.Text = "0x" + Convert.ToString(address3, 16);
            HeapEndBox.Text = "0x" + Convert.ToString(address4, 16);
            dataGridView1.Rows[fileselect].Cells[0].Value = "DirectTransfer.dmp" + Convert.ToString(fileselect);
            dataGridView1.Rows[fileselect].Cells[1].Value = "0x" + Convert.ToString(address1, 16);
            dataGridView1.Rows[fileselect].Cells[2].Value = "0x" + Convert.ToString(address2, 16);
            dataGridView1.Rows[fileselect].Cells[3].Value = "0x" + Convert.ToString(address3, 16);
            dataGridView1.Rows[fileselect].Cells[4].Value = "0x" + Convert.ToString(address4, 16);

            // create dump file
            BinaryWriter fileStream = new BinaryWriter(new FileStream("DirectTransfer.dmp" + Convert.ToString(fileselect), FileMode.Create, FileAccess.Write));
            fileStream.BaseStream.Seek(0, SeekOrigin.Begin);
            int magic = 0x4E5A4445;
            byte[] buffer = BitConverter.GetBytes(magic); 
            fileStream.BaseStream.Write(buffer, 0, 4);
            fileStream.BaseStream.Seek(134, SeekOrigin.Begin);
            buffer = BitConverter.GetBytes(address1);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address2);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address3);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address4);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes((dataGridView1.Rows[fileselect].Cells[5].Value != null)? Convert.ToInt64(dataGridView1.Rows[fileselect].Cells[5].Value.ToString(), 16):0);
            fileStream.BaseStream.Write(buffer, 0, 8);


            //pointer_candidate = new long[30000000, 2];

            info = new PointerInfo();
            new Thread(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] dataset = null;
                int c1 = 0;
                int totaldata = 0;
                do
                {
                    c1 = receivedata(ref dataset);
                    if (c1 == 0) break;
                    fileStream.BaseStream.Write(dataset, 0, c1);
                    this.RecSizeBox.Invoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < c1; i +=16)
                        {
                            //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                            //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                            Address from = new Address(MemoryType.MAIN, BitConverter.ToInt64(dataset, i ) - address1);
                            Address to = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i + 8) - address3);
                            info.AddPointer(from, to);
                        }
                        RecSizeBox.Text = Convert.ToString(totaldata+c1);
                        progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address1) / (((address2 - address1)==0)? 1: (address2 - address1)));
                        progressBar1.Value = progressBar2.Value;
                        timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    });
                    totaldata += c1;
                } while (c1 > 0);
                if (!user_abort2)
                {
                    do
                    {
                        c1 = receivedata(ref dataset);
                        if (c1 == 0) break;
                        fileStream.BaseStream.Write(dataset, 0, c1);
                        this.RecSizeBox.Invoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < c1; i += 16)
                            {
                            //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                            //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                            Address from = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i) - address3);
                                Address to = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i + 8) - address3);
                                info.AddPointer(from, to);
                            }
                            RecSizeBox.Text = Convert.ToString(totaldata + c1);
                            progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address3) / (address4 - address3));
                            progressBar1.Value = progressBar2.Value;
                            timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                        });
                        totaldata += c1;
                    } while (c1 > 0);
                }
                info.MakeList();
                fileStream.BaseStream.Close();
                this.RecSizeBox.Invoke((MethodInvoker)delegate
                {
                    buttonSearch.Enabled = true;
                });
                while (s.Available < 4) ;
                b = new byte[s.Available];
                s.Receive(b);
                this.RecSizeBox.Invoke((MethodInvoker)delegate
                {
                    showerror(b);
                    progressBar2.Value = 100;
                    progressBar1.Value = progressBar2.Value;
                    RecSizeBox.BackColor = System.Drawing.Color.LightGreen;
                    timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    stopbutton.Enabled = false;
                });
            }).Start();

            //dataGridView2.DataSource= (from arr in pointer_candidate select new { Col1 = arr[0], Col2 = arr[1] });
            //Form1.DataBind();
        }


        private void attachdmntbutton_Click(object sender, EventArgs e)
        {
            if (attached) return;
            if (!s.Connected)
            {
                button2_Click(sender, e);
                System.Threading.Thread.Sleep(500);
            }
            if (!command_available()) return;
            byte[] msg = { 0x1A }; //_attach_dmnt
            int a = s.Send(msg);
            //k = BitConverter.GetBytes(curpid);
            //a = s.Send(k);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                attachdmntbutton.BackColor = System.Drawing.Color.LightGreen;
                getstatus_Click(sender, e);
                button2_Click_1(sender, e);
                pid0Box.Text = curpidBox.Text;
                button8_Click(sender, e);
                disconnectbutton.Enabled = true;
                resumebutton.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button2_Click_1(sender, e);
            if (!command_available()) return;
            byte[] msg = { 0x18 }; //_detach_dmnt
            int a = s.Send(msg);
            //k = BitConverter.GetBytes(curpid);
            //a = s.Send(k);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                attachdmntbutton.BackColor = System.Drawing.Color.White;
                //attached = true;
            }
        }

        private void curpidBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x0A }; //_attach
            int a = s.Send(msg);
            byte[] k = new byte[8];
            long k1 = Convert.ToInt64(pidBox.Text);
            k = BitConverter.GetBytes(k1);
            a = s.Send(k);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                attachbutton1.BackColor = System.Drawing.Color.LightGreen; 
                attachbutton2.BackColor = System.Drawing.Color.White;
                attached = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x0A }; //_attach
            int a = s.Send(msg);
            byte[] k = new byte[8];
            long k1 = Convert.ToInt64(pid0Box.Text);
            k = BitConverter.GetBytes(k1);
            a = s.Send(k);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                attachbutton1.BackColor = System.Drawing.Color.White;
                attachbutton2.BackColor = System.Drawing.Color.LightGreen;
                attached = true;
            }
        }

        private void pidBox_TextChanged(object sender, EventArgs e)
        {
            byte[] msg = { 0x11 }; //_get_titleid
            int a = s.Send(msg);
            byte[] k = new byte[8];
            long pid = Convert.ToInt64(pidBox.Text);
            k = BitConverter.GetBytes(pid);
            a = s.Send(k);
            while (s.Available < 8) ;
            int c = s.Receive(k);
            long TID = BitConverter.ToInt64(k, 0);
            TIDBox.Text = "0x" + Convert.ToString(TID, 16);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            showerror(b);
        }

        private void pid0Box_TextChanged(object sender, EventArgs e)
        {
            byte[] msg = { 0x11 }; //_get_titleid
            int a = s.Send(msg);
            byte[] k = new byte[8];
            long pid = Convert.ToInt64(pid0Box.Text);
            k = BitConverter.GetBytes(pid);
            a = s.Send(k);
            while (s.Available < 8) ;
            int c = s.Receive(k);
            long TID = BitConverter.ToInt64(k, 0);
            TID0Box.Text = "0x" + Convert.ToString(TID, 16);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            showerror(b);
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void addressBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void ipBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            fileselect = 0;
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            fileselect = 1;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            fileselect = 2;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
         
        }

        private void radioButton12_CheckedChanged_1(object sender, EventArgs e)
        {
            fileselect = 3;
        }

        private void radioButton11_CheckedChanged_1(object sender, EventArgs e)
        {
            fileselect = 4;
        }

        private void stopbutton_Click(object sender, EventArgs e)
        {
            user_abort = true;
            stopbutton.Enabled = false;
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    if (s.Connected == false) 
        //    {
        //        connectBtn.Text = "Connect";
        //        ipBox.BackColor = System.Drawing.Color.White;
        //        ipBox.ReadOnly = false;
        //    }
        //}

        private void getbookmarkbutton_Click(object sender, EventArgs e)
        {
            getbookmarkbutton.BackColor = System.Drawing.Color.White;
            //if (!is_attached()) return;
            if (!command_available()) return;
            stopbutton.Enabled = true;
            //progressBar1.Value = 0;
            progressBar2.Value = 0;
            RecSizeBox.Text = "";
            getbookmarkbutton.Enabled = false;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            RecSizeBox.Text = "0";
            byte[] msg = { 0x1B }; //_getbookmark
            int a = s.Send(msg);

            //byte[] k = new byte[8];
            //long k1 = Convert.ToInt64(pid0Box.Text);
            //k = BitConverter.GetBytes(k1);
            //a = s.Send(k);
            int index = 0;
            new Thread(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] dataset = null;
                int c1 = 0;
                int totaldata = 0;
                do
                {
                    c1 = receivedata(ref dataset);
                    if (c1 == 0) break;
                    this.RecSizeBox.Invoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < c1; i += 8)
                        {
                            if (index + 2 > dataGridView4.RowCount)
                            {
                                dataGridView4.Rows.Add();
                            };
                            //pointers_candidates.AddNew();
                            //pointers_candidates.Current["From"] = BitConverter.ToInt64(dataset, i);
                            //DataRow workRow = workTable.NewRow();
                            dataGridView4.Rows[index++].Cells[0].Value = "0x" + Convert.ToString(BitConverter.ToInt64(dataset, i), 16);
                            RecSizeBox.Text = Convert.ToString(index);
                        }
                        //progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address1) / (((address2 - address1) == 0) ? 1 : (address2 - address1)));
                        //progressBar1.Value = progressBar2.Value;
                        timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    });
                    totaldata += c1;
                } while (c1 > 0);
                while (s.Available < 4) ;
                //System.Threading.Thread.Sleep(50);
                byte[] b = new byte[s.Available];
                s.Receive(b);
                this.RecSizeBox.Invoke((MethodInvoker)delegate
                {
                    showerror(b);
                    if (BitConverter.ToInt32(b, 0) == 0)
                    {
                        getbookmarkbutton.BackColor = System.Drawing.Color.LightGreen;
                    }
                    else
                    {
                        getbookmarkbutton.BackColor = System.Drawing.Color.Red;
                    }
                    progressBar2.Value = 100;
                    //progressBar1.Value = progressBar2.Value;
                    RecSizeBox.BackColor = System.Drawing.Color.LightGreen;
                    timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    stopbutton.Enabled = false;
                    getbookmarkbutton.Enabled = true;
                });
            }).Start();

            //while (s.Available < 4) ;
            //byte[] b = new byte[s.Available];
            //s.Receive(b);
            //if (!showerror(b))
            //{
            //    getbookmarkbutton.BackColor = System.Drawing.Color.LightGreen;
            //}
        }

        private void button4_Click_1(object sender, EventArgs e)
        {

                //button2_Click_1(sender, e);
                //s.Close();
                //connectBtn.Text = "Connect";
                //ipBox.BackColor = System.Drawing.Color.White;
                //ipBox.ReadOnly = false;
                //attached = false;
                //attachbutton1.BackColor = System.Drawing.Color.White;
                //attachbutton2.BackColor = System.Drawing.Color.White;
                //command_inprogress = false;


            string ipPattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";
            if (!Regex.IsMatch(ipBox.Text, ipPattern))
            {
                ipBox.BackColor = System.Drawing.Color.Red;
                return;
            }
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipBox.Text), 7331);
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            if (config.AppSettings.Settings["ipAddress"] == null) config.AppSettings.Settings.Add("ipAddress", ipBox.Text);
            else
                config.AppSettings.Settings["ipAddress"].Value = ipBox.Text;
            config.Save(ConfigurationSaveMode.Minimal);
            if (s.Connected == false)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    IAsyncResult result = s.BeginConnect(ep, null, null);
                    bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);
                    if (conSuceded == true)
                    {
                        try
                        {
                            s.EndConnect(result);
                        }
                        catch
                        {
                            this.ipBox.Invoke((MethodInvoker)delegate
                            {
                                ipBox.BackColor = System.Drawing.Color.Red;
                                ipBox.ReadOnly = false;
                            });
                            return;
                        }

                        this.connectBtn.Invoke((MethodInvoker)delegate
                        {
                            this.connectBtn.Text = "Disconnect";
                        });
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {
                            ipBox.BackColor = System.Drawing.Color.LightGreen;
                            ipBox.ReadOnly = true;
                            //this.refreshBtn.Visible = true;
                            //this.Player1Btn.Visible = true;
                            //this.Player2Btn.Visible = true;
                        });

                    }
                    else
                    {
                        s.Close();
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {
                            ipBox.BackColor = System.Drawing.Color.Red;
                        });
                        MessageBox.Show("Could not connect to the SE tools server"); //, Go to https://github.com/ for help."
                    }
                }).Start();
            }
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            button2_Click_1(sender, e);
            s.Close();
            connectBtn.Text = "Connect";
            ipBox.BackColor = System.Drawing.Color.White;
            ipBox.ReadOnly = false;
            attached = false;
            attachbutton1.BackColor = System.Drawing.Color.White;
            attachbutton2.BackColor = System.Drawing.Color.White;
            command_inprogress = false;
            attachdmntbutton.BackColor = System.Drawing.Color.White;
            pausebutton.Enabled = false;
            resumebutton.Enabled = false;
            return;
        }

        private void pausebutton_Click(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x09 }; //_pause
            int a = s.Send(msg);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                pausebutton.Enabled = false;
                resumebutton.Enabled = true;
            }
        }

        private void resumebutton_Click(object sender, EventArgs e)
        {
            if (!command_available()) return;
            byte[] msg = { 0x08 }; //_resume
            int a = s.Send(msg);
            while (s.Available < 4) ;
            byte[] b = new byte[s.Available];
            s.Receive(b);
            if (!showerror(b))
            {
                pausebutton.Enabled = true;
                resumebutton.Enabled = false;
            }
        }
        private bool showdebug = false;
        private object workTable;

        private void button6_Click_2(object sender, EventArgs e)
        {
            if (showdebug)
            {
                pictureBox2.BringToFront();
                showdebug = false;
            } else
            {
                pictureBox2.SendToBack();
                showdebug = true;
            }
        }

        private void testbutton_Click(object sender, EventArgs e)
        {
            pausebutton_Click(sender, e);
            if (!is_attached()) return;
            if (!command_available()) return;
            stopbutton.Enabled = true;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            byte[] msg = { 0x19 }; //_dump_ptr { 0x16 }; //_search_local
            int a = s.Send(msg);
            byte[] b;

            //while (s.Available < 4) ;
            //b = new byte[s.Available];
            //s.Receive(b);

            byte[] k = new byte[8 * 4];
            while (s.Available < 8 * 4) ;
            int c = s.Receive(k);
            long address1 = BitConverter.ToInt64(k, 0);
            long address2 = BitConverter.ToInt64(k, 8);
            long address3 = BitConverter.ToInt64(k, 16);
            long address4 = BitConverter.ToInt64(k, 24);
            MainStartBox.Text = "0x" + Convert.ToString(address1, 16);
            MainEndBox.Text = "0x" + Convert.ToString(address2, 16);
            HeapStartBox.Text = "0x" + Convert.ToString(address3, 16);
            HeapEndBox.Text = "0x" + Convert.ToString(address4, 16);
            dataGridView1.Rows[fileselect].Cells[0].Value = "DirectTransfer.dmp" + Convert.ToString(fileselect);
            dataGridView1.Rows[fileselect].Cells[1].Value = "0x" + Convert.ToString(address1, 16);
            dataGridView1.Rows[fileselect].Cells[2].Value = "0x" + Convert.ToString(address2, 16);
            dataGridView1.Rows[fileselect].Cells[3].Value = "0x" + Convert.ToString(address3, 16);
            dataGridView1.Rows[fileselect].Cells[4].Value = "0x" + Convert.ToString(address4, 16);

            // create dump file
            BinaryWriter fileStream = new BinaryWriter(new FileStream("DirectTransfer.dmp" + Convert.ToString(fileselect), FileMode.Create, FileAccess.Write));
            fileStream.BaseStream.Seek(0, SeekOrigin.Begin);
            int magic = 0x4E5A4445;
            byte[] buffer = BitConverter.GetBytes(magic);
            fileStream.BaseStream.Write(buffer, 0, 4);
            fileStream.BaseStream.Seek(134, SeekOrigin.Begin);
            buffer = BitConverter.GetBytes(address1);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address2);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address3);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes(address4);
            fileStream.BaseStream.Write(buffer, 0, 8);
            buffer = BitConverter.GetBytes((dataGridView1.Rows[fileselect].Cells[5].Value != null) ? Convert.ToInt64(dataGridView1.Rows[fileselect].Cells[5].Value.ToString(), 16) : 0);
            fileStream.BaseStream.Write(buffer, 0, 8);


            //pointer_candidate = new long[30000000, 2];
            info = new PointerInfo();
            new Thread(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] dataset = null;
                int c1 = 0;
                int totaldata = 0;
                do
                {
                    c1 = receivedata(ref dataset);
                    if (c1 == 0) break;
                    fileStream.BaseStream.Write(dataset, 0, c1);
                    this.RecSizeBox.Invoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < c1; i += 16)
                        {
                            //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                            //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                            //pointerdump.Rows.Add(new Object[] { BitConverter.ToInt64(dataset, i), BitConverter.ToInt64(dataset, i + 8) });
                            Address from = new Address(MemoryType.MAIN, BitConverter.ToInt64(dataset, i) - address1);
                            Address to = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i + 8) - address3);
                            info.AddPointer(from, to);
                        }
                        RecSizeBox.Text = Convert.ToString(totaldata + c1);
                        progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address1) / (((address2 - address1) == 0) ? 1 : (address2 - address1)));
                        progressBar1.Value = progressBar2.Value;
                        timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    });
                    totaldata += c1;
                } while (c1 > 0);
                if (!user_abort2)
                {
                    do
                    {
                        c1 = receivedata(ref dataset);
                        if (c1 == 0) break;
                        fileStream.BaseStream.Write(dataset, 0, c1);
                        this.RecSizeBox.Invoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < c1; i += 16)
                            {
                                //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                                //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                                pointerdump.Rows.Add(new Object[] { BitConverter.ToInt64(dataset, i), BitConverter.ToInt64(dataset, i + 8) });
                                Address from = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i) - address3);
                                Address to = new Address(MemoryType.HEAP, BitConverter.ToInt64(dataset, i + 8) - address3);
                                info.AddPointer(from, to);
                            }
                            RecSizeBox.Text = Convert.ToString(totaldata + c1);
                            progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address3) / (address4 - address3));
                            progressBar1.Value = progressBar2.Value;
                            timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                        });
                        totaldata += c1;
                    } while (c1 > 0);
                }
                info.MakeList();
                fileStream.BaseStream.Close();
                this.RecSizeBox.Invoke((MethodInvoker)delegate
                {
                    buttonSearch.Enabled = true;
                });
                while (s.Available < 4) ;
                b = new byte[s.Available];
                s.Receive(b);
                this.RecSizeBox.Invoke((MethodInvoker)delegate
                {
                    showerror(b);
                    progressBar2.Value = 100;
                    progressBar1.Value = progressBar2.Value;
                    RecSizeBox.BackColor = System.Drawing.Color.LightGreen;
                    timeusedBox.Text = Convert.ToString(sw.ElapsedMilliseconds);
                    stopbutton.Enabled = false;
                });
            }).Start();

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void bindingNavigatorMoveFirstItem_Click(object sender, EventArgs e)
        {

        }

        private void bindingNavigator1_RefreshItems_1(object sender, EventArgs e)
        {

        }

        private void dataGridView6_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pointers_candidates_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            DataRow workRow = pointerdump.NewRow();
            workRow["From"] = 123;
            workRow["To"] = 456;
            pointerdump.Rows.Add(workRow); //pointerdump.Rows.Add(new Object[] {1, "Smith"});  
        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            textBox8_TextChanged_1(sender, e);
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged_1(object sender, EventArgs e)
        {
            try
            {
                if (textBox8.Text != "" && textBox9.Text != "")
                    if (button8.Text == "Dec")
                        textBox10.Text = Convert.ToString(Convert.ToInt64(textBox8.Text) ^ Convert.ToInt64(textBox9.Text));
                    else
                        textBox10.Text = Convert.ToString(Convert.ToInt64(textBox8.Text, 16) ^ Convert.ToInt64(textBox9.Text, 16),16);
            }
            catch 
            {
                textBox10.Text = "err";
            };
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (button8.Text == "Dec")
                {
                    button8.Text = "Hex";
                    textBox8.Text = Convert.ToString(Convert.ToInt64(textBox8.Text), 16);
                    textBox9.Text = Convert.ToString(Convert.ToInt64(textBox9.Text), 16);
                }
                else
                {
                    button8.Text = "Dec";
                    textBox8.Text = Convert.ToString(Convert.ToInt64(textBox8.Text, 16));
                    textBox9.Text = Convert.ToString(Convert.ToInt64(textBox9.Text, 16));
                };
            }
            catch { textBox10.Text = "err"; };
            textBox8_TextChanged_1(sender, e);
        }
    }
}
