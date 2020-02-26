#nullable enable
namespace SteamGameParse.Data.Models
{
    public class SystemRequirementses : Entity
    {
        public SystemType SystemType { get; set; }
        public RequirementsType RequirementsType { get; set; }
        public string? Processor { get; set; }
        public string? Memory { get; set; }
        public string? Graphics { get; set; }
        public string? Os { get; set; }
        public string? Storage { get; set; }
        public string? DirectX { get; set; }
    }
}