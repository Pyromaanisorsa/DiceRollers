using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoDiceManager : MonoBehaviour
{
    public static GoDiceManager Instance;

    // Queue to run actions in MainThread
    private static readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    // Reference to launched process
    private Process diceServer;
    
    // Server communication & background thread for receiving messages
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    // Boolean statses
    private bool isRunning = false;
    public bool diceConnected { get; private set; } = false; // Used to keep track inside Unity if a dice is connected to the server

    // Server connection config data
    private readonly string host = "127.0.0.1";
    private readonly int port = 5005;

    // UI Elements for dice display
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private TMP_Text diceName;
    [SerializeField] private TMP_Text diceBattery;
    [SerializeField] private TMP_Text diceRoll;
    private string currentDice;

    private void Awake()
    {
        // Create instance and ensure that THERE CAN ONLY BE ONE!
        if(Instance != null && Instance != this) 
        {
            UnityEngine.Debug.Log("There can only be one!");

            //Rebind UI references to new scene references (works for current demo setup)
            Instance.connectButton = connectButton;
            Instance.disconnectButton = disconnectButton;
            Instance.diceName = diceName;
            Instance.diceBattery = diceBattery;
            Instance.diceRoll = diceRoll;

            //Clean out old listeners
            Instance.connectButton.onClick.RemoveAllListeners();
            Instance.disconnectButton.onClick.RemoveAllListeners();

            //Setup button listeners to the new buttons
            Instance.connectButton.onClick.AddListener(Instance.ConnectDice);
            Instance.disconnectButton.onClick.AddListener(Instance.DisconnectDice);
            Instance.disconnectButton.gameObject.SetActive(false);

            // Update Dice UI if dice was connected during restart
            if (Instance.diceConnected)
            {
                Instance.diceName.text = Instance.currentDice;
                connectButton.gameObject.SetActive(false);
                disconnectButton.gameObject.SetActive(true);
            }

            Destroy(gameObject);
            return;
        }

        // Store instance reference, add listeners to buttons and start connecting to diceServer
        Instance = this;
        DontDestroyOnLoad(gameObject);
        connectButton.onClick.AddListener(ConnectDice);
        disconnectButton.onClick.AddListener(DisconnectDice);
        disconnectButton.gameObject.SetActive(false);
        ConnectToServer();
    }

    private void Update()
    {
        // Check if there are any actions to be run in MainThread
        while (mainThreadActions.TryDequeue(out var action))
            action?.Invoke();
    }

    // Add Action to MainThread Queue from background thread
    private static void RunOnMainThread(Action action)
    {
        mainThreadActions.Enqueue(action);
    }

    // Connect to DiceServer / Background service
    private void ConnectToServer()
    {
        // Start server process
        StartPythonServer();

        // Connection variables
        int attempts = 0;
        int maxAttemps = 5;
        bool isConnected = false;
        client = new TcpClient(); // Used to establish TCP connection to the diceServer

        // Try connecting few times (if Unity is faster than the server startup)
        while (!isConnected && attempts < maxAttemps) 
        {
            try 
            {
                client.Connect(host, port);
                isConnected = true;
            }
            catch 
            {
                attempts++;
                UnityEngine.Debug.Log($"Connection attempt #{attempts}");
                Thread.Sleep(500);
            }
        }

        // Start listening for messages from server
        if (isConnected) 
        {
            stream = client.GetStream();
            isRunning = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Start();
            UnityEngine.Debug.Log("Connected to python dice server");
        }
        else 
        {
            UnityEngine.Debug.Log("Couldn't connect to python dice server.");
        }
    }

    // Start the background Dice server
    private void StartPythonServer()
    {
        // Get diceServer path
        string exePath = Path.Combine(Application.streamingAssetsPath + "/diceServer", "diceServer.exe");

        // Check if .exe even exists
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("Python server executable not found: " + exePath);
            return;
        }

        // Configure process start info
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Create and configure new process
        diceServer = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        // Attach output and error handlers
        diceServer.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log("[Server] " + args.Data);
        diceServer.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError("[Server ERROR] " + args.Data);

        // Try starting the process and begin reading output
        try
        {
            diceServer.Start();
            diceServer.BeginOutputReadLine();
            diceServer.BeginErrorReadLine();
            UnityEngine.Debug.Log("Python server started.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to start Python server: " + ex.Message);
        }
    }

    // Read messages received from server
    private void ReceiveLoop()
    {
        // Temporarily store incoming data here (1KB size)
        byte[] buffer = new byte[1024];

        // Keep running ReceiveLoop as long as the server connection lasts
        while (isRunning) 
        {
            try 
            {
                int length = stream.Read(buffer, 0, buffer.Length); // Blocking call; waits until data is available
                if(length > 0) 
                {
                    // Decode message to string / json
                    string message = Encoding.UTF8.GetString(buffer, 0, length);
                    HandleMessage(message);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Server error: " + e.Message);
                isRunning = false;
            }
        }
    }

    // Handle received messages
    private void HandleMessage(string message)
    {
        try
        {
            // Convert message to JObject json
            JObject messageJson = JObject.Parse(message);

            // Check if json contains type field that is not null value
            if (messageJson["type"] != null)
            {
                // Get type value from messageJson
                string msgType = messageJson["type"].ToString();
                switch (msgType)
                {
                    // Send received roll value to RollManager (used for active local rolla)
                    case "roll":
                        int rollValue = messageJson["value"].ToObject<int>();
                        UnityEngine.Debug.Log($"Rolled {rollValue}.");
                        RunOnMainThread(() =>
                        {
                            diceRoll.text = $"Rolled {rollValue}";
                            RollManager.Instance.SendLocalRollValue(rollValue);

                        });
                        break;
                    // Set connected diceName visible and toggle connect/disconnect button
                    case "connect":
                        bool success = messageJson["success"].ToObject<bool>();
                        if (success)
                        {
                            diceConnected = true;
                            string name = messageJson["message"].ToString();

                            UnityEngine.Debug.Log($"Connected dice {name}.");
                            RunOnMainThread(() =>
                            {
                                currentDice = name;
                                diceName.text = name;
                                disconnectButton.gameObject.SetActive(true);
                            });
                        }
                        else
                        {
                            UnityEngine.Debug.Log($"Failed to connect dice.");
                            RunOnMainThread(() =>
                            {
                                connectButton.gameObject.SetActive(true);
                            });
                            
                        }
                        break;
                    // Remove diceName text and toggle connect/disconnect button
                    case "disconnect":
                        diceConnected = false;
                        UnityEngine.Debug.Log($"Dice disconnected: {messageJson["die"]}");
                        RunOnMainThread(() =>
                        {
                            diceName.text = "";
                            connectButton.gameObject.SetActive(true);
                        });
                        break;
                    // Unknown type
                    default:
                        UnityEngine.Debug.Log($"Unknown event-message type: {msgType}");
                        break;
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[GoDiceManager] JSON parse error: {e.Message}\n{e}");
        }
    }

    // Send JSON message to server
    private void SendCommand(DiceCommand command)
    {
        // Try sending message to diceServer
        try
        {
            // Stream null check
            if (stream == null || !stream.CanWrite)
                return;

            // Convert command string to Json, encode it with UTF8 and write the json as byte array to network stream
            string json = JsonConvert.SerializeObject(command);
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[GoDiceManager] Send failed: {e.Message}");
        }
    }

    // Connect dice function for ConnectButton
    private void ConnectDice()
    {
        // Send "connect" message to diceServer
        connectButton.gameObject.SetActive(false);
        SendCommand(new DiceCommand("connect"));
    }

    // Disconnect dice function for DisconnectButton
    private void DisconnectDice()
    {
        // Send "discconect" message to diceServer
        disconnectButton.gameObject.SetActive(false);
        SendCommand(new DiceCommand("disconnect"));
    }

    // Game shut down; tell server to shut down and kill the process
    private void OnApplicationQuit()
    {
        // Send server "shutdown" command
        SendCommand(new DiceCommand("shutdown"));
        isRunning = false;

        // Closes network stream, TCP client and wait for receiveThread to finish
        stream?.Close();
        client?.Close();
        receiveThread?.Join();

        // Kill python server if it's still active
        if (diceServer != null && !diceServer.HasExited)
        {
            diceServer.Kill();
            diceServer.Dispose();
            UnityEngine.Debug.Log("Python server stopped.");
        }
    }
}

// JSON message helper class for communicating with server
public class DiceCommand
{
    public DiceCommand(string type) 
    {
        this.type = type;
    }

    public string type { get; set; }
}
