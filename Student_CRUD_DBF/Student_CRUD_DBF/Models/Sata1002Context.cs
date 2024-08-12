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

        => optionsBuilder.UseNpgsql("connection");

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
