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
                        float delta = 40f;
                        Rover.GetComponent<RectTransform>().anchoredPosition = new Vector2(20f + currentCol * delta, 20f + currentRow * delta);
                        float zRot = 0;
                        if (currentDirection == Directions.Right)
                            zRot = 270f;
                        else if (currentDirection == Directions.Down)
                            zRot = 180f;
                        else if (currentDirection == Directions.Left)
                            zRot = 90f;
                        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, zRot);

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
        GameEndingText.text = "";
        currentDirection = Directions.Up;
        currentRow = 0;
        currentCol = 0;
        Rover.GetComponent<RectTransform>().anchoredPosition = new Vector2(20f, 20f);
        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);

        ClearInstructions();
        float delta = 40f;
        for (int r = 0; r < rows; r++)
        {
            int flagIndex = -1;
            if (r == (rows - 1))
            {
                flagIndex = Random.Range(0, cols);
            }
            for (int c = 0; c < cols; c++)
            {
                int randVal = Random.Range(0, 100);
                bool isFlag = flagIndex == c;
                bool isRock = !isFlag && randVal < 15 && r > 0;
                tiles[r * cols + c] = Instantiate(TilePrefab, new Vector3(0, 0, 0), Quaternion.identity, TileContainer.transform);
                tiles[r * cols + c].GetComponent<RectTransform>().anchoredPosition = new Vector2(c * delta, r * delta);
                tiles[r * cols + c].GetComponent<Tile>().Init(isFlag, isRock);
            }
        }
    }

    void ClearInstructions()
    {
        instructions.Clear();
        InstructionsText.text = "";
    }

    public void SelectRotateRight()
    {
        instructions.Add("R");
        UpdateInstruction();
    }
    public void SelectRotateLeft()
    {
        instructions.Add("L");
        UpdateInstruction();
    }
    public void SelectForward()
    {
        instructions.Add("F");
        UpdateInstruction();
    }
    public void SelectDelete()
    {
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
        currentInstruction = 0;
        isRunning = true;
        runTimer = runTimerMax;
    }
}
