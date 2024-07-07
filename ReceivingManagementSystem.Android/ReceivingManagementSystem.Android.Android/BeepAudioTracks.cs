using Android.Media;
using Android.Content.Res;
using ReceivingManagementSystem.Android.Interfaces;
using ReceivingManagementSystem.Common;
using System.Collections.Generic;
using Xamarin.Forms;
using ReceivingManagementSystem.Android.Droid;

[assembly: Dependency(typeof(ReceivingManagementSystem.Android.BeepAudioTracks))]
namespace ReceivingManagementSystem.Android
{
    public class BeepAudioTracks : IBeepAudioTracks
    {
        private static Dictionary<AudioTrackName, MediaPlayer> audioTracks = new Dictionary<AudioTrackName, MediaPlayer>();

        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        public static float volume { get; private set; }

        public BeepAudioTracks()
        {
            volume = 1.0f;
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
        }

        public void SetupAudioTracks()
        {
            audioTracks = new Dictionary<AudioTrackName, MediaPlayer>();

            string track1 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track1, string.Empty);
            string track2 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track2, string.Empty);
            string track3 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track3, string.Empty);
            string track4 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track4, string.Empty);
            string track5 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track5, string.Empty);

            audioTracks.Add(AudioTrackName.TRACK_1, LoadSoundFile(track1, "track1"));
            audioTracks.Add(AudioTrackName.TRACK_2, LoadSoundFile(track2, "track2"));
            audioTracks.Add(AudioTrackName.TRACK_3, LoadSoundFile(track3, "track3"));
            audioTracks.Add(AudioTrackName.TRACK_4, LoadSoundFile(track4, "track4"));
            audioTracks.Add(AudioTrackName.TRACK_5, LoadSoundFile(track5, "track5"));
        }

        private MediaPlayer LoadSoundFile(string settingTrack, string defaultTrack)
        {
            MediaPlayer player = new MediaPlayer();
            AssetFileDescriptor afd;
            AssetManager context = MainActivity.AppContext.Assets;

            if (string.IsNullOrEmpty(settingTrack))
            {
                afd = context.OpenFd(defaultTrack + ".wav");
            }
            else
            {
                afd = context.OpenFd(settingTrack + ".wav");
            }

            player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
            player.Prepare();

            return player;
        }

        public void ReleaseAudioTracks()
        {
            foreach (var track in audioTracks.Values)
            {
                track.Release();
            }
        }

        public void Play(AudioTrackName audioTrackName)
        {
            var mediaPlayer = audioTracks[audioTrackName];
            mediaPlayer.Stop();
            mediaPlayer.Start();
        }

        public void Stop(AudioTrackName audioTrackName)
        {
            var mediaPlayer = audioTracks[audioTrackName];
            mediaPlayer.Stop();
        }

        public bool IsPlaying(AudioTrackName audioTrackName)
        {
            var mediaPlayer = audioTracks[audioTrackName];
            return mediaPlayer.IsPlaying;
        }

        public void StopAudioTracks()
        {
            foreach (var mediaPlayer in audioTracks.Values)
            {
                mediaPlayer.Stop();
            }
        }
    }
}
