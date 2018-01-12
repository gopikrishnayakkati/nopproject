﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Shipping;

namespace Nop.Data.Mapping.Shipping
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class ShippingMethodMap : NopEntityTypeConfiguration<ShippingMethod>
    {
        public override void Configure(EntityTypeBuilder<ShippingMethod> builder)
        {
            base.Configure(builder);
            builder.ToTable("ShippingMethod");
            builder.HasKey(sm => sm.Id);
            builder.Property(sm => sm.Name).IsRequired().HasMaxLength(400);
        }
    }
}
