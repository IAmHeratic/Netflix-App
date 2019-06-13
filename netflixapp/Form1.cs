/*
 * Jose E. Rodriguez
 * CS 341 Spring 2018
 * Project 8 Part 1 - Netflix Database App
 * University of Illinois at Chicago
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace NetflixApp
{
    public partial class Form1 : Form
    {
        // Returns the name of the last button pressed
        private string lastButtonPressed = "None";

        public Form1()
        {
            InitializeComponent();
        }

        /* Checks that the database exists */
        private bool fileExists(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                string msg = string.Format("Input file not found: '{0}'",
                    filename);

                MessageBox.Show(msg);
                return false;
            }

            // exists!
            return true;
        }


        /* Displays Average Rating and Movie ID in text boxes right of listbox */
        private void getAvgRating_movieID()
        {
            int index = this.listBox1.SelectedIndex;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get ID and Average rating for the movie
            BusinessTier.Movie movie = btier.GetMovie(this.listBox1.Items[index].ToString());

            if (movie == null)
            {
                MessageBox.Show("Movie could not be found!");
            }
            else
            {
                // Movie exists, display ID and AVG rating
                BusinessTier.MovieDetail m = btier.GetMovieDetail(movie.MovieID);
                this.textBox2.Text = movie.MovieID.ToString();
                this.textBox3.Text = m.AvgRating.ToString();
            }

            // Done
            this.textBox7.Text = this.listBox1.Items[index].ToString();
        }


        /* Gets the users ID and occupation and inserts into text boxes */
        private void getUserID_Occupation()
        {
            int index = this.listBox1.SelectedIndex;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get user ID and occupation if user exists
            BusinessTier.User user = btier.GetNamedUser(this.listBox1.Items[index].ToString());

            if(user == null)
            {
                MessageBox.Show("User does not exist!");
                return;
            }
            else
            {
                // User exists, display their User ID and Occupation, if any
                this.textBox2.Text = user.UserID.ToString();

                if (string.IsNullOrWhiteSpace(user.Occupation))
                    this.textBox3.Text = "N/A";
                else
                    this.textBox3.Text = user.Occupation.ToString();
            }

            // Done
            this.textBox4.Text = this.listBox1.Items[index].ToString();
        }


        /* Increments the appropriate rating counter */
        private void countRating(ref int[] ratings, int rating)
        {
            if (rating == 1)
                ratings[1]++;
            else if (rating == 2)
                ratings[2]++;
            else if (rating == 3)
                ratings[3]++;
            else if (rating == 4)
                ratings[4]++;
            else
                ratings[5]++;
        }


        /* When the user clicks a movie name in the listbox, a different listbox 
           displays all of its ratings, a third listbox displays statistics about the ratings */
        private void getAllMovieRatings()
        {
            int index = this.listBox1.SelectedIndex;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get the movie and all of it's ratings
            BusinessTier.Movie movie = btier.GetMovie(this.listBox1.Items[index].ToString());

            if (movie == null)
            {
                MessageBox.Show("Movie does not exist!");
                return;
            }
            else
            {
                // Display ratings and count ratings
                int[] ratings = new int[] { 0, 0, 0, 0, 0, 0 };
                this.listBox2.Items.Clear();
                BusinessTier.MovieDetail movieDetails = btier.GetMovieDetail(movie.MovieID);

                if (movieDetails.NumReviews == 0)
                {
                    this.listBox2.Items.Add("No Reviews");
                }
                else
                {
                    // For all reviews, get User ID + Rating and insert
                    this.listBox2.Items.Add(movie.MovieName);
                    this.listBox2.Items.Add("");

                    foreach (BusinessTier.Review R in movieDetails.Reviews)
                    {
                        this.listBox2.Items.Add(R.UserID + ": " + R.Rating);
                        countRating(ref ratings, R.Rating);
                    }
                }

                // Display the rating counts
                int ratingsTotal = 0;
                this.listBox3.Items.Clear();
                this.listBox3.Items.Add(movie.MovieName);
                this.listBox3.Items.Add("");

                for (int i = 1; i <= 5; ++i)
                {
                    this.listBox3.Items.Add(i + ":" + ratings[i]);
                    ratingsTotal += ratings[i];
                }

                this.listBox3.Items.Add("");
                this.listBox3.Items.Add("Total: " + ratingsTotal);
            }
        }
        

        /* Displays all of the users reviews in a listbox */
        public void getAllUserReviews()
        {
            int index = this.listBox1.SelectedIndex;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get user if the user exists
            BusinessTier.User user = btier.GetNamedUser(this.listBox1.Items[index].ToString());

            if (user == null)
            {
                MessageBox.Show("User does not exist!");
                return;
            }
            else
            {
                // Get all of the users reviews
                this.listBox2.Items.Clear();
                BusinessTier.UserDetail userDetails = btier.GetUserDetail(user.UserID);

                if (userDetails.NumReviews == 0)
                {
                    this.listBox2.Items.Add("No Reviews");
                }
                else
                {
                    // For all reviews, get movie name + rating and insert
                    this.listBox2.Items.Add(user.UserName);
                    this.listBox2.Items.Add("");

                    foreach (BusinessTier.Review U in userDetails.Reviews)
                    {
                        BusinessTier.Movie m = btier.GetMovie(U.MovieID);
                        this.listBox2.Items.Add(m.MovieName + "->" + U.Rating);
                    }
                }
            }
        }


        /* List box where user selects a movie and avg rating + movie ID are returned */
        private void listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            if(this.lastButtonPressed == "All Movies")
            {
                getAvgRating_movieID();
                getAllMovieRatings();
            } else if (this.lastButtonPressed == "All Users")
            {
                getUserID_Occupation();
                getAllUserReviews();
            }

            this.Cursor = Cursors.Default;
        }


        /* Movies button that retrieves all movies and inserts into listbox */
        private void all_movies_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get list of movies
            IReadOnlyList < BusinessTier.Movie > movies = btier.GetAllMovies();

            // Clear all data in list and text boxes
            this.listBox1.Items.Clear();
            this.listBox2.Items.Clear();
            this.textBox2.Text = "";
            this.textBox3.Text = "";

            // Insert movie names in list box
            foreach (BusinessTier.Movie M in movies)
            {
                listBox1.Items.Add(M.MovieName);
            }

            // Change labels
            this.label1.Text = "Movie ID";
            this.label2.Text = "AVG Rating";
            this.label3.Text = "Movie Ratings";

            // Done
            this.lastButtonPressed = "All Movies";
            //this.listBox1.SelectedIndex = 0;
            this.Cursor = Cursors.Default;
        }


        /* Lists all the users in the listbox*/
        private void all_users_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if(!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Retrieve all users and insert into listbox
            IReadOnlyList< BusinessTier.User > userList = btier.GetAllNamedUsers();

            // Clear all data in list and text boxes
            this.listBox1.Items.Clear();
            this.listBox2.Items.Clear();
            this.textBox2.Text = "";
            this.textBox3.Text = "";

            // Insert all usersnames
            foreach(BusinessTier.User U in userList)
            {
                this.listBox1.Items.Add(U.UserName);
            }

            // Change labels
            this.label1.Text = "User ID";
            this.label2.Text = "Occupation";
            this.label3.Text = "User's Ratings";

            // Done
            this.lastButtonPressed = "All Users";
            this.Cursor = Cursors.Default;
        }


        /* When this button is clicked, display the top N movies */
        private void topNMovies_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            // test connection to database
            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Check that textbox has valid input
            if(string.IsNullOrWhiteSpace(this.textBox6.Text))
            {
                MessageBox.Show("Please enter a value in the textbox!");
                return;
            }

            int N = Convert.ToInt32(this.textBox6.Text);

            if (N < 1)
            {
                MessageBox.Show("Please enter a number greater than 0!");
                return;
            }

            // Get top N movies
            this.listBox1.Items.Clear();
            this.Cursor = Cursors.WaitCursor;
            IReadOnlyList<BusinessTier.Movie> topNMovies = btier.GetTopMoviesByAvgRating(N);


            // Insert the top N movies
            foreach(BusinessTier.Movie M in topNMovies)
            {
                BusinessTier.MovieDetail movieDetail = btier.GetMovieDetail(M.MovieID);
                this.listBox1.Items.Add(M.MovieName + ":" + movieDetail.AvgRating);
            }

            // Done
            this.Cursor = Cursors.Default;
            this.lastButtonPressed = "Top N Movies";
        }


        /* Inserts a rating into the database */
        private void insert_movieRating_Click(object sender, EventArgs e)
        {
            // Check that a movie was selected
            if (string.IsNullOrWhiteSpace(this.textBox7.Text))
            {
                MessageBox.Show("Please select a Movie from the list!");
                return;
            }

            // Check that a Username was selected
            if (string.IsNullOrWhiteSpace(this.textBox4.Text))
            {
                MessageBox.Show("Please select a Username from the list!");
                return;
            }

            // Check that Rating is valid
            int rating = 0;
            string s = this.textBox5.Text;
            bool validRating = int.TryParse(s, out rating);

            if (!(validRating && (rating <= 5) && (rating >= 1)))
            {
                MessageBox.Show("Please enter a rating between 1 and 5!");
                return;
            }

            // test connection to database
            BusinessTier.Business btier = new BusinessTier.Business(this.textBox1.Text);

            if (!btier.TestConnection())
            {
                MessageBox.Show("Can't connect to the database!");
                return;
            }

            // Get movie ID and user ID
            BusinessTier.Movie M = btier.GetMovie(this.textBox7.Text);
            BusinessTier.User U = btier.GetNamedUser(this.textBox4.Text);

            // Insert review
            btier.AddReview(M.MovieID, U.UserID, rating);
            MessageBox.Show("Review was succesfully inserted!");
        }
    }
}
