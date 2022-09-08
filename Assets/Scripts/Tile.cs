using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    Image TileImage;
    [SerializeField]
    Sprite GroundSprite;
    [SerializeField]
    Sprite FlagSprite;
    [SerializeField]
    Sprite RockSprite;

    bool isFlag = false;
    bool isRock = false;

    // Update is called once per frame
    public void Init(bool _isFlag, bool _isRock)
    {
        if (_isFlag)
        {
            isFlag = true;
            TileImage.sprite = FlagSprite;
        }
        else if (_isRock)
        {
            isRock = true;
            TileImage.sprite = RockSprite;
        }
        else
        {
            TileImage.sprite = GroundSprite;
        }
    }

    public bool IsFlag()
    {
        return isFlag;
    }

    public bool IsRock()
    {
        return isRock;
    }
}
