using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace ReflectionSDK
{

    public class ReflectionSongBuilderWindow : EditorWindow
    {
        string SongTitle = "Song Title";
        string Album = "Album";
        string Artists = "Artists";
        string Year = "Year";

        [SerializeField]
        Texture2D albumArt;
        [SerializeField]
        AudioClip Track;


        [MenuItem("Reflection/Song Builder")]
        static void OpenSongBuilderWindow()
        {
            EditorWindow.GetWindow(typeof(ReflectionSongBuilderWindow));
        }

        void OnGUI()
        {
            GUILayout.Label("Song Builder", EditorStyles.boldLabel);
            GUILayout.Label("make a song with specific details, and packs it into simple binary files");

            SongTitle = EditorGUILayout.TextField("Song Title", SongTitle);
            Album = EditorGUILayout.TextField("Song Album", Album);
            Artists = EditorGUILayout.TextField("Song Artists", Artists);
            Year = EditorGUILayout.TextField("Song Year", Year);

            GUILayout.Label("Album Art:");
            albumArt = (Texture2D)EditorGUILayout.ObjectField("Album Art", albumArt, typeof(Texture2D), false);
            GUILayout.Label("Track:");
            Track = (AudioClip)EditorGUILayout.ObjectField("Track", Track, typeof(AudioClip), false);

            if (GUILayout.Button("Pack Song"))
            {
                Pack();
            }

            if (GUILayout.Button("Load"))
            {
                LoadSong();
            }
        }

        public void Pack()
        {
            Song song = new Song();
            song.Title = SongTitle;
            song.Artists = Artists;
            song.Year = Year;
            song.AlbumArt = albumArt;
            song.Album = Album;
            song.track = Track;

            Debug.Log("Created a song with the details, track name: " + song.Title);

            // Serialize the song data
            byte[] serializedData;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialize Texture2D data (raw texture bytes)
                byte[] textureBytes = song.AlbumArt.EncodeToPNG(); // Convert to PNG (or JPEG, etc.)
                formatter.Serialize(stream, textureBytes);

                // Serialize AudioClip data (raw audio bytes)
                float[] audioSamples = new float[song.track.samples * song.track.channels];
                song.track.GetData(audioSamples, 0); // Get audio data

                // Convert float samples to bytes (assuming 32-bit float PCM)
                byte[] audioBytes = new byte[audioSamples.Length * sizeof(float)];
                Buffer.BlockCopy(audioSamples, 0, audioBytes, 0, audioBytes.Length);

                formatter.Serialize(stream, audioBytes);

                // Serialize the rest of the song data
                formatter.Serialize(stream, song);
                serializedData = stream.ToArray();
            }

            // Save the serialized data to a .reflection file
            string filePath = EditorUtility.SaveFilePanel("Save Song", "", "NewSong.reflection", "reflection");
            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllBytes(filePath, serializedData);
                Debug.Log($"Song saved as {filePath}");
            }
        }


        // New method for loading a song from a .reflection file
        public void LoadSong()
        {
            string filePath = EditorUtility.OpenFilePanel("Load Song", "", "reflection");
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("No file selected. Load operation canceled.");
                return;
            }

            try
            {
                byte[] serializedData = File.ReadAllBytes(filePath);

                using (MemoryStream stream = new MemoryStream(serializedData))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialize Texture2D data
                    byte[] textureBytes = (byte[])formatter.Deserialize(stream);
                    Texture2D loadedAlbumArt = new Texture2D(1, 1); // Create an empty texture
                    loadedAlbumArt.LoadImage(textureBytes);

                    // Deserialize AudioClip data
                    byte[] audioBytes = (byte[])formatter.Deserialize(stream);
                    float[] audioSamples = new float[audioBytes.Length / sizeof(short)];
                    Buffer.BlockCopy(audioBytes, 0, audioSamples, 0, audioBytes.Length);

                    // Create an AudioClip from the loaded audio samples
                    AudioClip loadedTrack = AudioClip.Create("LoadedTrack", audioSamples.Length, 1, 44100, false);
                    loadedTrack.SetData(audioSamples, 0);

                    // Deserialize the rest of the song data
                    Song loadedSong = (Song)formatter.Deserialize(stream);

                    Debug.Log($"Loaded song: {loadedSong.Title}");
                    Debug.Log($"Album: {loadedSong.Album}");
                    Debug.Log($"Artists: {loadedSong.Artists}");
                    Debug.Log($"Genre: {loadedSong.Genre}");
                    Debug.Log($"Year: {loadedSong.Year}");
                    // Add more properties as needed
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading song: {e.Message}");
            }
        }
    }
}
