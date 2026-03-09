# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具，其中包含Blazor Server和MCP服务。请在生成代码时遵循以下指导：

## General Guidelines
- 准确性和确定性是最重要的，否则宁愿不回答。
- 没有明确要求的情况下，不要私自构建项目来验证代码是否正确，而是通过IDE提供的各种错误提示和警告来判断代码的正确性。
- 本项目的本地化包括中英文；在Razor中使用由源生成器生成的`Localizer.<Key>`常量；

## 技术栈和语言偏好
- 主要语言是:C#
- AterStudio项目是 ASP.NET Core和Blazor Server项目
- 前端使用Fluentui-blazor组件库

## 重要的文件和目录
- `src/Apps/CommandLine`: 是命令行程序。
- `src/Definition/CodeGenerator`: 使用roslyn解析和实现代码生成逻辑
- `src/Definition/Entity`: 实体模型项目
- `src/Apps/Dashboard`: 是 Studio命令的 Web的服务项目，基于ASP.NET Core和Blazor Server。
- `src/Modules/CoreMod`: 是业务实现的主要模块，CoreStudio直接引用该项目。
