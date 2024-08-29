# 前端请求服务代码生成器

本工具是通过后端提供的`OpenApi`文档，通常是通过 `swagger` 生成的`json`文档，生成前端请求服务代码。

目前支持使用`axios`与`angular HttpClient`的请求类库生成的内容，其中包含了请求依赖的所有的`typescript`类型文件。

## 使用方法

### 安装工具

```bash
npm install -g ater.dry
```

### 使用工具

安装后，你可以使用`drygen`命令，目前只有一个命令`service`使用方法如下:

```bash
drygen service <url> -o ./output -t axios
```

其中

- url是`openapi`文档的地址
- `-o`是输出的目录
- `-t`是生成的请求类库，目前支持`axios`与`ngHttp`。


