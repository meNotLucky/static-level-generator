using System.Linq;

namespace LevelGenerator.Utility
{
    public enum ExitDirection { Top, Right, Bottom, Left }
    
    public enum GridAlignment { Horizontal, Vertical }
    
    public static class GridLevelUtility
    {
        // --- UTILITY FUNCTIONS --- //

        public static bool ValidateSeed(string seed)
        {
            var seedValid = true;
            var stringData = seed.Split('-');
            if (stringData.Length != 4) seedValid = false;
            if (stringData.Any(t => t.Length < 7 || t.Length > 10))
            {
                seedValid = false;
            }

            return seedValid;
        }

        public static uint[] ExtractSeedData(string seed)
        {
            var seedData = new uint[4];
            var stringData = seed.Split('-');
            for (var i = 0; i < stringData.Length; i++)
            {
                seedData[i] = uint.Parse(stringData[i]);
            }

            return seedData;
        }
    }
}