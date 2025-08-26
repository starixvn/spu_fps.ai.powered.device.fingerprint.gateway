# Swagger API Documentation

## Cách sử dụng Swagger UI

### 1. Chạy ứng dụng
```bash
dotnet run
```

### 2. Truy cập Swagger UI
Mở trình duyệt và truy cập: `https://localhost:7001/swagger`

### 3. Các API Endpoints có sẵn

#### Device Fingerprint APIs
- **GET** `/api/DeviceFingerprint` - Lấy danh sách tất cả fingerprints
- **GET** `/api/DeviceFingerprint/{deviceId}` - Lấy thông tin fingerprint của thiết bị
- **POST** `/api/DeviceFingerprint` - Tạo fingerprint mới cho thiết bị
- **PUT** `/api/DeviceFingerprint/{deviceId}` - Cập nhật thông tin fingerprint
- **DELETE** `/api/DeviceFingerprint/{deviceId}` - Xóa fingerprint của thiết bị

#### Weather Forecast APIs (mẫu)
- **GET** `/WeatherForecast` - Lấy dự báo thời tiết

### 4. Test API trong Swagger UI

1. Chọn endpoint bạn muốn test
2. Click "Try it out"
3. Nhập thông tin cần thiết (nếu có)
4. Click "Execute"
5. Xem kết quả response

### 5. Cấu hình Swagger

Swagger được cấu hình trong `Program.cs`:
- Chỉ hiển thị trong môi trường Development
- Title: "FPS AI Powered Device Fingerprint Gateway API"
- Version: "v1"
- Route prefix: "/swagger"

### 6. OpenAPI JSON

Bạn cũng có thể truy cập OpenAPI specification tại:
- `/swagger/v1/swagger.json`

### 7. Tùy chỉnh thêm

Để thêm các tính năng Swagger khác, bạn có thể:

1. **Thêm Authentication**:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});
```

2. **Thêm XML Comments**:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

3. **Thêm Response Examples**:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters();
});
```
