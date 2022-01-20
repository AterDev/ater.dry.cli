# cli
cli tools
## 实体模型解析

## 常用Dto类型
- UpdateDto
  - 过滤列表属性,如`List<string>`,`List<Blog>`
  - 转换非列表非空导航属性，如 `Blog` 使其变成 `Name+Id` 形式，即 `BlogId`
  - 除Required特性外的属性，都设置为可空类型
- FilterDto
  - 具有Required 属性
  - 非列表且不为空,如 `public string Title`
  - 过滤列表及导航属性
  - 过滤`Id,CreatedTime,UpdatedTime`等名称，基类处理
  - 转换非列表导航属性的Id,如 `UserId`
- ItemDto 作为数组中的元素时
  - 过滤Content字段
  - 过滤超过1000长度的字段
  - 过滤列表
  - 过滤导航字段
  - **字段可空要与原类型保持一致**
- ShortDto 
  - 过滤`Content`字段
  - 过滤超过2000长度的字段
  - 过滤列表导航属性，如`List<Blog>`

## 数据仓储接口操作
- 添加
- 删除
- 更新
- 筛选查询
- 筛选查询并分页
- 根据id查询一个
- **根据ids查询多个**
- 是否存在
- 批量更新
- 批量删除
- 批量添加

### 数据仓储生成
- 在`Interface`目录，生成`IDataStore.cs`，提供接口
- 在`DataStore`目录，生成`DataStoreBase.cs`，接口的实现
- 生成对应 `DataStore`
  - 重写列表及分页查询
  - 重写删除
  - 重写更新
  - 重写添加
- 重新生成`DataStoreExtensions`,扫描所有Store，用来进行仓储服务注册

### Rest API生成
- 
## 命令

### Dto生成命令
`dto entityPath [--output --force]`
- 默认跳过已生成
- 强制覆盖选项
- 生成`GlobalUsing.cs`，以引用依赖
- 生成基础类，如`FilterBase`,`PageResult`

### API服务生成
`api entityPath [--dtoPath --storePath --output --contextName --type]`
- 参数说明
  - contextName，使用EF时的 数据库上下文名称，不指定将会自动搜索，默认为'ContextBase'
- 生成DTO
- 生成DataStore
- 生成ApiController
- 支持多种类型(`Rest`,`GRPC`,`GraphQL`)生成

### gRPC服务生成

### 配置项
- 项目目录
- dto所在项目目录

### 功能
前提：先进行配置，如没有配置则提示。

