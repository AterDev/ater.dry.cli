# Update及studio更新

## 2026/09/01更新内容

### Update命令功能更新

- update命令中显示，现在分成了左右两栏，由于左边路径比较长，通常无法完整显示出来，修改成只显示文件名即可。
- update时，排除对`src/Definition/EntityFramework/Migrations/`目录下文件的对比。
- update时，排除对`src/Perigon/Perigon.AspNetCore/Constants/WebConst.cs`文件内容的对比.
- 按回车确认后，更新文件内容后，然后使用dotnet build，对解决方案进行编译，然后输出成功或错误信息。

### Studio 自定义模板和任务

关于studio中的功能，去除关于prompt(提示词)和模板相关的菜单和功能，以及生成任务和MCP相关功能。这些功能菜单都不需要了，后续将通过cli/mcp 方式提供相关功能。 使用者将通过skill机制利用AI来做相关的工作。

MCP在工具去除 ExecuteGenerateTaskAsync，因为不再有任务机制了。

## template skills

对Perigon.templates中的perigon skill进行更新

- 添加对update 命令的使用说明

## 文档更新

指更新 perigon.docs 仓库文档内容。

- update 命令是用来对比当前项目与最新模板的差异，然后进行替换升级使用的。在文档中要说明，如何使用它进行升级，以及它升级时的逻辑，如有的覆盖，有的不覆盖，对比内容主要包含了.agents和基础架构内容。
- 更新小版本，在小版本更新记录中，描述关于studio中对自定义模板和任务的更新情况(移除)，用户可以通过mcp 去实现模板的创建和代码的生成。