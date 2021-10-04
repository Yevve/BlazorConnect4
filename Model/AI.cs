using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }

    }



    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }

        public static RandomAI ConstructFromFile(string fileName)
        {
            RandomAI temp = (RandomAI)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            temp.generator = new Random();
            return temp;
        }
    }

    [Serializable]
    public class QAgent : AI
    {

        private CellColor playerColor;
        //Piska eller morot
        public float goodBoy = 1F;
        public float badBoy = -1F;
        public float invalidMove = -0.1F;
        public float mediocreMove = 0.1F;
        public float AIScore = 0F;
        private int numberOfReps = 0;
        public int wins;
        public int losses;
        public int draws;

        Dictionary<String, double[]> brainDict = new Dictionary<string, double[]>();

        public QAgent(CellColor playerColor)
        {
            this.playerColor = playerColor;

        }
        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            return temp;
        }


        /**
         * PARAM: Cell[,] grid
         * RETURN:int action
         * 
         * Takes in a position on the board and returns a valid move.
         */
        public override int SelectMove(Cell[,] grid)
        {
            //Maybe use a seed?
            double epsilon = 0.85F;
            //Check if AI should do an epsilon move or a random move
            Random randomGen = new Random();

            int randomAction1 = randomGen.Next(0, 7);
            //While there are no valid moves, make a random move and validate it
            if (randomGen.NextDouble() < epsilon)
            {
                int randomAction = randomGen.Next(0, 7);
                while (!GameEngine.IsValid(grid, randomAction))
                {
                    randomAction = randomGen.Next(0, 7);
                }
                return randomAction;
            }
            else
            {
                int column = 0;
                double previusColumnValue = searchInBrain(grid, column);

                for (int i = 1; i < 7; i++)
                {
                    double nextColumnValue = searchInBrain(grid, i);
                    if (previusColumnValue < nextColumnValue)
                    {
                        column = i;
                        previusColumnValue = nextColumnValue;
                    }

                }
                return column;
            }

        }
        // go through the brain and search for the right column to put down the piece.
        public double searchInBrain(Cell[,] grid, int column)
        {
            Random random = new Random();

            String gridString = GameBoard.GridToString(grid);
            if (!brainDict.ContainsKey(gridString))
            {

                double[] randomMove = { random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble() };
                brainDict.Add(gridString, randomMove);
                return randomMove[0];
            }

            return brainDict[gridString][column];
        }
        // add rewards to brain
        public void updateBrain(Cell[,] grid, int column, float reward)
        {
            Random random = new Random();

            String gridString = GameBoard.GridToString(grid);
            if (!brainDict.ContainsKey(gridString))
            {

                double[] randomMove = { random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble(), random.NextDouble() };
                brainDict.Add(gridString, randomMove);
            }
            brainDict[gridString][column] = reward;
        }
        public void brainTrainingCamp(QAgent agent, int iterations)
        {
            newGameEngine brainTrainingEngine = new newGameEngine();
            for (int i = 0; i < iterations; i += 1)
            {
                brainTrainingEngine.Reset();

                Cell[,] grid = brainTrainingEngine.Board.Grid;
                CellColor player = brainTrainingEngine.Player;
                CellColor playerTurn = CellColor.Red;
                int move;
                int previousMove;

                while (i < iterations)
                {
                    if (playerTurn == player)
                    {
                        move = SelectMove(grid);
                        previousMove = move;
                    }
                    else
                    {
                        //Oppnent makes a move
                        move = agent.SelectMove(grid);
                    }
                    if (!brainTrainingEngine.IsValid(grid, move))
                    {
                        if (playerTurn == player)
                        {
                            //Invalid move
                            updateBrain(grid, move, invalidMove);
                            continue;
                        }

                    }
                    brainTrainingEngine.Play(move, playerTurn);

                    if (brainTrainingEngine.IsWin(move, playerTurn))
                    {
                        if (playerTurn == player)
                        {
                            //Knowledge ++
                            Console.WriteLine("Win");
                            updateBrain(grid, move, goodBoy);
                            wins += 1;
                            break;
                        }
                        else
                        {
                            //Knowledge --
                            Console.WriteLine("Lost");
                            updateBrain(grid, move, badBoy);
                            losses += 1;
                            break;
                        }


                    }
                    if (brainTrainingEngine.IsDraw())
                    {
                        //small reward for atleaset trying :)
                        updateBrain(grid, move, mediocreMove);
                        Console.WriteLine("Draw");
                        draws += 1;
                        break;
                    }
                    playerTurn = brainTrainingEngine.ChangePlayer(playerTurn);
                }

                Console.WriteLine(playerTurn);
                Console.WriteLine("Studies complete");
                Console.Write("Wins: " + wins + ", Losses: " + losses + ", Draws: " + draws + "  ");
            }
        }
    }
}
