
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Casino
{
    public class DataManager : Singleton<DataManager>
    {
        private string profileName;
        private bool clearProfileOnStart;
     [SerializeField]  private GameData data = new GameData();
        private bool dataDirty = false;

        private DefaultProfile defaultProfile;
        private CasinoConfig casinoConfig;


        #region Runtime config links

        private GameTypeConfig currentGameTypeConfig;
        private SimpleGameConfig currentSimpleGameConfig;

        public GameTypeConfig CurrentGameTypeConfig => currentGameTypeConfig;
        public SimpleGameConfig CurrentSimpleGameConfig => currentSimpleGameConfig;

        public LobbyGamesConfig CurrentGameLobbyConfig
        {
            get { return casinoConfig.FindGame(GameManager.Instance.CurrentGame); }
        }

        public GameMeta CurrentSimpleGameMeta
        {
            get
            {
                var games = CurrentGameLobbyConfig.games;
                int foundIndex = Array.FindIndex(games, curGame => curGame.gameNameTag == GameManager.Instance.CurrentSimpleGameTag);
                if (foundIndex != -1)
                {
                    return games[foundIndex];
                }

                return null;
            }
        }

        #endregion

        #region Runtime generated data

        private int levelNumber = 0;        

        #endregion

        private List<IMoneyListener> moneyListeners = new List<IMoneyListener>();
        private List<IExperienceListener> expListeners = new List<IExperienceListener>();

        public CasinoConfig CasinoConfig
        {
            get { return casinoConfig; }
        }

        public void SetSoundValue(float soundValue, bool autoSave = true)
        {
            data.SoundValue = soundValue;
            if(autoSave)
                Save();
        }

        public void SetMusicValue(float musicValue, bool autoSave = true)
        {
            data.MusicValue = musicValue;
            if(autoSave)
                Save();
        }

        public float MusicValue
        {
            get { return data.MusicValue; }
        }

        public float SoundValue
        {
            get { return data.SoundValue; }
        }

        public SlotsGameData SlotsData
        {
            get { return data.SlotsData; }
        }

        private string FilePath
        {
            get { return Path.Combine(Application.persistentDataPath, profileName + ".json"); }
        }

        public GameData GetCurrentData
        {
            get { return data; } 
        }

        void Awake()
        {
            Debug.Log("DataManager awake");
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            Debug.Log("DataManager start");
            if (clearProfileOnStart)
            {
                Clear();
            }
            else
            {
               Load();
            }
        }

        public void Init(string profileName, bool clearProfileOnStart, DefaultProfile defaultProfile, CasinoConfig casinoConfig)
        {
            this.profileName = profileName;
            this.clearProfileOnStart = clearProfileOnStart;
            this.defaultProfile = defaultProfile;
            this.casinoConfig = casinoConfig;

            if (!Debug.isDebugBuild)
                this.clearProfileOnStart = false;

            Debug.Log("Profile path: " + FilePath);
        }


        public void Clear()
        {
            Debug.Log("CLEAR");
            if (defaultProfile != null)
                data = defaultProfile.profileData;
            else
                data = new GameData();

            Save();

            if (File.Exists(FilePath))
            {
                Load(); //this Load need for avoid deep copy of GameData class implement
            }
            else
            {
                Debug.LogError("Profile not saved! Check file system!");
                data = new GameData();
            }

            SetMusicValue(data.MusicValue, false);
            SetSoundValue(data.SoundValue, false);
        }

        [ContextMenu("Save")]
        public void Save()
        {
            Debug.Log("SAVE");
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, false));
        }

        void Load()
        {
            if (File.Exists(FilePath))
            {
                Debug.Log("LOAD");
                data = JsonUtility.FromJson<GameData>(File.ReadAllText(FilePath));

                UpdateRuntimeByLoadedData();
            }
            else
            {
                Clear();
            }
        }

        private void SetDataDirty()
        {
            if (dataDirty == false)
            {
                dataDirty = true;
                Invoke("DefferSave", 1.0f);
            }
        }

        void DefferSave()
        {
            Save();
            dataDirty = false;
        }

        void UpdateRuntimeByLoadedData()
        {
            levelNumber = CalculateLevelNumber();

            SetMusicValue(data.MusicValue, false);
            SetSoundValue(data.SoundValue, false);
        }

        #region Money

        public int Money
        {
            get { return data.PlayerData.Money; }
        }

#if UNITY_EDITOR
        public void SetMoney(int money)
        {
            int lastMoney = data.PlayerData.Money;

            data.PlayerData.Money = money;

            if (moneyListeners.Count > 0)
            {
                moneyListeners.ForEach(curListener => curListener.OnMoneyChange(data.PlayerData.Money, lastMoney));
            }
            SetDataDirty();
        }
