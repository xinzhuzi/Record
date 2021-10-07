---
title: AI,行为树,FSM
date: 2020-05-08 11:41:32
categories:
- AI
tags:
- AI
---

# AI关键词,取自 http://www.aisharing.com/

|关键词|中文释义|解释|
|--|--|--|
|Decision|决策|底层逻辑|
|Behavior|行为|一般情况下指的是单个动作|
|Logic Action|逻辑动作|角色需要做出负和游戏身份的动作|
|Behavior Pool|行为池|一系列行为组成的集合|
|Simple Behavior|单一行为|在组织过程中的表现|
|Composite Behavior|复合行为|在组织过程中的表现|
|Sequence|序列|一个行为接着一个行为做|
|Parallel|并行|多个行为同时进行|
|Selector|选择|从候选行为中选择一个执行,包括脚本选择和随机选择|
|Behavior Command|行为命令|面向AI决策层的接口,AI把行为命令压入行为队列，等待行为机的处理。这里的命令都是预先定义好的，再用上次举过的小狗的例子，决策层发现小狗的疲劳度很高了，需要“睡觉”，就可以发一个定义好的睡觉的命令，然后再由下层行为机负责处理。|
|Behavior Machine/FSM|行为机器|行为机的主要任务是维护一个行为队列，并执行队列中的行为命令，每一个行为命令都对应一个定义好的执行单元，即行为结点.基于动态命令队列的行为系统（Based on Dynamic Command Queue’s Behavior System）|
|Behavior Node|FSM中的节点|真正的行为执行单元，是一组简单行为的集合。结点内部，根据行为命令中的相关信息，进行处理，如选择动画，播放动画等等|
|Behavior Tree|行为树|基于静态树结构的行为系统（Based on Static Tree’s Behavior System）,BT的叶子节点上全部是行为节点,中间的全部是控制节点,控制节点就包括了前面说过的三种基本结构:选择,并行,序列|
|Behavior Node|BT中的节点|具体的执行效果,可以用FSM结构,先决条件Precondition,Enter,Execute,Exit来完成|
|Control Node|控制节点|顾名思义,即控制执行效果的节点,需要提供一个Transition接口的调用来完成相关的清理工作,比如从这个节点转到另外一个节点,必须调用上一个节点的Transition来完成转换|
|Share Data/Blackboard|共享数据/黑板|分为全局共享数据与单个行为树,状态机的共享数据|
|Scoring System|基于分数系统的AI设计|分数系统的一个基本思想，是为每个单独的行为打分，根据分数的高低来决定做哪个行为，而所有的因素就是打分的依据|
|Enter|进入|每个节点的入口方法|
|Exit|离开|每个节点的离开方法|
|Execute|运行|每个节点的运行方法|
|ActionNode|行为节点|具体到最终效果的节点|
|ControlNode|控制节点|具体执行哪条线哪个行为节点的节点|
|Precondition|前提条件|执行某个节点前需要的前提|
|InputParam|输入|行为树的输入,可以是黑板,也可以是世界信息|
|OutputParam|输出|行为树的输出,也可以是Request请求,这是决策层,输出的请求到行为层,进行动画播放|
|Working Memory|工作池子|缓存池,双缓存等等|
|Decentralized|分散的|共享数据是分散的|
|Centralized|集中的|共享数据是集中的|
|GOAP|目标导向型行动计划|一种AI设计架构,Goal目标,plan计划,Action操作,Oriented导向,1:目标选择(Goal Selector);2:传递目标到计划器(Planner);3:计划器根据目标和前提(Precondition)生成多个子计划(Plan,NodeTree);4:进行计划实施(Plan Stepper);5:计划实施的表现效果(Effect)|
|Planner|计划器|再GOAP中都会存在一个计划器的模块|
|Precondition&Effect|前提和效果|某个行为需要满足的条件,这个行为对世界的影响|
|Goal Selector|目标选择模块|选择AI当前需要万层的目标(相当于更高层的决策层)|
|Plan Stepper|计划实施模块|相当于行为层|













