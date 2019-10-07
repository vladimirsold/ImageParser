using Newtonsoft.Json;
using System;
using System.IO;

namespace ImageParser
{
    interface IImageInfo
    {
        int Height { get; }
        int Width { get; }
        string Format { get; }
        long Size { get; }
    }

    public static class ArrayExtension
    {
        public static int GetInt32MSB(this byte[] data, int position)
        {
            var number = new byte[4];
            Array.Copy(data, position, number, 0, 4);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(number);
            }
            return BitConverter.ToInt32(number, 0);
        }
    }

    public class ImageParser : IImageParser
    {

        public string GetImageInfo(Stream stream)
        {
            const int headerBufferSize = 4;
            var headerBuffer = new byte[headerBufferSize];
            stream.Read(headerBuffer, 0, headerBufferSize);
            string header = BitConverter.ToString(headerBuffer);
            IImageInfo imageInfo = GetInfo(stream, header);
            return JsonConvert.SerializeObject(imageInfo);
        }

        private IImageInfo GetInfo(Stream stream, string header)
        {
            if(header.StartsWith("47-49-46"))
            {
                return new GifFileInfo(stream);
            }
            if(header.StartsWith("4D-42") || header.StartsWith("42-4D"))
            {
                return new BmpFileInfo(stream);
            }
            if(header.StartsWith("89-50-4E-47"))
            {
                return new PngFileInfo(stream);
            }
            throw new ArgumentException();
        }
    }

    abstract class ImageFileInfo : IImageInfo
    {
        public int Height { get; protected set; }
        public int Width { get; protected set; }
        public string Format { get; protected set; }
        public long Size { get; }
        public ImageFileInfo(Stream stream)
        {
            Size = stream.Length;
        }
    }

    class PngFileInfo : ImageFileInfo
    {
        const int positionIHDRChunk = 8;
        const int sizeIHDRChunk = 25;
        const int positionHeightInIHDR = 12;
        const int positionWidthInIHDR = 8;
        readonly byte[] IHDRChunk;
        public PngFileInfo(Stream stream) : base(stream)
        {
            Format = "Png";
            IHDRChunk = new byte[sizeIHDRChunk];
            stream.Seek(positionIHDRChunk, SeekOrigin.Begin);
            stream.Read(IHDRChunk, 0, sizeIHDRChunk);
            Height = IHDRChunk.GetInt32MSB(positionHeightInIHDR);
            Width = IHDRChunk.GetInt32MSB(positionWidthInIHDR);
        }
    }

    class BmpFileInfo : ImageFileInfo
    {
        readonly BmpInfo bmpInfo;
        public BmpFileInfo(Stream stream) : base(stream)
        {
            Format = "Bmp";
            bmpInfo = new BmpInfo(stream);
            Width = bmpInfo.GetWidthImage();
            Height = bmpInfo.GetHeightImage();
        }
    }

    class BmpInfo
    {
        const int positionInFileStream = 14;
        public string Version { get; }
        readonly byte[] data;
        readonly int size;

        public BmpInfo(Stream stream)
        {
            var biSize = new byte[4];
            stream.Seek(positionInFileStream, SeekOrigin.Begin);
            stream.Read(biSize, 0, 4);
            size = BitConverter.ToInt32(biSize, 0);
            Version = SetVersion();
            var dataLength = size - biSize.Length;
            data = new byte[dataLength];
            stream.Read(data, 0, dataLength);
        }

        private string SetVersion()
        {
            switch(size)
            {
                case 12:
                    return "CORE";
                case 40:
                    return "3";
                case 108:
                    return "4";
                case 124:
                    return "5";
                default:
                    throw new InvalidDataException();
            }
        }

        public int GetWidthImage()
        {
            if(Version == "CORE")
            {
                return BitConverter.ToInt16(data, 0);
            }
            return BitConverter.ToInt32(data, 0);

        }

        public int GetHeightImage()
        {
            if(Version == "CORE")
            {
                return BitConverter.ToInt16(data, 2);
            }
            return BitConverter.ToInt32(data, 4);
        }
    }

    class GifFileInfo : ImageFileInfo
    {
        const int sizeScreenDescriptor = 7;
        const int positionScreenDescriptor = 6;
        readonly byte[] screenDescriptor;

        public GifFileInfo(Stream stream) : base(stream)
        {
            Format = "Gif";
            screenDescriptor = new byte[sizeScreenDescriptor];
            stream.Seek(positionScreenDescriptor, SeekOrigin.Begin);
            stream.Read(screenDescriptor, 0, sizeScreenDescriptor);
            Width = BitConverter.ToInt16(screenDescriptor, 0);
            Height = BitConverter.ToInt16(screenDescriptor, 2);
        }
    }
}