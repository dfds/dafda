namespace Dafda.Tests.Consuming;

using Dafda.Consuming;
using Xunit;

public class TestKafkaDeadLetterQueue
{
    [Fact]
    public void uses_configured_topic_name_when_provided()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName("orders.dead-letter", "orders", "order-processor");

        Assert.Equal("orders.dead-letter", topic);
    }

    [Fact]
    public void derives_topic_name_from_source_topic_and_group_id_when_not_provided()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName(null, "orders", "order-processor");

        Assert.Equal("orders.order-processor.dead-letter", topic);
    }

    [Fact]
    public void derives_topic_name_from_source_topic_only_when_group_id_is_missing()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName(null, "orders", null);

        Assert.Equal("orders.dead-letter", topic);
    }
}