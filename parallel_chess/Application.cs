using System.Runtime.InteropServices;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using System.Numerics;

namespace parallel_chess;

class Application
{
    
    private Color blackSquareColor = Color.DarkGreen;
    private Color whiteSquareColor = Color.Beige;
    private Color backgroundAnnotationColor = Color.Black;
    private Color textAnnotationColor = Color.White;
    
    private Color movedPieceBorderColor = Raylib.GetColor(0xff9999ff);
    
    private Color selectedSquareColor = Raylib.GetColor(0xff000080);
    private Color possibleMoveColor = Raylib.GetColor(0x0000ff80);
    private Color dependentMoveColor = Raylib.GetColor(0xff00ff80);

    private Vector4 blackSquareColorGUI;
    private Vector4 whiteSquareColorGUI;
    private Vector4 backgroundAnnotationColorGUI;
    private Vector4 textAnnotationColorGUI;
    
    private Vector4 movedPieceBorderColorGUI;
    
    private Vector4 selectedSquareColorGUI;
    private Vector4 possibleMoveColorGUI;
    private Vector4 dependentMoveColorGUI;
    
    private bool waitForPromotion = false;
    private Coordinate promotionSquare;

    private Player winner = Player.None;
    
    private bool settings = false;
    private KeyboardKey settingsKey = Raylib_cs.KeyboardKey.LeftControl;
    

    private int windowWidth = Raylib.GetRenderWidth();
    private int windowHeight = Raylib.GetRenderHeight();

    private int boardSize;
    private int boardOffsetX;
    private int boardOffsetY;

    private int annotationSize = 20;
    private float annotationFontSize = 10;
    private Font annotationFont = Raylib.GetFontDefault();
    private int margin = 10;

    private int mouseX;
    private int mouseY;
    private bool mouseOnBoard;

    private Coordinate selectedSquare = new(-1,-1);
    
    private Dictionary<Piece, Texture2D> pieceTextures;
    private Dictionary<Piece, Texture2D> borderTextures;

    private Image iconWhite;
    private Image iconBlack;


