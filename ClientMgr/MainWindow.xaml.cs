using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClientMgr
{
    public partial class MainWindow : Window
    {
        private int _currentPage;
        private const int _elementsPerPage = 2;
        public MainWindow()
        {
            InitializeComponent();

            Update();

            SearchBox.TextChanged += (_, __) => Update();
        }

        private async void Update()
        {
            Cursor = Cursors.Wait;

            string search = SearchBox.Text;

            // How much there's clients and how much left after filtering
            int total = 0;
            int shown = 0;
            int totalPages = 0;

            ClientsGrid.ItemsSource = await Task.Run(() =>
            {
                var temp = Client.GetClients();

                total = temp.Count();

                // Filtering

                if (!string.IsNullOrEmpty(search))
                {
                    temp = temp.Where(x =>
                        x.FirstName.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > 0 ||
                        x.MiddleName.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > 0 ||
                        x.LastName.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > 0);
                }

                shown = temp.Count();

                // Pagination
                totalPages = (int)Math.Ceiling((double)temp.Count() / _elementsPerPage);

                temp = temp
                    .Skip(_currentPage * _elementsPerPage)
                    .Take(_elementsPerPage);

                return temp.ToList();
            });

            // Create buttons for every page / Next & Previous
            PaginationPanel.Children.Clear();

            Button prev = CreatePaginationButton(_currentPage - 1, "<");

            for (int i = 0; i < totalPages; i++)
            {
                CreatePaginationButton(i, $"{i}");
            }

            Button next = CreatePaginationButton(_currentPage + 1, ">");

            prev.IsEnabled = _currentPage > 0 && totalPages > 0;
            next.IsEnabled = _currentPage < totalPages - 1;

            StatusTextBlock.Text = $"{shown} item(s) shown | {total} total";

            // Show no entries found text if there's nothing to show
            NoEntriesText.Visibility = totalPages > 0 ? Visibility.Collapsed : Visibility.Visible;

            Cursor = null;
        }

        private Button CreatePaginationButton(int page, string glyph)
        {
            Button btn = new Button
            {
                Content = glyph,
                Tag = page
            };

            btn.Click += (sender, __) =>
            {
                _currentPage = (int)(sender as Button).Tag;
                Update();
            };

            if (_currentPage == page)
                btn.IsEnabled = false;

            PaginationPanel.Children.Add(btn);
            return btn;
        }
    }

    public class Client
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public Client(string firstName, string lastName, string middleName)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }

        public Client(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = "";
        }

        public static IEnumerable<Client> GetClients()
        {
            yield return new Client("Rick", "Rubin");
            yield return new Client("John", "Frusciante", "Anthony");
            yield return new Client("Jimi", "Hendrix");
            yield return new Client("Steve", "Ray", "Vaughan");
            yield return new Client("Michael", "Balzary");
        }
    }
}