#endif

        public void AddMoney(int money)
        {
            int lastMoney = data.PlayerData.Money;
            data.PlayerData.Money += Mathf.Max(money, 0);

            if (lastMoney != data.PlayerData.Money && moneyListeners.Count > 0)
            {
                moneyListeners.ForEach(curListener => curListener.OnMoneyChange(data.PlayerData.Money, lastMoney));
            }
            SetDataDirty();
        }

        public bool CheckForSpend(int money)
        {
            return money <= data.PlayerData.Money;
        }

        public int SpendMoney(int money)
        {
            if (CheckForSpend(money))
            {
                int lastMoney = data.PlayerData.Money;
                data.PlayerData.Money = Mathf.Max(data.PlayerData.Money - Mathf.Max(money, 0), 0);

                if (lastMoney != data.PlayerData.Money && moneyListeners.Count > 0)
                {
                    moneyListeners.ForEach(curListener => curListener.OnMoneyChange(data.PlayerData.Money, lastMoney));
                }
                SetDataDirty();
            }

            return data.PlayerData.Money;
        }

        public void AddMoneyListener(IMoneyListener listener)
        {
            if (!moneyListeners.Contains(listener))
                moneyListeners.Add(listener);
        }

        public void RemoveMoneyListener(IMoneyListener listener)
        {
            moneyListeners.Remove(listener);
        }

        #endregion

        #region Experience

        public int Experience
        {
            get { return data.PlayerData.Experience; }
        }

        public void AddExperience(int exp, bool enableLevelUp = true)
        {
            int lastExp = data.PlayerData.Experience;
            data.PlayerData.Experience += Mathf.Max(exp, 0);

            if (lastExp != data.PlayerData.Experience && expListeners.Count > 0)
            {
                expListeners.ForEach(curListener => curListener.OnExperienceChange(data.PlayerData.Experience, lastExp));
            }

            if (enableLevelUp)
                CheckForNewLevel();

            SetDataDirty();
        }

        public void CheckForNewLevel()
        {
            int newLevelNumber = CalculateLevelNumber();
            if (levelNumber != newLevelNumber)
            {
                levelNumber = newLevelNumber;
                expListeners.ForEach(curListener => curListener.OnLevelUp(levelNumber));
            }
        }

        int CalculateLevelNumber()
        {
            return 1; //need algorithm for generate level number from exp
        }

        #endregion

        #region Manage GameSlots data

        public int GenerateSpinId()
        {
            int newId = ++data.SlotsData.lastSpinId;

            SetDataDirty();

            return newId;
        }

        public void SetNewPeriod(PeriodData newData)
        {
            data.SlotsData.periodData = newData;

            SetDataDirty();
        }

        public void FinishPeriod(bool overrideNextTrend, bool positiveNextTrend = true)
        {
            data.SlotsData.periodData.finished = true;
            data.SlotsData.periodData.needOverrideNextTrend = overrideNextTrend;
            data.SlotsData.periodData.nextPositiveTrend = positiveNextTrend;

            SetDataDirty();
        }

        public void PushNewSpin(SpinInfo spinInfo)
        {
            //data.SlotsData.periodData.leftSpinCount++;

            //Debug.Log("New spin: " + spinInfo.ToString());

            SetDataDirty();
        }

        public void SetActivatedSpinModificator(SpinModificatorType modType)
        {
            data.SlotsData.modActivated = modType;
            SetDataDirty();
        }

        public void StartDecreaseBankForFinishPeriod()
        {
            data.SlotsData.finishPeriodModData.startSpinIdByDescreaseBank = data.SlotsData.lastSpinId;
            SetDataDirty();
        }

        public void StartPayUserPeriod(int bigWinSpinIndex)
        {
            data.SlotsData.userPaid = false;
            data.SlotsData.payUserModData.startSpinId = data.SlotsData.lastSpinId;
            data.SlotsData.payUserModData.bigWinSpinIndex = bigWinSpinIndex;
            SetDataDirty();
        }

        public void UserPaid()
        {
            data.SlotsData.userPaid = true;
            SetDataDirty();
        }

        public void UserAbsent()
        {
            data.SlotsData.userAbsent = true;
        }

        public void SetmaxBetMode(bool isMaxBet)
        {
            SlotsGame.Instance.CurrentBet = isMaxBet ? SlotsGame.Instance.MaxBet : SlotsGame.Instance.MaxBet - 10;
        }

        #endregion

        #region Manage games

        public void LoadGameTypeConfig(LobbyGamesConfig lobbyGameConfig)
        {
            if (currentGameTypeConfig != null && currentGameTypeConfig.gameType != lobbyGameConfig.gameType)
            {
                currentGameTypeConfig = null;
            }
            else if (currentGameTypeConfig != null && currentGameTypeConfig.gameType == lobbyGameConfig.gameType)
            {
                return;
            }

            if (lobbyGameConfig.gameType == CasinoGameType.Slots)
                currentGameTypeConfig = Resources.Load<SlotsGameConfig>(lobbyGameConfig.mainGameConfigInResources);

            currentSimpleGameConfig = null;
        }

        public void LoadSimpleGameConfig(string gameNameTag)
        {
            if (currentGameTypeConfig == null)
                return;

            var games = CurrentGameLobbyConfig.games;
            int foundIndex = Array.FindIndex(games, curGame => curGame.gameNameTag == gameNameTag);
            if (foundIndex != -1)
            {
                currentSimpleGameConfig = currentGameTypeConfig.LoadSimpleGameConfig(games[foundIndex].gameConfigInResources);
            }
        }

        public void ClearCurrentGameConfigs()
        {
            currentGameTypeConfig = null;
            currentSimpleGameConfig = null;
        }
        #endregion

        public void StartAbsentUserPeriod()
        {
            data.SlotsData.userAbsent = false;
            data.SlotsData.absentUserModData.startSpinId = data.SlotsData.lastSpinId;
        }

        public void MakeMaxBetPeriod(int countSpintToNext)
        {
            data.SlotsData.makeMaxBetModData.countSpinToNextPeriod = countSpintToNext;
            data.SlotsData.makeMaxBetModData.countNoMaxBet = 0;
        }   
    }
}
