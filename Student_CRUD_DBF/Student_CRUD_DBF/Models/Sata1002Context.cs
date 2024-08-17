using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Student_CRUD_DBF.Models;

public partial class Sata1002Context : DbContext
{
    public Sata1002Context()
    {
    }

    public Sata1002Context(DbContextOptions<Sata1002Context> options)
        : base(options)
    {
    }

   
    public virtual DbSet<Student> Students { get; set; }

   
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Database=sata1002;User Id=postgres;Password=AdvEnt4pgSQL");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Standart).HasDefaultValue(0);
        });

       

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
