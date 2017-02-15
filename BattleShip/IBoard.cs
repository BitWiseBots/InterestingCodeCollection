namespace BattleShip
{
    public interface IBoard
    {
        void PlaceShip(string startCoord, string endCoord);
        bool TakeShot(string coord);
        bool IsShipRemaining();

        int SideLength { get; }
        string PlayerName { get; }
        CellState this[int x, int y] { get; }
        void ClearShips();
    }
}