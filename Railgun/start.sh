#!/bin/bash
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=false
dotnet Railgun.dll
