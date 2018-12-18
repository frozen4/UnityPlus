using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAtlas : ScriptableObject
{
    public Texture2D Tex;

    public Sprite[] SpriteList;

    public Sprite FindSprite(string sp_name)
    {
        if (SpriteList != null)
        {
            for (int i = 0; i < SpriteList.Length; i++)
            {
                if (SpriteList[i].name == sp_name)
                {
                    return SpriteList[i];
                }
            }
        }
        return null;
    }
}
