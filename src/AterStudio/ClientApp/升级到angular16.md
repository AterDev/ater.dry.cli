# 升级到Angular 16

Angular 16版本带来了不少重大的变更，但官方的升级帮助页面却出现了重要的失误，缺失重要的升级步骤说明。本篇文章就介绍如何更新到最新的版本，以及如何快速的提高的程序的编译速度。

## 升级

以下示例演示，如何从Angular14 升级到Angular16。

### 修改包版本

升级到Ng16 有些前提条件

- Node.js versions: v16 and v18.
- TypeScript version 4.9.3 or later.
- Zone.js version 0.13.x or later.

你可以手动修改`package.json`文件中的内容

```json
"zone.js": "~0.13.0"
"typescript": "~5.0.0"

```

然后运行`pnpm install`或`npm install`。

> [!TIP]
> 如果安装过程中出错，可尝试删除node_modules文件夹，然后重新尝试

### 检测更新

使用`ng update`进行更新检测，工具会提示你哪些内容需要升级，然后按照提示执行相应的命令，通常包括：

-
