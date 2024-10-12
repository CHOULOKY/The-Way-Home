using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("#BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    private AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    public enum Sfx { Swing_Girl, Air_Swing_Girl, Swing_Robot, Melee, Melee2, Hit, Hit2, Destroy, Destroy2, Dead, Lose, Win }
    // Swing: 플레이어 공격 효과음 | Melee: 적 피격 효과음, 오브젝트 파괴 효과음
    // Hit: 플레이어 피격 효과음 | Destroy 
    // Walk, Walk2, Jump_Start, Jump_Land
    // Button, Select

    private void Awake()
    {
        instance = this;
        Init();
    }

    private void Init()
    {
        // 배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // 효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int index = 0; index < sfxPlayers.Length; index++) {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
        }
    }

    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
            bgmPlayer.Play();
        else
            bgmPlayer.Stop();
    }

    public void PlaySfx(Sfx sfx)
    {
        for (int index = 0; index < sfxPlayers.Length; index++) {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            if(sfx==Sfx.Lose) {
                sfxPlayers[loopIndex].volume = sfxVolume * 0.25f;
            } else {
                sfxPlayers[loopIndex].volume = sfxVolume;
            }

            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx == Sfx.Melee || sfx == Sfx.Destroy)
                ranIndex = Random.Range(0, 2);

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
}
