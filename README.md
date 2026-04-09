# DocumentSearch

A multilingual document full-text search API built with ASP.NET Core 8 and PostgreSQL. Supports Thai language search via a PyThaiNLP tokenizer sidecar service.

## Architecture

```
┌─────────────────────┐     ┌──────────────────────┐     ┌──────────────────┐
│  DocumentSearch API │────▶│  Thai Tokenizer       │     │  PostgreSQL 17   │
│  ASP.NET Core 8     │     │  FastAPI + PyThaiNLP  │     │  tsvector + GIN  │
│  :5079              │     │  :57100               │     │  :5432           │
└─────────────────────┘     └──────────────────────┘     └──────────────────┘
```

- Thai text is tokenized by PyThaiNLP (`newmm` engine) before indexing and querying — batch tokenization is used for efficient multi-document ingestion
- PostgreSQL `tsvector` with `simple` config and GIN index handles full-text search
- Works with mixed-language content (Thai + English in the same document)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Getting Started

**1. Start infrastructure (PostgreSQL + tokenizer)**

```bash
docker-compose up --build
```

**2. Apply database migrations**

```bash
cd DocumentSearch
dotnet ef database update
```

**3. Run the API**

```bash
dotnet run
```

Swagger UI is available at `http://localhost:5079/swagger`.

On first startup, the API automatically seeds 4 sample Thai documents from `Persistance/Seed/Mock.json`.

## Configuration

`appsettings.json`:

```json
{
  "ApiKey": "<your-api-key>",
  "ConnectionStrings": {
    "DefaultConnection": "<postgres-connection-string>"
  },
  "ThaiTokenizer": {
    "BaseUrl": "http://localhost:57100"
  }
}
```

> When running the API inside Docker, change `ThaiTokenizer:BaseUrl` to `http://tokenizer:8000`.

## Authentication

All endpoints require an API key passed in the request header:

```
X-Api-Key: <your-api-key>
```

## API Endpoints

### Search documents

```
GET /api/document/{search}
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string (path) | Search keyword or phrase (Thai or English) |

**Response `200 OK`:**
```json
[
  {
    "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "rank": 0.075990885
  }
]
```

---

### Add documents

```
POST /api/document
```

Accepts a single document or an array of documents. All `content` and `fileName` fields are batch-tokenized in two HTTP calls to the tokenizer sidecar regardless of how many documents are submitted.

**Request body (single):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "report.pdf",
  "content": "เนื้อหาเอกสารภาษาไทย"
}
```

**Request body (batch):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "report.pdf",
    "content": "เนื้อหาเอกสารภาษาไทย"
  },
  {
    "id": "7cb96a12-1234-4321-a1b2-3c4d5e6f7a8b",
    "fileName": "summary.pdf",
    "content": "สรุปเนื้อหา"
  }
]
```

**Response `201 Created`**

---

### Delete a document

```
DELETE /api/document/{id}
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | GUID (path) | Document ID to delete |

## Tokenizer Sidecar API

The tokenizer is a FastAPI service wrapping PyThaiNLP's `newmm` engine.

### Single tokenize

```
POST /tokenize
```

```json
// Request
{ "text": "ข้อความภาษาไทย" }

// Response
{ "result": "ข้อความ ภาษา ไทย" }
```

### Batch tokenize

```
POST /tokenize/batch
```

Tokenizes multiple strings in one call. Results are returned in the same order as the input.

```json
// Request
{ "texts": ["ข้อความภาษาไทย", "เนื้อหาเอกสาร"] }

// Response
{ "results": ["ข้อความ ภาษา ไทย", "เนื้อหา เอกสาร"] }
```

> The DocumentSearch API uses `/tokenize/batch` internally when indexing documents, keeping tokenizer round-trips constant at 2 regardless of batch size.

## Project Structure

```
DocumentSearch/
├── Auth/                        # API key authentication handler
├── Controllers/
│   └── DocumentController.cs    # Search, add, delete endpoints
├── Extensions/
│   └── SwaggerExtensions.cs     # Swagger + API key UI setup
├── Migrations/                  # EF Core migrations
├── Models/
│   └── Document.cs              # Document entity
├── Persistance/
│   ├── Configurations/
│   │   └── DocumentConfiguration.cs  # EF config, tsvector GIN index
│   ├── Seed/
│   │   ├── DocumentSeeder.cs    # Startup seed logic
│   │   └── Mock.json            # Sample Thai documents
│   └── DocumentDbContext.cs
├── Services/
│   ├── IThaiTokenizerService.cs
│   └── ThaiTokenizerService.cs  # HTTP client to tokenizer sidecar
├── appsettings.json
└── Program.cs

tokenizer/                       # Python tokenizer sidecar
├── main.py                      # FastAPI app, POST /tokenize and POST /tokenize/batch
├── requirements.txt
└── Dockerfile

docker-compose.yml               # PostgreSQL 17 + tokenizer
```