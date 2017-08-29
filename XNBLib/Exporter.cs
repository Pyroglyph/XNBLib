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
        private GraphicsDeviceManager _graphics;
        private static ContentManager _contentManager;

        private string[] _files;
        private string _outPath;

        public Exporter(string[] files, string outPath = "")
        {
            _graphics = new GraphicsDeviceManager(this);
            _files = files;
            if ((outPath.Length > 0) && !outPath.EndsWith("\\"))
                outPath += "\\";
            _outPath = outPath;
        }

        protected override void LoadContent()
        {
            _contentManager = Content;
            for (var i = 0; i < _files.Length; i++)
                if (File.Exists(_files[i]))
                    using (var inStream = File.OpenRead(_files[i]))
                        if (Encoding.UTF8.GetString(inStream.ReadBytes(0, 3)) == "XNB")
                        {
                            var file = _files[i].Substring(0, (_files[i].Length - 4));
                            if (Encoding.UTF8.GetString(inStream.ReadBytes(12, 49)) == "Microsoft.Xna.Framework.Content.SoundEffectReader")
                            {
                                var result = SoundExporter.ConvertToWav(file, inStream, ((_outPath == "") ? Path.ChangeExtension(_files[i], ".wav") : (_outPath + Path.GetFileNameWithoutExtension(file) + ".wav")));
                                continue;
                            }
                            var obj = _contentManager.Load<Object>(file);
                            var type = obj.GetType();
                            if (type == typeof(Texture2D))
                            {
                                var content = ((Texture2D)obj);
                                using (var outStream = File.Open(((_outPath == "") ? Path.ChangeExtension(_files[i], ".png") : (_outPath + Path.GetFileNameWithoutExtension(file) + ".png")), FileMode.Create))
                                    content.SaveAsPng(outStream, content.Width, content.Height);
                            }
                            else if (type == typeof(SpriteFont))
                            {
                                var content = ((SpriteFont)obj);
                                using (var outStream = File.Open(((_outPath == "") ? Path.ChangeExtension(_files[i], ".png") : (_outPath + Path.GetFileNameWithoutExtension(file) + ".png")), FileMode.Create))
                                    content.Texture.SaveAsPng(outStream, content.Texture.Width, content.Texture.Height);
                            }
                        }
        }
    }

    public static class Extensions
    {
        public static byte[] ReadBytes(this FileStream stream, int index, int length = 0)
        {
            stream.Seek(index, SeekOrigin.Begin);
            var bytes = new byte[(length == 0) ? (stream.Length - index) : Math.Min((stream.Length - index), length)];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)stream.ReadByte();
            return bytes;
        }
    }
}