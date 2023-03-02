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


app.MapGet("/", () => "Ol� Mundo");

app.MapGet("frases", async () =>
await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));


// Defini��o do mapeamento 

//retornar uma lista de tarefas (essa express�o lambda vai atuar como Manipulador de rota)
app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

//incluir as tarefas
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
    //Created vai retornar no body do request o Id da tarefa que foi incluida
});

app.Run();



/* essa classe permite gerenciar informa��es sobre tarefas que representam atividades
que poder�o estar conclu�das ou n�o */
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