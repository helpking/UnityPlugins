#!/bin/bash 

CMD=$0
CUR_DIR=`pwd`

# 错误命令输出
InvalidCmdFormat() {
	echo ""
	echo "Invalid Cmd Format!!"
    echo ""
    echo "格式:"
    echo ""
    echo "${CMD} -u [Unity Path] -o [AdHoc/AppStore/Enterprise/Development] -m [Debug/Release] -p [工程名] -g [游戏名] -n [打包No] -f [Export PList File]"
    echo ""
    echo "描述"
    echo ""
    echo "  必选项:"
    echo ""
    echo "    -u : [Unity Path]。"
    echo "         Unity的安装路径。(如：Mac下为 : /Applications/Unity/Unity.app/Contents/MacOS/Unity)"
    echo ""
    echo "    -o : [AdHoc/AppStore/Enterprise/Development]"
    echo "         选项。官方有app-store, ad-hoc, package, enterprise, development, and developer-id。"
    echo "         目前只支持 : AdHoc/AppStore/Enterprise/Development"
    echo ""
    echo "    -m : [debug/release]"
    echo "         打包模式。目前仅支持debug/release。"
    echo ""
    echo "    -p : [工程名]。"
    echo "         与导出文件关联。如工程名如果指定为：RTC。则导出的文件名为 : RTC_Debug_1.0.0_-1_20190708163428_com.zy.rtc.ipa"
    echo ""
    echo "  可选项:"
    echo ""
    echo "    -g : [游戏名]。"
    echo "         是安装App后，手机上显示的名字。若不指定，则为工程名。"
    echo ""
    echo "    -n : [打包No]。"
    echo "         CI工具链上的打包No。如Teamcity或Jenkins。"
    echo ""
    echo "    -f : [Export PList File]。"
    echo "         Ipa导出用的PList文件路径。"
}

if [ $# -lt 8 ];then
	InvalidCmdFormat
    exit 2
fi

METHOD="Development"
BUILD_MODE="Debug"
PRO_NAME=""
GAME_NAME=""
BUILD_NO=-1
UNITY_PATH="/Applications/Unity/Unity.app/Contents/MacOS/Unity"
DEFINES=""
IPA_EXPORT_PLIST_FILE=""
while getopts 'u:o:m:p:g:n:d:f:' opt
do
    case "$opt" in
    u)
		UNITY_PATH=${OPTARG}
		if [ -z "$UNITY_PATH" ]; then
			InvalidCmdFormat
		    exit 1
		fi
		;;
    o)
		# 尽管官方有app-store, ad-hoc, package, enterprise, development, and developer-id 6种
		# 本脚本暂时只支持 以下四种
        if [ ${OPTARG} != "AdHoc" ] && [ ${OPTARG} != "AppStore" ] && [ ${OPTARG} != "Enterprise" ] && [ ${OPTARG} != "Development" ]; then
		    InvalidCmdFormat
		    exit 1
        fi
        METHOD=${OPTARG}
        ;;
    m)
		# 打包模式
		if [ ${OPTARG} != "Debug" ] && [ ${OPTARG} != "Release" ]; then
		    InvalidCmdFormat
		    exit 1
        fi
        BUILD_MODE=${OPTARG}
        ;;
    p)
		# 工程名
		PRO_NAME=${OPTARG}
		if [ -z "$PRO_NAME" ]; then
			InvalidCmdFormat
		    exit 1
		fi
        ;;
    g)
		# 游戏名
		GAME_NAME=${OPTARG}
        ;;
    n)
		# 打包No
		BUILD_NO=${OPTARG}
		;;
    f)
		# Export PList File
		IPA_EXPORT_PLIST_FILE=${OPTARG}
		;;
    ?)
	    InvalidCmdFormat
	    exit 2
        ;;
    esac
done

# 游戏名若未指定
if [ -z "$GAME_NAME" ]; then
	GAME_NAME=${PRO_NAME}
fi

# UNITY程序的路径#
UNITY_PATH=${UNITY_PATH}

# 游戏程序路径#
PROJECT_PATH=${CUR_DIR}
CHECK_TMP=`echo ${PROJECT_PATH} | grep "/Shell"`
if [ -n "$CHECK_TMP" ]; then
	PROJECT_PATH=${PROJECT_PATH}/..
fi

# 生成的Xcode工程路径
XCODE_EXPORT_PATH=${PROJECT_PATH}/Output
if [ ! -d ${XCODE_EXPORT_PATH} ]; then
	# 创建导出目录
	echo "mkdir ${XCODE_EXPORT_PATH}"
	mkdir ${XCODE_EXPORT_PATH}
fi

XCODE_EXPORT_PATH_TMP=${XCODE_EXPORT_PATH}/${BUILD_NO}
if [ ! -d ${XCODE_EXPORT_PATH_TMP} ]; then
	# 创建导出目录
	echo "mkdir ${XCODE_EXPORT_PATH_TMP}"
	mkdir ${XCODE_EXPORT_PATH_TMP}
fi

XCODE_EXPORT_PATH_TMP=${XCODE_EXPORT_PATH_TMP}/iOS
if [ ! -d ${XCODE_EXPORT_PATH_TMP} ]; then
	# 创建导出目录
	echo "mkdir ${XCODE_EXPORT_PATH_TMP}"
	mkdir ${XCODE_EXPORT_PATH_TMP}
