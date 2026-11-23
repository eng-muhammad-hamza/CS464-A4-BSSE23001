using UnityEngine;

public static class SaveData
{
    public const int levelCount = 5;

    const string unlockedKey = "unlocked";
    const string bestPrefix = "best";
    public const string musicVolumeKey = "musicVolume";
    public const string sfxVolumeKey = "sfxVolume";

    public const float defaultMusicVolume = 0.5f;
    public const float defaultSfxVolume = 1f;

    static string bestKey(int level) => bestPrefix + level;

    public static int unlocked => Mathf.Clamp(PlayerPrefs.GetInt(unlockedKey, 1), 1, levelCount);

    public static bool isUnlocked(int level) => level <= unlocked;

    public static void unlock(int level)
    {
        level = Mathf.Clamp(level, 1, levelCount);
        if (level <= unlocked) return;
        PlayerPrefs.SetInt(unlockedKey, level);
        PlayerPrefs.Save();
    }

    public static bool tryGetBest(int level, out float seconds)
    {
        string key = bestKey(level);
        if (PlayerPrefs.HasKey(key))
        {
            seconds = PlayerPrefs.GetFloat(key, 0f);
            return true;
        }
        seconds = 0f;
        return false;
    }

    public static bool recordTime(int level, float seconds)
    {
        if (level < 1 || level > levelCount) return false;
        if (tryGetBest(level, out float best) && seconds >= best) return false;
        PlayerPrefs.SetFloat(bestKey(level), seconds);
        PlayerPrefs.Save();
        return true;
    }

    public static string formatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        return minutes > 0 ? $"{minutes}:{seconds - minutes * 60f:00.0}" : $"{seconds:0.0} s";
    }

    public static float getVolume(string key)
    {
        float fallback = key == sfxVolumeKey ? defaultSfxVolume : defaultMusicVolume;
        return Mathf.Clamp(PlayerPrefs.GetFloat(key, fallback), 0.0001f, 1f);
    }

    public static void setVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp(value, 0.0001f, 1f));
        PlayerPrefs.Save();
    }
}
