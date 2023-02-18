using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessCat
{
    public class ChessSquare
    {
        public short x;
        public short y;
        public ChessSquare(short posX, short posY)
        {
            x = posX;
            y = posY;
        }
        public ChessSquare(ChessSquare moveFrom, short dx, short dy)
        {
            x = (short)(moveFrom.x + dx);
            y = (short)(moveFrom.y + dy);
        }
        public bool isInBounds()
        {
            return x >= 0 && x <= 7 && y >= 0 && y <= 7;
        }
        public override string ToString()
        {
            char[] lettersByCol = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'};
            return lettersByCol[x].ToString() + (y+1);
        }
    }

    public class ChessMove
    {
        public ChessSquare origin;
        public ChessSquare destination;
        public ChessMove(ChessSquare origin, ChessSquare destination)
        {
            this.origin = origin;
            this.destination = destination;
        }
        public override string ToString()
        {
            return origin + " to " + destination;
        }
    }

    public class ChessMoveInfo
    {
        public ChessColor movedBy;
        public ChessMove move;
        public Piece capture;
        public Piece promoteTo;
        public bool castled = false;
        public bool castledKingSide;
        public ChessMoveInfo(ChessColor movedBy, ChessMove move, Piece capture)
        {
            this.movedBy = movedBy;
            this.move = move;
            this.capture = capture;
        }
    }

    public enum MoveResult
    {
        Normal,
        Illegal,
        Checks,
        Captures,
        ChecksCaptures,
        Checkmates,
        Stalemates
    }
}
