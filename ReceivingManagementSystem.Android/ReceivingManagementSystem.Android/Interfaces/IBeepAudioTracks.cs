using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Android.Interfaces
{
    public interface IBeepAudioTracks
    {
        void SetupAudioTracks();
        void ReleaseAudioTracks();
        void Play(AudioTrackName audioTrackName);
        void Stop(AudioTrackName audioTrackName);
        bool IsPlaying(AudioTrackName audioTrackName);
        void StopAudioTracks();
    }

    /**
     * オーディオトラックの名前
     * Audio track name
     */
    public enum AudioTrackName
    {
        TRACK_1 = 0
        , TRACK_2
        , TRACK_3
        , TRACK_4
        , TRACK_5
    }
}
