# Face API UWP Demo

Some of Azure Cognitive Services are now available as container!! Check out the [Blog:Bringing AI to the edge](https://azure.microsoft.com/en-us/blog/bringing-ai-to-the-edge/)

This is an UWP sample for Face API. To install the Face API conteiner, check out [Install and run containers](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-how-to-install-containers)

# How to use the app

1. Get Face API key.
2. git clone or download the solution.
3. Open Services/Settings.cs and enter the key to FaceKey.
4. Run the app. Give camera and mic permissions.

# Use this for cloud version

This sample works with both container and cloud version. To use it with cloud, simply swap the FaceServiceClient in ViewModels/MainPageViewModel.cs file.

# Prerequisites

- Windows 10
- Face API key
- Docker

Check [This arcile](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-how-to-install-containers) for minimum hardware requirements

# LICENSE
MIT
