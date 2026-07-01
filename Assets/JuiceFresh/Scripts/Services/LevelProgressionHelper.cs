namespace JuiceFresh.Scripts
{
    public static class LevelProgressionHelper
    {
        public static int CalculateLevel(int playerLevel)
        {
            if (playerLevel <= 45)
                return playerLevel;

            int beyond45 = playerLevel - 45;
            int cyclePosition = beyond45 % 36;
            int result = cyclePosition + 10;

            if (result > 45)
                result = (result - 45) + 9;

            return result;
        }
    }
}
