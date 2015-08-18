﻿using System;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Helpers;
using Popcorn.Messaging;

namespace Popcorn.ViewModels.Tabs
{
    /// <summary>
    /// The popular movies tab
    /// </summary>
    public sealed class PopularTabViewModel : TabsViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the PopularTabViewModel class.
        /// </summary>
        private PopularTabViewModel()
        {
            RegisterMessages();
            RegisterCommands();
            TabName = LocalizationProviderHelper.GetLocalizedValue<string>("PopularTitleTab");
        }

        #endregion

        #region Methods

        #region Method -> InitializeAsync
        /// <summary>
        /// Load asynchronously the popular movies for the current instance
        /// </summary>
        /// <returns>Instance of PopularTabViewModel</returns>
        private async Task<PopularTabViewModel> InitializeAsync()
        {
            await LoadNextPageAsync();
            return this;
        }
        #endregion

        #region Method -> CreateAsync
        /// <summary>
        /// Initialize asynchronously an instance of the GreatestTabViewModel class
        /// </summary>
        /// <returns>Instance of GreatestTabViewModel</returns>
        public static Task<PopularTabViewModel> CreateAsync()
        {
            var ret = new PopularTabViewModel();
            return ret.InitializeAsync();
        }
        #endregion

        #region Method -> RegisterMessages

        /// <summary>
        /// Register messages
        /// </summary>
        private void RegisterMessages()
        {
            Messenger.Default.Register<ChangeLanguageMessage>(
                this,
                language => { TabName = LocalizationProviderHelper.GetLocalizedValue<string>("PopularTitleTab"); });
        }

        #endregion

        #region Method -> RegisterCommands

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            ReloadMovies = new RelayCommand(async () =>
            {
                await LoadNextPageAsync();
            });
        }

        #endregion

        #region Method -> LoadNextPageAsync

        /// <summary>
        /// Load next page
        /// </summary>
        public async Task LoadNextPageAsync()
        {
            Page++;
            IsLoadingMovies = true;
            try
            {
                var movieResults =
                    await MovieService.GetPopularMoviesAsync(Page,
                        MaxMoviesPerPage,
                        CancellationLoadNextPageToken.Token);
                var movies = movieResults.Item1.ToList();
                MaxNumberOfMovies = movieResults.Item2;

                foreach (var movie in movies)
                {
                    Movies.Add(movie);
                }

                await UserService.ComputeMovieHistoryAsync(movies);
                await MovieService.DownloadCoverImageAsync(movies);
            }
            catch (Exception)
            {
                Page--;
            }
            finally
            {
                IsLoadingMovies = false;
                IsMovieFound = Movies.Any();
                CurrentNumberOfMovies = Movies.Count();
            }
        }

        #endregion

        #endregion
    }
}