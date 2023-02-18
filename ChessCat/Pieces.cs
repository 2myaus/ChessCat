using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessCat
{
    public enum Piece
    {
        Empty,
        WKing,
        WQueen,
        WRook,
        WKnight,
        WBishop,
        WPawn,
        BKing,
        BQueen,
        BRook,
        BKnight,
        BBishop,
        BPawn,
        FakeWPawn, //"Piece" which is behind a double advanced pawn to allow en passant
        FakeBPawn
    }
}
