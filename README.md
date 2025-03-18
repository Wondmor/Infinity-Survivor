一个第三人称射击动作角色扮演游戏demo，以《Warframe》作为参考

使用有限状态机实现主角和敌人的控制系统和动画切换。
![image](https://github.com/user-attachments/assets/a5c54e87-01d8-47e6-98ce-4d6fbd2a8df8)

使用 A*寻路算法规划敌人移动 AI。

使用 InputSystem 管理玩家输入，并支持运行时重新绑定按键，维护 InputActionAsset 文件，并利用 JSON 格式保存到 PlayerPrefs
![image](https://github.com/user-attachments/assets/edfb1cfb-c5f6-4800-a1ac-36ad0757e953)

使用对象池，重复利用对象实例，根据储存的物品 ID 进行刷新，实现无限滚动背包。

编写 PlayerData，BackpackData 等类，记录必要数据，序列化为 JSON 字符串并写入本地文件，实现存档系统。

将场景划分为触发区域，通过碰撞器检测玩家进入，结合 JSON 表配置刷怪规则，使用分帧加载和对象池优化怪物生成。

使用 JSON 表配置怪物掉落，采用分层随机实现不同掉落。

编写基于 UGUI 的 UI 框架：
  ◼ 弹窗类型：重载多个方法便于显示弹窗，包括设置有时限的纯文本提示，传入回调绑定到单个按钮的提示等。用于显示拾取物品等提示。
  ◼ 界面类型：提供通用单例 Controller 和 Panel 基类，包含查找对应 prefab 并显示，初始化面板，设置信息等方法，注册按钮事件监听等方法，用于设置界面，背包界面等
  ◼ 利用协程、Dotween、动画机等方式实现界面动态和平滑

应用设计模式：
  ◼ 实现泛型单例基类，便于制作单例控制器
  ◼ 利用异步状态机，实现场景管理，异步加载资源
  ◼ 利用观察者模式，实现各种消息的传递
  ◼ 利用工厂模式，辅助刷怪

