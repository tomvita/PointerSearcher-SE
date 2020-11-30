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
using System.Collections;

namespace PointerSearcher
{
    public partial class Form1 : Form
    {
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

        private Rectangle dragBoxFromMouseDown;
        private object valueFromMouseDown;

        public Form1()
        {
            InitializeComponent();
            int maxDepth = 4;
            int maxOffsetNum = 1;
            long maxOffsetAddress = 0x800;
            textBoxDepth.Text = maxDepth.ToString();
            textBoxOffsetNum.Text = maxOffsetNum.ToString();
            textBoxOffsetAddress.Text = maxOffsetAddress.ToString( "X" );
            buttonSearch.Enabled = false;
            buttonNarrowDown.Enabled = false;
            buttonCancel.Enabled = false;
            progressBar1.Maximum = 100;

            result = new List<List<IReverseOrderPath>>();

            dgvBookmarks.MouseMove += this.DgvBookmarks_MouseMove;
            dgvBookmarks.MouseDown += DgvBookmarks_MouseDown;
            dgvDumpTargets.DragOver += this.DgvDumpTargets_DragOver;
            dgvDumpTargets.DragDrop += this.DgvDumpTargets_DragDrop;
        }

        private void DgvBookmarks_MouseMove( Object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
            {
                if ( dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains( e.X, e.Y ) )
                {
                    dgvBookmarks.DoDragDrop( valueFromMouseDown, DragDropEffects.Copy );
                }
            }
        }

