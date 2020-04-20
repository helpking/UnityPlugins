#!/bin/bash 

CMD=$0
CUR_DIR=`pwd`

# 分支保存临时文件
TMP_FILE="${CUR_DIR}/.branches"

# 取得分支
git branch -a | sed 's/^[ \t]*//g' | sed 's/* //g' | sed 's/->//g' | sed 's/origin\/master//g' > ${TMP_FILE}

BRANCHES=""
# 遍历文件，取得相应分支
for BRANCH in `cat ${TMP_FILE}`
do
	if [[ ${BRANCH} == remotes* ]]; then
		TEST="a"
	else
		if [ -z ${BRANCHES} ]; then
			BRANCHES=${BRANCH}
		else
			BRANCHES="${BRANCHES}:${BRANCH}"
		fi
	fi
done

echo "${BRANCHES}"