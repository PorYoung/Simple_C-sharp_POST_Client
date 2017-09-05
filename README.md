# Simple_C-sharp_POST_Client
### 简介
A simple C# post client written by WPF.   

一个由WPF编写的简单的C# POST请求客户端，其主要功能是向192.168.3.5（校园网验证服务器）发出一个POST请求。   

模拟用户登录的过程，进而实现了校园网登录验证的功能。   

使用注册表保存用户名和密码，并做了简单的静默安装与卸载、限制客户端多开。   

### 目录导航
bin/Debug 目录下包含了可执行的exe文件   

MainWindow.xaml.cs 里包含了主要的代码   

Shortcut.cs 是一个生成桌面快捷方式的类（因为C#没有直接生成桌面快捷方式的API可以调用）   

其他文件大多由Visual Studio创建WPF项目时默认生成的   
