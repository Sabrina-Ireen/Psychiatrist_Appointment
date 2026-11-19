using PsychiatristAppointment.Context;

var builder = WebApplication.CreateBuilder(args);

// ----------------- CORS -----------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular dev
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ----------------- Services -----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DapperContext>();

var app = builder.Build();

// ----------------- Middleware -----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();
app.Run();
