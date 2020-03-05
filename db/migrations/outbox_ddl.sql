CREATE TABLE _outbox (
    "Id" uuid NOT NULL,
    "Topic" varchar(255) NOT NULL,
    "Key" varchar(255) NOT NULL,
    "Payload" text NOT NULL,
    "OccurredUtc" timestamp NOT NULL,
    "ProcessedUtc" timestamp NULL,

    CONSTRAINT _outbox_pk PRIMARY KEY ("Id")
);

CREATE INDEX _outbox_processedutc_idx ON _outbox ("ProcessedUtc" NULLS FIRST);
CREATE INDEX _outbox_occurredutc_idx ON _outbox ("OccurredUtc" ASC);
