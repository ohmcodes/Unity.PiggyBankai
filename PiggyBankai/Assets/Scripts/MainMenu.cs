using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button AudioOnButton;
    [SerializeField] private Button AudioOffButton;
    [SerializeField] private TMP_InputField txt_playername;
    [SerializeField] private LeaderboardManager leaderboardManager;

    private string[] playerNames = {
        "JumpMaster", "CoinCollector", "PlatformHero", "LevelJumper", "StarSeeker", "PipeSurfer", "BlockBuster", "FlagGetter", "MushroomMage", "FireFlower",
        "SuperRunner", "CloudLeaper", "CastleConqueror", "KoopaKiller", "YoshiRider", "WarpWhiz", "PowerUpPro", "ShellShocker", "BowserBeater", "PrincessSaver",
        "AdventureAce", "QuestQuasher", "DungeonDiver", "TreasureHunter", "GemGrabber", "KeyKeeper", "DoorDasher", "SecretSeeker", "BossBuster", "FinalFighter",
        "SpeedSprinter", "AgileJumper", "GravityGuru", "BounceBuddy", "SwingMaster", "ClimbKing", "SlidePro", "DashDemon", "FlipFlopper", "TwistTurner",
        "PixelPioneer", "RetroRunner", "ArcadeAce", "GameGuru", "LevelLord", "ScoreSmasher", "HighFlyer", "LowCrawler", "SideScroller", "VerticalVoyager",
        "EpicExplorer", "LegendaryLeaper", "MythicMover", "HeroicHopper", "BraveBounder", "CourageousClimber", "DaringDasher", "FearlessFlyer", "GallantGamer",
        "IntrepidJumper", "ValiantVault", "BoldBouncer", "StalwartSprinter", "ResoluteRunner", "TenaciousTurner", "UnwaveringWarrior", "VigilantVoyager", "ZealousZoomer"
    };
    void Start()
    {
        int muted = PlayerPrefs.GetInt("Muted", 1); // Default to 1 (audio on)
        AudioListener.volume = muted;
        if (muted == 0)
        {
            AudioOnButton.gameObject.SetActive(false);
            AudioOffButton.gameObject.SetActive(true);
        }
        else
        {
            AudioOnButton.gameObject.SetActive(true);
            AudioOffButton.gameObject.SetActive(false);
        }

        // Load or randomize player name
        LoadOrRandomizePlayerName();
    }



    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void AudioOn()
    {
        AudioListener.volume = 1f;

        PlayerPrefs.SetInt("Muted", 1);
        PlayerPrefs.Save();
    }

    public void AudioOff()
    {
        AudioListener.volume = 0f;

        PlayerPrefs.SetInt("Muted", 0);
        PlayerPrefs.Save();
    }

    public void RandomizePlayerName()
    {
        if (txt_playername != null && playerNames.Length > 0)
        {
            int randomIndex = Random.Range(0, playerNames.Length);
            string randomName = playerNames[randomIndex];
            txt_playername.text = randomName;

            SavePlayerName();
        }
    }

    private void LoadOrRandomizePlayerName()
    {
        if (txt_playername != null)
        {
            // Try to load saved player name
            string savedName = PlayerPrefs.GetString("PlayerName", playerNames[Random.Range(0, playerNames.Length)]);
            
            if (!string.IsNullOrEmpty(savedName))
            {
                // Use saved name
                txt_playername.text = savedName;

                // Sync name with server
                if (leaderboardManager != null)
                {
                    leaderboardManager.UpdatePlayerName(savedName);
                }
                //Debug.Log("Loaded player name: " + savedName);
            }
            else
            {
                // Generate and save new random name
                RandomizePlayerName();
                //Debug.Log("No saved name found. Generated random name: " + txt_playername.text);
            }
        }
    }

    public void SavePlayerName()
    {
        if (txt_playername != null)
        {
            string playerName = txt_playername.text.Trim();
            if (!string.IsNullOrEmpty(playerName))
            {
                PlayerPrefs.SetString("PlayerName", playerName);
                PlayerPrefs.Save();

                // Update name on server
                if (leaderboardManager != null)
                {
                    leaderboardManager.UpdatePlayerName(playerName);
                }
            }
        }
    }
}
