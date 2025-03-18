# 可复用UI框架

这是一个为Unity项目设计的轻量级、可扩展的UI框架，提供了分层UI管理、自动组件绑定、弹窗系统等功能。

## 核心功能

- **分层UI管理**：UI分为背景、主界面、弹窗和加载四个层级，方便管理不同类型的UI元素
- **自动组件绑定**：使用`AutoBind`特性自动绑定UI组件，减少手动拖拽工作
- **标准化生命周期**：为所有UI面板提供标准的初始化、显示、隐藏和销毁流程
- **弹窗队列系统**：支持弹窗队列，自动管理多个弹窗的显示顺序
- **UI控制器模式**：将UI逻辑与显示分离，提高代码可维护性
- **动画支持**：内置基础动画效果，并支持自定义入场和退出动画

## 框架结构

### 核心类

- **UIManager**：UI管理器，负责管理UI层级和已打开的面板
- **BasePanel**：所有UI面板的基类，提供生命周期方法和自动组件绑定
- **BasePopup**：所有弹窗的基类，提供弹窗特有功能如背景遮罩、位置设置等
- **BaseUIController**：UI控制器基类，负责控制面板的打开、关闭和数据传递
- **PopupSystem**：弹窗系统，负责弹窗的显示、队列管理和关闭
- **UIInitializer**：UI系统初始化器，在游戏启动时初始化UI系统

### 示例类

- **MainMenuPanel/Controller**：主菜单面板和控制器示例
- **SettingsPanel/Controller**：设置面板和控制器示例
- **MessagePopup**：消息弹窗示例
- **UsageExample**：框架使用示例

## 如何使用

### 1. 初始化UI系统

在场景中创建一个空物体，添加`UIInitializer`组件，并设置主画布和弹窗配置。

```csharp
// UIInitializer会自动初始化UIManager和PopupSystem
public class UIInitializer : MonoBehaviour
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private List<PopupConfigItem> popupConfigs;
    
    private void Awake()
    {
        // 初始化UIManager
        InitializeUIManager();
        
        // 初始化PopupSystem
        InitializePopupSystem();
        
        Debug.Log("UI系统初始化完成");
    }
}
```

### 2. 创建面板

1. 创建一个继承自`BasePanel`的类:

```csharp
public class YourPanel : BasePanel
{
    // 使用AutoBind特性自动绑定组件
    [AutoBind("Path/To/Button")] private Button yourButton;
    
    protected override void PreInitialize()
    {
        base.PreInitialize();
        // 初始化事件监听等
        yourButton.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        // 处理按钮点击
    }
}
```

2. 创建对应的控制器:

```csharp
public class YourPanelController : BaseUIController<YourPanel>
{
    protected override string PanelPrefabPath => "UI/Panels/YourPanel";
    
    // 添加控制器特有方法
    public void DoSomething()
    {
        // ...
    }
}
```

### 3. 创建弹窗

1. 创建一个继承自`BasePopup`的类:

```csharp
public class YourPopup : BasePopup
{
    [AutoBind("Title")] private Text titleText;
    [AutoBind("Content")] private Text contentText;
    [AutoBind("ConfirmButton")] private Button confirmButton;
    
    protected override void PreInitialize()
    {
        base.PreInitialize();
        
        // 设置按钮事件
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }
    
    public override void SetData(object data)
    {
        if (data is YourPopupData popupData)
        {
            titleText.text = popupData.Title;
            contentText.text = popupData.Content;
        }
    }
    
    private void OnConfirmClicked()
    {
        // 处理确认按钮点击
        Close();
    }
}

// 弹窗数据类
public class YourPopupData
{
    public string Title { get; set; }
    public string Content { get; set; }
    
    public YourPopupData(string title, string content)
    {
        Title = title;
        Content = content;
    }
}
```

2. 在`PopupSystem`中注册弹窗:

```csharp
// 在UIInitializer中
PopupSystem.Instance.AddPopupConfig("YourPopup", new PopupConfig
{
    PrefabPath = "UI/Popups/YourPopup",
    DefaultPosition = PopupPosition.Center
});
```

### 4. 使用UI

```csharp
// 打开面板
YourPanelController.Instance.OpenPanel(panel => {
    // 面板打开后的回调
    // 可以设置面板数据
    panel.SetData(yourData);
});

// 关闭面板
YourPanelController.Instance.ClosePanel();

// 显示弹窗
YourPopupData data = new YourPopupData("标题", "内容");
PopupSystem.Instance.ShowPopup<YourPopup>(data);

// 显示在指定位置的弹窗
PopupSystem.Instance.ShowPopup<YourPopup>(data, PopupPosition.Top);
```

## 自定义动画

可以重写`PlayEnterAnimation`和`PlayExitAnimationAndDestroy`方法来自定义入场和退出动画:

```csharp
protected override IEnumerator PlayEnterAnimation()
{
    // 执行基类动画
    yield return base.PlayEnterAnimation();
    
    // 添加自定义动画
    transform.localScale = Vector3.zero;
    
    float duration = 0.3f;
    float startTime = Time.time;
    
    while (Time.time - startTime < duration)
    {
        float t = (Time.time - startTime) / duration;
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
        yield return null;
    }
    
    transform.localScale = Vector3.one;
}
```

## 注意事项

1. 确保所有UI预制体都放在`Resources`文件夹下，以便通过路径加载
2. 使用`AutoBind`特性时，路径是相对于UI组件的根物体的相对路径
3. 每个面板/弹窗都应该有对应的数据类，用于传递数据
4. 控制器类都使用单例模式，可通过`Instance`属性访问
5. 弹窗默认会进入队列，按先后顺序显示，如果队列中有多个弹窗

## 示例场景

查看`Assets/Scenes/UIExample.unity`场景，了解框架的基本用法。 