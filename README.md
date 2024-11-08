# Task Management System

## Getting Started

## Table of Contents
- [Prerequisites](#prerequisites)
- [Backend Setup](#backend-setup)
- [Frontend Setup](#frontend-setup)
- [Tech Stack](#tech-stack)
- [Additional Information](#additional-information)

## Prerequisites
Make sure you have the following installed on your machine:
- .NET SDK (version 6.0 or later)
- Node.js (version 14.0 or later)
- npm (Node Package Manager)
- SQL Server (or other preferred database)

## Backend Setup
### 1. Clone the repository:
```bash
git clone https://github.com/MuhammadEssa25/10Pearls_Project.git
 cd 10Pearls_Project/
```

### 2. Configure the database:
- Update the `appsettings.json` file with your database connection string.
- Example `appsettings.json`:
  ```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=taskapp;User=taskappuser;Password=Offline1234!;"
  },
    ...
  }
  ```

### 3. Run Entity Framework migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Install dependencies:
```bash
dotnet restore
```

### 5. Running the Backend
```bash
dotnet run
```
The backend API will be available at `http://localhost:5205`


## Frontend Setup
### 1. Navigate to the frontend directory:
```bash
cd ../../frontend
```

### 2. Install dependencies:
```bash
npm install bootstrap
npm install react-router-dom
npm install jwt-decode
npm install react-bootstrap

```


### 3. Running the Frontend
```bash
npm start
```
The frontend will be available at ` http://localhost:3000/`.


# Tech Stack:
The following technologies are used in this project:

Frontend:
React + TypeScript for building the user interface
Redux for state management in React
Bootstrap for styling
React Router for handling routing
Axios for HTTP requests
JWT for user authentication

Backend:
ASP.NET Core Web API
Entity Framework Core for ORM and database management
xUnit for unit testing (to be implemented)
Serilog for application logging (to be implemented)
Swagger for API documentation

Database:
SQL Server (or other preferred databases like MySQL using Pomelo.EntityFrameworkCore.MySql)
Tools & Services:

SonarQube for analyzing code quality (to be implemented)
Visual Studio for development (optional)

## Additional Information
Authentication: This system supports JWT-based authentication. You will need to create a JWT token for secure API access.
Logging: Application logs are being handled by Serilog, which will provide insights into the application's behavior (implementation to be done).
Testing: Unit tests are being implemented using xUnit for various backend services.