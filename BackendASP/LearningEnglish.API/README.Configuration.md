# Configuration Files

This project uses multiple `appsettings.json` files for different environments:

## Files Structure

- **`appsettings.json`** - Base configuration template (committed to Git)
- **`appsettings.Development.json`** - Local development settings (ignored by Git)
- **`appsettings.Docker.json`** - Docker container settings (ignored by Git)
- **`appsettings.Example.json`** - Example configuration for reference

## Setup Instructions

1. Copy `appsettings.json` to `appsettings.Development.json`:
   ```bash
   cp appsettings.json appsettings.Development.json
   ```

2. Update `appsettings.Development.json` with your local credentials:

### Required Configuration

#### Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=YourDatabase;Username=postgres;Password=YourPassword"
}
```

#### JWT Settings
```json
"Jwt": {
  "Key": "Your-Secret-Key-Minimum-32-Characters",
  "Issuer": "ELearningEnglish",
  "Audience": "ELearningEnglishAudience"
}
```

#### SMTP (Email)
```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "User": "your-email@gmail.com",
  "Password": "your-app-password",
  "EnableSsl": true,
  "FromName": "E-Learning English Platform"
}
```

#### Azure Speech API
Get your keys from: https://portal.azure.com
```json
"AzureSpeech": {
  "SubscriptionKey": "your-azure-speech-key",
  "Region": "southeastasia",
  "Endpoint": "https://southeastasia.api.cognitive.microsoft.com/"
}
```

#### MinIO Object Storage
```json
"MinIO": {
  "Endpoint": "your-minio-endpoint.com",
  "AccessKey": "your-access-key",
  "SecretKey": "your-secret-key",
  "UseSSL": true
},
"Minio": {
  "BaseUrl": "https://your-minio-console.com"
}
```

#### Oxford Dictionary API
Get your keys from: https://developer.oxforddictionaries.com/
```json
"OxfordDictionary": {
  "BaseUrl": "https://od-api-sandbox.oxforddictionaries.com/api/v2",
  "AppId": "your-oxford-app-id",
  "AppKey": "your-oxford-app-key"
}
```

#### Unsplash API
Get your keys from: https://unsplash.com/developers
```json
"Unsplash": {
  "ApplicationId": "your-unsplash-app-id",
  "AccessKey": "your-unsplash-access-key",
  "SecretKey": "your-unsplash-secret-key",
  "BaseUrl": "https://api.unsplash.com"
}
```

#### Cloudinary (Optional)
```json
"Cloudinary": {
  "CloudName": "your-cloud-name",
  "ApiKey": "your-api-key",
  "ApiSecret": "your-api-secret"
}
```

## Docker Configuration

For Docker deployment, create `appsettings.Docker.json` with:
- Database host should be `db` (service name in docker-compose)
- All other services should use container-resolvable hostnames

## Security Notes

⚠️ **NEVER commit files containing real credentials:**
- `appsettings.Development.json`
- `appsettings.Docker.json`
- `appsettings.Production.json`

These files are automatically ignored by `.gitignore`.

## Environment Priority

ASP.NET Core loads settings in this order (later overrides earlier):
1. `appsettings.json` (base template)
2. `appsettings.{Environment}.json` (Development/Docker/Production)
3. Environment variables
4. Command-line arguments
