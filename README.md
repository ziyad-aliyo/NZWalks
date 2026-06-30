# NZWalks

A backend Web API for tracking geographical regions and connecting them to specific walking trails.

## Features

- Region management: Tracks different geographical areas and regions.
- Walk trails tracking: Connects and manages specific walking paths within those regions.
- Difficulty levels: Categorizes trails based on difficulty settings.

## Technologies Used

- C#
- .NET 8 Web API
- Entity Framework Core
- SQL Server

## Swagger UI

<img width="1898" height="1077" alt="image" src="https://github.com/user-attachments/assets/674bee55-d7b6-476f-afe7-2c9bc426d0c4" />


## How to Run

1. Clone the project:
   git clone https://github.com/ziyad-aliyo/NZWalks.git

2. Update the connection string in appsettings.json with your SQL Server credentials.

3. Run migrations:
   dotnet ef database update

4. Run the application:
   dotnet run
