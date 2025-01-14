﻿using Music_Processor.Interfaces;

namespace Music_Processor.Services;

public class FileService : IFileService
{
    public string[] GetAllFoldersInPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The path cannot be null or empty.", nameof(path));
        }

        return Directory.GetDirectories(path);
    }

    public string[] GetAllAudioFilesInFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The path cannot be null or empty.", nameof(path));
        }

        string[] audioFileFormats = Constants.Constants.AudioFileFormats;

        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        return files.Where(file => audioFileFormats.Contains(Path.GetExtension(file).ToLower())).ToArray();
    }

    public string? GetSpotDLFileInFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The path cannot be null or empty.", nameof(path));
        }

        var spotdlFile = Directory.GetFiles(path, "*.spotdl", SearchOption.AllDirectories).FirstOrDefault();

        return spotdlFile;
    }
}