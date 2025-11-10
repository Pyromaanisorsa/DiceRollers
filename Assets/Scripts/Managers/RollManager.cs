using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class RollManager : MonoBehaviour
{
    // UI Elements & Instance of RollManager
    public static RollManager Instance;
    [Header("UI Elements")]
    [SerializeField] private TMP_Text textFieldTitle;
    [SerializeField] private TMP_Text textFieldTop;
    [SerializeField] private TMP_Text textFieldMiddleTop;
    [SerializeField] private TMP_Text textFieldMiddleBottom;
    [SerializeField] private TMP_Text textFieldBottom;
    [SerializeField] private TMP_Text textFieldRollStatus;
    [SerializeField] private TMP_Text textFieldRollResult;
    [SerializeField] private GameObject rollUI;
    [SerializeField] private Button retryButton;
    [Space(10)]

    [Header("Polling Wait Settings")]
    [SerializeField][Range(3, 5)] private int pollingAttempts;
    [SerializeField][Range(3, 10)] private int pollingRate;
    [SerializeField][Range(5, 20)] private int localRollTimeout;
    [Space(10)]

    // Inner lock variables to allow only one roll / request at a time
    private bool rolling = false;
    private bool requesting = false;

    // Received Roll data
    private int requiredRoll;
    private int rollBonusValue;
    public string playerID;
    private RollType currentRollType;
    private Action<int, bool> callback;

    // Coroutine reference for local rolling
    private Coroutine rollRoutine;
    
    // AWS Routes
    private static string requestRollUrl = "YOUR-REQUESTROLL HTTP GATEWAY URL+ROUTE";
    private static string checkResultUrl = "YOUR-CHECKROLLRESULT HTTP GATEWAY URL+ROUTE";

    // Create instance for manager
    void Awake()
    {
        // Create instance and ensure that THERE CAN ONLY BE ONE!
        if (Instance == null)
        {
            Instance = this;
            retryButton.onClick.AddListener(() => RollAWS());
        }
        // Instance already exists, you fool!
        else
        {
            Debug.Log("There can only be one!");
            Destroy(gameObject);
        }
    }
    
    // Start dialogue dice roll event. Locally if dice connected, else via AWS
    public void StartDialogueRoll(int requiredRoll, AttributeType attribute, int rollBonusValue, string playerID)
    {
        // Only one roll allowed at same time
        if (rolling)
            return;

        // Lock the roll, enable roll UI and setup texts
        rolling = true;
        rollUI.SetActive(true);
        textFieldTitle.text = $"{attribute} DC";
        textFieldTop.text = $"Required Roll: {requiredRoll}";
        textFieldMiddleTop.text = $"Attribute Bonus: {rollBonusValue}";
        textFieldMiddleBottom.text = "Roll Result: ";
        textFieldBottom.text = "Total Result: ";
        textFieldRollStatus.text = "";
        textFieldRollResult.text = "";

        // Store values needed for rolling
        currentRollType = RollType.Dialogue;
        this.requiredRoll = requiredRoll;
        this.rollBonusValue = rollBonusValue;
        this.playerID = playerID;
        callback = null;

        // Check via GoDiceManager if dice connected; either roll locally or via AWS
        if (GoDiceManager.Instance.diceConnected == true)
            rollRoutine = StartCoroutine(RollLocalRoutine());
        else
            RollAWS();
    }

    // Start turn dice roll event.
    public void StartTurnRoll(string playerID, string rollerID, int startAP, Action<int, bool> callback)
    {
        // Only one roll allowed at same time
        if (rolling)
            return;

        // Lock the roll, enable roll UI and setup texts
        rolling = true;
        rollUI.SetActive(true);
        textFieldTitle.text = $"StartTurn AP Roll";
        textFieldTop.text = rollerID;
        textFieldMiddleTop.text = $"Starting AP: {startAP}";
        textFieldMiddleBottom.text = "Roll Result: ";
        textFieldBottom.text = "Total AP for turn: ";
        textFieldRollStatus.text = "";
        textFieldRollResult.text = "";

        // Store necessary values for combat roll
        currentRollType = RollType.StartTurn;
        rollBonusValue = startAP;
        this.playerID = playerID;
        this.callback = callback;

        // Check via GoDiceManager if dice connected; either roll locally or via AWS
        if (GoDiceManager.Instance.diceConnected == true)
            rollRoutine = StartCoroutine(RollLocalRoutine());
        else
            RollAWS();
    }

    // Start ability dice roll event. Locally if dice connected, else via AWS
    public void StartAbilityRoll(string playerID, string rollerID, string abilityName, Action<int, bool> callback)
    {
        // Only one roll allowed at same time
        if (rolling)
            return;

        // Lock the roll, enable roll UI and setup texts
        rolling = true;
        rollUI.SetActive(true);
        textFieldTitle.text = $"Ability Roll: {abilityName}";
        textFieldTop.text = $"Caster: {rollerID}";
        textFieldMiddleTop.text = "";
        textFieldMiddleBottom.text = "Roll Result: ";
        textFieldBottom.text = "";
        textFieldRollStatus.text = "";
        textFieldRollResult.text = "";

        // Store necessary values for combat roll
        currentRollType = RollType.Ability;
        rollBonusValue = 0;
        this.playerID = playerID;
        this.callback = callback;

        // Check via GoDiceManager if dice connected; either roll locally or via AWS
        if (GoDiceManager.Instance.diceConnected == true)
            rollRoutine = StartCoroutine(RollLocalRoutine());
        else
            RollAWS();
    }

    // Start rolling via AWS
    private async Task RollAWS()
    {
        // Only allow one rollRequest at time (spares me from the bills / don't have to commit tax fraud)
        if (requesting)
            return;

        // Lock operation and hide retry button
        requesting = true;
        retryButton.gameObject.SetActive(false);

        // Try to start rollRequest in AWS
        if (!await RequestRoll())
        {
            textFieldRollStatus.text = "Failed to create request to AWS!";
            requesting = false; rolling = false;
            retryButton.gameObject.SetActive(true);
            return;
        }

        // Poll for RollResult from AWS
        int rollValue = await PollForResult();

        // Used to store boolean result of roll (Success/Failure)
        bool result;

        // Didn't receive roll in time; roll locally
        if (rollValue == 0)
        {
            Debug.Log("Failed to get the result from AWS in time.");
            textFieldRollStatus.text = "Failed to get the result in time, rolling locally.";
            rollValue = UnityEngine.Random.Range(1, 21);
            result = ProcessRollResult(rollValue);
            await Task.Delay(5000);

            rollUI.SetActive(false);
            rolling = false; requesting = false;
            ReturnRollResult(rollValue, result);
            return;
        }

        // Process the result and return the rollResult to caller
        result = ProcessRollResult(rollValue);
        await Task.Delay(3000);
        rollUI.SetActive(false);
        rolling = false; requesting = false;
        ReturnRollResult(rollValue, result);

        // Start rollRequest entry in AWS
        async Task<bool> RequestRoll()
        {
            // Set up a web request using UnityWebRequest to POST form data
            string jsonData = $"{{\"playerID\": \"{playerID}\"}}";
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(requestRollUrl, jsonData))
            {
                request.timeout = 15;
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
                request.SetRequestHeader("Content-Type", "application/json");

                // Send web request asynchronously
                var operation = request.SendWebRequest();

                // Wait until the operation is completed (non-blocking)
                while (!operation.isDone)
                    await Task.Yield();

                // If request creation successful; return true
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    Debug.Log("Roll request sent successfully" + response);
                    textFieldRollStatus.text = "Roll request sent successfully.";
                    return true;
                }
                else
                {
                    Debug.LogError("Error: " + request.error);
                    return false;
                }
            }
        }

        // Poll for rollRequest until it's complete / timeout
        async Task<int> PollForResult()
        {
            // Set up a web request Json
            string jsonData = $"{{\"playerID\": \"{playerID}\"}}";
            string jsonPayload = JsonUtility.ToJson(jsonData);
            int usedAttemtps = 0; // Track polling tries

            // Keep trying to poll for roll result until out of pollingAttempts OR request is ready
            while (true && usedAttemtps < pollingAttempts)
            {
                // Wait for polling delay time and set up a UnityWebRequest
                await Task.Delay(pollingRate * 1000); // polling delay; multiplies int by 1000 to get full seconds
                using (UnityWebRequest request = UnityWebRequest.PostWwwForm(checkResultUrl, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData); // Convert Jason to bytes
                    request.timeout = 15;
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw); // sending what to server as bytes
                    request.downloadHandler = new DownloadHandlerBuffer(); // store response as bytes
                    request.SetRequestHeader("Content-Type", "application/json"); // tell server that data contains json for decoding

                    // Send web request asynchronously
                    var operation = request.SendWebRequest();

                    // Wait until the operation is completed (non-blocking)
                    while (!operation.isDone)
                        await Task.Yield();

                    // Request successful; check response
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        // Convert response to local class format
                        RollResponse response = JsonUtility.FromJson<RollResponse>(request.downloadHandler.text);

                        // If roll value is not 0; return the value to Roll flow
                        if (response.value != 0)
                        {
                            // Result is ready, handle the roll value
                            Debug.Log($"Roll result for {playerID}: {response.value}");
                            textFieldRollStatus.text = "Roll result obtained for " + playerID;

                            // Stop polling now that we have the result
                            return response.value;
                        }
                        // Request not ready yet; try again if any attempts left
                        else 
                        {
                            Debug.Log("Roll not completed yet, retrying...");
                            textFieldRollStatus.text = $"Roll not completed yet.\nAttempts used: {usedAttemtps+1}/{pollingAttempts}";
                        }
                    }
                    // Error attempting polling request
                    else
                    {
                        Debug.LogError("Error checking roll result: " + request.error);
                        textFieldRollStatus.text = $"Error checking roll result.\nAttempts used: {usedAttemtps+1}/{pollingAttempts}";
                        return 0;
                    }
                }
                usedAttemtps++;
            }
            return 0;
        }
    }

    // 1st part of local roll flow: wait for roll result - roll with RNG if time out
    IEnumerator RollLocalRoutine()
    {
        // Wait for rollTimeOut to get the local dice roll value
        requesting = true;
        for (int i = 0; i < localRollTimeout; i++)
        {
            textFieldRollStatus.text = $"Time left {localRollTimeout - i} seconds.";
            yield return new WaitForSeconds(1);
        }

        // Timed out; roll locally with RNG
        requesting = false;
        Debug.Log("Failed to get local roll in time.");
        textFieldRollStatus.text = "Failed to get the result in time, rolling RNG.";

        int rollValue = UnityEngine.Random.Range(1, 21);
        bool result = ProcessRollResult(rollValue);
        StartCoroutine(FinishLocalRoll(rollValue, result));
    }

    // 2nd part of local roll flow: Send local rolled value via GoDiceManager
    public void SendLocalRollValue(int rollValue)
    {
        // Only use function if rolling (rollRoutine != null, ensures that only local rolls will be able to run this function flow)
        if (rolling == true && requesting == true && rollRoutine != null)
            RollLocal(rollValue);
    }

    // 3rd part of local roll flow: Use received roll value to process the roll
    private void RollLocal(int rollValue)
    {
        // Stop the local roll countdown coroutine
        if (requesting == true)
        {
            requesting = false;
            if (rollRoutine != null)
                StopCoroutine(rollRoutine);

            // Process the roll with received value
            bool result = ProcessRollResult(rollValue);
            StartCoroutine(FinishLocalRoll(rollValue, result));
        }
    }

    // Delay closing roll UI after local roll result. Finish the local roll.
    IEnumerator FinishLocalRoll(int rollValue, bool result)
    {
        yield return new WaitForSeconds(3);
        rollUI.SetActive(false);
        rolling = false;
        ReturnRollResult(rollValue, result);
    }

    // Process if roll successful
    private bool ProcessRollResult(int rollValue)
    {
        // Calculate final roll value
        int endResult = rollValue + rollBonusValue;
        textFieldMiddleBottom.text += rollValue;
        if(rollBonusValue != 0)
            textFieldBottom.text += endResult;

        // Non-Dialogue (Combat) rolls don't need success/failure textField updates
        if(currentRollType != RollType.Dialogue) 
        {
            if (rollValue == 20)
                return true;
            else if (rollValue == 1)
                return false;
            else if (endResult >= requiredRoll)
                return true;
            else
                return false;
        }

        // For rollTypes that can succeed/fail; print success/failure to textFields
        if (rollValue == 20)
        {
            textFieldRollResult.text = "CRITICAL SUCCESS";
            return true;
        }
        else if (rollValue == 1)
        {
            textFieldRollResult.text = "CRITICAL FAILURE";
            return false;
        }
        else if (endResult >= requiredRoll)
        {
            textFieldRollResult.text = "SUCCESS";
            return true;
        }
        else
        {
            textFieldRollResult.text = "FAILURE";
            return false;
        }
    }

    // Roll Ending: Returns roll result to caller / calls callback function
    private void ReturnRollResult(int rollValue, bool rollResult) 
    {
        // Return back to caller depending on RollType (someday Dialogue will use callback too)
        switch (currentRollType)
        {
            case RollType.Dialogue:
                DialogueManager.Instance.ResultProcess(rollResult);
                break;
            case RollType.StartTurn:
                callback(rollValue, rollResult);
                break;
            case RollType.Ability:
                callback(rollValue, rollResult);
                break;
        }
    }
}

// Enum for PlayerAttributes used for rolling
public enum AttributeType
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
};

// Enum for RollType
public enum RollType
{
    Dialogue,
    StartTurn,
    Ability
}

// Enum for RollResponse from AWS
[System.Serializable]
public class RollResponse
{
    public string message;
    public string stats;
    public int value;
}