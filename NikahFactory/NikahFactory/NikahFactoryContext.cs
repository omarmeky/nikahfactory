using NikahFactory.Migrations;
using NikahFactory.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NikahFactory
{
    public class NikahFactoryContext:DbContext 
    {
        public NikahFactoryContext():base("DefaultConnection")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<NikahFactoryContext, Configuration>());
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
    }
}