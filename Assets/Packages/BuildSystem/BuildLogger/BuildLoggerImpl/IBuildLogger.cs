namespace BuildSystem.BuildLoggerImpl
{
	/// <summary>
	/// Log类型.
	/// </summary>
	public enum TLogType {
		kInvalid = -1,
		/// <summary>
		/// 信息.
		/// </summary>
		kInfo,
		/// <summary>
		/// 警告.
		/// </summary>
		kWarning,
		/// <summary>
		/// 错误.
		/// </summary>
		kError,
		/// <summary>
		/// 异常.
		/// </summary>
		kException,
		kMax
	}

	public interface IBuildLogger
	{		
		void OpenBlock(string blockName);
		void CloseBlock(string blockName);
		void LogMessage(string message);
		void LogWarning(string message);
		void LogError(string message);
		void LogException(string message);
	}
} //namespace UnityBuildSystem.BuildLoggerImpl