using System;

namespace BattleShip
{
    public class ConsolePresenter : IPresenter
    {
        private int _previousPromptLength;

        public void PrintGameState(IBoard currentPlayerBoard)
        {
            PrintBase();
            PrintCurrent(currentPlayerBoard);
        }

        public void PrintGameState(IBoard currentPlayerBoard, IBoard targetPlayerBoard)
        {
            PrintGameState(currentPlayerBoard);
            PrintEnemyBoard(targetPlayerBoard);
        }

        public string PromptPlayer(string question)
        {
            Console.SetCursorPosition(25,1);
            Console.Write(new string(' ', _previousPromptLength));
            Console.SetCursorPosition(25, 1);

            Console.CursorVisible = true;
            Console.Write(question);

            var result = Console.ReadLine();
            Console.CursorVisible = false;

            _previousPromptLength = question.Length + result?.Length ?? 0;
            return result;
        }

        public void PrintSwitchScreen(string nextPlayerName)
        {
            Console.Clear();
            Console.SetCursorPosition(10,5);
            Console.Write($" ╔══════════════════════════════{new string('═', nextPlayerName.Length)}╗");
            Console.SetCursorPosition(10, 6);
            Console.Write($" ║  Press enter when {nextPlayerName} is ready. ║");
            Console.SetCursorPosition(10, 7);
            Console.Write($" ╚══════════════════════════════{new string('═', nextPlayerName.Length)}╝");

            Console.ReadLine();
        }

        public void PrintVictoryScreen(string playerName)
        {
            Console.Clear();
            Console.SetCursorPosition(10, 5);
            Console.Write($" ╔═════════════════{new string('═', playerName.Length)}╗");
            Console.SetCursorPosition(10, 6);
            Console.Write($" ║ {playerName} is the winner! ║");
            Console.SetCursorPosition(10, 7);
            Console.Write($" ╚═════════════════{new string('═', playerName.Length)}╝");

            Console.ReadLine();
            Console.Clear();
        }

        private void PrintBase()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Console.WriteLine(" ╔════════════════════╗");
            Console.WriteLine(" ║     Enemy Board    ║");
            Console.WriteLine(" ║   A B C D E F G H  ║");
            Console.WriteLine(" ║ 1                  ║");
            Console.WriteLine(" ║ 2                  ║");
            Console.WriteLine(" ║ 3                  ║");
            Console.WriteLine(" ║ 4                  ║");
            Console.WriteLine(" ║ 5                  ║");
            Console.WriteLine(" ║ 6                  ║");
            Console.WriteLine(" ║ 7                  ║");
            Console.WriteLine(" ║ 8                  ║");
            Console.WriteLine(" ╠════════════════════╣");
            Console.WriteLine(" ║     Your Board     ║");
            Console.WriteLine(" ║   A B C D E F G H  ║");
            Console.WriteLine(" ║ 1                  ║");
            Console.WriteLine(" ║ 2                  ║");
            Console.WriteLine(" ║ 3                  ║");
            Console.WriteLine(" ║ 4                  ║");
            Console.WriteLine(" ║ 5                  ║");
            Console.WriteLine(" ║ 6                  ║");
            Console.WriteLine(" ║ 7                  ║");
            Console.WriteLine(" ║ 8                  ║");
            Console.WriteLine(" ╚════════════════════╝");
        }

        private void PrintEnemyBoard(IBoard board)
        {
            for (var x = 0; x < board.SideLength; x++)
            {
                for (var y = 0; y < board.SideLength; y++)
                {
                    var cellValue = board[x, y];

                    switch (cellValue)
                    {
                        case CellState.Ship:
                        case CellState.Blank:
                            PrintCell(x, y, "-");
                            break;
                        case CellState.Miss:
                            PrintCell(x, y, "o");
                            break;
                        case CellState.Hit:
                            PrintCell(x, y, "x");
                            break;
                    }
                }
            }
        }

        private void PrintCurrent(IBoard board)
        {
            for (var x = 0; x < board.SideLength; x++)
            {
                for (var y = 0; y < board.SideLength; y++)
                {
                    var cellValue = board[x, y];

                    switch (cellValue)
                    {
                        case CellState.Hit:
                            PrintCell(x, y, "x", true);
                            break;
                        case CellState.Ship:
                            PrintCell(x, y, "S", true);
                            break;
                        case CellState.Blank:
                            PrintCell(x, y, "-", true);
                            break;
                        case CellState.Miss:
                            PrintCell(x, y, "o", true);
                            break;

                    }
                }
            }
        }

        private void PrintCell(int x, int y, string value, bool personalBoard = false)
        {
            var yOffset = personalBoard ? 14 : 3;

            Console.SetCursorPosition((x * 2) + 5, y + yOffset);
            Console.Write(value);
        }
    }
}