using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Helpers;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxQueue
    {
        [Fact]
        public async Task Fails_for_unregistered_outgoing_messages()
        {
            var sut = A.OutboxQueue.Build();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Enqueue(new[] {new Message()}));
        }

        [Fact]
        public async Task Can_delegate_persistence_for_outgoing_message()
        {
            var spy = new OutboxEntryRepositorySpy();

            var sut = A.OutboxQueue
                .With(
                    A.OutgoingMessageRegistry
                        .Register<Message>("foo", "bar", @event => "baz")
                        .Build()
                )
                .With(spy)
                .Build();

            await sut.Enqueue(new[] { new Message() });

            Assert.NotEmpty(spy.OutboxEntries);
        }

        [Fact]
        public async Task Can_forward_headers()
        {
            var spy = new OutboxEntryRepositorySpy();

            var sut = A.OutboxQueue
                .With(
                    A.OutgoingMessageRegistry
                        .Register<Message>("foo", "bar", @event => "baz")
                        .Build()
                )
                .With(spy)
                .Build();

            var metadata = new Metadata()
            {
                MessageId = "183388b5-a8e9-4cb4-b553-6699632461c7",
                CausationId = "183388b5-a8e9-4cb4-b553-6699632461c7",
                CorrelationId = "183388b5-a8e9-4cb4-b553-6699632461c7"
            };

            await sut.Enqueue(new[] { new Message() }, metadata);

            var expected = @"{
                            ""messageId"":""183388b5-a8e9-4cb4-b553-6699632461c7"",
                            ""type"":""bar"",
                            ""causationId"":""183388b5-a8e9-4cb4-b553-6699632461c7"",
                            ""correlationId"":""183388b5-a8e9-4cb4-b553-6699632461c7"",
                            ""data"":{
                                }
                            }";


            AssertJson.Equal(expected, spy.OutboxEntries[0].Payload );
        }

        public class Message
        {
        }
    }
}