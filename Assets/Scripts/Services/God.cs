using UnityEngine;

namespace Services
{
    public class God : MonoBehaviour
    {
        [SerializeField] private LocalizationService _localizationService;
        [SerializeField] private SettingsService _settingsService;
        [SerializeField] private DatabaseService _databaseService;
        [SerializeField] private InputService _inputService;
        [SerializeField] private AssetsService _assetsService;

        public static LocalizationService Localization { get; private set; }
        public static SettingsService Settings { get; private set; }
        public static DatabaseService Database { get; private set; }
        public static InputService Input { get; private set; }
        public static AssetsService Assets { get; private set; }

        void Awake()
        {
            Assign();
            Init();
        }

        private void Assign()
        {
            Localization = _localizationService;
            Database = _databaseService;
            Settings = _settingsService;
            Input = _inputService;
            Assets = _assetsService;
        }

        private void Init()
        {
            Localization.Init();
            Database.Init();
            Settings.Init();
            Input.Init();
            Assets.Init();
        }
    }
}