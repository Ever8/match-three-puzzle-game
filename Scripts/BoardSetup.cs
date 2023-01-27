using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardSetup : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }


    public void SetupBoard()
    {
        if (board == null)
            return;

        SetupTiles();

        SetupGamePieces();

        List<GamePiece> startingCollectibles = board.boardQuery.FindAllCollectibles();
        board.collectibleCount = startingCollectibles.Count;

        SetupCamera();

        board.boardFiller.FillBoard(board.fillYOffset, board.fillMoveTime);
    }

    public void SetupTiles()
    {
        if (board == null)
            return;

        foreach (StartingObject sTile in board.startingTiles)
        {
            if (sTile != null)
            {
                board.boardFiller.MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
            }

        }

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allTiles[i, j] == null)
                {
                    board.boardFiller.MakeTile(board.tileNormalPrefab, i, j);
                }
            }
        }
    }

    public void SetupGamePieces()
    {
        if (board == null)
            return;

        foreach (StartingObject sPiece in board.startingGamePieces)
        {
            if (sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
                board.boardFiller.MakeGamePiece(piece, sPiece.x, sPiece.y, board.fillYOffset, board.fillMoveTime);
            }

        }
    }

    public void SetupCamera()
    {
        if (board == null)
            return;


        Camera.main.transform.position = new Vector3((float)(board.width - 1) / 2f, (float)(board.height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = (float)board.height / 2f + (float)board.borderSize;

        float horizontalSize = ((float)board.width / 2f + (float)board.borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

    }

}

