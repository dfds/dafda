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

CREATE OR REPLACE FUNCTION _outbox_notifier()
RETURNS trigger AS $FN$
BEGIN
	PERFORM pg_notify('dafda_outbox',
		json_build_object(
			'operation', TG_OP,
			'record', row_to_json(NEW)
		)::text
	);

	RETURN NEW;
END;
$FN$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS _outbox_trigger ON _outbox;

CREATE TRIGGER _outbox_trigger
AFTER INSERT
ON _outbox
FOR EACH ROW
EXECUTE PROCEDURE _outbox_notifier();

