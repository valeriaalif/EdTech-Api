## EdTEch API

## Description
Rest API developed in C# .NET Core 8.0 for educational purposes. The API is composed of various methods including user authentication
by token and user segmentation by distinct priviledges. The API uses Dapper to map values and Swagger UI to run methods. The API
can be used indistictively for as many web applications as needed. 

## Contents
This application contains different methods to suit requirements:
- Register Account
- Login
- Fetching Users

## Requirements
- Visual Studio 2022
- SQL Management Studio



## Dependencies

To download this dependencies use NuGet Package Manager on Visual Studio 
Note: Make sure each of the dependencies is compatible with your .NET version.
It is recommended not to install the latest version but the version that matches yours. 
Example: If using .NET 8.0 select the version that matches. 

 <PackageReference Include="BCrypt.Net-Core" Version="1.6.0" />
 <PackageReference Include="Dapper" Version="2.1.66" />
 <PackageReference Include="MailKit" Version="4.12.1" />
 <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.2" />
 <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.3.0" />
 <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
 <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.6">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
 </PackageReference>
 <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.DependencyModel" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.Logging.Configuration" Version="9.0.6" />
 <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.6" />
 <PackageReference Include="Microsoft.Identity.Client" Version="4.73.0" />
 <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
 <PackageReference Include="Swashbuckle.AspNetCore" Version="8.0.0" />
 <PackageReference Include="Swashbuckle.AspNetCore.Filters" Version="9.0.0" />
 <PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="8.0.0" />
 <PackageReference Include="System.Data.SqlClient" Version="4.9.0" />

 
## How to Test
After running the application it will display on your browser using Swagger UI.
You either can test it form Swagger UI or using Postman. 
- Swagger UI  
<img src="https://github.com/user-attachments/assets/97b08ed1-1933-4315-95de-2d446bdb3b5e" alt="Swagger UI" width="400" height= "200"/>


- Postman  
<img src="https://github.com/user-attachments/assets/00f2428e-579d-434a-90a8-67bd4a574666" alt="Postman" width="400" height= "300"/>






