using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Tic_Tac_Toe
{
    public class Game
    {
        public Game(Form form, bool isHost, string ip = null)
        {
            messageReceiver.DoWork += (sender, args) => MessageWork(form);
            if (isHost)
            {
                yourChar = "X";
                opponentChar = "0";
                server = new TcpListener(System.Net.IPAddress.Any, 3000);
                server.Start();
                socket = server.AcceptSocket();
            }
            else
            {
                yourChar = "0";
                opponentChar = "X";
                try
                {
                    client = new TcpClient(ip ?? string.Empty, 3000);
                    socket = client.Client;
                    messageReceiver.RunWorkerAsync();
                }
                catch
                {
                    MessageBox.Show(@"Error");
                }
            }
        }

        private bool turn = true;
        private int turnCount;
        private bool isWinner = false;
        private Socket socket;
        private string yourChar;
        private string opponentChar;
        private BackgroundWorker messageReceiver = new BackgroundWorker();
        private TcpListener server = null;
        private TcpClient client = null;

        private void MessageWork(Form form)
        {
            if (isWinner)
                return;
            DisableButtons(form);
            ReceiveMove(form);
            if (!isWinner)
                EnableButtons(form);
        }

        private void ReceiveMove(Form form)
        {
            byte[] buffer = new byte[1];
            socket.Receive(buffer);
            foreach (var btn in form.Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                b.Enabled = false;
                MessageBox.Show(buffer[0].ToString());
            }

        }
        
        public void GenerateButtons(Form form)
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
                    btn.Click += (sender, e) => MakeTurn(btn, form);
                    btn.MouseEnter += (sender, e) => ShowTooltip(btn);
                    btn.MouseLeave += (sender, e) => HideTooltip(btn);
                    x += 103;
                    form.Controls.Add(btn);
                }

                y += 103;
            }
        }

        public void NewGame(Form form)
        {
            turn = true;
            turnCount = 0;
            foreach (var btn in form.Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                b.Enabled = true;
                b.Text = "";
            }
        }

        private void ShowTooltip(Button btn)
        {
            if (btn.Enabled)
            {
                if (turn)
                {
                    btn.Text = @"X";
                }
                else
                {
                    btn.Text = @"0";
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

        private void MakeTurn(Button btn, Form form)
        {
            if (turn) btn.Text = @"X";
            else btn.Text = @"0";

            turn = !turn;
            btn.Enabled = false;
            Winner(form);
        }

        private void Winner(Form form)
        {
            if (form.Controls["0_0"].Text == form.Controls["0_1"].Text &&
                form.Controls["0_1"].Text == form.Controls["0_2"].Text && !form.Controls["0_0"].Enabled)
                isWinner = true;
            else if (form.Controls["1_0"].Text == form.Controls["1_1"].Text &&
                     form.Controls["1_1"].Text == form.Controls["1_2"].Text && !form.Controls["1_0"].Enabled)
                isWinner = true;
            else if (form.Controls["2_0"].Text == form.Controls["2_1"].Text &&
                     form.Controls["2_1"].Text == form.Controls["2_2"].Text && !form.Controls["2_0"].Enabled)
                isWinner = true;
            else if (form.Controls["0_0"].Text == form.Controls["1_0"].Text &&
                     form.Controls["1_0"].Text == form.Controls["2_0"].Text && !form.Controls["0_0"].Enabled)
                isWinner = true;
            else if (form.Controls["0_1"].Text == form.Controls["1_1"].Text &&
                     form.Controls["1_1"].Text == form.Controls["2_1"].Text && !form.Controls["0_1"].Enabled)
                isWinner = true;
            else if (form.Controls["0_2"].Text == form.Controls["1_2"].Text &&
                     form.Controls["1_2"].Text == form.Controls["2_2"].Text && !form.Controls["0_2"].Enabled)
                isWinner = true;
            else if (form.Controls["0_0"].Text == form.Controls["1_1"].Text &&
                     form.Controls["1_1"].Text == form.Controls["2_2"].Text && !form.Controls["0_0"].Enabled)
                isWinner = true;
            else if (form.Controls["0_2"].Text == form.Controls["1_1"].Text &&
                     form.Controls["1_1"].Text == form.Controls["2_0"].Text && !form.Controls["0_2"].Enabled)
                isWinner = true;
            turnCount++;

            if (isWinner)
            {
                if (turn) MessageBox.Show(@"0 won");
                else MessageBox.Show(@"X won");
                DisableButtons(form);
            }
            else if (turnCount == 9)
            {
                MessageBox.Show(@"Draw");
            }
        }

        private void DisableButtons(Form form)
        {
            foreach (var btn in form.Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                b.Enabled = false;
            }
        }

        private void EnableButtons(Form form)
        {
            foreach (var btn in form.Controls.OfType<Button>().Cast<Control>().ToList())
            {
                Button b = (Button) btn;
                if (b.Text == "")
                {
                    b.Enabled = true;
                }
            }
        }
    }
}