# 说明

这是一个命令行工具，专门针对`ASP.NET Core`项目提供前后端的代码生成。

可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：
- Dto文件，增加、更新、查询、列表等Dto文件
- 仓储文件，数据仓储层代码
- 控制器文件
- Typescript 接口类型
- Angular或Axios 的请求服务
- Angular基础CURD页面

## 文档

本命令行工具在[ater.web.templates](https://www.nuget.org/packages/ater.web.templates)项目模板中集成，建议直接使用该项目模板创建Web项目。

## 安装

如果使用`ater.web.templates`7.0及以上版本创建项目，项目中已集成`droplet`，无需安装。

如果你想单独使用，请使用`dotnet tool`命令安装，如：
```
dotnet tool install --global ater.droplet.cli --version 6.2-beta2
```
请到[nuget](https://www.nuget.org/packages/ater.droplet.cli)中查询最新版本！

## 使用
请使用`droplet --help` 查看命令帮助信息。

使用`droplet [command] --help` 查看具体命令帮助信息。

也可以查看[在线文档](https://github.com/AterDev/ater.docs/tree/dev/cn/droplet%20cli)。

