using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;
using Packages.Common.Extend;
using Packages.Settings;

namespace Packages.SE
{

    /// <summary>
    /// 音效频道
    /// </summary>
    public sealed class SeChannel : ClassExtension, IDisposable
    {
        /// <summary>
        /// 播放队列顺番
        /// </summary>
        public int PlayOrder { get; set; }

        private readonly AudioSource _audioSource;

        /// <summary>
        /// 音源
        /// </summary>
        public AudioSource AudioSource => _audioSource;

        /// <summary>
        /// 渐变信息
        /// </summary>
        private readonly SeFadeInfo _fade = new SeFadeInfo();

        private bool _isFadeStart;

		/// <summary>
		/// 是否空闲
		/// </summary>
        public bool Idle => !_audioSource.isPlaying || !_audioSource.clip;

		/// <summary>
        /// 渐入/渐出回掉函数
        /// </summary>
        private event Action<SeFadeType, float> _onFadeCallback;
        public event Action<SeFadeType, float> OnFadeCallback
        {
	        add { _onFadeCallback += value; } remove { _onFadeCallback -= value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iAudioSource">音源</param>
        /// <param name="iInfo">音效信息</param>
        public SeChannel(AudioSource iAudioSource, SeData iInfo) {
            _audioSource = iAudioSource;

            // 设定频道相关信息
            _audioSource.mute = iInfo.mute;
            _audioSource.bypassEffects = iInfo.bypassEffects;
            _audioSource.bypassListenerEffects = iInfo.bypassListenerEffects;
            _audioSource.bypassReverbZones = iInfo.bypassReverbZones;
            _audioSource.playOnAwake = iInfo.playOnAwake;
            _audioSource.loop = iInfo.loop;
            _audioSource.priority = iInfo.priority;
            _audioSource.volume = iInfo.volume;
            _audioSource.pitch = iInfo.pitch;
            _audioSource.panStereo = iInfo.panStereo;
            _audioSource.spatialBlend = iInfo.spatialBlend;
            _audioSource.reverbZoneMix = iInfo.reverbZoneMix;
            _audioSource.dopplerLevel = iInfo.se3D.dopplerLevel;
            _audioSource.spread = iInfo.se3D.spread;
            _audioSource.rolloffMode = iInfo.se3D.rollOff;
            _audioSource.minDistance = iInfo.se3D.minDistance;
            _audioSource.maxDistance = iInfo.se3D.maxDistance;

            // 渐变信息
            _fade.type = iInfo.fade.type;
            _fade.fadeSpeed = iInfo.fade.fadeSpeed;
            _fade.maxVolume = iInfo.fade.maxVolume;

            // 回调函数
            _fade.OnFade += OnFade;
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="iAudioClip">音频剪辑</param>
        /// <param name="iFadeType">渐入/渐出类型</param>
        /// <param name="iOnFade">渐入/渐出回调函数</param>
        public void Play(
            AudioClip iAudioClip, 
            SeFadeType iFadeType = SeFadeType.None, 
            Action<SeFadeType, float> iOnFade = null)
        {
            _audioSource.Stop();
            _audioSource.clip = iAudioClip;
            switch (iFadeType)
            {
	            case SeFadeType.FadeIn:
		            _audioSource.volume = 0.0f;
		            break;
	            case SeFadeType.FadeOut:
		            _audioSource.volume = _fade.maxVolume;
		            break;
	            case SeFadeType.None:
		            break;
	            default:
		            throw new ArgumentOutOfRangeException("iFadeType", iFadeType, null);
            }
            _audioSource.Play();

            // 渐变信息设定
            if(null == _fade || SeFadeType.None == iFadeType) {
                _isFadeStart = false;
            } else {
                _isFadeStart = true;

                _fade.type = iFadeType;
                if(null != iOnFade) {
                    _fade.OnFade += iOnFade;
                }
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _audioSource.Stop();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            _audioSource.Pause();
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        public void Resume()
        {
            _audioSource.UnPause();
        }

        public void Update() {
            // 非渐变类型
            if(false == _isFadeStart) {
                return;
            }

            // 尚未播放
            if(false == AudioSource.isPlaying) {
                return;
            }

            // 渐变
            if(null != _fade && null != AudioSource) {
                _isFadeStart = !_fade.Fade(AudioSource);
            }
        }

        /// <summary>
        /// 渐变事件
        /// </summary>
        /// <param name="iFadeType">渐变类型</param>
        /// <param name="iRestTime">剩余时间</param>
        public void OnFade(SeFadeType iFadeType, float iRestTime) {
            Info("OnFade():Type:{0} RestTime:{1}", iFadeType, iRestTime);
            _onFadeCallback?.Invoke(iFadeType, iRestTime);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_audioSource);

            // 回调函数
            _isFadeStart = false;
            _fade.OnFade -= OnFade;
        }
    }

    /// <summary>
    /// 音效管理器(基类)
    /// </summary>
    public abstract class SeManagerBase : ManagerBehaviourBase<SeManagerBase> {

        /// <summary>
        /// 播放准备完毕
        /// </summary>
        [FormerlySerializedAs("ReadyForPlay")] 
        public bool readyForPlay;

        /// <summary>
        /// 背景音乐队列（AssetPath)
        /// </summary>
        protected Queue<string> BgmQueue = new Queue<string>();
        /// <summary>
        /// 当前BGM文件路径
        /// </summary>
        protected string CurBgmPath;

        /// <summary>
        /// SE音量(0.0f ~ 1.0f)
        /// </summary>
        private const string KeySeVolume = "SEVolume";

        /// <summary>
        /// BGM音量(0.0f ~ 1.0f)
        /// </summary>
        private const string KeyBgmVolume = "BGMVolume";

		/// <summary>
		/// 取得游戏音效音量
		/// 备注：
		/// 	代表着，怼游戏全局音效音量的百分比设定
		/// </summary>
		/// <returns>音效音量</returns>
        private static float GetGameVolume()
        {
            return PlayerPrefs.GetFloat(KeySeVolume, 1.0f);
        }

		/// <summary>
		/// 设定游戏音效音量（0.0f ～ 1.0f）
		/// 备注：
		/// 	代表着，怼游戏全局音效音量的百分比设定
		/// </summary>
		/// <param name="iVolume">音效音量（0.0f ～ 1.0f）</param>
		/// <returns>音效音量</returns>
		private static float SetGameVolume(float iVolume) {
			iVolume = 0.0f >= iVolume ? 0.0f : iVolume;
			iVolume = iVolume >= 1.0f ? 1.0f : iVolume;
			PlayerPrefs.SetFloat(KeySeVolume, iVolume);
			return iVolume;
		}

        /// <summary>
		/// 取得游戏BGM音量
		/// 备注 ：
		/// 	代表着，怼游戏全局BGM音量的百分比设定
		/// </summary>
		/// <returns>BGM音量</returns>
        private static float GetBgmVolume()
        {
            return PlayerPrefs.GetFloat(KeyBgmVolume, 1.0f);
        }

		/// <summary>
		/// 设定游戏BGM音量（0.0f ～ 1.0f）
		/// 备注 ：
		/// 	代表着，怼游戏全局BGM音量的百分比设定
		/// </summary>
		/// <param name="iVolume">音效音量（0.0f ～ 1.0f）</param>
		/// <returns>BGM音量</returns>
		private static float SetBgmVolume(float iVolume) {
			iVolume = 0.0f >= iVolume ? 0.0f : iVolume;
			iVolume = iVolume >= 1.0f ? 1.0f : iVolume;
			PlayerPrefs.SetFloat(KeyBgmVolume, iVolume);
			return iVolume;
		}
		
		/// <summary>
		/// 音效频道
		/// </summary>
		private readonly Dictionary<SeChannelType, List<SeChannel>> _channels = 
			new Dictionary<SeChannelType, List<SeChannel>>();

		private float _volume;
        /// <summary>
        /// 音效音量
        /// </summary>
        /// <returns>音效音量</returns>
        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
				// 设定游戏音效音量
				_volume = SetGameVolume(value);
				foreach(var it in _channels)
				{
                    if(SeChannelType.Bgm == it.Key) {
                        continue;
                    }
					foreach(var channel in it.Value) {
                        if (0.0f == channel.AudioSource.volume)
                        {
                            channel.AudioSource.volume = _volume;
                        }
                        else
                        {
                            channel.AudioSource.volume *= _volume;
                        }
					}
				}
            }
        }

		private float _bgmVolume;
        /// <summary>
        /// BGM音量
        /// </summary>
        /// <returns>BGM音量</returns>
        public float BgmVolume
        {
            get
            {
                return _bgmVolume;
            }
            set
            {
				// 设定游戏BGM音量
				_bgmVolume = SetBgmVolume(value);
				foreach(var it in _channels)
				{
                    if(SeChannelType.Bgm != it.Key) {
                        continue;
                    }
					foreach(var channel in it.Value) {
                        if (0.0f == channel.AudioSource.volume)
                        {
                            channel.AudioSource.volume = _bgmVolume;
                        }
                        else
                        {
                            channel.AudioSource.volume *= _bgmVolume;
                        }
					}
				}
            }
        }

        private AudioListener _listener;
        /// <summary>
        /// 音频监听器;
        /// </summary>
        /// <returns></returns>
        public AudioListener Listener => _listener;

        private readonly Dictionary<string, AudioClip> _cachePool = new Dictionary<string, AudioClip>();
        /// <summary>
        /// 音频缓存池
        /// </summary>
        /// <returns>音频缓存池</returns>
        public Dictionary<string, AudioClip> CachePool => _cachePool;

        #region Singleton
        protected override void SingletonAwake()
        {
            base.SingletonAwake();

			// 取得游戏音效音量
            _volume = GetGameVolume();
            _bgmVolume = GetBgmVolume();

			// 创建音效监听器
            CreateListener();
            // 创建频道
            for(var channelIdx = (int)SeChannelType.Ui; 
                channelIdx < (int)SeChannelType.Unkown; 
                ++channelIdx) {
                CreateChannels((SeChannelType)channelIdx);
            }
            Initialized = true;
        }

        protected override void SingletonDestroy()
        {
            base.SingletonDestroy();
            StopAll();
            ClearCachePool();
        }

        /// <summary>
        /// 释放.
        /// </summary>
        public void OnDestroy()
        {
            foreach(KeyValuePair<SeChannelType,List<SeChannel>> it in _channels) {
				foreach(SeChannel channel in it.Value) {
					channel.Dispose();
				}
			}
            _channels.Clear();
        }

#endregion

		/// <summary>
		/// 创建音效监听器
		/// </summary>
        private void CreateListener()
        {
            var go = new GameObject("Listener");
            go.transform.SetParent(transform, false);
            _listener = go.AddComponent<AudioListener>();
        }

		/// <summary>
		/// 追加频道
		/// </summary>
		/// <param name="iType">音频类型</param>
		/// <param name="iChannel">频道</param>
		private void AddChannel(SeChannelType iType, SeChannel iChannel) {

			List<SeChannel> channelsTmp;
			if(false == _channels.TryGetValue(iType, out channelsTmp)) {
				channelsTmp = new List<SeChannel>();
				_channels[iType] = channelsTmp;
			}

            // 设置频道音量
            if (iType == SeChannelType.Bgm)
            {
                iChannel.AudioSource.volume = _bgmVolume;
            }
            else if (iType != SeChannelType.Bgm)
            {
	            iChannel.AudioSource.volume = _volume;
            }
            channelsTmp.Add(iChannel);
		}

        /// <summary>
        /// 创建频道
        /// </summary>
        /// <param name="iChannelType">频道类型</param>
        protected void CreateChannels(SeChannelType iChannelType)
        {
            GameObject channelsObj = null;
            var channelTrans = transform.Find(iChannelType.ToString());
            if(null != channelTrans) {
                channelsObj = channelTrans.gameObject;
            }
			if(null == channelsObj) {
				channelsObj = new GameObject(iChannelType.ToString());
                channelsObj.transform.SetParent(transform, false);
			}

			var seDatas = SysSettings.GetInstance().data.se;
			if(null == seDatas || 0 >= seDatas.Count) {
				this.Error("CreateChannels():There is no SE setting data in build info asset!!!");
				return;
			}
			var seDatasTmep = seDatas
				.Where(iO => iChannelType == iO.channelType)
				.ToList();
			foreach(var loop in seDatasTmep) {
				var channelCount = loop.channelCount;
				for(var _ = 0; _ < channelCount; ++_) {
                    // 生成频道
					var chanelName = $"Channel_{_}";
					var channelObj = new GameObject(chanelName);
                	channelObj.transform.SetParent(channelsObj.transform, false);

					// 追加频道
					var audioSource = channelObj.AddComponent<AudioSource>();
                    if(null == audioSource) {
	                    this.Error("CreateChannels():Add AudioSource Failed!!!(channel:{0})", chanelName);
                        continue;
                    }
                    // 音效设置

                    AddChannel(loop.channelType, new SeChannel(audioSource, loop));
				}
			}
        }

        /// <summary>
        /// 取得频道音源
        /// </summary>
        /// <param name="iChannelType">频道类型</param>
        /// <returns>频道音源</returns>
        public AudioSource GetAudioSource(SeChannelType iChannelType = SeChannelType.Ui) {
            List<SeChannel> channelList;
			if(false == _channels.TryGetValue(iChannelType, out channelList)) {
				this.Error("Play():this type(type={0}) is not exist!!!", iChannelType);
				return null;
			}
			if(null == channelList || 0 >= channelList.Count) {
				this.Error("Play():there is no channel in this type(type={0})", iChannelType);
				return null;
			}

            // 取得当前空闲频道
            var channel = channelList.Find(iC => iC.Idle);
			// 没有空闲频道，则取得最早播放的频道来作为当前播放频道
			if (null != channel) return channel.AudioSource;
			// 便利查找最早播放的频道
            foreach(var loop in channelList) {
	            if(null == channel) {
		            channel = loop;
	            } else {
		            channel = channel.PlayOrder >= loop.PlayOrder ? loop : channel;
	            }
            }

            return channel?.AudioSource;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="iAudioClip">音效</param>
        /// <param name="iChannelType">音效类型</param>
        /// <param name="iFadeType">渐入/渐出类型</param>
        /// <param name="iOnFade">渐入/渐出类型回调函数</param>
        public void Play(
            AudioClip iAudioClip, 
            SeChannelType iChannelType = SeChannelType.Ui,
            SeFadeType iFadeType = SeFadeType.None,
            Action<SeFadeType, float> iOnFade = null)
        {
            List<SeChannel> channelList;
			if(false == _channels.TryGetValue(iChannelType, out channelList)) {
				this.Error("Play():this type(type={0}) is not exist!!!", iChannelType);
				return;
			}
			if(null == channelList || 0 >= channelList.Count) {
				this.Error("Play():there is no channel in this type(type={0})", iChannelType);
				return;
			}

            // 取得当前最大顺番
            // 取得空闲频道
			var maxPlayOrder = 0;
            var channel = channelList.Find(iC => iC.Idle);
			// 没有空闲频道，则取得最早播放的频道来作为当前播放频道
            if (null == channel)
            {
                // 便利查找最早播放的频道
                foreach(var loop in channelList) {
                    if(null == channel) {
                        channel = loop;
                    } else {
                        channel = channel.PlayOrder >= loop.PlayOrder ? loop : channel;
                    }
                    maxPlayOrder = maxPlayOrder <= loop.PlayOrder ? loop.PlayOrder : maxPlayOrder;
                }
            }

            if (null != channel)
            {
                channel.Play(iAudioClip, iFadeType, iOnFade);
                channel.PlayOrder = maxPlayOrder + 1;
            } else {
	            this.Error("Play():There is too busy or no channel to play SE !!!(type:{0})", iChannelType);
			}
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        /// <param name="iPath">路径</param>
        protected abstract AudioClip LoadAudio(string iPath);

		/// <summary>
		/// 播放音效
		/// </summary>
		/// <param name="iPath">路径</param>
		/// <param name="iType">频道类型</param>
		/// <param name="iFadeType">渐变类型</param>
		/// <param name="iOnFade">渐入/渐出类型回调函数</param>
        public void Play(
            string iPath, 
            SeChannelType iType = SeChannelType.Ui,
            SeFadeType iFadeType = SeFadeType.None,
            Action<SeFadeType, float> iOnFade = null)
        {
			if (string.IsNullOrEmpty(iPath))
            {
	            this.Error("Play():The path of SE is null or empty: {0}", iPath);
                return;
            }
            var audioClip = LoadAudio(iPath);
            if (audioClip != null)
            {
                Play(audioClip, iType, iFadeType, iOnFade);
            }
            else
            {
	            this.Error("Play():The SE is not found!!!(path={0})", iPath);
            }
        }

        /// <summary>
		/// 播放音效(特效)
		/// </summary>
		/// <param name="iPath">路径</param>
		/// <param name="iFadeType">渐变类型</param>
		/// <param name="iOnFade">渐入/渐出类型回调函数</param>
        public void PlayEffect(
            string iPath,
            SeFadeType iFadeType = SeFadeType.None,
            Action<SeFadeType, float> iOnFade = null) {
            Play(iPath, SeChannelType.Effect, iFadeType, iOnFade);
        }

        /// <summary>
		/// 播放音效(BGM)
		/// </summary>
		/// <param name="iPath">路径</param>
		/// <param name="iFadeType">渐变类型</param>
		/// <param name="iOnFade">渐入/渐出类型回调函数</param>
        public void PlayBgm(
            string iPath,
            SeFadeType iFadeType = SeFadeType.None,
            Action<SeFadeType, float> iOnFade = null) {

            // 保存BGM队列
            BgmQueue.Enqueue(iPath);
            CurBgmPath = iPath;
            Play(CurBgmPath, SeChannelType.Bgm, iFadeType, iOnFade);
        }

        /// <summary>
        /// Pop播放BGM
        /// </summary>
		/// <param name="iFadeType">渐变类型</param>
        /// <param name="iOnFade">渐入/渐出类型回调函数</param>
        public void PopBgm(
            SeFadeType iFadeType = SeFadeType.None,
            Action<SeFadeType, float> iOnFade = null) {
            if(0 < BgmQueue.Count) {
                CurBgmPath = BgmQueue.Dequeue();
            }
            Play(CurBgmPath, SeChannelType.Bgm, iFadeType, iOnFade);
        }

		/// <summary>
		/// 停止所有音效
		/// </summary>
		public void StopAll() {
			foreach(var it in _channels) {
				foreach(var channel in it.Value) {
					channel.Stop();
				}
			}
		}

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="iType">频道类型</param>
        public void Pause(SeChannelType iType = SeChannelType.Unkown) {
            if(SeChannelType.Unkown == iType) {
                // 暂停所有
                foreach(var it in _channels) {
                    foreach(var loop in it.Value) {
                        loop.Pause();
                    }
                }
            } else {
                List<SeChannel> channelsTmp;
                if (!_channels.TryGetValue(iType, out channelsTmp)) return;
                foreach(var loop in channelsTmp) {
	                loop.Pause();
                }

            }
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        /// <param name="iType">频道类型</param>
        public void Resume(SeChannelType iType = SeChannelType.Unkown) {

            if(SeChannelType.Unkown == iType) {
                // 暂停所有
                foreach(var it in _channels) {
                    foreach(var loop in it.Value) {
                        loop.Resume();
                    }
                }
            } else {
                List<SeChannel> channelsTmp;
                if (!_channels.TryGetValue(iType, out channelsTmp)) return;
                foreach(var loop in channelsTmp) {
	                loop.Resume();
                }

            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update() {
            foreach(var it in _channels) {
                foreach(var loop in it.Value) {
                    loop.Update();
                }
            }
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        /// <param name="iType">频道类型</param>
        public void Stop(SeChannelType iType = SeChannelType.Ui) {

			List<SeChannel> channelsTmp;
			if(false == _channels.TryGetValue(iType, out channelsTmp)) {
				this.Warning("Stop():Type:{0} not exist!!", iType);
				return;
			}
			if(null == channelsTmp || 0 >= channelsTmp.Count) {
				this.Warning("Stop():Type:{0} not exist!!", iType);
				return;
			}
			foreach(SeChannel channel in channelsTmp) {
				channel.Stop();
			}
		}

        /// <summary>
        /// 停止特效音效
        /// </summary>
        public void StopEffect() {
            Stop(SeChannelType.Effect);
        }

        /// <summary>
        /// 停止BGM音效
        /// </summary>
        public void StopBgm() {
            Stop(SeChannelType.Bgm);
        }

		/// <summary>
		/// 清空缓冲池
		/// </summary>
		private void ClearCachePool()
        {
            foreach (var it in _cachePool)
            {
                Resources.UnloadAsset(it.Value);
            }
            _cachePool.Clear();
        }
	}
}
