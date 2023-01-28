using MinimalAPI.Models;
using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(builder.Configuration.GetConnectionString("Postgres"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/todotems", async (IDocumentSession session) => 
    await session.Query<Todo>().ToListAsync());

app.MapGet("/todoitems/complete", async (IDocumentSession session) =>
    await session.Query<Todo>().Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, IDocumentSession session) =>
    await session.LoadAsync<Todo>(id));

app.MapPost("/todoitems", async (Todo todo, IDocumentSession session) =>
{
    session.Store(todo);
    await session.SaveChangesAsync();
    
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, IDocumentSession session) =>
{
    var todo = await session.LoadAsync<Todo>(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    session.Update(todo);
    await session.SaveChangesAsync();

    return Results.Ok();
});

app.MapDelete("/todoitems/{id}", async (int id, IDocumentSession session) =>
{
    if (await session.LoadAsync<Todo>(id) is Todo todo)
    {
        session.Delete(todo);
        await session.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();