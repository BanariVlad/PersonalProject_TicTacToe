using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net.Sockets;

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

            if(isHost)
            {
                PlayerChar = "X";
                OpponentChar = "0";
                server = new TcpListener(System.Net.IPAddress.Any, 3000);
                server.Start();
                sock = server.AcceptSocket();
            }
            else
            {
                PlayerChar = "0";
                OpponentChar = "X";
                try
                {
                    client = new TcpClient(ip, 3000);
                    sock = client.Client;
                    messageReceiver.RunWorkerAsync();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }
        
        private bool turn = true;
        private int turnCount;
        private bool isWinner;
        private BackgroundWorker messageReceiver = new BackgroundWorker();
        private TcpListener server;
        private TcpClient client;
        private string PlayerChar;
        private string OpponentChar;
        private Socket sock;

        private void MessageReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            
            DisableButtons();
            ReceiveMove();
            if (!isWinner)
                EnableButtons();
        }

        private void GenerateButtons()
        {
            var y = 30;
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
            if (btn.Enabled)
            {
                if (turn)
                {
                    btn.Text = PlayerChar;
                }
                else
                {
                    btn.Text = OpponentChar;
                }
            }
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
            if (Controls["0_0"].Text == Controls["0_1"].Text &&
                Controls["0_1"].Text == Controls["0_2"].Text && Controls["0_0"].Text != "")
                isWinner = true;
            else if (Controls["1_0"].Text == Controls["1_1"].Text &&
                     Controls["1_1"].Text == Controls["1_2"].Text && Controls["1_0"].Text != "")
                isWinner = true;
            else if (Controls["2_0"].Text == Controls["2_1"].Text &&
                     Controls["2_1"].Text == Controls["2_2"].Text && Controls["2_0"].Text != "")
                isWinner = true;
            else if (Controls["0_0"].Text == Controls["1_0"].Text &&
                     Controls["1_0"].Text == Controls["2_0"].Text && Controls["0_0"].Text != "")
                isWinner = true;
            else if (Controls["0_1"].Text == Controls["1_1"].Text &&
                     Controls["1_1"].Text == Controls["2_1"].Text && Controls["0_1"].Text != "")
                isWinner = true;
            else if (Controls["0_2"].Text == Controls["1_2"].Text &&
                     Controls["1_2"].Text == Controls["2_2"].Text && Controls["0_2"].Text != "")
                isWinner = true;
            else if (Controls["0_0"].Text == Controls["1_1"].Text &&
                     Controls["1_1"].Text == Controls["2_2"].Text && Controls["0_0"].Text != "")
                isWinner = true;
            else if (Controls["0_2"].Text == Controls["1_1"].Text &&
                     Controls["1_1"].Text == Controls["2_0"].Text && Controls["0_2"].Text != "")
                isWinner = true;

            if (isWinner)
            {
                if (turn) MessageBox.Show($"{PlayerChar} won");
                else MessageBox.Show($"{OpponentChar} won");
                DisableButtons();
            }
            else if (turnCount == 9)
            {
                MessageBox.Show(@"Draw");
            }
        }

        private void DisableButtons()
        {
            foreach (var btn in Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                b.Enabled = false;
            }
        }

        private void EnableButtons()
        {
            foreach (var btn in Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                if (b.Text == "")
                {
                    b.Enabled = true;
                }
            }
        }
        
        private void ReceiveMove()
        {
            byte[] buffer = new byte[1024];
            int ascii = sock.Receive(buffer);
            char[] chars = new char[ascii];
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(buffer, 0, ascii, chars, 0);
            var name = new String(chars);
            Controls[name].Text = OpponentChar;
        }
    }

}