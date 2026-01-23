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

### Method 1: Unity Package Manager (Git URL)

Add the following to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cysharp.messagepipe": "1.7.0",
    "com.modesttree.zenject": "9.2.0",
    "com.irisss.dimessagebus": "https://github.com/irisss7777/DiMessageBus.git"
  }
}
