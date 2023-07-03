#!/bin/bash

git config --global user.email "manny@realcast.io"
git config --global user.name "Cloud Build"
CurrentBranch=`git rev-parse --abbrev-ref HEAD`

git checkout ./ProjectSettings/ProjectSettings.asset
git apply projectsettings.patch
git add ./ProjectSettings/ProjectSettings.asset
git commit -m "Unity Cloud Build : Update bundle version"
git pull origin $CurrentBranch
git push origin $CurrentBranch