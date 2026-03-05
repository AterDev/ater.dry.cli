# Perigon Node CLI

基于 `node-api-dotnet` Native AOT 场景的 Node 命令行工具，提供 `request` 命令，参数与现有 C# `RequestCommand` 保持一致。

## 命令

- `perigon request <path|url> <outputPath> [-t|--type angular|axios|csharp] [-m|--only-model]`

## 说明

- 运行时加载的是 Native AOT 产物 `.node` 模块，目标机器不需要安装 .NET runtime。
- 若本地不存在 `.node` 产物，CLI 会自动执行 `dotnet publish` 生成（该步骤需要 .NET SDK，仅用于构建阶段）。
- 默认 `--type angular`。
- `--only-model` 为布尔开关，指定后仅生成模型文件。
- 可通过环境变量 `PERIGON_BRIDGE_MODULE` 指定桥接 `.node` 模块路径。
