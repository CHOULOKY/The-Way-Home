using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu]
public class ChapterDatabase : ScriptableObject
{
    public Chapter[] chapter;

    public int ChapterCount
    {
        get { return chapter.Length; }
    }

    public Chapter GetChapter(int index)
    {
        return chapter[index];
    }
}
