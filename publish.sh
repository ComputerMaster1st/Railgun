#!/bin/bash
dotnet publish -o Published -c Release
rsync -r -F ./Published/* cm1-01:~/RailgunLive --progress