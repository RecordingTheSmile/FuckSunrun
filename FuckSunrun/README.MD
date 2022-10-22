# 阳光小助手
一款基于.NET平台开发的高可用阳光体育跑步平台。

目前支持的功能：
 - [x] 自动在指定时间运行跑步程序
 - [x] 程序重启不影响定时任务，重启期间未执行的任务将自动执行
 - [x] 任务执行失败自动重试
 - [x] 任务状态邮件通知
 - [x] 简单的多用户管理系统

设想中，未实现的功能：
- [ ] 支持更多任务状态推送方式
- [ ] 添加Docker支持
- [ ] 前端UI增加手机端适配

# 演示网站
https://sunrun.gq

# 部署方法
1. 下载`Release`中的压缩包并解压，或者`clone`本项目代码后自行使用`dotnet`编译。
2. 在项目根目录创建一个`appsettings.Production.json`文件，文件内容复制`appsettings.Development.json`文件。
文件中，`ConnectionStrings.PgSQL`按照演示填写PostgreSQL数据库连接信息，Redis填写Redis连接信息。
Smtp信息按照文件中的字段填写。
3. 部署到服务器上，运行。
4. 关于如何在服务器上安装dotnet运行时，请参见微软文档：
[如何在Windows，MacOS，Linux上安装.NET运行时](https://learn.microsoft.com/zh-cn/dotnet/core/install/)
。对大多数Linux发行版来说，执行两条命令就可以了，安装还是很容易的。
5. 关于如何在服务器上安装PostgreSQL，请参见：
[PostgreSQL官方下载页面](https://www.postgresql.org/download/)
6. 关于如何在服务器上安装Redis，请参见：
[Redis官方页面](https://redis.io/docs/getting-started/installation/)

# 技术信息
1. 平台：.NET 6
2. 数据库： PostgreSQL,Redis
3. 定时任务库：Hangfire
4. 前端框架：Layui + Pear Admin

# 协议
GPL

且附加条款：
不得以任何理由使用本项目从事盈利活动！

# 演示图片
![登录](https://i.niupic.com/images/2022/10/22/a9ET.png)
![仪表盘](https://i.niupic.com/images/2022/10/22/a9EW.png)
![任务添加](https://i.niupic.com/images/2022/10/22/a9EU.png)

# 交流群
目前仅有Telegram交流群，欢迎各位大佬进群指导。如有任何使用问题也欢迎进群交流或直接提交Issue！

[加群链接：@sunrun_helper](https://t.me/sunrun_helper)

![群QR码](https://i.niupic.com/images/2022/10/22/a9ES.jpg)