using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceBar : MonoBehaviour
{
    public enum State
    {
        Select,
        Delete, 
        Spawn
    }

    public State state = State.Select;
    public PieceType pieceType;
    public int pieceTeam;

    public Sprite unhover;
    public Sprite hover;

    public Image selectHover;

    private Image lastHover = null;

    public void ButtonHover(Image hoverSpace)
    {
        if (lastHover != null)
            lastHover.sprite = unhover;

        hoverSpace.sprite = hover;
        lastHover = hoverSpace;
    }

    public void UnhoverLast()
    {
        if (lastHover != null)
        {
            lastHover.sprite = unhover;
            lastHover = null;
        }
    }

    public void SelectButton()
    {
        state = State.Select;
    }

    public void DeleteButton()
    {
        state = State.Delete;
    }

    public void SelectPieceType(int pieceValue)
    {
        state = State.Spawn;

        foreach (PieceType value in Enum.GetValues(typeof(PieceType)))
        {
            if (Convert.ToInt32(value) == pieceValue)
            {
                pieceType = value;
                break;
            }
        }
    }

    public void SelectPieceTeam(int team)
    {
        pieceTeam = team;
    }
}
