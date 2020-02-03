using System;
using Dafda.Consuming;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestJsonMessageEmbeddedDocument
    {
        private const string MessageJson = "{\"messageId\":\"1\",\"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"1\",\"messageId\":\"1\",\"vehicleId\":\"1\",\"timeStamp\":\"2019-09-16T07:59:01Z\",\"position\":{\"latitude\":1,\"longitude\":2}}}";

        [Fact]
        public void Can_access_message_headers()
        {
            var sut = new JsonMessageEmbeddedDocument(MessageJson);

            Assert.Equal("1", sut.MessageId);
            Assert.Equal("vehicle_position_changed", sut.Type);
        }

        [Fact]
        public void Can_decode_data_body()
        {
            var sut = new JsonMessageEmbeddedDocument(MessageJson);

            var data = sut.ReadDataAs<VehiclePositionChanged>();

            Assert.Equal("1", data.AggregateId);
            Assert.Equal("1", data.MessageId);
            Assert.Equal("1", data.VehicleId);
            Assert.Equal(new DateTime(2019, 9, 16, 7, 59, 1, DateTimeKind.Utc), data.TimeStamp);
            Assert.Equal(1, data.Position.Latitude);
            Assert.Equal(2, data.Position.Longitude);
        }

        public class VehiclePositionChanged
        {
            public string AggregateId { get; set; }
            public string MessageId { get; set; }
            public string VehicleId { get; set; }
            public DateTime TimeStamp { get; set; }
            public Position Position { get; set; }
        }

        public class Position
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}