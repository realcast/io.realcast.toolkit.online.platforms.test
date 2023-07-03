#!/bin/bash

UnityPlayerPath=$(cygpath -w "$UNITY_PLAYER_PATH")
chmod u+x Unity-Cloud-Build/ovr-platform-util.exe
cd Unity-Cloud-Build
./ovr-platform-util.exe self-update
./ovr-platform-util.exe upload-quest-build --app-id $APP_ID --app-secret $APP_SECRET --apk "$UnityPlayerPath" --channel $CHANNEL  --notes "`git rev-parse --short HEAD` - `git log --pretty=format:%s -1`" --debug_symbols_dir Debug/ --debug-symbols-pattern *.so