using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;

public class StartGameAR : MonoBehaviour
{
    [SerializeField] private SharedSpaceManager _sharedSpaceManager;
    private const int MAX_AMOUNT_CLIENTS_ROOM = 2;

    [SerializeField] private Texture2D _targetImage;
    [SerializeField] private float _targetImageSize = 1.0f;
    private string roomName = "TestRoom";

    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button CreateRoomButton;
    [SerializeField] private Button JoinRoomButton;
    private bool isHost;

    public static event Action OnStartSharedSpaceHost;
    public static event Action OnJoinSharedSpaceClient;
    public static event Action OnStartGame;
    public static event Action OnStartSharedSpace;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Subscribe to shared space manager state changes
        _sharedSpaceManager.sharedSpaceManagerStateChanged += SharedSpaceManagerOnsharedSpaceManagerStateChanged;

        // Set up button listeners
        StartGameButton.onClick.AddListener(StartGame);
        CreateRoomButton.onClick.AddListener(CreateGameHost);
        JoinRoomButton.onClick.AddListener(JoinGameClient);

        // Disable the start game button initially
        StartGameButton.interactable = false;

        BlitImageForColocalization.OnTextureRendered += BlitImageForColocalizationOnTextureRendered;

    }

    private void OnDestroy()
    {

        _sharedSpaceManager.sharedSpaceManagerStateChanged -= SharedSpaceManagerOnsharedSpaceManagerStateChanged;
         BlitImageForColocalization.OnTextureRendered -= BlitImageForColocalizationOnTextureRendered;
    }



    private void BlitImageForColocalizationOnTextureRendered(Texture2D texture)
    {
        SetTargetImage(texture);
        StartSharedSpace();
    }

    private void SetTargetImage(Texture2D texture2D)
    {
        _targetImage = texture2D;
    }


    private void SharedSpaceManagerOnsharedSpaceManagerStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        // When tracking is available, allow room creation
        if (args.Tracking)
        {
            Debug.Log("Tracking is active. Room creation enabled.");
            StartGameButton.interactable = true;
        }
        else
        {
            Debug.LogWarning("Tracking is not active. Cannot create room.");
        }
    }

    private void StartGame()
    {
        Debug.Log("Start Game button pressed.");

        OnStartGame?.Invoke();

        if (isHost)
        {
            if (NetworkManager.Singleton != null)
            {
                Debug.Log("Starting host...");
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                Debug.LogError("NetworkManager is not initialized.");
            }
        }
        else
        {
            if (NetworkManager.Singleton != null)
            {
                Debug.Log("Starting client...");
                NetworkManager.Singleton.StartClient();
            }
            else
            {
                Debug.LogError("NetworkManager is not initialized.");
            }
        }
    }

    private void StartSharedSpace()
    {
        Debug.Log("Starting shared space...");

        OnStartSharedSpace?.Invoke();

        if (_sharedSpaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.MockColocalization)
        {
            Debug.Log("Using Mock Colocalization for room creation.");

            var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                MAX_AMOUNT_CLIENTS_ROOM,
                "MockColocalizationDemo"
            );

            _sharedSpaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
        }
        else if (_sharedSpaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.ImageTrackingColocalization)
        {
            Debug.Log("Using Image Tracking Colocalization for room creation.");

            var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(
                _targetImage, _targetImageSize
            );

            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                MAX_AMOUNT_CLIENTS_ROOM,
                "ImageColocalization"
            );

            _sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomArgs);
        }
        else
        {
            Debug.LogError("Unknown colocalization type.");
        }
    }

    private void CreateGameHost()
    {
        Debug.Log("Creating host room...");
        isHost = true;
        OnStartSharedSpaceHost?.Invoke();

        // Ensure shared space is started before networking
        StartSharedSpace();
    }

    private void JoinGameClient()
    {
        Debug.Log("Joining existing room as client...");
        isHost = false;
        OnJoinSharedSpaceClient?.Invoke();

        // Ensure shared space is started before networking
        StartSharedSpace();
    }
}
