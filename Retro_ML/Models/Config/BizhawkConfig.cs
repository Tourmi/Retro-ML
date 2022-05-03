using Newtonsoft.Json;
using Retro_ML.Utils;

namespace Retro_ML.Models.Config
{
    public class BizhawkConfig
    {
        private dynamic? data;

        #region Constructor
        private BizhawkConfig(string json)
        {
            data = JsonConvert.DeserializeObject(json);
        }
        #endregion

        #region Properties

        public bool SoundEnabled
        {
            get => (bool)data!.SoundEnabled;
            set => data!.SoundEnabled = value;
        }

        public int Volume
        {
            get => (int)data!.SoundVolume;
            set => data!.SoundVolume = value;
        }

        public bool Unthrottled
        {
            get => (bool)data!.Unthrottled;
            set => data!.Unthrottled = value;
        }

        public int ZoomFactor
        {
            get => (int)data!.TargetZoomFactors.SNES;
            set => data!.TargetZoomFactors.SNES = value;
        }

        public int DispMethod
        {
            get => (int)data!.DispMethod;
            set => data!.DispMethod = value;
        }

        public int DispSpeedupFeatures
        {
            get => (int)data!.DispSpeedupFeatures;
            set => data!.DispSpeedupFeatures = value;
        }

        #endregion

        #region Methods

        public string Serialize() => JsonConvert.SerializeObject(data, Formatting.Indented, SerializationUtils.JSON_PASCAL_CASE_CONFIG);

        public static BizhawkConfig Deserialize(string json)
        {
            BizhawkConfig cfg = new(json);

            return cfg;
        }
        #endregion

    }
}
