# 说明
> This is a code assistance tool that provides code generation functions for `ASP.NET Core` projects and `Typescript front-end` projects, and supports command line and graphical interface operations. It is recommended to use the [Ater.web](https://github.com/AterDev/ater.web) template to create projects

这是一个代码辅助工具，面向`ASP.NET Core`项目以及`Typescript前端`项目提供代码生成的功能，支持命令行与图形界面操作。请使用 [Ater.web](https://github.com/AterDev/ater.web) 模板创建项目。

> It can help developers generate common code templates according to entity models (. cs files), including:
> - Dto files, add, update, query, list and other Dto files
> - Data store layer:entity DbSet wrapper
> - Manager layer:business implementation code
> - Controller API
> - Typescript interface type
> - Angular or Axios request service
> - Angular Basic CURD Page

可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：
- Dto文件，增加、更新、查询、列表等Dto文件
- 数据仓储层代码,对实体的数据操作封装
- Manager层，业务实现代码
- 生成部分测试代码
- 控制器接口
- Typescript 接口类型
- Angular或Axios 的请求服务
- Angular基础CURD页面


## 安装
> Check package version first!

首先检查包版本，工具依赖.NET SDK,对应关系如下：

|Package Version|.NET SDK Version|
|-|-|
|6.x|6.0|
|7.x|7.0|

> If you want to use it alone, please use the 'dotnet tool' command to install it, such as:

如果你想单独使用，请使用`dotnet tool`命令安装，如：
```
dotnet tool install --global ater.droplet.cli
```

> please use the latest version from [nuget](https://www.nuget.org/packages/ater.droplet.cli)!

请到[nuget](https://www.nuget.org/packages/ater.droplet.cli)中查询最新版本！

## 使用
> One command to start the UI interface and enjoy it!

一条命令启动UI界面!
```pwsh
droplet studio
```

> Show entities and generate codes!

查看实体模型，并根据实体模型生成相应的代码:

![entities](./images/code%20generate.png)


> generate actions

选择要生成的内容

![generate-actons](./images/generate%20actions.png)

> edit dtos

在线编辑dto

![edit dtos](./images/edit%20dtos.png)

> front-end support

前端内容生成

![front-end](./images/front-end.png)


> You can also use `droplet --help` to view command help information.
>
> Use `droplet [command] --help` to view specific command help information.
>
> You can also view [Online Documentation](https://github.com/AterDev/ater.docs/tree/dev/cn/droplet%20cli).


你可以使用`droplet --help` 查看命令帮助信息。

或者使用`droplet [command] --help` 查看具体命令帮助信息。

也可以查看[在线文档](https://github.com/AterDev/ater.docs/tree/dev/cn/droplet%20cli)。

