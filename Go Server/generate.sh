#!/bin/zsh

protoc pb/subspb.proto --go_out=plugins=grpc:.
