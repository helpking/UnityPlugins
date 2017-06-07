#!/bin/bash 

# 取得工程名
projectName=`cat ./BuildInfo | awk -F ':' '{print $3}'`

echo ${projectName}

