# 说明
> This is a command line tool, specifically for ` ASP NET Core 'project provides front and rear code generation.

这是一个命令行工具，专门针对`ASP.NET Core`项目提供前后端的代码生成。

> It can help developers generate common code templates according to entity models (. cs files), including:
> - Dto files, add, update, query, list and other Dto files
> - Warehouse document, data warehouse layer code
> - Controller files
> - Typescript interface type
> - Angular or Axios request service
> - Angular Basic CURD Page

可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：
- Dto文件，增加、更新、查询、列表等Dto文件
- 仓储文件，数据仓储层代码
- 控制器文件
- Typescript 接口类型
- Angular或Axios 的请求服务
- Angular基础CURD页面

## 文档
> This command line tool is integrated by [ater. web. templates](https://www.nuget.org/packages/ater.web.templates).It is recommended to use this project template directly to create a Web project.

本命令行工具在[ater.web.templates](https://www.nuget.org/packages/ater.web.templates)项目模板中集成，建议直接使用该项目模板创建Web项目。

## 安装
> If you create a project with 'ater. web. templates' 7.0 and above,' droplet 'has been integrated into the project, so you do not need to install it.

如果使用`ater.web.templates`7.0及以上版本创建项目，项目中已集成`droplet`，无需安装。

> If you want to use it alone, please use the 'dotnet tool' command to install it, such as:

如果你想单独使用，请使用`dotnet tool`命令安装，如：
```
dotnet tool install --global ater.droplet.cli --version 6.2-beta2
```

请到[nuget](https://www.nuget.org/packages/ater.droplet.cli)中查询最新版本！

## 使用
> Please use `droplet --help` to view command help information.
>
> Use `droplet [command] --help` to view specific command help information.
>
> You can also view [Online Documentation](https://github.com/AterDev/ater.docs/tree/dev/cn/droplet%20cli).


请使用`droplet --help` 查看命令帮助信息。

使用`droplet [command] --help` 查看具体命令帮助信息。

也可以查看[在线文档](https://github.com/AterDev/ater.docs/tree/dev/cn/droplet%20cli)。

