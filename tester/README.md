# Tester

Small console application for end to end testing of your changes locally.

## Using the tester app

First create a local nuget package with your changes. Update the nuget reference in the tester project to use this package.

Run:

```bash
make package
```

To start a local Kafka instance, run:

```bash
docker-compose up
```
