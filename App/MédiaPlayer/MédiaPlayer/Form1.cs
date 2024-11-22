using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        private TextBox SearchBox;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadMusicFiles();  
        }

        private void LoadMusicFiles()
        {
            string musicFolderPath = @"..\..\..\Music";

            try
            {
                // Effacer les éléments existants dans le TreeView
                MusicList.Nodes.Clear();

                // Récupérer tous les fichiers mp3 du répertoire spécifié
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");  

                foreach (string musicFile in musicFiles)
                {
                    // Récupérer uniquement le nom du fichier sans le chemin complet
                    string fileName = Path.GetFileName(musicFile);
                    MusicList.Nodes.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des fichiers musicaux : " + ex.Message);
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = SearchBox.Text.ToLower(); // Récupère le texte de recherche en minuscule

            // Recharger les fichiers et les filtrer en fonction de la recherche
            string musicFolderPath = @"..\..\..\Music";

            try
            {
                // Effacer les éléments existants dans le TreeView
                MusicList.Nodes.Clear();

                // Récupérer tous les fichiers mp3 du répertoire spécifié
                string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3");  

                // Filtrer les fichiers en fonction de la recherche
                var filteredFiles = musicFiles
                    .Where(file => Path.GetFileName(file).ToLower().Contains(searchText)) // Filtre par nom de fichier
                    .ToArray();

                foreach (string musicFile in filteredFiles)
                {
                    string fileName = Path.GetFileName(musicFile);
                    MusicList.Nodes.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des fichiers musicaux : " + ex.Message);
            }
        }
    }
}
