README.md


FileHub - CW-AMD201
Overview
FileHub is a multi-service file-sharing system that simulates a SaaS upload/download product with security controls, access restrictions, and storage quotas.

The project is designed to separate functionality into dedicated services:

AuthService.API: user registration/login, JWT authentication, and OTP password reset.

FileService.API: file upload/download management, MongoDB metadata, Firebase Storage, and file access permissions.

Gateway: Ocelot API Gateway that centralizes endpoints and forwards requests to the appropriate service.

Frontend (fe): React + Vite SPA for uploading, managing, and downloading shared files.

Overall Architecture
The frontend and APIs operate with the following structure:

Browser
   |
   | (HTTP/HTTPS)
   v
Frontend React (http://localhost:7070)
   |
   | (proxy/gateway)
   v
API Gateway Ocelot (https://localhost:7000)
   |-- /api/auth/* -> AuthService.API (https://localhost:7001)
   |-- /api/files/* -> FileService.API (https://localhost:7002)
This architecture provides:

Clear separation between authentication and file services.

The ability to change the backend without modifying the frontend.

Support for multiple deployment environments through ocelot.json and ocelot.Render.json.

Project Goals
Registration & authentication using email and password.

JWT authentication for protected endpoints.

Password reset using OTP sent by email.

File upload with optional password protection, expiration date, and download limit.

Secure download using password protection and download-count limits.

File management: view files, delete files, and update metadata.

Firebase Storage integration for file storage.

React frontend integration with the backend through the API Gateway.

Main Components
1. AuthService.API
Responsibilities:

Register new users.

Authenticate users during login and issue JWT tokens.

Handle password-reset requests, generate OTPs, and send emails.

Validate JWT tokens for protected endpoints.

Important controllers:

AuthController: register, login, request-password-reset, reset-password, and authorization checks.

EmailController: email-sending test endpoint.

OtpServiceController: create/clear OTP operations.

OtpCodesController and UsersController: supporting CRUD operations.

Technologies:

ASP.NET Core Web API

MongoDB C# Driver

BCrypt for password hashing

JWT Bearer for endpoint security

Mail service for sending OTP emails

2. FileService.API
Responsibilities:

Upload files and store metadata.

Store file data in MongoDB Atlas.

Store binary files in Firebase Storage.

Check file access permissions: password, expiration date, and download limit.

Return an image thumbnail when the uploaded file is an image.

Main controllers/services:

FilesController: upload, list, file details, password verification, download, thumbnail, access update, and delete.

FileService: business logic for upload/download/verify/update/delete.

StorageService: upload/download files to Firebase or local storage.

ThumbnailService: generate PNG thumbnails for images.

UploadLimitService: control storage quota and file-size limits.

3. API Gateway
All requests are routed through Ocelot:

/api/auth/* -> AuthService.API

/api/files/* -> FileService.API

The Gateway uses three main configuration files:

Gateway/ocelot.json: local development.

Gateway/ocelot.Docker.json: Docker.

Gateway/ocelot.Render.json: production deployment.

Gateway CORS:

Allows the frontend at http://localhost:7070 or https://cw-amd201.onrender.com.

4. Frontend (fe)
React + Vite SPA providing:

File upload form.

User file list.

File sharing.

Password verification before downloading.

The frontend uses:

axios with JWT/cookie

zustand for state management

react-router for routing

tailwindcss and shadcn UI

Main Data Flows
Registration & Login
The user sends POST /api/auth/register.

AuthService creates a new user in MongoDB.

The user logs in using POST /api/auth/login.

AuthService verifies the password and returns a JWT token.

The frontend uses the token to call protected endpoints.

Password Reset with OTP
The user submits their email to POST /api/auth/request-password-reset.

AuthService generates an OTP and stores it in MongoDB.

The mail service sends the OTP to the user's email.

The user sends POST /api/auth/reset-password with their email, OTP, and new password.

If the OTP is valid, the new password is hashed and stored.

File Upload
The frontend sends POST /files/upload with multipart/form-data.

FileService checks the file size and storage quota.

The binary file is stored in Firebase Storage.

A thumbnail is generated if the file is an image.

File metadata is stored in MongoDB.

The API returns file information (DTO) to the frontend.

File Download
The user opens a shared link or the frontend calls /files/{id}/download.

If the file is password-protected, /files/{id}/verify-password is called first.

FileService checks the expiration date, download limit, and password.

If valid, the file stream is returned and downloadCount is increased.

Main Data Models
AuthService.API
User document:

Id (Mongo ObjectId string)

Gmail

Password (BCrypt hash)

OtpCode document:

Id

Email

Code

ExpiresAt

IsUsed

FileService.API
FileRecord document:

Id (Mongo ObjectId string)

UserId (integer obtained from the JWT claim)

FileName

StoredFileName

ContentType

FileSizeBytes

UploadDate

PasswordHash (SHA256 when the file is protected)

ExpiryDate

DownloadLimit

DownloadCount

ThumbnailPath

Important DTOs
UploadFileRequestDto:

File (IFormFile)

Password?

ExpiryDate?

DownloadLimit?

UpdateFileRequestDto:

FileName?

Password?

ExpiryDate?

DownloadLimit?

FileRecordResponseDto returns:

FileId

FileName

Size

ContentType

UploadDate

HasPassword

ExpiryDate

DownloadLimit

DownloadCount

DownloadUrl

ThumbnailUrl

StorageQuotaDto returns:

UsedBytes

MaxBytes

FileCount

UsagePercentage

Detailed Local Deployment
Step 1: AuthService.API
cd AuthService.API
Update the configuration:

MongoDB:ConnectionString must exist.

Jwt:Issuer = AuthService.API

Jwt:Audience = FileHub

Mailjet:ApiKey, Mailjet:SecretKey, Mailjet:FromEmail, Mailjet:FromName

Run:

dotnet run
Swagger endpoint: https://localhost:7001/swagger (when running locally through the gateway).

Step 2: FileService.API
cd FileService.API
Update the configuration:

MongoDB:ConnectionString

Firebase:BucketName

Firebase:CredentialFilePath

Jwt:Key must be the same as the AuthService JWT key.

Run:

dotnet run
Step 3: Gateway
cd Gateway
dotnet run
Step 4: Frontend
cd fe
npm install
npm run dev
Then open http://localhost:7070.

Detailed Environment Configuration
AuthService.API
Key	Description
MongoDB:ConnectionString	MongoDB Atlas or local connection string.
Jwt:Issuer	JWT issuer.
Jwt:Audience	JWT audience.
Jwt:ExpireMinutes	JWT expiration time.
Mailjet:ApiKey	API key for sending emails.
Mailjet:SecretKey	Secret key for sending emails.
Mailjet:FromEmail	Email address used to send OTPs.
Mailjet:FromName	Display name used when sending emails.
Note: Program.cs requires MongoDB:ConnectionString to be configured; otherwise, the service will not start.

FileService.API
Key	Description
MongoDB:ConnectionString	Connection string to MongoDB Atlas.
MongoDB:DatabaseName	Database name (default: FileServiceDB).
Firebase:BucketName	Firebase Storage bucket name.
Firebase:CredentialFilePath	Path to the JSON credentials file.
Jwt:Key	JWT secret key.
Jwt:Issuer	Must match AuthService.
Jwt:Audience	Must match AuthService.
Gateway
Local: uses Gateway/ocelot.json.

Docker: uses Gateway/ocelot.Docker.json.

Production: uses Gateway/ocelot.Render.json.

API Reference
AuthService.API
POST /api/auth/register
Request:

{
  "gmail": "user@example.com",
  "password": "Password123"
}
Response:

{
  "success": true,
  "message": "Account created successfully."
}
POST /api/auth/login
Request:

{
  "gmail": "user@example.com",
  "password": "Password123"
}
Response:

{
  "success": true,
  "message": "Login successful.",
  "token": "<jwt-token>"
}
POST /api/auth/request-password-reset
Request:

{
  "email": "user@example.com"
}
POST /api/auth/reset-password
Request:

{
  "gmail": "user@example.com",
  "otp": "ABC123",
  "newPassword": "NewPass123"
}
POST /api/auth/Check_Authorize
Requires Authorization: Bearer <token>.

FileService.API
POST /api/files/upload
Content-Type: multipart/form-data

Requires JWT.

Fields:

file

password (optional)

expiryDate (optional)

downloadLimit (optional)

GET /api/files/my-files
Available to authenticated users.

Returns the user's file list.

GET /api/files/storage-quota
Returns the current storage usage and limit.

GET /api/files/{id}
Returns detailed file information by id.

POST /api/files/{id}/verify-password
Request:

{
  "password": "secret"
}
GET /api/files/{id}/download?password=...
Downloads the file.

If the file is password-protected, provide the password.

GET /api/files/{id}/thumbnail
Streams the PNG thumbnail if available.

PUT /api/files/{id}/access
Request:

{
  "fileName": "updated-name.pdf",
  "password": "newpass",
  "expiryDate": "2026-12-31",
  "downloadLimit": 20
}
DELETE /api/files/{id}
Deletes the file when the user is authenticated and is the owner.

Frontend and User Flow
Upload a File
Open the Upload page.

Select a file.

Enter an optional protection password.

Select an optional expiration date.

Set the download limit.

Click Upload.

Manage Personal Files
The My Files page displays:

File name

Download count

Thumbnail

Password-protection status

Expiration date

Users can update file settings or delete files.

Share a File
The frontend creates a sharing link in the form /share/{fileId}.

If the file is password-protected, the downloader must enter the password before downloading.

Docker and Deployment
Build Containers
docker build -t authservice-api ./AuthService.API
docker build -t fileservice-api ./FileService.API
docker build -t api-gateway ./Gateway
docker build -t filehub-frontend ./fe
Run Containers
docker run -d -p 7001:8080 --name authservice authservice-api
docker run -d -p 7002:8080 --name fileservice fileservice-api
docker run -d -p 7000:8080 --name gateway api-gateway
docker run -d -p 80:80 --name fe filehub-frontend
Render / Production Deployment
Gateway/ocelot.Render.json is configured with the hostnames authservice-api-exnn.onrender.com and fileservice-api.onrender.com.

Make sure the URLs and downstream ports match the Render services.

Update the CORS configuration if the deployed frontend uses a different origin.

Testing and Troubleshooting
Open Swagger UI for each service to test endpoints.

Check the JWT token and Authorization header.

If file upload fails, check:

FileService.API may not be able to read the Firebase credentials.

MongoDB:ConnectionString may be invalid.

Jwt:Key may not match the AuthService.

If password reset fails, check whether the OTP has expired after 5 minutes.

Important Notes
Never store users' passwords in plain text.

Keep JWT tokens secure.

Firebase credentials and MongoDB connection strings must be protected.

File download speed depends on Firebase Storage and network limitations.

Future Improvements
Extend UploadLimitService to support multiple quota tiers.

Add separate share expiration settings for sharing links.

Add refresh token support if long-lived tokens are required.

Add an admin dashboard for managing users and files.