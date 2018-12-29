using System;
using System.Data;
using conduit_api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace conduit_api.Infrastructure
{
    public class ConduitContext : DbContext
    {
        private readonly string _databaseName = Startup.DATABASE_FILE;
        private IDbContextTransaction _currentTransaction;

        public ConduitContext(DbContextOptions options) : base(options)
        {

        }

        public ConduitContext(string databaseName)
        {
            _databaseName = databaseName;
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<ArticleFavorite> ArticleFavorites { get; set; }
        public DbSet<FollowedPeople> FollowedPeoples { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databaseName}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArticleTag>(b =>
            {
                b.HasKey(at => new { at.TagId, at.ArticleId });

                b.HasOne(at => at.Article)
                    .WithMany(a => a.ArticleTags)
                    .HasForeignKey(at => at.TagId);

                b.HasOne(at => at.Tag)
                    .WithMany(t => t.ArticleTags)
                    .HasForeignKey(at => at.TagId);
            });

            modelBuilder.Entity<ArticleFavorite>(b =>
            {
                b.HasKey(af => new { af.ArticleId, af.PersonId });

                b.HasOne(af => af.Article)
                    .WithMany(a => a.ArticleFavorites)
                    .HasForeignKey(af => af.ArticleId);

                b.HasOne(af => af.Person)
                    .WithMany(p => p.ArticleFavorites)
                    .HasForeignKey(af => af.PersonId);
            });

            modelBuilder.Entity<FollowedPeople>(b =>
            {
                b.HasKey(fp => new { fp.ObserverId, fp.TargetId });
            });
        }

        #region Transaction Handling

        public void BeginTransaction()
        {
            if (_currentTransaction != null) return;

            _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CommitTransaction()
        {
            try
            {
                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        #endregion
    }
}