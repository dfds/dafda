using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class TestProducer : IClassFixture<DafdaActivitySourceFixture>
    {
        private readonly DafdaActivitySourceFixture _fixture;

        public TestProducer(DafdaActivitySourceFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetDafdaActivitySource();
        }

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
                headers: new Metadata ()
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
                context: new MessageHandlerContext(new Metadata())
            );


            var expected = @"
                            {
                            ""messageId"":""1"",
                            ""type"":""bar"",
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
                                ""traceparent"":""{spy.ProducerActivityId}"",
                                ""baggage"":""som=der"",
                                ""data"":{{
                                    ""id"":""dummyId""
                                    }}
                                }}";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task Can_produce_multiple_messages()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "id1" },
                new Message { Id = "id2" },
                new Message { Id = "id3" }
            };

            await sut.Produce(messages);

            Assert.Equal(3, spy.ProduceCallCount);
            Assert.Equal("foo", spy.Topic);
        }

        [Fact]
        public async Task produces_multiple_messages_to_expected_topic()
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

            var messages = new[]
            {
                new Message { Id = "id1" },
                new Message { Id = "id2" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(expectedTopic, spy.Topic);
            Assert.Equal(2, spy.ProduceCallCount);
        }

        [Fact]
        public async Task produces_multiple_messages_with_expected_partition_keys()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "key1" },
                new Message { Id = "key2" },
                new Message { Id = "key3" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(3, spy.ProduceCallCount);
            Assert.Contains("key1", spy.AllKeys);
            Assert.Contains("key2", spy.AllKeys);
            Assert.Contains("key3", spy.AllKeys);
        }

        [Fact]
        public async Task produces_multiple_messages_with_expected_serialized_values()
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

            var messages = new[]
            {
                new Message { Id = "id1" },
                new Message { Id = "id2" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(expectedValue, spy.Value);
            Assert.Equal(2, spy.ProduceCallCount);
        }

        [Fact]
        public async Task produces_expected_multiple_messages_without_headers_using_default_serializer()
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

            var messages = new[]
            {
                new Message { Id = "dummyId1" },
                new Message { Id = "dummyId2" }
            };

            await sut.Produce(
                messages: messages,
                headers: []
            );

            Assert.Equal(2, spy.ProduceCallCount);
            var expected = @"{
                                ""messageId"":""1"",
                                ""type"":""bar"",
                                ""data"":
                                {
                                    ""id"":""dummyId2""
                                    }
                                }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_expected_multiple_messages_with_headers_using_default_serializer()
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

            var messages = new[]
            {
                new Message { Id = "dummyId1" },
                new Message { Id = "dummyId2" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(2, spy.ProduceCallCount);
            var expected = @"{
                                ""messageId"":""1"",
                                ""type"":""bar"",
                                ""foo-key"":""foo-value"",
                                ""data"":
                                {
                                    ""id"":""dummyId2""
                                    }
                                }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_multiple_messages_using_metadata()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "key1" },
                new Message { Id = "key2" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Metadata()
            );

            Assert.Equal(2, spy.ProduceCallCount);
            Assert.Contains("key1", spy.AllKeys);
            Assert.Contains("key2", spy.AllKeys);
        }

        [Fact]
        public async Task produces_multiple_messages_using_message_handler_context()
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

            var messages = new[]
            {
                new Message { Id = "0" },
                new Message { Id = "1" }
            };

            await sut.Produce(
                messages: messages,
                context: new MessageHandlerContext(new Metadata())
            );

            Assert.Equal(2, spy.ProduceCallCount);
            var expected = @"
                            {
                            ""messageId"":""1"",
                            ""type"":""bar"",
                            ""data"":
                            {
                                ""id"":""1""
                                }
                            }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_multiple_messages_using_message_handler_context_with_additional_headers()
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

            var messages = new[]
            {
                new Message { Id = "0" },
                new Message { Id = "1" }
            };

            await sut.Produce(
                messages: messages,
                context: new MessageHandlerContext(new Metadata()),
                headers: new Dictionary<string, string>
                {
                    { "additional-key", "additional-value" }
                }
            );

            Assert.Equal(2, spy.ProduceCallCount);
            var expected = @"
                            {
                            ""messageId"":""1"",
                            ""type"":""bar"",
                            ""additional-key"":""additional-value"",
                            ""data"":
                            {
                                ""id"":""1""
                                }
                            }";

            AssertJson.Equal(expected, spy.Value);
        }

        [Fact]
        public async Task produces_multiple_messages_preserves_order()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "first" },
                new Message { Id = "second" },
                new Message { Id = "third" },
                new Message { Id = "fourth" },
                new Message { Id = "fifth" }
            };

            await sut.Produce(messages);

            Assert.Equal(5, spy.ProduceCallCount);
            Assert.Equal("first", spy.ProducedMessages[0].Key);
            Assert.Equal("second", spy.ProducedMessages[1].Key);
            Assert.Equal("third", spy.ProducedMessages[2].Key);
            Assert.Equal("fourth", spy.ProducedMessages[3].Key);
            Assert.Equal("fifth", spy.ProducedMessages[4].Key);
        }

        [Fact]
        public async Task produces_multiple_messages_with_headers_preserves_order()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "alpha" },
                new Message { Id = "beta" },
                new Message { Id = "gamma" }
            };

            await sut.Produce(
                messages: messages,
                headers: new Dictionary<string, object>
                {
                    { "foo-key", "foo-value" }
                }
            );

            Assert.Equal(3, spy.ProduceCallCount);
            Assert.Equal("alpha", spy.ProducedMessages[0].Key);
            Assert.Equal("beta", spy.ProducedMessages[1].Key);
            Assert.Equal("gamma", spy.ProducedMessages[2].Key);
            Assert.Collection(spy.ProducedMessages,
                msg => Assert.Equal(1, msg.Order),
                msg => Assert.Equal(2, msg.Order),
                msg => Assert.Equal(3, msg.Order)
            );
        }

        [Fact]
        public async Task produces_multiple_messages_with_context_preserves_order()
        {
            var spy = new KafkaProducerSpy();

            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<Message>("foo", "bar", @event => @event.Id)
                    .Build()
                )
                .Build();

            var messages = new[]
            {
                new Message { Id = "msg1" },
                new Message { Id = "msg2" },
                new Message { Id = "msg3" },
                new Message { Id = "msg4" }
            };

            await sut.Produce(
                messages: messages,
                context: new MessageHandlerContext(new Metadata())
            );

            Assert.Equal(4, spy.ProduceCallCount);
            var orderedKeys = spy.ProducedMessages.OrderBy(m => m.Order).Select(m => m.Key).ToArray();
            Assert.Equal(new[] { "msg1", "msg2", "msg3", "msg4" }, orderedKeys);
        }
    }
}