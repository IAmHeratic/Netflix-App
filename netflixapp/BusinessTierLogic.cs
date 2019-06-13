/*
 * Jose E. Rodriguez
 * CS 341 SPring 2018
 * Project 8 - Netflix Database App - Part 2
 * University of Illinois at Chicago
 */

//
// BusinessTier:  business logic, acting as interface between UI and data store.
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

  //
  // Business:
  //
  public class Business
  {
    //
    // Fields:
    //
    private string _DBFile;
    private DataAccessTier.Data dataTier;


    //
    // Constructor:
    //
    public Business(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;

      dataTier = new DataAccessTier.Data(DatabaseFilename);
    }


    //
    // TestConnection:
    //
    // Returns true if we can establish a connection to the database, false if not.
    //
    public bool TestConnection()
    {
      return dataTier.TestConnection();
    }


    //
    // GetNamedUser:
    //
    // Retrieves User object based on USER NAME; returns null if user is not
    // found.
    //
    // NOTE: there are "named" users from the Users table, and anonymous users
    // that only exist in the Reviews table.  This function only looks up "named"
    // users from the Users table.
    //
    public User GetNamedUser(string UserName)
    {
            // Handle special character '
            UserName = UserName.Replace("'", "''");

            // SQL string
            string SQL = string.Format(@"
            SELECT UserID, Occupation
            FROM Users
            WHERE UserName = '" + UserName + @"'");

            // Execute SQL
            DataSet ds = dataTier.ExecuteNonScalarQuery(SQL);
            DataTable dt = ds.Tables["TABLE"];

            // Create user object
            UserName = UserName.Replace("''", "'");
            int UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
            string Occupation = Convert.ToString(dt.Rows[0]["Occupation"]);
            User u = new User(UserID, UserName, Occupation);

            // return User
            return u;
    }



    public IReadOnlyList<Movie> GetAllMovies()
    {
            // SQL string
            string SQL = (@"
            SELECT MovieName, MovieID
            FROM Movies
            ORDER BY MovieName");

            // Execute SQL
            DataSet ds = dataTier.ExecuteNonScalarQuery(SQL);

            // Create list of movies
            Movie m;
            List<Movie> movies = new List<Movie>();

            foreach (DataRow row in ds.Tables["TABLE"].Rows)
            {
                m = new Movie(Convert.ToInt32(row["MovieID"]), Convert.ToString(row["MovieName"]));
                movies.Add(m);
            }

            // Return list of movies
            return movies;
    }


    //
    // GetAllNamedUsers:
    //
    // Returns a list of all the users in the Users table ("named" users), sorted 
    // by user name.
    //
    // NOTE: the database also contains lots of "anonymous" users, which this 
    // function does not return.
    //
    public IReadOnlyList<User> GetAllNamedUsers()
    {
            // SQL string
            string SQL = @"
            SELECT *
            FROM Users
            ORDER BY UserName ASC";

            // Execute SQL
            DataSet ds = dataTier.ExecuteNonScalarQuery(SQL);

            // Insert all users into list
            string username, occupation;
            int user_ID;
            User u;
            List<User> users = new List<User>();

            foreach(DataRow row in ds.Tables["TABLE"].Rows)
            {
                user_ID = Convert.ToInt32(row["UserID"]);
                username = Convert.ToString(row["UserName"]);
                occupation = Convert.ToString(row["Occupation"]);
                u = new User(user_ID, username, occupation);
                users.Add(u);
            }

            // Return list
            return users;
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE ID; returns null if movie is not
    // found.
    //
    public Movie GetMovie(int MovieID)
    {
            // SQL string
            string SQL = string.Format(@"
            SELECT MovieName
            FROM Movies
            WHERE MovieID = " + MovieID);

            // Execute string
            object MovieName = dataTier.ExecuteScalarQuery(SQL);

            // Was object found
            if (MovieName == null)
                return null;

            // Movie object
            Movie M = new Movie(MovieID, MovieName.ToString());
            return M;
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE NAME; returns null if movie is not
    // found.
    //
    public Movie GetMovie(string MovieName)
    {
            // Handle single quotes in movie names
            MovieName = MovieName.Replace("'", "''");

            // SQL string
            string SQL = string.Format(@"
            SELECT MovieID
            FROM Movies
            WHERE MovieName = '" + MovieName + "'");

            // Execute SQL
            object MovieID = dataTier.ExecuteScalarQuery(SQL);

            // Was object found
            if (MovieID == null)
                return null;

            // Movie object
            MovieName = MovieName.Replace("''", "'");
            Movie M = new Movie((int)MovieID, MovieName);
            return M;
    }


    //
    // AddReview:
    //
    // Adds review based on MOVIE ID, returning a Review object containing
    // the review, review's id, etc.  If the add failed, null is returned.
    //
    public Review AddReview(int MovieID, int UserID, int Rating)
    {
            // SQL string
            string SQL = string.Format(@"
            INSERT INTO Reviews(MovieID, UserID, Rating) VALUES("+ MovieID +", "+ UserID +", "+ Rating + @");
            SELECT ReviewID
            FROM Reviews
            WHERE ReviewID = SCOPE_IDENTITY();");

            // Execute SQL
            object result = dataTier.ExecuteScalarQuery(SQL);
            int ReviewID = Convert.ToInt32(result);

            // Create review object
            Review r = new Review(ReviewID, MovieID, UserID, Rating);

            // return Review object
            return r;
    }


    //
    // GetMovieDetail:
    //
    // Given a MOVIE ID, returns detailed information about this movie --- all
    // the reviews, the total number of reviews, average rating, etc.  If the 
    // movie cannot be found, null is returned.
    //
    public MovieDetail GetMovieDetail(int MovieID)
    {
            // Get AVG rating, reviews, num reviews, and movie object
            
            // SQL string to get reviews
            string SQL1 = string.Format(@"
            SELECT ReviewID, MovieID, UserID, Rating
            FROM Reviews
            WHERE MovieID = "+ MovieID +@"
            ORDER BY Rating DESC, UserID ASC");

            // SQL string to get movie name
            string SQL2 = string.Format(@"
            SELECT MovieName
            FROM Movies
            WHERE MovieID = " + MovieID);

            // Execute SQL
            DataSet ds = dataTier.ExecuteNonScalarQuery(SQL1);
            object result = dataTier.ExecuteScalarQuery(SQL2);

            // Loop through table of reviews and count reviews, sum rating
            Review r;
            int r_ID, m_ID, u_ID, rating;
            int num_reviews = 0;
            double sum_ratings = 0;
            List<Review> reviews = new List<Review>();

            foreach(DataRow row in ds.Tables["TABLE"].Rows)
            {
                r_ID = Convert.ToInt32(row["ReviewID"]);
                m_ID = Convert.ToInt32(row["MovieID"]);
                u_ID = Convert.ToInt32(row["UserID"]);
                rating = Convert.ToInt32(row["Rating"]);
                num_reviews++;
                sum_ratings += rating;
                r = new Review(r_ID, m_ID, u_ID, rating);
                reviews.Add(r);
            }

            // Compute Average Rating
            double avg_rating = 0.0;

            if (num_reviews > 0)
                avg_rating = sum_ratings / num_reviews;

            // Create movie detail object and return it
            Movie m = new Movie(MovieID, (string)result);
            MovieDetail md = new MovieDetail(m, avg_rating, num_reviews, reviews);
            return md;
    }


    //
    // GetUserDetail:
    //
    // Given a USER ID, returns detailed information about this user --- all
    // the reviews submitted by this user, the total number of reviews, average 
    // rating given, etc.  If the user cannot be found, null is returned.
    //
    public UserDetail GetUserDetail(int UserID)
    {
            // Get all reviews, total # of reviews, 

            // SQL string to get all reviews
            string SQL1 = string.Format(@"
            SELECT M.MovieName, R.ReviewID, R.MovieID, R.UserID, R.Rating
            FROM Reviews R
            INNER JOIN Movies M ON M.MovieID = R.MovieID
            WHERE R.UserID = "+ UserID +@"
            ORDER BY M.MovieName ASC");

            // SQL string to get UserName
            string SQL2 = string.Format(@"
            SELECT UserName, Occupation
            FROM Users
            WHERE UserID = " + UserID);

            // Execute SQL
            DataSet ds1 = dataTier.ExecuteNonScalarQuery(SQL1);
            DataSet ds2 = dataTier.ExecuteNonScalarQuery(SQL2);
            DataTable dt = ds2.Tables["TABLE"];

            // Create list of reviews, count reviews, and sum ratings
            Review r;
            int num_reviews = 0;
            double sum_ratings = 0.0;
            int ReviewID, MovieID, u_ID, rating;
            List<Review> reviews = new List<Review>();

            foreach(DataRow row in ds1.Tables["TABLE"].Rows)
            {
                //
                ReviewID = Convert.ToInt32(row["ReviewID"]);
                MovieID = Convert.ToInt32(row["MovieID"]);
                u_ID = Convert.ToInt32(row["UserID"]);
                rating = Convert.ToInt32(row["Rating"]);
                num_reviews++;
                sum_ratings += rating;
                r = new Review(ReviewID, MovieID, u_ID, rating);
                reviews.Add(r);
            }

            // Create User object
            string UserName = Convert.ToString(dt.Rows[0]["UserName"]);
            string Occupation = Convert.ToString(dt.Rows[0]["Occupation"]);
            User u = new User(UserID, UserName, Occupation);

            // Create UserDetail object and return it
            double avg_rating = num_reviews / sum_ratings;
            UserDetail ud = new UserDetail(u, avg_rating,num_reviews, reviews);
            return ud;
    }


    //
    // GetTopMoviesByAvgRating:
    //
    // Returns the top N movies in descending order by average rating.  If two
    // movies have the same rating, the movies are presented in ascending order
    // by name.  If N < 1, an EMPTY LIST is returned.
    //
    public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
    {
            // SQL string
            string SQL = string.Format(@"
            SELECT TOP " + N + @" t.MovieName, t.MovieID, AVG(CAST(t.Rating AS DECIMAL(10,2))) AS avg_rating
            FROM
            (
	            SELECT M.MovieName, R.MovieID, R.Rating
	            FROM Movies M, Reviews R
	            WHERE M.MovieID = R.MovieID
            ) t
            GROUP BY t.MovieName, t.MovieID
            ORDER BY avg_rating DESC, t.MovieName DESC");

            // Execute SQL
            DataSet ds = dataTier.ExecuteNonScalarQuery(SQL);

            // For each movie add the movie to list
            Movie m;
            int movieID;
            string movieName;
            List<Movie> movies = new List<Movie>();

            foreach (DataRow row in ds.Tables["TABLE"].Rows)
            {
                movieID = Convert.ToInt32(row["MovieID"]);
                movieName = Convert.ToString(row["MovieName"]);
                m = new Movie(movieID, movieName);
                movies.Add(m);
            }

            // return list of movies
            return movies;
    }


  }//class
}//namespace
