#!/bin/bash 

#UNITY程序的路径#
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity

#游戏程序路径#
PROJECT_PATH=/Users/helpking/Desktop/06.Demo/IosBuildTest

#在Unity中构建apk#
$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod CommandBuildSystem.ProjectBuild.BuildForAndroid -projectName NFF -quit


echo "Apk生成完毕"


