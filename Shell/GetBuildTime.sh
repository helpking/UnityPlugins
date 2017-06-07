#!/bin/bash 

# 取得打包编译时刻（YYYYMMDDHHMMSS）
buildTime=`cat ./BuildInfo | awk -F ':' '{print $1}'`

echo ${buildTime}

