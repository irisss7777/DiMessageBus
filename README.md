# MessagePipe Zenject Integration

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity3d.com)
[![MessagePipe](https://img.shields.io/badge/MessagePipe-1.7%2B-green.svg)](https://github.com/Cysharp/MessagePipe)
[![Zenject](https://img.shields.io/badge/Zenject-9.2%2B-orange.svg)](https://github.com/modesttree/Zenject)

A lightweight, high-performance integration layer between [MessagePipe](https://github.com/Cysharp/MessagePipe) and [Zenject](https://github.com/modesttree/Zenject) for Unity projects. Simplifies the pub/sub pattern with automatic dependency resolution, caching, and lifecycle management.

## ✨ Features

- **Automatic Caching** - Publishers and subscribers are cached by type for optimal performance
- **Zenject Integration** - Seamlessly works with Zenject's dependency injection
- **Lifecycle Management** - Subscriptions automatically disposed with Zenject's `IMessageDisposable`
- **Thread-Safe** - Uses `ConcurrentDictionary` for safe access from multiple threads
- **Lightweight** - Minimal overhead on top of MessagePipe
- **Easy to Use** - Simple API that reduces boilerplate code

## 📦 Installation

### Prerequisites

First, install the required dependencies:

1. **[MessagePipe](https://github.com/Cysharp/MessagePipe)** - via Unity Package Manager
2. **[Zenject](https://github.com/modesttree/Zenject)** (Extenject) - via Unity Package Manager

### Method 1: Unity Package Install

**[DiMessageBus](https://github.com/irisss7777/DiMessageBus/releases/tag/DiMessageBus)** - UnityPackage

### Method 2: Unity Package Manager (Git URL)

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cysharp.messagepipe": "1.7.0",
    "com.modesttree.zenject": "9.2.0",
    "com.irisss.dimessagebus": "https://github.com/irisss7777/DiMessageBus.git"
  }
}
```

## 🚀 Complete Usage Example

Step 1: Define Your Message Types
```
// Signal for creating a lobby
public struct CreateLobbySignal
{

}

// Signal for starting the game
public struct StartGameSignal
{

}

// DTO for connected lobby information
public struct ConnectedLobbyDto
{
    public string LobbyId;
    public string LobbyName;
    public int PlayerCount;
}
```

Step 2: Create an Installer to Bind MessageBus
```
using MessagePipe;
using Zenject;

public class MessageInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Bind MessagePipe to the container
        var options = Container.BindMessagePipe();

        // Bind MessageBus as single instance (accessible everywhere)
        Container.Bind<MessageBus>().AsSingle();

        // Register message brokers for all your message types
        Container.RegisterMessageBroker<CreateLobbySignal>(options);
        Container.RegisterMessageBroker<StartGameSignal>(options);
        Container.RegisterMessageBroker<ConnectedLobbyDto>(options);
        
        // Optional: Register additional message types in separate methods
        InputBind(options);
        WebBind(options);
    }

    private void InputBind(MessagePipeOptions options)
    {
        // Register input-related messages
        Container.RegisterMessageBroker<PlayerInputSignal>(options);
        Container.RegisterMessageBroker<UISignal>(options);
    }

    private void WebBind(MessagePipeOptions options)
    {
        // Register web-related messages
        Container.RegisterMessageBroker<NetworkConnectedSignal>(options);
        Container.RegisterMessageBroker<NetworkErrorSignal>(options);
    }
}
```

Step 3: Create a Service that Subscribes to Messages
```
using System;
using System.Threading;
using MessagePipe;
using UnityEngine;
using Zenject;

public class WebLobbyService : IInitializable, IMessageDisposable
{
    [Inject] private readonly MessageBus _messageBus;
    [Inject] private readonly WebServiceHandler _webServiceHandler;
    
    private CancellationTokenSource _cancellationTokenSource;
    private LobbyService.LobbyServiceClient _lobbyService;
    private AccessToken _accessToken;

    // Required for automatic subscription cleanup
    public event Action OnDispose;

    public void Initialize()
    {
        // Subscribe to multiple message types with automatic disposal
        _messageBus.Subscribe<CreateLobbySignal>((CreateLobbySignal signal) => OnCreateLobby(), this);
        _messageBus.Subscribe<StartGameSignal>((StartGameSignal signal) => OnStartGame(), this);

        SetupService();
    }

    private void OnCreateLobby()
    {
        Debug.Log($"Creating lobby: {signal.LobbyName} for {signal.MaxPlayers} players");
        CreateLobby(signal).Forget(); // Using UniTask's Forget() for fire-and-forget
    }

    private void OnStartGame()
    {
        Debug.Log($"Starting game with mode: {signal.GameMode}");
        StartGame(signal).Forget();
    }

    private async UniTaskVoid CreateLobby()
    {
        // Async lobby creation logic
        var lobby = await _lobbyService.CreateLobbyAsync();
        _messageBus.Publish(new ConnectedLobbyDto
        {
            LobbyId = lobby.Id,
            LobbyName = lobby.Name,
            PlayerCount = lobby.PlayerCount
        });
    }

    private async UniTaskVoid StartGame()
    {
        // Async game start logic
        await _lobbyService.StartGameAsync();
    }

    private void SetupService()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        // Additional setup logic
    }

    // Cleanup when service is disposed (called by Zenject)
    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        OnDispose?.Invoke(); // This will automatically unsubscribe all message subscriptions
    }
}
```

Step 4: Publish Messages from Anywhere
```
using Zenject;

public class LobbyController : MonoBehaviour
{
    [Inject] private MessageBus _messageBus;
    
    public void OnCreateLobbyButtonClicked()
    {
        // Publish a message when UI button is clicked
        _messageBus.Publish(new CreateLobbySignal
        {
            LobbyName = "My Awesome Lobby",
            MaxPlayers = 4
        });
    }

    public void OnGameStartButtonClicked()
    {
        // Publish a message to start the game
        _messageBus.Publish(new StartGameSignal
        {
            GameMode = "Deathmatch"
        });
    }
}

public class NetworkManager : MonoBehaviour
{
    [Inject] private MessageBus _messageBus;
    
    private void OnLobbyConnected(LobbyInfo lobbyInfo)
    {
        // Publish a DTO when network event occurs
        _messageBus.Publish(new ConnectedLobbyDto
        {
            LobbyId = lobbyInfo.Id,
            LobbyName = lobbyInfo.Name,
            PlayerCount = lobbyInfo.Players.Count
        });
    }
}
```

Step 5: Simple Component Example (MonoBehaviour)
```
using UnityEngine;
using Zenject;

public class SimpleMessageHandler : MonoBehaviour, IMessageDisposable
{
    [Inject] private MessageBus _messageBus;
    
    public event Action OnDispose;
    
    private void Start()
    {
        // Subscribe to messages
        _messageBus.Subscribe<ConnectedLobbyDto>((ConnectedLobbyDto message) => OnLobbyConnected(message), this);
    }
    
    private void OnLobbyConnected(ConnectedLobbyDto dto)
    {
        Debug.Log($"Connected to lobby: {dto.LobbyName} with {dto.PlayerCount} players");
        UpdateUI(dto);
    }
    
    private void UpdateUI(ConnectedLobbyDto dto)
    {
        // Update your UI here
    }
    
    private void OnDestroy()
    {
        // Automatically unsubscribes all message subscriptions
        OnDispose?.Invoke();
    }
}
```
