using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;
using RulesEngineEditor.Models;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RulesEngineEditor.Data
{
    public class RulesEngineEditorDbContext : DbContext
    {
        public RulesEngineEditorDbContext(DbContextOptions<RulesEngineEditorDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //As of 9/2021 with .NET 5, SQLite does not handle FK cascading deletes
            //options.UseSqlite($"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{System.IO.Path.DirectorySeparatorChar}RulesEngineEditor.db");
        }

        public DbSet<WorkflowData> Workflows { get; set; }
        public DbSet<RuleData> Rules { get; set; }
        public DbSet<ScopedParamData> ScopedParams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WorkflowData>(entity => {
                entity.ToTable("Workflow");
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Ignore(i => i.WorkflowsToInject);
                entity.HasMany(r => r.Rules).WithOne().OnDelete(DeleteBehavior.ClientCascade);
                entity.HasMany(g => g.GlobalParams).WithOne().OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<RuleData>(entity => {
                entity.ToTable("Rule");
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Ignore(i => i.IsSuccess);
                entity.Ignore(i => i.ErrorMessage);
                entity.Ignore(i => i.ExceptionMessage);
                entity.Ignore(i => i.WorkflowsToInject);

                entity.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions()))
                .HasJsonConversion();

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<RuleActions>(v, new JsonSerializerOptions()));
                entity.HasMany(r => r.Rules).WithOne().OnDelete(DeleteBehavior.ClientCascade);
                entity.HasMany(g => g.LocalParams).WithOne().OnDelete(DeleteBehavior.ClientCascade);
            });

            modelBuilder.Entity<ScopedParamData>(entity => {
                entity.ToTable("ScopedParam");
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
            });
        }
    }
    public static class JSONHelper
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
        {
            var options = new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            ValueConverter<T, string> converter = new ValueConverter<T, string>
            (
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<T>(v, options) ?? new T()
            );

            ValueComparer<T> comparer = new ValueComparer<T>
            (
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}