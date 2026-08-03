namespace Dafda.Tests.Consuming;

using Dafda.Consuming;
using Xunit;

public class TestKafkaDeadLetterQueue
{
    [Fact]
    public void uses_configured_topic_name_when_provided()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName("orders.dead-letter", "orders");

        Assert.Equal("orders.dead-letter", topic);
    }

    [Fact]
    public void derives_topic_name_from_source_topic_when_not_provided()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName(null, "orders");

        Assert.Equal("orders.dead-letter", topic);
    }
}