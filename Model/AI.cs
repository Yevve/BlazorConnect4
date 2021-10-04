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
        [NonSerialized] Random generator;

        //Piska eller morot
        public float goodBoy = 1;
        public float badBoy = -1;
        public float invalidMove = -0.1F;
        public float AIScore = 0F;
        public float numIOfRuns = 0;
        private CellColor playerColor;
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

        public override int SelectMove(Cell[,] grid)
        {
            float epsilon = 0.85F;

            int move =  epsilionEvaluation(grid,epsilon);

            return move;

        }
        public int epsilionEvaluation(Cell[,] grid,float epsilon)
        {
            Random randomValue = new Random();

            if (randomValue.NextDouble() < epsilon)
            {
                // make a random move.
                int currentMove = randomValue.Next(7);
                while(!newGameEngine.IsValid(grid, currentMove))
                {
                    // repeat random move is outside the grid.
                    currentMove = randomValue.Next(7);
                }
                return currentMove;
            }
            else
            {
                // search in brainDict for old moves that were best in this grid varient.
            }

            return 0;
        }

        public bool isValid(int col, Cell[,] grid)
        {
            bool isValid = grid[col, 0].Color == CellColor.Blank;
            return isValid;
        }
    }
}
