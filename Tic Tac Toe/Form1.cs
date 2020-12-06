using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tic_Tac_Toe
{
    public partial class Form1 : Form
    {
        private Game Game = new Game();
        public Form1()
        {
            InitializeComponent();
            Game.GenerateButtons(this);
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.NewGame(this);
        }
    }
}