using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net.Sockets;
using CheckWinner;
using Excel = Microsoft.Office.Interop.Excel;

namespace Tic_Tac_Toe
{
    public partial class Gameplay : Form
    {
        public Gameplay(bool isHost, string ip = null)
        {
            GenerateButtons();
            InitializeComponent();
            messageReceiver.DoWork += MessageReceiver_DoWork;
            CheckForIllegalCrossThreadCalls = false;

            if (isHost)
            {
                PlayerChar = "X";
                OpponentChar = "0";
                label1.Text = $@"Your character is {PlayerChar}";
                label2.Text = @"Your Turn!";
                server = new TcpListener(System.Net.IPAddress.Any, 3000);
                server.Start();
                sock = server.AcceptSocket();
            }
            else
            {
                PlayerChar = "0";
                OpponentChar = "X";
                label1.Text = $@"Your character is {PlayerChar}";
                label2.Text = @"Opponent's Turn!";
                try
                {
                    client = new TcpClient(ip ?? string.Empty, 3000);
                    sock = client.Client;
                    messageReceiver.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }

        private int wins;
        private int draws;
        private int loses;
        private string isWinner;
        private BackgroundWorker messageReceiver = new BackgroundWorker();
        private TcpListener server;
        private TcpClient client;
        private string PlayerChar;
        private string OpponentChar;
        private Socket sock;

        private void MessageReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableButtons();
            label2.Text = @"Opponent's Turn!";
            ReceiveMove();
            label2.Text = @"Your Turn!";
            if (isWinner == "")
                EnableButtons();
        }

        private void GenerateButtons()
        {
            var y = 66;
            for (int i = 0; i < 3; i++)
            {
                var x = 15;
                for (int j = 0; j < 3; j++)
                {
                    var btn = new Button
                    {
                        Location = new Point(x, y), Size = new Size(100, 100), Font = new Font("Arial", 30),
                        Name = $"{i}_{j}"
                    };
                    btn.Click += (sender, e) => MakeTurn(btn);
                    btn.MouseEnter += (sender, e) => ShowTooltip(btn);
                    btn.MouseLeave += (sender, e) => HideTooltip(btn);
                    x += 103;
                    Controls.Add(btn);
                }

                y += 103;
            }
        }

        private void ShowTooltip(Button btn)
        {
            btn.Text = PlayerChar;
        }

        private void HideTooltip(Button btn)
        {
            if (btn.Enabled)
            {
                btn.Text = "";
            }
        }

        private void MakeTurn(Button btn)
        {
            btn.Text = PlayerChar;
            byte[] value = System.Text.Encoding.ASCII.GetBytes(btn.Name);
            sock.Send(value);
            messageReceiver.RunWorkerAsync();
            DisableButtons();
            Winner();
        }

        private void Winner()
        {
            isWinner = new Class1().Winner(this);
            if (isWinner != "")
            {
                if (isWinner == PlayerChar)
                {
                    MessageBox.Show($@"{isWinner} won");
                    wins++;
                }
                else if (isWinner == "draw")
                {
                    MessageBox.Show(@"Draw!");
                    draws++;
                }
                else if (isWinner == OpponentChar)
                {
                    MessageBox.Show($@"{isWinner} won");
                    loses++;
                }

                foreach (var btn in Controls.OfType<Button>().Cast<Control>().ToList())
                {
                    var b = (Button) btn;
                    b.Text = "";
                    b.Enabled = true;
                }
            }
        }

        private void DisableButtons()
        {
            foreach (var btn in Controls.OfType<Button>().Cast<Control>().ToList())
            {
                var b = (Button) btn;
                b.Enabled = false;
            }
        }

        private void EnableButtons()
        {
            foreach (var btn in Controls.OfType<Button>().Cast<Control>().ToList())
            {
                var b = (Button) btn;
                if (b.Text == "")
                {
                    b.Enabled = true;
                }
            }
        }

        private void ReceiveMove()
        {
            var buffer = new byte[1024];
            var ascii = sock.Receive(buffer);
            var chars = new char[ascii];
            var d = System.Text.Encoding.UTF8.GetDecoder();
            var charLen = d.GetChars(buffer, 0, ascii, chars, 0);
            var name = new String(chars);
            Controls[name].Text = OpponentChar;
            Winner();
        }

        private void exportStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object misValue = System.Reflection.Missing.Value;
            Excel.Application xlApp = new Excel.ApplicationClass();
            var xlWorkBook = xlApp.Workbooks.Add(misValue);
            var xlWorkSheet = (Excel.Worksheet) xlWorkBook.Worksheets.Item[1];
            xlWorkSheet.Cells[2, 1] = "Wins";
            xlWorkSheet.Cells[3, 1] = "Draws";
            xlWorkSheet.Cells[4, 1] = "Loses";
            xlWorkSheet.Cells[1, 2] = $@"{PlayerChar}";
            xlWorkSheet.Cells[2, 2] = $@"{wins}";
            xlWorkSheet.Cells[3, 2] = $@"{draws}";
            xlWorkSheet.Cells[4, 2] = $@"{loses}";
            xlApp.Visible = true;
        }
    }
}