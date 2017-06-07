#!/bin/bash 

CMD=$0

while getopts 'm:' opt
do
    case "$opt" in
    m)
		# 尽管官方有app-store, ad-hoc, package, enterprise, development, and developer-id 6种
		# 本脚本暂时只支持 以下四种
        if [ ${OPTARG} != "AdHoc" ] && [ ${OPTARG} != "AppStore" ] && [ ${OPTARG} != "Enterprise" ] && [ ${OPTARG} != "Development" ];then
            echo "Error! Should enter the method (AdHoc AppStore Enterprise or Development)"
        	echo ""
            exit 1
        fi
        METHOD=${OPTARG}
        ;;
    \?)
		echo "Command Format: ${CMD} -m [AdHoc/AppStore/Enterprise/Development]"
        echo ""
        exit 2
        ;;
    esac
done

# UNITY程序的路径#
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity

# 游戏程序路径#
PROJECT_PATH=/Users/helpking/Desktop/01.WorkDir/99.Tools/UnityPlugins

# Build Shell Dir
BUILD_SHELL_DIR=${PROJECT_PATH}/Shell

# IOS打包脚本路径#
BUILD_IOS_SHELL=${BUILD_SHELL_DIR}/buildios.sh
if [ ! -f ${BUILD_IOS_SHELL} ]; then
	echo "The shell file build ios is not exist!!! file:${BUILD_IOS_SHELL}"
	exit 2
fi

# Xcode导出工程名
XCODE_PROJECT_NAME=Demo

# Ipa Export Plist
IPA_EXPORT_PLIST_FILE=${BUILD_SHELL_DIR}/ipaExport_${METHOD}.plist
if [ ! -f ${IPA_EXPORT_PLIST_FILE} ]; then
	echo "The plist file is not exist for export!!! file:${IPA_EXPORT_PLIST_FILE}"
	exit 2
fi

# 生成的Xcode工程路径
XCODE_PROJECT_PATH=${PROJECT_PATH}/Output
if [ ! -d ${XCODE_PROJECT_PATH} ]; then
	mkdir ${XCODE_PROJECT_PATH}
fi
XCODE_PROJECT_PATH=${XCODE_PROJECT_PATH}/iOS
if [ ! -d ${XCODE_PROJECT_PATH} ]; then
	mkdir ${XCODE_PROJECT_PATH}
fi
XCODE_PROJECT_PATH=${XCODE_PROJECT_PATH}/XcodeProject
if [ ! -d ${XCODE_PROJECT_PATH} ]; then
	mkdir ${XCODE_PROJECT_PATH}
fi
rm -rf ${XCODE_PROJECT_PATH}/*

# 将unity导出成xcode工程
# 前台运行
# echo "$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod BuildSystem.ProjectBuild.ExportXcodeProject -projectName ${XCODE_PROJECT_NAME} -debug -quit"
# $UNITY_PATH -projectPath $PROJECT_PATH -executeMethod BuildSystem.ProjectBuild.ExportXcodeProject -projectName ${XCODE_PROJECT_NAME} -debug -quit
# 后台运行
echo "$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod BuildSystem.ProjectBuild.ExportXcodeProject -projectName ${XCODE_PROJECT_NAME} -debug  -batchmode -quit "
$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod BuildSystem.ProjectBuild.ExportXcodeProject -projectName ${XCODE_PROJECT_NAME} -debug  -batchmode -quit 

echo "XCODE工程生成完毕"
open ${XCODE_PROJECT_PATH}

# 开始生成ipa
echo "sh $BUILD_IOS_SHELL ${XCODE_PROJECT_PATH} ${XCODE_PROJECT_NAME} ${IPA_EXPORT_PLIST_FILE} Debug"
sh $BUILD_IOS_SHELL ${XCODE_PROJECT_PATH} ${XCODE_PROJECT_NAME} ${IPA_EXPORT_PLIST_FILE} Release

echo "ipa生成完毕"


