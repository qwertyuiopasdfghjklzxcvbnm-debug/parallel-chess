using System.Reflection;

namespace parallel_chess;

enum Piece
{
    PawnBlack,
    PawnWhite,
    BishopBlack,
    BishopWhite,
    KnightBlack,
    KnightWhite,
    RookBlack,
    RookWhite,
    QueenBlack,
    QueenWhite,
    KingBlack,
    KingWhite,
    
    Empty,
}

enum Player
{
    Black,
    White,
    None
}

static class PieceMethods
{
    public static Player GetPlayer(this Piece piece)
    {
        if (piece == Piece.Empty)
        {
            return Player.None;
        }
        if (piece == Piece.PawnBlack || piece == Piece.BishopBlack || piece == Piece.KnightBlack ||
                 piece == Piece.RookBlack || piece == Piece.QueenBlack || piece == Piece.KingBlack)
        {
            return Player.Black;
        }
        return Player.White;
    }
    
    
}

class Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    private const string Translation = "abcdefgh";

    public Coordinate(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public Coordinate(string coordinate)
    {
        this.X = (int)(coordinate[1] - '0') - 1;
        this.Y = Translation.IndexOf(coordinate[0]);
    }

    public override string ToString()
    {
        return $"{Translation[X]}{Y+1}";
    }
    
}

class Move
{
    public Piece MovedPiece;
    
    public Coordinate MoveFrom;
    
    public Coordinate MoveTo;

    public Piece TakenPiece;

    public Move(Coordinate moveFrom, Coordinate moveTo, Piece movedPiece=Piece.Empty, Piece takenPiece = Piece.Empty)
    {
        this.MovedPiece = movedPiece;
        this.MoveFrom = moveFrom;
        this.MoveTo = moveTo;
        this.TakenPiece = takenPiece;
    }

    public Move(int formX, int formY, int toX, int toY, Piece movedPiece=Piece.Empty, Piece takenPiece = Piece.Empty)
    {
        this.MovedPiece = movedPiece;
        this.MoveFrom = new Coordinate(formX, formY);
        this.MoveTo = new Coordinate(toX, toY);
        this.TakenPiece = takenPiece;
    }

    public Move(string moveFrom, string moveTo, Piece movedPiece=Piece.Empty, Piece takenPiece = Piece.Empty)
    {
        this.MovedPiece = movedPiece;
        this.MoveFrom = new Coordinate(moveFrom);
        this.MoveTo = new Coordinate(moveTo);
        this.TakenPiece = takenPiece;
    }

    public Move Copy()
    {
        Coordinate moveFrom = new Coordinate(MoveFrom.X, MoveFrom.Y);
        Coordinate moveTo = new Coordinate(MoveTo.X, MoveTo.Y);
        return new Move(moveFrom, moveTo, MovedPiece, MovedPiece);
    }
}   

class GameState
{
    public Piece[,] Board { get; set; }
    public Player OnTurn { get; set; }

    public GameState()
    {
        this.Board = new Piece[8, 8]
        {
            {Piece.RookWhite,  Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.RookBlack  },
            {Piece.KnightWhite,Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.KnightBlack},
            {Piece.BishopWhite,Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.BishopBlack},
            {Piece.QueenWhite, Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.QueenBlack },
            {Piece.KingWhite,  Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.KingBlack  },
            {Piece.BishopWhite,Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.BishopBlack},
            {Piece.KnightWhite,Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.KnightBlack},
            {Piece.RookWhite,  Piece.PawnWhite,Piece.Empty,Piece.Empty,Piece.Empty,Piece.Empty,Piece.PawnBlack,Piece.RookBlack  },
        };
    
        this.OnTurn = Player.White;
    }

    public Piece GetPiece(Coordinate coordinate)
    {
        return this.Board[coordinate.X, coordinate.Y];
    }
    
    public Piece GetPiece(int x, int y)
    {
        return this.Board[x, y];
    }

