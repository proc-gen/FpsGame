using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FpsGame.Containers
{
    public class SettingsContainer
    {
        public string Resolution { get; set; }
        public string WindowMode { get; set; }
        public float MouseSensitivity { get; set; } = 0.5f;
        public float ControllerSensitivity { get; set; } = 5f;
    }
}
