using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BattleShip.Tests
{
    [TestFixture]
    public class BoardTests
    {

        [Test]
        public void Constructor_ShouldInitializeBoard()
        {
            var board = new Board(8, "Bob");

            Assert.That(board.PlayerName, Is.EqualTo("Bob"));
            Assert.That(board.SideLength, Is.EqualTo(8));
            Assert.Throws<IndexOutOfRangeException>(()=>
            {
                var test = board[0, 8];
            });
            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                var test = board[8, 0];
            });
        }

        [Test]
        public void PlaceShip_ShouldThrowException_WhenCoordinatesDiagonal()
        {
            var board = new Board(8, "Bob");

            var exception = Assert.Throws<ArgumentException>(() => board.PlaceShip("A2", "C4"));

            Assert.That(exception.Message, Is.EqualTo("Entered Ship Placement Coordinates cannot be on a diagonal: A2 C4"));
        }

        [Test]
        public void PlaceShip_ShouldThrowException_WhenCoordinatesTooFarApart()
        {
            var board = new Board(8, "Bob");

            var exception = Assert.Throws<ArgumentException>(() => board.PlaceShip("A2", "A5"));

            Assert.That(exception.Message, Is.EqualTo("Entered Ship Placement Coordinates must be 2 spaces apart: A2 A5"));
        }

        [Test]
        public void PlaceShip_ShouldThrowException_WhenCoordinatesTooClose()
        {
            var board = new Board(8, "Bob");

            var exception = Assert.Throws<ArgumentException>(() => board.PlaceShip("A2", "A3"));

            Assert.That(exception.Message, Is.EqualTo("Entered Ship Placement Coordinates must be 2 spaces apart: A2 A3"));
        }

        [Test]
        public void PlaceShip_ShouldThrowException_WhenStartCoordinateOutOfBounds()
        {
            var board = new Board(8, "Bob");

            var exception = Assert.Throws<ArgumentException>(() => board.PlaceShip("A9", "A7"));

            Assert.That(exception.Message, Is.EqualTo("Entered Coordinates were invalid: A9"));
        }

        [Test]
        public void PlaceShip_ShouldThrowException_WhenEndCoordinateOutOfBounds()
        {
            var board = new Board(8, "Bob");

            var exception = Assert.Throws<ArgumentException>(() => board.PlaceShip("B7", "B9"));

            Assert.That(exception.Message, Is.EqualTo("Entered Coordinates were invalid: B9"));
        }

        [Test]
        public void PlaceShip_ShouldSucceed_WhenCoordinatesHorizontal()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "C2");

            Assert.That(board[0,1], Is.EqualTo(CellState.Ship));
            Assert.That(board[1,1], Is.EqualTo(CellState.Ship));
            Assert.That(board[2,1], Is.EqualTo(CellState.Ship));
        }

        [Test]
        public void PlaceShip_ShouldSucceed_WhenCoordinatesVertical()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");

            Assert.That(board[0, 1], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 2], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 3], Is.EqualTo(CellState.Ship));
        }

        [Test]
        public void PlaceShip_ShouldSucceed_WhenCoordinatesHorizontalAndReversed()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("C2", "A2");

            Assert.That(board[0, 1], Is.EqualTo(CellState.Ship));
            Assert.That(board[1, 1], Is.EqualTo(CellState.Ship));
            Assert.That(board[2, 1], Is.EqualTo(CellState.Ship));
        }

        [Test]
        public void PlaceShip_ShouldSucceed_WhenCoordinatesVerticalAndReversed()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A4", "A2");

            Assert.That(board[0, 1], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 2], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 3], Is.EqualTo(CellState.Ship));
        }

        [Test]
        public void ClearShips_ShouldRemoveShips()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");

            Assert.That(board[0, 1], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 2], Is.EqualTo(CellState.Ship));
            Assert.That(board[0, 3], Is.EqualTo(CellState.Ship));

            board.ClearShips();

            Assert.That(board[0, 1], Is.EqualTo(CellState.Blank));
            Assert.That(board[0, 2], Is.EqualTo(CellState.Blank));
            Assert.That(board[0, 3], Is.EqualTo(CellState.Blank));
        }

        [Test]
        public void TakeShot_ShouldThrowException_WhenDuplicateShotsTaken()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A5");
            var exception = Assert.Throws<ArgumentException>(() => board.TakeShot("A5"));

            Assert.That(exception.Message, Is.EqualTo("Entered Coordinates have already been attempted: A5"));
        }

        [Test]
        public void TakeShot_ShouldRecordMiss_WhenShipNotAtCoord()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A5");

            Assert.That(board[0,4], Is.EqualTo(CellState.Miss));
        }

        [Test]
        public void TakeShot_ShouldRecordHit_WhenShipAtCoord()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A3");

            Assert.That(board[0, 2], Is.EqualTo(CellState.Hit));
        }

        [Test]
        public void IsShipRemaining_ShouldReturnTrue_WhenNoShotsTaken()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");

            Assert.That(board.IsShipRemaining, Is.True);
        }

        [Test]
        public void IsShipRemaining_ShouldReturnTrue_WhenNoHitsRecorded()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A5");

            Assert.That(board.IsShipRemaining, Is.True);
        }

        [Test]
        public void IsShipRemaining_ShouldReturnTrue_WhenOneHitRecorded()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A3");

            Assert.That(board.IsShipRemaining, Is.True);
        }

        [Test]
        public void IsShipRemaining_ShouldReturnTrue_WhenTwoHitsRecorded()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A3");
            board.TakeShot("A2");

            Assert.That(board.IsShipRemaining, Is.True);
        }

        [Test]
        public void IsShipRemaining_ShouldReturnFalse_WhenThreeHitsRecorded()
        {
            var board = new Board(8, "Bob");

            board.PlaceShip("A2", "A4");
            board.TakeShot("A3");
            board.TakeShot("A2");
            board.TakeShot("A4");

            Assert.That(board.IsShipRemaining, Is.False);
        }
    }
}
