using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Verse
{
	public static class LongEventHandler
	{
		private class QueuedLongEvent
		{
			public Action eventAction;

			public IEnumerator eventActionEnumerator;

			public string levelToLoad;

			public string eventTextKey = string.Empty;

			public string eventText = string.Empty;

			public bool doAsynchronously;

			public Action<Exception> exceptionHandler;

			public bool alreadyDisplayed;

			public bool UseAnimatedDots
			{
				get
				{
					return this.doAsynchronously || this.eventActionEnumerator != null;
				}
			}

			public bool ShouldWaitUntilDisplayed
			{
				get
				{
					return !this.doAsynchronously && !this.alreadyDisplayed && this.eventActionEnumerator == null && !this.eventText.NullOrEmpty();
				}
			}

			public bool UseStandardWindow
			{
				get
				{
					return !LongEventHandler.currentEvent.doAsynchronously && LongEventHandler.currentEvent.eventActionEnumerator == null;
				}
			}
		}

		private static Queue<QueuedLongEvent> eventQueue = new Queue<QueuedLongEvent>();

		private static QueuedLongEvent currentEvent = null;

		private static Thread eventThread = null;

		private static AsyncOperation levelLoadOp = null;

		private static List<Action> toExecuteWhenFinished = new List<Action>();

		private static bool executingToExecuteWhenFinished = false;

		private static readonly object CurrentEventTextLock = new object();

		private static readonly Vector2 GUIRectSize = new Vector2(240f, 75f);

		public static bool ShouldWaitForEvent
		{
			get
			{
				if (!LongEventHandler.AnyEventNowOrWaiting)
				{
					return false;
				}
				if (LongEventHandler.currentEvent != null && !LongEventHandler.currentEvent.ShouldWaitUntilDisplayed)
				{
					return true;
				}
				if (LongEventHandler.currentEvent == null && LongEventHandler.eventQueue.Any())
				{
					QueuedLongEvent queuedLongEvent = LongEventHandler.eventQueue.Peek();
					if (queuedLongEvent.doAsynchronously)
					{
						return true;
					}
					if (!queuedLongEvent.ShouldWaitUntilDisplayed)
					{
						return true;
					}
				}
				if (Find.UIRoot != null && Find.WindowStack != null)
				{
					return false;
				}
				return true;
			}
		}

		public static bool AnyEventNowOrWaiting
		{
			get
			{
				return LongEventHandler.currentEvent != null || LongEventHandler.eventQueue.Count > 0;
			}
		}

		public static bool ForcePause
		{
			get
			{
				return LongEventHandler.AnyEventNowOrWaiting;
			}
		}

		public static void QueueLongEvent(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = action;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			LongEventHandler.eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(IEnumerable action, string textKey, Action<Exception> exceptionHandler = null)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventActionEnumerator = action.GetEnumerator();
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = false;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			LongEventHandler.eventQueue.Enqueue(queuedLongEvent);
		}

		public static void QueueLongEvent(Action preLoadLevelAction, string levelToLoad, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler)
		{
			QueuedLongEvent queuedLongEvent = new QueuedLongEvent();
			queuedLongEvent.eventAction = preLoadLevelAction;
			queuedLongEvent.levelToLoad = levelToLoad;
			queuedLongEvent.eventTextKey = textKey;
			queuedLongEvent.doAsynchronously = doAsynchronously;
			queuedLongEvent.exceptionHandler = exceptionHandler;
			LongEventHandler.eventQueue.Enqueue(queuedLongEvent);
		}

		public static void ClearQueuedEvents()
		{
			LongEventHandler.eventQueue.Clear();
		}

		public static void LongEventsOnGUI()
		{
			if (LongEventHandler.currentEvent != null)
			{
				Vector2 gUIRectSize = LongEventHandler.GUIRectSize;
				float num = gUIRectSize.x;
				object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
				Monitor.Enter(currentEventTextLock);
				try
				{
					Text.Font = GameFont.Small;
					float a = num;
					Vector2 vector = Text.CalcSize(LongEventHandler.currentEvent.eventText + "...");
					num = Mathf.Max(a, (float)(vector.x + 40.0));
				}
				finally
				{
					Monitor.Exit(currentEventTextLock);
				}
				double x = ((float)UI.screenWidth - num) / 2.0;
				float num2 = (float)UI.screenHeight;
				Vector2 gUIRectSize2 = LongEventHandler.GUIRectSize;
				double y = (num2 - gUIRectSize2.y) / 2.0;
				float width = num;
				Vector2 gUIRectSize3 = LongEventHandler.GUIRectSize;
				Rect rect = new Rect((float)x, (float)y, width, gUIRectSize3.y);
				rect = rect.Rounded();
				if (!LongEventHandler.currentEvent.UseStandardWindow || Find.UIRoot == null || Find.WindowStack == null)
				{
					if (UIMenuBackgroundManager.background == null)
					{
						UIMenuBackgroundManager.background = new UI_BackgroundMain();
					}
					UIMenuBackgroundManager.background.BackgroundOnGUI();
					Widgets.DrawShadowAround(rect);
					Widgets.DrawWindowBackground(rect);
					LongEventHandler.DrawLongEventWindowContents(rect);
				}
				else
				{
					Find.WindowStack.ImmediateWindow(62893994, rect, WindowLayer.Super, (Action)delegate
					{
						LongEventHandler.DrawLongEventWindowContents(rect.AtZero());
					}, true, false, 1f);
				}
			}
		}

		public static void LongEventsUpdate(out bool sceneChanged)
		{
			sceneChanged = false;
			if (LongEventHandler.currentEvent != null)
			{
				if (LongEventHandler.currentEvent.eventActionEnumerator != null)
				{
					LongEventHandler.UpdateCurrentEnumeratorEvent();
				}
				else if (LongEventHandler.currentEvent.doAsynchronously)
				{
					LongEventHandler.UpdateCurrentAsynchronousEvent();
				}
				else
				{
					LongEventHandler.UpdateCurrentSynchronousEvent(out sceneChanged);
				}
			}
			if (LongEventHandler.currentEvent == null && LongEventHandler.eventQueue.Count > 0)
			{
				LongEventHandler.currentEvent = LongEventHandler.eventQueue.Dequeue();
				if (LongEventHandler.currentEvent.eventTextKey == null)
				{
					LongEventHandler.currentEvent.eventText = string.Empty;
				}
				else
				{
					LongEventHandler.currentEvent.eventText = LongEventHandler.currentEvent.eventTextKey.Translate();
				}
			}
		}

		public static void ExecuteWhenFinished(Action action)
		{
			LongEventHandler.toExecuteWhenFinished.Add(action);
			if (LongEventHandler.currentEvent != null && !LongEventHandler.currentEvent.ShouldWaitUntilDisplayed)
				return;
			if (!LongEventHandler.executingToExecuteWhenFinished)
			{
				LongEventHandler.ExecuteToExecuteWhenFinished();
			}
		}

		public static void SetCurrentEventText(string newText)
		{
			object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
			Monitor.Enter(currentEventTextLock);
			try
			{
				if (LongEventHandler.currentEvent != null)
				{
					LongEventHandler.currentEvent.eventText = newText;
				}
			}
			finally
			{
				Monitor.Exit(currentEventTextLock);
			}
		}

		private static void UpdateCurrentEnumeratorEvent()
		{
			try
			{
				float num = (float)(Time.realtimeSinceStartup + 0.10000000149011612);
				while (LongEventHandler.currentEvent.eventActionEnumerator.MoveNext())
				{
					if (num <= Time.realtimeSinceStartup)
						return;
				}
				IDisposable disposable = LongEventHandler.currentEvent.eventActionEnumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
				LongEventHandler.currentEvent = null;
				LongEventHandler.eventThread = null;
				LongEventHandler.levelLoadOp = null;
				LongEventHandler.ExecuteToExecuteWhenFinished();
			}
			catch (Exception ex)
			{
				Log.Error("Exception from long event: " + ex);
				if (LongEventHandler.currentEvent != null)
				{
					IDisposable disposable2 = LongEventHandler.currentEvent.eventActionEnumerator as IDisposable;
					if (disposable2 != null)
					{
						disposable2.Dispose();
					}
					if ((object)LongEventHandler.currentEvent.exceptionHandler != null)
					{
						LongEventHandler.currentEvent.exceptionHandler(ex);
					}
				}
				LongEventHandler.currentEvent = null;
				LongEventHandler.eventThread = null;
				LongEventHandler.levelLoadOp = null;
			}
		}

		private static void UpdateCurrentAsynchronousEvent()
		{
			if (LongEventHandler.eventThread == null)
			{
				LongEventHandler.eventThread = new Thread((ThreadStart)delegate
				{
					LongEventHandler.RunEventFromAnotherThread(LongEventHandler.currentEvent.eventAction);
				});
				LongEventHandler.eventThread.Start();
			}
			else if (!LongEventHandler.eventThread.IsAlive)
			{
				bool flag = false;
				if (!LongEventHandler.currentEvent.levelToLoad.NullOrEmpty())
				{
					if (LongEventHandler.levelLoadOp == null)
					{
						LongEventHandler.levelLoadOp = SceneManager.LoadSceneAsync(LongEventHandler.currentEvent.levelToLoad);
					}
					else if (LongEventHandler.levelLoadOp.isDone)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					LongEventHandler.currentEvent = null;
					LongEventHandler.eventThread = null;
					LongEventHandler.levelLoadOp = null;
					LongEventHandler.ExecuteToExecuteWhenFinished();
				}
			}
		}

		private static void UpdateCurrentSynchronousEvent(out bool sceneChanged)
		{
			sceneChanged = false;
			if (!LongEventHandler.currentEvent.ShouldWaitUntilDisplayed)
			{
				try
				{
					if ((object)LongEventHandler.currentEvent.eventAction != null)
					{
						LongEventHandler.currentEvent.eventAction();
					}
					if (!LongEventHandler.currentEvent.levelToLoad.NullOrEmpty())
					{
						SceneManager.LoadScene(LongEventHandler.currentEvent.levelToLoad);
						sceneChanged = true;
					}
					LongEventHandler.currentEvent = null;
					LongEventHandler.eventThread = null;
					LongEventHandler.levelLoadOp = null;
					LongEventHandler.ExecuteToExecuteWhenFinished();
				}
				catch (Exception ex)
				{
					Log.Error("Exception from long event: " + ex);
					if (LongEventHandler.currentEvent != null && (object)LongEventHandler.currentEvent.exceptionHandler != null)
					{
						LongEventHandler.currentEvent.exceptionHandler(ex);
					}
					LongEventHandler.currentEvent = null;
					LongEventHandler.eventThread = null;
					LongEventHandler.levelLoadOp = null;
				}
			}
		}

		private static void RunEventFromAnotherThread(Action action)
		{
			try
			{
				if ((object)action != null)
				{
					action();
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception from asynchronous event: " + ex);
				try
				{
					if (LongEventHandler.currentEvent != null && (object)LongEventHandler.currentEvent.exceptionHandler != null)
					{
						LongEventHandler.currentEvent.exceptionHandler(ex);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Exception was thrown while trying to handle exception. Exception: " + arg);
				}
			}
		}

		private static void ExecuteToExecuteWhenFinished()
		{
			if (LongEventHandler.executingToExecuteWhenFinished)
			{
				Log.Warning("Already executing.");
			}
			else
			{
				LongEventHandler.executingToExecuteWhenFinished = true;
				for (int i = 0; i < LongEventHandler.toExecuteWhenFinished.Count; i++)
				{
					try
					{
						LongEventHandler.toExecuteWhenFinished[i]();
					}
					catch (Exception arg)
					{
						Log.Error("Could not execute post-long-event action. Exception: " + arg);
					}
				}
				LongEventHandler.toExecuteWhenFinished.Clear();
				LongEventHandler.executingToExecuteWhenFinished = false;
			}
		}

		private static void DrawLongEventWindowContents(Rect rect)
		{
			if (LongEventHandler.currentEvent != null)
			{
				if (Event.current.type == EventType.Repaint)
				{
					LongEventHandler.currentEvent.alreadyDisplayed = true;
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				float num = 0f;
				if (LongEventHandler.levelLoadOp != null)
				{
					float f = 1f;
					if (!LongEventHandler.levelLoadOp.isDone)
					{
						f = LongEventHandler.levelLoadOp.progress;
					}
					string text = "LoadingAssets".Translate() + " " + f.ToStringPercent();
					Vector2 vector = Text.CalcSize(text);
					num = vector.x;
					Widgets.Label(rect, text);
				}
				else
				{
					object currentEventTextLock = LongEventHandler.CurrentEventTextLock;
					Monitor.Enter(currentEventTextLock);
					try
					{
						Vector2 vector2 = Text.CalcSize(LongEventHandler.currentEvent.eventText);
						num = vector2.x;
						Widgets.Label(rect, LongEventHandler.currentEvent.eventText);
					}
					finally
					{
						Monitor.Exit(currentEventTextLock);
					}
				}
				Text.Anchor = TextAnchor.MiddleLeft;
				Vector2 center = rect.center;
				rect.xMin = (float)(center.x + num / 2.0);
				Widgets.Label(rect, LongEventHandler.currentEvent.UseAnimatedDots ? GenText.MarchingEllipsis(0f) : "...");
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}
}
