using System;
using System.Collections.Generic;
using Dafda.Tests.Builders;
using System.Text.Json;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestJsonIncomingMessageFactory
    {
        private const string MessageJson = "{\"messageId\":\"message-id\",\"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"vehicleId\":\"vehicle-id\",\"timeStamp\":\"2019-09-16T07:59:01Z\",\"position\":{\"latitude\":1,\"longitude\":2}}}";

        [Fact]
        public void Can_access_message_headers()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();

            var message = sut.Create(MessageJson);

            Assert.Equal("message-id", message.Metadata.MessageId);
            Assert.Equal("vehicle_position_changed", message.Metadata.Type);
        }
        
        private const string MessageJsonWithNonStringMetadata = "{\"messageId\":12345, \"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"vehicleId\":\"vehicle-id\",\"timeStamp\":\"2019-09-16T07:59:01Z\",\"position\":{\"latitude\":1,\"longitude\":2}}}";

        [Fact]
        public void Can_read_message_headers_non_string()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();

            var message = sut.Create(MessageJsonWithNonStringMetadata);
            
            Assert.Equal("12345", message.Metadata.MessageId);
        }

        [Fact]
        public void Can_decode_data_body()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();

            var message = sut.Create(MessageJson);
            var data = (VehiclePositionChanged) message.ReadDataAs(typeof(VehiclePositionChanged));

            Assert.Equal("aggregate-id", data.AggregateId);
            Assert.Equal("vehicle-id", data.VehicleId);
            Assert.Equal(new DateTime(2019, 9, 16, 7, 59, 1, DateTimeKind.Utc), data.TimeStamp);
            Assert.Equal(1, data.Position.Latitude);
            Assert.Equal(2, data.Position.Longitude);
        }

        private const string MessageJsonWithNonCamelCaseFields = "{\"messageId\":12345, \"type\":\"vehicle_position_changed\",\"data\":{\"aggregateId\":\"aggregate-id\",\"VehicleId\":\"vehicle-id\",\"TIMEStamp\":\"2019-09-16T07:59:01Z\",\"PosiTion\":{\"latitude\":1,\"longitude\":2}}}";

        [Fact]
        public void Can_read_message_with_non_camel_case_data_fields()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();

            var message = sut.Create(MessageJsonWithNonCamelCaseFields);
            var data = (VehiclePositionChanged)message.ReadDataAs(typeof(VehiclePositionChanged));

            Assert.Equal("aggregate-id", data.AggregateId);
            Assert.Equal("vehicle-id", data.VehicleId);
            Assert.Equal(new DateTime(2019, 9, 16, 7, 59, 1, DateTimeKind.Utc), data.TimeStamp);
            Assert.Equal(1, data.Position.Latitude);
            Assert.Equal(2, data.Position.Longitude);
        }

        private const string MalformedMessage = "{\"aliceCooper\":\"Your cruel device, your blood like ice\"}";

        [Fact]
        public void Malformed_message_throws_exception()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();
            Assert.Throws<KeyNotFoundException>(() => sut.Create(MalformedMessage));
        }

        private const string InvalidMessage = "{This is not json at all}";

        [Fact]
        public void InvalidMessage_message_throws_exception()
        {
            var sut = new JsonIncomingMessageFactoryBuilder().Build();
            Assert.ThrowsAny<JsonException>(() => sut.Create(InvalidMessage));
        }

        public record VehiclePositionChanged(string AggregateId, string VehicleId, DateTime TimeStamp, Position Position);
        public record Position(double Latitude, double Longitude);
    }
}
