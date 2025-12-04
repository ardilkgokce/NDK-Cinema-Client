using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR;

public struct UDPState
{
    private UdpClient udpClient;
    private IPEndPoint ipEndPoint;
    public UDPState(UdpClient udpClient, IPEndPoint ipEndPoint)
    {
        this.udpClient = udpClient;
        this.ipEndPoint = ipEndPoint;
    }

    public UdpClient GetUDPClient()
    {
        return udpClient;
    }

    public IPEndPoint GetIPEndPoint()
    {
        return ipEndPoint;
    }
}

public class PicoCommunicator : MonoBehaviour {
    private VideoManager videoManager;

    private bool startMovie = false;
    private bool stopMovie = false;
    private int selectedMovie = 2;

    private double videoPlayerStart;
    private double videoPlayerStop;

    private bool isPaused = false;

    private UdpClient udpClient;
    private IPEndPoint ipEndPoint;
    private UDPState udpState;
    private string ipAddress = "0.0.0.0";

    private InputDevice rightController;
    private bool previousTriggerState = false;
    private bool previousButtonState = false;

    // DEBUG: UDP paket alındığında ses çal
    private AudioSource debugAudioSource;
    private bool playDebugSound = false;
    private string lastReceivedMessage = "";
    private void Start()
    {
        Application.runInBackground = true;
        videoManager = GetComponent<VideoManager>();

        // DEBUG: Ses kaynağı oluştur (bip sesi için)
        debugAudioSource = gameObject.AddComponent<AudioSource>();
        debugAudioSource.playOnAwake = false;

        try
        {
            udpClient = new UdpClient(4242);
            ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.70.150"), 4242);
            udpState = new UDPState(udpClient, ipEndPoint);
            udpClient.BeginReceive(new AsyncCallback(DataReceived), udpState);
            Debug.Log("NDK-Cinema-Client is waiting for packages on port 4242");
        }
        catch (Exception exception)
        {
            Debug.Log("UDP ERROR: " + exception.ToString());
        }
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress tempIPAddress in ipHostEntry.AddressList)
        {
            if (tempIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = tempIPAddress.ToString();
                break;
            }
        }
    }

    private void Update()
    {
        // DEBUG: UDP paket alındığında bip sesi çal
        if (playDebugSound)
        {
            PlayBeepSound();
            Debug.Log("UDP RECEIVED: " + lastReceivedMessage);
            playDebugSound = false;
        }

        if (startMovie)
        {
            Debug.Log("UPDATE: Starting movie " + selectedMovie);
            videoManager.StartMovie(selectedMovie, videoPlayerStart, videoPlayerStop);
            startMovie = false;
        }
        if (stopMovie)
        {
            videoManager.StopMovie();
            stopMovie = false;
        }
        // Quest 3: A butonu veya sağ trigger ile Audio Request gönder
        if (GetAudioRequestInput())
        {
            DataDispatched("Audio_Request");
        }
        if (Input.GetKeyDown(KeyCode.J))
        {

        }
    }

    // DEBUG: Bip sesi oluştur ve çal
    private void PlayBeepSound()
    {
        int sampleRate = 44100;
        int frequency = 800; // Hz
        float duration = 0.3f; // saniye

        int sampleCount = (int)(sampleRate * duration);
        AudioClip beepClip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.5f;
        }
        beepClip.SetData(samples, 0);

        debugAudioSource.clip = beepClip;
        debugAudioSource.Play();
    }
    
    private bool GetAudioRequestInput()
    {
        if (!rightController.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0)
                rightController = devices[0];
        }

        if (rightController.isValid)
        {
            bool currentButtonState;
            if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out currentButtonState))
            {
                bool buttonJustPressed = currentButtonState && !previousButtonState;
                previousButtonState = currentButtonState;
                if (buttonJustPressed)
                    return true;
            }
            
            float triggerValue;
            if (rightController.TryGetFeatureValue(CommonUsages.trigger, out triggerValue))
            {
                bool currentTriggerState = triggerValue > 0.5f;
                bool triggerJustPressed = currentTriggerState && !previousTriggerState;
                previousTriggerState = currentTriggerState;
                if (triggerJustPressed)
                    return true;
            }
        }

        return false;
    }

    private void DataReceived(IAsyncResult iAsyncResult)
    {
        UdpClient newUdpClient = ((UDPState)iAsyncResult.AsyncState).GetUDPClient();
        IPEndPoint receivedIPEndPoint = new IPEndPoint(0, 0);

        Byte[] receivedBytes = newUdpClient.EndReceive(iAsyncResult, ref receivedIPEndPoint);
        string receivedString = Encoding.ASCII.GetString(receivedBytes);
        Debug.Log("UDP PACKET RECEIVED: " + receivedString + " from " + receivedIPEndPoint.ToString());

        // DEBUG: Ses çalmak için flag ayarla (main thread'de çalacak)
        lastReceivedMessage = receivedString;
        playDebugSound = true;

        // Start_Movie_5_10
        // Stop_Movie_0_0
        if (isPaused == false)
        {
            string receivedCommand = receivedString.Split(' ')[0];
            string[] splitCommand = receivedCommand.Split('_');
            switch (splitCommand[0] + "_" + splitCommand[1])
            {
                case "Start_Haunted":
                    selectedMovie = 0;
                    break;
                case "Start_DinoChase":
                    selectedMovie = 1;
                    break;
                case "Start_Motocross":
                    selectedMovie = 2;
                    break;
                case "Start_Starlight":
                    selectedMovie = 3;
                    break;
                case "Start_PiratesGold":
                    selectedMovie = 4;
                    break;
                case "Start_RiverRush":
                    selectedMovie = 5;
                    break;
                case "Start_WallOfChina":
                    selectedMovie = 6;
                    break;
                case "Start_Wonderland":
                    selectedMovie = 7;
                    break;
                case "Start_CocaCola":
                    selectedMovie = 8;
                    break;
                case "Start_NightBefore":
                    selectedMovie = 9;
                    break;
                case "Start_ToyFactory":
                    selectedMovie = 10;
                    break;
                case "Start_SantaFly":
                    selectedMovie = 11;
                    break;
                case "Stop_Movie":
                    stopMovie = true;
                    break;
            }
            if (!stopMovie)
            {
                videoPlayerStart = double.Parse(splitCommand[2], System.Globalization.CultureInfo.InvariantCulture);
                videoPlayerStop = double.Parse(splitCommand[3], System.Globalization.CultureInfo.InvariantCulture);
                startMovie = true;
            }
        }
        newUdpClient.BeginReceive(new AsyncCallback(DataReceived), iAsyncResult.AsyncState);
    }

    private void DataDispatched(string dataString)
    {
        try
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(dataString);
            udpClient.Send(byteArray, byteArray.Length, "192.168.70.150", 2424);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.ToString());
        }
    }

    private void OnApplicationFocus(bool isFocused)
    {
        this.isPaused = !isFocused;
    }

    private void OnApplicationPause(bool isPaused)
    {
        this.isPaused = isPaused;
    }
}