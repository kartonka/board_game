using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace Marci30
{
    public partial class Form1 : Form
    {
        private const string BTN_TEXT_BASE = "Dobj egy kockával! Legutóbbi dobás:";
        private readonly string FIELD_TASK_DB_PATH;
        private readonly string FIELD_PIC_PATH;
        private readonly string COVER_PIC_PATH;
        private readonly PictureBox[] pictureBoxes;
        private readonly TextBox[] textBoxes;
        private readonly TextBox[] playerTextBoxes;
        private readonly Field[] fields;
        private readonly List<Player> players = new List<Player>();
        private readonly Pen[] playerPens;
        private Rectangle[] playerRectangles;
        private Font playerLetterFont;
        private int picHeight, picWidth;//pix
        private int? selectedPlayer = null;

        public Form1()
        {
            InitializeComponent();

            var paths = File.ReadAllLines("paths.ini");

            FIELD_TASK_DB_PATH = paths[0];
            FIELD_PIC_PATH = paths[1];
            COVER_PIC_PATH = paths[2];

            button1.Text = BTN_TEXT_BASE + '-';
            button1.Focus();
            KeyPress += Form1_KeyPress;
            BackColor = Color.Bisque;
            button1.BackColor = Color.White;
            button1.ForeColor = Color.Black;

            textBoxes = new TextBox[]
            {
            textBox1,
            textBox2,
            textBox3,
            textBox4,
            textBox5,
            textBox6,

            textBox7,
            textBox8,
            textBox9,
            textBox10,
            textBox11,
            textBox12,

            textBox13,
            textBox14,
            textBox15,
            textBox16,
            textBox17,
            textBox18,

            textBox19,
            textBox20,
            textBox21,
            textBox22,
            textBox23,
            textBox24,

            textBox25,
            textBox26,
            textBox27,
            textBox28,
            textBox29,
            textBox30
            };
            playerTextBoxes = new TextBox[] { textBoxP1, textBoxP2, textBoxP3, textBoxP4 };

            pictureBoxes = new PictureBox[]
            {
            pictureBox1,
            pictureBox2,
            pictureBox3,
            pictureBox4,
            pictureBox5,
            pictureBox6,

            pictureBox7,
            pictureBox8,
            pictureBox9,
            pictureBox10,
            pictureBox11,
            pictureBox12,

            pictureBox13,
            pictureBox14,
            pictureBox15,
            pictureBox16,
            pictureBox17,
            pictureBox18,

            pictureBox19,
            pictureBox20,
            pictureBox21,
            pictureBox22,
            pictureBox23,
            pictureBox24,

            pictureBox25,
            pictureBox26,
            pictureBox27,
            pictureBox28,
            pictureBox29,
            pictureBox30
            };

            foreach (PictureBox pb in pictureBoxes)
            {
                pb.MouseClick += PictureBoxClick;
                pb.MouseDoubleClick += PictureBoxDoubleClick;
                pb.Visible = true;
                pb.Image = LoadPic(COVER_PIC_PATH);
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Paint += PictureboxPaint;
            }

            fields = new Field[pictureBoxes.Length];

            for (int i = 0; i < fields.Length; i++)
                fields[i] = new Field();

            for (int i = 0; i < textBoxes.Length; i++)
            {
                textBoxes[i].Visible = true;
                textBoxes[i].Text = (i + 1).ToString();
            }

            playerPens = new Pen[] { new Pen(Brushes.Red), new Pen(Brushes.Green), new Pen(Brushes.Blue), new Pen(Brushes.Black) };

            FindFieldContent();
            Load += Form1_Load;
        }

        private Bitmap LoadPic(string path)
        {
            Image i = Image.FromFile(path);
            PropertyItem pi = i.PropertyItems.Select(x => x)
                                 .FirstOrDefault(x => x.Id == 0x0112);

            if (pi == null)
            {
                pi = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));

                pi.Id = 0x0112;   // orientation
                pi.Len = 2;
                pi.Type = 3;
                pi.Value = new byte[2] { 1, 0 };

                pi.Value[0] = 1;

                i.SetPropertyItem(pi);
            }

            Bitmap bmp = new Bitmap(i);

            byte o = pi.Value[0];

            if (o == 2) bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            if (o == 3) bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            if (o == 4) bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (o == 5) bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
            if (o == 6) bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            if (o == 7) bmp.RotateFlip(RotateFlipType.Rotate90FlipY);
            if (o == 8) bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);

            return bmp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            picHeight = pictureBox1.Height;
            picWidth = pictureBox1.Width;

            playerRectangles = new Rectangle[playerPens.Length];

            for (int i = 0; i < playerRectangles.Length; i++)
            {
                playerRectangles[i].X = (i % 2) * picWidth / 2;
                playerRectangles[i].Y = (i / 2) * picHeight / 2;
                playerRectangles[i].Width = Math.Min(picWidth, picHeight) / 2;
                playerRectangles[i].Height = Math.Min(picWidth, picHeight) / 2;
            }

            playerLetterFont = new Font(FontFamily.GenericSansSerif, (int)(54.0 / 164 * picHeight));
        }

        private void PictureboxPaint(object sender, PaintEventArgs e)
        {
            int position = int.Parse(((PictureBox)sender).Name.Substring("pictureBox".Length)) - 1;

            for (int i = 0; i < players.Count; i++)
                if (players[i].Position == position)
                    DrawPlayer(e.Graphics, i);
        }

        private void DrawPlayer(Graphics g, int i)
        {
            g.FillEllipse(playerPens[i].Brush, playerRectangles[i]);
            g.DrawString(players[i].Letter.ToString(), playerLetterFont, (selectedPlayer != null && selectedPlayer == i) ? Brushes.Yellow : Brushes.White, playerRectangles[i]);
        }

        private void FindFieldContent()
        {
            textBoxTitle.Text = FindTaskParts(out string[] titles, out string[][] subtasks);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(FIELD_PIC_PATH);
            Shell32.FolderItems items = folder.Items();

            for (int i = 0; i < items.Count; i++)
            {
                var fileName = folder.GetDetailsOf(items.Item(i), 0);
                DecompilePicName(Path.GetFileNameWithoutExtension(fileName), out var position, out var taskNumber);

                fields[position - 1].TaskNumbers.Add(taskNumber);
                fields[position - 1].Title = titles[position - 1];

                if (subtasks[position - 1].Length < taskNumber)
                {
                    MessageBox.Show("Nem tartozik feladat a képhez:" + fileName);
                    fields[position - 1].Tasks.Add("");
                }
                else
                {
                    fields[position - 1].Tasks.Add(subtasks[position - 1][taskNumber - 1]);
                }

                fields[position - 1].Paths.Add(FIELD_PIC_PATH + fileName);
            }
        }

        private string FindTaskParts(out string[] titles, out string[][] tasks)
        {
            string gameName;
            var lines = File.ReadAllLines(FIELD_TASK_DB_PATH, System.Text.Encoding.GetEncoding(1252));
            titles = new string[pictureBoxes.Length];
            tasks = new string[pictureBoxes.Length][];

            gameName = lines[0].Split(';')[0];

            for (int i = 0; i < pictureBoxes.Length; i++)
            {
                DecompileLine(lines[i + 1], out string title, out List<string> subtasks);
                titles[i] = title;
                tasks[i] = subtasks.ToArray();
            }

            return gameName;
        }

        private void DecompileLine(string line, out string title, out List<string> subtasks)
        {
            var lineparts = line.Split(';');

            if (lineparts.Length < 4)
            {
                MessageBox.Show("Hibás sor:" + line);
                Environment.Exit(0);
                throw new NotImplementedException();
            }
            else
            {
                title = lineparts[1];
                subtasks = new List<string>();

                for (int i = 1; i < lineparts.Length / 2; i++)
                {
                    if (lineparts[i * 2] == "")
                        break;
                    else
                        subtasks.Add(lineparts[i * 2]);
                }
            }
        }

        private void DecompilePicName(string fileName, out int position, out int taskNumber)
        {
            for (int i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '_')
                {
                    if (!int.TryParse(fileName.Substring(0, i), out position) || !int.TryParse(fileName.Substring(i + 1), out taskNumber) || position < 1 || position > pictureBoxes.Length || taskNumber < 0)
                    {
                        MessageBox.Show("Nem támogatott filenév:" + fileName);
                        Environment.Exit(0);
                    }
                    else
                        return;
                }
            }

            MessageBox.Show("Nem támogatott filenév:" + fileName);
            Environment.Exit(0);
            throw new NotImplementedException();
        }

        private void PictureBoxDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int position = int.Parse(((PictureBox)sender).Name.Substring("pictureBox".Length)) - 1;
                fields[position].IncrementTaskNumber();
                string path = fields[position].GetCurrentPath();
                pictureBoxes[position].Image = LoadPic(path);
                string title = position + 1 + ": " + fields[position].Title;
                textBoxes[position].Text = title;

                ShowBigPic(title, fields[position].GetCurrentTask(), path);
            }
        }

        private void BigPic_FormClosed(object sender, FormClosedEventArgs e)
        {
            Enabled = true;
        }

        private void PictureBoxClick(object sender, MouseEventArgs e)
        {
            int position = int.Parse(((PictureBox)sender).Name.Substring("pictureBox".Length)) - 1;

            if (e.Button == MouseButtons.Left)
            {
                int picQuarter = (e.Location.X / (picWidth / 2)) + (e.Location.Y / (picHeight / 2)) * 2;

                if (players.Count > picQuarter && players[picQuarter].Position == position)
                {//click on a player happened -> it gets selected (even if click on already selected)
                    selectedPlayer = picQuarter;
                }
                else if (selectedPlayer != null && players[(int)selectedPlayer].Position != position)
                {//if a player is selected and the click did not occur on a player but on a field, where the player could go-> move player
                    players[(int)selectedPlayer].MoveTo(position);
                    selectedPlayer = null;
                }
                else
                {
                    selectedPlayer = null;
                }

                TriggerRedrawPlayers();
            }
            else
            {//right click -> enlarge pic, if not hidden
                if (!fields[position].IsHidden())
                    ShowBigPic((position + 1) + ": " + fields[position].Title, fields[position].GetCurrentTask(), fields[position].GetCurrentPath());
            }
        }

        private void ShowBigPic(string title, string task, string path)
        {
            var bigPic = new BigPic(title, task, path);
            bigPic.FormClosed += BigPic_FormClosed;
            Enabled = false;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {//new player requested
                var capitalLetter = e.KeyChar.ToString().ToUpper()[0];
                bool playerExists = false;

                //check if player exists
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Letter == capitalLetter)
                    {
                        selectedPlayer = i;
                        playerExists = true;
                        TriggerRedrawPlayers();
                        break;
                    }
                }

                if (!playerExists && players.Count < playerRectangles.Length)
                {//new player to be created
                    players.Add(new Player(capitalLetter));
                    UpdateDrinkCounters();
                    TriggerRedrawPlayers();
                }
            }
            else if (e.KeyChar == (char)Keys.Back && selectedPlayer != null)
            {
                if (MessageBox.Show("Tényleg törölni szeretnéd " + players[(int)selectedPlayer].Letter + " játékost?", "Játékos törlése", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {//delete player (also from board)
                    players.RemoveAt((int)selectedPlayer);
                    UpdateDrinkCounters();
                    TriggerRedrawPlayers();
                }
            }
            else if (e.KeyChar == '-' && selectedPlayer != null)
            {
                players[(int)selectedPlayer].Undrink();
                UpdateDrinkCounters();
                //playerTextBoxes[(int)selectedPlayer].Text = players[(int)selectedPlayer].Drinks.ToString();
            }
            else if (e.KeyChar == '+' && selectedPlayer != null)
            {
                players[(int)selectedPlayer].Drink();
                UpdateDrinkCounters();
                //playerTextBoxes[(int)selectedPlayer].Text = players[(int)selectedPlayer].Drinks.ToString();
            }
        }

        private void UpdateDrinkCounters()
        {
            for (int i = 0; i < playerTextBoxes.Length; i++)
            {
                if (players.Count > i)
                {
                    playerTextBoxes[i].BackColor = playerPens[i].Color;
                    playerTextBoxes[i].Text = players[i].Drinks.ToString();
                }
                else
                {
                    playerTextBoxes[i].BackColor = default;
                    playerTextBoxes[i].Text = "";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            button1.Text = BTN_TEXT_BASE + (r.Next(6) + 1);

            if (button1.BackColor == Color.White)
            {
                button1.BackColor = Color.Black;
                button1.ForeColor = Color.White;
            }
            else
            {
                button1.BackColor = Color.White;
                button1.ForeColor = Color.Black;
            }
        }

        private void TriggerRedrawPlayers()
        {
            foreach (PictureBox pb in pictureBoxes)
                pb.Invalidate();
        }

        private class Field
        {
            private int taskNumber = -1;//means that it's hidden
            public readonly List<int> TaskNumbers = new List<int>();
            public string Title { get; set; }
            public readonly List<string> Tasks = new List<string>();
            public readonly List<string> Paths = new List<string>();

            private int GetIDX()
            {
                int idx;

                for (idx = 0; idx < TaskNumbers.Count; idx++)
                {
                    if (TaskNumbers[idx] - 1 == taskNumber)
                        break;
                }

                return idx;
            }

            public string GetCurrentTask() { return Tasks[GetIDX()]; }
            public string GetCurrentPath() { return Paths[GetIDX()]; }
            public void IncrementTaskNumber() { taskNumber = Math.Min(taskNumber + 1, TaskNumbers.Count - 1); }
            public bool IsHidden() { return taskNumber == -1; }
        }

        private class Player
        {
            public char Letter { get; private set; }
            public int Position { get; private set; } = 0;
            public int Drinks { get; private set; } = 0;

            public Player(char letter)
            {
                Letter = letter;
            }

            public void MoveTo(int position)
            {
                Position = position;
            }

            public void Drink()
            {
                Drinks++;
            }

            public void Undrink()
            {
                Drinks = Math.Max(0, Drinks - 1);
            }
        }
    }
}
