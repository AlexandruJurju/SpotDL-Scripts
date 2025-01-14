﻿using Microsoft.Extensions.Logging;
using Music_Processor.Constants;
using Music_Processor.Interfaces;

namespace Music_Processor.CLI.Commands;

public class WriteMetadataFileCommand : IMenuCommand
{
    private readonly IFileService _fileService;
    private readonly ILogger<WriteMetadataFileCommand> _logger;
    private readonly IMetadataService _metadataService;

    public WriteMetadataFileCommand(ILogger<WriteMetadataFileCommand> logger, IFileService fileService, IMetadataService metadataService)
    {
        _logger = logger;
        _fileService = fileService;
        _metadataService = metadataService;
    }

    public string Name => "Write metadata file";

    public async Task ExecuteAsync()
    {
        var baseDirectory = AppPaths.PlaylistsDirectory;
        string[] availablePlaylists = _fileService.GetAllFoldersInPath(baseDirectory);

        foreach (var playlist in availablePlaylists)
        {
            var folderName = Path.GetFileName(playlist);
            Console.WriteLine(folderName);
        }

        Console.Write("Enter playlist name: ");
        var playlistName = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(playlistName))
        {
            Console.WriteLine("Please provide a valid playlist name.");
            return;
        }

        // check if entered playlist name exists in available playlists
        var playlistFolders = availablePlaylists.Select(path => Path.GetFileName(path)).ToList();
        if (!playlistFolders.Contains(playlistName))
        {
            Console.WriteLine("Playlist does not exist.");
            return;
        }

        // TODO: use hashing here as well, use a file for the playlist metadata, hash it if its the same read from that file; otherwise check each song hash
        try
        {
            var folderPath = Path.Combine(AppPaths.PlaylistsDirectory, playlistName);
            var playlistMetadata = _metadataService.GetPlaylistSongsMetadata(folderPath);
            await _metadataService.SaveMetadataToFileAsync(playlistMetadata, playlistName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write metadata for playlist");
            Console.WriteLine($"\nError: {ex.Message}");
        }
    }
}