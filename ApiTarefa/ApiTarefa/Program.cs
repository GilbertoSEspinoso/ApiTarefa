using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "Olá Mundo");

app.MapGet("frases", async () =>
await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));


// Definição do mapeamento 

//retornar uma lista de tarefas (essa expressão lambda vai atuar como Manipulador de rota)
app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

//retornar uma unica tarefa
app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

//retornar tarefas que foram concluidas
app.MapGet("/tarefas/concluida", async (AppDbContext db) =>
                                    await db.Tarefas.Where(t => t.IsConcluida).ToListAsync());

//incluir as tarefas
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
    //Created vai retornar no body do request o Id da tarefa que foi incluida
});

//atualizar uma tarefa
app.MapPut("/tarefa/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null) return Results.NotFound();

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

//Deletar uma tarefa
app.MapDelete("/tarefa/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});

app.Run();



/* essa classe permite gerenciar informações sobre tarefas que representam atividades
que poderão estar concluídas ou não */
class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}

/* essa class coordena a funcionalidade do EntityFrameworkCore pro nosso modelo
 de dados definidos por tarefa */
class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}