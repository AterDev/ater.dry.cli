# Droplet CLI

**droplet** 是一个智能代码辅助工具，主要提供代码生成功能，它可以分析您的实体，智能的帮助您生成相关的数据传输对象、数据库读写操作以及API接口。

它作为`dotnet`命令行工具提供，同时支持`Web UI`操作界面。

## 特性

- 基于实体模型的智能分析，了解用户的业务意图
- 智能生成DTO文件，包括增加、更新、查询、列表等常用DTO
- 智能生成数据操作及业务逻辑实现，包括常见的新增、更新、筛选功能
- 生成部分测试代码
- 生成控制器接口
- 根据Swagger OpenApi生成用于Typescript的接口类型
- 根据Swagger OpenApi生成用于Angular或Axios的请求服务
- Angular基础CURD页面
- 其他更多高级功能

### 对ASP.NET Core的支持

droplet 命令工具可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：

- Dto文件，增加、更新、查询、列表等Dto文件
- 仓储文件，数据仓储层代码
- 控制器文件
- Protobuf文件
- 客户端请求服务

### 对Typescript的支持

对于前端，可以根据swagger OpenApi的json内容，生成请求所需要的代码(.ts)，包括：

- 请求服务,`xxx.service.ts`
- 接口模型,`xxx.ts`

## 项目模板支持

可使用[ater.web.templates](https://www.nuget.org/packages/ater.web.templates)项目模板，建议配合使用！

## 安装前提

- 安装[`.NET SDK`](https://dotnet.microsoft.com/zh-cn/download)

## 版本

首先检查包版本，工具依赖.NET SDK,对应关系如下：

|Package Version|.NET SDK Version|支持|
|-|-|-|
|6.x|6.0+|不再支持|
|7.x|7.0+|当前版本|
|8.x|8.0+|未来支持|

## 安装工具

使用`dotnet tool`命令安装：

```pwsh
dotnet tool install --global ater.droplet.cli
```

可到[nuget](https://www.nuget.org/packages/ater.droplet.cli)中查询最新版本！

> [!TIP]
> 模板的主版本与.NET的主版本号保持一致，如要安装模板的7.x版本，则需要`.NET 7.x`。

## 使用

### ⭐使用图形界面

一条命令启动UI界面!

```pwsh
droplet studio
```

### 使用命令行

你可以使用`droplet --help` 查看命令帮助信息。

或者使用`droplet [command] --help` 查看具体命令帮助信息。
