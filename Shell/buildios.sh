#!/bin/bash  

#参数判断  
if [ $# != 4 ];then  
    echo "Params error!"  
    echo "Need two params: 1.path of project 2.name of ipa file"  
    exit  
elif [ ! -d $1 ];then  
    echo "The first param is not a dictionary."  
    exit      

fi  

# XCode版本信息
echo "XCode 版本信息"
xcodebuild -version

# 显示当前SDK信息
echo "SDK 信息"
xcodebuild -showsdks

# 工程路径  
PROJECT_PATH=$1  

#IPA名称  
IPA_NAME=$2  

#ipa file path
IPA_FILE_PATH=${PROJECT_PATH}/${IPA_NAME}.ipa

# Export Plist File
EXPORT_PLIST_FILE=$3

# 编译模式（Debug/Release）
BUILD_MODE=$4

# 输出路径（Ipa 输出路径）  
OUTPUT_DIR=${PROJECT_PATH}/build  
if [ ! -d ${OUTPUT_DIR} ]; then
	mkdir ${OUTPUT_DIR}
fi

# ArchivePath
ARCHIVE_PATH=${OUTPUT_DIR}/${IPA_NAME}.xcarchive

# 编译工程  
cd $PROJECT_PATH  
pwd

# 显示工程列表
xcodebuild -list

# 清空&并生成xcarchive文件
xcodebuild -configuration ${BUILD_MODE} clean archive -archivePath ${ARCHIVE_PATH} -scheme Unity-iPhone

# 导出ipa文件
xcodebuild -exportArchive -archivePath ${ARCHIVE_PATH} -exportOptionsPlist ${EXPORT_PLIST_FILE} -exportPath ${OUTPUT_DIR}

# 改名字
if [ -f ${OUTPUT_DIR}/Unity-iPhone.ipa ]; then
	echo "mv ${OUTPUT_DIR}/Unity-iPhone.ipa ${OUTPUT_DIR}/${IPA_NAME}.ipa"
	mv ${OUTPUT_DIR}/Unity-iPhone.ipa ${OUTPUT_DIR}/${IPA_NAME}.ipa
fi