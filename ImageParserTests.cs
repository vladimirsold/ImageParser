using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System;

namespace ImageParser
{
    [TestFixture]
    public class ImageParserTests
    {
        class TestImageInfo : IImageInfo
        {
            public int Height { get; set; }

            public int Width { get; set; }

            public string Format { get; set;}

            public long Size { get; set; }
        }
        private ImageParser parser;
        private string jsonBmp;
        private string jsonPng;
        private string jsonGif;
        [SetUp]
        public void SetUp()
        {
            parser = new ImageParser();
            jsonBmp = JsonConvert.SerializeObject(new TestImageInfo { Height = 292, Width = 640, Format = "Bmp", Size = 560694 });
            jsonPng = JsonConvert.SerializeObject(new TestImageInfo { Height = 292, Width = 640, Format = "Png", Size = 229983 });
            jsonGif = JsonConvert.SerializeObject(new TestImageInfo { Height = 292, Width = 640, Format = "Gif", Size = 70291 });
        }

        [Test]
        public void BmpImageTest()
        {

            using(var file = new FileStream(AppDomain.CurrentDomain.BaseDirectory+"image.bmp", FileMode.Open, FileAccess.Read))
            {
                Assert.AreEqual(jsonBmp, parser.GetImageInfo(file));
            }
            
        }

        [Test]
        public void PngImageTest()
        {

            using(var file = new FileStream(AppDomain.CurrentDomain.BaseDirectory +"image.png", FileMode.Open, FileAccess.Read))
            {
                Assert.AreEqual(jsonPng, parser.GetImageInfo(file));
            }

        }

        [Test]
        public void GifImageTest()
        {

            using(var file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "image.gif", FileMode.Open, FileAccess.Read))
            {
                Assert.AreEqual(jsonGif, parser.GetImageInfo(file));
            }

        }
    }
}