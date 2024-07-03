using Photon.Chat;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class ChapterManager : MonoBehaviourPunCallbacks
{
    public ChapterDatabase chapterDB;

    public Text chapterText;
    public SpriteRenderer artworkSprite;

    private int selectedOption = 0;

    private void Start()
    {

    }

    public void NextOption()
    {
        selectedOption++;
        if (selectedOption >= chapterDB.ChapterCount)
        {
            selectedOption = 0;
        }

        UpdateCharacter(selectedOption);
        //Save();
    }

    public void BackOption()
    {
        selectedOption--;
        if (selectedOption < 0)
        {
            selectedOption = chapterDB.ChapterCount - 1;
        }

        UpdateCharacter(selectedOption);
        //Save();
    }

    private void UpdateCharacter(int selectedOption)
    {
        Chapter chapter = chapterDB.GetChapter(selectedOption);
        //artworkSprite.sprite = chapter.chapterSprite;
        chapterText.text = chapter.chapterNum;
    }


    [PunRPC]
    void ChapterRPC(string chapterNum)
    {
        chapterText.text = chapterNum;
    }

    /*private void Load()
    {
        selectedOption = PlayerPrefs.GetInt("SelectedOption");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("SelectedOption", selectedOption);
    }

    public void ChangedScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }*/


}

