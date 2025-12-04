using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR;

public class DebugUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI debugText;

    [Header("References (Auto-assigned if null)")]
    public VideoPlayer videoPlayer;
    public PicoCommunicator picoCommunicator;

    private StringBuilder logBuilder = new StringBuilder();
    private List<string> logHistory = new List<string>();
    private const int MAX_LOG_LINES = 30;

    // Network status
    private string localIP = "Checking...";
    private string udpStatus = "Initializing...";
    private int packetsReceived = 0;
    private string lastCommand = "None";
    private DateTime lastCommandTime;

    // Video status
    private string currentVideoPath = "None";
    private string videoStatus = "Idle";
    private string fileExistsStatus = "N/A";

    // Controller status
    private string controllerStatus = "Checking...";

    private float updateInterval = 0.5f;
    private float nextUpdateTime;

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (picoCommunicator == null)
            picoCommunicator = FindObjectOfType<PicoCommunicator>();

        // Get local IP
        GetLocalIP();

        // Register to Unity log callback
        Application.logMessageReceived += OnLogMessageReceived;

        // Initial log
        AddLog("[SYSTEM] DebugUI Started");
        AddLog("[SYSTEM] Unity Version: " + Application.unityVersion);
        AddLog("[SYSTEM] Platform: " + Application.platform);

        // Check video folder
        CheckVideoFolder();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateDebugDisplay();
        }

        // Check controller status
        CheckControllerStatus();
    }

    private void GetLocalIP()
    {
        try
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipHostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    AddLog("[NETWORK] Local IP: " + localIP);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            localIP = "Error: " + e.Message;
            AddLog("[NETWORK ERROR] " + e.Message);
        }
    }

    private void CheckVideoFolder()
    {
        string videoPath = "/storage/emulated/0/Movies/";

        AddLog("[VIDEO] Checking video folder: " + videoPath);

        try
        {
            if (Directory.Exists(videoPath))
            {
                AddLog("[VIDEO] Movies folder EXISTS");
                string[] files = Directory.GetFiles(videoPath, "*.mp4");
                AddLog("[VIDEO] Found " + files.Length + " MP4 files:");
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    AddLog("  - " + fileName);
                }
            }
            else
            {
                AddLog("[VIDEO WARNING] Movies folder NOT FOUND at: " + videoPath);

                // Try alternative paths
                string[] altPaths = new string[]
                {
                    Application.persistentDataPath,
                    Application.dataPath,
                    "/sdcard/Movies/",
                    "/storage/self/primary/Movies/"
                };

                foreach (string altPath in altPaths)
                {
                    AddLog("[VIDEO] Checking alt path: " + altPath);
                    if (Directory.Exists(altPath))
                    {
                        AddLog("[VIDEO] Alt path exists: " + altPath);
                    }
                }
            }
        }
        catch (Exception e)
        {
            AddLog("[VIDEO ERROR] " + e.Message);
        }
    }

    private void CheckControllerStatus()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0 && devices[0].isValid)
        {
            controllerStatus = "Right Controller: Connected";
        }
        else
        {
            controllerStatus = "Right Controller: NOT FOUND";
        }
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        string prefix = "[LOG]";
        switch (type)
        {
            case LogType.Error:
                prefix = "[ERROR]";
                break;
            case LogType.Warning:
                prefix = "[WARN]";
                break;
            case LogType.Exception:
                prefix = "[EXCEPTION]";
                break;
        }

        // Filter relevant logs
        if (condition.Contains("UDP") || condition.Contains("Movie") ||
            condition.Contains("Video") || condition.Contains("NDK") ||
            condition.Contains("Starting") || condition.Contains("Error") ||
            condition.Contains("Exception") || condition.Contains("UPDATE"))
        {
            AddLog(prefix + " " + condition);

            // Track specific events
            if (condition.Contains("UDP PACKET RECEIVED"))
            {
                packetsReceived++;
                lastCommand = condition.Replace("UDP PACKET RECEIVED: ", "").Split(' ')[0];
                lastCommandTime = DateTime.Now;
            }
            else if (condition.Contains("is waiting for packages"))
            {
                udpStatus = "Listening on port 4242";
            }
            else if (condition.Contains("UDP ERROR"))
            {
                udpStatus = "ERROR: " + condition;
            }
            else if (condition.Contains("Starting Movie"))
            {
                videoStatus = "Starting...";
            }
        }
    }

    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logEntry = "[" + timestamp + "] " + message;

        logHistory.Add(logEntry);

        // Keep only last N lines
        while (logHistory.Count > MAX_LOG_LINES)
        {
            logHistory.RemoveAt(0);
        }
    }

    private void UpdateDebugDisplay()
    {
        if (debugText == null) return;

        logBuilder.Clear();

        // Header
        logBuilder.AppendLine("========== NDK CINEMA DEBUG ==========");
        logBuilder.AppendLine();

        // Network Status
        logBuilder.AppendLine("--- NETWORK ---");
        logBuilder.AppendLine("Local IP: " + localIP);
        logBuilder.AppendLine("UDP Status: " + udpStatus);
        if (picoCommunicator != null)
            logBuilder.AppendLine("Target Server: " + picoCommunicator.serverIP + ":" + picoCommunicator.serverPort);
        else
            logBuilder.AppendLine("Target Server: (PicoCommunicator not found)");
        logBuilder.AppendLine("Packets Received: " + packetsReceived);
        logBuilder.AppendLine("Last Command: " + lastCommand);
        if (lastCommandTime != default)
        {
            logBuilder.AppendLine("Last Command Time: " + lastCommandTime.ToString("HH:mm:ss"));
        }
        logBuilder.AppendLine();

        // Video Status
        logBuilder.AppendLine("--- VIDEO ---");
        if (videoPlayer != null)
        {
            logBuilder.AppendLine("Status: " + (videoPlayer.isPlaying ? "PLAYING" : "STOPPED"));
            logBuilder.AppendLine("URL: " + (string.IsNullOrEmpty(videoPlayer.url) ? "None" : videoPlayer.url));

            if (videoPlayer.isPlaying)
            {
                logBuilder.AppendLine("Time: " + videoPlayer.time.ToString("F1") + "s / " + videoPlayer.length.ToString("F1") + "s");
                logBuilder.AppendLine("Frame: " + videoPlayer.frame + " / " + videoPlayer.frameCount);
            }

            // Check if current video file exists
            if (!string.IsNullOrEmpty(videoPlayer.url))
            {
                string filePath = videoPlayer.url.Replace("file:///", "/");
                try
                {
                    if (File.Exists(filePath))
                    {
                        FileInfo fi = new FileInfo(filePath);
                        logBuilder.AppendLine("File: EXISTS (" + (fi.Length / 1024 / 1024) + " MB)");
                    }
                    else
                    {
                        logBuilder.AppendLine("File: NOT FOUND!");
                    }
                }
                catch (Exception e)
                {
                    logBuilder.AppendLine("File Check Error: " + e.Message);
                }
            }
        }
        else
        {
            logBuilder.AppendLine("VideoPlayer: NOT FOUND!");
        }
        logBuilder.AppendLine();

        // Controller Status
        logBuilder.AppendLine("--- CONTROLLER ---");
        logBuilder.AppendLine(controllerStatus);
        logBuilder.AppendLine();

        // App Status
        logBuilder.AppendLine("--- APP ---");
        logBuilder.AppendLine("Time: " + Time.time.ToString("F1") + "s");
        logBuilder.AppendLine("FPS: " + (1f / Time.deltaTime).ToString("F0"));
        logBuilder.AppendLine("Focus: " + (Application.isFocused ? "YES" : "NO"));
        logBuilder.AppendLine();

        // Log History
        logBuilder.AppendLine("--- LOG HISTORY ---");
        foreach (string log in logHistory)
        {
            logBuilder.AppendLine(log);
        }

        debugText.text = logBuilder.ToString();
    }

    // Public methods to be called from other scripts
    public void LogUDPReceived(string message, string fromIP)
    {
        packetsReceived++;
        lastCommand = message;
        lastCommandTime = DateTime.Now;
        AddLog("[UDP IN] " + message + " from " + fromIP);
    }

    public void LogUDPSent(string message, string toIP)
    {
        AddLog("[UDP OUT] " + message + " to " + toIP);
    }

    public void LogVideoEvent(string eventType, string details)
    {
        AddLog("[VIDEO] " + eventType + ": " + details);
    }

    public void LogError(string source, string error)
    {
        AddLog("[ERROR] " + source + ": " + error);
    }
}
