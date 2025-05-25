using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject board;
    public GameObject UI;
   
    public GameObject BoardControls;
    public GameObject InGameUI;
    public GameObject InGameMainElements;

    public GameObject WS;
    public GameObject WL;
    public GameObject BS;
    public GameObject BL;

    public GameObject EndGamePannel;
    public GameObject dropdown;
    public GameObject enPassantDropdown;

    public Button startButton;

    public Sprite whiteSide;
    public Sprite blackSideRotated;

    public void RotateBoard()
    {
        bool isBoardReversed = board.GetComponent<Board>().isReversed;

        if (isBoardReversed)
        {
            board.GetComponent<Board>().isReversed = false;
            board.GetComponent<SpriteRenderer>().sprite = whiteSide;
        }
        else
        {
            board.GetComponent<Board>().isReversed = true;
            board.GetComponent<SpriteRenderer>().sprite = blackSideRotated;
        }

        board.transform.Rotate(0, 0, 180);

        RotatePieces();
    }

    public void FirstToPlay(int index)
    {
        switch (index)
        {
            case 0:
                board.GetComponent<Board>().playerTurn = 0;
                break;
            case 1:
                board.GetComponent<Board>().playerTurn = 1;
                break;
        }
    }

    public void AiSide(int index)
    {
        board.GetComponent<Board>().aiSide = index;
    }

    public void AddAdjustments()
    {   
        var boardScript = board.GetComponent<Board>();

        //Set all moves to zero and Set pawn not on starting place moves to 1
        foreach (ChessPiece piece in boardScript.chessPieces)
            if (piece != null)
            {
                if (piece.type == PieceType.Pawn)
                {
                    if (piece.team == 0)
                    {
                        if (piece.currentY != 1)
                            piece.movesMade = 1;
                        else
                            piece.movesMade = 0;
                    }
                    else
                    {
                        if (piece.currentY != 6)
                            piece.movesMade = 1;
                        else
                            piece.movesMade = 0;
                    }
                }
                else
                    piece.movesMade = 0;
            }

        // Allow castling
        if (!IsSelected(WS))
        {
            if (CheckCastling(new Vector2Int(4, 0), new Vector2Int(7, 0), 0))
            {
                //boardScript.chessPieces[4, 0].movesMade = 1;
                boardScript.chessPieces[7, 0].movesMade = 1;
            }
        }

        if (!IsSelected(WL))
        {
            if (CheckCastling(new Vector2Int(4, 0), new Vector2Int(0, 0), 0))
            {
               // boardScript.chessPieces[4, 0].movesMade = 1;
                boardScript.chessPieces[0, 0].movesMade = 1;
            }
        }

        if (!IsSelected(BS))
        {
            if (CheckCastling(new Vector2Int(4, 7), new Vector2Int(7, 7), 1))
            {
                //boardScript.chessPieces[4, 7].movesMade = 1;
                boardScript.chessPieces[7, 7].movesMade = 1;
            }
        }

        if (!IsSelected(BL))
        {
            if (CheckCastling(new Vector2Int(4, 7), new Vector2Int(0, 7), 1))
            {
                //boardScript.chessPieces[4, 7].movesMade = 1;
                boardScript.chessPieces[0, 7].movesMade = 1;
            }
        }

        TMP_Dropdown currentDropdown = enPassantDropdown.GetComponent<TMP_Dropdown>();

        if (currentDropdown.options.Count > 1)
        {
            if (currentDropdown.value != 0)
            {
                Vector2Int enPassantPos = GetPositionByString(currentDropdown.options[currentDropdown.value].text);
                boardScript.chessPieces[enPassantPos.x, enPassantPos.y].movesMade = 1;

                if (boardScript.chessPieces[enPassantPos.x, enPassantPos.y].team == 0)
                    Board.lastMoveW = enPassantPos;
                else
                    Board.lastMoveB = enPassantPos;
            }
        }

        FirstToPlay(dropdown.GetComponent<TMP_Dropdown>().value);
    }

    public void SetUIActive()
    {
        UI.gameObject.SetActive(true);
    }

    public void SetAvailableActions()
    {
        if (IsValidPos())
            startButton.interactable = true;
        else 
            startButton.interactable = false;

        TMP_Dropdown currentDropdown = enPassantDropdown.GetComponent<TMP_Dropdown>();
        List<Vector2Int> enPassantList = new List<Vector2Int>();

        foreach (var piece in board.GetComponent<Board>().chessPieces)
        {
            if (piece != null && piece.type == PieceType.Pawn)
            {
                List<Vector2Int>  tempList = CheckEnPassant(new Vector2Int(piece.currentX, piece.currentY), piece.team);
                foreach (Vector2Int position in tempList)
                {
                    if (!enPassantList.Contains(position))
                        enPassantList.Add(position);
                }
            } 
        }

        TMP_Dropdown.OptionData currentSelection = new TMP_Dropdown.OptionData();

        if (currentDropdown.options.Count > 1 && currentDropdown.value >= 0)
            currentSelection = currentDropdown.options[currentDropdown.value];

        currentDropdown.ClearOptions();
        currentDropdown.options.Add(new TMP_Dropdown.OptionData());
        
        foreach (Vector2Int position in enPassantList)
        {
            TMP_Dropdown.OptionData currentValue = new TMP_Dropdown.OptionData(GetLetterByIndex(position.x) + Convert.ToString(position.y + 1));
            currentDropdown.options.Add(currentValue);

            if (currentValue.text == currentSelection.text)
                currentDropdown.value = currentDropdown.options.IndexOf(currentValue);
        }

        currentDropdown.RefreshShownValue();
    }

    public void DisableButtons()
    {
        BoardControls.GetComponent<CanvasGroup>().interactable = false;
        board.GetComponent<Board>().isPause = true;
        InGameMainElements.GetComponent<CanvasGroup>().interactable = false;
    }

    public void EnableButtons()
    {
        BoardControls.GetComponent<CanvasGroup>().interactable = true;
        board.GetComponent<Board>().isPause = false;
        InGameMainElements.GetComponent<CanvasGroup>().interactable = true;
    }

    public void OpenEndGamePannel(string text)
    {
        EndGamePannel.GetComponentInChildren<TMP_Text>().text = text;
        EndGamePannel.gameObject.SetActive(true);
    }

    private bool IsSelected(GameObject toggle)
    {
        if (toggle.GetComponent<Toggle>().isOn == true)
            return true;

        return false;
    }

    private bool CheckCastling(Vector2Int kingCord, Vector2Int rookCord, int team)
    {
        var boardScript = board.GetComponent<Board>();

        ChessPiece king = boardScript.chessPieces[kingCord.x, kingCord.y];
        ChessPiece rook = boardScript.chessPieces[rookCord.x, rookCord.y];

        if (king != null && rook != null && king.team == team && king.type == PieceType.King && rook.team == team && rook.type == PieceType.Rook)
            return true;

        return false;
    }

    private List<Vector2Int> CheckEnPassant(Vector2Int pawnCord, int team)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        var fullBoard = board.GetComponent<Board>().chessPieces;
        int firstToMove = dropdown.GetComponent<TMP_Dropdown>().value;

        if (team == 0 && pawnCord.y == 4 && firstToMove == 0 || team == 1 && pawnCord.y == 3 && firstToMove == 1)
        {
            //Left
            if (pawnCord.x != 0 && fullBoard[pawnCord.x - 1, pawnCord.y] != null && fullBoard[pawnCord.x - 1, pawnCord.y].team != team && fullBoard[pawnCord.x - 1, pawnCord.y].type == PieceType.Pawn)
                result.Add(new Vector2Int(pawnCord.x - 1, pawnCord.y));

            //Right
            if (pawnCord.x != 7 && fullBoard[pawnCord.x + 1, pawnCord.y] != null && fullBoard[pawnCord.x + 1, pawnCord.y].team != team && fullBoard[pawnCord.x + 1, pawnCord.y].type == PieceType.Pawn)
                result.Add(new Vector2Int(pawnCord.x + 1, pawnCord.y));
        }

        return result;
    }
    private bool IsValidPos()
    {
        var boardScript = board.GetComponent<Board>();
        ChessPiece[,] chessBoard = boardScript.chessPieces;

        ChessPiece wKing = null;
        ChessPiece bKing = null;

        int wKingCount = 0;
        int bKingCount = 0;

        int wPawnAmount = 0;
        int bPawnAmount = 0;

        //King's amount
        foreach (ChessPiece piece in chessBoard)
        {
            if (piece != null)
            {
                if (piece.type == PieceType.King)
                {
                    if (piece.team == 0)
                    {
                        wKing = piece;
                        wKingCount++;
                    }

                    else
                    {
                        bKing = piece;
                        bKingCount++;
                    }
                }
                else if (piece.type == PieceType.Pawn)
                {
                    if (piece.team == 0)
                        wPawnAmount++;

                    else
                        bPawnAmount++;
                }
            }
        }

        if (wKingCount != 1 || bKingCount != 1 || wPawnAmount > 8 || bPawnAmount > 8)
            return false;

        //Side and check
        if (boardScript.IsCheck(chessBoard, 0) && dropdown.GetComponent<TMP_Dropdown>().value == 1)
            return false;
        if (boardScript.IsCheck(chessBoard, 1) && dropdown.GetComponent<TMP_Dropdown>().value == 0)
            return false;

        //Checkmate and Null
        if (boardScript.IsCheckMate(chessBoard, 0) && dropdown.GetComponent<TMP_Dropdown>().value == 0)
            return false;
        if (boardScript.IsCheckMate(chessBoard, 1) && dropdown.GetComponent<TMP_Dropdown>().value == 1)
            return false;

        if (boardScript.IsNull(chessBoard, 0))
            return false;
        if (boardScript.IsNull(chessBoard, 1))
            return false;

        //Pawns on the first and the last lines
        for (int i = 0; i < Board.TILE_COUNT_X; i++)
        {
            if (boardScript.chessPieces[i, 0] != null && boardScript.chessPieces[i, 0].type == PieceType.Pawn)
                return false;

            if (boardScript.chessPieces[i, 7] != null && boardScript.chessPieces[i, 7].type == PieceType.Pawn)
                return false;
        }

        return true;
    }

    private void RotatePieces()
    {
        foreach (Transform childTransform in board.transform)
        {
            childTransform.Rotate(0, 0, 180);
        }
    }

    private string GetLetterByIndex(int index)
    {
        switch (index)
        {
            case 0: return "a";
            case 1: return "b";
            case 2: return "c";
            case 3: return "d";
            case 4: return "e";
            case 5: return "f";
            case 6: return "g";
            case 7: return "h";
            default: return "";
        }
    }

    private Vector2Int GetPositionByString(string str)
    {
        int x, y;

        switch (str[0])
        {
            case 'a': x = 0; break;
            case 'b': x = 1; break;
            case 'c': x = 2; break;
            case 'd': x = 3; break;
            case 'e': x = 4; break;
            case 'f': x = 5; break;
            case 'g': x = 6; break;
            case 'h': x = 7; break;
            default: x = 0; break;
        }

        y = (int)Char.GetNumericValue(str[1]) - 1;

        return new Vector2Int(x, y);
    }
}
