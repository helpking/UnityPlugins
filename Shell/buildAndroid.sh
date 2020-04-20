#!/bin/bash 

# 当前路径
CMD=$0
CUR_DIR=`pwd`
echo "Current Directory : ${CUR_DIR}"

# 错误命令输出
InvalidCmdFormat() {
	echo ""
	echo "Invalid Cmd Format!!"
    echo ""
    echo "格式:"
    echo ""
    echo "${CMD} -u [Unity Path] -n [打包No] -m [Debug/Release] -l [打包日志文件] -g [游戏名] -n [打包No] -f [Export PList File]"
    echo ""
    echo "描述"
    echo ""
    echo "  必选项:"
    echo ""
    echo "  可选项:"
    echo ""
    echo "    -u : [Unity Path]。"
    echo "         Unity的安装路径。(如：Mac下为 : /Applications/Unity/Unity.app/Contents/MacOS/Unity)"
    echo ""
    echo "    -n : [打包No]。"
    echo "         CI工具链上的打包No。如Teamcity或Jenkins。本地打包默认为 : -1。"
    echo ""
    echo "    -l : [Unity打包日志路径]。"
    echo "         若不指定则默认输出目录问当前工程下目录下的Output/Android/[打包No]下的Build.log。"
}

# 参数个数校验
#if [ $# -lt 1 ];then
#	InvalidCmdFormat
#    exit 2
#fi

# UNITY PATH
PRO_UNITY_PATH=${UNITY_PATH}
UNITY_LOG_PATH=""
BUILD_NUMBER=-1

# 输入参数说明
while getopts ':u:n:l:' opt
do
    case "$opt" in
    u)
		PRO_UNITY_PATH=${OPTARG}
		echo "Unity Path : ${PRO_UNITY_PATH}"
		if [ -z "$PRO_UNITY_PATH" ]; then
			InvalidCmdFormat
		    exit 1
		fi
		;;
	l)
		UNITY_LOG_PATH=${OPTARG}
		echo "Unity Log Path : ${UNITY_LOG_PATH}"
		if [ -z "$UNITY_LOG_PATH" ]; then
			InvalidCmdFormat
		    exit 1
		fi
		;;
	n)
		BUILD_NUMBER=${OPTARG}
		echo "Build Number : ${BUILD_NUMBER}"
		;;
    \?)
		InvalidCmdFormat
	    exit 2
        ;;
    esac
done

# 游戏程序路径
PROJECT_PATH=${CUR_DIR}
# 当前目录是否为Shell目录,若是Shell目录则删除Shell，退回上层目录
_STATUS=$(echo ${CUR_DIR} | grep "Shell$")
if [ -n "$_STATUS" ]; then
	PROJECT_PATH=$(echo ${PROJECT_PATH%/*})
fi
echo "Project Path : ${PROJECT_PATH}"

# 输出目录
OUTPUT_PATH=${PROJECT_PATH}/Output
if [ ! -d ${OUTPUT_PATH} ]; then
	echo "mkdir ${OUTPUT_PATH}"
	mkdir ${OUTPUT_PATH}
fi
OUTPUT_PATH=${OUTPUT_PATH}/${BUILD_NUMBER}
if [ ! -d ${OUTPUT_PATH} ]; then
	echo "mkdir ${OUTPUT_PATH}"
	mkdir ${OUTPUT_PATH}
fi
OUTPUT_PATH=${OUTPUT_PATH}/Android
if [ ! -d ${OUTPUT_PATH} ]; then
	echo "mkdir ${OUTPUT_PATH}"
	mkdir ${OUTPUT_PATH}
fi

# Unity日志文件目录
if [ -z "$UNITY_LOG_PATH" ]; then
	UNITY_LOG_PATH=${OUTPUT_PATH}/UnityBuild.log
fi
echo "Unity Build Log : ${UNITY_LOG_PATH}"

# 导入外部指定宏
#echo "$PRO_UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Defines.Editor.DefinesSetting.AddAndroidDefines -projectName NFF -debug -defines LOCALIZATION_CN -batchmode -quit"
#$PRO_UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Defines.Editor.DefinesSetting.AddAndroidDefines -projectName NFF -debug -defines LOCALIZATION_CN -batchmode -quit

#在Unity中构建apk#
#echo "$PRO_UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Command.Editor.ProjectBuild.BuildForAndroid -projectName UnityPlugins -gameName Sample -debug -batchmode -quit"
#$PRO_UNITY_PATH -projectPath $PROJECT_PATH -executeMethod Packages.Command.Editor.ProjectBuild.BuildForAndroid -projectName UnityPlugins -gameName Sample -debug -batchmode -quit

#echo "Apk生成完毕"
