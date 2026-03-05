# DryGen Node CLI

纯 Node.js 实现的 OpenAPI 客户端代码生成工具。

目标：复用现有输出规范（目录/命名/基础模板），但不再依赖 `node-api-dotnet` 或 .NET runtime。

## 命令

- `drygen request <pathOrUrl> <outputPath> [-t|--type angular|axios] [-m|--only-model]`

示例：

- `drygen request ./openapi.json ./src -t angular`
- `drygen request https://example.com/swagger/v1/swagger.json ./src -t axios`
- `drygen request ./openapi.json ./src -t angular --only-model`

## 生成内容

- `services/<clientName>/base.service.ts`
- `services/<clientName>/models/**/*.model.ts`
- Angular: `services/<clientName>/services/*.service.ts` + `services/<clientName>/<clientName>-client.ts`
- Axios: `services/<clientName>/services/*.service.ts`
- Angular 额外生成：`pipe/<clientName>/enum-text.pipe.ts`

## 说明

- 默认 `--type angular`
- `--only-model` 仅生成模型文件
- 仅支持 `angular` 与 `axios`
