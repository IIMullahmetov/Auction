using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace Auction.Models
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser
	{

		public virtual ICollection<Auction> Auction { get; set; }
		public virtual ICollection<AuctionUser> AuctionUsers { get; set; }
		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
		{
			Auction = new HashSet<Auction>();
			AuctionUsers = new HashSet<AuctionUser>();
			// Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
			ClaimsIdentity userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Здесь добавьте утверждения пользователя
			return userIdentity;
		}
	}


	[Table("Auction")]
	public partial class Auction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public Auction() => AuctionUsers = new HashSet<AuctionUser>();

		public int Id { get; set; }

		public DateTime AuctionStart { get; set; }

		public int? AuctionInfo_Id { get; set; }

		public DateTime AuctionEnd { get; set; }

		public int Price { get; set; }

		[StringLength(128)]
		public string Winner { get; set; }

		public virtual ApplicationUser User { get; set; }

		public virtual AuctionInfo AuctionInfo { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<AuctionUser> AuctionUsers { get; set; }
	}


	[Table("AuctionInfo")]
	public partial class AuctionInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AuctionInfo() => Auction = new HashSet<Auction>();

		public int Id { get; set; }

		[Required]
		[StringLength(8)]
		public string Name { get; set; }

		[Required]
		[StringLength(8)]
		public string Product { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Auction> Auction { get; set; }
	}

	public class AuctionUser
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int? Auction_Id { get; set; }
		public string User_Id { get; set; }
		public Auction Auction { get; set; }
		public ApplicationUser User { get; set; }

	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public DbSet<Auction> Auctions { get; set; }
		public DbSet<AuctionInfo> AuctionInfos { get; set; }
		public DbSet<AuctionUser> AuctionUsers { get; set; }


		public ApplicationDbContext()
			: base("Context", throwIfV1Schema: false)
		{
		}

		public static ApplicationDbContext Create()
		{
			return new ApplicationDbContext();
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			//modelBuilder.Entity<AspNetRoles>()
			//	.HasMany(e => e.AspNetUsers)
			//	.WithMany(e => e.AspNetRoles)
			//	.Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

			//modelBuilder.Entity<AspNetUsers>()
			//	.HasMany(e => e.AspNetUserClaims)
			//	.WithRequired(e => e.AspNetUsers)
			//	.HasForeignKey(e => e.UserId);

			//modelBuilder.Entity<AspNetUsers>()
			//	.HasMany(e => e.AspNetUserLogins)
			//	.WithRequired(e => e.AspNetUsers)
			//	.HasForeignKey(e => e.UserId);

			modelBuilder.Entity<ApplicationUser>()
				.HasMany(e => e.Auction)
				.WithOptional(e => e.User)
				.HasForeignKey(e => e.Winner);

			//modelBuilder.Entity<Auction>()
			//	.HasMany(e => e.Users)
			//	.WithMany(e => e.AuctionUsers)
			//	.Map(m => m.ToTable("AuctionUser").MapRightKey("User_Id"));

			modelBuilder.Entity<AuctionUser>()
				.HasOptional(e => e.Auction)
				.WithMany(e => e.AuctionUsers)
				.HasForeignKey(e => e.Auction_Id);

			modelBuilder.Entity<AuctionUser>()
				.HasOptional(e => e.User)
				.WithMany(e => e.AuctionUsers)
				.HasForeignKey(e => e.User_Id);

			modelBuilder.Entity<AuctionInfo>()
				.HasMany(e => e.Auction)
				.WithOptional(e => e.AuctionInfo)
				.HasForeignKey(e => e.AuctionInfo_Id);
			base.OnModelCreating(modelBuilder);
		}
	}
}