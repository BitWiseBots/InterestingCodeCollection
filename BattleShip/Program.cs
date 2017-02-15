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
            var exit = false;
            var presenter = new ConsolePresenter();
            while (!exit)
            {
                new Game(new Board(8, "Player One"), new Board(8, "Player Two"), presenter);

                
                var response = presenter.PromptPlayer("Play another game (Y or N)?");

                if (response.ToUpper() == "N")
                {
                    exit = true;
                }
            }
        }
    }
}
