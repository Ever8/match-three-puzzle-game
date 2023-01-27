using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class StartingObject
{
    public GameObject prefab;
    public int x;
    public int y;
    public int z;
}

[RequireComponent(typeof(BoardDeadlock))]
[RequireComponent(typeof(BoardShuffler))]
[RequireComponent(typeof(BoardInput))]
[RequireComponent(typeof(BoardQuery))]
[RequireComponent(typeof(BoardSetup))]
[RequireComponent(typeof(BoardTiles))]
[RequireComponent(typeof(BoardBomber))]
[RequireComponent(typeof(BoardFiller))]
[RequireComponent(typeof(BoardMatcher))]
[RequireComponent(typeof(BoardCollapser))]
[RequireComponent(typeof(BoardHighlighter))]

public class Board : MonoBehaviour
{

    public int width;
    public int height;

    public int borderSize;
        
    public GameObject tileNormalPrefab;
        
    public GameObject tileObstaclePrefab;

    public GameObject[] gamePiecePrefabs;
       
    public GameObject[] adjacentBombPrefabs;

    public GameObject[] columnBombPrefabs;

    public GameObject[] rowBombPrefabs;
	
    public GameObject colorBombPrefab;

    public int maxCollectibles = 3;

    public int collectibleCount = 0;

    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;

    public GameObject[] collectiblePrefabs;

    public GameObject clickedTileBomb;
	
    public GameObject targetTileBomb;

    public float swapTime = 0.5f;

    public Tile[,] allTiles;

    public GamePiece[,] allGamePieces;

    public Tile clickedTile;

    public Tile targetTile;

    public bool playerInputEnabled = true;

    public StartingObject[] startingTiles;

    public StartingObject[] startingGamePieces;

    public ParticleManager particleManager;

    public int fillYOffset = 10;
	
    public float fillMoveTime = 0.5f;

    public int scoreMultiplier = 0;

    public bool isRefilling = false;

    public BoardDeadlock boardDeadlock;
    public BoardShuffler boardShuffler;
    public BoardSetup boardSetup;
    public BoardFiller boardFiller;
    public BoardHighlighter boardHighlighter;
    public BoardQuery boardQuery;
    public BoardInput boardInput;
    public BoardMatcher boardMatcher;
    public BoardCollapser boardCollapser;
    public BoardTiles boardTiles;
    public BoardBomber boardBomber;
    public BoardClearer boardClearer;

    public float delay = 0.2f;

    private void Awake()
    {

        boardDeadlock = GetComponent<BoardDeadlock>();
        boardShuffler = GetComponent<BoardShuffler>();
        boardSetup = GetComponent<BoardSetup>();
        boardFiller = GetComponent<BoardFiller>();
        boardHighlighter = GetComponent<BoardHighlighter>();
        boardQuery = GetComponent<BoardQuery>();
        boardInput = GetComponent<BoardInput>();
        boardMatcher = GetComponent<BoardMatcher>();
        boardCollapser = GetComponent<BoardCollapser>();
        boardTiles = GetComponent<BoardTiles>();
        boardBomber = GetComponent<BoardBomber>();
        boardClearer = GetComponent<BoardClearer>();
    }

    void Start()
    {
        allTiles = new Tile[width, height];

        allGamePieces = new GamePiece[width, height];

        particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
    }



    public void TestDeadlock()
    {
        boardDeadlock.IsDeadlocked(allGamePieces, 3);
    }

    public void ShuffleBoard()
    {
        if (playerInputEnabled)
        {
            StartCoroutine(boardShuffler.ShuffleBoardRoutine(this));
        }
    }

    public void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    public IEnumerator SwitchTilesRoutine(Tile tileA, Tile tileB)
    { 
        if (playerInputEnabled && !GameManager.Instance.IsGameOver)
        {
            GamePiece clickedPiece = allGamePieces[tileA.xIndex, tileA.yIndex];
            GamePiece targetPiece = allGamePieces[tileB.xIndex, tileB.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(tileB.xIndex, tileB.yIndex, swapTime);
                targetPiece.Move(tileA.xIndex, tileA.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> tileAMatches = boardMatcher.FindMatchesAt(tileA.xIndex, tileA.yIndex);
                List<GamePiece> tileBMatches = boardMatcher.FindMatchesAt(tileB.xIndex, tileB.yIndex);
                List<GamePiece> colorMatches = boardBomber.ProcessColorBombs(clickedPiece, targetPiece);


                if (tileBMatches.Count == 0 && tileAMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(tileA.xIndex, tileA.yIndex, swapTime);
                    targetPiece.Move(tileB.xIndex, tileB.yIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    Vector2 swipeDirection = new Vector2(tileB.xIndex - tileA.xIndex, tileB.yIndex - tileA.yIndex);

                    boardBomber.ProcessBombs(tileA, tileB, clickedPiece, targetPiece, tileAMatches, tileBMatches);

                    List<GamePiece> piecesToClear = tileAMatches.Union(tileBMatches).ToList().Union(colorMatches).ToList();

                    yield return StartCoroutine(ClearAndRefillBoardRoutine(piecesToClear));

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateMoves();
                    }
                }
            }
        }
    }


    public void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    public IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {

        playerInputEnabled = false;
        isRefilling = true;

        List<GamePiece> matches = gamePieces;

        scoreMultiplier = 0;
        do
        {
            scoreMultiplier++;

            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            yield return null;

            yield return StartCoroutine(boardFiller.RefillRoutine());

            matches = boardMatcher.FindAllMatches();

            yield return new WaitForSeconds(delay);

        }
		while (matches.Count != 0);

        if (boardDeadlock.IsDeadlocked(allGamePieces, 3))
        {
            yield return new WaitForSeconds(delay*5f);

            yield return StartCoroutine(boardShuffler.ShuffleBoardRoutine(this));
           
            yield return new WaitForSeconds(delay*5f);

            yield return StartCoroutine(boardFiller.RefillRoutine());
        }


        playerInputEnabled = true;
        isRefilling = false;
    }

    
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        List<GamePiece> matches = new List<GamePiece>();

        yield return new WaitForSeconds(delay);

        bool isFinished = false;

        while (!isFinished)
        {
            List<GamePiece> bombedPieces = boardQuery.GetBombedPieces(gamePieces);

            gamePieces = gamePieces.Union(bombedPieces).ToList();

            bombedPieces = boardQuery.GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombedPieces).ToList();

            List<GamePiece> collectedPieces = boardQuery.GetCollectedPieces(gamePieces);

            List<int> columnsToCollapse = boardQuery.GetColumns(gamePieces);

            boardClearer.ClearPieceAt(gamePieces, bombedPieces);

            boardTiles.BreakTileAt(gamePieces);

            boardBomber.InitAllBombs();
                        
            yield return new WaitForSeconds(delay);

            movingPieces = boardCollapser.CollapseColumn(columnsToCollapse);
                        
            while (!boardQuery.IsCollapsed(movingPieces))
            {
                yield return null;
            }
            yield return new WaitForSeconds(delay);

            matches = boardMatcher.FindMatchesAt(movingPieces);

            collectedPieces = boardQuery.FindCollectiblesAt(0, true);

            matches = matches.Union(collectedPieces).ToList();


            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                scoreMultiplier++;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBonusSound();
                }

                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }



}