    public bool ValidateMove(Move move)
    {
        
        move.MovedPiece = this.GetPiece(move.MoveFrom);
        move.TakenPiece = this.GetPiece(move.MoveTo);
        
        if (move.MovedPiece == Piece.Empty || move.MoveFrom == move.MoveTo || move.MovedPiece.GetPlayer()!=this.OnTurn||
            move.MovedPiece.GetPlayer() == this.GetPiece(move.MoveTo).GetPlayer() || move.TakenPiece != this.GetPiece(move.MoveTo))
        {
            return false;
        }
        
        int moveVectorX =  move.MoveTo.X-move.MoveFrom.X;
        int moveVectorY = move.MoveTo.Y-move.MoveFrom.Y;
        int dirX = Math.Sign(moveVectorX);
        int dirY = Math.Sign(moveVectorY);
        
        switch (move.MovedPiece)
        {
            case Piece.PawnWhite:
                if (moveVectorX == 0 && moveVectorY == 1 && this.GetPiece(move.MoveTo) == Piece.Empty)
                {
                    return true;
                }
                if (move.MoveFrom.Y == 1 && moveVectorX == 0 && moveVectorY == 2
                    && this.GetPiece(move.MoveFrom.X, move.MoveFrom.Y + 1) == Piece.Empty
                    && this.GetPiece(move.MoveTo) == Piece.Empty)
                {
                    return true;
                    
                }
                if (Math.Abs(moveVectorX) == 1 && moveVectorY == 1 && this.GetPiece(move.MoveTo) != Piece.Empty)
                {
                    return true;
                }
                
                break;
            case Piece.PawnBlack:
                if (moveVectorX == 0 && moveVectorY == -1 && this.GetPiece(move.MoveTo) == Piece.Empty)
                {
                    return true;
                }
                if (move.MoveFrom.Y == 6 && moveVectorX == 0 && moveVectorY == -2
                    && this.GetPiece(move.MoveFrom.X, move.MoveFrom.Y - 1) == Piece.Empty
                    && this.GetPiece(move.MoveTo) == Piece.Empty)
                {
                    return true;
                    
                }
                if (Math.Abs(moveVectorX) == 1 && moveVectorY == -1 && this.GetPiece(move.MoveTo) != Piece.Empty)
                {
                    return true;
                }

                break;
            case Piece.BishopBlack: case Piece.BishopWhite:
                if (Math.Abs(moveVectorX) != Math.Abs(moveVectorY))
                {
                    return false;
                }

                for (int i = 1; i < 8; i++)
                {
                    int x = move.MoveFrom.X + dirX * i;
                    int y = move.MoveFrom.Y + dirY * i;
                    if (x < 0 || y < 0 || x >= 8 || y >= 8)
                    {
                        return false;
                    }

                    if (move.MoveTo.X == x && move.MoveTo.Y == y)
                    {
                        return true;
                    }
                    
                    if (this.GetPiece(x, y) != Piece.Empty)
                    {
                        return false;
                    }

                    
                }
                break;
            case Piece.KnightBlack: case Piece.KnightWhite:
                if (Math.Abs(moveVectorX) + Math.Abs(moveVectorY) == 3 && moveVectorX != 0 && moveVectorY != 0)
                {
                    return true;
                }
                break;
            case Piece.RookBlack: case Piece.RookWhite:
                if (moveVectorX*moveVectorY != 0)
                {
                    return false;
                }

                for (int i = 1; i < 8; i++)
                {
                    int x = move.MoveFrom.X + dirX*i;
                    int y = move.MoveFrom.Y + dirY*i;
                    if (x < 0 || y < 0 || x >= 8 || y >= 8)
                    {
                        return false;
                    }
                    
                    if (move.MoveTo.X == x && move.MoveTo.Y == y)
                    {
                        return true;
                    }

                    if (this.GetPiece(x, y) != Piece.Empty)
                    {
                        return false;
                    }

                    
                }
                break;
            case Piece.KingBlack: case Piece.KingWhite:
                if (Math.Abs(moveVectorX) <= 1 && Math.Abs(moveVectorY) <= 1)
                {
                    return true;
                }
                break;
            case Piece.QueenBlack: case Piece.QueenWhite:
                if (Math.Abs(moveVectorX) != Math.Abs(moveVectorY) && Math.Abs(moveVectorX) != 0 &&
                    Math.Abs(moveVectorY) != 0)
                {
                    return false;
                }
                for (int i = 1; i < 8; i++)
                {
                    int x = move.MoveFrom.X + dirX * i;
                    int y = move.MoveFrom.Y + dirY * i;
                    if (x < 0 || y < 0 || x >= 8 || y >= 8)
                    {
                        return false;
                    }
                    
                    if (move.MoveTo.X == x && move.MoveTo.Y == y)
                    {
                        return true;
                    }

                    if (this.GetPiece(x, y) != Piece.Empty)
                    {
                        return false;
                    }

                    
                }
                break;
        }
        
        return false;
    }

    public bool TryPlayMove(Move move)
    {
        bool valid = ValidateMove(move);
        if (valid)
        {
            Board[move.MoveFrom.X, move.MoveFrom.Y] = Piece.Empty;
            Board[move.MoveTo.X, move.MoveTo.Y] = move.MovedPiece;
        }
        return valid;
    }

