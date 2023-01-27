using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardFiller : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (board == null)
            return null;

        if (board.boardQuery.IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(board.boardQuery.GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    public GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (board == null)
            return null;

        if (board.boardQuery.IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(board.boardQuery.GetRandomCollectible(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    public void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {

        int maxInterations = 100;
        int iterations = 0;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
           
                if (board.allGamePieces[i, j] == null && board.allTiles[i, j].tileType != TileType.Obstacle)
                {
                    if (j == board.height - 1 && board.boardQuery.CanAddCollectible())
                    {
                        FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                        board.collectibleCount++;
                    }

                    else
                    {
                        FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                        iterations = 0;

                        while (board.boardQuery.HasMatchOnFill(i, j))
                        {
                            board.boardClearer.ClearPieceAt(i, j);
                            FillRandomGamePieceAt(i, j, falseYOffset, moveTime);

                            iterations++;

                            if (iterations >= maxInterations)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }


    public void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> unusedPieces = new Queue<GamePiece>(gamePieces);

        int maxIterations = 100;
        int iterations = 0;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allGamePieces[i, j] == null && board.allTiles[i, j].tileType != TileType.Obstacle)
                {
                    board.allGamePieces[i, j] = unusedPieces.Dequeue();

                    iterations = 0;

                    while (board.boardQuery.HasMatchOnFill(i, j))
                    {
                        unusedPieces.Enqueue(board.allGamePieces[i, j]);

                        board.allGamePieces[i, j] = unusedPieces.Dequeue();

                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }



    public GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if (board == null)
            return null;

 
        if (prefab != null && board.boardQuery.IsWithinBounds(x, y))
        {

            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(board);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }


    public void MakeColorBombBooster(int x, int y)
    {
        if (board == null)
            return;

        if (board.boardQuery.IsWithinBounds(x, y))
        {
            GamePiece pieceToReplace = board.allGamePieces[x, y];

            if (pieceToReplace != null)
            {
                board.boardClearer.ClearPieceAt(x, y);
                GameObject bombObject = MakeBomb(board.colorBombPrefab, x, y);
                board.boardBomber.InitBomb(bombObject);
            }
        }
    }


    public void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (board == null)
            return;

        if (prefab != null && board.boardQuery.IsWithinBounds(x, y))
        {

            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + x + "," + y + ")";
            board.allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            board.allTiles[x, y].Init(x, y, board);
        }
    }

    public void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (board == null)
            return;

        if (prefab != null && board.boardQuery.IsWithinBounds(x, y))
        {
            prefab.GetComponent<GamePiece>().Init(board);
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);



            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            prefab.transform.parent = transform;
        }
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (board == null)
            return;

        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD:  Invalid GamePiece!");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;

        if (board.boardQuery.IsWithinBounds(x, y))
        {
            board.allGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }

    public IEnumerator RefillRoutine()
    {
        board.boardFiller.FillBoard(board.fillYOffset, board.fillMoveTime);

        yield return null;
    }



}