    private KeyboardKey playMovesKey = Raylib_cs.KeyboardKey.Enter;
    
    
    private GameState gameState = new();
    private MoveBuilder moveBuilder;
    
    
    public void Start()
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(800, 800, "Parallel Chess - White on turn");
            Raylib.SetWindowMinSize(600,600);
            Raylib.SetWindowMaxSize(Raylib.GetMonitorWidth(Raylib.GetCurrentMonitor()),Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor()));
            
            Raylib.SetTargetFPS(30);
            
            rlImGui.Setup(true);	
            
            whiteSquareColorGUI = Raylib.ColorNormalize(whiteSquareColor);
            blackSquareColorGUI = Raylib.ColorNormalize(blackSquareColor);
            backgroundAnnotationColorGUI = Raylib.ColorNormalize(backgroundAnnotationColor);
            textAnnotationColorGUI = Raylib.ColorNormalize(textAnnotationColor);
            
            movedPieceBorderColorGUI = Raylib.ColorNormalize(movedPieceBorderColor);
            
            selectedSquareColorGUI = Raylib.ColorNormalize(selectedSquareColor);
            possibleMoveColorGUI = Raylib.ColorNormalize(possibleMoveColor);
            dependentMoveColorGUI = Raylib.ColorNormalize(dependentMoveColor);
            
            pieceTextures = new()
            {
                {Piece.PawnWhite,Raylib.LoadTexture("pawn_white.png")},
                {Piece.PawnBlack,Raylib.LoadTexture("pawn_black.png")},
                {Piece.BishopWhite,Raylib.LoadTexture("bishop_white.png")},
                {Piece.BishopBlack,Raylib.LoadTexture("bishop_black.png")},
                {Piece.KnightWhite,Raylib.LoadTexture("knight_white.png")},
                {Piece.KnightBlack,Raylib.LoadTexture("knight_black.png")},
                {Piece.RookWhite,Raylib.LoadTexture("rook_white.png")},
                {Piece.RookBlack,Raylib.LoadTexture("rook_black.png")},
                {Piece.QueenWhite,Raylib.LoadTexture("queen_white.png")},
                {Piece.QueenBlack,Raylib.LoadTexture("queen_black.png")},
                {Piece.KingWhite,Raylib.LoadTexture("king_white.png")},
                {Piece.KingBlack,Raylib.LoadTexture("king_black.png")},
                {Piece.Empty,Raylib.LoadTexture("empty.png")},
            };
            
            borderTextures = new()
            {
                {Piece.PawnWhite,Raylib.LoadTexture("pawn_border.png")},
                {Piece.PawnBlack,Raylib.LoadTexture("pawn_border.png")},
                {Piece.BishopWhite,Raylib.LoadTexture("bishop_border.png")},
                {Piece.BishopBlack,Raylib.LoadTexture("bishop_border.png")},
                {Piece.KnightWhite,Raylib.LoadTexture("knight_border.png")},
                {Piece.KnightBlack,Raylib.LoadTexture("knight_border.png")},
                {Piece.RookWhite,Raylib.LoadTexture("rook_border.png")},
                {Piece.RookBlack,Raylib.LoadTexture("rook_border.png")},
                {Piece.QueenWhite,Raylib.LoadTexture("queen_border.png")},
                {Piece.QueenBlack,Raylib.LoadTexture("queen_border.png")},
                {Piece.KingWhite,Raylib.LoadTexture("king_border.png")},
                {Piece.KingBlack,Raylib.LoadTexture("king_border.png")},
                {Piece.Empty,Raylib.LoadTexture("empty.png")},
            };

            iconWhite = Raylib.LoadImage("icon_white.png");
            iconBlack = Raylib.LoadImage("icon_black.png");
            
            Raylib.SetWindowIcon(iconWhite);
            
            moveBuilder = new MoveBuilder(gameState);
            
            windowWidth = Raylib.GetRenderWidth();
            windowHeight = Raylib.GetRenderHeight();

            boardSize = Math.Min(windowWidth - 2 * margin, windowHeight - 2 * margin);

            boardOffsetX = (windowWidth - boardSize) / 2;
            boardOffsetY = (windowHeight - boardSize) / 2;

            annotationSize = Math.Max(20, Math.Min(boardSize / 30, 50));
            annotationFontSize = annotationSize / 2;

            Console.Clear();
            this.mainLoop();

            
            rlImGui.Shutdown();	
            Raylib.CloseWindow();
        }

    private void mainLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsWindowResized())
            {
                windowWidth = Raylib.GetRenderWidth();
                windowHeight = Raylib.GetRenderHeight();

                boardSize = Math.Min(windowWidth - 2 * margin, windowHeight - 2 * margin);

                boardOffsetX = (windowWidth - boardSize) / 2;
                boardOffsetY = (windowHeight - boardSize) / 2;

                annotationSize = Math.Max(20, Math.Min(boardSize / 30, 50));
                annotationFontSize = annotationSize / 2;
            }
            
            mouseX = (8*(Raylib.GetMouseX()-boardOffsetX-annotationSize))/(boardSize-2*annotationSize);
            mouseY = 7 - (8*(Raylib.GetMouseY()-boardOffsetY-annotationSize))/(boardSize-2*annotationSize);
            mouseOnBoard = (Raylib.GetMouseX()>boardOffsetX+annotationSize) && (Raylib.GetMouseY()>boardOffsetY+annotationSize) && (Raylib.GetMouseX()<boardOffsetX+boardSize)
                           && (Raylib.GetMouseX()<boardOffsetX+boardSize-annotationSize) && (Raylib.GetMouseY() < boardOffsetY + boardSize - annotationSize);
            
            
            //inputs
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                onLeftClick();
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                onRightClick();
            }

            
            if (Raylib.IsKeyPressed(playMovesKey) && !settings && !waitForPromotion && winner == Player.None && moveBuilder.moves.Count > 0)
            {
                gameState = moveBuilder.currentState.Copy();
                if (gameState.OnTurn == Player.Black)
                {
                    if (!gameState.isOnBoard(Piece.KingWhite))
                    {
                        winner = Player.Black;
                    }
                    gameState.OnTurn = Player.White;
                    Raylib.SetWindowTitle("Parallel Chess - White on turn");
                    Raylib.SetWindowIcon(iconWhite);
                }
                else
                {
                    if (!gameState.isOnBoard(Piece.KingBlack))
                    {
                        winner = Player.White;
                    }
                    gameState.OnTurn = Player.Black;
                    Raylib.SetWindowTitle("Parallel Chess - Black on turn");
                    Raylib.SetWindowIcon(iconBlack);
                }
                moveBuilder = new MoveBuilder(gameState);
            }
            
            if (Raylib.IsKeyPressed(settingsKey))
            {
                settings = !settings;
            }
            
            
            //drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.White);
            
            
            this.Draw();
            
            if (winner != Player.None)
            {
                drawWinMessage(winner);
            }
            
            
            rlImGui.Begin();
            if (settings)
            {
                drawSettings();
                settingsUpdate();
            }

            if (waitForPromotion)
            {
                doPromotion();
            }
            rlImGui.End();
            
            Raylib.EndDrawing();
            
            
        }
    }

    private void Draw()
    {
        this.drawBoard(moveBuilder.currentState);
        this.drawPieces(moveBuilder.currentState);
    }
    
    private void drawBoard(GameState state)
    {
        //draw board
        Raylib.DrawRectangle(boardOffsetX,boardOffsetY,boardSize,boardSize,backgroundAnnotationColor);
        
        
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //to prevent rounding errors leaving spaces between squares
                int startX = i * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize;
                int startY = j * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize;
                int endX = (i + 1) * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize;
                int endY = (j + 1) * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize;

                Raylib.DrawRectangle(startX, startY, endX - startX, endY - startY,
                    (i + j) % 2 == 0 ? whiteSquareColor : blackSquareColor);

              
            }
        }

        List<Coordinate> highlightSquares;
        Color highlightColor = possibleMoveColor;
        
        bool hasSelectedPlayed = false;
        if (selectedSquare.X == -1 || selectedSquare.Y == -1)
        {
            highlightSquares = new List<Coordinate>();
            
        }
        else
        {
            hasSelectedPlayed = moveBuilder.HasPlayed(selectedSquare);
            if (hasSelectedPlayed)
            {
                highlightColor = dependentMoveColor;
                List<int> moveIndecies = moveBuilder.FindDependentMoves(selectedSquare);
                highlightSquares = new List<Coordinate>();
                Coordinate movedFrom = moveBuilder.moves[moveBuilder.FindMoveIndex(selectedSquare)].MoveFrom;
                Raylib.DrawRectangle(movedFrom.X * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize,
                    (7 - movedFrom.Y) * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize,
                    (movedFrom.X + 1) * (boardSize - 2 * annotationSize) / 8 - movedFrom.X * (boardSize - 2 * annotationSize) / 8,
                    (8 - movedFrom.Y) * (boardSize - 2 * annotationSize) / 8 - (7 - movedFrom.Y) * (boardSize - 2 * annotationSize) / 8,
                    selectedSquareColor);
                
                foreach (int m in moveIndecies)
                {
                    Move move = moveBuilder.moves[m];
                    highlightSquares.Add(move.MoveTo);
                }
            }
            else
            {
                highlightSquares = state.GetPossibleMoves(selectedSquare);
            }

            Raylib.DrawRectangle(selectedSquare.X * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize,
                (7 - selectedSquare.Y) * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize,
                (selectedSquare.X + 1) * (boardSize - 2 * annotationSize) / 8 - selectedSquare.X * (boardSize - 2 * annotationSize) / 8,
                (8 - selectedSquare.Y) * (boardSize - 2 * annotationSize) / 8 - (7 - selectedSquare.Y) * (boardSize - 2 * annotationSize) / 8,
                selectedSquareColor);
        }
        foreach (Coordinate c in highlightSquares)
        {
            int startX = c.X * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize;
            int startY = (7 - c.Y) * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize;
            int endX = (c.X + 1) * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize;
            int endY = (8 - c.Y) * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize;
            Raylib.DrawRectangle(startX, startY, endX - startX, endY - startY, highlightColor);
        }

        
        
        //draw annotations
        string annotationY = "12345678";
        string annotationX = "abcdefgh";
        for (int i = 0; i < 8; i++)
        {
            
            int start = i * (boardSize - 2*annotationSize) / 8;
            int end = (i+1) * (boardSize - 2*annotationSize) / 8;
            int center = (start+end)/2;

            string text = annotationY[(7-i)..(7-i+1)];
            //Vector2 textSize = Raylib.MeasureTextEx(annotationFont, text, annotationFontSize,0);
            Vector2 textSize = new Vector2(annotationSize/2, annotationSize/2);
            Vector2 position = new Vector2(boardOffsetX + (annotationSize - (int)textSize[0]) / 2, boardOffsetY + annotationSize + center - (int)textSize[1] / 2);
            Raylib.DrawTextEx(annotationFont,text,position,annotationFontSize,0,textAnnotationColor);
            position[0] += boardSize - annotationSize;
            Raylib.DrawTextEx(annotationFont,text,position,annotationFontSize,0,textAnnotationColor);
            
            
            text = annotationX[i..(i+1)];
            //textSize = Raylib.MeasureTextEx(annotationFont, text, annotationFontSize,0);
            position = new Vector2(boardOffsetX + annotationSize + center - (int)textSize[0] / 2, boardOffsetY + (annotationSize - (int)textSize[1]) / 2);
            Raylib.DrawTextEx(annotationFont,text,position,annotationFontSize,0,textAnnotationColor);
            position[1] += boardSize - annotationSize;
            Raylib.DrawTextEx(annotationFont,text,position,annotationFontSize,0,textAnnotationColor);
        }
        
        
    }
    
    private void drawPieces(GameState state)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //to prevent rounding errors leaving spaces between squares
                int startX = i * (boardSize - 2 * annotationSize) / 8 + boardOffsetX + annotationSize;
                int startY = j * (boardSize - 2 * annotationSize) / 8 + boardOffsetY + annotationSize;

                bool hasPlayed = moveBuilder.HasPlayed(new Coordinate(i, 7 - j));
                Color borderColor = Color.White;
                if (hasPlayed)
                {
                    borderColor = movedPieceBorderColor;
                }
                
                //draw pieces
                Texture2D borderImage = borderTextures[state.GetPiece(i, 7 - j)];
                
                Raylib.DrawTextureEx(borderImage, new Vector2(startX, startY), 0,
                    (float)(boardSize - 2 * annotationSize) / 800, borderColor);
                
                Texture2D pieceImage = pieceTextures[state.GetPiece(i, 7 - j)];
                Raylib.DrawTextureEx(pieceImage, new Vector2(startX, startY), 0,
                    (float)(boardSize - 2 * annotationSize) / 800, Color.White);

            }
        }
    }

    private void drawWinMessage(Player winner)
    {

        int offsetX = boardOffsetX + 2 * annotationSize;
        int offsetY = boardOffsetY+boardSize/3;
        int sizeX = boardSize - 4 * annotationSize;
        int sizeY = boardSize / 3;
        Raylib.DrawRectangle(offsetX,offsetY,sizeX,sizeY,Raylib.Fade(backgroundAnnotationColor,0.7f));
        
        float fontSize = boardSize/8;
        float textSpacing = boardSize / 40;
        
        string text = winner + " won!";
        //Vector2 textSize = Raylib.MeasureTextEx(annotationFont, text, fontSize, textSpacing);
        Vector2 textSize = new Vector2((int)(5.8*fontSize), (int)(0.6*fontSize));
        Vector2 position = new Vector2(offsetX + (sizeX-textSize[0])/2, offsetY + (sizeY-textSize[1])/2);
        Raylib.DrawTextEx(annotationFont, text, position, fontSize, textSpacing, textAnnotationColor);

    }
    
    private void drawSettings()
    {
        ImGui.Begin("Settings");
        
        ImGui.Text("Game options");
        if (ImGui.Button("Reset Move"))
        {
            moveBuilder = new MoveBuilder(gameState);
            waitForPromotion = false;
        }

        if (ImGui.Button("Reset Game"))
        {
            gameState = new GameState();
            moveBuilder = new MoveBuilder(gameState);
            winner = Player.None;
            waitForPromotion = false;
        }

        
        ImGui.Spacing();
        //color settings
        ImGui.Text("Style Settings");
        ImGui.ColorEdit4("White Square Color",ref whiteSquareColorGUI);
        ImGui.ColorEdit4("Black Square Color",ref blackSquareColorGUI);
        ImGui.ColorEdit4("Chessboard Border Color", ref backgroundAnnotationColorGUI);
        ImGui.ColorEdit4("Chessboard Annotation Color",ref textAnnotationColorGUI);
        
        ImGui.ColorEdit4("Moved Piece Border Highlight", ref movedPieceBorderColorGUI);
        
        ImGui.ColorEdit4("Selected Square Highlight",ref selectedSquareColorGUI);
        ImGui.ColorEdit4("Possible Move Highlight",ref possibleMoveColorGUI);
        ImGui.ColorEdit4("Dependent Move Highlight",ref dependentMoveColorGUI);
        

        ImGui.End();
    }

    private void settingsUpdate()
    {
        whiteSquareColor = Raylib.ColorFromNormalized(whiteSquareColorGUI);
        blackSquareColor = Raylib.ColorFromNormalized(blackSquareColorGUI);
        backgroundAnnotationColor = Raylib.ColorFromNormalized(backgroundAnnotationColorGUI);
        textAnnotationColor = Raylib.ColorFromNormalized(textAnnotationColorGUI);
        
        movedPieceBorderColor = Raylib.ColorFromNormalized(movedPieceBorderColorGUI);
        
        selectedSquareColor = Raylib.ColorFromNormalized(selectedSquareColorGUI);
        possibleMoveColor = Raylib.ColorFromNormalized(possibleMoveColorGUI);
        dependentMoveColor = Raylib.ColorFromNormalized(dependentMoveColorGUI);
        
    }

    private void doPromotion()
    {
        ImGui.Begin("Promotion");

        if (ImGui.Button("Return Move",new Vector2(boardSize/8+6,boardSize/8+6)))
        {
            moveBuilder.Remove(moveBuilder.moves.Last().MoveTo);
            waitForPromotion = false;
        }
        ImGui.SameLine();
        
        
        int pieceId = gameState.OnTurn == Player.White ? 6 : 7;
        
            
        if (ImGui.ImageButton("Bishop", pieceId, new Vector2(boardSize/8, boardSize/8)))
        {
            Piece p = gameState.OnTurn == Player.White ? Piece.BishopWhite : Piece.BishopBlack;
            moveBuilder.currentState.Board[promotionSquare.X,promotionSquare.Y] = p;
            waitForPromotion = false;
        }
        ImGui.SameLine();
        pieceId += 2;
        if (ImGui.ImageButton("Knight", pieceId, new Vector2(boardSize/8, boardSize/8)))
        {
            Piece p = gameState.OnTurn == Player.White ? Piece.KnightWhite : Piece.KnightBlack;
            moveBuilder.currentState.Board[promotionSquare.X,promotionSquare.Y] = p;
            waitForPromotion = false;
        }
        ImGui.SameLine();
        pieceId += 2;
        if (ImGui.ImageButton("Rook", pieceId, new Vector2(boardSize/8, boardSize/8)))
        {
            Piece p = gameState.OnTurn == Player.White ? Piece.RookWhite : Piece.RookBlack;
            moveBuilder.currentState.Board[promotionSquare.X,promotionSquare.Y] = p;
            waitForPromotion = false;
        }
        ImGui.SameLine();
        pieceId += 2;
        if (ImGui.ImageButton("Queen", pieceId, new Vector2(boardSize/8, boardSize/8)))
        {
            Piece p = gameState.OnTurn == Player.White ? Piece.QueenWhite : Piece.QueenBlack;
            moveBuilder.currentState.Board[promotionSquare.X,promotionSquare.Y] = p;
            waitForPromotion = false;
        }
        ImGui.End();
        
    }
    
    private void onLeftClick()
    {
        if (!mouseOnBoard || settings || waitForPromotion || winner != Player.None)
        {
            return;
        }
        Coordinate newPos = new Coordinate(mouseX, mouseY);

        if (selectedSquare.X == -1 || selectedSquare.Y == -1)
        {
            selectedSquare = newPos;
            return;
        }

        Move m = new Move(selectedSquare, new Coordinate(mouseX, mouseY));
        bool succesfull = moveBuilder.Add(m);
        if (succesfull)
        {
            selectedSquare = new Coordinate(-1, -1);
            if ((m.MovedPiece == Piece.PawnWhite && m.MoveTo.Y == 7) ||
                (m.MovedPiece == Piece.PawnBlack && m.MoveTo.Y == 0))
            {
                waitForPromotion = true;
                promotionSquare = m.MoveTo;
            }
            return;
        }

        selectedSquare = newPos;

    }

    private void onRightClick()
    {
        if (!mouseOnBoard || settings || waitForPromotion || winner != Player.None)
        {
            return;
        }

        if (selectedSquare.X == mouseX && selectedSquare.Y == mouseY)
        {
            if (moveBuilder.HasPlayed(selectedSquare))
            {
                moveBuilder.Remove(selectedSquare);
                selectedSquare = new Coordinate(-1, -1);
            }
        }
        else
        {
            selectedSquare = new Coordinate(mouseX, mouseY);
        }
    }
}
