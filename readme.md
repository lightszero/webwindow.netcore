# webwindow.netcore

## 介绍 

这个库是为dotnet core 程序开发GUI用的

他的基本原理是调用electron来完成的，但是和electron.net发布一个独立的版本不同，这个库设计为任何一个你的dotnet core 程序，另外“加上”GUI的部分

比如简单的为NEOCLI 加上一个界面，这个界面可以随时通过NEOCLI命令呼叫出来，也可以关闭

## 基本使用方法

```
static async void MainAsync()
{
    //init
    WindowMgr windowmgr = new WindowMgr();
    await windowmgr.Init();

    //当GUI全关闭时，让这个进程也退出
    windowmgr.onAllWindowClose += () => bexit = true;
    
    //create window
    WindowCreateOption op = new WindowCreateOption();
    op.title = "hello world";
    var window = await WindowRemote.Create(windowmgr, op);

    //eval
    var time = DateTime.Now.ToString();
    await window.Remote_Eval("document.body.innerHTML='hello world<hr/>" + time + "'");
}
```
### 1.Init
首先初始化windowmgr。
这个过程做了比较多的事情，需要时间，所以是一个async方法
```    
    WindowMgr windowmgr = new WindowMgr();
    await windowmgr.Init();
```
### 2.配置所有窗口全关闭事件
如果是一个纯粹的图形化程序，所有窗口关闭也就该退出了
```   
    windowmgr.onAllWindowClose += () => bexit = true;
```
### 3.创建window,默认创建就显示
```
    WindowCreateOption op = new WindowCreateOption();
    op.title = "hello world";
    var window = await WindowRemote.Create(windowmgr, op);
```
### 4.执行js
这里使用js来给窗口加一点显示内容
```
    var time = DateTime.Now.ToString();
    await window.Remote_Eval("document.body.innerHTML='hello world<hr/>" + time + "'");
```
## 示例
例子在本仓库的 vs_part\samples中

## 0.helloworld

该例子展示基本过程

## 1.sendback

该例子展示如何从窗口向c#程序发送信息，比如输入内容和响应事件

## 2.workwithfiles

该例子展示如何使用文件，例子展示了使用一个硬盘上的图片

## 3.workwithhtml

该例子展示如何配合传统的html工作流开发

该例子使用了一个完整的小网站，包括一个html 一个js 一个img

展示了如何显示该网站，并且在js中编写和c#交互的逻辑

## 原理细节

### 1.唤起electron,执行web/host/index.js

唤起方式 就是进程调用

web/host/index.js 实现了一个rpc接口，这个库通过rpc接口让elctron 弹出窗口

### 2.打开窗口 执行web/win/index.html

利用rpc接口，通知electron进程 打开一个browerwindow

这个窗口执行web/win/index.html 他内部包含一个websocket client

而本库实现了一个websocket server

通过websocket 可以指挥这个窗口做事，主要是jseval

### 3.窗口中提供了一个 __api.sendback([])供js 向dotnet core 发送内容
比如点击事件

### 4.协同退出
情况一
当本库所在的进程退出，websocket server 失活，
本程序打开的browserwindow 同时关闭
而electron进程设定为其打开的所有browserwindow关闭时 退出进程

则全部都关闭

情况二
当用户关闭了窗口，会导致该窗口对应的websocket断开，可知，可做适当处理。
当用户关闭了所有窗口，可知，如需要则同时退出进程


