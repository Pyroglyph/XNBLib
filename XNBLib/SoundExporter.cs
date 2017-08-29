// Note: This file was originally written by gameking008.
// Integrated into XNB Exporter by Pyroglyph.

using System;
using System.IO;

namespace XNBLib
{
    public class SoundExporter
    {
        public static Result ConvertToWav(string inFile, FileStream inStream, string outFile = "")
        {
            if (outFile == "")
                outFile = Path.ChangeExtension(inFile, ".wav");
            ushort wFormatTag;
            ushort nChannels;
            uint nSamplesPerSec;
            uint nAvgBytesPerSec;
            ushort nBlockAlign;
            ushort wBitsPerSample;
            int dataChunkSize;
            byte[] waveData;
            inStream.Position = 0;
            using (var br = new BinaryReader(inStream))
            {
                var format = new string(br.ReadChars(3));
                if (format != "XNB")
                    return new Result(Result.Types.InvalidFileFormat, format);
                var platform = br.ReadChar();
                if (platform != 'w')
                    return new Result(Result.Types.InvalidPlatform, platform.ToString());
                int xnaVersion = br.ReadByte();
                if (xnaVersion != 5)
                    return new Result(Result.Types.UninplementedXNAVersion, xnaVersion.ToString());
                var profile = br.ReadByte();
                if (profile != 0)
                    return new Result(Result.Types.UnimplementedProfile, profile.ToString());
                var fileLength = br.ReadUInt32();
                if (fileLength != inStream.Length)
                    return new Result(Result.Types.FileLengthMismatch, (fileLength + " - should be " + inStream.Length));
                var typeCount = (uint)Read7BitEncodedInt(br);
                if (typeCount != 1)
                    return new Result(Result.Types.TooManyTypes, typeCount.ToString());
                // Should be in a for loop but I'm too lazy to add support for multiple types.
                var type = br.ReadString();
                if (type != "Microsoft.Xna.Framework.Content.SoundEffectReader")
                    return new Result(Result.Types.WrongTypeReaderName, type);
                var typeReaderVersion = br.ReadInt32();
                if (typeReaderVersion != 0)
                    return new Result(Result.Types.WrongTypeReaderVersion, typeReaderVersion.ToString());
                var sharedResourcesCount = (uint)Read7BitEncodedInt(br);
                if (sharedResourcesCount != 0)
                    return new Result(Result.Types.TooManySharedResources, sharedResourcesCount.ToString());
                if (Read7BitEncodedInt(br) != 1)
                    return new Result(Result.Types.UnknownError);
                // WAVE format
                var formatChunkSize = br.ReadUInt32();
                if (formatChunkSize != 18)
                    return new Result(Result.Types.WrongFormatChunkSize, formatChunkSize.ToString());
                if ((wFormatTag = br.ReadUInt16()) != 1)
                    return new Result(Result.Types.UnimplementedWAVCodec, "Must be PCM");
                nChannels = br.ReadUInt16();
                nSamplesPerSec = br.ReadUInt32();
                nAvgBytesPerSec = br.ReadUInt32();
                nBlockAlign = br.ReadUInt16();
                wBitsPerSample = br.ReadUInt16();
                if (nAvgBytesPerSec != (nSamplesPerSec * nChannels * (wBitsPerSample / 8)))
                    return new Result(Result.Types.InvalidAvergateBytesPerSec);
                if (nBlockAlign != (nChannels * (wBitsPerSample / 8)))
                    return new Result(Result.Types.InvalidBlockAlign);
                br.ReadUInt16();
                waveData = br.ReadBytes(dataChunkSize = br.ReadInt32());
            }
            using (var fs = File.Create(outFile))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write("RIFF".ToCharArray());
                bw.Write(dataChunkSize + 36);
                bw.Write("WAVE".ToCharArray());
                bw.Write("fmt ".ToCharArray());
                bw.Write(16);
                bw.Write(wFormatTag);
                bw.Write(nChannels);
                bw.Write(nSamplesPerSec);
                bw.Write(nAvgBytesPerSec);
                bw.Write(nBlockAlign);
                bw.Write(wBitsPerSample);
                bw.Write("data".ToCharArray());
                bw.Write(dataChunkSize);
                bw.Write(waveData);
            }
            return new Result(Result.Types.Success);
        }

        static int Read7BitEncodedInt(BinaryReader br)
        {
            var num = 0;
            var num2 = 0;
            while (num2 != 35)
            {
                var b = br.ReadByte();
                num |= (b & 127) << num2;
                num2 += 7;
                if ((b & 128) == 0)
                    return num;
            }
            throw new FormatException("Failed to read a Microsoft 7-bit encoded integer");
        }

        public class Result
        {
            public readonly Types Type;

            public enum Types
            {
                Success,
                UnknownError,
                InvalidFileFormat,
                InvalidPlatform,
                UninplementedXNAVersion,
                UnimplementedProfile,
                FileLengthMismatch,
                TooManyTypes,
                WrongTypeReaderName,
                WrongTypeReaderVersion,
                TooManySharedResources,
                WrongFormatChunkSize,
                UnimplementedWAVCodec,
                InvalidAvergateBytesPerSec,
                InvalidBlockAlign
            }

            public readonly string Details;

            public Result(Types type, string details = "")
            {
                Type = type;
                Details = details;
            }
        }
    }
}