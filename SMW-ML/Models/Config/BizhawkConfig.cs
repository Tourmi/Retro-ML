using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace SMW_ML.Models.Config
{
    public class BizhawkConfig
    {
        private dynamic? data;

        private static readonly JsonSerializerSettings JSON_CONFIG = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        #region Constructor
        public BizhawkConfig(string path)
        {
            data = JsonConvert.DeserializeObject(File.ReadAllText(path));
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
        public void Serialize(string path)
        {
            string emulatorCfg = JsonConvert.SerializeObject(data, Formatting.Indented, JSON_CONFIG);
            File.WriteAllText(path, emulatorCfg);
        }
        #endregion

    }
}
