using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<MestrePokemon> MestresPokemon { get; set; } = null!;
        public DbSet<Pokemon> Pokemons { get; set; } = null!;
        public DbSet<Captura> Capturas { get; set; } = null!;
        public DbSet<Evolution> Evolutions { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração para MestrePokemon
            modelBuilder.Entity<MestrePokemon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);

                // Índice único para CPF
                entity.HasIndex(e => e.Cpf).IsUnique();

                // Relacionamento com Capturas
                entity.HasMany(e => e.Capturas)
                      .WithOne(c => c.MestrePokemon)
                      .HasForeignKey(c => c.MestrePokemonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para Pokemon
            modelBuilder.Entity<Pokemon>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Propriedade Name
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                // Propriedade SpriteBase64
                entity.Property(e => e.SpriteBase64)
                      .IsRequired();

                // Propriedade Hash
                entity.Property(e => e.Hash)
                      .IsRequired()
                      .HasMaxLength(64);

                // Índice para Hash
                entity.HasIndex(e => e.Hash);

                // Relacionamento com Capturas
                entity.HasMany(e => e.Capturas)
                      .WithOne(c => c.Pokemon)
                      .HasForeignKey(c => c.PokemonId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Evolutions
                entity.HasMany(e => e.Evolutions)
                      .WithOne(evo => evo.Pokemon)
                      .HasForeignKey(evo => evo.PokemonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para Captura
            modelBuilder.Entity<Captura>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataCaptura).IsRequired();
            });

            // Configuração para Evolution
            modelBuilder.Entity<Evolution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                // Relacionamento com Pokémon base
                entity.HasOne(e => e.Pokemon)
                      .WithMany(p => p.Evolutions)
                      .HasForeignKey(e => e.PokemonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}