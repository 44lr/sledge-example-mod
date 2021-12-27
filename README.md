# Sledge example mod
This is an example of what a Sledge mod would look like.
You can download Sledge here: https://github.com/44lr/sledge

## Build requirements:
* [Sledge](https://github.com/44lr/sledge)
* [Visual Studio](https://visualstudio.microsoft.com/)
* C# SDK (.NET 6)

## How to build
1. Clone the repository
2. Open the solution
3. On the Solution Explorer, right click Dependencies, and click "Add Project Reference", a window will open
4. On that window, click "Browse", locate sledgelib.dll (located in the mods folder of Sledge), add it and click okay
5. Build the project
6. Create a file named sledge_examplemod.info.json (or whatever you named the dll).
7. That file must contain the following text: ```{
	"sTypeName": "examplemod",
	"sMethodName": "Init"
}```
8. Put sledge_examplemod.info.json and sledge_examplemod.dll into the mods directory of your Sledge installation.
