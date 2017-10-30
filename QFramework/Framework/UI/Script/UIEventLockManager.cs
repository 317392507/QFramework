/****************************************************************************
 * Copyright (c) 2017 liuzhenhua@putao.com
 * Copyright (c) 2017 liangxie
 * 
 * http://liangxiegame.com
 * https://github.com/liangxiegame/QFramework
 * https://github.com/liangxiegame/QSingleton
 * https://github.com/liangxiegame/QChain
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/

namespace QFramework
{
	using UnityEngine;
	using System;
	using Core;

	public enum UIFilterEvent
	{
		Began = QMgrID.UIFilter,
		DelayLock,
		Lock,
		UnLock,
		LockObjEvent,
		UnlockObjEvent,
		Ended
	}

	/// <summary>
	/// 互斥事件
	/// </summary>
	public class UILockOnClickEventMsg : QMsg
	{
		public ushort DstEventId;
		public float LockTime = 0.2f;

		public QBoolDelegate.WithVoid Validate = delegate
		{
			return true;
		};

		public UILockOnClickEventMsg(float lockTime) : base((ushort) UIFilterEvent.DelayLock)
		{
			LockTime = lockTime;
		}
	}

	public class LockMsg : QMsg
	{
		public LockMsg() : base((ushort) UIFilterEvent.Lock)
		{
		}
	}

	public class UnLockMsg : QMsg
	{
		public UnLockMsg() : base((ushort) UIFilterEvent.UnLock)
		{
		}
	}

	public class UILockObjEventMsg : QMsg
	{
		public GameObject LockedObj;

		public UILockObjEventMsg(GameObject lockedObj) : base((ushort) UIFilterEvent.LockObjEvent)
		{
			LockedObj = lockedObj;
		}
	}

	public class UIUnlockObjEventMsg : QMsg
	{
		public GameObject UnlockedObj;

		public UIUnlockObjEventMsg(GameObject unlockedObj) : base((ushort) UIFilterEvent.UnlockObjEvent)
		{
			this.UnlockedObj = unlockedObj;
		}
	}

	[QMonoSingletonAttribute("[Event]/UIEventLockManager")]
	public class UIEventLockManager : QMgrBehaviour, ISingleton
	{
		protected override void SetupMgrId()
		{
			mMgrId = QMgrID.UIFilter;
		}

		public static UIEventLockManager Instance
		{
			get { return QMonoSingletonProperty<UIEventLockManager>.Instance; }
		}

		public void OnSingletonInit()
		{
		}

		public void Dispose()
		{
		}

		public bool LockBtnOnClick { get; private set; }
		public bool Lock { protected get; set; }
		public GameObject LockedObj { get;protected set; }

		void Awake()
		{
			RegisterEvents(new[]
			{
				(ushort) UIFilterEvent.DelayLock,
				(ushort) UIFilterEvent.UnLock,
				(ushort) UIFilterEvent.Lock
			});
			LockBtnOnClick = false;
			Lock = false;
		}

		protected override void ProcessMsg(int key, QMsg msg)
		{
			Log.i("{0}",msg.EventID);
			switch (key)
			{
				case (ushort) UIFilterEvent.DelayLock:
					Log.i("receive");
					UILockOnClickEventMsg lockOnClickEventMsg = msg as UILockOnClickEventMsg;
					LockBtnOnClick = true;
					DelayNode delayNode = new DelayNode(lockOnClickEventMsg.LockTime)
					{
						OnEndedCallback = delegate
						{
							LockBtnOnClick = false;
						}
					};
					StartCoroutine(delayNode.Execute());
					break;
				case (ushort) UIFilterEvent.Lock:
					Log.i("Lock");
					Lock = true;
					break;
				case (ushort) UIFilterEvent.UnLock:
					Log.i("UnLock");
					Lock = false;
					break;

				case (int) UIFilterEvent.LockObjEvent:
				{
					UILockObjEventMsg lockObjEventMsg = msg as UILockObjEventMsg;
					if (null == LockedObj)
					{
						LockedObj = lockObjEventMsg.LockedObj;
					}
					else if (LockedObj == lockObjEventMsg.LockedObj)
					{
						// maybe two finger in one obj
						Log.w("error: curLockedObj is already seted");
					}
					else if (LockedObj != lockObjEventMsg.LockedObj)
					{
						throw new Exception("error: pre obj need unlocked");
					}
				}
					break;
				case (int) UIFilterEvent.UnlockObjEvent:
				{
					UIUnlockObjEventMsg unlockObjEventMsg = msg as UIUnlockObjEventMsg;
					if (unlockObjEventMsg.UnlockedObj == LockedObj)
					{
						unlockObjEventMsg.UnlockedObj = null;
						LockedObj = null;
					}
					else if (LockedObj == null)
					{
						Log.w ("error: curLockedObj is already unlocked");
					}
					else if (LockedObj != unlockObjEventMsg.UnlockedObj)
					{
						throw new Exception("error: pre obj need unlocked");
					}
				}
					break;
			}
		}
	}
}