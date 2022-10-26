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
    GameObject ProgramPanel;
    [SerializeField]
    GameObject IntroPanel;
    [SerializeField]
    TextMeshProUGUI IntroText;
    [SerializeField]
    GameObject PlayFieldPanel;
    [SerializeField]
    GameObject EnterProgramPanel;
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
    [SerializeField]
    GameObject TryAgainButton;
    [SerializeField]
    GameObject LiveCamera;

    int rows = 10;
    int cols = 10;

    GameObject [] tiles = new GameObject[100];

    [SerializeField]
    GameObject FirstPersonPlayfield;
    [SerializeField]
    GameObject MainCamera;
    [SerializeField]
    GameObject [] RockPrefab = new GameObject[4];
    [SerializeField]
    GameObject GoalPrefab;
    GameObject [] firstPersonTiles = new GameObject[100];
    Vector3 currentPos;
    Vector3 desiredPos;
    Vector3 currentRot;
    Vector3 desiredRot;

    List<string> instructions = new List<string>();
    List<string> internalInstructions = new List<string>();

    int currentInstruction = 0;
    int currentInternalInstruction = 0;
    int currentRow = 0;
    int currentCol = 0;
    bool isRunning = false;
    float runTimer = 0;
    float runTimerMax = 1f;
    float runTimerEndMax = .3f;
    bool hitRock = false;
    bool foundGoal = false;

    bool showFirstPerson = false;

    string [] introText = {"Welcome to the Mars Rover command center. Your mission is to program the rover to reach the space capsule without hitting any boulders along the way.",
        "Study the overhead map to choose a route for the rover. Then create a program made up of movement instructions for the the rover (up, down, left, right). When your program is ready, you can send the instructions to the rover and see how well your program works!"};
    int currentIntroText = 0;

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

                float maxTime = hitRock || foundGoal ? .3f : 1f;
                float lerpScale = hitRock || foundGoal ? 3.33f : 1f;
                MainCamera.transform.localPosition = Vector3.Lerp(currentPos, desiredPos, (maxTime - runTimer) * lerpScale);
                MainCamera.transform.localEulerAngles = Vector3.Lerp(currentRot, desiredRot, (maxTime - runTimer) * lerpScale);

                if (runTimer <= 0)
                {
                    currentInternalInstruction++;
                    if (hitRock)
                    {
                        Lose();
                    }
                    else if (foundGoal)
                    {
                        Win();
                    }
                    else if (currentInternalInstruction >= internalInstructions.Count)
                    {
                        Stuck();
                    }
                    else
                    {
                        string instr = internalInstructions[currentInternalInstruction];
                        // highlight first instruction
                        if (currentInstruction == -1)
                            currentInstruction = 0;
                        HighlightInstruction();
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

                            currentInstruction++;
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
                        float xEndAdjust = 0f;
                        float zEndAdjust = -7f;
                        if (currentDirection == Directions.Right)
                        {
                            zRot = 270f;
                            yRot = 90f;
                            xEndAdjust = -7f;
                            zEndAdjust = 0;
                        }
                        else if (currentDirection == Directions.Down)
                        {
                            zRot = 180f;
                            yRot = 180f;
                            xEndAdjust = 0;
                            zEndAdjust = 7f;
                        }
                        else if (currentDirection == Directions.Left)
                        {
                            zRot = 90f;
                            yRot = 270f;
                            xEndAdjust = 7f;
                            zEndAdjust = 0;
                        }
                        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, zRot);

                        if (tiles[currentCol + currentRow * cols].GetComponent<Tile>().IsFlag())
                        {
                            foundGoal = true;
                        }
                        else if (tiles[currentCol + currentRow * cols].GetComponent<Tile>().IsRock())
                        {
                            hitRock = true;
                        }
                        else
                        {
                            xEndAdjust = 0;
                            zEndAdjust = 0;
                        }

                        // set first person position and orientation
                        desiredPos = new Vector3(currentCol * 10f + xEndAdjust, 3f, currentRow * 10f + zEndAdjust);
                        desiredRot = new Vector3(0, yRot, 0);
                        currentPos = MainCamera.transform.localPosition;
                        currentRot = MainCamera.transform.localEulerAngles;
                        if (currentRot.y == 0f && desiredRot.y == 270f)
                            desiredRot = new Vector3(desiredRot.x, -90f, desiredRot.z);
                        else if (currentRot.y == 270f && desiredRot.y == 0)
                            desiredRot = new Vector3(desiredRot.x, 360f, desiredRot.z);

                        runTimer = foundGoal || hitRock ? runTimerEndMax : runTimerMax;
                    }
                }
            }
        }
    }

    void Win()
    {
        GameEndingText.text = "YOU REACHED THE GOAL!";
        TryAgainButton.SetActive(true);
        isRunning = false;
    }

    void Lose()
    {
        Camera.main.GetComponent<CameraShake>().StartShake();
        GameEndingText.text = "YOU HIT A BOULDER!";
        TryAgainButton.SetActive(true);
        isRunning = false;
    }

    void Stuck()
    {
        GameEndingText.text = "NO MORE INSTRUCTIONS!";
        TryAgainButton.SetActive(true);
        isRunning = false;
    }

    void ToggleDisplay()
    {
        ProgramPanel.SetActive(!showFirstPerson);
        FirstPersonPlayfield.SetActive(showFirstPerson);
        LiveCamera.SetActive(showFirstPerson);
    }

    public void InitPlayField()
    {
        isRunning = false;
        hitRock = false;
        foundGoal = false;
        GameEndingText.text = "";
        TryAgainButton.SetActive(false);
        LiveCamera.SetActive(false);
        currentDirection = Directions.Up;
        currentRow = 1;
        currentCol = 1;
        Rover.GetComponent<RectTransform>().anchoredPosition = new Vector2(90f, 90f);
        Rover.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);

        ClearInstructions();
        ClearTiles();

        // reset first person
        MainCamera.transform.localPosition = new Vector3(10f, 3f, 10f);
        MainCamera.transform.localEulerAngles = new Vector3(0, 0, 0);
        currentPos = new Vector3(10f, 3f, 10f);
        desiredPos = new Vector3(10f, 3f, 10f);
        currentRot = new Vector3(0, 0, 0);
        desiredRot = new Vector3(0, 0, 0);

        showFirstPerson = false;
        ToggleDisplay();

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
                    int rockRandVal = Random.Range(0, RockPrefab.Length);
                    GameObject GO = Instantiate(RockPrefab[rockRandVal], new Vector3(c * 10f, .4f, r * 10f), Quaternion.identity, FirstPersonPlayfield.transform);
                    GO.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 100f), 0f);
                    firstPersonTiles[r * cols + c] = GO;
                }
                else if (isFlag)
                {
                    GameObject GO = Instantiate(GoalPrefab, new Vector3(c * 10f, .4f, r * 10f), Quaternion.identity, FirstPersonPlayfield.transform);
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
        internalInstructions.Clear();
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

    public void SelectUp()
    {
        if (isRunning)
            return;
        instructions.Add("U");
        UpdateInstruction();
    }

    public void SelectDown()
    {
        if (isRunning)
            return;
        instructions.Add("D");
        UpdateInstruction();
    }

    public void SelectLeft()
    {
        if (isRunning)
            return;
        instructions.Add("L");
        UpdateInstruction();
    }

    public void SelectRight()
    {
        if (isRunning)
            return;
        instructions.Add("R");
        UpdateInstruction();
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

    void HighlightInstruction()
    {
        string instr = "";
        for (int i = 0; i < instructions.Count; i++)
        {
            if (i == currentInstruction)
                instr = instr + "<color=\"yellow\"><b>";
            instr = instr + instructions[i];
            if (i == currentInstruction)
                instr = instr + "</color=\"yellow\"></b>";
        }
        InstructionsText.text = instr;
    }

    public void StartRover()
    {
        if (isRunning)
            return;

        CreateInternalInstructions();
        currentInstruction = -1;
        currentInternalInstruction = -1;
        HighlightInstruction();
        isRunning = true;
        runTimer = runTimerMax;
        showFirstPerson = true;
        ToggleDisplay();
    }

    void CreateInternalInstructions()
    {
        for (int i = 0; i < instructions.Count; i++)
        {
            string prevInstr = i == 0 ? "U" : instructions[i - 1];

            if (prevInstr == "U")
            {
                if (instructions[i] == "U")
                {
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "R")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "D")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "L")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
            }
            else if (prevInstr == "R")
            {
                if (instructions[i] == "R")
                {
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "D")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "L")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "U")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
            }
            else if (prevInstr == "D")
            {
                if (instructions[i] == "D")
                {
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "L")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "U")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "R")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
            }
            else if (prevInstr == "L")
            {
                if (instructions[i] == "L")
                {
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "U")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "R")
                {
                    internalInstructions.Add("R");
                    internalInstructions.Add("R");
                    internalInstructions.Add("F");
                }
                else if (instructions[i] == "D")
                {
                    internalInstructions.Add("L");
                    internalInstructions.Add("F");
                }
            }
        }
    }

    public void ToggleView()
    {
        showFirstPerson = !showFirstPerson;
        ToggleDisplay();
    }

    public void NextInstruction()
    {
        currentIntroText++;
        if (currentIntroText  < introText.Length)
        {
            IntroText.text = introText[currentIntroText];
        }
        else
        {
            IntroPanel.SetActive(false);
            PlayFieldPanel.SetActive(true);
            EnterProgramPanel.SetActive(true);
        }
    }
}
