using System.Net.NetworkInformation;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Data.SqlTypes;
using System.Drawing;

namespace ChessCat
{
    public enum ChessColor
    {
        White,
        Black
    }
    public class Game
    {
        static ChessColor GetPieceColor(Piece piece)
        {
            switch (piece)
            {
                case Piece.WKing: return ChessColor.White;
                case Piece.WQueen: return ChessColor.White;
                case Piece.WRook: return ChessColor.White;
                case Piece.WKnight: return ChessColor.White;
                case Piece.WBishop: return ChessColor.White;
                case Piece.WPawn: return ChessColor.White;
                case Piece.FakeWPawn: return ChessColor.White;
            }
            return ChessColor.Black;
        }

        public List<ChessMoveInfo> moveHistory;

        void SetDefaultGame()
        {
            SetPiece(0, 0, Piece.WRook);
            SetPiece(1, 0, Piece.WKnight);
            SetPiece(2, 0, Piece.WBishop);
            SetPiece(3, 0, Piece.WQueen);
            SetPiece(4, 0, Piece.WKing);
            SetPiece(5, 0, Piece.WBishop);
            SetPiece(6, 0, Piece.WKnight);
            SetPiece(7, 0, Piece.WRook);
            for(byte i = 0; i < 8; i++)
            {
                SetPiece(i, 1, Piece.WPawn);
            }

            SetPiece(0, 7, Piece.BRook);
            SetPiece(1, 7, Piece.BKnight);
            SetPiece(2, 7, Piece.BBishop);
            SetPiece(3, 7, Piece.BQueen);
            SetPiece(4, 7, Piece.BKing);
            SetPiece(5, 7, Piece.BBishop);
            SetPiece(6, 7, Piece.BKnight);
            SetPiece(7, 7, Piece.BRook);
            for (byte i = 0; i < 8; i++)
            {
                SetPiece(i, 6, Piece.BPawn);
            }
            WQRookMoved = false;
            WKRookMoved = false;
            BQRookMoved = false;
            BKRookMoved = false;
        }

        void SwapToPlay()
        {
            if (ToPlay == ChessColor.Black) ToPlay = ChessColor.White;
            else ToPlay = ChessColor.Black;
        }

