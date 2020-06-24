---
title:  Unity后工业化流程
date: 2020-06-20 11:41:32
categories:
- Unity
tags:
- IndustrialAutomation
---

# Unity后工业化流程

* 1 后工业自动化:是创建游戏之后,需要验证这个游戏的整个流程,并且输出性能评测数据,报告给具体的开发人员.形成一个内环开发链,有输入有输出.
* 2 第一步:打包自动化(例如:打 apk,ipa,app,exe等).
* 3 第二步:安装游戏(中间涉及到 adb 的使用,root 手机等)
* 4 第三步:连接 UPR,并启动游戏
* 5 第四步:开启自动玩游戏流程.
* 6 第五步:分析性能报告输出结果,报告给每一位开发人员.
* 7 第六步:资源,AB包的检查自动化,此流程可自成一条线,但也属于 Unity 后工业流程化的一部分.

***

# 成本
* 1 开发人员一名;
* 2 mac 电脑一台;
* 3 Android手机一部,尽量 root,此 root 会减少搭建流程中的很多细节问题;
* 4 Jenkins 基本操作,安装并且创建任务,得到可执行结果.(官网:https://www.jenkins.io/)
* 5 shell 基础(基本学习:https://www.runoob.com/linux/linux-shell.html)
* 6 Python 基础(B 站上找个全套 Python 看完即可)
* 7 Unity 打包(https://docs.unity3d.com/Manual/CommandLineArguments.html)
* 8 UPR 基础操作(https://upr.unity.cn/instructions)
* 9 Airtest IDE 软件操作(http://airtest.netease.com/)
* 10 一定要 shell,Python,Unity,Jenkins,UPR,Airtest,adb 串联起来才可以搭建起来整个流程.

***

# 准备

* 1 安装 [Jenkins](https://www.jenkins.io/),具体的配置请查看教程: https://www.jianshu.com/p/5ad61bb45b32
* 2 安装 Unity ,官网下载安装即可.
* 3 安装 Python ,安装 3.7 版本,如果安装了 Python3.8 也是可以做的.
* 4 安装 Airtest IDE ,直接下载即可,本教程中采取的是 AirtestIDE.app的使用,没有使用 Python 环境打造.
* 5 下载 UPR mac 版到电脑,放在工作文件夹中.请查看官网https://upr.unity.cn/ 

# 工业化流程

* 1 创建一个 Jenkins 任务,选择自由构建,最终会执行一个脚本.以下代码为 shell 脚本里面的代码,最终会在 Jenkins 里面执行.
![Jenkins配置1](Jenkins配置1.png)
![Jenkins配置2](Jenkins配置2.png)

* 2 在 SVN 上面创建一个文件夹,用来存放当前教程中用到的工具以及代码
![SVN 目录](SVN目录.png)

* 3:介绍从 shell 脚本Auto_Check_All.sh 运行到打包GetAndReport.py 的步骤.首先是 Unity 打包策略.也就是第一步,形成打包自动化,真正的 Jenkins 打包界面是需要很多参数形成的,此流程仅仅为了方便,因此做简化.需要注意的是,此打包方式一定是开发模式.(请移步官网 https://docs.unity3d.com/Manual/CommandLineArguments.html)

```
# unity 中可以执行命令的文件路径
UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity
# 项目路径
PROJECT_PATH= xxx/xxx/
# 打包之前先杀掉所有打开的 Unity 工程
killall Unity
# 进行打包,这个地方不懂的可以查看https://github.com/xinzhuzi/UnityTools 进行模拟操作
${UNITY_PATH} -quit -projectPath ${PROJECT_PATH} -executeMethod ProjectBuild.BuildForAndroid_DEV  -logFile ${PROJECT_PATH}/build.txt

```

* 4:统一 adb 工具,将 AirTestIDE 中的 ADB工具 与 UPR MAC版工具中的数据保持一致.我使用的是直接将AirTestIDE中的 adb 覆盖到 UPR 中.AirTestIDE中的 adb路径为:/Applications/AirtestIDE.app/Contents/MacOS/airtest/core/android/static/adb/mac/adb,使用AirTestIDE链接手机,有时候是硬件问题,有时候是软件问题,都可以在官网或者官网群里面解决,里面的网易小姐姐很善良,人又聪明,说话还好听.(一定要连接正常,如有操作错误,请移步官网教程:http://airtest.netease.com/tutorial/Tutorial.html)
* 5:我使用的是小米系列,有时候需要亮屏操作,这一步有的要有的不要,自行摘选即可.
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

* 6:覆盖安装apk
```
# adb devices

# 卸载这个 app,目前这一步在启动 APP 的时候会产生Android弹框,不推荐使用,如果 root 手机可以避免这一步,或者其他手段可以避免,则执行这一步是最好的
# adb uninstall  (apk的唯一标示符)

# 覆盖安装这个 app,此 APK 一定是开发者模式的 APK
cd UPR路径/platform-tools/
./adb install -r xxx/xxx.apk

```

* 7 先开启另一个进程,在此进程中进行自动停止 UPR 的检测,所以为什么前面 Jenkins 上面需要时间限制,就是在模拟真实玩家可以玩游戏玩多久,有轻度用户,重度用户之分.具体细则还需要大家慢慢扩展.
```
# 多少毫秒后暂停玩游戏,xxx 表示随意路径
open -n "xxx/Auto_End_Game.sh"

############################################
# Auto_End_Game内容很简单,只有 3 行

sleep ${Runtime} #默认暂停 500s,也就是玩游戏大约有 500s 左右的时间,其他操作请自行扩展,比如自定义截图等

cd UPR路径
./UnityPerfProfiler --stop
```


* 8:自动化玩游戏
我采用的策略比较粗暴.打开 Airtest IDE 软件,并一直连接上 Android 手机,编写自动化玩游戏的代码,编写代码需要根据游戏自行编写,因为在脚本里面提前启动此 app,所有我在自动化玩游戏里面先休眠了 30s,等待游戏开启.此软件在之后的正式流程下会一直连接在手机上面,并且一直开着.
自动化流程脚本里面编写:
```
# 开始自动化玩游戏,将AirtestIDE设置为焦点,5 秒之后模拟按下 F5,启动自动化玩游戏
# 首先暂停 2s,防止前面代码与下面代码带来冲突
sleep 2s
# 将打开的AirtestIDE.app 变为焦点app,也就是屏幕最前方的一个 app
open -a "/Applications/AirtestIDE.app"
# 暂停 5s ,避免冲突
sleep 5s

# 模拟按下 F5,启动自动化玩游戏

function prompt() {
  osascript &lt;&lt;EOT
    tell application "System Events"
      key code 96
    end tell
EOT
}
 
value="$(prompt)"
```

* 9:启动连接到 UPR 服务,此时会UPR自动启动游戏.
```
# Adb 启动游戏,目前不适用
# ./adb shell am start -n com.xlcw.twgame.cn/com.unity3d.player.UnityPlayerActivity

# 启动 UPR,这个线程到最后都会卡住,SessionId 表示在 UPP 官网上面创建的测试 id
首先获取SessionId,使用 Python 脚本获取
SessionId=`python3 xxx/UPR_Get_SessionId.py`
echo $SessionId
# 这个地方具体请看官网的操作
cd UPR路径
./UnityPerfProfiler -p ip地址 -s ${SessionId}  -n com.xlcw.twgame.cn    

```
* 10:python 脚本UPR_Get_SessionId.py 进行创建SessionId
```
url = "https://upr.unity.cn/backend/sessions/create"
# 其中parameter需要自己上网页里面查看是什么,每个人的项目中都不一样,这个地方不贴这些东西了,必填;
# cookies也一样,必填;
# headers不填也行.

res = requests.post(url=url, data=parameter,
                        headers=headers_post, cookies=cookies)
print(json.loads(res.text)["SessionId"])
# 这样执行完毕之后,即可在 shell 脚本里面拿到这个值
```
![性能检测.png](性能检测.png)   
![资源检测.png](资源检测.png)
*  11:数据二次处理
等待Auto_End_Game脚本结束,停止UPR,(可选操作:关闭 app,执行完毕AirtestIDE.app 中的自动玩游戏代码,关闭AirtestIDE.app等).等待 10s 之后,使用 Python 进行二次数据处理,具体到某一函数,资源,发送给具体的开发人员.最后,将数据分析结果发送到企业微信群里面.此时可以根据每个开发人员的情况进行分发处理任务.
```
#shell 脚本里面编写
python3 xxx/GetAndReport.py ${SessionId}

#python文件 GetAndReport.py 中编写
# headers_get中包含 cookie,请自行查找.
res = requests.get(
        "https://upr.unity.cn/backend/summary/" + SessionId, headers=headers_get)
# 得到结果之后,进行二次处理,比如我这边就只处理了整体数据
jd = json.loads(res.text)
data = "\n性能数据:{" + "ReservedMono峰值:" + \
        str(jd["summary"]["maxReservedMono"]) + "MB," + "纹理资源峰值:" + \
        str(jd["summary"]["maxTexture"]) + "MB," + "动画资源峰值:" + \
        str(jd["summary"]["maxAnimationClip"]) + "MB," + \
        "音频资源峰值:" + str(jd["summary"]["maxAudio"]) + "MB," + \
        "网格资源峰值:" + str(jd["summary"]["maxMesh"]) + "MB," + \
        "DrawCall峰值:" + str(jd["summary"]["maxDrawCalls"]) + "次," + \
        "Tris峰值:" + str(jd["summary"]["maxTriangles"]) + "面" + "}"

# 然后上报企业微信里面,这个需要根据企业微信进行定制开发,官网:https://work.weixin.qq.com/ 
```
![最终@所有人.png](最终@所有人.png)

* 12:资源与 AB 包检测
```
# 根据官网的 shell 写法,直接在 Jenkins 里面输出即可,处理二次数据与上面方式一样.
```

***
# 开始

* 1 点击 Jenkins 上面的构建按钮,等待自动发出项目报告.

***
# 结束
* 1 最终的流程是:自动化打包--&gt;自动化安装apk--&gt;自动化玩游戏--&gt;自动化输出性能报告.
* 2 其中只需要 Python 自动化玩游戏的维护成本即可.
* 3 购买 UPR 企业服务,可以最终享受到,那个函数,那个资源最终指向哪个开发人员去解决.
* 4 为项目节省时间,为公司创造价值,为游戏行业奠定 Unity后工业化流程.
***
# 广告
* 1 明显效益:可以节省研发人员成本.隐藏效益:嗯~~~,也可以将很多测试人员干掉,为公司节省财富,不过我不建议老板这么干.
* 2 请一定重视这个 <<Unity后工业化流程>> ,搭建成功就可以为一个项目节省大量时间;推广到整个公司,就能为公司带来巨大效益;如果整个游戏行业都有这个流程,那就是一场改革.
* 3 本公司招人