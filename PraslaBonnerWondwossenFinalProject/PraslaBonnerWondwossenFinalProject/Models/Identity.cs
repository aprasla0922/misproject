﻿using System.Data.Entity;

using System.Security.Claims;

using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Microsoft.AspNet.Identity.EntityFramework;

using System.ComponentModel.DataAnnotations;

using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.AspNet.Identity.Owin;

namespace PraslaBonnerWondwossenFinalProject.Models

{

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class AppUser : IdentityUser

    {

        public AppUser() {
            Payees = new List<Payee>();
        }

        // Put any additional fields that you need for your user here

        //For instance

        [Required]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Display(Name = "Middle Initial")]
        public string Middle { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public Int32 Zip { get; set; }

        [Column(TypeName = "DateTime2")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-YYYY}", ApplyFormatInEditMode = true)]
        public DateTime Birthday { get; set; }

        public String SSN { get; set; }

        public bool? isActive { get; set; }

        public virtual List<BankAccount> BankAccounts { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<Dispute> Disputes { get; set; }

        public virtual List<Payee> Payees { get; set; }
        public virtual StockPortfolio StockPortfolio { get; set; }
        
   
        public Boolean hasAccount() {
            if (this.BankAccounts.Count == 0) {
                return false;
            }
            return true;
        }
        public int Age { get {
                int outter = 0;
                outter = DateTime.Now.Year - Birthday.Year;
                if (DateTime.Now.DayOfYear<Birthday.DayOfYear)
                {
                    outter = outter - 1;
                }
                return outter;} }
        //This method allows you to create a new user

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppUser> manager)

        {

            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            await manager.UpdateSecurityStampAsync(this.Id);
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here

            return userIdentity;

        }

    }



    // Here's your db context for the project.  All of your db sets should go in here

    public partial class AppDbContext : IdentityDbContext<AppUser>

    {

        //Add dbsets here, for instance there's one for books

        //Remember, Identity adds a db set for users, so you shouldn't add that one - you will get an error

        public DbSet<Payee> Payees { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<StockPortfolio> StockPortfolios { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockQuote> StockQuotes { get; set; }
        public DbSet<Sales> Sales { get; set; }



        //Make sure that your connection string name is correct here.

        public AppDbContext()

            : base("MyDBConnection", throwIfV1Schema: false)

        {

        }



        public static AppDbContext Create()

        {

            return new AppDbContext();

        }

        public DbSet<AppRole> AppRoles { get; set; }

        public System.Data.Entity.DbSet<PraslaBonnerWondwossenFinalProject.Models.PurchasedStock> PurchasedStocks { get; set; }
    }

}