////
//// baha'a alsharif and 4 others
//// 2015-12-29
//// 
////
////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;


namespace ConnectFour
{
    #region Form's Code
    public partial class Form1 : Form
    {
        GameGrid g;
        public Form1()
        {
            InitializeComponent();
            g = new GameGrid(panel1);
            button1_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.Initialize((int)numericUpDown1.Value, (int)numericUpDown2.Value, radioButton3.Checked, radioButton1.Checked);

            if (radioButton5.Checked)
                g.InitializeTwoCpuPlayers((int)numericUpDown4.Value, (int)numericUpDown5.Value);
        }
        private void panel1_Resize(object sender, EventArgs e) { g.UpdatePaint(); }
        private void Form1_Move(object sender, EventArgs e) { g.UpdatePaint(); }
        private void panel1_MouseMove(object sender, MouseEventArgs e) { g.Hover(e.X); }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            g.MouseDown(e.X);
        }
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            g.MouseUp();
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Visible = radioButton3.Checked;
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            g.ai._DEPTH = (int)numericUpDown3.Value;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Visible = radioButton3.Checked;
            groupBox4.Visible = radioButton5.Checked;

            button1_Click(null, null);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            g.ai._DEPTH = (int)numericUpDown4.Value;
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            g.ai2._DEPTH = (int)numericUpDown5.Value;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            g.NEXT();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.FINISH();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }
    }

    #endregion


    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

                                 #region Game Structure and AI player


    public enum GameStatus { NotInitialized = 0, YellowTurn = -1, RedTurn = 1, BoardFull = 2, YellowWin = 3, RedWin = 4 }
    public static class Cell { public static readonly int YELLOW = -1, RED = 1, EMPTY = 0; }

    public class GameGrid
    {
        public AIPlayer ai;
        public AIPlayer ai2;
        public bool vsCpu = true;
        public GameStatus Status;

        private int[,] columns { get; set; }

        public int this[int i, int j] { set { columns[i, j] = value; } get { return columns[i, j]; } }

        public Panel graphics { get; set; }
        private Pen linePen, thinPen;

        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public GameGrid(Panel g)
        {
            graphics = g;
            linePen = new Pen(Brushes.Black, 3);
            thinPen = new Pen(Brushes.Black);
            Status = GameStatus.NotInitialized;
        }

        public GameGrid Initialize(int rows, int cols, bool vscpu = true, bool humanRed = true)
        {
            Rows = rows;
            Cols = cols;

            columns = new int[rows, cols];

            Status = GameStatus.RedTurn;

            UpdatePaint();

            vsCpu = vscpu;
            ai = new AIPlayer(this, humanRed ? Cell.YELLOW : Cell.RED);

            if (!humanRed)
                aiplay();

            lockgame = false;
            return this;
        }
        public GameGrid InitializeTwoCpuPlayers(int depth1, int depth2)
        {
            ai = new AIPlayer(this, Cell.RED) { _DEPTH = depth1 };
            ai2 = new AIPlayer(this, Cell.YELLOW) { _DEPTH = depth2 };

            return NEXT();
        }

        public GameGrid NEXT()
        {
            switch (Status)
            {
                case GameStatus.YellowTurn:
                    play(ai.Play());
                    break;
                case GameStatus.RedTurn:
                    play(ai2.Play());
                    break;
            }
            UpdatePaint();
            lockgame = true;
            return this;
        }
        public GameGrid FINISH()
        {
            while (Status == GameStatus.YellowTurn || Status == GameStatus.RedTurn)
                NEXT();
            return this;
        }


        public bool lockgame = false;
        private bool mouseDown = false;
        private int lastX = -1;
        public GameGrid Hover(int x)
        {
            if (lockgame)
                return this;

            x = getCol(x);
            if (x != lastX)
                UpdatePaint(x);
            lastX = x;
            return this;
        }
        public GameGrid MouseDown(int x)
        {
            if (lockgame)
                return this;

            mouseDown = true;
            x = getCol(x);
            UpdatePaint(x);
            return this;
        }
        public GameGrid MouseUp()
        {
            if (lockgame)
                return this;

            mouseDown = false;
            Play(lastX);
            UpdatePaint(lastX);
            return this;
        }



        private int getCol(int x)
        {
            if (Status == GameStatus.NotInitialized)
                return -1;
            return (int)Math.Floor((decimal)(x / (graphics.Width / Cols)));
        }

        public void Play(int col)
        {
            if (play(col))
                aiplay();
        }

        private void aiplay()
        {
            if (vsCpu && !(Status >= GameStatus.BoardFull))
                play(ai.Play());
        }

        public bool play(int col)
        {
            if (!(col != -1 && col < Cols))
                return false;

            int i = Rows - 1;
            for (; i >= 0; i--)
                if (columns[i, col] == Cell.EMPTY)
                {
                    if (Status == GameStatus.YellowTurn)
                    {
                        columns[i, col] = Cell.YELLOW;
                        Status = GameStatus.RedTurn;
                        break;
                    }
                    else if (Status == GameStatus.RedTurn)
                    {
                        columns[i, col] = Cell.RED;
                        Status = GameStatus.YellowTurn;
                        break;
                    }
                }

            if (i == -1)
                return false;

            if(isBoardFull())
                Status = GameStatus.BoardFull;

            if (isWinner(i, col, columns[i, col]))
            {
                if (columns[i, col] == Cell.RED)
                    Status = GameStatus.RedWin;
                else
                    Status = GameStatus.YellowWin;
            }

            switch (Status)
            {
                case GameStatus.BoardFull:
                    MessageBox.Show("Board is full, No winner");
                    Status = GameStatus.NotInitialized;

                    break;
                case GameStatus.YellowWin:
                    MessageBox.Show("Yellow Win");
                    Status = GameStatus.NotInitialized;

                    break;
                case GameStatus.RedWin:
                    MessageBox.Show("Red Win");
                    Status = GameStatus.NotInitialized;

                    break;
            }
            return true;
        }

        public bool isWinner(int i, int j, int player)
        {
            return VerticalOrHorizontalWin(i,j,player) || DiagonallyWin(player);
        }
        public bool VerticalOrHorizontalWin(int i, int j, int player)
        {
            int counter;

            counter = 0;
            for (int t = 0; t < Rows; t++)
                if (columns[t, j] == player)
                {
                    if (++counter >= 4)
                        return true;
                }
                else
                    counter = 0;

            counter = 0;
            for (int t = 0; t < Cols; t++)
                if (columns[i, t] == player)
                {
                    if (++counter >= 4)
                        return true;
                }
                else
                    counter = 0;

            return false;
        }

        public bool DiagonallyWin(int player)
        {
            for (int x = Rows - 1; x > 2; x--)
                for (int y = Cols - 1; y > 2; y--)
                {
                    if (columns[x, y] == player &&
                        columns[x - 1, y - 1] == player &&
                        columns[x - 2, y - 2] == player &&
                        columns[x - 3, y - 3] == player)
                        return true;
                }
            for (int x = Rows - 1; x > 2; x--)
                for (int y = 0; y < 4; y++)
                    if (columns[x, y] == player &&
                        columns[x - 1, y + 1] == player &&
                        columns[x - 2, y + 2] == player &&
                        columns[x - 3, y + 3] == player)
                        return true;
            return false;
        }

        bool isBoardFull()
        {
            for (int j = 0; j < columns.GetLength(1); j++)
                if (columns[0, j] == Cell.EMPTY)
                    return false;
            return true;
        }


        int freezeAfterFinish = 0;
        public GameGrid UpdatePaint(int column = -1)
        {
            if (Status == GameStatus.NotInitialized)
                if (freezeAfterFinish++ > 2)
                {
                    Initialize(Rows, Cols, vsCpu, ai.myColor == Cell.YELLOW);
                    freezeAfterFinish = 0;
                }

            Graphics g = graphics.CreateGraphics();
            int cellWidth = graphics.Width / Cols;
            int cellHeight = graphics.Height / Rows;

            // clear every thing
            g.FillRectangle(Brushes.White, 0, 0, graphics.Width, graphics.Height);

            // draw mouse hover effect
            if (column != -1 && column < Cols)
            {
                if (mouseDown)
                    g.FillRectangle(Brushes.Orange, column * cellWidth, 0, cellWidth, graphics.Height);
                else
                    g.FillRectangle(((Status == GameStatus.RedTurn) ? Brushes.Tomato : Brushes.Yellow), column * cellWidth, 0, cellWidth, graphics.Height);
            }

            // draw cirules
            int margin = 4;
            for (int i = 0; i < columns.GetLength(0); i++)
                for (int j = 0; j < columns.GetLength(1); j++)
                    if (columns[i, j] == Cell.RED)
                    {
                        g.FillEllipse(Brushes.Red, j * cellWidth + margin, i * cellHeight + margin, cellWidth - margin * 2, cellHeight - margin * 2);
                        g.DrawEllipse(thinPen, j * cellWidth + margin, i * cellHeight + margin, cellWidth - margin * 2, cellHeight - margin * 2);
                    }
                    else if (columns[i, j] == Cell.YELLOW)
                    {
                        g.FillEllipse(Brushes.Yellow, j * cellWidth + margin, i * cellHeight + margin, cellWidth - margin * 2, cellHeight - margin * 2);
                        g.DrawEllipse(thinPen, j * cellWidth + margin, i * cellHeight + margin, cellWidth - margin * 2, cellHeight - margin * 2);
                    }


            // draw grids columns
            for (int i = 0; i <= Cols; i++)
                g.DrawLine(linePen, i * cellWidth, 0, i * cellWidth, cellHeight * Rows);

            // draw grids rows
            for (int i = 0; i <= Rows; i++)
                g.DrawLine(linePen, 0, i * cellHeight, cellWidth * Cols, i * cellHeight);


            return this;
        }
    }



    public class AIPlayer
    {
        public int _DEPTH = 3;
        Stopwatch watch = new Stopwatch();

        Random random = new Random();
        GameGrid game;
        public int myColor = Cell.YELLOW;
        public AIPlayer(GameGrid g, int mycolor)
        {
            game = g;
            myColor = mycolor;
        }

        List<int> GetAvailableMoves()
        {
            List<int> res = new List<int>();
            for (int j = 0; j < game.Cols; j++)
                for (int i = game.Rows - 1; i >= 0; i--)
                    if (game[i, j] == Cell.EMPTY)
                    {
                        res.Add(j);
                        break;
                    }
            return res;
        }

        string taps = "";
        int MOVE_NUMBER = 0;
        int MinMax(int depth, int r, int c, int player, ref int Index)
        {
            if (depth <= 0)
                return evaluation(r, c, player);

            Console.WriteLine(taps + "Deth = {0}, Player = {1}", depth, (isItMyTurn(player) ? "YLW" : "RED"));  // this for debugging

            List<int> moves = GetAvailableMoves();
            int bestMove = int.MinValue;
            int row, col, val, j;
            while (moves.Count > 0)
            {
                j = random.Next(moves.Count);

                col = moves[j];
                row = getRowfromColumn(col);

                if (_DEPTH == depth && game.isWinner(row, col, myColor) && (Index = col) >= 0)
                {
                    Console.WriteLine(taps.Remove(taps.Length - 1) + "Best move = " + int.MaxValue + " Col = " + Index); // this for debugging
                    return int.MaxValue;
                }

                recordTheMove(row, col, player);

                taps += "\t"; // this for debugging
                
                val = MinMax(depth - 1, row, col, Not(player), ref Index);
                taps = taps.Remove(taps.Length - 1); // this for debugging
                Console.WriteLine(taps + "Col = {0}, Val = {1}", col, val); // this for debugging

                unrecordTheMove(row, col);


                if (val > bestMove)
                {
                    bestMove = val;
                    Index = col;
                }
                moves.RemoveAt(j);
            }

            Console.WriteLine(taps + "Best move = " + bestMove + " Col = " + Index); // this for debugging
            return bestMove;
        }

        bool isItMyTurn(int player)
        {
            return player == myColor;
        }

        int evaluation(int i, int j, int player)
        {
            recordTheMove(i, j, player);

            int value = 0;

            if (game.isWinner(i, j, player))
                value = int.MaxValue;
            else if (game.isWinner(i, j, Not(player)))
                value = int.MinValue;
            else
              value = NumberOfThreesAndTwos(player) - NumberOfThreesAndTwos(Not(player)); //value = NumberOfThreesAndTwos(myColor) - NumberOfThreesAndTwos(Not(myColor));

            unrecordTheMove(i, j);

            return value;
        }

        int NumberOfThreesAndTwos(int player)
        {
            int count_3 = 0;
            int count_2 = 0;
            int tmp;

            // Vertical
            for (int j = 0, space; j < game.Cols; j++)
            {
                tmp = space = 0;
                for (int i = 0; i < game.Rows; i++)
                {
                    if (game[i, j] == Not(player))
                        break;
                    else if (game[i, j] == player)
                    {
                        if (space == 0)
                            break;
                        if (++tmp == 2 && space >= 2)
                            count_2++;
                        else if (tmp == 3 && space >= 1)
                        {
                            count_2--;
                            count_3++;
                            break; // bcz we already get the three so no need for search in this column anymore
                        }
                    }
                    else
                        space++;
                }
            }

            // Horizantal
            int uselessThreeBecauseThereIsGapUnderIt = 0;
            for (int i = 0, spaces, blocked, oldtmp; i < game.Rows; i++)
            {
                tmp = spaces = blocked = oldtmp = 0;
                for (int j = 0; j < game.Cols; j++)
                {
                    if (game[i, j] == Not(player))
                    {
                        if (blocked == 2 && oldtmp == 2)
                            count_2--;
                        else if (blocked == 3 && oldtmp == 3)
                            count_3--;

                        oldtmp = tmp = spaces = blocked = 0;
                    }
                    else if (game[i, j] == player)
                    {
                        if (spaces > 1)
                            tmp = spaces = 0;

                        blocked++;
                        oldtmp = tmp + 1;
                        if (++tmp == 2)
                            count_2++;
                        else if (tmp == 3)
                        {
                            count_2--;
                            count_3++;
                            tmp = 0;
                        }
                    }
                    else
                    {
                        spaces++;
                        blocked++;
                        if (tmp >= 2 && j + 1 < game.Cols && game[i, j + 1] == player && i + 1 < game.Rows && game[i + 1, j] != Cell.EMPTY)
                            uselessThreeBecauseThereIsGapUnderIt++;
                    }
                }
            }
            //count_3 -= uselessThreeBecauseThereIsGapUnderIt;


            // Diagonal
            for (int x = game.Rows - 1; x > 2; x--)
            {
                for (int y = game.Cols - 1; y > 2; y--)
                    if (game[x, y] == player &&
                        game[x - 1, y - 1] == player &&
                        game[x - 2, y - 2] == player &&
                        game[x - 3, y - 3] == Cell.EMPTY)
                        count_3++;
                    else if (game[x, y] == player &&
                        game[x - 1, y - 1] == player &&
                        game[x - 2, y - 2] == Cell.EMPTY &&
                        game[x - 3, y - 3] == Cell.EMPTY)
                        count_2++;

                for (int y = 0; y < 4; y++)
                    if (game[x, y] == player &&
                        game[x - 1, y + 1] == player &&
                        game[x - 2, y + 2] == player)
                        count_3++;
                    else if (game[x, y] == player &&
                        game[x - 1, y + 1] == player &&
                        game[x - 2, y + 2] == Cell.EMPTY &&
                        game[x - 3, y + 3] == Cell.EMPTY)
                        count_2++;
            }

            return (count_2 * 10) + (count_3 * 1000); ;
        }

        private void recordTheMove(int i, int j, int player)
        {
            game[i, j] = player;
        }
        private void unrecordTheMove(int i, int j)
        {
            game[i, j] = Cell.EMPTY;
        }
        private int getRowfromColumn(int j)
        {
            int i = game.Rows - 1;
            for (; i >= 0; i--)
                if (game[i, j] == Cell.EMPTY)
                    break;
            return i;
        }

        public int Play()
        {
            game.lockgame = true;
            int ColumnIndexOfBestMoveOfMinMaxAlgorithm = -1;

            Console.WriteLine("\n" + ++MOVE_NUMBER + "\t-----------------------------------------------");
            int x = MinMax(_DEPTH, 0,0, myColor,ref ColumnIndexOfBestMoveOfMinMaxAlgorithm);
            Console.WriteLine("BEST MOVE = " + x + "  COL = " + ColumnIndexOfBestMoveOfMinMaxAlgorithm);

            game.lockgame = false;
            return ColumnIndexOfBestMoveOfMinMaxAlgorithm;
        }

        public int Not(int myColor)
        {
            if (myColor == Cell.RED)
                return Cell.YELLOW;
            return Cell.RED;
        }
    }


                                        #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
}