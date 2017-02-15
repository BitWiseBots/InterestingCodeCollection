namespace BattleShip
{
    public interface IPresenter
    {
        void PrintGameState(IBoard currentPlayerBoard);
        void PrintGameState(IBoard currentPlayerBoard, IBoard targetPlayerBoard);

        string PromptPlayer(string question);

        void PrintSwitchScreen(string nextPlayerName);
        void PrintVictoryScreen(string playerName);
    }
}