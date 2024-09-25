using UnityEngine;

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
