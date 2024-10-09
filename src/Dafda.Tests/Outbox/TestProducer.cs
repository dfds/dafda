using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Diagnostics;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Newtonsoft.Json.Linq;
using OpenTelemetry.Context.Propagation;
using Xunit;

namespace Dafda.Tests.Outbox;

[Collection("Serializing")]
public class TestProducer
{
    [Fact]
    public void TryDeserializePayload_should_deserialize_json_string_to_dictionary()
    {
        // Arrange
        string payload =
            "{\"messageId\":\"1b13a5e1-742e-45fb-ab7a-76d547e4e327\",\"type\":\"userdisabled\",\"traceparent\":\"00-1f2c12212e50621b49c80175a064d193-35a6133087d5e877-01\",\"causationId\":\"1b13a5e1-742e-45fb-ab7a-76d547e4e327\",\"correlationId\":\"1b13a5e1-742e-45fb-ab7a-76d547e4e327\",\"data\":{\"dfdsUserId\":\"8cfb2d2d-9113-48c8-8c8d-41ade8d79989\"}}";
        // Act
        bool result = DafdaActivitySource.TryDeserializePayload(payload, out var payloadDictionary);

        // Assert
        Assert.True(result);
        Assert.NotNull(payloadDictionary);
        Assert.Equal("1b13a5e1-742e-45fb-ab7a-76d547e4e327", payloadDictionary["messageId"]);
        Assert.Equal("userdisabled", payloadDictionary["type"]);
        Assert.Equal("00-1f2c12212e50621b49c80175a064d193-35a6133087d5e877-01", payloadDictionary["traceparent"]);
        Assert.Equal("1b13a5e1-742e-45fb-ab7a-76d547e4e327", payloadDictionary["causationId"]);
        Assert.Equal("1b13a5e1-742e-45fb-ab7a-76d547e4e327", payloadDictionary["correlationId"]);

        // Deserialize the data field to ensure it's a JSON object
        var dataObject = JObject.Parse(payloadDictionary["data"].ToString());
        Assert.Equal("8cfb2d2d-9113-48c8-8c8d-41ade8d79989", dataObject["dfdsUserId"]);
        Assert.Equal("{\"dfdsUserId\":\"8cfb2d2d-9113-48c8-8c8d-41ade8d79989\"}", payloadDictionary["data"].ToString());
    }

    [Fact]
    public void TryDeserializePayload_should_deserialize_complex_json_string_to_dictionary()
    {
        // Arrange
        string payload =
            "{\"messageId\":\"0b8db195-62bd-43ee-9123-70b64adf9fff\",\"type\":\"AssociateAccessRelationsChanged\",\"data\":{\"freightPayerId\":22937,\"externalAssociates\":[{\"associateId\":75900,\"accessToFreightPayer\":1},{\"associateId\":51147,\"accessToFreightPayer\":1}],\"changedBy\":\"b3bef642-e347-417c-9bec-2660f4376ggg\"},\"correlationId\":null,\"causationId\":\"fecb6bee0a799cbd\",\"tenantId\":\"Production\"}";

        // Act
        bool result = DafdaActivitySource.TryDeserializePayload(payload, out var payloadDictionary);

        // Assert
        Assert.True(result);
        Assert.NotNull(payloadDictionary);
        Assert.Equal("0b8db195-62bd-43ee-9123-70b64adf9fff", payloadDictionary["messageId"]);
        Assert.Equal("AssociateAccessRelationsChanged", payloadDictionary["type"]);

        // Deserialize the data field to ensure it's a JSON object
        var dataObject = JObject.Parse(payloadDictionary["data"].ToString());
        Assert.Equal(22937, dataObject["freightPayerId"]);
        Assert.Equal("b3bef642-e347-417c-9bec-2660f4376ggg", dataObject["changedBy"]);
        Assert.Equal(2, dataObject["externalAssociates"].Count());

        //Assert.Empty(payloadDictionary["correlationId"]);
        Assert.Equal("fecb6bee0a799cbd", payloadDictionary["causationId"]);
        Assert.Equal("Production", payloadDictionary["tenantId"]);
    }

    [Fact]
    public async Task Creates_activity_when_producing_outbox_message()
    {
        var topic = "foo";
        var type = "bar";
        DafdaActivitySource.Propagator = new CompositeTextMapPropagator(
            new TextMapPropagator[]
            {
                new TraceContextPropagator(),
                new BaggagePropagator()
            });

        // Arrange
        var activitySource = new ActivitySource("ActivitySourceName");
        using var activity = activitySource.StartActivity("MethodType:/Path");

        var spy = new KafkaProducerSpy();
        var outboxProducer = new OutboxProducer(spy);
        var guid = Guid.NewGuid();

        var payload = $@"{{
                                ""messageId"":""{guid.ToString()}"",
                                ""type"":""{type}"",
                                ""causationId"":""1"",
                                ""correlationId"":""1"",                           
                                ""data"":{{
                                    ""id"":""dummyId""
                                    }}
                                }}";


        var outboxEntry = new OutboxEntry(
            messageId: guid,
            topic: topic,
            key: type,
            payload: payload,
            occurredUtc: DateTime.UtcNow
        );

        var activities = new List<Activity>();

        // Act
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "Dafda",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => activities.Add(activity),
            ActivityStopped = activity => activities.Add(activity),
        };
        ActivitySource.AddActivityListener(activityListener);

        await outboxProducer.Produce(outboxEntry);

        // Assert
        Assert.Equal("foo", spy.Topic);
        Assert.Equal("bar", spy.Key);
        var jsonObject = JObject.Parse(spy.Value);
        Assert.True(jsonObject["traceparent"] != null, "The JSON does not contain the traceparent element.");
        Assert.Contains(activities, a => a.DisplayName == $"{OpenTelemetryMessagingOperation.Producer.Publish} {topic} {type}");

        // Deserialize the data field to ensure it's a JSON object
        var dataObject = JObject.Parse(jsonObject["data"].ToString());
        Assert.Equal("dummyId", dataObject["id"]);
        
        // Check that the serialized JSON does not contain unwanted escaped characters
        Assert.DoesNotContain("\\u0022", spy.Value);
        Assert.DoesNotContain("\\", spy.Value);
    }
}