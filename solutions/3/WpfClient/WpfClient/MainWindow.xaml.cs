using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfClient.Clients;
using WpfClient.Entities;

namespace WpfClient
{
    class Item
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientActor clientActor = new ClientActor();
        ClientMovie clientMovie = new ClientMovie();
        ClientGenre clientGenre = new ClientGenre();
        ClientMovieActor clientMovieActor = new ClientMovieActor();

        int pageSize = 5;
        int currentPage = 1;

        List<Movie> allMoviesTemp;
        List<Actor> allActorsTemp;

        int yearMin;
        int yearMax;
        float ratingMin;
        float ratingMax;
        float priceMin;
        float priceMax;

        public MainWindow()
        {
            InitializeComponent();

            // UPDATE TABS
            tc_Movies.SelectionChanged += Tc_Movies_SelectionChanged;
            
            // ALL MOVIES
            tb_SearchMovie_main.TextChanged += (s, e) => UpdateMoviesMain();
            cb_Genre_main.SelectionChanged += (s, e) => UpdateMoviesMain();
            tb_Year_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Year_to_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Rating_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Rating_to_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Price_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Price_to_main.TextChanged += (s, e) => UpdateMoviesMain();

            // ACTOR
            list_Actors.SelectionChanged += List_Actors_SelectionChangedAsync;
            btn_Actors_Prev.Click += Btn_Actors_Prev_Click;
            btn_Actors_Next.Click += Btn_Actors_Next_Click;

            // MOVIE
            cb_Movies_Descr.SelectionChanged += Cb_Movie_Descr_SelectionChangedAsync;

            // ADD
            btn_AddMovie.Click += Btn_AddMovie_ClickAsync;
            btn_AddActor.Click += Btn_AddActor_ClickAsync;

            // DELETE
            tb_SearchMovie.TextChanged += Tb_SearchMovie_TextChanged;
            tb_SearchActor.TextChanged += Tb_SearchActor_TextChanged;
            btn_DeleteMovie.Click += Btn_DeleteMovie_ClickAsync;
            btn_DeleteActor.Click += Btn_DeleteActor_ClickAsync;
        }

