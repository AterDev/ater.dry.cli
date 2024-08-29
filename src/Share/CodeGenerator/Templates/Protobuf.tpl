syntax = "proto3";

option csharp_namespace = "Grpc.#@Namespace#Service";

package #@Namespace#

import "google/protobuf/duration.proto";  
import "google/protobuf/timestamp.proto";
import "google/protobuf/wrappers.proto";
import "google/protobuf/struct.proto";

#@Services#
#@Messages#