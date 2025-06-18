using System;
using UnityEngine;
using UnityEngine.Video;

public class VideoChanger : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject firstVideo,secondVideo;
    private bool hasSwitched = false;
    [SerializeField] private VideoClip firstClip;
    [SerializeField] private VideoClip secondClip;


    private void Awake()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
        
    }
    private void Start()
    {
        PlayFirstVideo();
    }
    private void PlayFirstVideo()
    {
        videoPlayer.clip = firstClip;
        videoPlayer.isLooping = false;
        firstVideo.SetActive(true);
        secondVideo.SetActive(false);
        videoPlayer.Play();
    }
    private void PlaySecondVideo()
    {
        videoPlayer.clip = secondClip;
        videoPlayer.isLooping = true;
        firstVideo.SetActive(false);
        secondVideo.SetActive(true);
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (!hasSwitched)
        {
            hasSwitched = true;
            PlaySecondVideo();
        }
    }
}
