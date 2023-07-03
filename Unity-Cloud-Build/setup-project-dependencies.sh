#!/bin/bash

shouldWriteExternalSetupFile=false
demoFlag=""

if [[ $1 == "help" ]]
then
    echo "Usage : setup-project-dependencies.sh [platform [demo]]"
    echo "Available platforms : "
    ls Unity-Cloud-Build/platform-dependencies
    exit 0
elif [[ $1 != "" && $UNITY_VERSION == "" ]]
then
    JOB_BASE_NAME=$1
    shouldWriteExternalSetupFile=true

    if [[ $2 == "demo" ]]
    then
        demoFlag=$2
    fi
fi

scriptName="[Setup Project Dependencies]"

echo $scriptName" : Listing environment variables"
echo "*****************************"
env
echo "*****************************"

platformFile=""
unityPlatform=""
scriptingDefineSymbols=""

if [[ $JOB_BASE_NAME == *quest* ]]
then
    platformFile="oculus-quest"
    unityPlatform="Android"
elif [[ $JOB_BASE_NAME == *pico* ]]
then
    platformFile="pico"
    unityPlatform="Android"
elif [[ $JOB_BASE_NAME == *nolo* ]]
then
    platformFile="nolo"
    unityPlatform="Android"
elif [[ $JOB_BASE_NAME == *qiyu* ]]
then
    platformFile="qiyu"
    unityPlatform="Android"
elif [[ $JOB_BASE_NAME == *steam* ]]
then
    platformFile="steam"
    unityPlatform="Standalone"
elif [[ $JOB_BASE_NAME == *nightly* ]]
then
    platformFile="pico"
    unityPlatform="Android"
elif [[ $JOB_BASE_NAME == *bot* ]]
then
    platformFile="bot"
    unityPlatform="Server"
elif [[ $JOB_BASE_NAME == *playstation-vr2* ]]
then
    platformFile="playstation-vr2"
    unityPlatform="playstation5"
elif [[ $JOB_BASE_NAME == *yvr* ]]
then
    platformFile="yvr"
    unityPlatform="Android"
else
    echo $scriptName" : Cannot recognize platform, quitting..."
    exit 1
fi

if [ "$shouldWriteExternalSetupFile" = true ]
then
    echo -n $platformFile" "$demoFlag > external-project-setup.tmp
fi

if [[ $PRE_RELEASE == "true" ]]
then
    echo "PreRelease environnment variable detected."
    echo "Removing RC_ENABLE_LOGS from scripting define symbols"
    sed -e s/RC_ENABLE_LOGS//g -i Unity-Cloud-Build/platform-scripting-define-symbols/$platformFile
fi

platformFilePath=Unity-Cloud-Build/platform-dependencies/$platformFile
scriptingDefineSymbols=$( cat Unity-Cloud-Build/platform-scripting-define-symbols/$platformFile )

echo $scriptName" : Overwritting Packages/manifest.json with" $platformFilePath
cp $platformFilePath Packages/manifest.json

echo $scriptName" : Overwritting ProjectSettings' Scripting Define Symbols with :" $scriptingDefineSymbols"; coming from file Unity-Cloud-Build/platform-scripting-define-symbols/"$platformFilePath

# Use regex to find a platform under the property scriptingDefineSymbols and write the result to a file
if [[ $unityPlatform == "Android" ]]
then
    ./Unity-Cloud-Build/ag/ag.exe --numbers "scriptingDefineSymbols(.*\n)+?.*    Android:" ProjectSettings/ProjectSettings.asset > tmp.txt
elif [[ $unityPlatform == "Server" ]]
then
    ./Unity-Cloud-Build/ag/ag.exe --numbers "scriptingDefineSymbols(.*\n)+?.*    Server:" ProjectSettings/ProjectSettings.asset > tmp.txt
else
    ./Unity-Cloud-Build/ag/ag.exe --numbers "scriptingDefineSymbols(.*\n)+?.*    Standalone:" ProjectSettings/ProjectSettings.asset > tmp.txt
fi

# Only keep the line number on the last line
lineNumber=$( tail -n1 tmp.txt | cut -f 1 -d : )

# Remove temporary file
rm tmp.txt

# Replace line at $lineNumber with "    $unityPlatform: $scriptingDefineSymbols"
sed -i "${lineNumber}c\    $unityPlatform: $scriptingDefineSymbols" ProjectSettings/ProjectSettings.asset

echo $scriptName" : Dump manifest.json"
echo "********************************"
cat Packages/manifest.json
echo "********************************"

echo $scripting" : Dump projectSettings.asset"
echo "********************************"
cat ProjectSettings/ProjectSettings.asset
echo "********************************"
