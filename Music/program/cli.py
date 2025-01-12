﻿import sys
from pathlib import Path

from program.config import Config
from program.file_system_handler import FileSystemHandler
from program.metadata_processor import MetadataProcessor
from program.playlist_processor import PlaylistProcessor
from program.spotdl_wrapper import SpotDLWrapper


class CLI:
    def __init__(self, base_dir: Path):
        self.base_dir = base_dir
        self.config = Config(base_dir)
        self.fs_handler = FileSystemHandler()
        self.spotdl = SpotDLWrapper()
        self.processor = PlaylistProcessor(self.config)

    def run(self) -> None:
        """Main CLI loop"""
        while True:
            self._print_menu()
            choice = input("Choice (1-5): ").strip()

            try:
                self._handle_choice(choice)
            except KeyboardInterrupt:
                print("\nOperation cancelled by user")
            except Exception as e:
                print(f"\nError: {e}")

    def _print_menu(self) -> None:
        print("\nSpotify Playlist Sync Tool")
        print("------------------------")
        print("1. First-time sync")
        print("2. Update existing sync")
        print("3. Fix genres")
        print("4. Write song list")
        print("5. Metadata backup")
        print("6. Apply metadata from json ")
        print("7. Exit\n")

    def _handle_choice(self, choice: str) -> None:
        """Handle menu choice"""
        actions = {
            "1": self._handle_new_sync,
            "2": self._handle_existing_sync,
            "3": self._handle_fix_genres,
            "4": self._write_song_list,
            "5": self._handle_scan_metadata,
            "6": self._handle_apply_metadata_from_json,
            "7": sys.exit
        }

        if choice in actions:
            actions[choice]()
        else:
            print("Invalid choice!")

    def _handle_scan_metadata(self) -> None:
        playlists = self.fs_handler.get_available_playlists(self.base_dir)
        if playlists:
            print("\nAvailable playlists:\n" + "\n".join(playlists))

        playlist_name = input("\nEnter playlist name: ").strip()
        playlist_dir = self.base_dir / playlist_name

        if playlist_dir.exists():
            scanner = MetadataProcessor(playlist_dir)
            metadata_list = scanner.scan_directory()
            scanner.save_metadata_to_file(metadata_list)
        else:
            print(f"\nError: Folder {playlist_dir} not found")

    def _handle_new_sync(self) -> None:
        """Handle new playlist sync"""
        playlist_url = input("\nEnter Spotify playlist URL: ").strip()
        playlist_name = input("Enter playlist name: ").strip()
        self.spotdl.new_sync(playlist_url, playlist_name, self.base_dir)

    def _handle_existing_sync(self) -> None:
        """Handle existing playlist sync"""
        playlists = self.fs_handler.get_available_playlists(self.base_dir)
        if playlists:
            print("\nAvailable playlists:\n" + "\n".join(playlists))

        playlist_name = input("\nEnter playlist name: ").strip()
        self.spotdl.update_sync(playlist_name, self.base_dir)

    def _handle_fix_genres(self) -> None:
        """Handle fixing genres"""
        playlists = self.fs_handler.get_available_playlists(self.base_dir)
        if playlists:
            print("\nAvailable playlists:\n" + "\n".join(playlists))

        playlist_name = input("\nEnter playlist name: ").strip()
        playlist_dir = self.base_dir / playlist_name

        if playlist_dir.exists():
            self.processor.process_playlist(playlist_dir)
        else:
            print(f"\nError: Folder {playlist_dir} not found")

    def _write_song_list(self) -> None:
        playlists = self.fs_handler.get_available_playlists(self.base_dir)
        if playlists:
            print("\nAvailable playlists:\n" + "\n".join(playlists))

        playlist_name = input("\nEnter playlist name: ").strip()
        playlist_dir = self.base_dir / playlist_name

        if playlist_dir.exists():
            self.fs_handler.write_songs_list(playlist_dir)
        else:
            print(f"\nError: Folder {playlist_dir} not found")

    def _handle_apply_metadata_from_json(self) -> None:
        """Handle applying metadata from JSON file to songs"""
        playlists = self.fs_handler.get_available_playlists(self.base_dir)
        if playlists:
            print("\nAvailable playlists:\n" + "\n".join(playlists))

        playlist_name = input("\nEnter playlist name: ").strip()
        playlist_dir = self.base_dir / playlist_name

        if not playlist_dir.exists():
            print(f"\nError: Folder {playlist_dir} not found")
            return

        # Get metadata file path
        metadata_file = playlist_dir / "metadata_scan.json"
        if not metadata_file.exists():
            print(f"\nError: Metadata file not found at: {metadata_file}")
            return

        # Create metadata processor and load metadata
        metadata_processor = MetadataProcessor(playlist_dir)
        metadata_lookup = metadata_processor.load_metadata_from_json(metadata_file)

        if not metadata_lookup:
            print("\nError: No metadata found in the JSON file")
            return

        # Process the metadata using playlist processor
        self.processor.process_playlist_with_metadata(playlist_dir, metadata_lookup)