        private void Tc_Movies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                int tabIndex = tc_Movies.SelectedIndex;
                switch (tabIndex)
                {
                    case 0: // all movies
                        UpdateAllMoviesAsync();
                        break;
                    case 1: // movie
                        UpdateMovieAsync();
                        break;
                    case 2: // actor
                        UpdateActorAsync();
                        break;
                    case 3: // add
                        UpdateAddAsync();
                        break;
                    case 4: // delete
                        UpdateDeleteAsync();
                        break;
                    case 5: // summary
                        UpdateSummaryAsync();
                        break;
                    default:
                        break;
                }
            }
        }


        private async void Btn_DeleteActor_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (list_Actors_Delete.SelectedValue == null)
            {
                ShowInformation("Select actor!");
                return;
            }
            var actorInput = list_Actors_Delete.SelectedItem as Actor;

            Actor actor = await clientActor.GetActorAsync(actorInput.Id);
            if (actor == null)
            {
                ShowInformation("There is no such actor. Error!");
                return;
            }
            var result = MessageBox.Show($"You want to delete this actor:\n\"{actor.FirstName} {actor.LastName}\"\nAre you sure?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Actor delActor = await clientActor.DeleteActorAsync(actor.Id);
                UpdateDeleteAsync();
                ShowInformation($"Actor \"{delActor.FirstName} {delActor.LastName}\"\nwas deleted!");
            }

        }
        private async void Btn_DeleteMovie_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (list_Movies_Delete.SelectedValue == null)
            {
                ShowInformation("Select movie!");
                return;
            }

            var movieInput = list_Movies_Delete.SelectedValue as Movie;

            Movie movie = await clientMovie.GetMovieAsync(movieInput.Id);
            if (movie == null)
            {
                ShowInformation("There is no such movie. Error!");
                return;
            }
            var result = MessageBox.Show($"You want to delete this movie:\n\"{movie.Name}\"\nAre you sure?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Movie delMovie = await clientMovie.DeleteMovieAsync(movie.Id);
                UpdateDeleteAsync();
                ShowInformation($"Movie \"{delMovie.Name}\"\nwas deleted!");
            }

        }
        private void Tb_SearchActor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allActorsTemp == null)
                return;

            string token = tb_SearchActor.Text.Trim().ToLower();
            list_Actors_Delete.ItemsSource = allActorsTemp
                .Where(actor =>
            actor.FirstName.ToLower().Contains(token)
            || actor.LastName.ToLower().Contains(token)
            || (actor.FirstName + " " + actor.LastName).ToLower().Contains(token)
            || (actor.LastName + " " + actor.FirstName).ToLower().Contains(token))
                .ToList();

        }
        private void Tb_SearchMovie_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allMoviesTemp == null)
                return;

            string token = tb_SearchMovie.Text.Trim().ToLower();
            list_Movies_Delete.ItemsSource = allMoviesTemp
                .Where(movie => movie.Name.ToLower().Contains(token))
                .ToList();
        }


        private async void Btn_AddMovie_ClickAsync(object sender, RoutedEventArgs e)
        {
            string movieName = tb_MovieName.Text.Trim();
            if (movieName.Equals(""))
            {
                ShowInformation("Input movie name!");
                return;
            }

            if (cb_Year.SelectedItem == null)
            {
                ShowInformation("Select year!");
                return;
            }
            int year = (int)cb_Year.SelectedItem;

            if (cb_Genre.SelectedItem == null)
            {
                ShowInformation("Select genre!");
                return;
            }
            Genre genre = cb_Genre.SelectedItem as Genre;

            float rating = 0;
            bool resRating = float.TryParse(tb_Rating.Text, out rating);
            if (!resRating || rating < 0 || rating > 10)
            {
                ShowInformation("Rating must be in range 0,000..9,999!");
                return;
            }

            float price = 0;
            bool resPrice = float.TryParse(tb_Price.Text, out price);
            if (!resPrice || price < 0)
            {
                ShowInformation("Price must be more than 0!");
                return;
            }

            Movie m = new Movie { Name = movieName, GenreId = genre.Id, Year = year, Rating = rating, Price = price };
            Movie newMovie = await clientMovie.CreateMovieAsync(m);
            if (newMovie == null)
            {
                ShowInformation("This movie name already in use!");
                return;
            }

            UpdateAddAsync();
            tb_MovieName.Text = "";
            cb_Year.SelectedItem = null;
            cb_Genre.SelectedItem = null;
            tb_Rating.Text = "";
            tb_Price.Text = "";
            ShowInformation("Movie was added!");
        }
        private async void Btn_AddActor_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (cb_Movies_AddActor.SelectedItem == null)
            {
                ShowInformation("Select movie!");
                return;
            }
            Movie movie = cb_Movies_AddActor.SelectedItem as Movie;

            string firstName = tb_FirstName.Text.Trim();
            if (firstName.Equals(""))
            {
                ShowInformation("Input first name!");
                return;
            }

            string lastName = tb_LastName.Text.Trim();
            if (lastName.Equals(""))
            {
                ShowInformation("Input last name!");
                return;
            }

            Actor actor = new Actor { FirstName = firstName, LastName = lastName };
            Actor newActor = await clientActor.CreateActorAsync(actor);
            if (newActor == null)
            {
                ShowInformation("This actor name already in use!");
                return;
            }

            MovieActor ma = new MovieActor { MovieId = movie.Id, ActorId = newActor.Id };
            MovieActor newMovieActor = await clientMovieActor.CreateMovieActorAsync(ma);
            if (newMovieActor == null)
            {
                ShowInformation("This actor already connected to this movie!");
                return;
            }

            cb_Movies_AddActor.SelectedItem = null;
            tb_FirstName.Text = "";
            tb_LastName.Text = "";
            ShowInformation("Actor was added!");
        }
        private async void Cb_Movie_Descr_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var movieInput = cb_Movies_Descr.SelectedItem as Movie;
            if (movieInput == null)
                return;

            var movie = await clientMovie.GetMovieAsync(movieInput.Id);

            var sb = new StringBuilder();
            sb.AppendLine($"Name : {movie.Name}");
            sb.AppendLine($"Year : {movie.Year}");
            sb.AppendLine($"Genre : {movie.Genre.Name}");
            sb.AppendLine($"Rating : {movie.Rating}");
            sb.AppendLine($"Price : {movie.Price}");
            sb.AppendLine();
            sb.AppendLine("Actors:");
            foreach (var ma in movie.MovieActors)
            {
                sb.AppendLine($"{ma.Actor.FirstName} {ma.Actor.LastName}");
            }
            text_Movie_Descr.Text = sb.ToString();

            // download photo
            string path = await clientMovie.GetMoviePhotoAsync(movieInput.Id);
            if (path != null)
            {
                Uri uri = new Uri(path);
                BitmapImage bitmapImage = new BitmapImage(uri);
                img_Movie.Source = bitmapImage;
                img_Movie.HorizontalAlignment = HorizontalAlignment.Center;
                img_Movie.VerticalAlignment = VerticalAlignment.Center;
            }
        }
        private async void List_Actors_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            var actorInput = list_Actors.SelectedItem as Actor;
            if (actorInput == null)
                return;

            Actor actor = await clientActor.GetActorAsync(actorInput.Id);

            var sb = new StringBuilder();
            sb.AppendLine($"{actor.FirstName} {actor.LastName}");
            sb.AppendLine();
            sb.AppendLine("Movies:");
            var movieActors = actor.MovieActors.OrderBy(ma => -ma.Movie.Year);
            foreach (var ma in movieActors)
            {
                sb.AppendLine($"{ma.Movie.Year} {ma.Movie.Name}");
            }
            text_Actor_Descr.Text = sb.ToString();

            // download photo
            string path = await clientActor.GetActorPhotoAsync(actorInput.Id);
            if (path != null)
            {
                Uri uri = new Uri(path);
                BitmapImage bitmapImage = new BitmapImage(uri);
                img_Actor.Source = bitmapImage;
            }
        }
        private void Btn_Actors_Next_Click(object sender, RoutedEventArgs e)
        {
            if (allActorsTemp == null)
                return;

            int actorsCount = allActorsTemp.Count;
            int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
            if (currentPage + 1 > pagesCount)
                return;

            list_Actors.ItemsSource = allActorsTemp
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();
            currentPage++;
            lab_CurrentPage.Content = currentPage + "/" + pagesCount;

        }
        private void Btn_Actors_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (allActorsTemp == null)
                return;

            int actorsCount = allActorsTemp.Count;
            int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
            if (currentPage == 1)
                return;
            currentPage--;
            lab_CurrentPage.Content = currentPage + "/" + pagesCount;

            list_Actors.ItemsSource = allActorsTemp
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

        }
        private void UpdateMoviesMain()
        {
            if (allMoviesTemp == null)
                return;

            string NameToken = tb_SearchMovie_main.Text.Trim().ToLower();
            string genreName = cb_Genre_main.SelectedItem as string;
            genreName = genreName ?? "any";

            int yearFrom, yearTo;
            float ratingFrom, ratingTo, priceFrom, priceTo;

            if (int.TryParse(tb_Year_from_main.Text.Trim(), out yearFrom) == false || yearFrom == 0)
                yearFrom = yearMin;
            if (int.TryParse(tb_Year_to_main.Text.Trim(), out yearTo) == false || yearTo == 0)
                yearTo = yearMax;
            if (float.TryParse(tb_Rating_from_main.Text.Trim(), out ratingFrom) == false || ratingFrom == 0)
                ratingFrom = ratingMin;
            if (float.TryParse(tb_Rating_to_main.Text.Trim(), out ratingTo) == false || ratingTo == 0)
                ratingTo = ratingMax;
            if (float.TryParse(tb_Price_from_main.Text.Trim(), out priceFrom) == false || priceFrom == 0)
                priceFrom = priceMin;
            if (float.TryParse(tb_Price_to_main.Text.Trim(), out priceTo) == false || priceTo == 0)
                priceTo = priceMax;

            IEnumerable<Movie> query = null;

            query = allMoviesTemp
                    .Where(movie => movie.Name.ToLower().Contains(NameToken) &&
                    movie.Year >= yearFrom && movie.Year <= yearTo &&
                    movie.Rating >= ratingFrom && movie.Rating <= ratingTo &&
                    movie.Price >= priceFrom && movie.Price <= priceTo);

            if (!genreName.Equals("any"))
                query = query.Where(movie => movie.Genre.Name.Equals(genreName));

            var movies = query.ToList();
            dg_Movies.ItemsSource = movies;
        }


        private async void UpdateAllMoviesAsync()
        {
            List<Movie> movies = await clientMovie.GetAllMoviesAsync();
            movies = movies.OrderBy(m => m.Name).ToList();
            allMoviesTemp = movies;
            dg_Movies.ItemsSource = movies;
            dg_Movies.IsReadOnly = true;

            yearMin = movies.Min(m => m.Year);
            yearMax = movies.Max(m => m.Year);
            ratingMin = movies.Min(m => m.Rating);
            ratingMax = movies.Max(m => m.Rating);
            priceMin = movies.Min(m => m.Price);
            priceMax = movies.Max(m => m.Price);

            List<Genre> genres = await clientGenre.GetAllGenresAsync();
            var genresNames = genres.Select(g => g.Name).ToList();

            var genresNamesMain = new List<string>();
            genresNamesMain.Add("any");
            genresNamesMain.AddRange(genresNames);
            genresNamesMain.Sort();
            cb_Genre_main.ItemsSource = genresNamesMain;

        }
        private async void UpdateMovieAsync()
        {
            List<Movie> movies = await clientMovie.GetAllMoviesAsync();
            cb_Movies_Descr.ItemsSource = movies;
        }
        private async void UpdateActorAsync()
        {
            List<Actor> actors = await clientActor.GetAllActorsAsync();
            actors = actors.OrderBy(a => a.LastName).ToList();

            list_Actors.ItemsSource = actors
                .Take(pageSize)
                .ToList();
            int actorsCount = actors.Count();
            int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
            currentPage = 1;
            lab_CurrentPage.Content = $"{currentPage}/{pagesCount}";

            allActorsTemp = actors;
        }
        private async void UpdateAddAsync()
        {
            List<Movie> movies = await clientMovie.GetAllMoviesAsync();
            cb_Movies_AddActor.ItemsSource = movies;

            List<Genre> genres = await clientGenre.GetAllGenresAsync();
            cb_Genre.ItemsSource = genres;

            cb_Year.ItemsSource = Enumerable.Range(1800, 2025 - 1800).OrderBy(n => -n).ToList();
        }
        private async void UpdateDeleteAsync()
        {
            List<Movie> movies = await clientMovie.GetAllMoviesAsync();
            list_Movies_Delete.ItemsSource = movies;
            allMoviesTemp = movies;

            List<Actor> actors = await clientActor.GetAllActorsAsync();
            list_Actors_Delete.ItemsSource = actors;
            allActorsTemp = actors;
        }
        private async void UpdateSummaryAsync()
        {
            var allMovies = await clientMovie.GetAllMoviesAsync();
            var allActors = await clientActor.GetAllActorsAsync();


            var item1 = new Item { Name = "Movies count", Value = allMovies.Count.ToString() };
            var item2 = new Item { Name = "Actors count", Value = allActors.Count.ToString() };

            float item3Value = (float)allActors.Count / allMovies.Count;
            var item3 = new Item { Name = "Average actors in movie", Value = string.Format($"{item3Value:f2}") };

            Movie movie = allMovies.OrderBy(m => -m.Rating).FirstOrDefault();
            string item4Value = movie == null ? "none" :
                string.Format($"{movie.Name}, rating = {movie.Rating}");
            var item4 = new Item { Name = "The most popular movie", Value = item4Value };

            Actor topActor = await clientActor.GetTopActorAsync();
            string item5Value = topActor == null ? "none" :
                string.Format($"{topActor.FirstName} {topActor.LastName}, {topActor.MovieActors.Count} movies");
            var item5 = new Item { Name = "The most popular actor", Value = item5Value };

            list_Summary.ItemsSource = new List<Item> { item1, item2, item3, item4, item5 };

        }
        private void ShowInformation(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }




        // ASYNC TRAIN TESTS
        private async void MainFuncAsync()
        {
            Trace.WriteLine("-- Start MainFuncAsync");
            Trace.WriteLine("-- Sleep 12000..");
            Trace.WriteLine("-- Thread : " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(12000);
            Trace.WriteLine("-- call ChildFunc"); // тут отвисает
            int x = await ChildFunc();
            Trace.WriteLine("-- Sleep 12000.."); // тут опять зависает
            Trace.WriteLine("-- Thread : " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(12000);
            Trace.WriteLine("-- Finish MainFuncAsync, x = " + x);
        }
        private async Task<int> ChildFunc()
        {
            Trace.WriteLine("----- Start ChildFunc");
            Trace.WriteLine("----- Sleep 12000..");
            Trace.WriteLine("----- Thread : " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(12000);
            int x = await GrandChildFunc();
            Trace.WriteLine("----- Sleep 12000..");
            Trace.WriteLine("----- Thread : " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(12000);
            Trace.WriteLine("----- Finish ChildFunc");
            return x + 5;
        }
        private Task<int> GrandChildFunc()
        {
            return Task.Run(() =>
            {
                Trace.WriteLine("--------- Start GrandChildFunc");
                Trace.WriteLine("--------- Sleep 12000..");
                Trace.WriteLine("--------- Thread : " + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(12000);
                Trace.WriteLine("--------- Finish GrandChildFunc");
                return 5;
            });
        }
    }
}
