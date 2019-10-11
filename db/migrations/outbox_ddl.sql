CREATE SCHEMA IF NOT EXISTS outbox;

CREATE TABLE outbox."OutboxMessage" (
    "MessageId" uuid NOT NULL,
    "CorrelationId" varchar(255) NOT NULL,
    "Topic" varchar(255) NOT NULL,
    "Key" varchar(255) NOT NULL,
    "Type" varchar(255) NOT NULL,
    "Format" varchar(255) NOT NULL,
    "Data" text NOT NULL,
    "OccurredOnUtc" timestamp NOT NULL,
    "ProcessedUtc" timestamp NULL,

    CONSTRAINT domainevent_pk PRIMARY KEY ("MessageId")
);

CREATE INDEX domainevent_processedutc_idx ON outbox."OutboxMessage" ("ProcessedUtc" NULLS FIRST);

CREATE INDEX domainevent_occurredonutc_idx ON outbox."OutboxMessage" ("OccurredOnUtc" ASC);
