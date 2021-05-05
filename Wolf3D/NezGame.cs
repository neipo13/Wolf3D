using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Nez;
using System;
using System.IO;
using Wolf3D.Data;

namespace Wolf3D
{
    public class NezGame : Core
    {
        public const int designWidth = 640;
        public const int designHeight = 360;
        public const int uiSize = 72;

        public static int h => designHeight - uiSize + 1;
        public static SoundEffectInstance effectInstance;
        public static string currentSongName;

        public static SoundEffect mainMenuMusic;
        public static SoundEffect world1Music;

        public static GameSettings gameSettings;

        public NezGame() : base(windowTitle: "FROGENSTEIN 3D", isFullScreen: false, width: 640, height: 360)
        {
            var policy = Scene.SceneResolutionPolicy.BestFit;
            Scene.SetDefaultDesignResolution(designWidth, designHeight, policy, 0, 0);
            Window.AllowUserResizing = false;
            ExitOnEscapeKeypress = false;

            LoadSettings();
            
            Nez.Input.MaxSupportedGamePads = 1;
            IsMouseVisible = false;
        }
        protected override void Initialize()
        {
            base.Initialize();

            mainMenuMusic = Content.Load<SoundEffect>($"music/showdown_at_the_frog_carol");
            world1Music = Content.Load<SoundEffect>($"music/Frogtrack2");
            PlayMusic(mainMenuMusic);
            this.Activated += NezGame_Activated;
            Input.MouseLockState = MouseLockState.Unlocked;

            LoadSettings();
            Scene = new Scenes.SplashScreenScene(Scenes.SplashType.AllieCatGames);
        }

        public static void PlayMusic(SoundEffect soundEffect)
        {
            if (currentSongName == soundEffect.Name) return;//we are already playing that song
            if(effectInstance != null)
            {
                effectInstance.Stop();
            }
            effectInstance = soundEffect.CreateInstance();
            effectInstance.IsLooped = true;
            effectInstance.Volume = 0.2f * gameSettings.musicVolumeMultiplier;
            effectInstance.Play();
            currentSongName = soundEffect.Name;
        }

        public static void ApplySoundSettings()
        {
            if(effectInstance != null)
            {
                effectInstance.Volume = 0.2f * gameSettings.musicVolumeMultiplier;
            }
        }

        private void NezGame_Activated(object sender, System.EventArgs e)
        {
            Nez.Input.MouseLockState = Nez.Input.MouseLockState;
        }


        protected void LoadSettings()
        {
            //see if the file is there
            bool fileExists = File.Exists("settings.json");


            if (fileExists)
            {
                //load the settings
                try
                {
                    var json = File.ReadAllText("settings.json");
                    gameSettings = JsonConvert.DeserializeObject<GameSettings>(json);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("SETTINGS FILE CORRUPTED SOMEHOW");
                    gameSettings = new GameSettings();
                    SaveSettings(gameSettings);
                }
            }
            else {
                gameSettings = new GameSettings();
                SaveSettings(gameSettings);
            }
            ApplyScreenSettings();
        }

        public static void ApplyScreenSettings()
        {
            //setup window based on settings
            Screen.IsFullscreen = gameSettings.isFullscreen;
            Screen.SetSize(gameSettings.WindowWidth, gameSettings.WindowHeight);
            Screen.ApplyChanges();
        }

        public static void SaveSettings(GameSettings settings)
        {
            gameSettings = settings;
            //save the JSON
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(gameSettings));
        }

        [Nez.Console.Command("window", "set the window size")]
        public static void SetWindowType(string type = "")
        {
            switch (type.ToLower())
            {
                case "actual":
                    gameSettings.WindowHeight = NezGame.designHeight;
                    gameSettings.WindowWidth = NezGame.designWidth;
                    break;
                case "ten":
                    gameSettings.WindowHeight = 1080;
                    gameSettings.WindowWidth = 1920;
                    break;
                case "full":
                    gameSettings.isFullscreen = true;
                    break;
                case "windowed":
                    gameSettings.isFullscreen = false;
                    break;
                default:
                    gameSettings.WindowHeight = 1080;
                    gameSettings.WindowWidth = 1920;
                    break;
            }
            ApplyScreenSettings();
            SaveSettings(gameSettings);
        }

        [Nez.Console.Command("sfx", "set the sfx volume")]
        public static void SetSfxVolume(string vol = "")
        {
            int volume = 0;
            bool parsed = int.TryParse(vol, out volume);
            if (!parsed)
            {
                Nez.Console.DebugConsole.Instance.Log($"CANNOT PARSE `{vol}` to int");
            }
            //ensure its 0-100
            volume = Math.Max(0, volume);
            volume = Math.Min(100, volume);

            gameSettings.sfxVolume = volume;
            SaveSettings(gameSettings);
        }


        [Nez.Console.Command("music", "set the music volume")]
        public static void SetMusicVolume(string vol = "")
        {
            int volume = 0;
            bool parsed = int.TryParse(vol, out volume);
            if (!parsed)
            {
                Nez.Console.DebugConsole.Instance.Log($"CANNOT PARSE `{vol}` to int");
            }
            //ensure its 0-100
            volume = Math.Max(0, volume);
            volume = Math.Min(100, volume);

            gameSettings.musicVolume = volume;
            ApplySoundSettings();
            SaveSettings(gameSettings);
        }
    }
}
