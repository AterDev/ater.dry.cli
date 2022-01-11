# cli
cli tools
## 实体模型解析

## 常用Dto类型
- UpdateDto
  - 过滤列表属性,如`List<string>`,`List<Blog>`
  - 转换非列表非空导航属性，如 `Blog` 使其变成 `Name+Id` 形式，即 `BlogId`
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

## 生成Dto
- 默认跳过已生成
- 强制覆盖选项
- 生成`GlobalUsing.cs`，以引用依赖
- 生成基础类，如`FilterBase`

## 数据仓储接口操作
- 添加
- 删除
- 更新
- 筛选查询
- 详情
- 是否存在
- 批量更新
- 批量删除
- 批量添加

## 命令

### 配置项
- 项目目录
- dto所在项目目录

### 功能
前提：先进行配置，如没有配置则提示。

