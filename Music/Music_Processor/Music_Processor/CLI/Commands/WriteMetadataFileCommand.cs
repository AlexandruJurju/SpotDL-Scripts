﻿namespace Music_Processor.CLI.Commands;

public class WriteMetadataFileCommand : IMenuCommand
{
    public string Name => "Write metadata file";
    public int MenuNumber => MenuChoices.WriteMetadataFile;

    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}