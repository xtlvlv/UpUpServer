#!/bin/bash

# 安装步骤
# 编译release版本，压缩成zip包，上传到服务器UpUpServer目录下
# 执行 sh install.sh 自动解压
# 执行 sh run.sh 启动程序
# 执行 sh stop.sh 停止程序

# 路径
PROGRAM_PATH="/root/UpUpServer/net7.0"

# 把 /root/UpUpServer/net7.0/logs 目录下的文件拷贝到 /root/UpUpServer/logs 目录下, 如果没有logs目录，就创建logs目录
mkdir -p /root/UpUpServer/logs
cp -r ${PROGRAM_PATH}/logs/* /root/UpUpServer/logs

rm -rf ${PROGRAM_PATH}

unzip /root/UpUpServer/net7.0.zip -d /root/UpUpServer

