
using System;
using UnityEngine;


namespace ReflectionSDK
{
    [System.Serializable]
    public class Song
    {
        public string Title;
        public string Album;
        public string Artists;
        public string Genre;
        public string Year;
        [NonSerialized] // Exclude from default serialization
        public Texture2D AlbumArt;
        [NonSerialized] // Exclude from default serialization
        public AudioClip track;
    }
}
