---
title:  Unity后工业化流程
date: 2020-06-20 11:41:32
categories:
- Unity
tags:
- IndustrialAutomation
---

# Unity后工业化流程

* 1 后工业自动化:是创建游戏之后,需要验证这个游戏的整个流程,并且输出性能评测数据,报告给具体的开发人员.
* 2 第一步:打包自动化(例如:打 apk,ipa,app,exe等).
* 3 第二步:安装游戏(中间涉及到 adb 的使用,root 手机等)
* 4 第三步:连接 UPR,并启动游戏
* 5 第四步:开启自动玩游戏流程.
* 6 第五步:分析性能报告输出结果,报告给每一位开发人员.
* 7 第六步:附加资源检查自动化,此流程可自成一条线.

***

# 成本
* 1 开发人员一名,开发人员需要对以下条件熟悉或者精通;
* 2 mac 电脑一台;
* 3 Android手机一部,需要 root;
* 4 Jenkins 基本操作,安装并且创建任务,得到可执行结果
* 5 shell 基础
* 6 Python 基础
* 7 Unity 打包
* 8 UPR 基础操作
* 9 Airtest IDE 软件操作

***

# 准备

* 1 安装 [Jenkins](https://www.jenkins.io/),具体的配置请查看 https://www.jianshu.com/p/5ad61bb45b32
* 2 安装 Unity ,官网安装即可.
* 3 安装 Python ,安装 3.7 版本,如果安装了 Python3.8 也是可以做的.
* 4 安装 Airtest IDE ,直接下载即可,本教程采取的是 app,没有使用 Python 环境.
* 5 下载 UPR mac 版到电脑,放在工作文件夹中.请查看官网https://upr.unity.cn/

# 代码

* 1 创建一个 Jenkins 任务,选择自由构建,最终会执行一个脚本.以下代码为 shell 脚本里面的代码,最终会在 Jenkins 里面执行.
* 2 创建一个 Check_All 的文件夹,并将 UnityProfiler_upr mac 版放入
* 3:打包,涉及到 Unity 打包策略.也就是第一步,形成打包自动化,真正的 Jenkins 打包界面是需要很多参数形成的,此步简化.(请移步官网 https://docs.unity3d.com/Manual/CommandLineArguments.html)

```
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity
PROJECT_PATH= xxx/xxx/

killall Unity

${UNITY_PATH} -quit -projectPath ${PROJECT_PATH} -executeMethod ProjectBuild.BuildForAndroid_DEV  -logFile ${PROJECT_PATH}/build.txt

```

* 4:统一 adb 工具,将 AirTestIDE 中的 ADB工具 与 UPR MAC版工具中的数据保持一致.我使用的是直接将AirTestIDE中的 adb 覆盖到 UPR 中.AirTestIDE中的 adb路径为:/Applications/AirtestIDE.app/Contents/MacOS/airtest/core/android/static/adb/mac/adb,使用AirTestIDE链接手机(一定要连接正常,如有操作错误,请移步官网教程:http://airtest.netease.com/tutorial/Tutorial.html)
* 5:我使用的是小米8,所以需要亮屏操作
```
# 亮屏,xxx是 UPR 的路径
cd xxx
if [ "$(xxx/platform-tools/adb shell dumpsys power | grep state= | grep -oE '(ON|OFF)')" == OFF ] ; then
    echo "Screen is off. Turning on."
    ./adb shell input keyevent 26 # wakeup
    # ./adb shell input keyevent 82 # unlock
    echo "OK, should be on now."
fi
```

* 6:开启另一个终端,终端里面执行


```
# 多少毫秒后暂停玩游戏,xxx 表示随意路径
open -n "xxx/Auto_End_Game.sh"

#*******************************
#Auto_End_Game内容
sleep 500s #暂停 500s

cd UPR路径
./UnityPerfProfiler --stop

```


* 7:关于自动化玩游戏,我采用的策略比较粗暴.打开 Airtest IDE 软件,并一直连接上 Android 手机,编写自动化玩游戏的代码,编写代码需要根据游戏自行编写,因为在脚本里面提前启动此 app,所有我在自动化玩游戏里面先休眠了 30s,等待游戏开启.此软件在之后一直连接在手机上面,并且一直开着.脚本里面编写:

```
# 开始自动化玩游戏,将AirtestIDE设置为焦点,5 秒之后模拟按下 F5,启动自动化玩游戏
open -a "/Applications/AirtestIDE.app"

sleep 5s

function prompt() {
  osascript <<EOT
    tell application "System Events"
      key code 96
    end tell
EOT
}
 
value="$(prompt)"
```

* 8:启动连接到 UPR 服务,此时会UPR自动启动游戏.
```
# 启动 UPR,这个线程到最后都会卡住,SessionId 表示在 UPP 官网上面创建的测试 id
首先获取SessionId,使用 Python 脚本获取
SessionId=`python3 xxx/UPR_Get_SessionId.py`
echo $SessionId

cd UPR路径
./UnityPerfProfiler -p 10.19.8.78 -s ${SessionId}  -n com.xlcw.twgame.cn    

```
* 9:等待Auto_End_Game脚本结束,在结束之前,最好可以执行完毕AirtestIDE.app 中的自动玩游戏代码.最终执行,发送到企业微信群里面.这个地方可以做成二次数据处理,具体到某一函数,资源,发送给具体的开发人员,代码可以在 Python 里面直接发送.

```
curl 'xxx' \
-H 'Content-Type: application/json' \
-d '
{
    "msgtype": "text",
    "text": {
        "content": "关注性能增长，共创美好游戏:https://upr.unity.cn/projects/0b7a1b08-8f37-4783-beac-0f6d60145824",
        "mentioned_list":["xxx","@all"],
    }
}'
```

* 10:二次处理数据,报告给具体的某个开发人员

```
python3 /Users/dino/Documents/WorkSpace/TactileWars/Tools/Auto_Check_All/UPR/GetAndReport.py ${SessionId}

```

# 开始

* 1 点击 Jenkins 上面的构建按钮,等待自动发出项目报告.

# 结束
* 1 最终的流程是:自动化打包-->自动化安装apk-->自动化玩游戏-->自动化输出性能报告.
* 2 其中只需要 Python 自动化玩游戏的维护成本即可.
* 3 购买 UPR 企业服务,可以最终享受到,那个函数,那个资源最终指向哪个开发人员去解决.
* 4 为项目节省时间,为公司创造价值,为游戏行业奠定 Unity后工业化流程.


# 广告





