using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private string[] movieNames = { "Haunted", "DinoChase", "Motocross", "Starlight", "PiratesGold",
        "RiverRush", "WallOfChina", "Wonderland", "CocaCola", "NightBefore", "ToyFactory", "SantaFly" };

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        ticketsManager = GetComponent<TicketsManager>();

        // Video player event handlers
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.started += OnVideoStarted;

        Debug.Log("[VIDEO] VideoManager initialized");
        Debug.Log("[VIDEO] Video folder path: /storage/emulated/0/Movies/");

        // Check video files on start
        CheckAllVideoFiles();
    }

    private void CheckAllVideoFiles()
    {
        Debug.Log("[VIDEO] Checking video files...");
        string basePath = "/storage/emulated/0/Movies/";

        for (int i = 0; i < movieNames.Length; i++)
        {
            string filePath = basePath + "Movie_" + movieNames[i] + ".mp4";
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                Debug.Log("[VIDEO] FOUND: Movie_" + movieNames[i] + ".mp4 (" + (fi.Length / 1024 / 1024) + " MB)");
            }
            else
            {
                Debug.Log("[VIDEO] MISSING: Movie_" + movieNames[i] + ".mp4");
            }
        }
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.Log("[VIDEO ERROR] " + message);
        Debug.Log("[VIDEO ERROR] URL was: " + source.url);
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("[VIDEO] Prepared - Duration: " + source.length.ToString("F1") + "s, Resolution: " + source.width + "x" + source.height);
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        Debug.Log("[VIDEO] Playback started at time: " + source.time.ToString("F1") + "s");
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
        Debug.Log("[VIDEO] ========== STARTING MOVIE ==========");
        Debug.Log("[VIDEO] Movie Index: " + selectedMovie + " (" + (selectedMovie < movieNames.Length ? movieNames[selectedMovie] : "INVALID") + ")");
        Debug.Log("[VIDEO] Start Time: " + videoPlayerStart + "s, Stop Time: " + videoPlayerStop + "s");

        string videoUrl = "";
        switch (selectedMovie)
        {
            case 0:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_Haunted.mp4";
                break;
            case 1:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_DinoChase.mp4";
                break;
            case 2:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_Motocross.mp4";
                break;
            case 3:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_Starlight.mp4";
                break;
            case 4:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_PiratesGold.mp4";
                break;
            case 5:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_RiverRush.mp4";
                break;
            case 6:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_WallOfChina.mp4";
                break;
            case 7:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_Wonderland.mp4";
                break;
            case 8:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_CocaCola.mp4";
                break;
            case 9:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_NightBefore.mp4";
                break;
            case 10:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_ToyFactory.mp4";
                break;
            case 11:
                videoUrl = "file:///storage/emulated/0/Movies/Movie_SantaFly.mp4";
                break;
            default:
                Debug.Log("[VIDEO ERROR] Invalid movie index: " + selectedMovie);
                return;
        }

        videoPlayer.url = videoUrl;
        Debug.Log("[VIDEO] URL set to: " + videoUrl);

        // Check if file exists
        string filePath = videoUrl.Replace("file:///", "/");
        if (File.Exists(filePath))
        {
            FileInfo fi = new FileInfo(filePath);
            Debug.Log("[VIDEO] File EXISTS - Size: " + (fi.Length / 1024 / 1024) + " MB");
        }
        else
        {
            Debug.Log("[VIDEO ERROR] File NOT FOUND at: " + filePath);
        }

        MenuFade();

        this.videoPlayerStart = videoPlayerStart;
        this.videoPlayerStop = videoPlayerStop;

        if (selectedMovie == 7)
        {
            videoPlayer.targetTexture = wonderlandTexture;
            skyboxMaterial.mainTexture = wonderlandTexture;
            Debug.Log("[VIDEO] Using Wonderland texture");
        }
        else if (selectedMovie == 8)
        {
            videoPlayer.targetTexture = cocaColaTexture;
            skyboxMaterial.mainTexture = cocaColaTexture;
            Debug.Log("[VIDEO] Using CocaCola texture");
        }
        else
        {
            videoPlayer.targetTexture = masterTexture;
            skyboxMaterial.mainTexture = masterTexture;
            Debug.Log("[VIDEO] Using Master texture");
        }

        videoPlayer.Play();
        videoPlayer.time = videoPlayerStart;
        Debug.Log("[VIDEO] Play() called, seeking to: " + videoPlayerStart + "s");

        ticketsManager.UpdateTickets(0);
    }

    public void StopMovie()
    {
        Debug.Log("[VIDEO] ========== STOPPING MOVIE ==========");
        Debug.Log("[VIDEO] Current time: " + videoPlayer.time.ToString("F1") + "s");
        Debug.Log("[VIDEO] Watch duration: " + (videoPlayer.time - videoPlayerStart).ToString("F1") + "s");

        if (videoPlayer.time - videoPlayerStart <= 30)
        {
            Debug.Log("[VIDEO] Stopped early (< 30s)");
            ticketsManager.UpdateTickets(1);
        }
        else
        {
            Debug.Log("[VIDEO] Stopped after 30s");
            ticketsManager.UpdateTickets(2);
        }
        MenuFade();
        videoPlayer.Stop();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.started -= OnVideoStarted;
        }
    }
}