fi

XCODE_EXPORT_PATH_TMP=${XCODE_EXPORT_PATH_TMP}/XcodeProject
if [ ! -d ${XCODE_EXPORT_PATH_TMP} ]; then
	# 创建导出目录
	echo "mkdir ${XCODE_EXPORT_PATH_TMP}"
	mkdir ${XCODE_EXPORT_PATH_TMP}
fi

# 清空原有目录+文件
echo "rm -rf ${XCODE_EXPORT_PATH_TMP}/*"
rm -rf ${XCODE_EXPORT_PATH_TMP}/*

# 将unity导出成xcode工程
# 前台运行
# _cmd="$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Command.Editor.ProjectBuild.ExportXcodeProject \
# 		-projectName ${PRO_NAME} -${BUILD_MODE} -buildNo ${BUILD_NO} -quit"
# _cmd="$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Command.Editor.ProjectBuild.ExportXcodeProject \
# 		-projectName ${PRO_NAME} -${BUILD_MODE} -buildNo ${BUILD_NO} -batchmode -quit"
_cmd="$UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Command.Editor.ProjectBuild.ExportXcodeProject"

# 工程名
if [ -n "${PRO_NAME}" ]; then
	_cmd="${_cmd} -projectName ${PRO_NAME}"
fi

# 游戏名
if [ -n "${GAME_NAME}" ]; then
	_cmd="${_cmd} -gameName ${GAME_NAME}"
fi

_cmd="${_cmd} -${BUILD_MODE} -buildNo ${BUILD_NO} -batchmode -quit"
echo "${_cmd}"
`${_cmd}`

#echo "XCODE工程生成完毕"
BUILD_OUTPUT_DIR=${XCODE_EXPORT_PATH}/${BUILD_NO}/iOS/XcodeProject
open ${BUILD_OUTPUT_DIR}

# 进入工作目录
cd ${BUILD_OUTPUT_DIR}

# XCode版本信息
echo "XCode 版本信息"
xcodebuild -version

# 显示当前SDK信息
echo "SDK 信息"
xcodebuild -showsdks

# 打包输出路径（Ipa 输出路径）  
BUILD_OUTPUT_DIR=${BUILD_OUTPUT_DIR}/build  
if [ ! -d ${BUILD_OUTPUT_DIR} ]; then
	mkdir ${BUILD_OUTPUT_DIR}
fi

# 显示工程列表
xcodebuild -list

# ArchivePath
ARCHIVE_PATH=${BUILD_OUTPUT_DIR}/${PRO_NAME}.xcarchive

# 清空&并生成xcarchive文件
xcodebuild -configuration ${BUILD_MODE} clean archive -archivePath ${ARCHIVE_PATH} -scheme Unity-iPhone

# Ipa Export Plist
# 若未指定
if [ -z "$IPA_EXPORT_PLIST_FILE" ]; then
	# IPA_EXPORT_PLIST_FILE=${CUR_DIR}/ipaExport_${METHOD}.plist
	IPA_EXPORT_PLIST_FILE=`find ${CUR_DIR} -name "ipaExport_${METHOD}.plist"`
fi
if [ ! -f ${IPA_EXPORT_PLIST_FILE} ]; then
	echo "The plist file is not exist for export!!! file:${IPA_EXPORT_PLIST_FILE}"
	exit 2
fi

# 导出ipa文件
xcodebuild -exportArchive -archivePath ${ARCHIVE_PATH} -exportOptionsPlist ${IPA_EXPORT_PLIST_FILE} -exportPath ${BUILD_OUTPUT_DIR}

# 取得编译信息
BUILD_INFO_TMP=`find ${CUR_DIR} -name "BuildInfo"`
if [ -z "${BUILD_INFO_TMP}" ]; then
	echo "Build Info Invalid!!"
	exit 2
fi
BUILD_TIME=`cat ${BUILD_INFO_TMP} | awk -F ':' '{print $1}'`
echo "BuildTime:${BUILD_TIME}"

# 工程版本
PRO_VERSION=`cat ${BUILD_INFO_TMP} | awk -F ':' '{print $2}'`
echo "Project Version:${PRO_VERSION}"

# 工程名
PRO_NAME=`cat ${BUILD_INFO_TMP} | awk -F ':' '{print $3}'`
echo "Project Name:${PRO_NAME}"

# Build ID
BUILD_ID=`cat ${BUILD_INFO_TMP} | awk -F ':' '{print $4}'`
echo "Build Id:${BUILD_ID}"

# 改名字
if [ -f ${BUILD_OUTPUT_DIR}/Unity-iPhone.ipa ]; then
	echo "mv ${BUILD_OUTPUT_DIR}/Unity-iPhone.ipa ${BUILD_OUTPUT_DIR}/${PRO_NAME}_${BUILD_MODE}_${PRO_VERSION}_${BUILD_NO}_${BUILD_TIME}_${BUILD_ID}.ipa"
	mv ${BUILD_OUTPUT_DIR}/Unity-iPhone.ipa ${BUILD_OUTPUT_DIR}/${PRO_NAME}_${BUILD_MODE}_${PRO_VERSION}_${BUILD_NO}_${BUILD_TIME}_${BUILD_ID}.ipa
fi

# 返回当前目录
cd ${CUR_DIR}

echo "ipa生成完毕"
exit 0