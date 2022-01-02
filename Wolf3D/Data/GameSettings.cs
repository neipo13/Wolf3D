using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Data
{
    public enum WindowSize
    {
        TenEighty,
        ActualSize,
        FourKs
    }
    public class GameSettings
    {
        public bool isFullscreen { get; set; }
        public int musicVolume { get; set; }
        public int sfxVolume { get; set; }
        public int mouseSense { get; set; }

        public float mouseSenseMultiplier => mouseSense / 100f;

        public GameSettings()
        {
            isFullscreen = true;
            WindowWidth = 1920;
            WindowHeight = 1080;
            musicVolume = 100;
            mouseSense = 50;
            sfxVolume = 100;
        }


        public float sfxVolumeMultiplier => sfxVolume / 100f;
        public float musicVolumeMultiplier => musicVolume / 100f;

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
    }
}
