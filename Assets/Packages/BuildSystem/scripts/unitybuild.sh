buildsystemDir=Assets/Packages/UnityBuildSystem
#generate build_tag info
i=`set | grep ARG_TAG | wc -l`
if [ $i -gt 0 ]; then
	buildTag=`set | grep ARG_TAG | sed 's/ARG_TAG=//'`
else
	buildTag=`date '+LOCALCMD_%Y%m%d_%H%M%S'`
fi
echo UNITYBUILD: generated build_tag $buildTag

#clean unity log file
unityLogFile=~/Library/Logs/Unity/Editor.log
echo Clean > $unityLogFile

#start logging from unity log file into console
tail -F $unityLogFile &

#unity path
if [ -n "$REQUIRED_UNITY_VERSION" ]; then
  echo UNITYBUILD: REQUIRED_UNITY_VERSION is $REQUIRED_UNITY_VERSION

  unityVersion=`echo $REQUIRED_UNITY_VERSION | tr . _`
  unityVersion="UNITY_"$unityVersion"_PATH"
  if [ -z "${!unityVersion}" ]; then
    echo UNITYBUILD: Error: $unityVersion not set, can not find Unity version path, aborting
	kill %1
    exit 1
  fi

  unityBinPath=${!unityVersion}"/Unity.app/Contents/MacOS/Unity"
  if [ ! -f $unityBinPath ]; then
    echo UNITYBUILD: Error: Unity binaries not found at $unityBinPath, aborting
	kill %1
    exit 1
  fi
else
  echo UNITYBUILD: Warning: REQUIRED_UNITY_VERSION not set, falling back to default Unity path
  unityBinPath="/Applications/Unity_534p4/Unity.app/Contents/MacOS/Unity"
fi
#detecting target platform
if [ -n "$MY_CONF_PLATFORM" ]; then
	buildTarget="-buildTarget $MY_CONF_PLATFORM"
else
	echo UNITYBUILD: Warning: MY_CONF_PLATFORM not set, falling back to using configuration name prefix as the default build target
	if [[ $1 == ios* ]]; then
		echo UNITYBUILD: choosing iOS platform
		buildTarget="-buildTarget ios"
	elif [[ $1 == android* ]]; then
		echo UNITYBUILD: choosing Android platform
		buildTarget="-buildTarget android"
	fi
fi

#launch unity batch build
echo UNITYBUILD: building using Unity binary $unityBinPath
$unityBinPath -quit -batchmode -projectpath "$PWD" $buildTarget -executeMethod Glu.UnityBuildSystem.AutoBuild.Build +configTypeName "$@" +buildTag $buildTag

#catch the exit status
unityExit="$?"

#stop logging from unity log file into console
kill %1

exit $unityExit
