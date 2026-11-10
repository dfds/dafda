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
    public void derives_topic_name_from_group_id_and_source_topic_when_not_provided()
    {
        var topic = KafkaDeadLetterQueue.ResolveTopicName(null, "orders", "order-processor");

        Assert.Equal("order-processor.orders.dead-letter", topic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void throws_when_group_id_is_missing(string groupId)
    {
        Assert.Throws<System.InvalidOperationException>(() => KafkaDeadLetterQueue.ResolveTopicName(null, "orders", groupId));
    }
}