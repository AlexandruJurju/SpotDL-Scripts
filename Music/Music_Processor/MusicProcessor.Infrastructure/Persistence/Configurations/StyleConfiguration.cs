﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicProcessor.Domain.Entities;

namespace MusicProcessor.Infrastructure.Persistence.Configurations;

public class StyleConfiguration : IEntityTypeConfiguration<Style>
{
    public void Configure(EntityTypeBuilder<Style> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.RemoveFromSongs)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => e.Name)
            .IsUnique();

        // Many-to-many relationship with Genre
        builder.HasMany(e => e.Genres)
            .WithMany(e => e.Styles)
            .UsingEntity("genre_styles");

        // Many-to-many relationship with Song
        builder.HasMany(e => e.Songs)
            .WithMany(e => e.Styles)
            .UsingEntity("song_styles");
    }
}