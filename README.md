# Smart Theatre (Augmented Reality)

This project uses AR to show subtitles when a movie is played .

## Requirements

* Go v1.13
* GRPC
* Unity Engine 2019

### Setup

* Go to `Go Server` folder and run `go run main.go`
* It will be listening for the client to get connected.
* Use the same address provided by the server in client.
* Change it in both `Subtitle.cs` and `Controller.cs` files inside the `Assets/Scripts` folder
* Open the `C# client`folder in `Unity Engine 2019`.
* Enable `AR Core Support` in `Player settings` while building the project for android.
* Install and run the APK in any android phone with `ARCore` installed.
