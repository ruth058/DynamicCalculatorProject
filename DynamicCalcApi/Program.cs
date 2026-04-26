using DynamicCalcApi;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי קונטרולרים
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



var app = builder.Build();

// אתחול ה-Database
DbInitializer.Initialize("Data Source=calculator.db");
app.UseRouting();   
app.UseCors("AllowAll");

// הגדרת דף הבית (הכתובת הראשית) כדי שלא תקבל 404
app.MapGet("/", () => "The Calculator API is ready. 1,000,000 rows are loaded. Please use /api/v1/test or /api/v1/run-calc");

app.UseAuthorization();

app.MapControllers();

// קוד בדיקה: מדפיס לטרמינל את כל הכתובות שהשרת מכיר
app.Lifetime.ApplicationStarted.Register(() =>
{
    var actionProvider = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>();
    Console.WriteLine("\n========== AVAILABLE API ROUTES ==========");
    foreach (var action in actionProvider.ActionDescriptors.Items)
    {
        var route = action.AttributeRouteInfo?.Template;
        Console.WriteLine($"URL: /{route}");
    }
    Console.WriteLine("==========================================\n");
});

app.Run();