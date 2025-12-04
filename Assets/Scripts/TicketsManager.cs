using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class TicketsManager : MonoBehaviour
{
    public GameObject ticketsPanel;
    private TextMeshProUGUI ticketsInfo;

    private int movieStarted = 0;
    private int movieStoppedBefore30 = 0;
    private int movieStoppedAfter30 = 0;
    private int movieFinished = 0;
    private void Start()
    {
        ticketsInfo = ticketsPanel.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        if (PlayerPrefs.HasKey("Movie Started") == false)
        {
            PlayerPrefs.SetInt("Movie Started", movieStarted);
            PlayerPrefs.SetInt("Movie Stopped < 30s", movieStoppedBefore30);
            PlayerPrefs.SetInt("Movie Stopped > 30s", movieStoppedAfter30);
            PlayerPrefs.SetInt("Movie Finished", movieFinished);
        }
        else
        {
            movieStarted = PlayerPrefs.GetInt("Movie Started");
            movieStoppedBefore30 = PlayerPrefs.GetInt("Movie Stopped < 30s");
            movieStoppedAfter30 = PlayerPrefs.GetInt("Movie Stopped > 30s");
            movieFinished = PlayerPrefs.GetInt("Movie Finished");
        }
        ticketsPanel.SetActive(false);
        UpdateTicketsPanel();
    }

    private bool isClickedOnce = false;
    private bool isClickedTwice = false;
    private float isClickedAtAll = 0f;

    private InputDevice rightController;
    private bool previousButtonState = false;

    private bool GetTripleClickInput()
    {
        if (Input.GetKeyDown(KeyCode.T))
            return true;

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
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out currentButtonState))
            {
                bool justPressed = currentButtonState && !previousButtonState;
                previousButtonState = currentButtonState;
                return justPressed;
            }
        }

        return false;
    }

    private void Update()
    {
        if (isClickedOnce == false && GetTripleClickInput())
        {
            isClickedOnce = true;
        }
        else if (isClickedOnce && !isClickedTwice && GetTripleClickInput())
        {
            isClickedTwice = true;
        }
        else if (isClickedTwice && GetTripleClickInput())
        {
            ticketsPanel.SetActive(!ticketsPanel.activeSelf);
            isClickedOnce = isClickedTwice = false;
            isClickedAtAll = 0f;
        }
        if (isClickedOnce || isClickedTwice)
        {
            isClickedAtAll = isClickedAtAll + Time.deltaTime;
        }
        if (isClickedAtAll > 1f)
        {
            isClickedOnce = isClickedTwice = false;
            isClickedAtAll = 0f;
        }
        if (Input.GetKeyDown(KeyCode.P))
            PlayerPrefs.DeleteAll();
    }

    public void UpdateTickets(int actionType)
    {
        string[] actionString = ticketsInfo.text.Split('\n')[actionType].Split(' ');
        int ticketValue = int.Parse(actionString[actionString.Length - 1]) + 1;

        switch (actionType)
        {
            case 0:
                movieStarted = ticketValue;
                PlayerPrefs.SetInt("Movie Started", ticketValue);
                break;
            case 1:
                movieStoppedBefore30 = ticketValue;
                PlayerPrefs.SetInt("Movie Stopped < 30s", ticketValue);
                break;
            case 2:
                movieStoppedAfter30 = ticketValue;
                PlayerPrefs.SetInt("Movie Stopped > 30s", ticketValue);
                break;
            case 3:
                movieFinished = ticketValue;
                PlayerPrefs.SetInt("Movie Finished", ticketValue);
                break;
            default:
                break;
        }
        UpdateTicketsPanel();
    }
    
    private void UpdateTicketsPanel()
    {
        ticketsInfo.text =
            "Movie Started: " + movieStarted + "\n" +
            "Movie Stopped < 30s:  " + movieStoppedBefore30 + "\n" +
            "Movie Stopped > 30s: " + movieStoppedAfter30 + "\n" +
            "Movie Finished: " + movieFinished;
    }
}