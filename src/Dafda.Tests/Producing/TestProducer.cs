using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Diagnostics;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Tests.Helpers;
using Dafda.Tests.TestDoubles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Xunit;

namespace Dafda.Tests.Producing
{
    [Collection("Serializing")]
    public class TestProducer
    {
        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(new Message { Id = "dummyId" });

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("dummyId", spy.Key);
        }

        public class Message
        {
            public string Id { get; set; }
        }

        [Fact]
        public async Task produces_message_to_expected_topic()
        {
            var spy = new KafkaProducerSpy();

            var expectedTopic = "foo";

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>(expectedTopic, "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "dummyId" },
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(expectedTopic, spy.Topic);
        }

        [Fact]
        public async Task produces_message_with_expected_partition_key()
        {
            var spy = new KafkaProducerSpy();

            var expectedKey = "foo-partition-key";

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = expectedKey },
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(expectedKey, spy.Key);
        }

        [Fact]
        public async Task produces_message_with_expected_serialized_value()
        {
            var expectedValue = "foo-value";

            var spy = new KafkaProducerSpy(new PayloadSerializerStub(expectedValue));

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "dummyId" },
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(expectedValue, spy.Value);
        }

        [Fact]
        public async Task produces_expected_message_without_headers_using_default_serializer()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(new MessageIdGeneratorStub(() => "1"))
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "dummyId" },
                headers: new Dictionary<string, object>
                {
                }
            );

            var expected = @"{
                                ""messageId"":""1"",
                                ""type"":""bar"",
                                ""causationId"":""1"",
                                ""correlationId"":""1"",
                                ""data"":{
                                    ""id"":""dummyId""
                                    }
                                }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_expected_message_with_headers_using_default_serializer()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(new MessageIdGeneratorStub(() => "1"))
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "dummyId" },
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            var expected = @"{
                                ""messageId"":""1"",
                                ""type"":""bar"",
                                ""foo-key"":""foo-value"",
                                ""causationId"":""1"",
                                ""correlationId"":""1"",
                                ""data"":{
                                    ""id"":""dummyId""
                                    }
                                }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_message_using_metadata()
        {
            var spy = new KafkaProducerSpy();

            var expectedKey = "foo-partition-key";

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = expectedKey },
                headers: new Metadata
                {
                    CausationId = "my-causation",
                    CorrelationId = "my-correlation"
                }
            );

            Assert.Equal(expectedKey, spy.Key);
        }

        [Fact]
        public async Task produces_message_using_message_handler_context()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(new MessageIdGeneratorStub(() => "1"))
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "0" },
                context: new MessageHandlerContext(new Metadata
                {
                    CausationId = "my-causation",
                    CorrelationId = "my-correlation"
                })
            );


            var expected = @"
                            {
                            ""messageId"":""1"",
                            ""type"":""bar"",
                            ""correlationId"":""my-correlation"",
                            ""causationId"":""1"",
                            ""data"":{
                                ""id"":""0""
                                }
                            }";

            AssertJson.Equal(expected, spy.Value);
        }

        //[Fact(Skip = "This test has a static dependency that might break other tests")]
        [Fact]
        public async Task produces_message_with_traceparent_and_baggage_propagation_in_header()
        {
            DafdaActivitySource.Propagator = new CompositeTextMapPropagator(
                new TextMapPropagator[]
                {
                    new TraceContextPropagator(),
                    new BaggagePropagator()
                });

            var activitySource = new ActivitySource("ActivitySourceName");
            var activityListener = new ActivityListener
            {
                ShouldListenTo = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(activityListener);

            using var activity = activitySource.StartActivity("MethodType:/Path");
            Baggage.SetBaggage("som", "der");

            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(new MessageIdGeneratorStub(() => "1"))
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            await sut.Produce(
                message: new Message { Id = "dummyId" },
                headers: new Dictionary<string, object>
                {
                }
            );

            var expected = $@"{{
                                ""messageId"":""1"",
                                ""type"":""bar"",
                                ""causationId"":""1"",
                                ""correlationId"":""1"",
                                ""traceparent"":""{spy.ProducerActivityId}"",
                                ""baggage"":""som=der"",
                                ""data"":{{
                                    ""id"":""dummyId""
                                    }}
                                }}";

            AssertJson.Equal(expected, spy.Value);
        }
    }
}