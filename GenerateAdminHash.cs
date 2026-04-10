using BCrypt.Net;

// Script để generate BCrypt hash cho admin password
// Chạy: dotnet script GenerateAdminHash.cs

var password = "admin123";
var hash = BCrypt.Net.BCrypt.HashPassword(password);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"BCrypt Hash: {hash}");

