syntax = "proto3";

option csharp_namespace = "Share.Protobuf.${Namespace}";

import "google/protobuf/duration.proto";  
import "google/protobuf/timestamp.proto";
import "google/protobuf/wrappers.proto";
import "google/protobuf/any.proto";

${Services}
${Messages}