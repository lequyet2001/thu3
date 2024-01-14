# Hướng dẫn Cài đặt và Chạy Dự Án

## Cài đặt Redis Server

### Trên Ubuntu

1. Mở terminal và chạy các lệnh sau để cài đặt Redis Server:
   ```
   sudo apt update
   sudo apt install redis-server
   ```
2. Start server:
   ```
   sudo service redis-server start
   ```

## Tạo database
 
  - (https://github.com/lequyet2001/thu3/blob/master/SQL.sql)
 
## Clone dự project 
 ```
    git clone https://github.com/lequyet2001/thu3.git
 ```

### Mở file thu3.sln để mở dự án trên visual studio
* <a style="color:blue; text-decoration:none;">  [thu3.sln](https://github.com/lequyet2001/thu3/blob/master/thu3.sln)</a>
 
### Sửa file appsettings.json

  ```
  "ConnectionStrings": 
    {
       "db": "connect your string with sql server",
       "Redis": "localhost:6379"
    }
  ```

  

