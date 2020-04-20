using System;
using System.Collections;
using Packages.Common.Base;
using UnityEngine;
using UnityEngine.UI;
using Packages.Common.Base;
using UnityEngine.Serialization;

namespace Packages.SE
{

	/// <summary>
	/// 音效脚本
	/// </summary>
	[Serializable]
	public abstract class SeEffectBase : MonoBehaviour {
		private Button _button;
		private Toggle _toggle;	
        private bool _firstInitValue = true;
        private bool _init;

		/// <summary>
		/// 区分
		/// </summary>
		[FormerlySerializedAs("Division")] [SerializeField]
		public string division;

		/// <summary>
		/// AssetKey
		/// </summary>
		[FormerlySerializedAs("AssetKey")] [SerializeField]
		public string assetKey;

		/// <summary>
		/// 默认资源标识位
		/// </summary>
		[SerializeField]
		public bool Default;

		/// <summary>
		/// 循环播放标识为
		/// </summary>
		[FormerlySerializedAs("Loop")] 
		public bool loop;

		/// <summary>
		/// 延迟时间(单位：秒)
		/// </summary>
		[FormerlySerializedAs("Delay")] 
		public float delay;

		/// <summary>
		/// 是否接口输入
		/// </summary>
		public bool IsInteractable {
			get {
				if (null != _button) {
					return _button.IsInteractable();
				}
				return null != _toggle && _toggle.IsInteractable();
			}
		}

		/// <summary>
		/// 是否为BGM
		/// </summary>
		public virtual bool IsBgm =>
			false == string.IsNullOrEmpty(division) && 
			"BGM".Equals(division);

		void Awake()
        {
            // 初始化SE信息
			InitSeInfo();
        }

		/// <summary>
		/// 初始化SE信息
		/// </summary>
		private void InitSeInfo() {
			_button = GetComponent<Button>();
            _toggle = GetComponent<Toggle>();

            if(null != _toggle)
            {
                _firstInitValue = _toggle.isOn;
            }

            if(null != _button)
            {
                _button.onClick.AddListener(ButtonCall);
            }
            else if(null != _toggle)
            {
                _toggle.onValueChanged.AddListener(ToggleCall);
            }

			// 若是BGM
			if(IsBgm) {
				StartCoroutine(PlayAsync());
			}
		}

		/// <summary>
		/// 重置SE信息
		/// </summary>
		/// <param name="iDivision">区分</param>
		/// <param name="iAssetKey">Asset Key</param>
		public void ResetSeInfo(string iDivision, string iAssetKey) {
			division = iDivision;
			assetKey = iAssetKey;

			if(IsBgm) {
				loop = true;
			}

			// 初始化SE信息
			InitSeInfo();
		}

		public void ToggleCall(bool iSSelected)
		{
            if (false == _init)
            {
                _init = true;
                if (_firstInitValue == iSSelected) {
					return;
				}
            }

			// if button is disabled, no
			if (!_toggle.IsInteractable() || !iSSelected ) {
				return;
			}
			
			// 播放音效
			if (string.IsNullOrEmpty(assetKey)) return;
			if(0.0f >= delay) {
				Play();
			} else {
				StartCoroutine(PlayAsync());
			}
		}

		public void ButtonCall()
		{
			// if button is disabled, no
			if (!_button.IsInteractable()) {
				return;
			}
				
			// 播放音效
			if (string.IsNullOrEmpty(assetKey)) return;
			if(0.0f >= delay) {
				Play();
			} else {
				StartCoroutine(PlayAsync());
			}
		}

		/// <summary>
		/// 异步播放音效
		/// </summary>
		public IEnumerator PlayAsync() {
			yield return new WaitForSeconds(delay);

			// 等待SEManager准备完毕
			yield return new WaitUntil(() => SeManagerBase.Instance.readyForPlay);

			Play();
			yield return null;
		}

		/// <summary>
		/// 播放SE
		/// </summary>
		public abstract void Play();

		/// <summary>
		/// 取得音效路径
		/// </summary>
		/// <returns>音效路径</returns>
		protected abstract string GetSoundPath();
	}
}
