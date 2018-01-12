﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Logging;

namespace Nop.Data.Mapping.Logging
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class ActivityLogMap : NopEntityTypeConfiguration<ActivityLog>
    {
        public override void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            base.Configure(builder);
            builder.ToTable("ActivityLog");
            builder.HasKey(al => al.Id);
            builder.Property(al => al.Comment).IsRequired();
            builder.Property(al => al.IpAddress).HasMaxLength(200);
            builder.Property(al => al.EntityName).HasMaxLength(400);
            builder.HasOne(al => al.ActivityLogType)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey(al => al.ActivityLogTypeId);

            builder.HasOne(al => al.Customer)
                .WithMany()
                .IsRequired(true)
                .HasForeignKey(al => al.CustomerId);
        }
    }
}