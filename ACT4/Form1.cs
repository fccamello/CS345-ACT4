using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        //SixState currentState;

        SixState[] states;

        int moveCounter;
        int stateNum;
        int bestStateIdx;


        //bool stepMove = true;

        int[,,] hTable;
        ArrayList[] bMoves;
        Object[] chosenMove;

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            startState = randomSixState();
            //currentState = new SixState(startState);
            stateNum = 3;
            bestStateIdx = 0;

            states = new SixState[stateNum];
            states[0] = new SixState(startState);
            states[1] = randomSixState();
            states[2] = randomSixState();

            chosenMove = new object[stateNum];

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void updateUI()
        {
            //pictureBox1.Refresh();
            pictureBox2.Refresh();

            //label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            //label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label3.Text = "Attacking pairs: " + getAttackingPairs(states[bestStateIdx]);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(states);
            bMoves = getBestMoves(hTable);


            listBox1.Items.Clear();
            for (int i = 0; i < stateNum; i++)
            {
                if (bMoves[i].Count > 0)
                {
                    chosenMove[i] = chooseMove(bMoves[i]);
                }
            }

            foreach (Point move in bMoves[bestStateIdx])
            {
                listBox1.Items.Add(move);
            }
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == states[bestStateIdx].Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[,,] getHeuristicTableForPossibleMoves(SixState[] thisState)
        {
            int[,,] hStates = new int[stateNum, n, n];

            for (int i = 0; i < stateNum; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    for (int k = 0; k < n; k++)
                    {
                        SixState possible = new SixState(thisState[i]);
                        possible.Y[j] = k;
                        hStates[i, j, k] = getAttackingPairs(possible);
                    }
                }
            }

            return hStates;
        }

        private ArrayList[] getBestMoves(int[,,] heuristicTable)
        {
            ArrayList[] bestMoves = new ArrayList[stateNum];

            for (int i = 0; i < stateNum; i++)
            {
                bestMoves[i] = new ArrayList();
            }

            int[] bestHeuristicValue = new int[stateNum];


            for (int i = 0; i < stateNum; i++)
            {
                bestHeuristicValue[i] = heuristicTable[i, 0, 0];

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (bestHeuristicValue[i] > heuristicTable[i, j, k])
                        {
                            bestHeuristicValue[i] = heuristicTable[i, j, k];
                            bestMoves[i].Clear();
                            if (states[i].Y[j] != k)
                                bestMoves[i].Add(new Point(j, k));
                        }

                        else if (bestHeuristicValue[i] == heuristicTable[i, j, k])
                        {
                            if (states[i].Y[j] != k)
                                bestMoves[i].Add(new Point(j, k));
                        }
                    }
                }
            }

            for (int i = 0; i < stateNum; i++)
            {
                if (bestHeuristicValue[bestStateIdx] > bestHeuristicValue[i])
                {
                    bestStateIdx = i;
                }
            }

            label5.Text = "Possible Moves (H=" + bestHeuristicValue[bestStateIdx] + ")";
            return bestMoves;
        }

        private Object chooseMove(ArrayList possibleMoves)
        {
            int arrayLength = possibleMoves.Count;

            if (possibleMoves.Count == 0)
                return null;

            Random r = new Random();
            int randomMove = r.Next(arrayLength);

            return possibleMoves[randomMove];
        }

        private void executeMove(Point move)
        {
            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = states[bestStateIdx].Y[i];
            }
            states[bestStateIdx].Y[move.X] = move.Y;
            moveCounter++;

            for (int i = 0; i < stateNum; i++)
            {
                chosenMove[i] = null;
            }
            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(states[bestStateIdx]) > 0)
                executeMove((Point)chosenMove[bestStateIdx]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            states[0] = startState = randomSixState();
            //currentState = new SixState(startState);

            for (int i = 0; i < stateNum; i++)
            {
                states[i] = new SixState();
            }

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(states[bestStateIdx]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(states[bestStateIdx]) > 0)
            {
                for (int i = 0; i < stateNum; i++)

                {
                    executeMove((Point)chosenMove[i]);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
