using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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


namespace Practice1
{
    class Item
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public float Rating { get; set; }
        public float Price { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public List<MovieActor> MovieActors { get; set; }
        public Movie() => MovieActors = new List<MovieActor>();
    }
    class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Movie> Movies { get; set; }
        public Genre() => Movies = new List<Movie>();
    }
    class Actor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<MovieActor> MovieActors { get; set; }
        public Actor() => MovieActors = new List<MovieActor>();
    }
    class MovieActor
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int ActorId { get; set; }
        public Actor Actor { get; set; }
    }
    class MovieDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public MovieDbContext(bool init)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public MovieDbContext() { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .OnDelete(DeleteBehavior.Cascade);*/

            modelBuilder.Entity<MovieActor>()
                .HasKey(e => new { e.MovieId, e.ActorId });

            /*modelBuilder.Entity<Movie>()
                .HasOne(m => m.Genre)
                .WithMany(g => g.Movies)
                .OnDelete(DeleteBehavior.Restrict);*/

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=MovieDb;Trusted_connection=TRUE"
                    );
        }
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int pageSize = 5;
        int currentPage = 1;
        List<Movie> allMovies;
        List<Actor> allActors;
        int yearMin;
        int yearMax;
        float ratingMin;
        float ratingMax;
        float priceMin;
        float priceMax;

        public MainWindow()
        {
            InitializeComponent();

            btn_AddMovie.Click += Btn_AddMovie_Click;
            btn_AddActor.Click += Btn_AddActor_Click;
            cb_Movies_Descr.SelectionChanged += Cb_Movie_Descr_SelectionChanged;

            list_Actors.SelectionChanged += List_Actors_SelectionChanged;
            btn_Actors_Prev.Click += Btn_Actors_Prev_Click;
            btn_Actors_Next.Click += Btn_Actors_Next_Click;

            tb_SearchMovie.TextChanged += Tb_SearchMovie_TextChanged;
            tb_SearchActor.TextChanged += Tb_SearchActor_TextChanged;
            btn_DeleteMovie.Click += Btn_DeleteMovie_Click;
            btn_DeleteActor.Click += Btn_DeleteActor_Click;

            tb_SearchMovie_main.TextChanged += (s, e) => UpdateMoviesMain();
            cb_Genre_main.SelectionChanged += (s, e) => UpdateMoviesMain();
            tb_Year_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Year_to_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Rating_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Rating_to_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Price_from_main.TextChanged += (s, e) => UpdateMoviesMain();
            tb_Price_to_main.TextChanged += (s, e) => UpdateMoviesMain();

            btn_SummaryRefresh.Click += (s, e) => UpdateSummary();

            InsertDataIntoDb();

            UpdateGuiMovies();
            UpdateGuiActors();
            UpdateGuiGenresAndYears();
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            using (var db = new MovieDbContext())
            {

                var item1 = new Item { Name = "Movies count", Value = allMovies.Count.ToString() };
                var item2 = new Item { Name = "Actors count", Value = allActors.Count.ToString() };

                float item3Value = (float)allActors.Count / allMovies.Count;
                var item3 = new Item { Name = "Average actors in movie", Value = string.Format($"{item3Value:f2}") };

                Movie movie = allMovies.OrderBy(m => -m.Rating).FirstOrDefault();
                string item4Value = movie == null ? "none" :
                    string.Format($"{movie.Name}, rating = {movie.Rating}");
                var item4 = new Item { Name = "The most popular movie", Value = item4Value };

                Actor actor = db.Actors.Include(a => a.MovieActors)
                                .OrderBy(a => -a.MovieActors.Count).FirstOrDefault();
                string item5Value = actor == null ? "none" :
                    string.Format($"{actor.FirstName} {actor.LastName}, {actor.MovieActors.Count} movies");
                var item5 = new Item { Name = "The most popular actor", Value = item5Value };

                list_Summary.ItemsSource = new List<Item> { item1, item2, item3, item4, item5 };
            }
        }
        private void UpdateMoviesMain()
        {
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

            query = allMovies
                    .Where(movie => movie.Name.ToLower().Contains(NameToken) &&
                    movie.Year >= yearFrom && movie.Year <= yearTo &&
                    movie.Rating >= ratingFrom && movie.Rating <= ratingTo &&
                    movie.Price >= priceFrom && movie.Price <= priceTo);

            if (!genreName.Equals("any"))
                query = query.Where(movie => movie.Genre.Name.Equals(genreName));

            var movies = query.ToList();
            dg_Movies.ItemsSource = movies;
        }
        private void Btn_DeleteActor_Click(object sender, RoutedEventArgs e)
        {
            if (list_Actors_Delete.SelectedValue == null)
            {
                ShowInformation("Select actor!");
                return;
            }
            var actorInput = list_Actors_Delete.SelectedItem as Actor;
            using (var db = new MovieDbContext())
            {
                var actor = db.Actors.FirstOrDefault(a => a.FirstName.Equals(actorInput.FirstName) &&
                a.LastName.Equals(actorInput.LastName));
                if (actor == null)
                {
                    ShowInformation("There is no such actor. Error!");
                    return;
                }
                var result = MessageBox.Show($"You want to delete this actor:\n\"{actor.FirstName} {actor.LastName}\"\nAre you sure?",
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    db.Actors.Remove(actor);
                    db.SaveChanges();
                    UpdateGuiActors();
                    ShowInformation($"Actor \"{actor.FirstName} {actor.LastName}\"\nwas deleted!");
                }
            }
        }
        private void Btn_DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            if (list_Movies_Delete.SelectedValue == null)
            {
                ShowInformation("Select movie!");
                return;
            }

            var movieInput = list_Movies_Delete.SelectedValue as Movie;
            using (var db = new MovieDbContext())
            {
                var movie = db.Movies.FirstOrDefault(m => m.Name.Equals(movieInput.Name));
                if (movie == null)
                {
                    ShowInformation("There is no such movie. Error!");
                    return;
                }
                var result = MessageBox.Show($"You want to delete this movie:\n\"{movie.Name}\"\nAre you sure?",
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    db.Movies.Remove(movie);
                    db.SaveChanges();
                    UpdateGuiMovies();
                    ShowInformation($"Movie \"{movie.Name}\"\nwas deleted!");
                }
            }
        }
        private void Tb_SearchActor_TextChanged(object sender, TextChangedEventArgs e)
        {
            string token = tb_SearchActor.Text.Trim().ToLower();
            list_Actors_Delete.ItemsSource = allActors
                .Where(actor =>
            actor.FirstName.ToLower().Contains(token)
            || actor.LastName.ToLower().Contains(token)
            || (actor.FirstName + " " + actor.LastName).ToLower().Contains(token)
            || (actor.LastName + " " + actor.FirstName).ToLower().Contains(token))
                .ToList();

        }
        private void Tb_SearchMovie_TextChanged(object sender, TextChangedEventArgs e)
        {
            string token = tb_SearchMovie.Text.Trim().ToLower();
            list_Movies_Delete.ItemsSource = allMovies
                .Where(movie => movie.Name.ToLower().Contains(token))
                .ToList();
        }
        private void Btn_Actors_Next_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new MovieDbContext())
            {
                int actorsCount = db.Actors.Count();
                int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
                if (currentPage + 1 > pagesCount)
                    return;

                list_Actors.ItemsSource = db.Actors.OrderBy(a => a.LastName)
                    .Skip(currentPage * pageSize)
                    .Take(pageSize)
                    .ToList();
                currentPage++;
                lab_CurrentPage.Content = currentPage + "/" + pagesCount;
            }
        }
        private void Btn_Actors_Prev_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new MovieDbContext())
            {
                int actorsCount = db.Actors.Count();
                int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
                if (currentPage == 1)
                    return;
                currentPage--;
                lab_CurrentPage.Content = currentPage + "/" + pagesCount;

                list_Actors.ItemsSource = db.Actors.OrderBy(a => a.LastName)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }
        private void List_Actors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var actorInput = list_Actors.SelectedItem as Actor;
            if (actorInput == null)
                return;
            string filename;
            using (var db = new MovieDbContext())
            {
                var actor = db.Actors.Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                    .Where(a => a.FirstName == actorInput.FirstName && a.LastName == actorInput.LastName)
                    .First();

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
                filename = actor.FirstName + " " + actor.LastName + ".jpg";
            }

            string programPath = Environment.CurrentDirectory;
            Uri baseUri = new Uri(programPath);
            BitmapImage bitmapImage = null;
            try
            {
                try
                {
                    Uri uri = new Uri(baseUri, "../../images/actors/" + filename);
                    bitmapImage = new BitmapImage(uri);
                }
                catch
                {
                    Uri uri = new Uri(baseUri, "../../images/actors/" + "no_avatar.png");
                    bitmapImage = new BitmapImage(uri);
                }
                finally
                {
                    img_Actor.Source = bitmapImage;
                }
            }
            catch { }
        }
        private void Btn_AddActor_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Movies_AddActor.SelectedItem == null)
            {
                ShowInformation("Select movie!");
                return;
            }
            string movieName = cb_Movies_AddActor.SelectedItem as string;

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

            using (var db = new MovieDbContext())
            {
                Movie movie = db.Movies.Where(m => m.Name.Equals(movieName)).First();
                Actor actor = new Actor { FirstName = firstName, LastName = lastName };
                movie.MovieActors.Add(new MovieActor { Movie = movie, Actor = actor });
                db.SaveChanges();
            }
            UpdateGuiActors();
            ShowInformation("Actor was added!");
        }
        private void UpdateGuiMovies()
        {
            using (var db = new MovieDbContext())
            {
                var movies = db.Movies
                    .Include(m => m.Genre)
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor)
                    .OrderBy(m => m.Name);

                allMovies = movies.ToList();
                dg_Movies.ItemsSource = allMovies;
                dg_Movies.IsReadOnly = true;
                list_Movies_Delete.ItemsSource = allMovies;

                var moviesNames = movies.Select(m => m.Name).ToList();
                cb_Movies_Descr.ItemsSource = moviesNames;
                cb_Movies_AddActor.ItemsSource = moviesNames;

                yearMin = movies.Min(m => m.Year);
                yearMax = movies.Max(m => m.Year);
                ratingMin = movies.Min(m => m.Rating);
                ratingMax = movies.Max(m => m.Rating);
                priceMin = movies.Min(m => m.Price);
                priceMax = movies.Max(m => m.Price);
            }
        }
        private void UpdateGuiActors()
        {
            using (var db = new MovieDbContext())
            {
                var actors = db.Actors.OrderBy(a => a.LastName);

                list_Actors.ItemsSource = actors
                    .Take(pageSize)
                    .ToList();
                int actorsCount = actors.Count();
                int pagesCount = (int)Math.Ceiling((double)actorsCount / pageSize);
                currentPage = 1;
                lab_CurrentPage.Content = $"{currentPage}/{pagesCount}";

                allActors = actors.ToList();
                list_Actors_Delete.ItemsSource = allActors;
            }
        }
        private void UpdateGuiGenresAndYears()
        {
            using (var db = new MovieDbContext())
            {
                var genres = db.Genres.OrderBy(g => g.Name);

                var genresNames = genres.Select(g => g.Name).ToList();
                cb_Genre.ItemsSource = genresNames;
                var genresNamesMain = new List<string>();
                genresNamesMain.Add("any");
                genresNamesMain.AddRange(genresNames);
                genresNamesMain.Sort();
                cb_Genre_main.ItemsSource = genresNamesMain;

                cb_Year.ItemsSource = Enumerable.Range(1800, 2025 - 1800).OrderBy(n => -n).ToList();
            }
        }
        private void InsertDataIntoDb()
        {
            using (var db = new MovieDbContext(true))
            {
                // Genres
                Genre g1 = new Genre { Name = "drama" };
                Genre g2 = new Genre { Name = "fantasy" };
                Genre g3 = new Genre { Name = "crime" };
                Genre g4 = new Genre { Name = "comedy" };
                Genre g5 = new Genre { Name = "thriller" };
                db.Genres.AddRange(g1, g2, g3, g4, g5);
                db.SaveChanges();

                // Movies
                Movie m1 = new Movie { Name = "The Shawshank Redemption", Genre = g1, Year = 1994, Rating = 9.1f, Price = 300 };
                Movie m2 = new Movie { Name = "The Matrix", Genre = g2, Year = 1999, Rating = 8.5f, Price = 250 };
                Movie m3 = new Movie { Name = "Inception", Genre = g2, Year = 2010, Rating = 8.66f, Price = 220 };
                Movie m4 = new Movie { Name = "The Thirteenth Floor", Genre = g2, Year = 1999, Rating = 7.575f, Price = 180 };
                Movie m5 = new Movie { Name = "Green Book", Genre = g1, Year = 2018, Rating = 8.348f, Price = 320 };
                Movie m6 = new Movie { Name = "Training Day", Genre = g3, Year = 2001, Rating = 7.831f, Price = 260 };
                Movie m7 = new Movie { Name = "Scarface", Genre = g3, Year = 1983, Rating = 8.181f, Price = 400 };
                Movie m8 = new Movie { Name = "The Mask", Genre = g4, Year = 1994, Rating = 7.974f, Price = 330 };
                Movie m9 = new Movie { Name = "The Departed", Genre = g3, Year = 2006, Rating = 8.455f, Price = 290 };
                Movie m10 = new Movie { Name = "The Recruit", Genre = g5, Year = 2003, Rating = 7.364f, Price = 350 };
                db.Movies.AddRange(m1, m2, m3, m4, m5, m6, m7, m8, m9, m10);
                db.SaveChanges();

                // Actors
                Actor a1 = new Actor { FirstName = "Tim", LastName = "Robbins" };
                Actor a2 = new Actor { FirstName = "Morgan", LastName = "Freeman" };
                Actor a3 = new Actor { FirstName = "Keanu", LastName = "Reeves" };
                Actor a4 = new Actor { FirstName = "Laurence", LastName = "Fishburne" };
                Actor a5 = new Actor { FirstName = "Carrie-Anne", LastName = "Moss" };
                Actor a6 = new Actor { FirstName = "Hugo", LastName = "Weaving" };
                Actor a7 = new Actor { FirstName = "Leonardo", LastName = "DiCaprio" };
                Actor a8 = new Actor { FirstName = "Joseph", LastName = "Gordon-Levitt" };
                Actor a9 = new Actor { FirstName = "Ellen", LastName = "Page" };
                Actor a10 = new Actor { FirstName = "Tom", LastName = "Hardy" };
                Actor a11 = new Actor { FirstName = "Craig", LastName = "Bierko" };
                Actor a12 = new Actor { FirstName = "Gretchen", LastName = "Mol" };
                Actor a13 = new Actor { FirstName = "Armin", LastName = "Mueller-Stahl" };
                Actor a14 = new Actor { FirstName = "Viggo", LastName = "Mortensen" };
                Actor a15 = new Actor { FirstName = "Denzel", LastName = "Washington" };
                Actor a16 = new Actor { FirstName = "Ethan", LastName = "Hawke" };
                Actor a17 = new Actor { FirstName = "Al", LastName = "Pacino" };
                Actor a18 = new Actor { FirstName = "Steven", LastName = "Bauer" };
                Actor a19 = new Actor { FirstName = "Jim", LastName = "Carrey" };
                Actor a20 = new Actor { FirstName = "Cameron", LastName = "Diaz" };
                Actor a21 = new Actor { FirstName = "Matt", LastName = "Damon" };
                Actor a22 = new Actor { FirstName = "Jack", LastName = "Nicholson" };
                Actor a23 = new Actor { FirstName = "Mark", LastName = "Wahlberg" };
                Actor a24 = new Actor { FirstName = "Colin", LastName = "Farrell" };
                db.Actors.AddRange(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12,
                    a13, a14, a15, a16, a17, a18, a19, a20, a21, a22, a23, a24);
                db.SaveChanges();

                // MovieActor
                m1.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m1, Actor = a1 },
                    new MovieActor { Movie = m1, Actor = a2 }});
                m2.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m2, Actor = a3 },
                    new MovieActor { Movie = m2, Actor = a4 },
                    new MovieActor { Movie = m2, Actor = a5 },
                    new MovieActor { Movie = m2, Actor = a6 }});
                m3.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m3, Actor = a7 },
                    new MovieActor { Movie = m3, Actor = a8 },
                    new MovieActor { Movie = m3, Actor = a9 },
                    new MovieActor { Movie = m3, Actor = a10 }});
                m4.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m4, Actor = a11 },
                    new MovieActor { Movie = m4, Actor = a12 },
                    new MovieActor { Movie = m4, Actor = a13 }});
                m5.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m5, Actor = a14 }});
                m6.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m6, Actor = a15 },
                    new MovieActor { Movie = m6, Actor = a16 }});
                m7.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m7, Actor = a17 },
                    new MovieActor { Movie = m7, Actor = a18 }});
                m8.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m8, Actor = a19 },
                    new MovieActor { Movie = m8, Actor = a20 }});
                m9.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m9, Actor = a7 },
                    new MovieActor { Movie = m9, Actor = a21 },
                    new MovieActor { Movie = m9, Actor = a22 },
                    new MovieActor { Movie = m9, Actor = a23 }});
                m10.MovieActors.AddRange(new List<MovieActor> {
                    new MovieActor { Movie = m10, Actor = a17 },
                    new MovieActor { Movie = m10, Actor = a24 }});
                db.SaveChanges();
            }
        }
        private void Btn_AddMovie_Click(object sender, RoutedEventArgs e)
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
            string genreName = cb_Genre.SelectedItem as string;

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

            using (var db = new MovieDbContext())
            {
                Genre genre = db.Genres.Where(g => g.Name.Equals(genreName)).First();
                Movie m = new Movie { Name = movieName, Genre = genre, Year = year, Rating = rating, Price = price };
                db.Movies.Add(m);
                db.SaveChanges();
            }
            UpdateGuiMovies();
            ShowInformation("Movie was added!");
        }
        private void Cb_Movie_Descr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var movieName = cb_Movies_Descr.SelectedItem as string;
            string filename;
            using (var db = new MovieDbContext())
            {
                var movie = db.Movies
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor)
                    .Include(m => m.Genre)
                    .FirstOrDefault(m => m.Name.Equals(movieName));
                if (movie == null)
                    return;

                var sb = new StringBuilder();
                sb.AppendLine($"Name : {movie.Name}");
                sb.AppendLine($"Name : {movie.Year}");
                sb.AppendLine($"Name : {movie.Genre.Name}");
                sb.AppendLine($"Rating : {movie.Rating}");
                sb.AppendLine($"Price : {movie.Price}");
                sb.AppendLine();
                sb.AppendLine("Actors:");
                foreach (var ma in movie.MovieActors)
                {
                    sb.AppendLine($"{ma.Actor.FirstName} {ma.Actor.LastName}");
                }
                text_Movie_Descr.Text = sb.ToString();
                filename = movie.Name + ".jpg";
            }

            string programPath = Environment.CurrentDirectory;
            Uri baseUri = new Uri(programPath);
            BitmapImage bitmapImage = null;
            try
            {
                try
                {
                    Uri uri = new Uri(baseUri, "../../images/movies/" + filename);
                    bitmapImage = new BitmapImage(uri);
                }
                catch
                {
                    Uri uri = new Uri(baseUri, "../../images/movies/" + "no_image.png");
                    bitmapImage = new BitmapImage(uri);
                }
                finally
                {
                    img_Movie.Source = bitmapImage;
                    img_Movie.HorizontalAlignment = HorizontalAlignment.Center;
                    img_Movie.VerticalAlignment = VerticalAlignment.Center;
                }
            }
            catch { }
        }
        private void ShowInformation(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
