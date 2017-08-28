<!---
Someone please help me format this better :(
--->

# XNBLib (xnbl)
> XNBL can be pronounced "zen-bull"

XNBLib is the library behind XNBE, my XNB decompiler. It allows easy asset decompilation for all the formats that both XNA amd MonoGame support.

## Supported Formats
- Images (output as PNG)
- Audio (output as WAV)

## Usage
1. Import the library into your project.
2. Instantiate the Exporter class.
3. Pass a string array of paths to XNB files into `ExportFile()`.
4. (Optional) Subscribe to the `OnStatusUpdate` event to get console output as a string.
5. `Run()` the Exporter and wait for the files to decompile!

## Basic Example
```csharp
var exporter = new Exporter(new [] { "C:\\path\\to\\file.xnb" });
exporter.Run();
```
After running this, your decompiled files will be placed in the same folder as their XNB counterparts.