# MapReduce.GRPC.CSharp

A distributed MapReduce implementation using gRPC in C# that processes Word documents to perform word counting.

<img width="2558" height="1538" alt="Image" src="https://github.com/user-attachments/assets/3c3782f2-34d5-49c8-ab7c-8b8e45cec866" />

## Architecture

The project consists of four main components, each running as a separate microservice:

### Components

1. **Master** (`MapReduce.GRPC.CSharp.Master`)
   - Orchestrates the entire MapReduce pipeline
   - Reads DOCX files and extracts text content
   - Coordinates communication between workers
   - Runs on port 5075

2. **Map Worker** (`MapReduce.GRPC.CSharp.MapWorker`)
   - Processes input text lines and extracts word counts
   - Implements the Map phase of MapReduce
   - Runs on port 5001

3. **Shuffle Worker** (`MapReduce.GRPC.CSharp.ShuffleWorker`)
   - Aggregates word counts from multiple mappers
   - Implements the Shuffle phase of MapReduce
   - Runs on port 5002

4. **Reduce Worker** (`MapReduce.GRPC.CSharp.ReduceWorker`)
   - Performs final aggregation of word counts
   - Implements the Reduce phase of MapReduce
   - Runs on port 5003

5. **Common** (`MapReduce.GRPC.CSharp.Common`)
   - Contains shared gRPC protocol definitions
   - Defines the service contracts and message types

## Technology Stack

- **.NET 9.0** - Modern C# runtime
- **gRPC** - High-performance RPC framework
- **ASP.NET Core** - Web framework for hosting services
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **OpenXML** - DOCX file processing

## Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose
- Visual Studio 2022 or VS Code (optional)

## Quick Start

### Using Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/AbdalrahmanBashir/MapReduce.GRPC.CSharp.git
   cd MapReduce.GRPC.CSharp
   ```

2. **Run the entire system**
   ```bash
   docker-compose up --build
   ```

3. **View results**
   The system will automatically process the included `alice-in-wonderland.docx` file and output word counts to the console.

### Manual Setup

1. **Restore dependencies**
   ```bash
   dotnet restore
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run individual services** (in separate terminals)
   ```bash
   # Terminal 1 - Map Worker
   cd MapReduce.GRPC.CSharp.MapWorker
   dotnet run

   # Terminal 2 - Shuffle Worker
   cd MapReduce.GRPC.CSharp.ShuffleWorker
   dotnet run

   # Terminal 3 - Reduce Worker
   cd MapReduce.GRPC.CSharp.ReduceWorker
   dotnet run

   # Terminal 4 - Master
   cd MapReduce.GRPC.CSharp.Master
   dotnet run
   ```

## How It Works

### 1. Input Processing
The Master service reads a DOCX file and extracts text content line by line using the OpenXML library.

### 2. Map Phase
- Master sends text lines to Map Worker via gRPC streaming
- Map Worker splits each line into words and counts occurrences
- Returns (word, count) pairs

### 3. Shuffle Phase
- Master groups word counts into buckets based on hash codes
- Each bucket is sent to Shuffle Worker for aggregation
- Shuffle Worker combines counts for the same words

### 4. Reduce Phase
- Master sends aggregated results to Reduce Worker
- Reduce Worker performs final aggregation
- Returns final word count results

### 5. Output
Results are displayed in the console showing each word and its total count.

## API Reference

### gRPC Services

The system uses three gRPC services defined in `mapreduce.proto`:

```protobuf
service MapService {
    rpc Map(stream Line) returns (stream WordCount);
}

service ShuffleService {
    rpc Shuffle(stream WordCount) returns (stream WordCount);
}

service ReduceService {
    rpc Reduce(stream WordCount) returns (stream WordCount);
}
```

### Message Types

- `Line`: Contains a single line of text
- `WordCount`: Contains a word and its count

## Docker Configuration

Each service has its own Dockerfile and is configured in `docker-compose.yml`:

- **Map Worker**: Port 5001
- **Shuffle Worker**: Port 5002  
- **Reduce Worker**: Port 5003
- **Master**: Port 5075

The Master service includes environment variables to connect to the worker services.

## Project Structure

```
MapReduce.GRPC.CSharp/
├── MapReduce.GRPC.CSharp.Master/          # Orchestration service
├── MapReduce.GRPC.CSharp.MapWorker/       # Map phase implementation
├── MapReduce.GRPC.CSharp.ShuffleWorker/   # Shuffle phase implementation
├── MapReduce.GRPC.CSharp.ReduceWorker/    # Reduce phase implementation
├── MapReduce.GRPC.CSharp.Common/          # Shared gRPC definitions
├── docker-compose.yml                     # Container orchestration
└── README.md                              # This file
```

## Testing

The system includes a sample `alice-in-wonderland.docx` file for testing. You can replace this with any DOCX file by:

1. **Docker**: Mount a volume or copy your file into the Master container
2. **Manual**: Place your DOCX file in the Master project directory and specify the path as a command-line argument

## Monitoring and Logging

Each service includes comprehensive logging:
- Map Worker logs processing statistics
- Shuffle Worker logs aggregation results
- Reduce Worker logs final results
- Master logs pipeline progress

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 5001, 5002, 5003, and 5075 are available
2. **Docker issues**: Make sure Docker Desktop is running
3. **File not found**: Verify the DOCX file exists in the expected location

### Debug Mode

To run in debug mode, use:
```bash
docker-compose up --build --force-recreate
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the terms specified in the LICENSE.txt file.

## Acknowledgments

- Inspired by Google's MapReduce paper
- Built with modern .NET and gRPC technologies
- Uses OpenXML for document processing

