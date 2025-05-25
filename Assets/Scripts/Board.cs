using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngineInternal;

public class Board : MonoBehaviour
{
    const int DEPTH = 4;
    public const int TILE_COUNT_X = 8;
    public const int TILE_COUNT_Y = 8;
    public const int PIECES_COUNT = 12;
    const int WIN_SCORE = 1000000;

    const float EDITOR_POS = -2.09f;
    const float EDITOR_SCALE = 0.9f;

    const float EDGE_MOD = -3.5f;
    const float Z_POS = -1f;

    public enum GameMode
    {
        BoardEditor = 0,
        PvP = 1,
        PvAI = 2
    }

    public GameMode gameMode;

    public bool isReversed = false;
    public bool awaitingPromotionChoice = false;
    public bool isPause = false;

    public int i = 0;
    public int playerTurn = 0;
    public int aiSide = 0;
    public int fiftyMovesRuleCount = 0;

    public GameObject pieceBar;
    public GameObject UIManager; 

    public GameObject[] prefabs;

    public GameObject tileCollider;
    public GameObject[,] tiles;
    public GameObject highlight;

    public GameObject point;
    public GameObject killPoint;

    public GameObject pieceChoiceWhite;
    public GameObject pieceChoiceBlack;

    public ChessPiece currentFocus = null;
    public ChessPiece[,] chessPieces;

    public static Vector2Int lastMoveW;
    public static Vector2Int lastMoveB;

    private bool gameEnded = false;

    private Vector2Int bestMoveW; 
    private ChessPiece bestPieceW;
    private Vector2Int bestMoveB;
    private ChessPiece bestPieceB;

    private Vector2Int promotionMove;

    private List<GameObject> currentPoints = new List<GameObject>();
    private List<GameObject> tilesSelected = new List<GameObject>();
    private List<Vector2Int> availableMoves = new List<Vector2Int>();

