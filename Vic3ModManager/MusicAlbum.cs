﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Vic3ModManager
{
    /// <summary>
    /// Represents a music album that contains a collection of songs.
    /// </summary>
    public class MusicAlbum
    {
        // Fields
        private string id;
        private LocalizableTextEntry title;
        private List<Song> songs;
        private string coverImagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicAlbum"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the album.</param>
        /// <param name="title">The title of the album.</param>
        /// <param name="songs">An array of songs contained in the album.</param>

        public MusicAlbum(string id = "untitled", LocalizableTextEntry? title = null, Song[]? songs = null, string coverImagePath = "")
        {
            Id = id;
            Title = title?? new LocalizableTextEntry("untitled");
            Songs = new List<Song>(songs ?? []);
            CoverImagePath = coverImagePath;
        }

        /// <summary>
        /// Adds a song to the album.
        /// </summary>
        /// <param name="song">The song to add to the album.</param>
        public void AddSong(Song song)
        {
            if (song == null)
                throw new ArgumentNullException(nameof(song));
            this.Songs.Add(song);
        }

        /// <summary>
        /// Removes a song from the album.
        /// </summary>
        /// <param name="song">The song to remove from the album.</param>
        public void RemoveSong(Song song)
        {
            if (song == null)
                throw new ArgumentNullException(nameof(song));
            this.Songs.Remove(song);
        }

        public void RemoveSongAt(int index)
        {
            if (index < 0 || index >= Songs.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            this.Songs.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the unique identifier for the album.
        /// </summary>
        public string Id { get => id; set => id = value ?? string.Empty; }

        /// <summary>
        /// Gets or sets the localizable title of the album.
        /// </summary>
        public LocalizableTextEntry Title { get => title; set => title = value ?? new(string.Empty); }

        /// <summary>
        /// Gets the list of songs in the album.
        /// </summary>
        public List<Song> Songs { get => songs; set => songs = value; }
        
        /// <summary>
        /// Gets or sets path to the album cover image.
        /// </summary>
        public string CoverImagePath { get => coverImagePath; set => coverImagePath = value; }

        public override string ToString()
        {
            return $"ID: {Id}, Title: {Title}";
        }
    }
}
