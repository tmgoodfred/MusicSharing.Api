# MusicSharing API Documentation

## Overview
The **MusicSharing API** is a RESTful **.NET 8** backend for managing users, songs, playlists, comments, ratings, categories, followers, analytics, and more.

- **Authentication:** JWT (Bearer)
- **Intended clients:** Web and mobile frontends (for example, Angular)

---

## Base URL

- **Production:** `https://your-production-domain/api/`  
- **Development:** `https://localhost:5001/api/`

---

## Authentication

- **Type:** JWT Bearer Token  
- **Login Endpoint:** `POST /api/user/login`  
- **Header:**
```
Authorization: Bearer <your-jwt-token>
```

**How to obtain a token:**  
Send username/email and password to `/api/user/login`. The successful response will include a JWT token.

---

## CORS

- CORS is enabled for frontend development (e.g., `http://localhost:4200` for Angular).
- Adjust allowed origins in your backend CORS policy as needed for production.

---

## Endpoints

### User

| Method | Route                      | Description            | Auth Required        |
|--------|----------------------------|------------------------|----------------------|
| GET    | `/api/user`                | List all users         | Yes (Admin)          |
| GET    | `/api/user/{id}`           | Get user profile       | Yes                  |
| POST   | `/api/user`                | Register new user      | No                   |
| PUT    | `/api/user/{id}`           | Update user profile    | Yes (Owner/Admin)    |
| DELETE | `/api/user/{id}`           | Delete user            | Yes (Owner/Admin)    |
| POST   | `/api/user/login`          | Login, returns JWT     | No                   |
| GET    | `/api/user/{id}/analytics` | User's song analytics  | Yes (Owner/Admin)    |

---

### Song

| Method | Route                        | Description                | Auth Required        |
|--------|------------------------------|----------------------------|----------------------|
| GET    | `/api/music`                 | List all songs             | No                   |
| GET    | `/api/music/{id}`            | Get song by ID             | No                   |
| POST   | `/api/music`                 | Create song                | Yes                  |
| PUT    | `/api/music/{id}`            | Update song                | Yes (Owner/Admin)    |
| DELETE | `/api/music/{id}`            | Delete song                | Yes (Owner/Admin)    |
| GET    | `/api/music/{id}/stream`     | Stream audio file          | No                   |
| GET    | `/api/music/{id}/download`   | Download audio file        | No                   |
| POST   | `/api/music/upload`          | Upload new song (multipart/form-data) | Yes      |
| GET    | `/api/music/{id}/artwork`    | Get song artwork           | No                   |
| GET    | `/api/music/search`          | Search / filter songs      | No                   |

#### Advanced Search Example
```
GET /api/music/search?title=love&artist=John&genre=Rock&minPlays=10&maxRating=4.5&fromDate=2024-01-01&tags=pop,summer
```

---

### Playlist

| Method | Route                                              | Description               | Auth Required |
|--------|----------------------------------------------------|---------------------------|---------------|
| GET    | `/api/playlist`                                    | List all playlists        | No            |
| GET    | `/api/playlist/{id}`                               | Get playlist by ID        | No            |
| POST   | `/api/playlist`                                    | Create playlist           | Yes           |
| PUT    | `/api/playlist/{id}`                               | Update playlist           | Yes (Owner)   |
| DELETE | `/api/playlist/{id}`                               | Delete playlist           | Yes (Owner)   |
| POST   | `/api/playlist/{playlistId}/songs/{songId}`        | Add song to playlist      | Yes (Owner)   |
| DELETE | `/api/playlist/{playlistId}/songs/{songId}`        | Remove song from playlist | Yes (Owner)   |

---

### Category

| Method | Route                  | Description                | Auth Required |
|--------|------------------------|----------------------------|---------------|
| GET    | `/api/category`        | List all categories        | No            |
| GET    | `/api/category/{id}`   | Get category by ID         | No            |
| POST   | `/api/category`        | Create category            | Yes (Admin)   |
| PUT    | `/api/category/{id}`   | Update category            | Yes (Admin)   |
| DELETE | `/api/category/{id}`   | Delete category            | Yes (Admin)   |

---

### Comments & Ratings

| Method | Route                                 | Description                | Auth Required       |
|--------|---------------------------------------|----------------------------|---------------------|
| GET    | `/api/comment/song/{songId}`          | Get comments for a song    | No                  |
| POST   | `/api/comment`                        | Add comment to a song      | Yes                 |
| DELETE | `/api/comment/{commentId}`            | Delete comment             | Yes (Owner/Admin)   |
| GET    | `/api/rating/song/{songId}`           | Get ratings for a song     | No                  |
| POST   | `/api/rating`                         | Add / update rating        | Yes                 |
| DELETE | `/api/rating/{ratingId}`              | Delete rating              | Yes (Owner/Admin)   |

---

### Followers & Activity

| Method | Route                                 | Description                        | Auth Required |
|--------|---------------------------------------|------------------------------------|---------------|
| POST   | `/api/follower/follow`                | Follow a user                      | Yes           |
| POST   | `/api/follower/unfollow`              | Unfollow a user                    | Yes           |
| GET    | `/api/follower/{userId}/followers`    | List followers                     | No            |
| GET    | `/api/follower/{userId}/following`    | List following                     | No            |
| GET    | `/api/activity/user/{userId}`         | User's activity                    | No            |
| GET    | `/api/activity/feed/{userId}`         | Activity feed (user + following)   | No            |
| POST   | `/api/activity`                       | Log activity                       | Yes           |

