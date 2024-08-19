#!/bin/bash

# 定义程序名字
PROGRAM_NAME="ServerTest.dll"

# 路径
PROGRAM_PATH="/root/UpUpServer/net7.0"

cd ${PROGRAM_PATH}

# 在后台执行程序
nohup dotnet ${PROGRAM_NAME} > /dev/null 2>&1 &

# 提示用户，可以根据需要自定义提示信息
echo "C# 程序已在后台启动"