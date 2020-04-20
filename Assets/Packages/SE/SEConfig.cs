using System;
using UnityEngine;
using UnityEngine.Serialization;
using Packages.Common.Base;

namespace Packages.SE
{
    /// <summary>
    /// 渐变类型
    /// </summary>
    public enum SeFadeType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 渐入
        /// </summary>
        FadeIn,

        /// <summary>
        /// 渐出
        /// </summary>
        FadeOut
    }

	/// <summary>
	/// 音效频道类型
	/// </summary>
	public enum SeChannelType
	{
		/// <summary>
		/// UI
		/// </summary>
		Ui,

		/// <summary>
		/// 背景
		/// </summary>
		Bgm,

		/// <summary>
		/// 特效
		/// </summary>
		Effect,

		/// <summary>
		/// 未知
		/// </summary>
		Unkown
	}

    /// <summary>
    /// 音效渐变信息
    /// </summary>
	[Serializable]
    public class SeFadeInfo : JsonDataBase<SeFadeInfo>
    {
        /// <summary>
        /// 渐变速度
        /// </summary>
        /// <returns></returns>
        [FormerlySerializedAs("FadeSpeed")] 
        public float fadeSpeed = 1.0f;

        /// <summary>
        /// 最大音量
        /// </summary>
        /// <returns></returns>
        [FormerlySerializedAs("MaxVolume")] 
        [UnityEngine.Range(0.0f, 1.0f)]
        public float maxVolume = 1.0f;

		/// <summary>
		/// 类型
		/// </summary>
        [FormerlySerializedAs("Type")] 
		public SeFadeType type = SeFadeType.None;

        /// <summary>
        /// 渐入/渐出回掉函数
        /// </summary>
        private event Action<SeFadeType, float> _onFade;
        public event Action<SeFadeType, float> OnFade { add { _onFade += value; } remove { _onFade -= value; } }

        /// <summary>
        /// 渐变
        /// </summary>
        /// <param name="iAudioSource">音源</param>
        /// <returns>true : 渐变中; false : 渐变结束;</returns>
        public bool Fade(AudioSource iAudioSource)
        {
            var bolFlg = false;
            var curVolume = 1.0f;
            switch (type)
            {
	            case SeFadeType.FadeIn:
	            {
		            var fadeVolume = iAudioSource.volume + Time.deltaTime * fadeSpeed;
		            curVolume = fadeVolume >= maxVolume ? maxVolume : fadeVolume;
		            iAudioSource.volume = curVolume;
		            if (curVolume >= maxVolume) { 
			            bolFlg = true; 
		            }

		            break;
	            }

	            case SeFadeType.FadeOut:
	            {
		            var fadeVolume = iAudioSource.volume - Time.deltaTime * fadeSpeed;
		            curVolume = 0.0f >= fadeVolume ? 0.0f : fadeVolume;
		            iAudioSource.volume = curVolume;
		            if (0.0f >= curVolume) { 
			            bolFlg = true; 
		            }

		            break;
	            }

	            case SeFadeType.None:
		            break;
	            default:
		            throw new ArgumentOutOfRangeException();
            }

            _onFade?.Invoke(type, curVolume);
            return bolFlg;
        }

		/// <summary>
		/// 清空
		/// </summary>
		public override void Clear() {
			base.Clear();
			fadeSpeed = 1.0f;
			maxVolume = 1.0f;
			type = SeFadeType.None;
		}

		/// <summary>
		/// 检测参数
		/// </summary>
		public void Check() {
			maxVolume = (1.0f <= maxVolume) ? 1.0f : maxVolume;
		}
    }

	/// <summary>
	/// 3D音效设置
	/// </summary>
	[Serializable]
	public class SE3DData : JsonDataBase<SE3DData> {
		/// <summary>
		/// 多普勒级别(0.0f ~ 5.0f)。默认：1.0f
		///   在[0,5]之间变化，默认值为1。
		///   决定了音源的多普勒效应的份量，如果设置为0，则没有多普勒效应。
		///     多普勒效应指的是当监听器与音源之间发生相对运动时，声音传递速度发生变化时的效果。
		///       假设有一架飞机掠过上空，飞机上的警报器每个一秒钟发出一次声音，如果相对静止，
		///       则听到的效果是完整的每个一秒一次警报(尽管发出到听到需要较长的时间，但是间隔应该是一样的)，
		///       而当飞机向我快速靠近时，由于距离在不断缩短，从发声到听到的距离在不断缩小，
		///       因此传递所化的时间越来越少，我方听到的警报应该是越来越紧促，
		///       而非均匀的每秒一次，靠近速度越快，警报间隔越短。
		///       当飞机快速离去时，则情况恰好相反，警报声音节奏越来越松缓。-
		/// </summary>
		[FormerlySerializedAs("DopplerLevel")] 
		[UnityEngine.Range(0.0f, 5.0f)]
		public float dopplerLevel = 1.0f;

		/// <summary>
		/// 3D立体声/多声道扬声器的空间扩散角度。(0 ~ 360)。默认：0
		///   设置扬声器空间的立体声传播角度。
		///   好像稍稍影响了回音效果，其它暂时没看出太大的影响，如果读者有更详细解释请告知。
		/// </summary>
		[FormerlySerializedAs("Spread")] 
		[UnityEngine.Range(0, 360)]
		public int spread;

		/// <summary>
		/// 声音衰减模式。默认：Logarithmic。
		///   代表了声音在距离上的衰减速度， (具体衰减数值由曲线决定，X-距离，Y-衰减后剩余百分比).
		///     Logarithmic ： 预制的对数衰减曲线，`可以修改`。
		///     Linear ： 预制的线性衰减曲线，`可以修改`。
		///     Custom ： 自定义的衰减曲线，完全手动设置。
		/// </summary>
		[FormerlySerializedAs("RollOff")] 
		public AudioRolloffMode rollOff = AudioRolloffMode.Logarithmic;

		/// <summary>
		/// 最小距离。
		///   默认值为1米，代表了音量曲线中的最大音量位置。
		///   超越最小距离时，声音将逐渐衰减。如果增大最小距离，
		///   则将相当于增大了3D世界中的声音，因为最小距离以下的位置均获得最大音量。
		///   如果减小最小距离，则相当于减小了3D世界中声音。
		///     因为由Volume参数可知，在曲线上1米处为最大音量，
		///     默认曲线 < 1 米处均获得最大音量，
		///     而当MinDistance < 1时，默认曲线中 X = 1处的音量响应也会变小。
		///     注意上图中Listener竖线代表了监听器与当前音源的相对距离，
		///     而与Volume曲线的交点即是：1米处的最大音量经过3D世界距离传播到当前位置而衰减后的音量大小，
		///     即如果在曲线上，1米处Y坐标是0.8，而Listener处Y坐标是0.4，
		///     那么最终音量是衰减了50%，此参数与Volume参数共同作用输出最终音量大小，也即Volume*0.5
		/// </summary>
		[FormerlySerializedAs("MinDistance")] 
		public float minDistance = 1.0f;

		/// <summary>
		/// 最大距离。默认：500.0f。
		///   当超出此距离时，声音将停止衰减。
		///   注意这里是停止衰减，也就是说，后续更远处听到的声音将保持在最大距离点的声音大小，不代表声音为0
		/// </summary>
		[FormerlySerializedAs("MaxDistance")] 
		public float maxDistance = 500.0f;

		/// <summary>
		/// 检测参数
		/// </summary>
		public void Check() {
			// 多普勒级别
			dopplerLevel = 0.0f >= dopplerLevel ? 0.0f : dopplerLevel;
			dopplerLevel = dopplerLevel >= 5.0f ? 5.0f : dopplerLevel;

			spread = 0 >= spread ? 0 : spread;
			spread %= 360;

			minDistance = 0 >= minDistance ? 0 : minDistance;
			maxDistance = 0 >= maxDistance ? 0 : maxDistance;
		}

		/// <summary>
		/// 清空
		/// </summary>
		public override void Clear() {
			dopplerLevel = 1.0f;
			spread = 0;
			rollOff = AudioRolloffMode.Logarithmic;
			minDistance = 0.0f;
			maxDistance = 0.0f;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		public override void Init() {
			dopplerLevel = 1.0f;
			spread = 0;
			rollOff = AudioRolloffMode.Logarithmic;
			minDistance = 1.0f;
			maxDistance = 500.0f;
		}
	}
	

	/// <summary>
	/// 音效数据定义
	/// </summary>
	[Serializable]
	public class SeData : JsonDataBase<SeData> {

		/// <summary>
		/// 音效频道类型
		/// </summary>
		[FormerlySerializedAs("ChannelType")] 
		public SeChannelType channelType = SeChannelType.Ui;

		/// <summary>
		/// 渐变信息
		/// </summary>
		[FormerlySerializedAs("Fade")] 
		public SeFadeInfo fade = new SeFadeInfo();

		/// <summary>
		/// 音效频道个数
		/// </summary>
		[FormerlySerializedAs("ChannelCount")] 
		public int channelCount = 1;

		/// <summary>
		/// 是否静音音频源。
		///   即音源在继续播放，但是被静音。
		///   与关闭音源的区别时，当恢复时，播放的进度不同。
		///     对于游戏中音效的暂停最好使用这项，它不会卸载声音数据，
		///     可以做到及时播放，音效一般比较多、占用内存小，使用静音可以让画面快速响应，
		///     且可以立刻恢复当前音效，暂停音乐可以使用AudioSource的Pause、Stop或者GameObject的enable，
		///     一般不需要及时响应，此时可以使用Stop，它会卸载音频数据节省内存。
		///     当然如果内存富余也可以使用其它方式，
		///   注意:
		///     Mute跟其它方式的区别:Mute的音源在继续播放着，只是听不到而已，
		///     Pause是播放暂停，而Stop是完全停止，恢复时将从头播放。
		/// </summary>
		[FormerlySerializedAs("Mute")] 
		public bool mute;

		/// <summary>
		/// 音源滤波开关
		///   是那些作用在当前音源的音频滤波器的开关。
		///     滤波器包括
		///       “高通滤波”、“低通滤波”、“回波滤波”、“扭曲滤波”、
		///       “回音滤波”、“和声滤波”等，
		///     这些滤波器可以设置在音源或者监听器上，
		///     勾选此项时，将使那些设置在音源的滤波器失效。
		/// </summary>
		[FormerlySerializedAs("BypassEffects")] 
		public bool bypassEffects;

		/// <summary>
		/// 监听器滤波开关
		///   是那些作用在当前监听器的音频滤波器的开关。
		///     同上，勾选此项时，将使那些设置在监听器的滤波器失效。
		/// </summary>
		[FormerlySerializedAs("BypassListenerEffects")] 
		public bool bypassListenerEffects;

		/// <summary>
		/// 回音混淆开关
		///   当勾选时，不执行回音混淆，即便现在玩家位于回音区域，此音源也不会产生回音。
		///   回音效果取决于监听器位置（一般代表玩家位置）与回音区域位置关系，
		///   而与音源没有直接关联。。
		/// </summary>
		[FormerlySerializedAs("BypassReverbZones")] 
		public bool bypassReverbZones;

		/// <summary>
		/// 启动播放开关.
		///   如果勾选的话，那么当GameObject加载并启用时，立刻播放音频，
		///   即相当于此音源GameObject的组件中Awake方法作用时开始播放。
		///   如果不勾选的话，需要手动调用Play()方法执行播放。。
		/// </summary>
		[FormerlySerializedAs("PlayOnAwake")] 
		public bool playOnAwake = true;

		/// <summary>
		/// 循环播放标识位
		/// </summary>
		[FormerlySerializedAs("Loop")] 
		public bool loop;

		/// <summary>
		/// 播放优先级
		///   决定了当前音源在当前场景存在的所有音源中的播放优先级。
		///   (优先级: 0 = 最重要. 256 = 最不重要. 默认值 = 128.).
		///   背景音乐最好使用0，避免它们有时被其它音源替换出去。
		///     一般手机或者其它播放设备最多允许32个音源同时播放，
		///     这个优先级参数决定了在超出音源数目时，需要暂时关闭一些不重要的音源，
		///     优先播放更重要的音源
		/// </summary>
		[FormerlySerializedAs("Priority")] 
		[UnityEngine.Range(0, 256)]
		public int priority = 128;

		/// <summary>
		/// 音源音量（0.0 ~ 1.0）。默认：1.0f;
		///    此音量代表了监听器处于距离音源1米时的音量大小，代表了最大音量处的声音大小。
		/// </summary>
		[FormerlySerializedAs("Volume")] 
		[UnityEngine.Range(0, 1.0f)]
		public float volume = 1.0f;

		/// <summary>
		/// 音频音调(-3.0f~+3.0f)。默认：1.0f;
		///   代表了播放音频时速度的变化量，默认值是1，代表了正常的播放速度。
		///     < 1 : 慢速播放。
		///     > 1 ：快速播放，速度越快，则音调越高。
		/// </summary>
		[FormerlySerializedAs("Pitch")] 
		[UnityEngine.Range(-3.0f, +3.0f)]
		public float pitch = 1.0f;

		/// <summary>
		/// 声道占比（左声道：-1.0f ～ 右声道：+1.0f）。默认：0.0f;
		///   此数值在[-1,1]之间变化，代表了2D音源的左右声道占比，默认值为0。
		///   代表左右声道输出同样音量大小。
		///     此数值针对2D音源或者2D、3D混合音源有效，纯3D音源无效
		/// </summary>
		[FormerlySerializedAs("PanStereo")] 
		[UnityEngine.Range(-1.0f, +1.0f)]
		public float panStereo;

		/// <summary>
		/// 空间混合。(2D:0.0f ~ 3D:1.0f)。默认：0.0f;
		///   此数值在[0,1]之间变化，指定当前音源是2D音源、3D音源，还是二者插值的复合音源。
		///   此参数决定了引擎作用在此音源上的3D效果的份量。
		///   主要影响“3D Sound Settings”属性组中的参数表现。
		///     比如，如果是2D音源，声音在距离上不衰减，也就没有多普勒效果。
		///   无论2D、3D，与音频滤波器不关联。
		///     例如即便是纯2D音源，它仍然响应回音区域，
		///     而滤波器的控制主要由相应的滤波参数和音源滤波混合参数决定，
		///     混合参数如前面讲到的Bypass Effects、Bypass Listner Effects两个开关以及后面讲到的Reverb Zone Mix。
		///     也就是说，此参数与“3D Sound Settings”属性组中的3D音频设置参数共同作用构成最终的输出，
		///     而当纯2D音源时，3D音频设置将无视，纯3D音源时，3D音频设置得到完整输出，非纯情况则插值混合输出。
		/// </summary>
		[FormerlySerializedAs("SpatialBlend")] 
		public float spatialBlend;

		/// <summary>
		/// 回音混合（0.0f ~ 1.1f）。默认：1.0f;
		///   设置输出到混响区域中的信号量。
		///   一般在[0,1]范围内变化, 不过也允许额外最多放大10分贝(1 - 1.1]来增强声音的远近效果。
		///   也就是说，回响效果的回响距离等很多复杂参数主要由回响滤波器实现，而回响音量大小主要由此因子来简单控制。
		/// </summary>
		[FormerlySerializedAs("ReverbZoneMix")] 
		[UnityEngine.Range(-1.0f, +1.1f)]
		public float reverbZoneMix = 1.0f;

		/// <summary>
		/// 3D声音设置
		/// </summary>
		[FormerlySerializedAs("SE3D")] 
		public SE3DData se3D = new SE3DData();

		/// <summary>
		/// 检测参数
		/// </summary>
		public void Check() {
			channelType = SeChannelType.Unkown == channelType ? SeChannelType.Ui : channelType;

			priority = 0 >= priority ? 0 : priority;
			priority = priority >= 256 ? 256 : priority;

			volume = 0.0f >= volume ? 0.0f : volume;
			volume = volume >= 1.0f ? 1.0f : volume;

			pitch = -3.0f >= pitch ? -3.0f : pitch;
			pitch = pitch >= 3.0f ? 3.0f : pitch;

			panStereo = -1.0f >= panStereo ? -1.0f : panStereo;
			panStereo = panStereo >= 1.0f ? 1.0f : panStereo;

			spatialBlend = 0.0f >= spatialBlend ? 0.0f : spatialBlend;
			spatialBlend = spatialBlend >= 1.0f ? 1.0f : spatialBlend;

			reverbZoneMix = 0.0f >= reverbZoneMix ? 0.0f : reverbZoneMix;
			reverbZoneMix = reverbZoneMix >= 1.1f ? 1.1f : reverbZoneMix;

			se3D?.Check();
			fade?.Check();

		}

		/// <summary>
		/// 清空
		/// </summary>
		public override void Clear() {
			channelType = SeChannelType.Ui;
			channelCount = 0;
			mute = false;
			bypassEffects = false;
			bypassListenerEffects = false;
			bypassReverbZones = false;
			playOnAwake = false;
			loop = false;
			priority = 0;
			volume = 0.0f;
			pitch = 0.0f;
			panStereo = 0.0f;
			spatialBlend = 0.0f;
			reverbZoneMix = 0.0f;
			se3D?.Clear();
			fade?.Clear();
		}

		/// <summary>
		/// 初始化
		/// </summary>
		public override void Init() {
			channelType = SeChannelType.Ui;
			channelCount = 0;
			mute = false;
			bypassEffects = false;
			bypassListenerEffects = false;
			bypassReverbZones = false;
			playOnAwake = true;
			loop = false;
			priority = 128;
			volume = 1.0f;
			pitch = 1.0f;
			panStereo = 0.0f;
			spatialBlend = 0.0f;
			reverbZoneMix = 1.0f;
			se3D?.Init();
			fade?.Init();
		}
	}
}
