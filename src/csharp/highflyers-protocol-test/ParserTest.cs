using System.IO;
using HighFlyers.Protocol;
using HighFlyers.Protocol.Frames;
using NUnit.Framework;

namespace HighFlyers.Test.Protocol
{
    [TestFixture]
    public class Parser
    {
        [Test]
        public void CheckNullElement()
        {
            var parser = new Parser<FrameBuilder>();

            parser.AppendBytes(new byte[]
            {
                0, 1 + 4, 0,
                5, 1, 0, 0,
                80,
                47, 73, 45, 44,
                FrameParserHelper.EndFrame
            });

            var frame = parser.LastFrame as TestStruct;
            Assert.AreEqual(256 + 5, frame.Field1);
            Assert.IsNull(frame.Field2);
            Assert.AreEqual(80, frame.Field3);
            Assert.IsNull(frame.Field4);
        }

        [Test]
        [ExpectedException(typeof(InvalidDataException))]
        public void ShouldThrowExceptionBecauseOfBadData()
        {
            var parser = new Parser<FrameBuilder>();
            parser.AppendBytes(new byte[] {FrameParserHelper.Sentinel, 99});
        }

        [Test]
        public void SimpleParsingData()
        {
            var parser = new Parser<FrameBuilder>();

            parser.AppendBytes(new byte[]
            {
                0, 255, 255,
                FrameParserHelper.Sentinel, FrameParserHelper.EndFrame, 1, 0, 0,
                FrameParserHelper.Sentinel, FrameParserHelper.Sentinel, 64, 23, 3, 11, 5, 2, 4,
                2,
                FrameParserHelper.Sentinel, FrameParserHelper.Sentinel, 4, 2, 1,
                114, 84, 5, 19,
                FrameParserHelper.EndFrame
            });

            var frame = parser.LastFrame as TestStruct;
            Assert.AreEqual(256 + FrameParserHelper.EndFrame, frame.Field1);
            Assert.AreEqual(2, frame.Field3);
        }

        [Test]
        public void StructInStruct()
        {
            var parser = new Parser<FrameBuilder>();

            parser.AppendBytes(new byte[]
            {
                1, 1 + 2, 0,
                FrameParserHelper.Sentinel, 1 + 4 + 8, 0,
                5, 1, 0, 0,
                80,
                52, 0, 0, 0,
                98,
                248, 53, 39, 134,
                FrameParserHelper.EndFrame
            });

            var frame = parser.LastFrame as SecondStruct;
            Assert.AreEqual(frame.Field1.Field1, 256 + 5);
            Assert.IsNull(frame.Field1.Field2);
            Assert.AreEqual(frame.Field1.Field3, 80);
            Assert.AreEqual(frame.Field1.Field4, 52);
            Assert.AreEqual(frame.Field2, 98);
            Assert.IsNull(frame.Field3);
        }
    }
}