    void Start()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        UIManager.GetComponent<UIManager>().SetAvailableActions();
        SetTiles();
        Zobrist.InitializeAndFillTable();
    }

    void Update()
    {
        if (!isPause) 
            switch (gameMode)
            {
                case GameMode.BoardEditor:
                    var barScript = pieceBar.GetComponent<PieceBar>();
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (EventSystem.current.IsPointerOverGameObject())
                            break;

                        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

                        if (hit.collider != null)
                        {
                            int tileX = Convert.ToInt32(hit.collider.GetComponent<Tile>().cordX);
                            int tileY = Convert.ToInt32(hit.collider.GetComponent<Tile>().cordY);

                            switch (barScript.state)
                            {
                                case PieceBar.State.Spawn:
                                    if (chessPieces[tileX, tileY] == null)
                                    {
                                        chessPieces[tileX, tileY] = SpawnOnePiece(barScript.pieceType, barScript.pieceTeam);

                                        PositionOnePiece(tileX, tileY);

                                        UIManager.GetComponent<UIManager>().SetAvailableActions();
                                    }
                                    break;
                                case PieceBar.State.Select:
                                    if (currentFocus == null)
                                    {
                                        if (chessPieces[tileX, tileY] != null)
                                        {
                                            currentFocus = chessPieces[tileX, tileY];
                                        }
                                    }
                                    else
                                    {
                                        if (currentFocus.currentX != tileX || currentFocus.currentY != tileY)
                                            BasicMove(chessPieces, currentFocus.currentX, currentFocus.currentY, tileX, tileY);

                                        currentFocus = null;

                                        UIManager.GetComponent<UIManager>().SetAvailableActions();
                                    }
                                    break;
                                case PieceBar.State.Delete:
                                    if (chessPieces[tileX, tileY] != null)
                                    {
                                        Destroy(chessPieces[tileX, tileY].gameObject);
                                        chessPieces[tileX, tileY] = null;
                                        UIManager.GetComponent<UIManager>().SetAvailableActions();
                                    }
                                    break;
                            }
                            break;
                        }
                    }
                    break;
                case GameMode.PvAI:
                    if (!gameEnded)
                    {
                        if (aiSide == 0)
                        {
                            if (playerTurn == 0)
                                AIAction(true);
                            else
                                PlayerAction();
                        }
                        else
                        {
                            if (playerTurn == 0)
                                PlayerAction();
                            else
                                AIAction(false);
                        }
                    }

                    break;
                case GameMode.PvP:
                    if (!gameEnded)
                        PlayerAction();
                    break;
                default:
                    Debug.Log("Invalid Game Mode.");
                    break;
            }
        }

    public void EnterMode(int mode)
    {
        PieceBar pieceBarScript = pieceBar.GetComponent<PieceBar>();
        UIManager UIManagerScript = UIManager.GetComponent<UIManager>();

        switch (mode)
        {
            case (int)GameMode.BoardEditor:
                gameMode = GameMode.BoardEditor;

                pieceBarScript.state = PieceBar.State.Select;
                pieceBarScript.ButtonHover(pieceBarScript.selectHover);

                UIManagerScript.SetUIActive();
                UIManagerScript.SetAvailableActions();
                UIManagerScript.InGameUI.gameObject.SetActive(false);

                HideAvailableMoves();
                CancelHighlight();

                gameObject.transform.position = new Vector3 (EDITOR_POS, 0, 0);
                gameObject.transform.localScale = new Vector3(EDITOR_SCALE, EDITOR_SCALE, EDITOR_SCALE);

                break;
            case (int)GameMode.PvP:
                gameMode = GameMode.PvP;

                UIManagerScript.InGameUI.gameObject.SetActive(true);
                
                Zobrist.Clear();

                gameObject.transform.position = new Vector3(0, 0, 0);
                gameObject.transform.localScale = new Vector3(1, 1, 1);

                fiftyMovesRuleCount = 0;
                gameEnded = false;
                break;
            case (int)GameMode.PvAI:
                UIManagerScript.InGameUI.gameObject.SetActive(true);
                gameMode = GameMode.PvAI;

                Zobrist.Clear();

                gameObject.transform.position = new Vector3(0, 0, 0);
                gameObject.transform.localScale = new Vector3(1, 1, 1);

                fiftyMovesRuleCount = 0;
                gameEnded = false;
                break;
            default:
                Debug.Log("Invalid mode");
                break;
        }

        currentFocus = null;
    }

    public bool IsCheck(ChessPiece[,] board, int team)
    {
        int kingPosX = 0;
        int kingPosY = 0;

        foreach (var piece in board)
        {
            if (piece != null && piece.team == team && piece.type == PieceType.King)
            {
                kingPosX = piece.currentX;
                kingPosY = piece.currentY;
            }
        }

        foreach (ChessPiece chessPiece in board)
        {
            if (chessPiece != null && chessPiece.team != team)
            {
                List<Vector2Int> currentMoves = chessPiece.GetAvailableMoves(board);

                if (currentMoves.Contains(new Vector2Int(kingPosX, kingPosY)))
                    return true;
            }
        }

        return false;
    }

    public bool IsCheckMate(ChessPiece[,] board, int team)
    {
        if (!IsAnyMoveAvailable(board, team) && IsCheck(board, team))
            return true;
        else 
            return false;
    }

    public bool IsNull(ChessPiece[,] board, int team)
    {
        //Stalemate
        if (!IsAnyMoveAvailable(board, team) && !IsCheck(board, team))
            return true;

        //Insufficient material 
        int material = 0;
        List<ChessPiece> currentPieces = new List<ChessPiece>();

        foreach (var piece in board)
        {
            if (piece != null && piece.type != PieceType.King)
            {
                material += piece.pieceValue;
                currentPieces.Add(piece);
            }
                
        }
        if (material == 0) //Only kings
            return true;
        else if ((material == 320 || material == 330) && currentPieces.Count == 1) //King and knight or king and bishop
            return true;
        else if ((material == 660) && currentPieces.Count == 2 && currentPieces[0].team != currentPieces[1].team)
        {
            if ((currentPieces[0].currentX + currentPieces[0].currentY) % 2 == (currentPieces[1].currentX + currentPieces[1].currentY) % 2) //King + bishop vs king + bishop with different tile color
                return true;
        }

        return false;
    }

    public void OnPromotionPieceSelected(int type)
    {
        PieceType chosen = (PieceType)type;
        
        if (currentFocus.team == 0)
            pieceChoiceWhite.SetActive(false);
        else
            pieceChoiceBlack.SetActive(false);

        MoveOnePiece(chessPieces, currentFocus.currentX, currentFocus.currentY, promotionMove.x, promotionMove.y, chosen);

        HideAvailableMoves();
        currentFocus = null;

        awaitingPromotionChoice = false;
    }

    public void ClearBoard()
    {
        for (int i = 0; i < TILE_COUNT_X; i++)
            for (int j = 0; j < TILE_COUNT_Y; j++)
                if (chessPieces[i, j] != null)
                {
                    Destroy(chessPieces[i, j].gameObject);
                    chessPieces[i, j] = null;
                }

        UIManager.GetComponent<UIManager>().SetAvailableActions();
    }

    public void SetBasicPos()
    {
        ClearBoard();
        SpawnAllPieces();
        PositionAllPieces();
        UIManager.GetComponent<UIManager>().SetAvailableActions();
    }

    private ChessPiece SpawnOnePiece(PieceType type, int team)
    {
        int teamModify = 0;
        if (team == 1) teamModify = 6;

        ChessPiece chessPiece = Instantiate(prefabs[(int)type + teamModify - 1], transform).GetComponent<ChessPiece>();

        chessPiece.type = type;
        chessPiece.team = team;

        return chessPiece;
    }

    private void SpawnAllPieces()
    {
        int whiteTeam = 0;
        int blackTeam = 1;

        chessPieces[0, 0] = SpawnOnePiece(PieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnOnePiece(PieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnOnePiece(PieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnOnePiece(PieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnOnePiece(PieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnOnePiece(PieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnOnePiece(PieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnOnePiece(PieceType.Rook, whiteTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnOnePiece(PieceType.Pawn, whiteTeam);
        }

        chessPieces[0, 7] = SpawnOnePiece(PieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnOnePiece(PieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnOnePiece(PieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnOnePiece(PieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnOnePiece(PieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnOnePiece(PieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnOnePiece(PieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnOnePiece(PieceType.Rook, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnOnePiece(PieceType.Pawn, blackTeam);
        }
    }

    private void PositionOnePiece(int x, int y)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetMoveVector(x, y);
        if (isReversed)
            chessPieces[x, y].transform.Rotate(0, 0, 180);
    }

    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0;  y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null) 
                    PositionOnePiece(x, y);
            }
        }
    }

    private Vector3 GetMoveVector(int posX, int posY)
    {
        Vector3 startPos = transform.position;
        Vector3 changeVector;

        if (isReversed)
            changeVector = Vector3.Scale(new Vector3(EDGE_MOD - posX + TILE_COUNT_X - 1, EDGE_MOD - posY + TILE_COUNT_Y - 1, 0f), transform.localScale);
        else
            changeVector = Vector3.Scale(new Vector3(EDGE_MOD + posX, EDGE_MOD + posY, 0f), transform.localScale);

        return changeVector + new Vector3(startPos.x, startPos.y, Z_POS);
    }

    private void SetTiles()
    {
        Vector3 startPos = transform.position;

        tiles = new GameObject[TILE_COUNT_X, TILE_COUNT_Y];

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                tiles[x, y] = Instantiate(tileCollider, transform);
                tiles[x, y].GetComponent<Tile>().cordX = x;
                tiles[x, y].GetComponent<Tile>().cordY = y;
                tiles[x, y].transform.position = GetMoveVector(x, y);
            }
        }
    }

    private void BasicMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
    {
        //Move for board editor mode
        board[startX, startY].currentX = endX;
        board[startX, startY].currentY = endY;

        if (board[endX, endY] != null)
            Destroy(board[endX, endY].gameObject);

        board[startX, startY].transform.position = GetMoveVector(endX, endY);
        board[endX, endY] = board[startX, startY];
        board[startX, startY] = null;
    }
    private void MoveOnePiece(ChessPiece[,] board, int startX, int startY, int endX, int endY, PieceType changeType)
    {
        //3 Moves repetition rule
        ulong newHash = board[startX, startY].team == 0 ? Zobrist.ComputeHash(board, false) : Zobrist.ComputeHash(board, true);
        int count = 0;

        if (Zobrist.occuredPositions.ContainsKey(newHash))
            Zobrist.occuredPositions[newHash]++;
        else
            Zobrist.occuredPositions[newHash] = 1;

        if (Zobrist.occuredPositions.TryGetValue(newHash, out count) && count >= 3)
            FinishGame(false, playerTurn);

        //50 Moves rule
        if (board[startX, startY].type == PieceType.Pawn || board[endX, endY] != null)
            fiftyMovesRuleCount = 0;
        else if (board[startX, startY].team == 0)
            fiftyMovesRuleCount++;

        if (fiftyMovesRuleCount == 50 && board[startX, startY].team == 1)
            FinishGame(false, playerTurn);

        //Tile highlight
        if (tilesSelected.Count >= 2)
            CancelHighlight();

        if (tilesSelected.Count % 2 == 0)
        {
            HighlightTile(startX, startY);
            HighlightTile(endX, endY);
        }
        else
            HighlightTile(endX, endY);

        board[startX, startY].movesMade += 1;

        //Castling
        if (board[startX, startY].type == PieceType.King)
        {
            if (startX == 4)
            {
                if (endX == startX + 2)
                {
                    board[TILE_COUNT_X - 1, startY].currentX = startX + 1;
                    board[TILE_COUNT_X - 1, startY].transform.position = GetMoveVector(startX + 1, startY);
                    board[startX + 1, startY] = board[TILE_COUNT_X - 1, startY];
                    board[TILE_COUNT_X - 1, startY] = null;
                }
                else if (endX == startX - 2)
                {
                    board[0, startY].currentX = startX - 1;
                    board[0, startY].transform.position = GetMoveVector(startX - 1, startY);
                    board[startX - 1, startY] = board[0, startY];
                    board[0, startY] = null;
                }
            }
        }

        //En passant 
        if (board[startX, startY].type == PieceType.Pawn && endX != startX && board[endX, endY] == null)
        {
            int direction = (board[startX, startY].team == 0) ? 1 : -1;
            Destroy(board[endX, endY - direction].gameObject);
            board[endX, endY - direction] = null;
        }

        //Promotion
        if (board[startX, startY].type == PieceType.Pawn && ((board[startX, startY].team == 0 && endY == TILE_COUNT_Y - 1) || (board[startX, startY].team == 1 && endY == 0)))
        {
            ChessPiece promoted = SpawnOnePiece(changeType, board[startX, startY].team);
            promoted.movesMade = board[startX, startY].movesMade;
            if (board[endX, endY] != null)
                Destroy(board[endX, endY].gameObject);
            board[endX, endY] = promoted;
            PositionOnePiece(endX, endY);
            Destroy(board[startX, startY].gameObject);
            board[startX, startY] = null;
        }
        else
        {
            board[startX, startY].currentX = endX;
            board[startX, startY].currentY = endY;

            if (board[endX, endY] != null)
            {
                Destroy(board[endX, endY].gameObject);
                board[endX, endY] = null;
            }

            board[startX, startY].transform.position = GetMoveVector(endX, endY);

            board[endX, endY] = board[startX, startY];
            board[startX, startY] = null;

            if (board[endX, endY].team == 0)
                lastMoveW = new Vector2Int(endX, endY);
            else
                lastMoveB = new Vector2Int(endX, endY);
        }

        //Check for game end
        playerTurn = playerTurn == 0 ? 1 : 0;

        if (IsCheckMate(chessPieces, playerTurn))
            FinishGame(true, playerTurn);

        if (IsNull(chessPieces, playerTurn))
            FinishGame(false, playerTurn);

        Debug.Log("score = " + CalculateAdvantage(chessPieces));
        int amount = 0;
        foreach (ChessPiece piece in chessPieces)
        {
            if (piece != null)
            {
                amount++;
            }
        }
        Debug.Log("Pieces =" + amount);
        Debug.Log("Operations =" + i);
        Debug.Log(endX + " " + endY);
        i = 0;
    }

    private void InvisibleMove(ChessPiece[,] board, int startX, int startY, int endX, int endY, out ChessPiece capturedPiece, out bool wasPromotion, out ChessPiece originalPiece)
    {
        capturedPiece = GetCapturedPiece(board, board[startX, startY], new Vector2Int(endX, endY));
        wasPromotion = false;
        originalPiece = board[startX, startY];

        board[startX, startY].movesMade += 1;

        //Castling
        if (board[startX, startY].type == PieceType.King)
        {
            if (startX == 4)
            {
                if (endX == startX + 2)
                {
                    board[TILE_COUNT_X - 1, startY].currentX = startX + 1;
                    board[startX + 1, startY] = board[TILE_COUNT_X - 1, startY];
                    board[TILE_COUNT_X - 1, startY] = null;
                }
                else if (endX == startX - 2)
                {
                    board[0, startY].currentX = startX - 1;
                    board[startX - 1, startY] = board[0, startY];
                    board[0, startY] = null;
                }
            }
        }

        //Promotion 
        if (board[startX, startY].type == PieceType.Pawn && ((board[startX, startY].team == 0 && endY == TILE_COUNT_Y - 1) || (board[startX, startY].team == 1 && endY == 0)))
        {
            ChessPiece promoted = SpawnOnePiece(PieceType.Queen, board[startX, startY].team);
            promoted.currentX = endX;
            promoted.currentY = endY;
            promoted.movesMade = board[startX, startY].movesMade;
            board[endX, endY] = promoted;
            wasPromotion = true;
        }
        else
        {
            board[startX, startY].currentX = endX;
            board[startX, startY].currentY = endY;

            board[endX, endY] = board[startX, startY];
        }

        
        board[startX, startY] = null;

        if (board[endX, endY].team == 0)
            lastMoveW = new Vector2Int(endX, endY);
        else
            lastMoveB = new Vector2Int(endX, endY);

        if (capturedPiece != null && (board[endX, endY].currentX != capturedPiece.currentX || board[endX, endY].currentY != capturedPiece.currentY))
            board[capturedPiece.currentX, capturedPiece.currentY] = null;
    }

    private void UndoMove(ChessPiece[,] board, int startX, int startY, int endX, int endY, ChessPiece capturedPiece, Vector2Int lastMove, bool wasPromotion, ChessPiece originalPiece)
    {
        if (!wasPromotion)
            board[endX, endY].movesMade -= 1;
        else
            originalPiece.movesMade -= 1;


        //Castling
        if (board[endX, endY].type == PieceType.King)
        {
            if (startX == 4)
            {
                if (endX == startX + 2)
                {
                    board[TILE_COUNT_X - 1, startY] = board[startX + 1, startY];
                    board[TILE_COUNT_X - 1, startY].currentX = TILE_COUNT_X - 1;
                    board[startX + 1, startY] = null;
                }
                else if (endX == startX - 2)
                {
                    board[0, startY] = board[startX - 1, startY];
                    board[0, startY].currentX = 0;
                    board[startX - 1, startY] = null;
                }
            }
        }

        if (wasPromotion)
        {
            Destroy(board[endX, endY].gameObject);
            board[startX, startY] = originalPiece;
            board[startX, startY].currentX = startX;
            board[startX, startY].currentY = startY;
            
        }
        else
        {
            board[startX, startY] = board[endX, endY];
            board[startX, startY].currentX = startX;
            board[startX, startY].currentY = startY;
        }

        if (capturedPiece != null)
        {
            if (capturedPiece.currentX == endX && capturedPiece.currentY == endY)
                board[capturedPiece.currentX, capturedPiece.currentY] = capturedPiece;
            else
            {
                board[capturedPiece.currentX, capturedPiece.currentY] = capturedPiece;
                board[endX, endY] = null;
            }
        }
        else
            board[endX, endY] = null;

        if (board[startX, startY].team == 0)
            lastMoveW = lastMove;
        else
            lastMoveB = lastMove;
    }

    private void ShowAvailableMoves()
    {
        availableMoves = currentFocus.GetAvailableMoves(chessPieces);

        PreventCheck(chessPieces, currentFocus, ref availableMoves);

        if (!availableMoves.Any())
        {
            currentFocus = null;
            return;
        }

        for (int i = 0; i < availableMoves.Count; i++)
        {
            if (chessPieces[availableMoves[i].x, availableMoves[i].y] == null)
                currentPoints.Add(Instantiate(point, transform));
            else
                currentPoints.Add(Instantiate(killPoint, transform));

            currentPoints[i].transform.position = GetMoveVector(availableMoves[i].x, availableMoves[i].y);
        }
    }

    private void HideAvailableMoves()
    {
        foreach (var point in currentPoints)
            Destroy(point.gameObject);

        currentPoints.Clear();
    }

    private void HighlightTile(int x, int y)
    {
        GameObject highlightedTile = Instantiate(highlight, transform);

        highlightedTile.transform.position = GetMoveVector(x, y);
        tilesSelected.Add(highlightedTile);
    }

    private void CancelHighlight()
    {
        foreach (var selection in tilesSelected)
            Destroy(selection.gameObject);

        tilesSelected.Clear();
    }

    private void PreventCheck(ChessPiece[,] board, ChessPiece piece, ref List<Vector2Int> currentAvailableMoves)
    {
        ChessPiece king = null;
        List<Vector2Int> tempAvailableMoves = new List<Vector2Int>();

        for (int i = 0; i < currentAvailableMoves.Count; i++)
            tempAvailableMoves.Add(currentAvailableMoves[i]);

        foreach (ChessPiece chessPiece in board)
        {
            if (chessPiece != null && chessPiece.type == PieceType.King && chessPiece.team == piece.team)
            {
                king = chessPiece;
                break;
            }
        }

        foreach (Vector2Int availableMove in tempAvailableMoves)
        {
            ChessPiece originalPiece;
            ChessPiece capturedPiece;

            int tempPosX = piece.currentX;
            int tempPosY = piece.currentY;
            bool wasPromotion;
            

            Vector2Int tempLastMove = (piece.team == 0) ? lastMoveW : lastMoveB;

            InvisibleMove(board, piece.currentX, piece.currentY, availableMove.x, availableMove.y, out capturedPiece, out wasPromotion, out originalPiece);

            foreach (ChessPiece chessPiece in board)
            {
                if (chessPiece != null && chessPiece.team != piece.team)
                {
                    List<Vector2Int> currentMoves = chessPiece.GetAvailableMoves(board);

                    if (currentMoves.Contains(new Vector2Int(king.currentX, king.currentY)))
                    {
                        currentAvailableMoves.Remove(availableMove);
                        break;
                    }
                }
            }

            UndoMove(board, tempPosX, tempPosY, availableMove.x, availableMove.y, capturedPiece, tempLastMove, wasPromotion, originalPiece);
        }
    }

    private int CalculateAdvantage(ChessPiece[,] board)
    {
        int whiteSum = 0;
        int blackSum = 0;

        bool isEndgame = Evaluation.IsEndgame(board);


        foreach (ChessPiece piece in board)
            if (piece != null)
            {
                if (piece.team == 0)
                    whiteSum += piece.pieceValue + Evaluation.EvaluatePiecePos(piece.type, isEndgame, piece.team, piece.currentX, piece.currentY);
                else
                    blackSum += piece.pieceValue + Evaluation.EvaluatePiecePos(piece.type, isEndgame, piece.team, piece.currentX, piece.currentY);
            }

        return whiteSum - blackSum;
    }

    private ChessPiece GetCapturedPiece(ChessPiece[,] board, ChessPiece piece, Vector2Int move)
    {
        if (piece.type == PieceType.Pawn)
        {
            if (board[move.x, move.y] != null)
                return board[move.x, move.y];

            if (piece.team == 0 && piece.currentY == 4 || piece.team == 1 && piece.currentY == 3)
            {
                int direction = (piece.team == 0) ? 1 : -1;

                if (move.x == piece.currentX - 1 && board[piece.currentX - 1, piece.currentY] != null)
                    return board[piece.currentX - 1, piece.currentY];
                else if (move.x == piece.currentX + 1 && board[piece.currentX + 1, piece.currentY] != null)
                    return board[piece.currentX + 1, piece.currentY];
                else
                    return board[move.x, move.y];
            }
            else
                return board[move.x, move.y];
        }
        else
            return board[move.x, move.y];
    }

    private bool IsAnyMoveAvailable(ChessPiece[,] board, int team)
    {
        foreach (ChessPiece piece in board)
        {
            if (piece != null &&  piece.team == team)
            {
                List<Vector2Int> availableMoves = piece.GetAvailableMoves(chessPieces);

                PreventCheck(board, piece, ref availableMoves);

                if (availableMoves.Any())
                    return true;
            }
        }

        return false;
    }

    private void PlayerAction()
    {
        if (awaitingPromotionChoice)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                int tileX = Convert.ToInt32(hit.collider.GetComponent<Tile>().cordX);
                int tileY = Convert.ToInt32(hit.collider.GetComponent<Tile>().cordY);

                int targetTeam = playerTurn;

                Debug.Log(tileX + " " + tileY);
                if (currentFocus == null)
                {
                    if (chessPieces[tileX, tileY] != null && chessPieces[tileX, tileY].team == targetTeam)
                    {
                        currentFocus = chessPieces[tileX, tileY];
                        ShowAvailableMoves();

                        if (tilesSelected.Count % 2 == 0)
                        {
                            HighlightTile(tileX, tileY);
                        }
                        else
                        {
                            CancelHighlight();
                            HighlightTile(tileX, tileY);
                        }

                        ulong hash = playerTurn == 0 ? Zobrist.ComputeHash(chessPieces, false) : Zobrist.ComputeHash(chessPieces, true);
                        Debug.Log("Hash is" + hash);
                    }
                }
                else
                {
                    if (availableMoves.Contains(new Vector2Int(tileX, tileY)))
                    {
                        if (currentFocus.type == PieceType.Pawn && ((currentFocus.team == 0 && tileY == 7) || (currentFocus.team == 1 && tileY == 0)))
                        {
                            promotionMove = new Vector2Int(tileX, tileY);

                            if (currentFocus.team == 0)
                                pieceChoiceWhite.SetActive(true);
                            else
                                pieceChoiceBlack.SetActive(true);

                            awaitingPromotionChoice = true;
                            return;
                        }

                        MoveOnePiece(chessPieces, currentFocus.currentX, currentFocus.currentY, tileX, tileY, PieceType.Queen);
                    }
                    else
                        CancelHighlight();
                    HideAvailableMoves();
                    currentFocus = null;
                }
            }
        }
    }

    private void AIAction(bool side)
    {
        ulong currentHash = Zobrist.ComputeHash(chessPieces, !side);
        int depth = DEPTH;

        if (Evaluation.IsEndgame(chessPieces))
            depth += 2;

        Minimax(chessPieces, depth, depth, int.MinValue, int.MaxValue, side, currentHash);

        if (side)
            MoveOnePiece(chessPieces, bestPieceW.currentX, bestPieceW.currentY, bestMoveW.x, bestMoveW.y, PieceType.Queen);
        else
            MoveOnePiece(chessPieces, bestPieceB.currentX, bestPieceB.currentY, bestMoveB.x, bestMoveB.y, PieceType.Queen);
    }

    private int Minimax(ChessPiece[,] board, int initialDepth, int depth, int alpha, int beta, bool maximizingPlayer, ulong hash)
    {
        bool breakCondition = false;
        bool gameOver = true;

        if (Zobrist.transpositionTable.TryGetValue(hash, out TranspositionPart cached))
        {
            if (cached.depth >= depth)
            {
                if (depth == initialDepth)
                {
                    if (maximizingPlayer)
                    {
                        bestPieceW = cached.bestPiece;
                        bestMoveW = cached.bestMove;
                    }
                    else
                    {
                        bestPieceB = cached.bestPiece;
                        bestMoveB = cached.bestMove;
                    }
                }

                return cached.evaluation;
            }
        }

        if (depth == 0)
        {
            return CalculateAdvantage(board);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (ChessPiece chessPiece in board)
            {
                if (breakCondition)
                    break;

                if (chessPiece != null && chessPiece.team == 0)
                {
                    List<Vector2Int> currentMoves = chessPiece.GetAvailableMoves(board);

                    PreventCheck(board, chessPiece, ref currentMoves);

                    if (currentMoves.Count != 0)
                        gameOver = false;

                    foreach (Vector2Int move in currentMoves)
                    {
                        i++;
                        int tempPosX = chessPiece.currentX;
                        int tempPosY = chessPiece.currentY;

                        bool wasPromotion;

                        ChessPiece capturedPiece;
                        ChessPiece originalPiece;
                        Vector2Int tempLastMove = lastMoveW;

                        InvisibleMove(board, chessPiece.currentX, chessPiece.currentY, move.x, move.y, out capturedPiece, out wasPromotion, out originalPiece);

                        ulong newHash = Zobrist.ComputeHash(board, false);

                        int eval = Minimax(board, initialDepth, depth - 1, alpha, beta, false, newHash);

                        UndoMove(board, tempPosX, tempPosY, move.x, move.y, capturedPiece, tempLastMove, wasPromotion, originalPiece);

                        if (eval > maxEval)
                        {
                            maxEval = eval;
                            if (depth == initialDepth)
                            {
                                bestMoveW = move;
                                bestPieceW = chessPiece;
                            }
                        }

                        //alpha = Math.Max(alpha, eval);
                        alpha = Math.Max(alpha, maxEval);

                        if (beta <= alpha)
                        {
                            breakCondition = true;
                            break;
                        }
                    }
                }
            }

            if (gameOver)
            {
                bool isInCheck = IsCheck(board, maximizingPlayer ? 0 : 1);
                if (isInCheck)
                {
                    // Checkmate
                    return maximizingPlayer ? -WIN_SCORE - depth : WIN_SCORE + depth;
                }
                else
                {
                    // Stalemate
                    return 0;
                }
            }

            Zobrist.transpositionTable[hash] = new TranspositionPart(maxEval, depth, bestPieceW, bestMoveW);

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (ChessPiece chessPiece in board)
            {
                if (breakCondition)
                    break;

                if (chessPiece != null && chessPiece.team == 1)
                {
                    List<Vector2Int> currentMoves = chessPiece.GetAvailableMoves(board);

                    PreventCheck(board, chessPiece, ref currentMoves);

                    if (currentMoves.Count != 0)
                        gameOver = false;

                    foreach (Vector2Int move in currentMoves)
                    {
                        i++;
                        int tempPosX = chessPiece.currentX;
                        int tempPosY = chessPiece.currentY;

                        bool wasPromotion;
                        
                        ChessPiece capturedPiece;
                        ChessPiece originalPiece;
                        Vector2Int tempLastMove = lastMoveB;

                        InvisibleMove(board, chessPiece.currentX, chessPiece.currentY, move.x, move.y, out capturedPiece, out wasPromotion, out originalPiece);

                        ulong newHash = Zobrist.ComputeHash(board, true);

                        int eval = Minimax(board, initialDepth, depth - 1, alpha, beta, true, newHash);

                        UndoMove(board, tempPosX, tempPosY, move.x, move.y, capturedPiece, tempLastMove, wasPromotion, originalPiece);

                        if (eval < minEval)
                        {
                            minEval = eval;
                            if (depth == initialDepth)
                            {
                                bestMoveB = move;
                                bestPieceB = chessPiece;
                            }
                        }

                        //beta = Math.Min(beta, eval);
                        beta = Math.Min(beta, minEval);

                        if (beta <= alpha)
                        {
                            breakCondition = true;
                            break;
                        }
                    }
                }

            }

            if (gameOver)
            {
                bool isInCheck = IsCheck(board, maximizingPlayer ? 0 : 1);
                if (isInCheck)
                {
                    // Checkmate
                    return maximizingPlayer ? -WIN_SCORE - depth : WIN_SCORE + depth;
                }
                else
                {
                    // Stalemate
                    return 0;
                }
            }

            Zobrist.transpositionTable[hash] = new TranspositionPart(minEval, depth, bestPieceB, bestMoveB);

            return minEval;
        }
    }

    private void FinishGame(bool isCheckMate, int loserTeam)
    {
        gameEnded = true;

        string winner = loserTeam == 0 ? "Black won" : "White won";
        string text = isCheckMate ? "Checkmate!\n" + winner : "Null!";

        UIManager.GetComponent<UIManager>().DisableButtons();
        UIManager.GetComponent<UIManager>().OpenEndGamePannel(text);
    }
}
