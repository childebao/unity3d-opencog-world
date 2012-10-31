using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class Logger
{
	/** Enumerator of the log level. */
	public enum Level {NONE, ERROR, WARN, INFO, DEBUG, FINE};

    private static Logger instance = null;

    public static Logger getInstance()
    {
        if (instance == null)
            instance = new Logger();
        return instance;
    }

	public Logger() {
		Level level = Level.NONE;
		try {
			level = (Level)Enum.Parse(typeof(Level), Config.getInstance().get("LOG_LEVEL"));
		} catch (ArgumentException ae) {
			UnityEngine.Debug.LogError("Logger: Failed to construct [" + ae.Message + "]");
		}
		SetLevel(level);
		
		if(level > Level.NONE) {
			isLogEnabled = true;
		} else {
			isLogEnabled = false;
		}
	}
	
	public void Error(System.Object logInfoObj)
    {
		Log(Level.ERROR, logInfoObj, true);
	}
	
	public void Warn(System.Object logInfoObj)
    {
		Log(Level.WARN, logInfoObj);	
	}
	
	public void Info(System.Object logInfoObj)
    {
		Log(Level.INFO, logInfoObj);
	}
	
	/** Avoid using Debug as the function name. */
	public void Debugging(System.Object logInfoObj)
    {
		Log(Level.DEBUG, logInfoObj, true);	
	}
	
	public void Fine(System.Object logInfoObj)
    {
		Log(Level.FINE, logInfoObj);
	}
	
	public void Log(Level level, System.Object logInfoObj, bool showTrace=false)
    {
		if (!isLogEnabled) 
			return;

        string logToPrint;

        if (showTrace)
        {
            StackTrace trace = new StackTrace();
            StackFrame frame = null;
            MethodBase method = null;

            frame = trace.GetFrame(2);
            method = frame.GetMethod();

            string callingMethod = method.ReflectedType.Name + "::" + method.Name;
            logToPrint = "[" + level.ToString() + "] " +
                                callingMethod + ": " + logInfoObj.ToString();
        }
        else
        {
            logToPrint = "[" + level.ToString() + "] " + logInfoObj.ToString();
        }
		if (level <= currentLevel) {
			/** 
			 * Use unity api for writing information to 
			 * unity editor console. 
			 */
			switch (level) {
			case Level.FINE:
			case Level.DEBUG:
			case Level.INFO:
				{
					UnityEngine.Debug.Log(logToPrint);
					break;
				}	
			case Level.WARN:
				{
					UnityEngine.Debug.LogWarning(logToPrint);
					break;
				}
			case Level.ERROR:
				{
					UnityEngine.Debug.LogError(logToPrint);
					break;
				}
			}
		}
	}
	
	public void SetLevel(Level level) {
		currentLevel = level;
	}
	
	public bool isEnabled() {
		return isLogEnabled;
	}
	
	private Level currentLevel;
	
	private bool isLogEnabled;
}

