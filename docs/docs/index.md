# Introduction

__Dafda__ is a small Kafka client library for .NET that provides some high-level conveniences to allow setting up producers and consumers in a consistent manner.

## Features

* Purpose built for Kafka alone.
* Built on top of [Confluent.Kafka](https://docs.confluent.io/current/clients/dotnet.html).
* Built with .NET Core's configuration, logging, & dependency injection in mind.
* Supports Kafka configuration from multiple sources (e.g. environment variables).
* Easy ("plug-and-play") registration of consumer and producer messages.
* Default (configurable) JSON message serialization/deserialization.
* Supports multiple typed producers akin to HttpClient factories.
* Consumer messages are consumed in the background using hosted services.
* Multiple consumers supported, which can be configured independently.
* Extendable Outbox pattern implementation.

