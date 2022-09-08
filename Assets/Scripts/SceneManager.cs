using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneManager : MonoBehaviour
{

    [SerializeField]
    GameObject TileContainer;
    [SerializeField]
    GameObject TilePrefab;
    [SerializeField]
    TextMeshProUGUI InstructionsText;

    int rows = 10;
    int cols = 10;

    GameObject [] tiles = new GameObject[100];

    List<string> instructions = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        InitPlayField();
    }

    public void InitPlayField()
    {
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
}
