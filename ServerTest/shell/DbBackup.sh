#!/bin/bash

# 根据参数执行不同的命令
# 1.日备份
# 2.周备份
# 3.月备份

# 获取参数
# $0 是脚本名字
# $1 是第一个参数 指定备份类型

if [ $1 -eq 1 ]; then
    # 日备份
    mongodump --db GameTest --out /root/MongoDBBackup/day$(date +"%Y-%m-%d")
elif [ $1 -eq 2 ]; then
    # 周备份
    mongodump --db GameTest --out /root/MongoDBBackup/week$(date +"%Y-%m-%d")
elif [ $1 -eq 3 ]; then
    # 月备份
    mongodump --db GameTest --out /root/MongoDBBackup/month$(date +"%Y-%m-%d")
else
    # 默认日备份
    mongodump --db GameTest --out /root/MongoDBBackup/default$(date +"%Y-%m-%d")
fi

# 删除7天前的备份
find /root/MongoDBBackup -mtime +7 -name "day*" -exec rm -rf {} \;