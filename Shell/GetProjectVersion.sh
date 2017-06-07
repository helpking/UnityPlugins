#!/bin/bash 

# 工程版本
projectVersion=`cat ./BuildInfo | awk -F ':' '{print $2}'`

echo ${projectVersion}