        List<ChessSquare> GetPossibleMovesFrom(ChessSquare square) //NOTE: "Possible" moves are any "legal" moves plus those which could move into check
        {
            Piece pieceType = GetPiece(square);
            ChessColor pieceColor = GetPieceColor(pieceType);

            List<ChessSquare> possibleSquares = new();
            
            if (GetPieceColor(pieceType) != ToPlay || pieceType == default)
            {
                return possibleSquares;
            }

            if (pieceType == Piece.WKing || pieceType == Piece.BKing)
            {
                bool wallToLeft = square.x == 0;
                bool wallToRight = square.x == 7;
                bool wallToTop = square.y == 7;
                bool wallToBottom = square.y == 0;
                if (!wallToLeft)
                {
                    if (!wallToTop)
                    {
                        possibleSquares.Add(new ChessSquare(square, -1, 1));
                    }
                    possibleSquares.Add(new ChessSquare(square, -1, 0));
                    if (!wallToBottom)
                    {
                        possibleSquares.Add(new ChessSquare(square, -1, -1));
                    }
                }
                if (!wallToRight)
                {
                    if (!wallToTop)
                    {
                        possibleSquares.Add(new ChessSquare(square, 1, 1));
                    }
                    possibleSquares.Add(new ChessSquare(square, 1, 0));
                    if (!wallToBottom)
                    {
                        possibleSquares.Add(new ChessSquare(square, 1, -1));
                    }
                }
                if (!wallToTop)
                {
                    possibleSquares.Add(new ChessSquare(square, 0, 1));
                }
                if (!wallToBottom)
                {
                    possibleSquares.Add(new ChessSquare(square, 0, -1));
                }
                possibleSquares.RemoveAll(square => (GetPiece(square) != default && GetPieceColor(GetPiece(square)) == pieceColor));
            }
            else if (pieceType == Piece.WKnight || pieceType == Piece.BKnight)
            {
                possibleSquares.Add(new ChessSquare(square, -1, 2));
                possibleSquares.Add(new ChessSquare(square, 1, 2));
                possibleSquares.Add(new ChessSquare(square, 2, 1));
                possibleSquares.Add(new ChessSquare(square, 2, -1));
                possibleSquares.Add(new ChessSquare(square, -1, -2));
                possibleSquares.Add(new ChessSquare(square, 1, -2));
                possibleSquares.Add(new ChessSquare(square, -2, -1));
                possibleSquares.Add(new ChessSquare(square, -2, 1));

                possibleSquares.RemoveAll(square => !square.isInBounds() || (GetPiece(square) != default && GetPieceColor(GetPiece(square)) == pieceColor));
            }
            else if(pieceType == Piece.WRook || pieceType == Piece.BRook)
            {
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, 0, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if(checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, 0, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, 0);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, 0);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
            }
            else if(pieceType == Piece.WBishop || pieceType == Piece.BBishop)
            {
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
            }
            else if(pieceType == Piece.WQueen || pieceType == Piece.BQueen)
            {
                //Bishop-like:
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                //Rook-like:
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, 0, i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, 0, (short)-i);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, (short)-i, 0);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
                for (byte i = 1; i < 8; i++)
                {
                    ChessSquare checking = new(square, i, 0);
                    Piece checkingPiece = GetPiece(checking);
                    if (!checking.isInBounds() || (checkingPiece != default && GetPieceColor(checkingPiece) == pieceColor))
                    {
                        break;
                    }
                    if (checkingPiece != default)
                    {
                        possibleSquares.Add(checking);
                        break;
                    }
                    possibleSquares.Add(checking);
                }
            }
            else if(pieceType == Piece.WPawn)
            {
                ChessSquare square1 = new(square, 0, 1);
                if(GetPiece(square1) == default && square.y != 7)
                {
                    possibleSquares.Add(square1);
                    if (square.y == 1)
                    {
                        ChessSquare square2 = new(square, 0, 2);
                        if(GetPiece(square2) == default) possibleSquares.Add(square2);
                    }
                }
                ChessSquare square3 = new(square, -1, 1);
                ChessSquare square4 = new(square, 1, 1);
                if (square3.isInBounds() && GetPiece(square3) != default && GetPieceColor(GetPiece(square3)) == ChessColor.Black) possibleSquares.Add(square3);

                if (square4.isInBounds() && GetPiece(square4) != default && GetPieceColor(GetPiece(square4)) == ChessColor.Black) possibleSquares.Add(square4);
            }
            else if(pieceType == Piece.BPawn)
            {
                ChessSquare square1 = new(square, 0, -1);
                if (GetPiece(square1) == default && square.y != 0)
                {
                    possibleSquares.Add(square1);
                    if (square.y == 6)
                    {
                        ChessSquare square2 = new(square, 0, -2);
                        if (GetPiece(square2) == default) possibleSquares.Add(square2);
                    }
                }
                ChessSquare square3 = new(square, -1, -1);
                ChessSquare square4 = new(square, 1, -1);
                if (square3.isInBounds() && GetPiece(square3) != default && GetPieceColor(GetPiece(square3)) == ChessColor.White) possibleSquares.Add(square3);

                if (square4.isInBounds() && GetPiece(square4) != default && GetPieceColor(GetPiece(square4)) == ChessColor.White) possibleSquares.Add(square4);
            }
            return possibleSquares;
        }

        List<ChessSquare> GetLegalMovesFrom(ChessSquare square) //Same as possible moves but excludes moves that go into check
        {
            Piece piece = GetPiece(square);
            List<ChessSquare> possibleSquares = GetPossibleMovesFrom(square);
            List<ChessSquare> impossibleSquares = new();
            foreach(ChessSquare possibleSquare in possibleSquares)
            {
                Piece capturedPiece = GetPiece(possibleSquare);
                SetPiece(square, default);
                SetPiece(possibleSquare, piece);

                ChessSquare kingSquare;
                Piece searching;
                if(ToPlay == ChessColor.White)
                {
                    searching = Piece.WKing;
                }
                else
                {
                    searching = Piece.BKing;
                }
                for(byte x = 0; x <= 7; x++)
                {
                    for(byte y = 0; y <= 7; y++)
                    {
                        if(GetPiece(x, y) == searching)
                        {
                            kingSquare = new ChessSquare(x, y);
                            goto searchEnd;
                        }
                    }
                }
                return possibleSquares;
            searchEnd:
                SwapToPlay();
                for (byte x = 0; x <= 7; x++)
                {
                    for (byte y = 0; y <= 7; y++)
                    {
                        if(GetPossibleMovesFrom(new ChessSquare((short)x, (short)y)).Find(square => square.x == kingSquare.x && square.y == kingSquare.y) != default)
                        {
                            impossibleSquares.Add(possibleSquare);
                            goto resetPosition;
                        }
                    }
                }
            resetPosition:
                SwapToPlay();
                SetPiece(possibleSquare, capturedPiece);
                SetPiece(square, piece);
            }
            foreach (ChessSquare removing in impossibleSquares) possibleSquares.Remove(removing);
            return possibleSquares;
        }

        public List<ChessMove> GetLegalMoves()
        {
            List<ChessMove> legalMoves = new();
            for (byte x = 0; x <= 7; x++)
            {
                for (byte y = 0; y <= 7; y++)
                {
                    ChessSquare originSquare = new(x, y);
                    List<ChessSquare> destinationSquares = GetLegalMovesFrom(originSquare);
                    foreach(ChessSquare destinationSquare in destinationSquares)
                    {
                        legalMoves.Add(new(originSquare, destinationSquare));
                    }
                }
            }
            return legalMoves;
        }

        public static ChessSquare GetSquare(string str)
        {
            ChessSquare square = new(0, 0);

            square.x = (short)"abcdefgh".IndexOf(str[0]);
            square.y = (short)"12345678".IndexOf(str[1]);
            return square;
        }
        public ChessMove GetMove(string str)
        {
            ChessSquare? origin = null;
            ChessSquare destination;

            int origin_num_idx = str.IndexOfAny("12345678".ToCharArray());
            int destination_num_idx = str.LastIndexOfAny("12345678".ToCharArray());

            destination = GetSquare(str.Substring(destination_num_idx - 1, 2));

            char? columnHint = null;

            if (origin_num_idx != destination_num_idx)
            {
                origin = GetSquare(str.Substring(origin_num_idx - 1, 2));
            }
            else
            {
                Piece pieceTypeMoving;
                if(origin_num_idx == 1)
                {
                    if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WPawn;
                    else pieceTypeMoving = Piece.BPawn;
                }
                else
                {
                    switch (str.ToLower()[0])
                    {
                        case 'k':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WKing;
                            else pieceTypeMoving = Piece.BKing;
                            break;
                        case 'q':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WQueen;
                            else pieceTypeMoving = Piece.BQueen;
                            break;
                        case 'r':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WRook;
                            else pieceTypeMoving = Piece.BRook;
                            break;
                        case 'b':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WBishop;
                            else pieceTypeMoving = Piece.BBishop;
                            break;
                        case 'n':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WKnight;
                            else pieceTypeMoving = Piece.BKnight;
                            break;
                        case 's':
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WKnight;
                            else pieceTypeMoving = Piece.BKnight;
                            break;
                        default:
                            if (ToPlay == ChessColor.White) pieceTypeMoving = Piece.WPawn;
                            else pieceTypeMoving = Piece.BPawn;
                            columnHint = str.ToLower()[0];
                            break;
                    }
                }
                foreach(ChessMove move in GetLegalMoves())
                {
                    if (GetPiece(move.origin) == pieceTypeMoving && move.destination.x == destination.x && move.destination.y == destination.y
                        && (columnHint == null || (columnHint != null && move.origin.ToString().StartsWith((char)columnHint))))
                    {
                        if(origin != null)
                        {
                            throw new Exception("Ambiguous movement");
                        }
                        origin = move.origin;
                    }
                }
            }

            if(origin == null)
            {
                throw new Exception("No pieces found to move");
            }

            return new ChessMove(origin, destination);
        }

        public bool IsStalemate()
        {
            if(GetLegalMoves().Count != 0)
            {
                return false;
            }
            SwapToPlay();
            Piece targetPiece = Piece.BKing;
            if (ToPlay == ChessColor.Black) targetPiece = Piece.WKing;
            foreach(ChessMove m in GetLegalMoves())
            {
                if(GetPiece(m.destination) == targetPiece)
                {
                    SwapToPlay();
                    return false;
                }
            }
            SwapToPlay();
            return true;
        }

        public bool IsCheckmate()
        {
            if (GetLegalMoves().Count != 0)
            {
                return false;
            }
            SwapToPlay();
            Piece targetPiece = Piece.BKing;
            if (ToPlay == ChessColor.Black) targetPiece = Piece.WKing;
            foreach (ChessMove m in GetLegalMoves())
            {
                if (GetPiece(m.destination) == targetPiece)
                {
                    SwapToPlay();
                    return true;
                }
            }
            SwapToPlay();
            return false;
        }

        public char GetPieceLetter(Piece piece)
        {
            switch (piece)
            {
                case Piece.WKing: return 'K';
                case Piece.WQueen: return 'Q';
                case Piece.WRook: return 'R';
                case Piece.WKnight: return 'N';
                case Piece.WBishop: return 'B';
                case Piece.WPawn: return 'P';
                case Piece.BKing: return 'k';
                case Piece.BQueen: return 'q';
                case Piece.BRook: return 'r';
                case Piece.BKnight: return 'n';
                case Piece.BBishop: return 'b';
                case Piece.BPawn: return 'p';
            }
            return '0';
        }

        public Piece GetPieceFromLetter(char C)
        {
            switch (C)
            {
                case 'K': return Piece.WKing;
                case 'Q': return Piece.WQueen;
                case 'R': return Piece.WRook;
                case 'N': return Piece.WKnight;
                case 'B': return Piece.WBishop;
                case 'P': return Piece.WPawn;
                case 'k': return Piece.BKing;
                case 'q': return Piece.BQueen;
                case 'r': return Piece.BRook;
                case 'n': return Piece.BKnight;
                case 'b': return Piece.BBishop;
                case 'p': return Piece.BPawn;
            }
            return default;
        }

        public string GetFEN()
        {
            string FENstring = "";
            Piece current;
            ChessSquare? fakePawnPos = null;
            for(short y = 7; y >= 0; y--)
            {
                byte emptycounter = 0;
                for(byte x = 0; x <= 7; x++)
                {
                    current = GetPiece(x, (byte)y);
                    if (current == default)
                    {
                        emptycounter++;
                    }
                    else if (current == Piece.FakeWPawn || current == Piece.FakeBPawn)
                    {
                        emptycounter++;
                        fakePawnPos = new(x, y);
                    }
                    else
                    {
                        if (emptycounter != 0) FENstring += emptycounter.ToString();
                        FENstring += GetPieceLetter(current);
                        emptycounter = 0;
                    }
                }
                if (emptycounter != 0) FENstring += emptycounter.ToString();
                if (y != 0) FENstring += '/';
            }

            FENstring += ' ';

            if (ToPlay == ChessColor.White) FENstring += 'w';
            else FENstring += 'b';

            FENstring += ' ';

            if (!WKingMoved)
            {
                if (!WKRookMoved) FENstring += 'K';
                if (!WQRookMoved) FENstring += 'Q';
            }
            if (!BKingMoved)
            {
                if (!BKRookMoved) FENstring += 'k';
                if (!BQRookMoved) FENstring += 'q';
            }
            if ((WKingMoved || (WKRookMoved && WQRookMoved)) && (BKingMoved || BKRookMoved && BQRookMoved)) FENstring += '-';

            if (fakePawnPos == null) FENstring += " - ";
            else FENstring += " " + fakePawnPos + " ";

            FENstring += "0 0"; //TODO: Implement 50 move rule

            return FENstring;
        }

        public void SetFromFEN(string FEN)
        {
            string[] parts = FEN.Split(' ');
            string[] rows = parts[0].Split('/');

            byte currenty = 7;
            foreach(string row in rows)
            {
                byte currentx = 0;
                foreach(char C in row)
                {
                    byte num;
                    if (byte.TryParse(C.ToString(), out num)) currentx += num;
                    else
                    {
                        SetPiece(currentx, currenty, GetPieceFromLetter(C));
                        currentx++;
                    }
                }
                currenty--;
            }

            if (parts[1] == "b") ToPlay = ChessColor.Black;

            if (!parts[2].Contains('K')) WKRookMoved = true;
            if (!parts[2].Contains('Q')) WQRookMoved = true;

            if (!parts[2].Contains('k')) BKRookMoved = true;
            if (!parts[2].Contains('q')) BQRookMoved = true;

            if (parts[3] != "-")
            {
                if(ToPlay == ChessColor.White) SetPiece(GetSquare(parts[3]), Piece.FakeBPawn);
                else SetPiece(GetSquare(parts[3]), Piece.FakeWPawn);
            }

            //TODO: implement 50 move rule here
        }

        bool WQRookMoved;
        bool WKRookMoved;
        bool WKingMoved;

        bool BQRookMoved;
        bool BKRookMoved;
        bool BKingMoved;

        Piece[] Board;
        public ChessColor ToPlay;
        public Game(string position = "", ChessColor toPlay = ChessColor.White)
        {
            moveHistory = new();
            Board = new Piece[64];
            ToPlay = toPlay;
            if (position != "") SetFromFEN(position);
            else SetDefaultGame();
        }

        private void clearFakePawns()
        {
            for (byte x = 0; x <= 7; x++)
            {
                for (byte y = 0; y <= 7; y++)
                {
                    Piece pieceType = GetPiece(x, y);
                    if(pieceType == Piece.FakeBPawn || pieceType == Piece.FakeWPawn)
                    {
                        SetPiece(x, y, default);
                    }
                }
            }
        }

        public (MoveResult, Piece) playMove(ChessMove move) //Returns moveresult and piece captured
        {
            List<ChessSquare> legalMoves = GetLegalMovesFrom(move.origin);
            if(legalMoves.Find(legalMove => legalMove.x == move.destination.x && legalMove.y == move.destination.y) == default)
            {
                return (MoveResult.Illegal, default);
            }
            Piece piece = GetPiece(move.origin);
            Piece capture = GetPiece(move.destination);
            SetPiece(move.origin, default);
            SetPiece(move.destination, piece);

            clearFakePawns();

            if (piece == Piece.WPawn && move.destination.y - move.origin.y == 2) SetPiece(move.destination.x, (byte)(move.destination.y - 1), Piece.FakeWPawn);
            else if (piece == Piece.BPawn && move.destination.y - move.origin.y == -2) SetPiece(move.destination.x, (byte)(move.destination.y + 1), Piece.FakeBPawn);
            SwapToPlay();

            if(piece == Piece.BKing) BKingMoved = true;
            else if(piece == Piece.WKing) WKingMoved = true;

            else if (piece == Piece.WRook && move.origin.y == 0)
            {
                if (move.origin.y == 0) WQRookMoved = true;
                else if (move.origin.x == 7) WKingMoved = true;
            }
            else if (piece == Piece.BRook && move.origin.y == 7)
            {
                if (move.origin.y == 0) BQRookMoved = true;
                else if (move.origin.x == 7) BKingMoved = true;
            }

            if (capture != default)
            {
                if(capture == Piece.FakeWPawn) SetPiece(move.destination.x, (byte)(move.destination.y + 1), default);

                if(capture == Piece.FakeBPawn) SetPiece(move.destination.x, (byte)(move.destination.y - 1), default);

                return (MoveResult.Captures, capture);
            }
            return (MoveResult.Normal, capture); //TODO: Add returning other moveresults
        }

        public MoveResult playCastle(bool isKingSide)
        {
            if (ToPlay == ChessColor.White)
            {
                if (WKingMoved || GetPiece(4, 0) != Piece.WKing) return MoveResult.Illegal;
                if (isKingSide)
                {
                    if (WKRookMoved ||
                        GetPiece(7, 0) != Piece.WRook ||
                        GetPiece(6, 0) != default ||
                        GetPiece(5, 0) != default
                        ) return MoveResult.Illegal;

                    if (playMove(new(new(4, 0), new(5, 0))).Item1 == MoveResult.Illegal) return MoveResult.Illegal;

                    ToPlay = ChessColor.White;

                    if (playMove(new(new(5, 0), new(6, 0))).Item1 == MoveResult.Illegal)
                    {
                        ToPlay = ChessColor.White;
                        SetPiece(4, 0, Piece.WKing);
                        SetPiece(5, 0, default);
                        return MoveResult.Illegal;
                    }

                    SetPiece(7, 0, default);
                    SetPiece(5, 0, Piece.WRook);
                    WKRookMoved = true;
                    return MoveResult.Normal;
                }
                else
                {
                    if (WQRookMoved ||
                        GetPiece(0, 0) != Piece.WRook ||
                        GetPiece(1, 0) != default ||
                        GetPiece(2, 0) != default ||
                        GetPiece(3, 0) != default
                        ) return MoveResult.Illegal;

                    if (playMove(new(new(4, 0), new(3, 0))).Item1 == MoveResult.Illegal) return MoveResult.Illegal;

                    ToPlay = ChessColor.White;

                    if (playMove(new(new(3, 0), new(2, 0))).Item1 == MoveResult.Illegal)
                    {
                        ToPlay = ChessColor.White;
                        SetPiece(4, 0, Piece.WKing);
                        SetPiece(3, 0, default);
                        return MoveResult.Illegal;
                    }

                    SetPiece(0, 0, default);
                    SetPiece(3, 0, Piece.WRook);
                    WQRookMoved = true;
                    return MoveResult.Normal;
                }
            }
            else
            {
                if (BKingMoved || GetPiece(4, 7) != Piece.BKing) return MoveResult.Illegal;
                if (isKingSide)
                {
                    if (BKRookMoved ||
                        GetPiece(7, 7) != Piece.BRook ||
                        GetPiece(6, 7) != default ||
                        GetPiece(5, 7) != default
                        ) return MoveResult.Illegal;

                    if (playMove(new(new(4, 7), new(5, 7))).Item1 == MoveResult.Illegal) return MoveResult.Illegal;

                    ToPlay = ChessColor.Black;

                    if (playMove(new(new(5, 7), new(6, 7))).Item1 == MoveResult.Illegal)
                    {
                        ToPlay = ChessColor.Black;
                        SetPiece(4, 7, Piece.WKing);
                        SetPiece(5, 7, default);
                        return MoveResult.Illegal;
                    }

                    SetPiece(7, 7, default);
                    SetPiece(5, 7, Piece.BRook);
                    BKRookMoved = true;
                    return MoveResult.Normal;
                }
                else
                {
                    if (BQRookMoved ||
                        GetPiece(0, 7) != Piece.BRook ||
                        GetPiece(1, 7) != default ||
                        GetPiece(2, 7) != default ||
                        GetPiece(3, 7) != default
                        ) return MoveResult.Illegal;

                    if (playMove(new(new(4, 7), new(3, 7))).Item1 == MoveResult.Illegal) return MoveResult.Illegal;

                    ToPlay = ChessColor.Black;

                    if (playMove(new(new(3, 7), new(2, 7))).Item1 == MoveResult.Illegal)
                    {
                        ToPlay = ChessColor.Black;
                        SetPiece(4, 7, Piece.BKing);
                        SetPiece(3, 7, default);
                        return MoveResult.Illegal;
                    }

                    SetPiece(0, 7, default);
                    SetPiece(3, 7, Piece.BRook);
                    BQRookMoved = true;
                    return MoveResult.Normal;
                }
            }
        }

        public (MoveResult, string) interpretMove(string moveStr)
        {
            ChessColor initToPlay = ToPlay;
            MoveResult moveResult;
            ChessMoveInfo? moveInfo = null;
            string moveDescription = "";
            if (moveStr == "O-O" || moveStr == "0-0")
            {
                moveDescription = "Castles king-side";
                if (initToPlay == ChessColor.White) moveInfo = new(initToPlay, new ChessMove(new(4, 0), new(6, 0)), default) { castled = true, castledKingSide = true };
                else moveInfo = new(initToPlay, new ChessMove(new(4, 7), new(6, 7)), default) { castled = true, castledKingSide = true };
                moveResult = playCastle(true);
            }
            else if(moveStr == "O-O-O" || moveStr == "0-0-0")
            {
                moveDescription = "Castles queen-side";
                if (initToPlay == ChessColor.White) moveInfo = new(initToPlay, new ChessMove(new(4, 0), new(2, 0)), default) { castled = true, castledKingSide = false };
                else moveInfo = new(initToPlay, new ChessMove(new(4, 7), new(2, 7)), default) { castled = true, castledKingSide = false };
                moveResult = playCastle(false);
            }

            else if (moveStr.Contains('='))
            {
                try
                {
                    int equalsIdx = moveStr.IndexOf('=');
                    char promotionPieceChar = moveStr.ToLower()[equalsIdx + 1];
                    Piece promotionPiece;
                    ChessMove move = GetMove(moveStr.Substring(0, equalsIdx));

                    Piece piece = GetPiece(move.origin);
                    if (piece != Piece.WPawn && piece != Piece.BPawn) throw new Exception("Not a pawn!");

                    switch (promotionPieceChar)
                    {
                        case 'q':
                            if (initToPlay == ChessColor.White) promotionPiece = Piece.WQueen;
                            else promotionPiece = Piece.BQueen;
                            break;
                        case 'r':
                            if (initToPlay == ChessColor.White) promotionPiece = Piece.WRook;
                            else promotionPiece = Piece.BRook;
                            break;
                        case 'b':
                            if (initToPlay == ChessColor.White) promotionPiece = Piece.WBishop;
                            else promotionPiece = Piece.BBishop;
                            break;
                        case 'n':
                            if (initToPlay == ChessColor.White) promotionPiece = Piece.WKnight;
                            else promotionPiece = Piece.BKnight;
                            break;
                        case 's':
                            if (initToPlay == ChessColor.White) promotionPiece = Piece.WKnight;
                            else promotionPiece = Piece.BKnight;
                            break;
                        default:
                            throw new Exception("Promotion piece not found");
                    }

                    (MoveResult, Piece) result = playMove(move);
                    if (result.Item1 == MoveResult.Illegal) moveResult = MoveResult.Illegal;
                    else
                    {
                        SetPiece(move.destination, promotionPiece);
                        moveResult = MoveResult.Normal;
                        moveInfo = new(initToPlay, move, result.Item2);
                        if(result.Item2 != default) moveDescription = move.origin + " captures " + move.destination + " & promotes to " + promotionPieceChar.ToString().ToUpper();
                        else moveDescription = move.origin + " promotes to " + promotionPieceChar.ToString().ToUpper();
                    }
                }
                catch
                {
                    moveResult = MoveResult.Illegal;
                }
            }
            else
            {
                try
                {
                    ChessMove move = GetMove(moveStr);

                    Piece piece = GetPiece(move.origin);

                    if (piece == Piece.WPawn && move.destination.y == 7) throw new Exception("Pawn must promote");
                    else if (piece == Piece.BPawn && move.destination.y == 0) throw new Exception("Pawn must promote");

                    (MoveResult, Piece) result = playMove(move);

                    moveResult = result.Item1;
                    moveInfo = new(initToPlay, move, result.Item2);

                    if (result.Item2 == default) moveDescription = GetPieceLetter(piece) + move.origin.ToString() + " to " + move.destination.ToString();
                    else moveDescription = GetPieceLetter(piece) + move.origin.ToString() + " takes " + move.destination.ToString();
                }
                catch
                {
                    moveResult = MoveResult.Illegal;
                }
            }
            if (moveResult != MoveResult.Illegal && moveInfo != null)
            {
                moveHistory.Add(moveInfo);
            }
            return (moveResult, moveDescription);
        }

        public void SetPiece(short x, short y, Piece piece) /*   Set a piece at 0-indexed position x and y   */
        {
            if (x < 0 || x > 7 || y < 0 || y > 7) throw new Exception("Out of bounds!");
            Board[y * 8 + x] = piece;
        }

        public void SetPiece(ChessSquare square, Piece piece)
        {
            SetPiece(square.x, square.y, piece);
        }

        public Piece GetPiece(byte x, byte y)
        {
            if(x < 0 || x > 7 || y < 0 || y > 7)
            {
                return default;
            }
            return Board[y * 8 + x];
        }

        public Piece GetPiece(ChessSquare square)
        {
            return GetPiece((byte)square.x, (byte)square.y);
        }
    }
}