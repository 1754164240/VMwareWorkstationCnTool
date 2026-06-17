# VMware Workstation 一键汉化工具

一个用于 VMware Workstation Pro 的 Windows 一键汉化小工具。

工具内置旧版 `messages\zh_CN` 语言文件，运行后会自动检测 VMware Workstation 安装目录，将内置的 `zh_CN` 复制到新版安装目录的 `messages` 文件夹中，并写入 `pref.locale = "zh_CN"` 到 VMware 偏好配置。

## 功能

- 一键汉化 VMware Workstation Pro
- 内置 `zh_CN` 语言包，无需手动准备语言文件
- 自动检测 VMware Workstation 安装目录
- 检测不到安装目录时支持手动选择
- 显示当前检测到的 VMware Workstation 版本
- 打开最新版 VMware Workstation 下载页
- 直接下载指定仓库最新 Release 中的 Windows exe 安装包
- 自动写入 `%APPDATA%\VMware\preferences.ini`
- 覆盖已有 `zh_CN` 前会自动备份
- 单文件绿色版发布，双击即可运行
- 请求管理员权限，便于写入安装目录

## 使用方法

1. 下载 `VMwareWorkstation汉化工具.exe`
2. 双击运行，按提示授予管理员权限
3. 确认检测到的 VMware Workstation 安装目录和版本
4. 点击“一键汉化”
5. 重启 VMware Workstation

如果没有自动检测到安装目录，可以点击“选择...”手动选择 VMware Workstation 的安装目录。

如果需要下载最新版 VMware Workstation，可以点击：

- “打开下载页”：跳转到 <https://github.com/201853910/VMwareWorkstation/releases/>
- “下载最新版”：自动读取该仓库最新 Release，并下载其中的 Windows `.exe` 安装包

## 汉化逻辑

程序会执行以下操作：

1. 从程序内置资源中读取 `zh_CN` 语言包
2. 定位 VMware Workstation 安装目录
3. 将 `zh_CN` 解压到：

```text
<VMware Workstation 安装目录>\messages\zh_CN
```

4. 修改当前用户的偏好配置：

```text
%APPDATA%\VMware\preferences.ini
```

并确保存在：

```ini
pref.locale = "zh_CN"
```

## 项目结构

```text
.
├─ VMwareWorkstationCnTool
│  ├─ WinForms 主程序
│  ├─ app.ico
│  └─ app.manifest
├─ VMwareWorkstationCnTool.Core
│  ├─ 安装目录检测
│  ├─ 语言包安装
│  ├─ preferences.ini 写入
│  └─ Resources\zh_CN.zip
├─ VMwareWorkstationCnTool.Tests
│  └─ 自动化测试
└─ publish
   └─ VMwareWorkstation汉化工具.exe
```

## 开发环境

- Windows
- .NET 9 SDK
- Windows Forms
- xUnit

## 构建

还原、测试和构建：

```powershell
dotnet restore
dotnet test .\VMwareWorkstationCnTool.Tests\VMwareWorkstationCnTool.Tests.csproj
dotnet build .\VMwareWorkstationCnTool.sln -c Release
```

发布单文件绿色版：

```powershell
dotnet publish .\VMwareWorkstationCnTool\VMwareWorkstationCnTool.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o .\publish
```

当前默认发布配置为：

- `SelfContained=true`
- `PublishSingleFile=true`
- `EnableCompressionInSingleFile=true`
- `RuntimeIdentifier=win-x64`

因此发布出的 exe 不需要用户提前安装 .NET 运行时。

## 关于体积

当前绿色版 exe 约 50 MB。

体积主要来自内置的 .NET 9 Windows Desktop Runtime 和 `zh_CN` 语言包。为了保证用户下载后双击即可运行，项目默认采用自包含发布。

如果想进一步缩小到约 2 MB，可以改为框架依赖发布：

```powershell
dotnet publish .\VMwareWorkstationCnTool\VMwareWorkstationCnTool.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true `
  -o .\publish
```

但这种方式要求用户电脑已安装 .NET 9 Desktop Runtime。

## 测试

运行：

```powershell
dotnet test .\VMwareWorkstationCnTool.Tests\VMwareWorkstationCnTool.Tests.csproj
```

测试覆盖：

- VMware 安装目录检测
- 手动目录校验
- 当前版本读取
- 最新 VMware Workstation Release 解析
- Windows exe 安装包选择和下载
- 内置语言包读取
- `zh_CN` 安装和备份
- `preferences.ini` 写入
- 完整一键汉化流程

## 注意事项

- 本工具仅适用于 Windows。
- 写入 VMware 安装目录通常需要管理员权限。
- 汉化后需要重启 VMware Workstation 才能生效。
- 如果 VMware 官方文件结构变化，可能需要更新语言包或适配逻辑。
- 使用前建议关闭正在运行的 VMware Workstation。

## 免责声明

本项目仅用于学习和个人使用。VMware Workstation 及相关商标归其权利人所有。请确保你拥有合法的软件使用授权，并自行承担使用本工具产生的风险。
