using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace XNBLib
{
    public class Exporter : Game
    {
        public delegate void StatusUpdated(string status);
        public event StatusUpdated OnStatusUpdate;
        public delegate void Completed();
        public event Completed OnCompleted;
        
        private GraphicsDeviceManager _graphics;
        private static ContentManager _contentManager;

        private readonly string[] _files;
        private readonly string _outPath;

        public Exporter(string[] files, string outPath = "")
        {
            _graphics = new GraphicsDeviceManager(this);
            _files = files;
            if (outPath.Length > 0)
            {
                if (!outPath.EndsWith("\\")) outPath += "\\";
                _outPath = outPath;
            }
        }

        protected override void LoadContent()
        {
            _contentManager = Content;
            
            // Iterate through all the files we were given
            for (var i = 0; i < _files.Length; i++)
            {
                // If the file turns out to be nonexistent, skip it
                if (!File.Exists(_files[i])) continue;

                // Open the file for reading
                using (var inStream = File.OpenRead(_files[i]))
                {
                    // Check if the file has an XNB header
                    if (Encoding.UTF8.GetString(inStream.ReadBytes(0, 3)) == "XNB")
                    {
                        UpdateStatus("Validated " + Path.GetFileNameWithoutExtension(_files[i]));

                        var file = _files[i].Substring(0, _files[i].Length - 4);

                        // If the file is a sound, the process is slightly different as we don't need ContentManager
                        if (Encoding.UTF8.GetString(inStream.ReadBytes(12, 49)) ==
                            "Microsoft.Xna.Framework.Content.SoundEffectReader")
                        {
                            // Convert the file to a WAV
                            var result = SoundExporter.ConvertToWav(file, inStream,
                                _outPath == ""
                                    ? Path.ChangeExtension(_files[i], ".wav")
                                    : _outPath + Path.GetFileNameWithoutExtension(_files[i]) + ".wav");

                            // Report back to the user if it was successful or not
                            if (result.Type == SoundExporter.Result.Types.Success)
                            {
                                UpdateStatus($"[SUCCESS] Exported {Path.GetFileNameWithoutExtension(_files[i])}.wav");
                            }
                            else
                            {
                                UpdateStatus("Audio Error >> " + result);
                                UpdateStatus("Please report this to gameking008 because I have no idea how to fix it.");
                                Environment.Exit(1);
                            }
                            continue;
                        }

                        // If the file is not a sound, we will need ContentManager
                        var obj = _contentManager.Load<object>(file);
                        var type = obj.GetType();
                        if (type == typeof(Texture2D))
                        {
                            // Export image
                            var content = (Texture2D) obj;
                            using (var outStream =
                                 File.Open(
                                     _outPath == ""
                                         ? Path.ChangeExtension(_files[i], ".png")
                                         : _outPath + Path.GetFileNameWithoutExtension(_files[i]) + ".png",
                                     FileMode.Create))
                                content.SaveAsPng(outStream, content.Width, content.Height);

                            UpdateStatus($"[SUCCESS] Exported {Path.GetFileNameWithoutExtension(_files[i])}.png");
                        }
                        else if (type == typeof(SpriteFont))
                        {
                            // Export font atlas
                            var content = (SpriteFont) obj;
                            using (var outStream =
                                File.Open(
                                    _outPath == ""
                                        ? Path.ChangeExtension(_files[i], ".png")
                                        : _outPath + Path.GetFileNameWithoutExtension(_files[i]) + ".png",
                                    FileMode.Create))
                                content.Texture.SaveAsPng(outStream, content.Texture.Width, content.Texture.Height);

                            UpdateStatus($"[SUCCESS] Exported {Path.GetFileNameWithoutExtension(_files[i])}.png");
                        }
                    }
                    else
                    {
                        // If the file does not have an XNB header, warn the user and skip it.
                        UpdateStatus("[WARNING] >> Skipping '" + Path.GetFileName(_files[i]) +
                                     "' as it does not have a valid header. It may be corrupt.");
                    }
                }
            }

            UpdateStatus("\nAll done!");

            // We're done!
            OnCompleted?.Invoke();
        }

        private void UpdateStatus(string message)
        {
            // This makes sure the console output goes through regardless
            // of whether the user has subscribed to the event or not.
            if (OnStatusUpdate != null)
                OnStatusUpdate(message);
            else
                Console.WriteLine(message);
        }
    }

    public static class Extensions
    {
        public static byte[] ReadBytes(this FileStream stream, int index, int length = 0)
        {
            stream.Seek(index, SeekOrigin.Begin);
            var bytes = new byte[length == 0 ? stream.Length - index : Math.Min(stream.Length - index, length)];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte) stream.ReadByte();
            return bytes;
        }
    }
}