    public GameState Copy()
    { 
        GameState g = new GameState();
        Array.Copy(this.Board, g.Board, g.Board.Length);
        g.OnTurn = this.OnTurn;
        return g;
    }

    internal void PlayMove(Move move)
    {
        Board[move.MoveFrom.X, move.MoveFrom.Y] = Piece.Empty;
        Board[move.MoveTo.X, move.MoveTo.Y] = move.MovedPiece;
    }
    
    public List<Coordinate> GetPossibleMoves(Coordinate from)
    {

        int x = from.X;
        int y = from.Y;
        Piece movedPiece = this.GetPiece(x, y);

        List<Coordinate> possibleMoves = new List<Coordinate>();

        if (movedPiece == Piece.Empty || movedPiece.GetPlayer() != this.OnTurn)
        {
            return possibleMoves;
        }


        switch (movedPiece)
        {
            case Piece.PawnWhite:
                if (y < 7)
                {
                    if (this.GetPiece(x, y + 1) == Piece.Empty)
                    {
                        possibleMoves.Add(new Coordinate(x, y + 1));
                        if (y < 6)
                        {
                            if (this.GetPiece(x, y + 2) == Piece.Empty && y == 1)
                            {
                                possibleMoves.Add(new Coordinate(x, y + 2));
                            }
                        }
                    }

                    if (x > 0)
                    {
                        if (this.GetPiece(x - 1, y+1).GetPlayer() != OnTurn && this.GetPiece(x - 1, y+1) != Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(x - 1, y+1));
                        }
                    }

                    if (x < 7)
                    {
                        if (this.GetPiece(x + 1, y+1).GetPlayer() != OnTurn && this.GetPiece(x + 1, y+1) != Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(x + 1, y+1));
                        }
                    }
                }

                break;
            case Piece.PawnBlack:
                if (y > 0)
                {
                    if (this.GetPiece(x, y - 1) == Piece.Empty)
                    {
                        possibleMoves.Add(new Coordinate(x, y - 1));
                        if (y > 1)
                        {
                            if (this.GetPiece(x, y - 2) == Piece.Empty && y==6)
                            {
                                possibleMoves.Add(new Coordinate(x, y - 2));
                            }
                        }
                    }

                    if (x > 0)
                    {
                        if (this.GetPiece(x - 1, y-1).GetPlayer() != OnTurn && this.GetPiece(x - 1, y-1) != Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(x - 1, y-1));
                        }
                    }

