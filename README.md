<!---
Someone please help me format this better :(
--->

# XNBLib
XNBLib is the library behind XNBE, my XNB decompiler. It allows easy asset decompilation for all the formats that both XNA amd MonoGame support.

## Supported Formats
- Images (output as PNG)
- Audio (output as WAV)
- SpriteFontTexture (output as PNG)

More planned!

## Usage
1. Import the library into your project.
2. Instantiate the Exporter class.
3. Pass a string array of paths to XNB files into `ExportFile()`.
4. (Optional) Also pass in a string that points to an output folder.
4. (Optional) Subscribe to the `OnStatusUpdate` event to get console output as a string.
5. (Optional) Subscribe to the `OnCompleted` event to do something when XNBLib has finished.
6. `Run()` the Exporter and wait for the files to decompile!

## Examples
### 1
```csharp
var exporter = new Exporter(new [] { "C:\\path\\to\\file.xnb" });
exporter.Run();
```
After this, your decompiled files will be placed in the same folder as their XNB counterparts.

### 2
```csharp
var exporter = new Exporter(new [] { "C:\\path\\to\\file.xnb" }, "C:\\output");
exporter.OnStatusUpdate += Exporter_OnStatusUpdate;
exporter.Run();

...

void Exporter_OnStatusUpdate(string status)
{
    Console.WriteLine("[XNBLib] " + status);
	// You can also leave this method blank to mute any console output.
}
```
After this, your decompiled files will be placed in C:/output and [XNBLib] will be appended to the start of any console output.

### Live Example
XNBExporter is a console application I developed to showcase XNBLib. Check it out [here](https://github.com/Pyroglyph/XNBExporter)!

### Acknowledgments
- gameking008 - For the WAVE exporter.
- [Dcrew](https://github.com/DeanReynolds) - For increasing efficiency, fixing bugs, and adding SpriteFontTexture support.