using System;

namespace BattleShip
{
    public class Game
    {
        private readonly IPresenter _presenter;

        private IBoard _currentPlayerBoard;
        private IBoard _targetPlayerBoard;

        public Game(IBoard playerOneBoard, IBoard playerTwoBoard, IPresenter presenter)
        {
            _currentPlayerBoard = playerOneBoard;
            _targetPlayerBoard = playerTwoBoard;
            _presenter = presenter;

            Setup(_currentPlayerBoard);
            Setup(_targetPlayerBoard);

            Start();
        }

        private void Setup(IBoard board)
        {
            _presenter.PrintSwitchScreen(board.PlayerName);            

            var confirmed = false;

            while (!confirmed)
            {
                _presenter.PrintGameState(board);

                var input = _presenter.PromptPlayer("Enter Coordinates for your ship (eg. A2 C2): ");
                var coords = input.Split(' ');

                if (coords.Length != 2)
                {
                    _presenter.PromptPlayer($"Entered Coordinates did not match expected format: {input}");
                    continue;
                }

                try
                {
                    board.PlaceShip(coords[0], coords[1]);
                }
                catch (Exception ex)
                {
                    _presenter.PromptPlayer(ex.Message);
                    continue;
                }

                _presenter.PrintGameState(board);

                if (_presenter.PromptPlayer("Are you sure (Y or N)?").ToUpper() == "Y")
                {
                    confirmed = true;
                }
                else
                {
                    board.ClearShips();
                }
            }
        }

        private void Start()
        {
            while (true)
            {
                try
                {
                    ProcessTurn();
                }
                catch (Exception ex)
                {
                    _presenter.PromptPlayer(ex.Message);
                    continue;
                }

                if (_targetPlayerBoard.IsShipRemaining())
                {
                    var temp = _targetPlayerBoard;
                    _targetPlayerBoard = _currentPlayerBoard;
                    _currentPlayerBoard = temp;

                    _presenter.PrintSwitchScreen(_currentPlayerBoard.PlayerName);
                }
                else
                {
                    _presenter.PrintVictoryScreen(_currentPlayerBoard.PlayerName);
                    break;
                }
            }
        }

        private void ProcessTurn()
        {

            _presenter.PrintGameState(_currentPlayerBoard,_targetPlayerBoard);
            var target = _presenter.PromptPlayer("Choose coordinates to attack (A2): ");

            if(_targetPlayerBoard.TakeShot(target))
            {
                _presenter.PromptPlayer("HIT");
            }
            else
            {
                _presenter.PromptPlayer("MISS");
            }
        }
    }
}