                    if (x < 7)
                    {
                        if (this.GetPiece(x + 1, y-1).GetPlayer() != OnTurn && this.GetPiece(x + 1, y-1) != Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(x + 1, y-1));
                        }
                    }
                }
                break;
            case Piece.BishopBlack: case Piece.BishopWhite:
                foreach (Tuple<int, int> dir in (Tuple<int,int>[])[new Tuple<int, int>(-1,-1), new Tuple<int, int>(1,-1), new Tuple<int, int>(-1,1), new Tuple<int, int>(1,1)])
                {
                    for (int n = 1; n < 8; n++)
                    {
                        int i = x + dir.Item1 * n;
                        int j = y + dir.Item2 * n;
                        if (i < 0 || j < 0 || i >= 8 || j >= 8)
                        {
                            break;
                        }

                        if (this.GetPiece(i, j) == Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                        }
                        else if (this.GetPiece(i, j).GetPlayer() != this.OnTurn)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                break;
            case Piece.KnightBlack: case Piece.KnightWhite:
                for (int i = -2; i <= 2; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    int j = 3 - Math.Abs(i);
                    
                    if (x + i >= 0 && x + i < 8 && y + j >= 0 && y + j < 8)
                    {
                        if (this.GetPiece(x + i, y + j).GetPlayer() != this.OnTurn)
                        {
                            possibleMoves.Add(new Coordinate(x+i,y+j));
                        }
                    }
                    if (x + i >= 0 && x + i < 8 && y - j >= 0 && y - j < 8)
                    {
                        if (this.GetPiece(x + i, y - j).GetPlayer() != this.OnTurn)
                        {
                            possibleMoves.Add(new Coordinate(x+i,y-j));
                        }
                    }
                    
                }
                break;
            case Piece.RookBlack: case Piece.RookWhite:
                foreach (Tuple<int, int> dir in (Tuple<int,int>[])[new Tuple<int, int>(0,-1), new Tuple<int, int>(1,0), new Tuple<int, int>(-1,0), new Tuple<int, int>(0,1)])
                {
                    for (int n = 1; n < 8; n++)
                    {
                        int i = x + dir.Item1 * n;
                        int j = y + dir.Item2 * n;
                        if (i < 0 || j < 0 || i >= 8 || j >= 8)
                        {
                            break;
                        }

                        if (this.GetPiece(i, j) == Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                        }
                        else if (this.GetPiece(i, j).GetPlayer() != this.OnTurn)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                break;
            case Piece.KingBlack: case Piece.KingWhite:
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (x + i >= 0 && x + i < 8 && y + j >= 0 && y + j < 8)
                        {
                            if (this.GetPiece(x + i, y + j).GetPlayer() != this.OnTurn)
                            {
                                possibleMoves.Add(new Coordinate(x+i,y+j));
                            }
                        }
                    }
                }
                break;
            case Piece.QueenBlack: case Piece.QueenWhite:
                foreach (Tuple<int, int> dir in (Tuple<int,int>[])[new Tuple<int, int>(-1,-1), new Tuple<int, int>(1,-1), new Tuple<int, int>(-1,1), new Tuple<int, int>(1,1),
                                                                    new Tuple<int, int>(0,-1), new Tuple<int, int>(1,0), new Tuple<int, int>(-1,0), new Tuple<int, int>(0,1)])
                {
                    for (int n = 1; n < 8; n++)
                    {
                        int i = x + dir.Item1 * n;
                        int j = y + dir.Item2 * n;
                        if (i < 0 || j < 0 || i >= 8 || j >= 8)
                        {
                            break;
                        }

                        if (this.GetPiece(i, j) == Piece.Empty)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                        }
                        else if (this.GetPiece(i, j).GetPlayer() != this.OnTurn)
                        {
                            possibleMoves.Add(new Coordinate(i,j));
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                break;
        }
        return possibleMoves;
    }

    public void ReverseMove(Move move)
    {
        this.Board[move.MoveFrom.X, move.MoveFrom.Y] = move.MovedPiece;
        this.Board[move.MoveTo.X, move.MoveTo.Y] = move.TakenPiece;
    }

    public bool isOnBoard(Piece piece)
    {
        foreach (Piece p in this.Board)
        {
            if (p == piece)
            {
                return true;
            }
        }
        return false;
    }
}

class MoveBuilder
{
    public GameState startingState;
    public GameState currentState;
    public List<Move> moves;

    public MoveBuilder(GameState state)
    {
        this.startingState = state.Copy();
        this.currentState = state.Copy();
        this.moves = new List<Move>();
    }

    

    public bool HasPlayed(Coordinate coord)
    {
        foreach (Move m in this.moves)
        {
            if (m.MoveTo.X == coord.X && m.MoveTo.Y == coord.Y)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool Add(Move move)
    {
        if (this.HasPlayed(move.MoveFrom))
        {
            return false;
        }
        if (currentState.ValidateMove(move))
        {
            this.moves.Add(move);
            this.currentState.PlayMove(move);
            return true;
        }
        return false;
    }
    
    public List<int> FindDependentMoves(Coordinate destination)
    {
        int index = this.FindMoveIndex(destination);
        if (index == -1)
        {
            return new List<int>();
        }
        List<int> moveList = new List<int>();
        GameState state = this.startingState.Copy();
        List<Move> copiedMoves = new List<Move>();
        for (int i = 0; i < this.moves.Count; i++)
        {

            copiedMoves.Add(this.moves[i].Copy());
            
        }
        
        for (int i = 0; i < copiedMoves.Count; i++)
        {
            if (i == index)
            {
                continue;
            }
            
            if (state.ValidateMove(copiedMoves[i]))
            {
                state.PlayMove(copiedMoves[i]);
            }
            else
            {
                moveList.Add(i);
            }
        }
        return moveList;
    }

    public int FindMoveIndex(Coordinate destination)
    {
        for (int i = 0; i < this.moves.Count; i++)
        {
            if (this.moves[i].MoveTo.X == destination.X && this.moves[i].MoveTo.Y == destination.Y)
            {
                return i;
            }
        }
        return -1;
    }
    
    public void Remove(Coordinate destination)
    {
        int index = FindMoveIndex(destination);
        if (index == -1)
        {
            return;
        }
        bool removed = false;
        List<int> moveList = this.FindDependentMoves(destination);
        
        for (int i = moveList.Count - 1; i >= 0; i -= 1)
        {
            if (removed == false && index > moveList[i])
            {
                currentState.ReverseMove(this.moves[index]);
                this.moves.RemoveAt(index);
                removed = true;
            }
            currentState.ReverseMove(this.moves[moveList[i]]);
            this.moves.RemoveAt(moveList[i]);
        }

        if (removed == false)
        {
            currentState.ReverseMove(this.moves[index]);
            this.moves.RemoveAt(index);
        }
    }
    
}