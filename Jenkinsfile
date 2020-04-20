@Library('ExtendLibs@V1.0.0')_

pipeline {
    agent any
    stages {
        stage('Input') {
            agent { label 'Agent' }
            steps {
                echo env.NODE_NAME
                script {

                    // 更新Source
                    gitCmd.UpdateSource('master', 
                        'GitLabPrivateKey_iMac_002_To_JenkinsBoy',
                        'ssh://git@192.168.101.38:54322/tools/UnityPlugins.git')

                    // 取得Job执行者
                    sysCmd.GetJobExecuter()

                    // 取得远程分支列表
                    gitCmd.GetRemoteBranches(env.WORKSPACE)
                    _branches = gitCmd.getOutput()

                    // 显示Job开始输入条件信息
                    // 其中指定分支名env.BRANCH_NAME
                    inputCmd.ShowJobStartCondition(_branches)
                }
            }
            post {
                success {
                    script {
                        gitCmd.PostSuccess("Input")
                    }
                }
                failure {
                    script {
                        gitCmd.PostFailure("Input")
                    }
                }
            }

        }
        stage('Build') {
            parallel {
                stage('AB') {
                    agent { label 'Agent && Git && Unity' }
                    when {
                        environment name : "AB_BUILD", value : "true"
                    }
                    stages {
                        stage('Source') {
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // 更新Source
                                    gitCmd.UpdateSource(env.BRANCH_NAME, 
                                        'GitLabPrivateKey_iMac_002_To_JenkinsBoy',
                                        'ssh://git@192.168.101.38:54322/tools/UnityPlugins.git')
                                }
                            }
                            post {
                                success {
                                    script {
                                        gitCmd.PostSuccess("Source Update")
                                    }
                                }
                                failure {
                                    script {
                                        gitCmd.PostFailure("Source Update")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("代码更新失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('Build') {
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // AB包打包
                                    unity3dCmd.ABBuild(
                                        env.UNITY_PATH, env.WORKSPACE, env.PROJECT_OUTPOUT)
                                }
                            }
                            post {
                                success {
                                    script {
                                        unity3dCmd.PostSuccess('AB Build')
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("AB包打包成功", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        unity3dCmd.PostFailure('AB Build')
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("AB包打包失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('Upload') { 
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // 上传AB包(Curl)
                                    uploadCmd.ABUploadByCurl(env.PROJECT_SHELL)
                                }
                            }
                            post {
                                success {
                                    script {
                                        uploadCmd.PostSuccess("Upload")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("AB包上传成功", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        uploadCmd.PostFailure("Upload")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("AB包上传失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                    }
                    post {
                        always {
                            script {
                                // AB结果列表推送
                                artifactsCmd.PublishABResult()
                                // 通知：钉钉
                                notifyCmd.NotifyDingTalk("AB Build Result Publish 完成", env.DINGDING_TOKEN)
                            }
                        }
                    }
                }
                stage('iOS/Android') {
                    agent { label 'Agent && Git && Unity' }
                    environment {
                        PROJECT_NM = "UnityPlugins"
                    }
                    stages {
                        stage('Source') {
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // 更新Source
                                    gitCmd.UpdateSource(env.BRANCH_NAME, 
                                        'GitLabPrivateKey_iMac_002_To_JenkinsBoy',
                                        'ssh://git@192.168.101.38:54322/tools/UnityPlugins.git')
                                }
                            }
                            post {
                                success {
                                    script {
                                        gitCmd.PostSuccess("Source Update")
                                    }
                                }
                                failure {
                                    script {
                                        gitCmd.PostFailure("Source Update")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("代码更新失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('Defines Settings') {
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // 上
                                    unity3dCmd.DefinesSettings(
                                        env.UNITY_PATH, env.WORKSPACE, 
                                        "LOCALIZATION,LOCALIZATION_CN")
                                }
                            }
                            post {
                                success {
                                    script {
                                        unity3dCmd.PostSuccess("Defines Settings")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("宏定义设置成功", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        unity3dCmd.PostFailure("Defines Settings")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("宏定义设置失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('iOS Build') { 
                            when {
                                environment name : "PLATFORM_TYPE", value : "iOS"
                            }
                            steps {
                                echo env.NODE_NAME
                                script {
                                    
                                    // iOS打包
                                    unity3dCmd.ExportXCodeProject(
                                        env.UNITY_PATH, env.WORKSPACE, env.PROJECT_OUTPOUT)

                                    // 导出archive文件
                                    xcodeCmd.Build("development", "1.0.0", "94185591-0de6-4289-ab67-624cbdd67aff")
                                }
                            }
                            post {
                                success {
                                    script {
                                        unity3dCmd.PostSuccess("iOS Build")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("ipa打包成功", env.DINGDING_TOKEN)
                                        // 推送成果物
                                        artifactsCmd.PublishArtifacts()
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("Ipa Archive Publish 完成", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        unity3dCmd.PostFailure("iOS Build")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("ipa打包失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('Android Build') { 
                            when {
                                not {
                                    environment name : "PLATFORM_TYPE", value : "iOS"
                                }
                            }
                            steps {
                                echo env.NODE_NAME
                                script {
                                    // Android打包
                                    unity3dCmd.AndroidBuild(
                                        env.UNITY_PATH, env.WORKSPACE, env.PROJECT_OUTPOUT)
                                }
                            }
                            post {
                                success {
                                    script {
                                        unity3dCmd.PostSuccess("Android Build")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("apk打包成功", env.DINGDING_TOKEN)
                                        // 推送成果物
                                        artifactsCmd.PublishArtifacts()
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("Apk Archive Publish 完成", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        unity3dCmd.PostFailure("Android Build")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("apk打包失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                        stage('UploadToCDN') {
                            when {
                                environment name : "UPLOAD_TO_CDN", value : "true"
                            } 
                            stages {
                                stage('Upload - Android') {
                                    when {
                                        not {
                                            environment name : "PLATFORM_TYPE", value : "iOS"
                                        }
                                    }
                                    steps {
                                        echo env.NODE_NAME
                                        script {
                                            // 上传Apk文件(Curl)
                                            uploadCmd.ApkUploadByCurl(env.PROJECT_SHELL)
                                        }
                                    }
                                }
                                stage('Upload - iOS') {
                                    when {
                                        environment name : "PLATFORM_TYPE", value : "iOS"
                                    }
                                    steps {
                                        echo env.NODE_NAME
                                        script {
                                            // 上传Ipa文件(Curl)
                                            uploadCmd.IpaUploadByCurl(env.PROJECT_SHELL)
                                        }
                                    }
                                }
                            }
                            post {
                                success {
                                    script {
                                        uploadCmd.PostSuccess("Upload")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("Apk/Ipa 上传成功", env.DINGDING_TOKEN)
                                    }
                                }
                                failure {
                                    script {
                                        uploadCmd.PostFailure("Upload")
                                        // 通知：钉钉
                                        notifyCmd.NotifyDingTalk("Apk/Ipa 上传失败", env.DINGDING_TOKEN)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        stage('Info') {
            steps {
                echo env.NODE_NAME
                script {
                    echo env.CUR_WORK_DIR
                    echo env.JOB_NAME
                    echo env.BUILD_NUMBER
                    echo env.BRANCH_NAME
                    echo env.PLATFORM_TYPE
                    echo env.BUILD_MODE
                    echo env.AB_BUILD
                }
            }
        }
    }
}