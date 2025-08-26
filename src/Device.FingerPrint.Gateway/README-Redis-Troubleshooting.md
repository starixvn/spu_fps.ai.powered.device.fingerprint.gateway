# Hướng dẫn khắc phục sự cố Redis

## Vấn đề thường gặp

### 1. Lỗi SocketClosed
**Triệu chứng:**
```
Redis connection error SocketClosed
StackExchange.Redis.RedisConnectionException: SocketClosed
```

**Nguyên nhân:**
- Kết nối mạng không ổn định
- Redis server bị restart
- Timeout settings quá thấp
- Connection pool không đủ

**Giải pháp:**
- Tăng `ConnectTimeout` và `ConnectRetry`
- Tăng `PoolSize`
- Kiểm tra kết nối mạng đến Redis server

### 2. Lỗi Build - RedisConfiguration Properties
**Triệu chứng:**
```
error CS1061: 'RedisConfiguration' does not contain a definition for 'ResponseTimeout'
error CS1061: 'RedisConfiguration' does not contain a definition for 'KeepAlive'
```

**Nguyên nhân:**
- Sử dụng các properties không tồn tại trong RedisConfiguration
- Thiếu package StackExchange.Redis

**Giải pháp:**
- Thêm package: `dotnet add package StackExchange.Redis --version 2.7.17`
- Chỉ sử dụng các properties hợp lệ: `ConnectTimeout`, `ConnectRetry`, `AbortOnConnectFail`, `PoolSize`

### 3. Cấu hình đã được cải thiện

#### appsettings.json
```json
{
  "Redis": {
    "ConnectTimeout": 10000,
    "ConnectRetry": 10,
    "PoolSize": 50,
    "AbortOnConnectFail": false,
    "Database": 14,
    "Expired": 604800,
    "Hosts": [
      {
        "Host": "10.18.200.194",
        "Port": "16410"
      }
    ]
  }
}
```

#### Tính năng mới đã thêm:
1. **Retry Logic**: Tự động thử lại khi gặp lỗi kết nối (tối đa 3 lần)
2. **Health Check Middleware**: Kiểm tra sức khỏe Redis trước mỗi request
3. **Graceful Error Handling**: Xử lý lỗi mà không làm crash ứng dụng
4. **Connection Pool Management**: Quản lý pool kết nối hiệu quả hơn
5. **Exponential Backoff**: Delay tăng dần giữa các lần retry

### 4. Monitoring và Logging

#### Health Check Endpoint
```
GET /healthz
```

#### Log Levels
- **Warning**: Khi Redis connection có vấn đề nhưng vẫn hoạt động
- **Error**: Khi Redis hoàn toàn không khả dụng
- **Info**: Khi kết nối được khôi phục

### 5. Best Practices

#### Production Environment
1. **Redis Cluster**: Sử dụng Redis cluster thay vì single instance
2. **Connection Pooling**: Đảm bảo pool size phù hợp với load
3. **Monitoring**: Sử dụng Prometheus metrics để monitor Redis
4. **Circuit Breaker**: Implement circuit breaker pattern

#### Development Environment
1. **Local Redis**: Sử dụng Docker Redis cho development
2. **Connection String**: Sử dụng connection string thay vì separate config
3. **Logging**: Enable detailed logging cho debugging

### 6. Commands hữu ích

#### Kiểm tra Redis connection
```bash
redis-cli -h 10.18.200.194 -p 16410 ping
```

#### Monitor Redis operations
```bash
redis-cli -h 10.18.200.194 -p 16410 monitor
```

#### Kiểm tra Redis info
```bash
redis-cli -h 10.18.200.194 -p 16410 info
```

### 7. Troubleshooting Steps

1. **Kiểm tra kết nối mạng**
   ```bash
   telnet 10.18.200.194 16410
   ```

2. **Kiểm tra Redis server status**
   ```bash
   redis-cli -h 10.18.200.194 -p 16410 ping
   ```

3. **Kiểm tra logs ứng dụng**
   - Tìm các log liên quan đến Redis connection
   - Kiểm tra error patterns

4. **Kiểm tra metrics**
   - Prometheus metrics cho Redis operations
   - Connection pool statistics

### 8. Emergency Procedures

#### Khi Redis hoàn toàn không khả dụng:
1. Ứng dụng sẽ trả về HTTP 503 cho các request cần Redis
2. Các operation không critical sẽ tiếp tục hoạt động
3. Connection pool sẽ tự động thử kết nối lại
4. Logs sẽ ghi lại tất cả các lỗi để phân tích

#### Recovery Steps:
1. Kiểm tra Redis server status
2. Restart Redis service nếu cần
3. Kiểm tra network connectivity
4. Monitor logs để đảm bảo kết nối được khôi phục

### 9. Build và Deployment

#### Để build thành công:
1. Đảm bảo có package StackExchange.Redis:
   ```bash
   dotnet add package StackExchange.Redis --version 2.7.17
   ```

2. Chỉ sử dụng các properties hợp lệ trong RedisConfiguration:
   - `ConnectTimeout`
   - `ConnectRetry`
   - `AbortOnConnectFail`
   - `PoolSize`
   - `Database`
   - `Expired`
   - `Hosts`

3. Build project:
   ```bash
   dotnet build
   ```

#### Warnings có thể bỏ qua:
- CS8618: Non-nullable property warnings (có thể fix bằng cách thêm `required` modifier)
- ASP0014: UseEndpoints warning (không ảnh hưởng đến functionality)
