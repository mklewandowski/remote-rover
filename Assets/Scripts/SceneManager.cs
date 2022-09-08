using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{

    [SerializeField]
    GameObject TileContainer;

    [SerializeField]
    GameObject TilePrefab;

    int rows = 10;
    int cols = 10;

    GameObject [] tiles = new GameObject[100];

    // Start is called before the first frame update
    void Start()
    {
        InitPlayField();
    }

    public void InitPlayField()
    {
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
                bool isRock = !isFlag && randVal < 15;
                tiles[r * cols + c] = Instantiate(TilePrefab, new Vector3(0, 0, 0), Quaternion.identity, TileContainer.transform);
                tiles[r * cols + c].GetComponent<RectTransform>().anchoredPosition = new Vector2(c * delta, r * delta);
                tiles[r * cols + c].GetComponent<Tile>().Init(isFlag, isRock);
            }
        }
    }
}
