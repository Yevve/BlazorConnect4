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
        public float AIScore = 0F;
        private int numberOfReps = 0;

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
        // go through the meamory and search for the right column to put down the piece. Seems like it is not going in here. Maybe needs rewards to make it work.
        public double searchInBrain(Cell[,] grid, int column)
        {
            Random random = new Random();
            
            String gridString = GameBoard.GridToString(grid);
            if (!brainDict.ContainsKey(gridString)){

                double[] randomMove ={random.NextDouble(),random.NextDouble(),random.NextDouble(),random.NextDouble(),random.NextDouble(),random.NextDouble(),random.NextDouble()};
                brainDict.Add(gridString, randomMove);
                return randomMove[0];
                Console.WriteLine(randomMove);// does not print shit 
            }

            return brainDict[gridString][column];
        }
       public void brainTrainingCamp(QAgent agent, int iterations)
        {
            newGameEngine brainTrainingEngine = new newGameEngine();
            Cell[,] grid = brainTrainingEngine.Board.Grid;

            CellColor player = brainTrainingEngine.Player;
            CellColor opponentAgent = agent.playerColor;
            CellColor playerTurn = CellColor.Red;

            int move = 0;
            int previousMove = move;

            if (playerTurn == player)
            {
                move = SelectMove(grid);
                previousMove = move;
            }
            else
            {
                move = agent.SelectMove(grid);
            }
            if (!brainTrainingEngine.IsValid(grid, move))
            {
                if(playerTurn == player)
                {
                    //TODO: Update memory
                }
                
            }
            brainTrainingEngine.Play(move, playerTurn);

            if (brainTrainingEngine.IsWin(move, playerTurn))
            {
                //TODO: Update memory
                //Knowledge ++
                Console.WriteLine("Win");
            }
            else
            {
                //TODO: Update memory
                //Knowledge --
                Console.WriteLine("Lose");
            }
            if (brainTrainingEngine.IsDraw())
            {
                //Break
                Console.WriteLine("Draw");
            }
            
        }
    }
}
