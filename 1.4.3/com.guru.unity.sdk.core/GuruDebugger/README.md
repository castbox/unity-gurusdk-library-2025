# Guru Debugger 面板

Version 1.0.0

# 使用方法

调用方式, 可直接调用

```c#
// 首次调用需要初始化
Debugger.Init():

// TODO: 加入你的Layout初始化方法     
    
// 显示 Debugger:
Debuggger.Show();

// 关闭回调
Debugger.OnClose += OnDebuggerClose;  
 
private void OnDebuggerClose(){
    
    // TODO: do sth when debugger is closed 
}

```

Layout 初始化
```c#
// 添加一个条目
// 一般一个条目的构成为  {tab}/{option} 的方式

// 添加一个 Key - Value item 
Debugger.Instance.AddOption("Start Info/Test Key", "Test Value");

// 添加一个整体可点击的 Item
Debugger.Instance.AddOption("Start Info/Yes, click me", "", () => {
    // TODO: the item is a pure button, add click event.
});

// 添加一个 Button
Debugger.Instance.AddOption("Start Info/Test Key", "Test Value")
    .AddButton("Button", ()=>{
        // TODO: do sth when button is clicked
    });

// Option 可以添加更多的内容, 但是不建议超过 5 个
Debugger.Instance.AddOption("Start Info/Test2", "valueof2")
    .AddLabel("Sth else to add")
    .AddCopyButton();



```






