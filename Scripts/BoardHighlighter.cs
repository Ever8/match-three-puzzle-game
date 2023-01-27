using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardHighlighter : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    void HighlightTileOff(int x, int y)
    {
        if (board == null)
            return;

        if (board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = board.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color col)
    {
        if (board == null)
            return;

        if (board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = board.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = col;
        }
    }

    void HighlightMatchesAt(int x, int y)
    {
        if (board == null)
            return;

        HighlightTileOff(x, y);
        var combinedMatches = board.boardMatcher.FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches()
    {
        if (board == null)
            return;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                HighlightMatchesAt(i, j);

            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        if (board == null)
            return;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

}
