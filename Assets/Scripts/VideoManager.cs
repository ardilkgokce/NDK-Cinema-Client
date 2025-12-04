using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
public class VideoManager : MonoBehaviour
{
    public DOTweenAnimation menuFadeAnimation;
    public Material skyboxMaterial;
    public RenderTexture masterTexture;
    public RenderTexture wonderlandTexture;
    public RenderTexture cocaColaTexture;

    private TicketsManager ticketsManager;

    private VideoPlayer videoPlayer;
    private double videoPlayerStart;
    private double videoPlayerStop;
    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        ticketsManager = GetComponent<TicketsManager>();
        // Debug.Log(Application.dataPath.Replace("/Assets", string.Empty));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) && !menuFadeAnimation.tween.IsPlaying())
        {
            if (!videoPlayer.isPlaying)
                StartMovie(0, 0d, 100d);
            else
                StopMovie();
        }
        if (videoPlayer.isPlaying && videoPlayer.time > videoPlayerStop)
        {
            MenuFade();
            
            videoPlayer.Stop();
            ticketsManager.UpdateTickets(3);
        }
    }

    private void MenuFade()
    {
        menuFadeAnimation.endValueFloat = (menuFadeAnimation.endValueFloat - 1f) * -1f;
        menuFadeAnimation.RecreateTweenAndPlay();
    }

    public void StartMovie(int selectedMovie, double videoPlayerStart, double videoPlayerStop)
    {
        Debug.Log("Starting Movie: " + selectedMovie + " " + videoPlayerStart + " " + videoPlayerStop);
        switch (selectedMovie)
        {
            case 0:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_Haunted.mp4";
                videoPlayer.targetTexture = masterTexture;
                skyboxMaterial.mainTexture = masterTexture;
                break;
            case 1:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_DinoChase.mp4";
                break;
            case 2:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_Motocross.mp4";
                break;
            case 3:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_Starlight.mp4";
                break;
            case 4:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_PiratesGold.mp4";
                break;
            case 5:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_RiverRush.mp4";
                break;
            case 6:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_WallOfChina.mp4";
                break;
            case 7:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_Wonderland.mp4";
                break;
            case 8:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_CocaCola.mp4";
                break;
            case 9:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_NightBefore.mp4";
                break;
            case 10:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_ToyFactory.mp4";
                break;
            case 11:
                videoPlayer.url = "file:///storage/emulated/0/Movies/Movie_SantaFly.mp4";
                break;
            default:
                break;
        
        }
        MenuFade();

        this.videoPlayerStart = videoPlayerStart;
        this.videoPlayerStop = videoPlayerStop;

        if (selectedMovie == 7)
        {
            videoPlayer.targetTexture = wonderlandTexture;
            skyboxMaterial.mainTexture = wonderlandTexture;
        }
        else if (selectedMovie == 8)
        {
            videoPlayer.targetTexture = cocaColaTexture;
            skyboxMaterial.mainTexture = cocaColaTexture;
        }
        else
        {
            videoPlayer.targetTexture = masterTexture;
            skyboxMaterial.mainTexture = masterTexture;
        }
        
        videoPlayer.Play();
        videoPlayer.time = videoPlayerStart;

        ticketsManager.UpdateTickets(0);
    }

    public void StopMovie()
    {
        if (videoPlayer.time - videoPlayerStart <= 30)
        {
            ticketsManager.UpdateTickets(1);
        }
        else
            ticketsManager.UpdateTickets(2);
        MenuFade();
        videoPlayer.Stop();
    }
}