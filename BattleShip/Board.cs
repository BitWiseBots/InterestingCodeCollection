using System;

namespace BattleShip
{
    public class Board : IBoard
    {
        private readonly CellState[,] _boardArray;

        public Board(int boardSize, string playerName)
        {
            SideLength = boardSize;
            PlayerName = playerName;
            _boardArray = new CellState[SideLength,SideLength];
        }

        public void PlaceShip(string startCoord, string endCoord)
        {
            var startPoint = GetPoint(startCoord);
            var endPoint = GetPoint(endCoord);

            if (startPoint.Item1 != endPoint.Item1 && startPoint.Item2 != endPoint.Item2)
            {
                throw new ArgumentException($"Entered Ship Placement Coordinates cannot be on a diagonal: {startCoord} {endCoord}");
            }

            if (Math.Abs(startPoint.Item1 - endPoint.Item1) != 2 && Math.Abs(startPoint.Item2 - endPoint.Item2) != 2)
            {
                throw new ArgumentException($"Entered Ship Placement Coordinates must be 2 spaces apart: {startCoord} {endCoord}");
            }
            
            _boardArray[startPoint.Item1,startPoint.Item2] = CellState.Ship;
            _boardArray[endPoint.Item1, endPoint.Item2] = CellState.Ship;

            if (startPoint.Item1 != endPoint.Item1)
            {
                if (startPoint.Item1 < endPoint.Item1)
                {
                    _boardArray[startPoint.Item1 + 1, startPoint.Item2] = CellState.Ship;
                }
                else
                {
                    _boardArray[startPoint.Item1 - 1, startPoint.Item2] = CellState.Ship;
                }
            }
            else
            {
                if (startPoint.Item2 < endPoint.Item2)
                {
                    _boardArray[startPoint.Item1, startPoint.Item2 + 1] = CellState.Ship;
                }
                else
                {
                    _boardArray[startPoint.Item1, startPoint.Item2 - 1] = CellState.Ship;
                }
            }
        }

        public void ClearShips()
        {
            for(var x = 0; x < SideLength; x++)
                for (var y = 0; y < SideLength; y++)
            {
                if (_boardArray[x, y] == CellState.Ship)
                {
                        _boardArray[x, y] = CellState.Blank;
                }
            }
        }

        public bool TakeShot(string coord)
        {
            switch (this[coord])
            {
                case CellState.Blank:
                    this[coord] = CellState.Miss;
                    return false;
                case CellState.Ship:
                    this[coord] = CellState.Hit;
                    return true;
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

        public string PlayerName { get; }

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
            return char.ToUpper(letter) - 65;
        }

        private int GetIndexFromNumber(char number)
        {
            return number - '1';
        }

        private bool IsValidCoord(int coord)
        {
            return coord >= 0 && coord < SideLength;
        }
    }
}