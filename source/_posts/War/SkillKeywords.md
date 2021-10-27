---
title: Unity War 技能系统关键词
date: 2021-11-01 11:41:32
top: 106
categories:
- War
tags:
- Ability
---

# 参考
* 1. https://zhuanlan.zhihu.com/p/147681650

# 关键词

|英语|汉语|含义|
|--|--|--|
|AOI|关注区域|area of interest,主要解决游戏中多人同屏的问题.算法有九宫格，十字链表，六边形网格等等.AOI搜寻到的列表并不能就完全等同于关注者列表。而客户端显示的就是关注者列表里面的内容。AOI搜寻到的列表只是用来获得关注者的手段。|
|Skill|技能|技能模块,技能数据准备,技能释放,前摇,技能释放持续,后摇等|
|Buff|增益状态|buff 状态,改变玩家属性,技能属性等等|
|projectile|子弹,抛射物|子弹模块,也就是技能释放物,技能释放出之后的效果模块|
|PassiveAblity|被动技能||
|GeneralAbility|主动释放技能|包含了 施法技能,普通一次性触发技能,引导类持续施法技能|
|ChannelAbility|引导类持续施法技能||
|ToggleAbility|开关类技能|点击技能开启/关闭效果,类似于持续添加某个状态,会消耗蓝量等等|
|ActivateAbility|激活类技能|点击右键激活/停止,一般是给普通攻击附加特殊效果|
|target|施法目标|有 3 种情况,不需要目标即可施法,需要选定目标,指定地点为目标|
|AbilityStart|技能起手|技能开始准备|
|Spell|施法|技能起手到施法中间,叫做技能前摇|
|SkillFinish|技能结束|施法到结束,叫做技能后摇阶段|
|Forced immediate cast|强制立即施放|技能前摇和后摇,都可以被打断,并强制立即施放另外一个技能|
|Channel|持续施法类技能||
|ChannelStart|引导开始|技能起手到引导开始是属于前摇阶段|
|ChannelContinue| 引导持续阶段|引导技能持续触发,间隔固定时间调用(ChannelThink)|
|ChannelFinish|引导结束|引导结束到技能结束属于后摇阶段|
|ThinkInterval|引导触发事件间隔|,在引导持续阶段中,每隔多少时间触发一次伤害|
|ChannelTime|引导阶段总时长|ChannelTime/ThinkInterval,表示触发多少次|
|CastPoint|引导开始之后至引导事件触发|,也就是引导触发事件间隔的缓冲,只有从开始时候的一次|
|Modifier|修改器|继承于 Buff,因为有角色的属性修改,有的角色不需要,不需要的继承 buff,需要的继承于Modifier|
|MotionModifier|运动效果修改器|继承于 Modifier,代表此类Buff提供修改玩家运动效果的功能。因为牵涉到与运动组件的交互，所以抽象出一个新的类。|
|Caster|Buff施加者|Buff施加者,可能为空,也可能不为空|
|Parent|父实体|Buff 当前挂载的目标|
|Ability|技能|Buff由哪个技能创建,有可能为空，也有可能不为空|
|BuffLayer|层数||
|BuffLevel|等级||
|BuffDuration|时长||
|BuffTag|标志位|标注这个Buff属于那些种类以及免疫哪些种类。|
|BuffImmuneTag|免疫BuffTag|标注这个Buff属于那些种类以及免疫哪些种类。|
|BuffContext| 上下文|Buff创建时的一些相关上下文数据|
|Buff stage|增益效果持续阶段|创建之前,实例化之后,生效之前,刷新当前数据阶段,销毁前,销毁后.|
|Stun|眩晕|（眩晕状态——目标不再响应任何操控）|
|Root|缠绕|缠绕，又称定身——目标不响应移动请求，但是可以执行某些操作，如施放某些技能|
|Silence|沉默|沉默——目标禁止施放技能|
|Invincible|无敌|几乎不受到所有的伤害和效果影响|
|Invisible|隐身|不可被其他人看见|
|Buff ModifyAttribute|属性修改|分为 2 层,一层为核心基础数据,第二层为外部层(等级,装备,成就,任务)|
|Buff ModifyMotion|运动表现修改|位移,突进,翻滚,千斤坠,击退,击飞,拖拽,吸引|
|AOE|大招|范围攻击,全屏攻击|
|TrackingProjectile|追踪子弹|子弹具有一个目标Target，它创建出来后以直线速度飞向指定目标。|
|LinearProjectile|线性子弹|子弹为等腰梯形检测盒子，无需目标，创建出来后沿着特定方向飞行一段距离。|
|Owner|实体类似于 Parent|表示子弹的创建者|
