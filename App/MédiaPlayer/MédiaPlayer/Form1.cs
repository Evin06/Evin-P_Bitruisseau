using System;
using System.IO;
using System.Windows.Forms;

namespace MédiaPlayer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadMusicFiles();  
        }
        private void LoadMusicFiles()
        {
            string musicFolderPath = @"..\..\..\Music";

            try
            {
      
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

    }
}