---

### Blog

| Method | Route                  | Description                | Auth Required |
|--------|------------------------|----------------------------|---------------|
| GET    | `/api/blog`            | List all blog posts        | No            |
| GET    | `/api/blog/{id}`       | Get blog post by ID        | No            |
| POST   | `/api/blog`            | Create blog post           | Yes (Admin)   |
| PUT    | `/api/blog/{id}`       | Update blog post           | Yes (Admin)   |
| DELETE | `/api/blog/{id}`       | Delete blog post           | Yes (Admin)   |

---

## Models & DTOs

> **Note:** Responses only include safe/public fields. See the codebase for full model definitions and internal fields.

### Example: UserProfileDto
```json
{
  "id": 1,
  "username": "user1",
  "email": "user1@example.com",
  "role": "User",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Example: Song
```json
{
  "id": 1,
  "title": "Song Title",
  "artist": "Artist Name",
  "filePath": "/mnt/user/music-files/audio/song.mp3",
  "artworkPath": "/mnt/user/music-files/artwork/art.jpg",
  "genre": "Rock",
  "tags": ["pop", "summer"],
  "uploadDate": "2024-01-01T00:00:00Z",
  "playCount": 123,
  "downloadCount": 45,
  "categories": [],
  "comments": [],
  "ratings": [],
  "userId": 1
}
```

### Example: Playlist
```json
{
  "id": 1,
  "name": "My Playlist",
  "description": "Chill songs",
  "userId": 1,
  "songs": []
}
```

Other models (Comment, Rating, Category, Follower, Activity, BlogPost) follow similar shapes—see the codebase for full definitions and nullable/private fields.

---

## Authentication Flow Example

**Request**
```
POST /api/user/login
Content-Type: application/json

{
  "usernameOrEmail": "user1@example.com",
  "password": "yourPassword"
}
```

**Response**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Usage**  
Include the token in the `Authorization` header for all protected endpoints:
```
Authorization: Bearer <token>
```

---

## Sample Request / Response Payloads

### Create User
**Request**
```json
{
  "username": "user1",
  "email": "user1@example.com",
  "passwordHash": "plaintextOrHashedPassword",
  "role": "User"
}
```

**Response** (UserProfileDto)
```json
{
  "id": 1,
  "username": "user1",
  "email": "user1@example.com",
  "role": "User",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Upload Song (multipart/form-data)

**Fields**
- `file`: audio file (required)
- `artwork`: image file (optional)
- `title`: string
- `artist`: string

**Example (curl)**
```bash
curl -X POST "https://your-api/api/music/upload"   -H "Authorization: Bearer <token>"   -F "file=@song.mp3"   -F "artwork=@cover.jpg"   -F "title=My Song"   -F "artist=The Band"
```

### Add Comment
**Request**
```json
{
  "songId": 1,
  "commentText": "Great track!",
  "isAnonymous": false,
  "userId": 1
}
```

**Response**
```json
{
  "id": 10,
  "songId": 1,
  "commentText": "Great track!",
  "isAnonymous": false,
  "userId": 1,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

## DTO Field Descriptions

### UserProfileDto

| Field     | Type     | Description                |
|-----------|----------|----------------------------|
| id        | int      | User ID                    |
| username  | string   | Username                   |
| email     | string   | Email address              |
| role      | string   | User role (User/Admin)     |
| createdAt | DateTime | Account creation date      |

### Song

| Field         | Type       | Description                        |
|---------------|------------|------------------------------------|
| id            | int        | Song ID                            |
| title         | string     | Song title                         |
| artist        | string     | Artist name                        |
| filePath      | string     | Server path to audio file          |
| artworkPath   | string     | Server path to artwork image       |
| genre         | string     | Song genre                         |
| tags          | string[]   | List of tags                       |
| uploadDate    | DateTime   | Upload date/time                   |
| playCount     | int        | Number of times played             |
| downloadCount | int        | Number of times downloaded         |
| categories    | Category[] | Associated categories              |
| comments      | Comment[]  | Comments on the song               |
| ratings       | Rating[]   | Ratings for the song               |
| userId        | int        | Uploader's user ID                 |

---

## Required Headers

For protected endpoints include:
```
Authorization: Bearer <token>
Content-Type: application/json
```

(Use `multipart/form-data` for file uploads.)

---

## Business Rules

- Only the owner or an admin can update or delete a song, playlist, or user.
- Only authenticated users can comment, rate, upload, or follow.
- Admin-only endpoints are marked in the endpoint tables.

---

## Error Handling

- Standard HTTP status codes are used (`400`, `401`, `403`, `404`, `500`, etc.).
- Error responses are JSON:
```json
{
  "error": "Description of the error."
}
```

---

## Special Notes

- **File Uploads:** Use `multipart/form-data` for song uploads.
- **Authorization:** Use JWT in the `Authorization` header for protected endpoints.
- **CORS:** Ensure your frontend origin is allowed in backend CORS policy.
- **Paths shown in examples** (e.g., `/mnt/user/...`) represent server filesystem locations—do not expose internal paths in public documentation if unnecessary.

---

## Contact & Contribution

For questions, issues, or contributions, see the GitHub repository:

https://github.com/tmgoodfred/MusicSharing.Api

---

> This documentation is auto-generated and should be kept up to date with API changes.
