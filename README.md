# Hướng dẫn Cài đặt và Chạy Dự Án

## Cài đặt Redis Server

### Trên Ubuntu

1. Mở terminal và chạy các lệnh sau để cài đặt Redis Server:
   ```bash
   sudo apt update
   sudo apt install redis-server
2. Start server:
   ```bash
   sudo service redis-server start
## Sửa file appsettings.json
 "ConnectionStrings": {
    "db": "connect your string with sql server",
    "Redis": "localhost:6379"}
  
