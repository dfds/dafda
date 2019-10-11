#!/bin/bash

kafkacat -b localhost:29092 -t test-topic -C
