using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoniWatchIApI;

public partial class MoniWatchIContext : DbContext
{
    public MoniWatchIContext()
    {
    }

    public MoniWatchIContext(DbContextOptions<MoniWatchIContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<Moni> Monis { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=wan;password=CnamOcc34!;database=MoniWatchI", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.6.12-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.BankAccountId).HasName("PRIMARY");

            entity.HasIndex(e => e.MoniId, "fk_bankaccounts_monis");

            entity.Property(e => e.BankAccountId).HasColumnType("int(11)");
            entity.Property(e => e.BankAccountBalance).HasPrecision(10, 2);
            entity.Property(e => e.BankAccountLabel).HasMaxLength(128);
            entity.Property(e => e.MoniId).HasColumnType("int(11)");

            entity.HasOne(d => d.Moni).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.MoniId)
                .HasConstraintName("fk_bankaccounts_monis");
        });

        modelBuilder.Entity<Moni>(entity =>
        {
            entity.HasKey(e => e.MoniId).HasName("PRIMARY");

            entity.HasIndex(e => e.MoniLogin, "idx_MoniLogin").IsUnique();

            entity.Property(e => e.MoniId).HasColumnType("int(11)");
            entity.Property(e => e.FirstName).HasMaxLength(32);
            entity.Property(e => e.LastName).HasMaxLength(32);
            entity.Property(e => e.MoniLogin).HasMaxLength(32);
            entity.Property(e => e.MoniPwd).HasMaxLength(64);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PRIMARY");

            entity.HasIndex(e => e.MoniId, "fk_tags_monis");

            entity.Property(e => e.TagId).HasColumnType("int(11)");
            entity.Property(e => e.MoniId).HasColumnType("int(11)");
            entity.Property(e => e.TagDescription).HasMaxLength(512);
            entity.Property(e => e.TagLabel).HasMaxLength(32);

            entity.HasOne(d => d.Moni).WithMany(p => p.Tags)
                .HasForeignKey(d => d.MoniId)
                .HasConstraintName("fk_tags_monis");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.HasIndex(e => e.BankAccountId, "fk_transactions_bankaccounts");

            entity.Property(e => e.TransactionId).HasColumnType("int(11)");
            entity.Property(e => e.BankAccountId).HasColumnType("int(11)");
            entity.Property(e => e.TransactionAmount).HasPrecision(10, 2);
            entity.Property(e => e.TransactionDescription).HasMaxLength(512);
            entity.Property(e => e.TransactionLabel).HasMaxLength(128);

            entity.HasOne(d => d.BankAccount).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BankAccountId)
                .HasConstraintName("fk_transactions_bankaccounts");

            entity.HasMany(d => d.Tags).WithMany(p => p.Transactions)
                .UsingEntity<Dictionary<string, object>>(
                    "TagsTransaction",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_tags_transactions_transactions"),
                    l => l.HasOne<Transaction>().WithMany()
                        .HasForeignKey("TransactionId")
                        .HasConstraintName("fk_tags_transactions_tags"),
                    j =>
                    {
                        j.HasKey("TransactionId", "TagId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("Tags_Transactions");
                        j.HasIndex(new[] { "TagId" }, "fk_tags_transactions_transactions");
                        j.IndexerProperty<int>("TransactionId").HasColumnType("int(11)");
                        j.IndexerProperty<int>("TagId").HasColumnType("int(11)");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
