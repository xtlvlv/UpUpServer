#!/bin/bash

# 定义程序名字
PROGRAM_NAME="ServerTest.dll"
# 路径
PROGRAM_PATH="/root/UpUpServer/net7.0"

# 找到进程id
pid=`ps -ef | grep ${PROGRAM_NAME} | grep -v grep | awk '{print $2}'`

# 杀死进程
kill -9 ${pid}

# 提示用户，可以根据需要自定义提示信息
echo "pid=${pid} 程序已停止"