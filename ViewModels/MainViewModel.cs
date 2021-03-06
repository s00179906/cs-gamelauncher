﻿using GameLauncher.Models;
using GameLauncher.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using GameLauncher.Views;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;
using System.Windows.Threading;
using GameLauncher.APIControllers;

namespace GameLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Members
        private ObservableCollection<Game> _games;
        private bool firstTimeConfiguration;
        private string _randomSelectedGameScreenshot;
        private decimal _imageWidth;
        private decimal _imageHeight;
        private string _gamesFoundText;
        private string _searchForGamesText;
        private decimal _imageWidthAndHeight = 1;
        #endregion

        #region Public Properties 
        public string RandomSelectedGameScreenshot
        {
            get { return _randomSelectedGameScreenshot; }
            set
            {
                _randomSelectedGameScreenshot = value;
                OnPropertyChanged(nameof(RandomSelectedGameScreenshot));
            }
        }
        public ObservableCollection<Game> Games
        {
            get { return _games; }
            set
            {
                _games = value;
                OnPropertyChanged(nameof(Games));
            }
        }
        public ICollectionView FilteredGames { get; set; }
        public CommandRunner AddFolderPathCommand { get; private set; }
        public CommandRunner DeleteFolderPathCommand { get; private set; }
        public CommandRunner FilterGamesCommand { get; private set; }
        public CommandRunner ResetAllSettingsCommand { get; private set; }
        public CommandRunner PassSelectedGameToViewCommand { get; private set; }
        public CommandRunner ScanGamesCommand { get; set; }
        public GameScanner Scanner { get; set; }
        public GameScanner GameScanner { get; set; }
        public static Game SelectedGame { get; set; }
        public Platform SelectedFolder { get; set; }
        public ChooseGameExesView Window { get; set; }
        public static int GameID { get; set; }
        
        public IGDBAPIController APIController { get; set; }
        public string JSONResponse { get; set; }
        public string Cover { get; set; }
        public List<Game> GameCovers { get; set; }
        public CommandRunner CloseSettingsCommand { get; set; }
        public ObservableCollection<string> GameGenresCBX { get; private set; }
        public Platform SelectedFolderValue { get; set; }
        public Random Random = new Random();
        public ObservableCollection<Game> MyProperty { get; set; }
        public decimal ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                OnPropertyChanged(nameof(ImageWidth));
            }
        }
        public decimal ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                OnPropertyChanged(nameof(ImageHeight));
            }
        }
        public string GamesFoundText
        {
            get { return _gamesFoundText; }
            set
            {
                _gamesFoundText = value;
                OnPropertyChanged(nameof(GamesFoundText));
            }
        }
        public string SearchForGamesText
        {
            get { return _searchForGamesText; }
            set
            {
                _searchForGamesText = value;
                OnPropertyChanged(nameof(SearchForGamesText));
                SearchForAGame(_searchForGamesText);
            }
        }
        public decimal ImageWidthAndHeight
        {
            get { return _imageWidthAndHeight; }
            set
            {
                _imageWidthAndHeight = value;
                OnPropertyChanged(nameof(ImageWidthAndHeight));
                MakeGamesBiggerOrSmaller();
            }
        }
        public SGDBAPIController SGDBAPIController { get; set; }

        private string _randomeWideGameCover;
        public string RandomWideGameCover
        {
            get
            {
                return _randomeWideGameCover;
            }
            set
            {
                _randomeWideGameCover = value;
                OnPropertyChanged(nameof(RandomWideGameCover));
            }
        }



        private string _testCover;

        public event PropertyChangedEventHandler PropertyChanged;

        public string TestCover
        {
            get { return _testCover; }
            set
            {
                _testCover = value;
                OnPropertyChanged(nameof(TestCover));
            }
        }

        public CommandRunner MakeGamesBiggerCommand { get; set; }


        #endregion

        #region Constructor
        public MainViewModel()
        {
            //MakeGamesBiggerCommand = new CommandRunner(MakeBigger);
            SearchForGamesText = "Search for a game";
            GameGenresCBX = new ObservableCollection<string>
            {
                "All Genres",
                "----------------------------------------",
                "FPS",
                "RPG",
                "ACTION",
                "ADVENTURE",
                "HORROR",
                "RACING"
            };
            //Properties.Settings.Default.Reset();
            CloseSettingsCommand = new CommandRunner(CloseSettings);
            //FirstTimeConfiguration();
            ScanGamesCommand = new CommandRunner(ScanGames);
            AddFolderPathCommand = new CommandRunner(AddDir);
            DeleteFolderPathCommand = new CommandRunner(DeleteDir);
            FilterGamesCommand = new CommandRunner(FilterGamesByPlatform);
            ResetAllSettingsCommand = new CommandRunner(ResetAllSettings);
            PassSelectedGameToViewCommand = new CommandRunner(PassSelectedGameToGameDetailedViewModel);
            Scanner = new GameScanner();
            Scanner.Scan();
            Games = Scanner.GetExecutables();

            //SGDBAPIController = new SGDBAPIController(Games);
            //PickRandomWideGameCover();

            GamesFoundText = string.Format($"We found {Games.Count.ToString()} games installed on your system.");
            MyProperty = Scanner.GetExecutables();
            FilteredGames = CollectionViewSource.GetDefaultView(Games);
            //APIController = new IGDBAPIController(Games);
            //TEST();
            ImageHeight = 208;
            ImageWidth = 400;
        }
        private void PickRandomWideGameCover()
        {
            foreach (Game game in Games)
            {
                //RandomWideGameCover = Games[0].WideGameCover[Random.Next(0, Games[0].WideGameCover.Count)];
                if (game.WideGameCovers.Count != 0)
                {
                    game.RandomWideGameCover = game.WideGameCovers[Random.Next(0, game.WideGameCovers.Count)];
                }
            }
        }
        private void MakeGamesBiggerOrSmaller()
        {
            int w = 400;
            int h = 208;

            decimal x = ImageWidthAndHeight;

            ImageWidth = w * x;
            ImageHeight = h * x;
        }

        private void MakeBigger(object obj)
        {
            ImageWidthAndHeight += 0.1M;
        }

        private void SearchForAGame(string searchedGame)
        {
            if (!String.IsNullOrEmpty(searchedGame) && !searchedGame.Equals("Search for a game"))
                FilteredGames.Filter = game => ((Game)game).Name.ToUpper().Contains(searchedGame.ToUpper());

            else if (String.IsNullOrEmpty(searchedGame) && !searchedGame.Equals("Search for a game"))
            {
                FilteredGames.Filter = game => game.Equals(game);
            }

        }

        private void PickRandomGameScreenshot()
        {
            RandomSelectedGameScreenshot = SelectedGame.GameScreenshots[Random.Next(0, SelectedGame.GameScreenshots.Count)];
        }

        private void TEST()
        {
            if (!File.Exists("gameDataAPI.json"))
            {
                File.WriteAllText("gameDataAPI.json", "[]");
            }

            var file = File.ReadAllText("gameDataAPI.json");

            var gameDataAPI = JsonConvert.DeserializeObject<List<Game>>(file);

            if (file == "[]")
            {
                APIController.GetGameCovers();
                SaveGameDataFromAPIToFile();
            }
            foreach (var game in Games)
            {
                // give back no game found?
                ReadGameDataFromFile();
                //this is the problem!!
                if (!gameDataAPI.Any(m => m.Name == game.Name))
                {
                    APIController.GetGameCovers();
                    SaveGameDataFromAPIToFile();
                }
            }
            if (file != "[]")
            {
                ReadGameDataFromFile();
            }

        }

        private void SaveGameDataFromAPIToFile()
        {
            var initialJson = File.ReadAllText(@"gameDataAPI.json");

            var list = JsonConvert.DeserializeObject<List<Game>>(initialJson);
            foreach (var game in Games)
            {
                if (!list.Any(m => m.Name == game.Name))
                {
                    list.Add(game);
                }
            }

            //create steam game

            var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(@"gameDataAPI.json", string.Empty);
            File.AppendAllText(@"gameDataAPI.json", convertedJson);
        }

        private void ReadGameDataFromFile()
        {
            var initialJson = File.ReadAllText(@"gameDataAPI.json");
            var list = JsonConvert.DeserializeObject<List<Game>>(initialJson);
            foreach (var game in Games)
            {
                foreach (var gameFromList in list)
                {

                    if (game.Name.Equals(gameFromList.Name))
                    {
                        game.GameCover = gameFromList.GameCover;
                        game.GameScreenshots = gameFromList.GameScreenshots;
                    }
                }
            }
        }

        private void PassSelectedGameToGameDetailedViewModel(object obj)
        {
            SelectedGame = obj as Game;
            GameLauncherViewModel.MainFrame.Navigate(new GameDetailedViewModel());

            //PickRandomGameScreenshot();
        }
        #endregion

        #region VM Methods
        private void CloseSettings(object obj)
        {
            GameLauncherViewModel.MainFrame.Content = new MainViewModel();
        }
        private void FirstTimeConfiguration()
        {
            firstTimeConfiguration = Properties.Settings.Default.FirstTimeConfig;
            if (firstTimeConfiguration)
            {
                File.WriteAllText(@"game.json", string.Empty);
                File.WriteAllText(@"game.json", "[]");
                Properties.Settings.Default.FolderPaths.Clear();
                Properties.Settings.Default.FirstTimeConfig = false;
                Properties.Settings.Default.Save();
            }
        }

        private void SetGameCover()
        {
            //foreach (var cover in GameCovers)
            //{
            //    foreach (var game in Games)
            //    {
            //        string gameNameFromOC = game.Name;
            //        string gameNameFromCovers = cover.Name;
            //        var charsToRemove = new string[] { ":", "-", "'", " " };

            //        foreach (var c in charsToRemove)
            //        {
            //            gameNameFromCovers = gameNameFromCovers.Replace(c, string.Empty);
            //            gameNameFromOC = gameNameFromOC.Replace(c, string.Empty);
            //        }

            //        if (gameNameFromOC.ToUpper() == gameNameFromCovers.ToUpper())
            //        {
            //            if (cover.GameCover != null || String.IsNullOrEmpty(cover.GameCover))
            //            {
            //                game.GameCover = cover.GameCover;
            //            }
            //            else
            //            {
            //                game.GameCover = "https://sisterhoodofstyle.com/wp-content/uploads/2018/02/no-image-1.jpg";
            //            }
            //            game.GameScreenshots = cover.GameScreenshots;
            //        }
            //    }
            //}
        }

        //Used to scan games when settings have been reset.
        private void ScanGames(object obj)
        {
            Scanner.Scan();
            RefreshGames();
        }

        private void AddDir(object obj)
        {
            string dirToAdd = Helper.SelectDir();

            if (!string.IsNullOrEmpty(dirToAdd))
            {
                Scanner.LibraryDirectories.Add(new Platform
                {
                    Name = "Agnostic",
                    InstallationPath = dirToAdd,
                    PlatformType = Platforms.NONE
                });

                RefreshGames();
            }
        }

        private void DeleteDir(object obj)
        {
            if (!String.IsNullOrEmpty(SelectedFolder.InstallationPath))
            {
                var x = Properties.Settings.Default.FolderPaths.Count.ToString();
                //remove from c# settings
                Helper.DeleteDir(SelectedFolder.InstallationPath);
                MessageBox.Show(Properties.Settings.Default.FolderPaths.Count.ToString());
                // remove from library directories 
                // because when vm gets instanciated the library from settings gets added to the oc
                Scanner.LibraryDirectories.Remove(SelectedFolder);

                RefreshGames();
            }
        }

        private void RefreshGames()
        {
            Games.Clear();
            foreach (var exe in Scanner.GetExecutables())
            {
                Games.Add(exe);
            }
            //APIController.GetGameCovers();
        }

        private void FilterGamesByPlatform(object obj)
        {
            if (obj as string == "ALL")
            {
                FilteredGames.Filter = game => game.Equals(game);
            }
            else
            {
                Enum.TryParse(obj as string, out Platforms selectedPlatform);
                FilteredGames.Filter = game => ((Game)game).Platform.Equals(selectedPlatform);
            }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void ResetAllSettings(object obj)
        {
            File.WriteAllText(@"game.json", string.Empty);
            File.WriteAllText(@"game.json", "[]");
            Scanner.LibraryDirectories.Clear();
            Properties.Settings.Default.FolderPaths.Clear();
            Properties.Settings.Default.Save();
            RefreshGames();
        }
        #endregion
    }
}
