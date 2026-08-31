using UnityEngine;

namespace Services
{
    public class God : MonoBehaviour
    {
        [SerializeField] private LocalizationService _localizationService;
        [SerializeField] private SettingsService _settingsService;
        [SerializeField] private DatabaseService _databaseService;

        public static LocalizationService Localization { get; private set; }
        public static SettingsService Settings { get; private set; }
        public static DatabaseService Database { get; private set; }

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
        }

        private void Init()
        {
            Localization.Init();
            Database.Init();
            Settings.Init();
        }
    }
}