        private void DgvBookmarks_MouseDown( Object sender, MouseEventArgs e )
        {
            DataGridView.HitTestInfo info = dgvBookmarks.HitTest( e.X, e.Y );
            if ( info.RowIndex >= 0 )
            {
                valueFromMouseDown = dgvBookmarks[1, info.RowIndex].Value?.ToString();
                if ( valueFromMouseDown != null )
                {
                    Size dragSize = SystemInformation.DragSize;
                    dragBoxFromMouseDown = new Rectangle( new Point( e.X - ( dragSize.Width / 2 ), e.Y - ( dragSize.Height / 2 ) ), dragSize );
                }
            }
            else
            {
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void DgvDumpTargets_DragOver( Object sender, DragEventArgs e )
        {
            Point clientPoint = dgvDumpTargets.PointToClient( new Point( e.X, e.Y ) );
            var hittest = dgvDumpTargets.HitTest( clientPoint.X, clientPoint.Y );

            e.Effect = hittest.ColumnIndex >= 5 && hittest.ColumnIndex <= 7 && hittest.RowIndex >= 0 ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void DgvDumpTargets_DragDrop( Object sender, DragEventArgs e )
        {
            if ( e.Effect == DragDropEffects.Copy )
            {
                string data = e.Data.GetData( typeof( string ) ) as string;
                Point clientPoint = dgvDumpTargets.PointToClient( new Point( e.X, e.Y ) );
                var hittest = dgvDumpTargets.HitTest( clientPoint.X, clientPoint.Y );

                if ( hittest.ColumnIndex >= 5 && hittest.ColumnIndex <= 7 && hittest.RowIndex >= 0 )
                {
                    dgvDumpTargets[hittest.ColumnIndex, hittest.RowIndex].Value = data;
                }
            }
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            pictureBox1.BringToFront();
            dgvDumpTargets.Rows.Add( 5 );
            dgvDumpTargets[8, 0].Value = 1;
            dgvDumpTargets[8, 1].Value = 2;
            dgvDumpTargets[8, 2].Value = 3;
            dgvDumpTargets[8, 3].Value = 4;
            dgvDumpTargets[8, 4].Value = 5;

            s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            s.Close();
            ipBox.Text = ConfigurationManager.AppSettings["ipAddress"];
            pictureBox2.BringToFront();
        }

        private async void buttonRead_Click( object sender, EventArgs e )
        {
            SetProgressBar( 0 );
            try
            {
                buttonRead.Enabled = false;


                IDumpDataReader reader = CreateDumpDataReader( dgvDumpTargets.Rows[fileselect], false );
                if ( reader == null )
                {
                    throw new Exception( "Invalid input" + Environment.NewLine + "Check highlighted cell" );
                }
                //reader.readsetup(); // not reading again so the change won't be overwritten by what is in the file
                dgvDumpTargets.Rows[0].Cells[1].Value = "0x" + Convert.ToString( reader.mainStartAddress(), 16 );
                dgvDumpTargets.Rows[0].Cells[2].Value = "0x" + Convert.ToString( reader.mainEndAddress(), 16 );
                dgvDumpTargets.Rows[0].Cells[3].Value = "0x" + Convert.ToString( reader.heapStartAddress(), 16 );
                dgvDumpTargets.Rows[0].Cells[4].Value = "0x" + Convert.ToString( reader.heapEndAddress(), 16 );
                //              dataGridView1.Rows[0].Cells[5].Value = "0x" + Convert.ToString(reader.TargetAddress(), 16);
                buttonSearch.Enabled = false;
                buttonNarrowDown.Enabled = false;
                buttonCancel.Enabled = true;

                cancel = new CancellationTokenSource();
                Progress<int> prog = new Progress<int>( SetProgressBar );

                info = await Task.Run( () => reader.Read( cancel.Token, prog ) );

                SetProgressBar( 100 );
                System.Media.SystemSounds.Asterisk.Play();

                buttonSearch.Enabled = true;
            }
            catch ( System.OperationCanceledException )
            {
                SetProgressBar( 0 );
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch ( Exception ex )
            {
                SetProgressBar( 0 );
                MessageBox.Show( "Read Failed" + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            finally
            {
                if ( cancel != null )
                {
                    cancel.Dispose();
                }

                buttonCancel.Enabled = false;
                buttonRead.Enabled = true;
            }
        }

        private async void buttonSearch_Click( object sender, EventArgs e )
        {
            result.Clear();
            txtPointerSearchResults.Text = "";
            buttonRead.Enabled = false;
            //buttonSearch.Enabled = false;
            buttonNarrowDown.Enabled = true;
            textBox2.Text = dgvDumpTargets.Rows[fileselect].Cells[0].Value.ToString();
            textBox2.Text = textBox2.Text.Remove( textBox2.Text.Length - 4, 4 ) + "bmk";
            SetProgressBar( 0 );
            if ( dgvDumpTargets.Rows[fileselect].Cells[5 + targetselect].Value == null )
            { MessageBox.Show( "Target not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error ); return; }
            try
            {
                maxDepth = Convert.ToInt32( textBoxDepth.Text );
                maxOffsetNum = Convert.ToInt32( textBoxOffsetNum.Text );
                maxOffsetAddress = Convert.ToInt32( textBoxOffsetAddress.Text, 16 );
                long heapStart = Convert.ToInt64( dgvDumpTargets.Rows[fileselect].Cells[3].Value.ToString(), 16 );
                long targetAddress = Convert.ToInt64( dgvDumpTargets.Rows[fileselect].Cells[5 + targetselect].Value.ToString(), 16 );
                Address address = new Address( MemoryType.HEAP, targetAddress - heapStart );

                if ( maxOffsetNum <= 0 )
                {
                    MessageBox.Show( "Offset Num must be greater than 0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
                else if ( maxOffsetAddress < 0 )
                {
                    MessageBox.Show( "Offset Range must be greater or equal to 0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
                else
                {
                    buttonCancel.Enabled = true;

                    cancel = new CancellationTokenSource();
                    Progress<double> prog = new Progress<double>( AddProgressBar );

                    FindPath find = new FindPath( maxOffsetNum, maxOffsetAddress );

                    await Task.Run( () =>
                     {
                         find.Search( cancel.Token, prog, 100.0, info, maxDepth, new List<IReverseOrderPath>(), address, result );
                     } );

                    SetProgressBar( 100 );
                    PrintPath();
                    System.Media.SystemSounds.Asterisk.Play();

                    buttonNarrowDown.Enabled = true;
                }
            }
            catch ( System.OperationCanceledException )
            {
                SetProgressBar( 0 );
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch ( Exception ex )
            {
                SetProgressBar( 0 );
                MessageBox.Show( "Read Failed" + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
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
            txtPointerSearchResults.Text = "";
            string output = "";
            if ( result.Count > 10000 )
            {
                output = result.Count.ToString() + " results";
            }
            else if ( result.Count > 0 )
            {
                List<string> pointers = new List<string>();
                foreach ( List<IReverseOrderPath> path in result )
                {
                    String str = "main";
                    for ( int i = path.Count - 1; i >= 0; i-- )
                    {
                        str = path[i].ToString( str );
                    }
                    pointers.Add( str );
                }

                //pointers = pointers.OrderBy( x => x ).ToList<string>();
                output = string.Join( Environment.NewLine, pointers );
                pointers = null;
            }
            else
            {
                output = "not found";
            }

            txtPointerSearchResults.Text = output;
            output = null;
        }

        private void ExportPath()
        {

            if ( result.Count > 0 )
            {
                txtPointerSearchResults.Text = "Exporting result to file ... " + result.Count.ToString();
                String filepath = textBox2.Text;
                //if ((filepath == "") || System.IO.File.Exists(filepath))
                //{
                //    textBox1.Text = "Book Mark File exist";
                //    return;
                //}
                BinaryWriter BM;
                try
                {
                    BM = new BinaryWriter( new FileStream( filepath, FileMode.Create, FileAccess.Write ) );
                    BM.BaseStream.Seek( 0, SeekOrigin.Begin );
                    int magic = 0x4E5A4445;
                    BM.Write( magic );
                    long fileindex = 0;
                    long depth = 0;
                    long[] chain = new long[13];

                    foreach ( List<IReverseOrderPath> path in result )
                    {

                        BM.BaseStream.Seek( 135 + fileindex * 8 * 14, SeekOrigin.Begin ); // sizeof(pointer_chain_t)  Edizon header size = 135
                        depth = 0;
                        for ( int i = path.Count - 1; i >= 0; i-- )
                        {
                            if ( path[i] is ReverseOrderPathOffset )
                            {
                                chain[depth] = ( path[i] as ReverseOrderPathOffset ).getOffset();
                            }
                            else
                            {
                                depth++;
                                chain[depth] = 0;
                            }
                        }
                        BM.Write( depth );
                        for ( long z = depth; z >= 0; z-- )
                        {
                            BM.Write( chain[z] );
                        }

                        fileindex++;
                    };
                    for ( long z = depth + 1; z < 13; z++ )
                    {
                        BM.Write( chain[z] );
                    }

                    BM.BaseStream.Seek( 5, SeekOrigin.Begin );
                    BM.Write( result.Count * 8 * 14 );
                    BM.BaseStream.Close();
                }
                catch ( IOException ) { txtPointerSearchResults.Text = "Cannot create file"; }
            }
            else
            {
                txtPointerSearchResults.Text = "not found";
            }
        }

        private void ExportPath2()
        {
            txtPointerSearchResults.Text = "Special chain exporting result to file ... " + result.Count.ToString();
            String filepath = textBox2.Text;
            BinaryWriter BM;
            try
            {
                BM = new BinaryWriter( new FileStream( filepath, FileMode.Create, FileAccess.Write ) );
                BM.BaseStream.Seek( 0, SeekOrigin.Begin );
                int magic = 0x4E5A4445;
                BM.Write( magic );
                long fileindex = 0;
                long depth = 0;
                long[] chain = new long[13];
                long runindex = 0, s_index = 1, s_offset = 1;
                foreach ( List<IReverseOrderPath> path in result )
                {
                    runindex++;
                    if ( runindex != s_index )
                    {
                        continue;
                    }

                    for ( long x = 0x10; x <= 0x300; x += 8 )
                    {
                        BM.BaseStream.Seek( 135 + fileindex * 8 * 14, SeekOrigin.Begin ); // sizeof(pointer_chain_t)  Edizon header size = 135
                        depth = 0;
                        for ( int i = path.Count - 1; i >= 0; i-- )
                        {
                            if ( path[i] is ReverseOrderPathOffset )
                            {
                                chain[depth] = ( path[i] as ReverseOrderPathOffset ).getOffset();
                            }
                            else
                            {
                                depth++;
                                chain[depth] = 0;
                            }
                        }
                        BM.Write( depth );
                        chain[s_offset] = x;
                        for ( long z = depth; z >= 0; z-- )
                        {
                            BM.Write( chain[z] );
                        }

                        fileindex++;
                    }
                };
                for ( long z = depth + 1; z < 13; z++ )
                {
                    BM.Write( chain[z] );
                }

                BM.BaseStream.Seek( 5, SeekOrigin.Begin );
                BM.Write( fileindex * 8 * 14 );
                BM.BaseStream.Close();
            }
            catch ( IOException ) { txtPointerSearchResults.Text = "Cannot create file"; }
        }
        private void dataGridView1_CellBeginEdit( object sender, DataGridViewCellCancelEventArgs e )
        {
            if ( e.ColumnIndex == 0 )
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"";
                ofd.Filter = "EdizonSE DumpFile(*.dmp*)|*.dmp*|All Files(*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.Title = "select EdiZon SE dump file";
                if ( ofd.ShowDialog() == DialogResult.OK )
                {
                    dgvDumpTargets.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ofd.FileName;
                }
                IDumpDataReader reader = CreateDumpDataReader( dgvDumpTargets.Rows[e.RowIndex], true );
                if ( reader != null )
                {
                    reader.readsetup();
                    dgvDumpTargets.Rows[e.RowIndex].Cells[1].Value = "0x" + Convert.ToString( reader.mainStartAddress(), 16 );
                    dgvDumpTargets.Rows[e.RowIndex].Cells[2].Value = "0x" + Convert.ToString( reader.mainEndAddress(), 16 );
                    dgvDumpTargets.Rows[e.RowIndex].Cells[3].Value = "0x" + Convert.ToString( reader.heapStartAddress(), 16 );
                    dgvDumpTargets.Rows[e.RowIndex].Cells[4].Value = "0x" + Convert.ToString( reader.heapEndAddress(), 16 );
                    dgvDumpTargets.Rows[e.RowIndex].Cells[5].Value = "0x" + Convert.ToString( reader.TargetAddress(), 16 );
                    // BM1

                }
            }
        }

        private async void buttonNarrowDown_Click( object sender, EventArgs e )
        {

            try
            {
                SetProgressBar( 0 );
                Dictionary<IDumpDataReader, long> dumps = new Dictionary<IDumpDataReader, long>();
                for ( int i = 0; i < dgvDumpTargets.Rows.Count; i++ )
                {
                    if ( i == fileselect )
                    {
                        continue;
                    }

                    DataGridViewRow row = dgvDumpTargets.Rows[i];
                    ClearRowBackColor( row );
                    if ( IsBlankRow( row ) )
                    {
                        continue;
                    }
                    IDumpDataReader reader = CreateDumpDataReader( row, true );
                    if ( reader != null )
                    {
                        reader.readsetup();
                        dgvDumpTargets.Rows[i].Cells[1].Value = "0x" + Convert.ToString( reader.mainStartAddress(), 16 );
                        dgvDumpTargets.Rows[i].Cells[2].Value = "0x" + Convert.ToString( reader.mainEndAddress(), 16 );
                        dgvDumpTargets.Rows[i].Cells[3].Value = "0x" + Convert.ToString( reader.heapStartAddress(), 16 );
                        dgvDumpTargets.Rows[i].Cells[4].Value = "0x" + Convert.ToString( reader.heapEndAddress(), 16 );
                        //                     dataGridView1.Rows[i].Cells[5].Value = "0x" + Convert.ToString(reader.TargetAddress(), 16);
                        long target = Convert.ToInt64( row.Cells[5 + targetselect].Value.ToString(), 16 );

                        dumps.Add( reader, target );
                    }
                }
                if ( dumps.Count == 0 )
                {
                    throw new Exception( "Fill out 2nd line to narrow down" );
                }
                buttonRead.Enabled = false;
                buttonSearch.Enabled = false;
                buttonNarrowDown.Enabled = false;
                buttonCancel.Enabled = true;

                cancel = new CancellationTokenSource();
                Progress<int> prog = new Progress<int>( SetProgressBar );

                List<List<IReverseOrderPath>> copyList = new List<List<IReverseOrderPath>>( result );

                result = await Task.Run( () => FindPath.NarrowDown( cancel.Token, prog, result, dumps ) );

                SetProgressBar( 100 );
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch ( System.OperationCanceledException )
            {
                SetProgressBar( 0 );
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch ( Exception ex )
            {
                SetProgressBar( 0 );
                MessageBox.Show( Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
            finally
            {
                if ( cancel != null )
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

        private bool IsBlankRow( DataGridViewRow row )
        {
            for ( int i = 0; i <= 5; i++ )
            {
                if ( row.Cells[i].Value == null )
                {
                    continue;
                }
                if ( row.Cells[i].Value.ToString() != "" )
                {
                    return false;
                }
            }
            return true;
        }

        private void ClearRowBackColor( DataGridViewRow row )
        {
            for ( int i = 0; i <= 5; i++ )
            {
                row.Cells[i].Style.BackColor = Color.White;
            }
        }
        private IDumpDataReader CreateDumpDataReader( DataGridViewRow row, bool allowUnknownTarget )
        {
            bool canCreate = true;
            String path = "";
            long mainStart = -1;
            long mainEnd = -1;
            long heapStart = -1;
            long heapEnd = -1;
            long target = -1;

            if ( row.Cells[0].Value != null )
            {
                path = row.Cells[0].Value.ToString();
            }
            if ( ( path == "" ) || !System.IO.File.Exists( path ) )
            {
                row.Cells[0].Style.BackColor = Color.Red;
                canCreate = false;
                return null;
            }
            if ( row.Cells[1].Value == null )
            {
                return new NoexsDumpDataReader( path, mainStart, mainEnd, heapStart, heapEnd );
            }

            try
            {
                mainStart = Convert.ToInt64( row.Cells[1].Value.ToString(), 16 );
            }
            catch
            {
                row.Cells[1].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                mainEnd = Convert.ToInt64( row.Cells[2].Value.ToString(), 16 );
            }
            catch
            {
                row.Cells[2].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                heapStart = Convert.ToInt64( row.Cells[3].Value.ToString(), 16 );
            }
            catch
            {
                row.Cells[3].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                heapEnd = Convert.ToInt64( row.Cells[4].Value.ToString(), 16 );
            }
            catch
            {
                row.Cells[4].Style.BackColor = Color.Red;
                canCreate = false;
            }
            try
            {
                target = Convert.ToInt64( row.Cells[5].Value.ToString(), 16 );
            }
            catch
            {
                row.Cells[5].Style.BackColor = Color.Red;
                canCreate = false;
            }
            if ( !canCreate )
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
            if ( !canCreate )
            {
                return null;
            }
            return new NoexsDumpDataReader( path, mainStart, mainEnd, heapStart, heapEnd );
        }

        private void dataGridView1_CellEnter( object sender, DataGridViewCellEventArgs e )
        {
            dgvDumpTargets.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.White;
            //  dataGridView1.BeginEdit(true);
        }

        private void SetProgressBar( int percent )
        {
            progressBar1.Value = percent;
            progressTotal = percent;
        }

        private void AddProgressBar( double percent )
        {
            progressTotal += percent;
            if ( progressTotal > 100 )
            {
                progressTotal = 100;
            }
            progressBar1.Value = (int)progressTotal;
        }

        private void buttonCancel_Click_1( object sender, EventArgs e )
        {
            if ( cancel != null )
            {
                cancel.Cancel();
            }
        }

        private void OnApplicationExit( object sender, EventArgs e )
        {
            //byte[] msg = { 0x1D }; //_dmnt_resume
            //int a = SendMessage(msg);
        }

        private void Export_to_SE_Click( object sender, EventArgs e )
        {
            ExportPath();
        }

        private void button1_Click( object sender, EventArgs e )
        {
            PrintPath();
        }

        private void radioButton1_CheckedChanged( object sender, EventArgs e )
        {
            targetselect = 0;
        }

        private void radioButton2_CheckedChanged( object sender, EventArgs e )
        {
            targetselect = 1;
        }

        private void radioButton3_CheckedChanged( object sender, EventArgs e )
        {
            targetselect = 2;
        }

        private void button2_Click( object sender, EventArgs e )
        {
            getbookmarkbutton.BackColor = System.Drawing.Color.White;
            button9.BackColor = System.Drawing.Color.White;
            if ( connectBtn.Text == "Disconnect" )
            {
                button2_Click_1( sender, e );
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
            if ( !Regex.IsMatch( ipBox.Text, ipPattern ) )
            {
                ipBox.BackColor = System.Drawing.Color.Red;
                return;
            }

            s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            IPEndPoint ep = new IPEndPoint( IPAddress.Parse( ipBox.Text ), 7331 );
            Configuration config = ConfigurationManager.OpenExeConfiguration( Application.ExecutablePath );
            if ( config.AppSettings.Settings["ipAddress"] == null )
            {
                config.AppSettings.Settings.Add( "ipAddress", ipBox.Text );
            }
            else
            {
                config.AppSettings.Settings["ipAddress"].Value = ipBox.Text;
            }

            config.Save( ConfigurationSaveMode.Minimal );
            if ( s.Connected == false )
            {
                new Thread( () =>
                 {
                     Thread.CurrentThread.IsBackground = true;
                     IAsyncResult result = s.BeginConnect( ep, null, null );
                     bool conSuceded = result.AsyncWaitHandle.WaitOne( 3000, true );
                     if ( conSuceded == true )
                     {
                         try
                         {
                             s.EndConnect( result );
                         }
                         catch
                         {
                             this.ipBox.Invoke( (MethodInvoker)delegate
                             {
                                 ipBox.BackColor = System.Drawing.Color.Red;
                                 ipBox.ReadOnly = false;
                             } );
                             return;
                         }

                         this.connectBtn.Invoke( (MethodInvoker)delegate
                         {
                             this.connectBtn.Text = "Disconnect";
                         } );
                         this.ipBox.Invoke( (MethodInvoker)delegate
                         {
                             ipBox.BackColor = System.Drawing.Color.LightGreen;
                             ipBox.ReadOnly = true;
                             //this.refreshBtn.Visible = true;
                             //this.Player1Btn.Visible = true;
                             //this.Player2Btn.Visible = true;
                         } );

                     }
                     else
                     {
                         s.Close();
                         this.ipBox.Invoke( (MethodInvoker)delegate
                         {
                             ipBox.BackColor = System.Drawing.Color.Red;
                         } );
                         MessageBox.Show( "Could not connect to the SE tools server" ); //, Go to https://github.com/ for help."
                     }
                 } ).Start();
            }
        }

        private bool command_available()
        {
            if ( !s.Connected )
            {
                MessageBox.Show( "Not connected" );
                return false;
            }
            if ( command_inprogress )
            {
                MessageBox.Show( "command_inprogress" );
                return false;
            }
            command_inprogress = true;
            errorBox.Text = "";
            return true;
        }

        private bool is_attached()
        {
            if ( !attached )
            {
                MessageBox.Show( "not attached" );
                return false;
            }
            return true;
        }

        private bool showerror( byte[] b )
        {
            errorBox.Text = Convert.ToString( b[0] ) + " . " + Convert.ToString( b[1] ) + " . " + Convert.ToString( b[2] ) + " . " + Convert.ToString( b[3] );
            if ( b[0] == 15 && b[1] == 8 )
            {
                errorBox.Text += "  pminfo not valid";
            }
            if ( b[0] == 93 && b[1] == 21 )
            {
                errorBox.Text += "  already attached";
            }
            if ( b[0] == 93 && b[1] == 19 )
            {
                errorBox.Text += "  invalid cmd";
            }
            if ( b[0] == 93 && b[1] == 33 )
            {
                errorBox.Text += "  user abort";
            }
            if ( b[0] == 93 && b[1] == 35 )
            {
                errorBox.Text += "  file not accessible";
            }
            user_abort = false;
            user_abort2 = false;
            command_inprogress = false;
            int e = BitConverter.ToInt32( b, 0 );
            return e != 0;
        }

        private void getstatus_Click( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.ListPids );
            byte[] k = new byte[4];
            int c = s.Receive( k );
            int count = BitConverter.ToInt32( k, 0 );
            byte[] b = new byte[count * 8];
            int d = s.Receive( b );
            long pid = BitConverter.ToInt64( b, ( count - 2 ) * 8 );
            long pid0 = BitConverter.ToInt64( b, ( count - 1 ) * 8 );
            int f = s.Available;
            c = s.Receive( k );

            pidBox.Text = Convert.ToString( pid );
            pid0Box.Text = Convert.ToString( pid0 );

            a = SendMessage( NoexsCommands.Status );
            b = new byte[4];
            while ( s.Available < 4 )
            {
                ;
            }

            c = s.Receive( b );
            count = BitConverter.ToInt32( k, 0 );
            statusBox.Text = Convert.ToString( b[0] ) + " . " + Convert.ToString( b[1] ) + " . " + Convert.ToString( b[2] ) + " . " + Convert.ToString( b[3] );
            if ( b[3] >= 152 ) { statusBox.BackColor = System.Drawing.Color.LightGreen; }
            else
            {
                statusBox.BackColor = System.Drawing.Color.Red;
            }

            f = s.Available;
            b = new byte[f];
            s.Receive( b );

            a = SendMessage( NoexsCommands.CurrentPid );
            k = new byte[8];
            while ( s.Available < 8 )
            {
                ;
            }

            c = s.Receive( k );
            long curpid = BitConverter.ToInt64( k, 0 );
            curpidBox.Text = Convert.ToString( curpid );
            while ( s.Available < 4 )
            {
                ;
            }

            b = new byte[s.Available];
            s.Receive( b );
            showerror( b );
            return;

            /*
            msg[0] = 0x11; //_get_titleid
            a = SendMessage( msg );
            k = new byte[8];
            k = BitConverter.GetBytes( pid );
            a = SendMessage( k );
            while ( s.Available < 8 )
            {
                ;
            }

            c = s.Receive( k );
            long TID = BitConverter.ToInt64( k, 0 );
            TIDBox.Text = "0x" + Convert.ToString( TID, 16 );
            while ( s.Available < 4 )
            {
                ;
            }

            b = new byte[s.Available];
            s.Receive( b );

            msg[0] = 0x11; //_get_titleid
            a = SendMessage( msg );
            k = new byte[8];
            k = BitConverter.GetBytes( pid0 );
            a = SendMessage( k );
            while ( s.Available < 8 )
            {
                ;
            }

            c = s.Receive( k );
            long TID0 = BitConverter.ToInt64( k, 0 );
            TID0Box.Text = "0x" + Convert.ToString( TID0, 16 );
            while ( s.Available < 4 )
            {
                ;
            }

            b = new byte[s.Available];
            s.Receive( b );


            msg[0] = 0x0A; //_attach
            a = SendMessage( msg );
            k = BitConverter.GetBytes( curpid );
            a = SendMessage( k );
            while ( s.Available < 4 )
            {
                ;
            }

            b = new byte[s.Available];
            s.Receive( b );
            */

        }

        private void button2_Click_1( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.detach );
            //k = BitConverter.GetBytes(curpid);
            //a = SendMessage(k);
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                attachbutton1.BackColor = System.Drawing.Color.White;
                attachbutton2.BackColor = System.Drawing.Color.White;
                attached = false;
            }
        }

        private int LZ_Uncompress( byte[] inbuf, ref byte[] outbuf, int insize )
        {
            uint inpos, outpos, i;
            uint front, back;
            if ( insize < 1 )
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                front = (uint)( inbuf[inpos] / 16 );
                back = (uint)( inbuf[inpos] & 0xF ) * 8 + 8;
                inpos++;
                for ( i = 0; i < front; i++ )
                {
                    outbuf[outpos + i] = inbuf[inpos + i];
                }

                for ( i = front; i < 8; i++ )
                {
                    outbuf[outpos + i] = outbuf[outpos - back + i];
                }

                inpos += front;
                outpos += 8;
            } while ( inpos < insize );
            return (int)outpos;
        }
        private int rle_Uncompress( byte[] inbuf, ref byte[] outbuf, int insize )
        {
            int inpos, outpos;
            byte value, len;
            if ( insize < 1 )
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                value = inbuf[inpos];
                len = inbuf[inpos + 1];
                for ( byte i = 0; i < len; i++ )
                    outbuf[outpos] = inbuf[inpos + i];
                inpos += 2;
                outpos += len;
            } while ( inpos < insize );
            return outpos;
        }
        private bool noerror()
        {
            while ( s.Available < 4 ) { }
            byte[] b = new byte[4];
            s.Receive( b );
            return !showerror( b );
        }
        private bool readmemblock(ref byte[] outbuf, long address, int size)
        {
            if ( !command_available() )
            {
                return false;
            }
            byte[] k = new byte[5];
            int len;
            int pos = 0;
            byte[] inbuf;
            int a = SendMessage( NoexsCommands.ReadMem );
            a = SendData( BitConverter.GetBytes( address ) );
            a = SendData( BitConverter.GetBytes( size ) );
            if ( noerror() )
            {
                while (size >0)
                {
                    if ( noerror() )
                    {
                        while ( s.Available < 5 ) { }
                        s.Receive( k );
                        len = BitConverter.ToInt32( k, 1 );
                        if (k[0] == 0) // no compression
                        {
                            inbuf = new byte[len];
                            while ( s.Available < len ) { }
                            s.Receive( inbuf );
                            for ( int i = 0; i < len; i++ )
                                outbuf[pos + i] = inbuf[i];
                            pos += len;
                            size -= len;
                        }
                        else
                        {
                            k = new byte[4];
                            while ( s.Available < 4 ) { }
                            s.Receive( k );
                            int rlesize = BitConverter.ToInt32( k, 0 );
                            inbuf = new byte[rlesize];
                            while ( s.Available < rlesize ) { }
                            s.Receive( inbuf );
                            int urlesize = 0;
                            for ( int i = 0; urlesize < len; i += 2 )
                            {
                                for ( int m = 0; m < inbuf[1]; m++ )
                                    outbuf[pos + urlesize + m] = inbuf[i];
                                urlesize += inbuf[i + 1];
                            }
                            pos += urlesize;
                            size -= urlesize;
                        }
                    }
                    
                }
                
            }
            return noerror();
        } 
        private int SendMessage( NoexsCommands cmd )
        {
            return s.Send( new byte[] { (byte)cmd } );
        }

        private int SendData( byte[] data )
        {
            return s.Send( data );
        }

        private int receivedata( ref byte[] dataset )
        {
            if ( !user_abort )
            {
                int a = SendMessage( NoexsCommands.Status ); // anything other than 0 
            }
            else
            {
                int a = SendMessage( NoexsCommands.Abort );
                user_abort2 = true;
            }
            byte[] k = new byte[4];
            while ( s.Available < 4 )
            {
                ;
            }

            int c = s.Receive( k );
            int size = BitConverter.ToInt32( k, 0 );
            if ( size > 0 )
            {
                byte[] datasetc = new byte[size];
                dataset = new byte[2048 * 32];
                while ( s.Available < size )
                {
                    ;
                }

                int dc = s.Receive( datasetc );
                size = LZ_Uncompress( datasetc, ref dataset, size );
            }
            else
            {
                dataset = new byte[8];
            }

            return size;
        }

        private long[,] pointer_candidate;

        private void button3_Click( object sender, EventArgs e )
        {
            if ( dgvDumpTargets.Rows[fileselect].Cells[0].Value != null && overwrite.Checked == false )
            {
                MessageBox.Show( "File exist, check overwrite if you wish to overwrite" );
                return;
            }
            pausebutton_Click( sender, e );
            if ( !is_attached() )
            {
                return;
            }

            if ( !command_available() )
            {
                return;
            }

            stopbutton.Enabled = true;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            RecSizeBox.Text = "0";
            int a = SendMessage( NoexsCommands.DumpPtr );
            byte[] b;
            a = SendMessage( NoexsCommands.DumpPtr );
            a = SendMessage( NoexsCommands.DumpPtr );
            a = SendMessage( NoexsCommands.DumpPtr );
            a = SendMessage( NoexsCommands.DumpPtr );
            //while (s.Available < 4) ;
            //b = new byte[s.Available];
            //s.Receive(b);

            byte[] k = new byte[8 * 4];
            while ( s.Available < 8 * 4 )
            {
                ;
            }

            int c = s.Receive( k );
            long address1 = BitConverter.ToInt64( k, 0 );
            long address2 = BitConverter.ToInt64( k, 8 );
            long address3 = BitConverter.ToInt64( k, 16 );
            long address4 = BitConverter.ToInt64( k, 24 );
            MainStartBox.Text = "0x" + Convert.ToString( address1, 16 );
            MainEndBox.Text = "0x" + Convert.ToString( address2, 16 );
            HeapStartBox.Text = "0x" + Convert.ToString( address3, 16 );
            HeapEndBox.Text = "0x" + Convert.ToString( address4, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[0].Value = "DirectTransfer.dmp" + Convert.ToString( fileselect );
            dgvDumpTargets.Rows[fileselect].Cells[1].Value = "0x" + Convert.ToString( address1, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[2].Value = "0x" + Convert.ToString( address2, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[3].Value = "0x" + Convert.ToString( address3, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[4].Value = "0x" + Convert.ToString( address4, 16 );

            // create dump file
            BinaryWriter fileStream = new BinaryWriter( new FileStream( "DirectTransfer.dmp" + Convert.ToString( fileselect ), FileMode.Create, FileAccess.Write ) );
            BinaryWriter fileStream2 = new BinaryWriter( new FileStream( "DirectTransfer.tmp" + Convert.ToString( fileselect ), FileMode.Create, FileAccess.Write ) );
            fileStream2.BaseStream.Seek( 0, SeekOrigin.Begin );
            fileStream.BaseStream.Seek( 0, SeekOrigin.Begin );
            int magic = 0x4E5A4445;
            byte[] buffer = BitConverter.GetBytes( magic );
            fileStream.BaseStream.Write( buffer, 0, 4 );
            fileStream.BaseStream.Seek( 135, SeekOrigin.Begin );
            buffer = BitConverter.GetBytes( address1 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address2 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address3 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address4 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( ( dgvDumpTargets.Rows[fileselect].Cells[5].Value != null ) ? Convert.ToInt64( dgvDumpTargets.Rows[fileselect].Cells[5].Value.ToString(), 16 ) : 0 );
            fileStream.BaseStream.Write( buffer, 0, 8 );


            //pointer_candidate = new long[30000000, 2];

            info = new PointerInfo();
            new Thread( () =>
             {
                 Stopwatch sw = Stopwatch.StartNew();
                 byte[] dataset = null;
                 int c1 = 0;
                 int totaldata = 0;
                 do
                 {
                     c1 = receivedata( ref dataset );
                     if ( c1 == 0 )
                     {
                         break;
                     }

                     if ( address1 > address3 )
                     {
                         fileStream2.BaseStream.Write( dataset, 0, c1 );
                     }
                     else
                     {
                         fileStream.BaseStream.Write( dataset, 0, c1 );
                     }

                     this.RecSizeBox.Invoke( (MethodInvoker)delegate
                     {
                         for ( int i = 0; i < c1; i += 16 )
                         {
                             //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                             //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                             Address from = new Address( MemoryType.MAIN, BitConverter.ToInt64( dataset, i ) - address1 );
                             Address to = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i + 8 ) - address3 );
                             info.AddPointer( from, to );
                         }
                         RecSizeBox.Text = Convert.ToString( totaldata + c1 );
                         //long starta = BitConverter.ToInt64(dataset, 0);
                         //if (starta > address2)
                         //{ starta = starta + 1; }
                         progressBar2.Value = (int)( 100 * ( BitConverter.ToInt64( dataset, 0 ) - address1 ) / ( ( ( address2 - address1 ) == 0 ) ? 1 : ( address2 - address1 ) ) );
                         progressBar1.Value = progressBar2.Value;
                         timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     } );
                     totaldata += c1;
                 } while ( c1 > 0 );
                 if ( !user_abort2 )
                 {
                     do
                     {
                         c1 = receivedata( ref dataset );
                         if ( c1 == 0 )
                         {
                             break;
                         }

                         fileStream.BaseStream.Write( dataset, 0, c1 );
                         this.RecSizeBox.Invoke( (MethodInvoker)delegate
                         {
                             for ( int i = 0; i < c1; i += 16 )
                             {
                                 //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                                 //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                                 Address from = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i ) - address3 );
                                 Address to = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i + 8 ) - address3 );
                                 info.AddPointer( from, to );
                             }
                             RecSizeBox.Text = Convert.ToString( totaldata + c1 );
                             progressBar2.Value = (int)( 100 * ( BitConverter.ToInt64( dataset, 0 ) - address3 ) / ( address4 - address3 ) );
                             progressBar1.Value = progressBar2.Value;
                             timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                         } );
                         totaldata += c1;
                     } while ( c1 > 0 );
                 }
                 info.MakeList();
                 fileStream2.BaseStream.Close();
                 if ( address1 > address3 )
                 {
                     BinaryReader fileStream3 = new BinaryReader( new FileStream( "DirectTransfer.tmp" + Convert.ToString( fileselect ), FileMode.Open, FileAccess.Read ) );
                     fileStream3.BaseStream.CopyTo( fileStream.BaseStream );
                     //fileStream3.BaseStream.Seek(0, SeekOrigin.Begin);
                     //byte[] cbuff = fileStream3.ReadBytes((int)fileStream.BaseStream.Length);
                     //fileStream.BaseStream.Write(cbuff, 0, (int)fileStream.BaseStream.Length);
                     fileStream3.Close();
                 }
                 fileStream.BaseStream.Close();
                 this.RecSizeBox.Invoke( (MethodInvoker)delegate
                 {
                     buttonSearch.Enabled = true;
                 } );
                 while ( s.Available < 4 )
                 {
                     ;
                 }

                 b = new byte[s.Available];
                 s.Receive( b );
                 this.RecSizeBox.Invoke( (MethodInvoker)delegate
                 {
                     showerror( b );
                     progressBar2.Value = 100;
                     progressBar1.Value = progressBar2.Value;
                     RecSizeBox.BackColor = System.Drawing.Color.LightGreen;
                     timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     stopbutton.Enabled = false;
                     resumebutton_Click( sender, e );
                 } );
             } ).Start();

            //dataGridView2.DataSource= (from arr in pointer_candidate select new { Col1 = arr[0], Col2 = arr[1] });
            //Form1.DataBind();
        }


        private void attachdmntbutton_Click( object sender, EventArgs e )
        {
            getbookmarkbutton.BackColor = System.Drawing.Color.White;
            button9.BackColor = System.Drawing.Color.White;
            if ( attached )
            {
                return;
            }

            if ( !s.Connected )
            {
                button2_Click( sender, e );
                System.Threading.Thread.Sleep( 500 );
            }
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.AttachDmnt );
            //k = BitConverter.GetBytes(curpid);
            //a = SendMessage(k);
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                attachdmntbutton.BackColor = System.Drawing.Color.LightGreen;
                getstatus_Click( sender, e );
                button2_Click_1( sender, e );
                pid0Box.Text = curpidBox.Text;
                button8_Click( sender, e );
                disconnectbutton.Enabled = true;
                resumebutton.Enabled = true;
            }
        }

        private void button5_Click( object sender, EventArgs e )
        {
            button2_Click_1( sender, e );
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.DetachDmnt );
            //k = BitConverter.GetBytes(curpid);
            //a = SendMessage(k);
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                attachdmntbutton.BackColor = System.Drawing.Color.White;
                //attached = true;
            }
        }

        private void button6_Click( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.Attach );
            byte[] k = new byte[8];
            long k1 = Convert.ToInt64( pidBox.Text );
            k = BitConverter.GetBytes( k1 );
            a = SendData( k );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                attachbutton1.BackColor = System.Drawing.Color.LightGreen;
                attachbutton2.BackColor = System.Drawing.Color.White;
                attached = true;
            }
        }

        private void button8_Click( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.Attach );
            byte[] k = new byte[8];
            long k1 = Convert.ToInt64( pid0Box.Text );
            k = BitConverter.GetBytes( k1 );
            a = SendData( k );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                attachbutton1.BackColor = System.Drawing.Color.White;
                attachbutton2.BackColor = System.Drawing.Color.LightGreen;
                attached = true;
            }
        }

        private void pidBox_TextChanged( object sender, EventArgs e )
        {
            int a = SendMessage( NoexsCommands.GetTitleId );
            byte[] k = new byte[8];
            long pid = Convert.ToInt64( pidBox.Text );
            k = BitConverter.GetBytes( pid );
            a = SendData( k );
            while ( s.Available < 8 )
            {
                ;
            }

            int c = s.Receive( k );
            long TID = BitConverter.ToInt64( k, 0 );
            TIDBox.Text = "0x" + Convert.ToString( TID, 16 );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            showerror( b );
        }

        private void pid0Box_TextChanged( object sender, EventArgs e )
        {
            int a = SendMessage( NoexsCommands.GetTitleId );
            byte[] k = new byte[8];
            long pid = Convert.ToInt64( pid0Box.Text );
            k = BitConverter.GetBytes( pid );
            a = SendData( k );
            while ( s.Available < 8 )
            {
                ;
            }

            int c = s.Receive( k );
            long TID = BitConverter.ToInt64( k, 0 );
            TID0Box.Text = "0x" + Convert.ToString( TID, 16 );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            showerror( b );
        }

        private void radioButton10_CheckedChanged( object sender, EventArgs e )
        {
            fileselect = 0;
        }

        private void radioButton9_CheckedChanged( object sender, EventArgs e )
        {
            fileselect = 1;
        }

        private void radioButton8_CheckedChanged( object sender, EventArgs e )
        {
            fileselect = 2;
        }

        private void radioButton12_CheckedChanged_1( object sender, EventArgs e )
        {
            fileselect = 3;
        }

        private void radioButton11_CheckedChanged_1( object sender, EventArgs e )
        {
            fileselect = 4;
        }

        private void stopbutton_Click( object sender, EventArgs e )
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

        private void getbookmarkbutton_Click( object sender, EventArgs e )
        {
            getbookmarkbutton.BackColor = System.Drawing.Color.White;
            button9.BackColor = System.Drawing.Color.White;
            //if (!is_attached()) return;
            if ( !command_available() )
            {
                return;
            }

            stopbutton.Enabled = true;
            //progressBar1.Value = 0;
            progressBar2.Value = 0;
            RecSizeBox.Text = "";
            getbookmarkbutton.Enabled = false;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            RecSizeBox.Text = "0";
            int a = SendMessage( NoexsCommands.GetBookmark );
            byte[] label = new byte[18];
            //byte[] k = new byte[8];
            //long k1 = Convert.ToInt64(pid0Box.Text);
            //k = BitConverter.GetBytes(k1);
            //a = SendMessage(k);
            int index = 0;
            new Thread( () =>
             {
                 Stopwatch sw = Stopwatch.StartNew();
                 byte[] dataset = null;
                 int c1 = 0;
                 int totaldata = 0;
                 do
                 {
                     c1 = receivedata( ref dataset );
                     if ( c1 == 0 )
                     {
                         break;
                     }
                     c1 = c1 - c1 % 26;
                     this.RecSizeBox.Invoke( (MethodInvoker)delegate
                     {
                         dgvBookmarks.Rows.Clear();
                         for ( int i = 0; i < c1; i += 8 )
                         {
                             //for ( int j = 0; j < 18; j++ )
                             //    label[j] = dataset[i + j];
                             var bkmLabel = System.Text.Encoding.UTF8.GetString( dataset, i,18);
                             i += 18;
                             var bkmAddress = "0x" + BitConverter.ToInt64( dataset, i ).ToString( "X" );
                             dgvBookmarks.Rows.Add( new object[] { ++index, bkmAddress, bkmLabel } );
                             RecSizeBox.Text = Convert.ToString( index );
                         }
                         //progressBar2.Value = (int)(100 * (BitConverter.ToInt64(dataset, 0) - address1) / (((address2 - address1) == 0) ? 1 : (address2 - address1)));
                         //progressBar1.Value = progressBar2.Value;
                         timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     } );
                     totaldata += c1;
                 } while ( c1 > 0 );
                 while ( s.Available < 4 )
                 {
                     ;
                 }
                 //System.Threading.Thread.Sleep(50);
                 byte[] b = new byte[s.Available];
                 s.Receive( b );
                 this.RecSizeBox.Invoke( (MethodInvoker)delegate
                 {
                     showerror( b );
                     if ( BitConverter.ToInt32( b, 0 ) == 0 )
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
                     timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     stopbutton.Enabled = false;
                     getbookmarkbutton.Enabled = true;
                 } );
             } ).Start();

            //while (s.Available < 4) ;
            //byte[] b = new byte[s.Available];
            //s.Receive(b);
            //if (!showerror(b))
            //{
            //    getbookmarkbutton.BackColor = System.Drawing.Color.LightGreen;
            //}
        }

        private void button4_Click_1( object sender, EventArgs e )
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
            if ( !Regex.IsMatch( ipBox.Text, ipPattern ) )
            {
                ipBox.BackColor = System.Drawing.Color.Red;
                return;
            }
            s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            IPEndPoint ep = new IPEndPoint( IPAddress.Parse( ipBox.Text ), 7331 );
            Configuration config = ConfigurationManager.OpenExeConfiguration( Application.ExecutablePath );
            if ( config.AppSettings.Settings["ipAddress"] == null )
            {
                config.AppSettings.Settings.Add( "ipAddress", ipBox.Text );
            }
            else
            {
                config.AppSettings.Settings["ipAddress"].Value = ipBox.Text;
            }

            config.Save( ConfigurationSaveMode.Minimal );
            if ( s.Connected == false )
            {
                new Thread( () =>
                 {
                     Thread.CurrentThread.IsBackground = true;
                     IAsyncResult result = s.BeginConnect( ep, null, null );
                     bool conSuceded = result.AsyncWaitHandle.WaitOne( 3000, true );
                     if ( conSuceded == true )
                     {
                         try
                         {
                             s.EndConnect( result );
                         }
                         catch
                         {
                             this.ipBox.Invoke( (MethodInvoker)delegate
                             {
                                 ipBox.BackColor = System.Drawing.Color.Red;
                                 ipBox.ReadOnly = false;
                             } );
                             return;
                         }

                         this.connectBtn.Invoke( (MethodInvoker)delegate
                         {
                             this.connectBtn.Text = "Disconnect";
                         } );
                         this.ipBox.Invoke( (MethodInvoker)delegate
                         {
                             ipBox.BackColor = System.Drawing.Color.LightGreen;
                             ipBox.ReadOnly = true;
                             //this.refreshBtn.Visible = true;
                             //this.Player1Btn.Visible = true;
                             //this.Player2Btn.Visible = true;
                         } );

                     }
                     else
                     {
                         s.Close();
                         this.ipBox.Invoke( (MethodInvoker)delegate
                         {
                             ipBox.BackColor = System.Drawing.Color.Red;
                         } );
                         MessageBox.Show( "Could not connect to the SE tools server" ); //, Go to https://github.com/ for help."
                     }
                 } ).Start();
            }
        }

        private void button6_Click_1( object sender, EventArgs e )
        {
            getbookmarkbutton.BackColor = System.Drawing.Color.White;
            button9.BackColor = System.Drawing.Color.White;
            button2_Click_1( sender, e );
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

        private void pausebutton_Click( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.Pause );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                pausebutton.Enabled = false;
                resumebutton.Enabled = true;
            }
        }

        private void resumebutton_Click( object sender, EventArgs e )
        {
            if ( !command_available() )
            {
                return;
            }

            int a = SendMessage( NoexsCommands.Resume );
            while ( s.Available < 4 )
            {
                ;
            }

            byte[] b = new byte[s.Available];
            s.Receive( b );
            if ( !showerror( b ) )
            {
                pausebutton.Enabled = true;
                resumebutton.Enabled = false;
            }
        }
        private bool showdebug = false;
        private object workTable;

        private void button6_Click_2( object sender, EventArgs e )
        {
            if ( showdebug )
            {
                pictureBox2.BringToFront();
                showdebug = false;
            }
            else
            {
                pictureBox2.SendToBack();
                showdebug = true;
            }
        }

        private void testbutton_Click( object sender, EventArgs e )
        {
            pausebutton_Click( sender, e );
            if ( !is_attached() )
            {
                return;
            }

            if ( !command_available() )
            {
                return;
            }

            stopbutton.Enabled = true;
            RecSizeBox.BackColor = System.Drawing.Color.White;
            int a = SendMessage( NoexsCommands.DumpPtr );
            byte[] b;

            //while (s.Available < 4) ;
            //b = new byte[s.Available];
            //s.Receive(b);

            byte[] k = new byte[8 * 4];
            while ( s.Available < 8 * 4 )
            {
                ;
            }

            int c = s.Receive( k );
            long address1 = BitConverter.ToInt64( k, 0 );
            long address2 = BitConverter.ToInt64( k, 8 );
            long address3 = BitConverter.ToInt64( k, 16 );
            long address4 = BitConverter.ToInt64( k, 24 );
            MainStartBox.Text = "0x" + Convert.ToString( address1, 16 );
            MainEndBox.Text = "0x" + Convert.ToString( address2, 16 );
            HeapStartBox.Text = "0x" + Convert.ToString( address3, 16 );
            HeapEndBox.Text = "0x" + Convert.ToString( address4, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[0].Value = "DirectTransfer.dmp" + Convert.ToString( fileselect );
            dgvDumpTargets.Rows[fileselect].Cells[1].Value = "0x" + Convert.ToString( address1, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[2].Value = "0x" + Convert.ToString( address2, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[3].Value = "0x" + Convert.ToString( address3, 16 );
            dgvDumpTargets.Rows[fileselect].Cells[4].Value = "0x" + Convert.ToString( address4, 16 );

            // create dump file
            BinaryWriter fileStream = new BinaryWriter( new FileStream( "DirectTransfer.dmp" + Convert.ToString( fileselect ), FileMode.Create, FileAccess.Write ) );
            fileStream.BaseStream.Seek( 0, SeekOrigin.Begin );
            int magic = 0x4E5A4445;
            byte[] buffer = BitConverter.GetBytes( magic );
            fileStream.BaseStream.Write( buffer, 0, 4 );
            fileStream.BaseStream.Seek( 135, SeekOrigin.Begin );
            buffer = BitConverter.GetBytes( address1 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address2 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address3 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( address4 );
            fileStream.BaseStream.Write( buffer, 0, 8 );
            buffer = BitConverter.GetBytes( ( dgvDumpTargets.Rows[fileselect].Cells[5].Value != null ) ? Convert.ToInt64( dgvDumpTargets.Rows[fileselect].Cells[5].Value.ToString(), 16 ) : 0 );
            fileStream.BaseStream.Write( buffer, 0, 8 );


            //pointer_candidate = new long[30000000, 2];
            info = new PointerInfo();
            new Thread( () =>
             {
                 Stopwatch sw = Stopwatch.StartNew();
                 byte[] dataset = null;
                 int c1 = 0;
                 int totaldata = 0;
                 do
                 {
                     c1 = receivedata( ref dataset );
                     if ( c1 == 0 )
                     {
                         break;
                     }

                     fileStream.BaseStream.Write( dataset, 0, c1 );
                     this.RecSizeBox.Invoke( (MethodInvoker)delegate
                     {
                         for ( int i = 0; i < c1; i += 16 )
                         {
                             //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                             //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                             //pointerdump.Rows.Add(new Object[] { BitConverter.ToInt64(dataset, i), BitConverter.ToInt64(dataset, i + 8) });
                             Address from = new Address( MemoryType.MAIN, BitConverter.ToInt64( dataset, i ) - address1 );
                             Address to = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i + 8 ) - address3 );
                             info.AddPointer( from, to );
                         }
                         RecSizeBox.Text = Convert.ToString( totaldata + c1 );
                         progressBar2.Value = (int)( 100 * ( BitConverter.ToInt64( dataset, 0 ) - address1 ) / ( ( ( address2 - address1 ) == 0 ) ? 1 : ( address2 - address1 ) ) );
                         progressBar1.Value = progressBar2.Value;
                         timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     } );
                     totaldata += c1;
                 } while ( c1 > 0 );
                 if ( !user_abort2 )
                 {
                     do
                     {
                         c1 = receivedata( ref dataset );
                         if ( c1 == 0 )
                         {
                             break;
                         }

                         fileStream.BaseStream.Write( dataset, 0, c1 );
                         this.RecSizeBox.Invoke( (MethodInvoker)delegate
                         {
                             for ( int i = 0; i < c1; i += 16 )
                             {
                                 //pointer_candidate[(totaldata+i)/16, 0] = BitConverter.ToInt64(dataset, i);
                                 //pointer_candidate[(totaldata+i)/16, 1] = BitConverter.ToInt64(dataset, i + 8);
                                 pointerdump.Rows.Add( new Object[] { BitConverter.ToInt64( dataset, i ), BitConverter.ToInt64( dataset, i + 8 ) } );
                                 Address from = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i ) - address3 );
                                 Address to = new Address( MemoryType.HEAP, BitConverter.ToInt64( dataset, i + 8 ) - address3 );
                                 info.AddPointer( from, to );
                             }
                             RecSizeBox.Text = Convert.ToString( totaldata + c1 );
                             progressBar2.Value = (int)( 100 * ( BitConverter.ToInt64( dataset, 0 ) - address3 ) / ( address4 - address3 ) );
                             progressBar1.Value = progressBar2.Value;
                             timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                         } );
                         totaldata += c1;
                     } while ( c1 > 0 );
                 }
                 info.MakeList();
                 fileStream.BaseStream.Close();
                 this.RecSizeBox.Invoke( (MethodInvoker)delegate
                 {
                     buttonSearch.Enabled = true;
                 } );
                 while ( s.Available < 4 )
                 {
                     ;
                 }

                 b = new byte[s.Available];
                 s.Receive( b );
                 this.RecSizeBox.Invoke( (MethodInvoker)delegate
                 {
                     showerror( b );
                     progressBar2.Value = 100;
                     progressBar1.Value = progressBar2.Value;
                     RecSizeBox.BackColor = System.Drawing.Color.LightGreen;
                     timeusedBox.Text = Convert.ToString( sw.ElapsedMilliseconds );
                     stopbutton.Enabled = false;
                 } );
             } ).Start();

        }

        private void button7_Click( object sender, EventArgs e )
        {
            DataRow workRow = pointerdump.NewRow();
            workRow["From"] = 123;
            workRow["To"] = 456;
            pointerdump.Rows.Add( workRow ); //pointerdump.Rows.Add(new Object[] {1, "Smith"});  
        }

        private void textBox9_TextChanged( object sender, EventArgs e )
        {
            textBox8_TextChanged_1( sender, e );
        }

        private void textBox8_TextChanged_1( object sender, EventArgs e )
        {
            try
            {
                if ( textBox8.Text != "" && textBox9.Text != "" )
                {
                    if ( button8.Text == "Dec" )
                    {
                        textBox10.Text = Convert.ToString( Convert.ToInt64( textBox8.Text ) ^ Convert.ToInt64( textBox9.Text ) );
                    }
                    else
                    {
                        textBox10.Text = Convert.ToString( Convert.ToInt64( textBox8.Text, 16 ) ^ Convert.ToInt64( textBox9.Text, 16 ), 16 );
                    }
                }
            }
            catch
            {
                textBox10.Text = "err";
            };
        }

        private void button8_Click_1( object sender, EventArgs e )
        {
            try
            {
                if ( button8.Text == "Dec" )
                {
                    button8.Text = "Hex";
                    textBox8.Text = Convert.ToString( Convert.ToInt64( textBox8.Text ), 16 );
                    textBox9.Text = Convert.ToString( Convert.ToInt64( textBox9.Text ), 16 );
                }
                else
                {
                    button8.Text = "Dec";
                    textBox8.Text = Convert.ToString( Convert.ToInt64( textBox8.Text, 16 ) );
                    textBox9.Text = Convert.ToString( Convert.ToInt64( textBox9.Text, 16 ) );
                };
            }
            catch { textBox10.Text = "err"; };
            textBox8_TextChanged_1( sender, e );
        }

        private void button9_Click( object sender, EventArgs e )
        {
            button9.BackColor = System.Drawing.Color.White;
            if ( textBox2.Text == "" ) { MessageBox.Show( "bookmark filename missing" ); return; };
            String filepath = textBox2.Text;
            BinaryReader BM;
            try
            {
                BM = new BinaryReader( new FileStream( filepath, FileMode.Open, FileAccess.Read ) );
                BM.BaseStream.Seek( 0, SeekOrigin.Begin );
                int readSize = (int)( BM.BaseStream.Length );
                byte[] buff;
                buff = BM.ReadBytes( readSize );

                if ( !command_available() )
                {
                    return;
                }

                int a = SendMessage( NoexsCommands.PutBookmark );
                while ( s.Available < 4 )
                {
                    ;
                }

                byte[] b = new byte[s.Available];
                s.Receive( b );
                if ( !showerror( b ) )
                {
                    byte[] fsize = BitConverter.GetBytes( readSize );
                    SendData( fsize );
                    SendData( buff );
                    while ( s.Available < 4 )
                    {
                        ;
                    }

                    b = new byte[s.Available];
                    s.Receive( b );
                    if ( !showerror( b ) )
                    { button9.BackColor = System.Drawing.Color.LightGreen; }
                }
                else { MessageBox.Show( "Remote file not accessible" ); }
                BM.BaseStream.Close();
            }
            catch ( IOException ) { txtPointerSearchResults.Text = "Cannot Read file"; }
        }

        private void button10_Click( object sender, EventArgs e )
        {
            ExportPath2();
        }


        private void button11_Click_1( Object sender, EventArgs e )
        {
            long targetAddress = Convert.ToInt64( dgvDumpTargets.Rows[fileselect].Cells[5 + targetselect].Value.ToString(), 16 );
            int size = Convert.ToInt32( dgvDumpTargets.Rows[fileselect].Cells[6 + targetselect].Value.ToString(), 16 );
            byte[] outbuf = new byte[size];
            readmemblock( ref outbuf, targetAddress, size );
            int a = 1;
        }
    }
}
