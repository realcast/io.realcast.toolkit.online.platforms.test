#!/bin/bash

UnityPlayerPath=$(cygpath -w "$UNITY_PLAYER_PATH")
scriptName="[Steam Post Export]"

echo $scriptName" : Listing environment variables"
echo "*****************************"
env
echo "*****************************"

echo $scriptName" : Renaming Unity Player"
mv $UnityPlayerPath "$UnityPlayerPath/../JUST HOOPS.exe"
mv $UnityPlayerPath/../Steam_Data "$UnityPlayerPath/../JUST HOOPS_Data"
rm -rf $UnityPlayerPath/../Steam_BackUpThisFolder_ButDontShipItWithYourGame
rmdir $UnityPlayerPath/../Steam_BackUpThisFolder_ButDontShipItWithYourGame