#nullable enable
using System;
using System.Collections.Generic;

namespace SteamGameParse.Data.Models
{
    public class AppData : Entity
    {
        public AppData()
        {
            Requirements = new List<SystemRequirementses>();
            Genres = new List<Genre>();
        }
        public string Name { get; set; }
        public string Developer { get; set; }
        public string Published { get; set; }
        public string ReleaseDate { get; set; }
        public List<Genre> Genres { get; set; }
        public List<SystemRequirementses> Requirements { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
    }
}