JobSystem 融入说明

======================================================
## 目录

### [8.FAQ](./JobSystem.md#8faq-1)

### [9.参考文献](./JobSystem.md#9%E5%8F%82%E8%80%83%E6%96%87%E7%8C%AE-1)

======================================================

#### 8.FAQ
[返回目录](./JobSystem.md#%E7%9B%AE%E5%BD%95)

* IJob的Entity即便是采用了JobSystem系统，但是实际执行的时候，还是有可能在主线程中被调用
* Unity3d场景中出现闪面的解决方法
```
    当你发现在unity3d场景中，发现有闪面的现象，
    基本上是由于面之间的距离太近导致的，专业术语Z-Fighting，
    出现这种情况可以调整摄像机的Clipping plane属性中的Near值来解决这个问题。
```


#### 9.参考文献
[返回目录](./JobSystem.md#%E7%9B%AE%E5%BD%95)

* [Unity* 实体组件系统 (ECS)、C# 作业系统和突发编译器入门](https://software.intel.com/es-es/node/782796?language=es)
* [Unity C# Job System介绍(一) Job System总览和多线程](https://zhuanlan.zhihu.com/p/56459126)
* [Unity C# Job System介绍(二) 安全性系统和NativeContainer](https://zhuanlan.zhihu.com/p/57626413)
* [Unity C# Job System介绍(三) Job的创建和调度](https://zhuanlan.zhihu.com/p/57859896)
* [Unity C# Job System介绍(四) 并行化Job和故障排除(完结)](https://zhuanlan.zhihu.com/p/58125078)
* [浅谈Unity ECS（一）Uniy ECS基础概念介绍：面向未来的ECS](https://zhuanlan.zhihu.com/p/59879279)
* [浅谈Unity ECS（二）Uniy ECS内存管理详解：ECS因何而快](https://zhuanlan.zhihu.com/p/64378775)
* [浅谈Unity ECS（三）Uniy ECS项目结构拆解：功能要点及案例分析](https://zhuanlan.zhihu.com/p/70782290)
* [Unity ECS 高性能探索详解](https://blog.csdn.net/sun124608666/article/details/100693050)
* [浅谈Unity ECS结合Job System详解](https://blog.csdn.net/sun124608666/article/details/100693224)
* [Unity3D 海水多线程渲染算法实现](https://blog.csdn.net/jxw167/article/details/75633778)
* [Unity Burst 用户指南](https://blog.csdn.net/alph258/article/details/83997917)
* [`Unity ECS 资料`](https://www.jianshu.com/p/05b8901d8e20)