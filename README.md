  Antes de que ejecuten el proyecto deben revisar si tienene los Nugget/Paquetes correctos deben ser 3 
      BCrypt.Net-Next(4.0.3)
      Microsoft.EntityFrameworkCore.SqlServer(9.0.3)
      Microsoft.EntityFrameworkCore.Tools(9.0.3)

En el caso de que no los tengan deben utilizar los siguientes Comandos en la Terminal de Paquetes 

dotnet restore
dotnet tool install --global dotnet-ef


CONFIGURAR LA BD

  Primero realiza el cambio en AppSetting 

         "DefaultConnection": "Server=DESKTOP-Q9UIS31\\SQLEXPRESS;Database=SchoolSystem;User Id=sa;Password=12345;TrustServerCertificate=True;" 
         
          Cambia los valores de "Server", "User Id" y "Password" con los datos con los  que te conectas a SSMS (SQL Server Managment Studio) 
        (En caso de no lograrlo contactarme)

Una vez cambiado los valores  ejecutar los siguientes comandos       

dotnet ef database update
dotnet run
Revisa en SSMS que las BD se creo

Cuando ejecutes el Programa BS(BirdSing) te enviara a una pagina extraña

La URL debe verse asi

https://localhost:7187/

Solo añade 
https://localhost:7187/Account/Login

User Admin:darwinMP@gmail.com
Password: Admin123

