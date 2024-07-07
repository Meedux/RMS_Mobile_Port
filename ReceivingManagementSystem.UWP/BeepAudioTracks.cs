using ReceivingManagementSystem.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.BeepAudioTracks))]
namespace ReceivingManagementSystem.UWP
{
    class BeepAudioTracks : ReceivingManagementSystem.Wrapper.IBeepAudioTracks
    {
        private static Dictionary<AudioTrackName, MediaElement> audioTracks = new Dictionary<AudioTrackName, MediaElement>();

        private ISaveSettingsWrapper _pSaveSettingsWrapper;

        // 設定情報
        // これらの値はResourcesから取得する
        // Setting information
        // These values are obtained from Resources
        public static float volume { get; private set; }

        //static BeepAudioTracks m_hBeepAudioTracks = null;

        public BeepAudioTracks()
        {
            volume = 1.0f;
            _pSaveSettingsWrapper = DependencyService.Get<ISaveSettingsWrapper>();
        }

        /**
         * オーディオトラックをあらかじめセットアップする
         * オーディオトラックを生成するのに一定の処理時間がかかるため、利用側のタイミングでセットアップできるようにしている
         * Set up audio tracks in advance
         * Since it takes a certain processing time to generate an audio track, it is set up at the timing of the user side
         * @param resources 設定情報のリソース Resources for configuration information
         */
        public void SetupAudioTracks()
        {
            audioTracks = new Dictionary<AudioTrackName, MediaElement>();

            string track1 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track1, string.Empty);
            string track2 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track2, string.Empty);
            string track3 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track3, string.Empty);
            string track4 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track4, string.Empty);
            string track5 = _pSaveSettingsWrapper.GetString(ReceivingManagementSystem.Common.Constants.Setting_Sound_Track5, string.Empty);

            audioTracks.Add(AudioTrackName.TRACK_1, string.IsNullOrEmpty(track1) ? LoadSoundFile("track1.wav") : LoadSoundFileSetting(track1));
            audioTracks.Add(AudioTrackName.TRACK_2, string.IsNullOrEmpty(track2) ? LoadSoundFile("track2.wav") : LoadSoundFileSetting(track2));
            audioTracks.Add(AudioTrackName.TRACK_3, string.IsNullOrEmpty(track3) ? LoadSoundFile("track3.wav") : LoadSoundFileSetting(track3));
            audioTracks.Add(AudioTrackName.TRACK_4, string.IsNullOrEmpty(track4) ? LoadSoundFile("track4.wav") : LoadSoundFileSetting(track4));
            audioTracks.Add(AudioTrackName.TRACK_5, string.IsNullOrEmpty(track5) ? LoadSoundFile("track5.wav") : LoadSoundFileSetting(track5));
        }

        private MediaElement LoadSoundFile(string strFileName)
        {
            MediaElement snd = new MediaElement();

            Task.Run(() => snd.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                snd.AutoPlay = false;
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + strFileName));
                var stream = await file.OpenAsync(FileAccessMode.Read);
                snd.SetSource(stream, file.ContentType);
            })).Wait();

            return snd;
        }

        private MediaElement LoadSoundFileSetting(string strFileName)
        {
            MediaElement snd = new MediaElement();

            Task.Run(() => snd.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                snd.AutoPlay = false;
                StorageFile file = await StorageFile.GetFileFromPathAsync(strFileName);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                snd.SetSource(stream, file.ContentType);
            })).Wait();

            return snd;
        }

        /**
         * すべてのオーディオトラックを解放する
         * アプリを終了するまでに必ず解放される必要がある
         * Free all audio tracks
         * It must be released before the application is closed
         */
        public void ReleaseAudioTracks()
        {
            //do nothing

        }

        /**
         * 指定した名前のオーディオトラックを再生する
         * Play an audio track with the specified name
         * @param audioTrackName オーディオトラック名 Audio track name
         */
        private async Task PlayAsync(AudioTrackName audioTrackName)
        {
            var mediaElement = audioTracks[audioTrackName];

            await mediaElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaElement.Stop();
                mediaElement.Volume = volume;
                mediaElement.Play();
            });
        }

        /**
         * 指定した名前のオーディオトラックを停止する
         * Stop the audio track with the specified name
         * @param audioTrackName オーディオトラック名 Audio track name
         */
        public void Stop(AudioTrackName audioTrackName)
        {
            var mediaElement = audioTracks[audioTrackName];
            mediaElement.Stop();
        }

        /**
         * 指定した名前のオーディオトラックが再生中かどうか
         * Whether an audio track with the specified name is playing
         * @param audioTrackName オーディオトラック名 Audio track name
         * @return 再生中:true、停止中:false Playing: true, stopped: false
         */
        public bool IsPlaying(AudioTrackName audioTrackName)
        {
            bool bPlaying = audioTracks[audioTrackName].CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing;
            return bPlaying;
        }

        /**
         * 全てのオーディオトラックを停止する
         * Stop all audio tracks
         */
        public void StopAudioTracks()
        {
            foreach (KeyValuePair<AudioTrackName, MediaElement> pEntry in audioTracks)
            {
                MediaElement audioTrack = pEntry.Value;
                audioTrack.Stop();
            }
        }

        #region 同期関数
        public void Play(AudioTrackName audioTrackName)
        {
            //完全同期化すると(await PlayAsync)を待機する -> アプリが固まるので、
            //再生だけを命令して終了。
            Task.Run(() => PlayAsync(audioTrackName));
        }

        #endregion

    }
}
