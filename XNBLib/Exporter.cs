using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace XNBLib
{
    public class Exporter : Game
    {
        public delegate void StatusUpdated(string status);
        public event StatusUpdated OnStatusUpdate;
        public delegate void Completed();
        public event Completed OnCompleted;

        private GraphicsDeviceManager _graphics;
        private static ResourceType _type;
        private readonly string[] _files;
        private readonly string _outPath;

        private enum ResourceType
        {
            Audio,
            Image
        }

        /// <summary>
        /// Decompiles XNB files.
        /// </summary>
        /// <param name="files">A string array of paths to XNB files.</param>
        /// <param name="outPath">The output directory where the decompiled files should go.</param>
        public Exporter(string[] files, string outPath = "")
        {
            _graphics = new GraphicsDeviceManager(this);
            _files = files;
            _outPath = outPath;
        }
        
        protected override void LoadContent()
        {
            for (var i = 0; i < _files.Length; i++)
                ExportFile(_files[i], _outPath);

            UpdateStatus("\nAll done!");

            // We're done!
            OnCompleted?.Invoke();
            Exit();
        }

        private void ExportFile(string file, string outPath)
        {
            if (!File.Exists(file)) return;


            if (outPath == "") outPath = Path.GetDirectoryName(file);

            // Validate the file before we try and parse it just in case.
            ValidateFile(file);

            switch (_type)
            {
                case ResourceType.Audio:
                    // Give the audio file to gameking008's SoundExporter so it can work it's wizardry.
                    var result = SoundExporter.ConvertToWav(file, outPath);
                    if (result.Type == SoundExporter.Result.Types.Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        UpdateStatus("Success! Exported " + Path.GetFileNameWithoutExtension(file) + ".wav");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        UpdateStatus("Audio Error >> " + result);
                        UpdateStatus("Please report this to gameking008 because I have no idea how to fix it.");
                        Environment.Exit(1);
                    }
                    break;
                case ResourceType.Image:
                    using (Stream stream = File.Open(outPath + Path.GetFileNameWithoutExtension(file) + ".png", FileMode.Create))
                    {
                        // We pass Content.Load() the texture without the .xnb extension since it is added automatically.
                        var texture = Content.Load<Texture2D>(file.Replace(".xnb", ""));
                        texture.SaveAsPng(stream, texture.Width, texture.Height);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        UpdateStatus("Success! Exported " + Path.GetFileNameWithoutExtension(file) + ".png");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("_type", _type, "");
            }
        }

        private void ValidateFile(string file)
        {
            UpdateStatus("Validating " + Path.GetFileNameWithoutExtension(file) + "...");

            // Read the given file.
            var bytes = File.ReadAllBytes(file);
            var s = string.Empty;

            // Get first 3 bytes
            for (var b = 0; b < 3; b++) s += Convert.ToChar(bytes[b]);
            if (s != "XNB")
            {
                // XNB header is missing. We can't parse this file. Exit.
                UpdateStatus(
                    "Input Error >> File did not pass validation. This does not appear to be an XNB file.");
                UpdateStatus("\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            else
            {
                // This looks like an XNB header. Now we determine it's type.
                // Convert the first 64 bytes into a string.
                for (var b = 0; b < 64; b++) s += Convert.ToChar(bytes[b]);
                // If said string contains the word sound (all XNB sounds do), then set the type accordingly.
                _type = s.Contains("Sound") ? ResourceType.Audio : ResourceType.Image;
                // Display the type to the user.
                UpdateStatus("Validated: " + Enum.GetName(typeof(ResourceType), _type));
            }
        }

        private void UpdateStatus(string message)
        {
            if (OnStatusUpdate != null)
                OnStatusUpdate(message);
            else
                Console.WriteLine(message);
        }
    }
}
