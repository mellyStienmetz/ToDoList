using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
            
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseCors("CorsPolicy");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});


app.MapGet("/todoitems",async (ToDoDbContext db) =>  await db.Items.ToListAsync());
app.MapPost("/todoitems/{name}",async (string name,ToDoDbContext db) => {
    var item=new Item(name);
    db?.Items.AddAsync(item);
    await db.SaveChangesAsync();
     return Results.Created($"/todoitems/{item.Id}",item); 
});
app.MapPut("/todoitems/{id}", async (int id,ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);

    if (todo is null) return Results.NotFound();

   todo.IsComplete=!todo.IsComplete;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id,ToDoDbContext db) =>
{
    if (await db.Items.FindAsync(id) is Item todo)
    {
        db.Items.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

// to local connection 
//server=localhost;user=root;password=Melly304!;database=tododb
app.Run();

