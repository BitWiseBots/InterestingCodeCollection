using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip
{
    public class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.InitGame();

            //prompt for new game
        }
    }

    public class Game
    {
        private readonly int _boardSize = 8;
        private Board _playerA;
        private Board _playerB;

        public void InitGame()
        {
            _playerA = new Board(_boardSize);
            _playerB = new Board(_boardSize);

            Setup();
            Start();
        }

        private void Setup()
        {
            //Prompt for player

            //Prompt for placement coords
            //Confirm

            //Next player
        }

        private void Start()
        {
            var currentPlayer = _playerA;
            var target = _playerB;
            while (true)
            {
                try
                {
                    ProcessTurn(target);

                }
                catch (Exception)
                {
                    //display error
                    continue;
                }

                if (target.IsShipRemaining())
                {
                    var temp = target;
                    target = currentPlayer;
                    currentPlayer = temp;

                    //Prompt for switch
                }
                else
                {
                    //print victory
                }
            }
        }

        private void ProcessTurn(Board target)
        {
            //Print target board
            //prompt player for coordinate
            //dispaly result
        }
    }

    public class ConsolePresenter
    {
        public void PrintBoard(Board board)
        {
            Console.Clear();

            //still need to print border
            for (var x = 0; x < board.SideLength; x++)
            {
                for (var y = 0; y < board.SideLength; y++)
                {
                    var cellValue = board[x, y];

                    switch (cellValue)
                    {
                        case CellState.Ship:
                        case CellState.Blank:
                            Console.Write(" ");
                            break;
                        case CellState.Miss:
                            Console.Write("o");
                            break;
                        case CellState.Hit:
                            Console.Write("x");
                            break;
                    }
                }
                Console.Write(Environment.NewLine);
            }

        }

        public void PrintShotResult(bool hit)
        {
            
        }

        public void PrintError(string message)
        {
            
        }

    }

    public class Board
    {
        private readonly CellState[,] _boardArray;

        public Board(int boardSize)
        {
            SideLength = boardSize;
            _boardArray = new CellState[SideLength,SideLength];
        }

        public void PlaceShip(string startCoord, string endCoord)
        {
            
        }

        public void RecordShot(string coord)
        {
            switch (this[coord])
            {
                case CellState.Blank:
                    this[coord] = CellState.Miss;
                    break;
                case CellState.Ship:
                    this[coord] = CellState.Hit;
                    break;
                default:
                    throw new ArgumentException($"Entered Coordinates have already been attempted: {coord}");
            }
        }

        public bool IsShipRemaining()
        {
            for (var x = 0; x < SideLength; x++)
                for (var y = 0; y < SideLength; y++)
                {
                    if (_boardArray[x, y] != CellState.Ship) continue;
                    return true;
                }
            return false;
        }

        public int SideLength { get; }

        public CellState this[int x, int y] => _boardArray[x, y];

        private CellState this[string coord]
        {
            get
            {
                var coords = GetPoint(coord);
                return this[coords.Item1, coords.Item2];
            }
            set
            {
                var point = GetPoint(coord);
                _boardArray[point.Item1, point.Item2] = value;
            }
        }

        private Tuple<int,int> GetPoint(string coord)
        {
            var xPos = GetIndexFromLetter(coord[0]);
            var yPos = GetIndexFromNumber(coord[1]);

            if (!IsValidCoord(xPos) || !IsValidCoord(yPos)) throw new ArgumentException($"Entered Coordinates were invalid: {coord}");

            return new Tuple<int,int>(xPos,yPos);
        }
        

        private int GetIndexFromLetter(char letter)
        {
            return char.ToUpper(letter) - 64;
        }

        private int GetIndexFromNumber(char number)
        {
            return number - '0';
        }

        private bool IsValidCoord(int coord)
        {
            return coord >= 0 && coord < SideLength;
        }
    }

   

    public enum CellState
    {
        Blank,
        Ship,
        Hit,
        Miss
    }
}
