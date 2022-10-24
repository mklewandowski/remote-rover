using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneManager : MonoBehaviour
{
    enum Directions {
        Up,
        Down,
        Left,
        Right
    }
    Directions currentDirection = Directions.Up;

    [SerializeField]
    GameObject TileContainer;
    [SerializeField]
    GameObject TilePrefab;
    [SerializeField]
    GameObject Rover;
    [SerializeField]
    TextMeshProUGUI InstructionsText;
    [SerializeField]
    TextMeshProUGUI GameEndingText;

    int rows = 10;
    int cols = 10;

    GameObject [] tiles = new GameObject[100];

    [SerializeField]
    GameObject FirstPersonPlayfield;
    [SerializeField]
    GameObject MainCamera;
    [SerializeField]
    GameObject RockPrefab;
    [SerializeField]
    GameObject GoalPrefab;
    GameObject [] firstPersonTiles = new GameObject[100];

    List<string> instructions = new List<string>();

    int currentInstruction = 0;
    int currentRow = 0;
    int currentCol = 0;
    bool isRunning = false;
    float runTimer = 0;
    float runTimerMax = .25f;

    // Start is called before the first frame update
    void Start()
    {
        InitPlayField();
    }
    void Update()
    {
        if (isRunning)
        {
            if (runTimer > 0)
            {
                runTimer -= Time.deltaTime;
                if (runTimer <= 0)
                {
                    if (currentInstruction >= instructions.Count)
                    {
                        isRunning = false;
                    }
                    else
                    {
                        string instr = instructions[currentInstruction];
                        if (instr == "F")
                        {
                            if (currentDirection == Directions.Up)
                                currentRow++;
                            else if (currentDirection == Directions.Right)
                                currentCol++;
                            else if (currentDirection == Directions.Down)
                                currentRow--;
                            else if (currentDirection == Directions.Left)
                                currentCol--;
                        }
                        else if (instr == "R")
                        {
                            if (currentDirection == Directions.Up)
                                currentDirection = Directions.Right;
                            else if (currentDirection == Directions.Right)
                                currentDirection = Directions.Down;
                            else if (currentDirection == Directions.Down)
                                currentDirection = Directions.Left;
                            else if (currentDirection == Directions.Left)
                                currentDirection = Directions.Up;
                        }
                        else if (instr == "L")
                        {
                            if (currentDirection == Directions.Up)
                                currentDirection = Directions.Left;
                            else if (currentDirection == Directions.Left)
                                currentDirection = Directions.Down;
                            else if (currentDirection == Directions.Down)
                                currentDirection = Directions.Right;
                            else if (currentDirection == Directions.Right)
                                currentDirection = Directions.Up;
                        }
                        float delta = 60f;
                        Rover.GetComponent<RectTransform>().anchoredPosition = new Vector2(30f + currentCol * delta, 30f + currentRow * delta);
                        float zRot = 0;
                        float yRot = 0;
                        if (currentDirection == Directions.Right)
                        {
                            zRot = 270f;
                            yRot = 90f;
                        }
                        else if (currentDirection == Directions.Down)
                        {
                            zRot = 180f;
                            yRot = 180f;
                        }
                        else if (currentDirection == Directions.Left)
                        {
                            zRot = 90f;
                            yRot = 270f;
                        }
                        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, zRot);

                        // move first person
                        MainCamera.transform.localEulerAngles = new Vector3(0, yRot, 0);
                        MainCamera.transform.localPosition = new Vector3(currentCol * 100f, MainCamera.transform.localPosition.y, currentRow * 100f);

                        if (tiles[currentCol + currentRow * cols].GetComponent<Tile>().IsFlag())
                            Win();
                        else if (tiles[currentCol + currentRow * cols].GetComponent<Tile>().IsRock())
                            Lose();

                        currentInstruction++;
                        runTimer = runTimerMax;
                    }
                }
            }
        }
    }

    void Win()
    {
        GameEndingText.text = "YOU FOUND THE FLAG!";
        isRunning = false;
    }

    void Lose()
    {
        GameEndingText.text = "YOU HIT A BOULDER!";
        isRunning = false;
    }

    public void InitPlayField()
    {
        isRunning = false;
        GameEndingText.text = "";
        currentDirection = Directions.Up;
        currentRow = 1;
        currentCol = 1;
        Rover.GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 90f);
        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);

        ClearInstructions();
        ClearTiles();

        // reset first person
        MainCamera.transform.localPosition = new Vector3(100f, MainCamera.transform.localPosition.y, 100f);

        float delta = 60f;
        int pStartCol = 1;
        for (int r = 0; r < rows; r++)
        {
            int pEndCol = r > 0 ? Random.Range(1, 9) : 1;
            int pMinCol = pStartCol < pEndCol ? pStartCol : pEndCol;
            int pMaxCol = pStartCol > pEndCol ? pStartCol : pEndCol;
            int flagIndex = -1;
            if (r == (rows - 2))
            {
                flagIndex = Random.Range(1, cols - 1);
                if (flagIndex < pMinCol)
                    pMinCol = flagIndex;
                else if (flagIndex > pMaxCol)
                    pMaxCol = flagIndex;
            }

            for (int c = 0; c < cols; c++)
            {
                int randVal = Random.Range(0, 100);
                bool isFlag = flagIndex == c;
                bool isRock = (r == 0 || r == rows - 1 || c == 0 || c == cols -1) || (!isFlag && (c < pMinCol || c > pMaxCol));
                tiles[r * cols + c] = Instantiate(TilePrefab, new Vector3(0, 0, 0), Quaternion.identity, TileContainer.transform);
                tiles[r * cols + c].GetComponent<RectTransform>().anchoredPosition = new Vector2(c * delta, r * delta);
                tiles[r * cols + c].GetComponent<Tile>().Init(isFlag, isRock);

                if (isRock)
                {
                    GameObject GO = Instantiate(RockPrefab, new Vector3(c * 100f, 5f, r * 100f), Quaternion.identity, FirstPersonPlayfield.transform);
                    firstPersonTiles[r * cols + c] = GO;
                }
                else if (isFlag)
                {
                    GameObject GO = Instantiate(GoalPrefab, new Vector3(c * 100f, 5f, r * 100f), Quaternion.identity, FirstPersonPlayfield.transform);
                    firstPersonTiles[r * cols + c] = GO;
                }
            }
            pStartCol = pEndCol;
        }
    }

    void ClearInstructions()
    {
        instructions.Clear();
        InstructionsText.text = "";
    }

    void ClearTiles()
    {
        for (int t = 0; t < tiles.Length; t++)
        {
            Destroy(tiles[t]);
        }
        for (int t = 0; t < firstPersonTiles.Length; t++)
        {
            Destroy(firstPersonTiles[t]);
        }
    }

    public void SelectRotateRight()
    {
        if (isRunning)
            return;
        instructions.Add("R");
        UpdateInstruction();
    }

    public void SelectRotateLeft()
    {
        if (isRunning)
            return;
        instructions.Add("L");
        UpdateInstruction();
    }

    public void SelectForward()
    {
        if (isRunning)
            return;
        instructions.Add("F");
        UpdateInstruction();
    }

    public void SelectDelete()
    {
        if (isRunning)
            return;
        if (instructions.Count == 0)
            return;
        instructions.RemoveAt(instructions.Count - 1);
        UpdateInstruction();
    }

    void UpdateInstruction()
    {
        string instr = "";
        for (int i = 0; i < instructions.Count; i++)
        {
            instr = instr + instructions[i];
        }
        InstructionsText.text = instr;
    }

    public void StartRover()
    {
        if (isRunning)
            return;
        currentInstruction = 0;
        isRunning = true;
        runTimer = runTimerMax;
    }
}
