﻿using Microsoft.Extensions.Logging;
using MusicProcessor.Application.Abstractions.DataAccess;
using MusicProcessor.Application.Abstractions.Interfaces;
using MusicProcessor.Domain.Entities;

namespace MusicProcessor.Application.Services;

public class SongProcessor(
    IGenreRepository genreRepository,
    IArtistRepository artistRepository,
    IStyleRepository styleRepository,
    ISongRepository songRepository,
    ILogger<SongProcessor> logger) : ISongProcessor
{
    private Dictionary<string, Artist> _existingArtists = new();
    private Dictionary<string, Genre> _existingGenres = new();
    private Dictionary<string, Style> _existingStyles = new();

    public async Task AddRawSongsToDbAsync(IEnumerable<Song> songs)
    {
        // Materialize songs collection once
        var songsList = songs.ToList();
        logger.LogInformation($"Processing {songsList.Count} songs");

        // Load all existing entities once
        _existingArtists = (await artistRepository.GetAllAsync()).ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
        logger.LogInformation($"Loaded {_existingArtists.Count} existing artists");

        _existingGenres = (await genreRepository.GetAllAsync()).ToDictionary(g => g.Name, StringComparer.OrdinalIgnoreCase);
        logger.LogInformation($"Loaded {_existingGenres.Count} existing genres");

        _existingStyles = (await styleRepository.GetAllAsync()).ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        logger.LogInformation($"Loaded {_existingStyles.Count} existing styles");

        foreach (var song in songsList)
        {
            logger.LogInformation($"Processing song: {song.Title} by {string.Join(", ", song.Artists.Select(a => a.Name))}");
            ProcessSongEntities(song);
        }

        await songRepository.AddRangeAsync(songsList);
        logger.LogInformation($"Successfully added {songsList.Count} songs to database");
    }

    private void ProcessSongEntities(Song song)
    {
        // Process Artists
        var artistsList = song.Artists.ToList();
        song.Artists.Clear();
        foreach (var artist in artistsList)
            if (_existingArtists.TryGetValue(artist.Name, out var existingArtist))
            {
                song.Artists.Add(existingArtist);
                logger.LogDebug($"Using existing artist: {artist.Name}");
            }
            else
            {
                _existingArtists[artist.Name] = artist;
                song.Artists.Add(artist);
                logger.LogDebug($"Adding new artist: {artist.Name}");
            }
        
        // Process Styles
        var stylesList = song.Styles.ToList();
        song.Styles.Clear();
        foreach (var style in stylesList)
            if (_existingStyles.TryGetValue(style.Name, out var existingStyle))
            {
                song.Styles.Add(existingStyle);
                logger.LogDebug($"Using existing style: {style.Name}");
            }
            else
            {
                _existingStyles[style.Name] = style;
                song.Styles.Add(style);
                logger.LogDebug($"Adding new style: {style.Name}");
            }
    }
}