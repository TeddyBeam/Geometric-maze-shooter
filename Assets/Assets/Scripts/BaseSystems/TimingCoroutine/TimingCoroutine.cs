using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using Extension.Attributes;
using UnityEngine.Profiling;

namespace BaseSystems.Timing
{
    public class TimingCoroutine : MonoBehaviour
    {
        #region Init
        public enum DebugInfoType
        {
            None = 0,
            SeperateCoroutines,
            SeperateTags
        }

        /// <summary>
        /// The time between calls to SlowUpdate.
        /// </summary>
        [Comment("How quickly the SlowUpdate segment ticks.")]
        public float TimeBetweenSlowUpdateCalls = 1f / 7f;
        /// <summary>
        /// The amount that each coroutine should be seperated inside the Unity profiler. NOTE: When the profiler window
        /// is not open this value is ignored and all coroutines behave as if "None" is selected.
        /// </summary>
        [Comment("How much data should be sent to the profiler window when it's open.")]
        public DebugInfoType ProfilerDebugAmount = DebugInfoType.SeperateCoroutines;
        /// <summary>
        /// Whether the manual timeframe should automatically trigger during the update segment.
        /// </summary>
        [Comment("When using manual timeframe, should it run automatically after the update loop or only when TriggerManualTimframeUpdate is called.")]
        public bool AutoTriggerManualTimeframe = true;
        /// <summary>
        /// The number of coroutines that are being run in the Update segment.
        /// </summary>
        [Comment("A count of the number of Update coroutines that are currently running."), Space(12)]
        public int UpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the FixedUpdate segment.
        /// </summary>
        [Comment("A count of the number of FixedUpdate coroutines that are currently running.")]
        public int FixedUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the LateUpdate segment.
        /// </summary>
        [Comment("A count of the number of LateUpdate coroutines that are currently running.")]
        public int LateUpdateCoroutines;
        /// <summary>
        /// The number of coroutines that are being run in the SlowUpdate segment.
        /// </summary>
        [Comment("A count of the number of SlowUpdate coroutines that are currently running.")]
        public int SlowUpdateCoroutines;
        /// <summary>
        /// The time in seconds that the current segment has been running.
        /// </summary>
        [HideInInspector]
        public double localTime;
        /// <summary>
        /// The time in seconds that the current segment has been running.
        /// </summary>
        public static float LocalTime { get { return (float)Instance.localTime; } }
        /// <summary>
        /// The amount of time in fractional seconds that elapsed between this frame and the last frame.
        /// </summary>
        [HideInInspector]
        public float deltaTime;
        /// <summary>
        /// The amount of time in fractional seconds that elapsed between this frame and the last frame.
        /// </summary>
        public static float DeltaTime { get { return Instance.deltaTime; } }
        /// <summary>
        /// When defined, all errors from inside coroutines will be passed into this function instead of falling through to the Unity console.
        /// </summary>
        public Action<Exception> OnError;
        /// <summary>
        /// Used for advanced coroutine control.
        /// </summary>
        public static Func<IEnumerator<float>, CoroutineHandle, IEnumerator<float>> ReplacementFunction;
        /// <summary>
        /// This event fires just before each segment is run.
        /// </summary>
        public static event Action OnPreExecute;
        /// <summary>
        /// You can use "yield return Timing.WaitForOneFrame;" inside a coroutine function to go to the next frame. 
        /// This is equalivant to "yeild return 0f;"
        /// </summary>
        public readonly static float WaitForOneFrame = 0f;
        /// <summary>
        /// The main thread that (almost) everything in unity runs in.
        /// </summary>
        public static Thread MainThread { get; private set; }

        private bool runningUpdate;
        private bool runningLateUpdate;
        private bool runningFixedUpdate;
        private bool runningSlowUpdate;
        private int currentUpdateFrame;
        private int currentFixedUpdateFrame;
        private int currentSlowUpdateFrame;
        private int nextUpdateProcessSlot;
        private int nextLateUpdateProcessSlot;
        private int nextFixedUpdateProcessSlot;
        private int nextSlowUpdateProcessSlot;
        private double lastUpdateTime;
        private double lastFixedUpdateTime;
        private double lastSlowUpdateTime;
        private float lastSlowUpdateDeltaTime;
        private ushort framesSinceUpdate;
        private ushort expansions = 1;
        private byte instanceID;

        private readonly Dictionary<CoroutineHandle, HashSet<ProcessData>> waitingTriggers = new Dictionary<CoroutineHandle, HashSet<ProcessData>>();
        private readonly Queue<Exception> _exceptions = new Queue<Exception>();
        private readonly Dictionary<CoroutineHandle, ProcessIndex> handleToIndex = new Dictionary<CoroutineHandle, ProcessIndex>();
        private readonly Dictionary<ProcessIndex, CoroutineHandle> indexToHandle = new Dictionary<ProcessIndex, CoroutineHandle>();
        private readonly Dictionary<ProcessIndex, string> processTags = new Dictionary<ProcessIndex, string>();
        private readonly Dictionary<string, HashSet<ProcessIndex>> taggedProcesses = new Dictionary<string, HashSet<ProcessIndex>>();

        private IEnumerator<float>[] UpdateProcesses = new IEnumerator<float>[InitialBufferSizeLarge];
        private IEnumerator<float>[] LateUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
        private IEnumerator<float>[] FixedUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
        private IEnumerator<float>[] SlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
        private bool[] UpdatePaused = new bool[InitialBufferSizeLarge];
        private bool[] LateUpdatePaused = new bool[InitialBufferSizeSmall];
        private bool[] FixedUpdatePaused = new bool[InitialBufferSizeMedium];
        private bool[] SlowUpdatePaused = new bool[InitialBufferSizeMedium];

        private const ushort FramesUntilMaintenance = 64;
        private const int ProcessArrayChunkSize = 64;
        private const int InitialBufferSizeLarge = 256;
        private const int InitialBufferSizeMedium = 64;
        private const int InitialBufferSizeSmall = 8;

        private static readonly Dictionary<byte, TimingCoroutine> ActiveInstances = new Dictionary<byte, TimingCoroutine>();
        private static TimingCoroutine instance;

        public static TimingCoroutine Instance
        {
            get
            {
                if (instance == null || !instance.gameObject)
                {
                    GameObject instanceHome = GameObject.FindGameObjectWithTag("TimingCoroutine");
                    Type movementType = Type.GetType("MovementEffects.Movement, MovementOverTime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

                    if (instanceHome == null)
                    {
                        instanceHome = new GameObject { tag = "TimingCoroutine", name = "TimingCoroutineSystem" };
                        DontDestroyOnLoad(instanceHome);

                        if (movementType != null)
                            instanceHome.AddComponent(movementType);

                        instance = instanceHome.AddComponent<TimingCoroutine>();
                    }
                    else
                    {
                        if (movementType != null && instanceHome.GetComponent(movementType) == null)
                            instanceHome.AddComponent(movementType);

                        instance = instanceHome.GetComponent<TimingCoroutine>() ?? instanceHome.AddComponent<TimingCoroutine>();
                    }
                }

                return instance;
            }

            set { instance = value; }
        }
        #endregion

        #region Monobehaviours
        protected virtual void Awake()
        {
            if (instance == null)
                instance = this;
            else
                deltaTime = instance.deltaTime;

            instanceID = 0x01;
            while (ActiveInstances.ContainsKey(instanceID))
                instanceID++;

            if (instanceID == 0x20)
            {
                GameObject.Destroy(gameObject);
                throw new OverflowException("Only 31 instances of MEC are allowed at one time.");
            }

            ActiveInstances.Add(instanceID, this);

            if (MainThread == null)
                MainThread = Thread.CurrentThread;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;

            ActiveInstances.Remove(instanceID);
        }

        protected virtual void Update()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (lastSlowUpdateTime + TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup && nextSlowUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.SlowUpdate };
                runningSlowUpdate = true;
                UpdateTimeValues(coindex.seg);

                for (coindex.i = 0; coindex.i < nextSlowUpdateProcessSlot; coindex.i++)
                {
                    if (!SlowUpdatePaused[coindex.i] && SlowUpdateProcesses[coindex.i] != null && !(localTime < SlowUpdateProcesses[coindex.i].Current))
                    {
                        if (ProfilerDebugAmount != DebugInfoType.None)
                        {
                            Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags
                                                     ? ("Processing Coroutine (Slow Update)" +
                                                        (processTags.ContainsKey(coindex) ? ", tag " + processTags[coindex] : ", no tag"))
                                                     : "Processing Coroutine (Slow Update)");
                        }

                        try
                        {
                            if (!SlowUpdateProcesses[coindex.i].MoveNext())
                            {
                                SlowUpdateProcesses[coindex.i] = null;
                            }
                            else if (SlowUpdateProcesses[coindex.i] != null && float.IsNaN(SlowUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    SlowUpdateProcesses[coindex.i] = null;
                                }
                                else
                                {
                                    SlowUpdateProcesses[coindex.i] = ReplacementFunction(SlowUpdateProcesses[coindex.i], indexToHandle[coindex]);

                                    ReplacementFunction = null;
                                    coindex.i--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            SlowUpdateProcesses[coindex.i] = null;
                        }

                        if (ProfilerDebugAmount != DebugInfoType.None)
                            Profiler.EndSample();
                    }
                }

                runningSlowUpdate = false;
            }

            if (nextUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.Update };
                runningUpdate = true;
                UpdateTimeValues(coindex.seg);

                for (coindex.i = 0; coindex.i < nextUpdateProcessSlot; coindex.i++)
                {
                    if (!UpdatePaused[coindex.i] && UpdateProcesses[coindex.i] != null && !(localTime < UpdateProcesses[coindex.i].Current))
                    {
                        if (ProfilerDebugAmount != DebugInfoType.None)
                        {
                            Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags
                                                     ? ("Processing Coroutine" +
                                                        (processTags.ContainsKey(coindex) ? ", tag " + processTags[coindex] : ", no tag"))
                                                     : "Processing Coroutine");
                        }

                        try
                        {
                            if (!UpdateProcesses[coindex.i].MoveNext())
                            {
                                UpdateProcesses[coindex.i] = null;
                            }
                            else if (UpdateProcesses[coindex.i] != null && float.IsNaN(UpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    UpdateProcesses[coindex.i] = null;
                                }
                                else
                                {
                                    UpdateProcesses[coindex.i] = ReplacementFunction(UpdateProcesses[coindex.i], indexToHandle[coindex]);

                                    ReplacementFunction = null;
                                    coindex.i--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            UpdateProcesses[coindex.i] = null;
                        }

                        if (ProfilerDebugAmount != DebugInfoType.None)
                            Profiler.EndSample();
                    }
                }
                runningUpdate = false;
            }

            if (++framesSinceUpdate > FramesUntilMaintenance)
            {
                framesSinceUpdate = 0;

                if (ProfilerDebugAmount != DebugInfoType.None)
                    Profiler.BeginSample("Maintenance Task");

                RemoveUnused();

                if (ProfilerDebugAmount != DebugInfoType.None)
                    Profiler.EndSample();
            }

            if (_exceptions.Count > 0)
                throw _exceptions.Dequeue();
        }

        protected virtual void FixedUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (nextFixedUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.FixedUpdate };
                runningFixedUpdate = true;
                UpdateTimeValues(coindex.seg);

                for (coindex.i = 0; coindex.i < nextFixedUpdateProcessSlot; coindex.i++)
                {
                    if (!FixedUpdatePaused[coindex.i] && FixedUpdateProcesses[coindex.i] != null && !(localTime < FixedUpdateProcesses[coindex.i].Current))
                    {
                        if (ProfilerDebugAmount != DebugInfoType.None)
                        {
                            Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags
                                                     ? ("Processing Coroutine" +
                                                        (processTags.ContainsKey(coindex) ? ", tag " + processTags[coindex] : ", no tag"))
                                                     : "Processing Coroutine");
                        }

                        try
                        {
                            if (!FixedUpdateProcesses[coindex.i].MoveNext())
                            {
                                FixedUpdateProcesses[coindex.i] = null;
                            }
                            else if (FixedUpdateProcesses[coindex.i] != null && float.IsNaN(FixedUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    FixedUpdateProcesses[coindex.i] = null;
                                }
                                else
                                {
                                    FixedUpdateProcesses[coindex.i] = ReplacementFunction(FixedUpdateProcesses[coindex.i], indexToHandle[coindex]);

                                    ReplacementFunction = null;
                                    coindex.i--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            FixedUpdateProcesses[coindex.i] = null;
                        }

                        if (ProfilerDebugAmount != DebugInfoType.None)
                            Profiler.EndSample();
                    }
                }

                runningFixedUpdate = false;
            }

            if (_exceptions.Count > 0)
                throw _exceptions.Dequeue();
        }

        protected virtual void LateUpdate()
        {
            if (OnPreExecute != null)
                OnPreExecute();

            if (nextLateUpdateProcessSlot > 0)
            {
                ProcessIndex coindex = new ProcessIndex { seg = Segment.LateUpdate };
                runningLateUpdate = true;
                UpdateTimeValues(coindex.seg);

                for (coindex.i = 0; coindex.i < nextLateUpdateProcessSlot; coindex.i++)
                {
                    if (!LateUpdatePaused[coindex.i] && LateUpdateProcesses[coindex.i] != null && !(localTime < LateUpdateProcesses[coindex.i].Current))
                    {
                        if (ProfilerDebugAmount != DebugInfoType.None)
                        {
                            Profiler.BeginSample(ProfilerDebugAmount == DebugInfoType.SeperateTags
                                                     ? ("Processing Coroutine" +
                                                        (processTags.ContainsKey(coindex) ? ", tag " + processTags[coindex] : ", no tag"))
                                                     : "Processing Coroutine");
                        }

                        try
                        {
                            if (!LateUpdateProcesses[coindex.i].MoveNext())
                            {
                                LateUpdateProcesses[coindex.i] = null;
                            }
                            else if (LateUpdateProcesses[coindex.i] != null && float.IsNaN(LateUpdateProcesses[coindex.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    LateUpdateProcesses[coindex.i] = null;
                                }
                                else
                                {
                                    LateUpdateProcesses[coindex.i] = ReplacementFunction(LateUpdateProcesses[coindex.i], indexToHandle[coindex]);

                                    ReplacementFunction = null;
                                    coindex.i--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            LateUpdateProcesses[coindex.i] = null;
                        }

                        if (ProfilerDebugAmount != DebugInfoType.None)
                            Profiler.EndSample();
                    }
                }

                runningLateUpdate = false;
            }

            if (_exceptions.Count > 0)
                throw _exceptions.Dequeue();
        }
        #endregion*/

        #region Work
        private void RemoveUnused()
        {
            var waitTrigsEnum = waitingTriggers.GetEnumerator();
            while (waitTrigsEnum.MoveNext())
            {
                if (waitTrigsEnum.Current.Value.Count == 0)
                {
                    waitingTriggers.Remove(waitTrigsEnum.Current.Key);
                    waitTrigsEnum = waitingTriggers.GetEnumerator();
                    continue;
                }

                if (handleToIndex.ContainsKey(waitTrigsEnum.Current.Key) && CoindexIsNull(handleToIndex[waitTrigsEnum.Current.Key]))
                {
                    CloseWaitingProcess(waitTrigsEnum.Current.Key);
                    waitTrigsEnum = waitingTriggers.GetEnumerator();
                }
            }

            ProcessIndex outer, inner;
            outer.seg = inner.seg = Segment.Update;
            for (outer.i = inner.i = 0; outer.i < nextUpdateProcessSlot; outer.i++)
            {
                if (UpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        UpdateProcesses[inner.i] = UpdateProcesses[outer.i];
                        UpdatePaused[inner.i] = UpdatePaused[outer.i];
                        MoveTag(outer, inner);

                        if (indexToHandle.ContainsKey(inner))
                        {
                            handleToIndex.Remove(indexToHandle[inner]);
                            indexToHandle.Remove(inner);
                        }

                        handleToIndex[indexToHandle[outer]] = inner;
                        indexToHandle.Add(inner, indexToHandle[outer]);
                        indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < nextUpdateProcessSlot; outer.i++)
            {
                UpdateProcesses[outer.i] = null;
                UpdatePaused[outer.i] = false;
                RemoveTag(outer);

                if (indexToHandle.ContainsKey(outer))
                {
                    handleToIndex.Remove(indexToHandle[outer]);
                    indexToHandle.Remove(outer);
                }
            }

            UpdateCoroutines = nextUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.FixedUpdate;
            for (outer.i = inner.i = 0; outer.i < nextFixedUpdateProcessSlot; outer.i++)
            {
                if (FixedUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        FixedUpdateProcesses[inner.i] = FixedUpdateProcesses[outer.i];
                        FixedUpdatePaused[inner.i] = FixedUpdatePaused[outer.i];
                        MoveTag(outer, inner);

                        if (indexToHandle.ContainsKey(inner))
                        {
                            handleToIndex.Remove(indexToHandle[inner]);
                            indexToHandle.Remove(inner);
                        }

                        handleToIndex[indexToHandle[outer]] = inner;
                        indexToHandle.Add(inner, indexToHandle[outer]);
                        indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < nextFixedUpdateProcessSlot; outer.i++)
            {
                FixedUpdateProcesses[outer.i] = null;
                FixedUpdatePaused[outer.i] = false;
                RemoveTag(outer);

                if (indexToHandle.ContainsKey(outer))
                {
                    handleToIndex.Remove(indexToHandle[outer]);
                    indexToHandle.Remove(outer);
                }
            }

            FixedUpdateCoroutines = nextFixedUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.LateUpdate;
            for (outer.i = inner.i = 0; outer.i < nextLateUpdateProcessSlot; outer.i++)
            {
                if (LateUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        LateUpdateProcesses[inner.i] = LateUpdateProcesses[outer.i];
                        LateUpdatePaused[inner.i] = LateUpdatePaused[outer.i];
                        MoveTag(outer, inner);

                        if (indexToHandle.ContainsKey(inner))
                        {
                            handleToIndex.Remove(indexToHandle[inner]);
                            indexToHandle.Remove(inner);
                        }

                        handleToIndex[indexToHandle[outer]] = inner;
                        indexToHandle.Add(inner, indexToHandle[outer]);
                        indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < nextLateUpdateProcessSlot; outer.i++)
            {
                LateUpdateProcesses[outer.i] = null;
                LateUpdatePaused[outer.i] = false;
                RemoveTag(outer);

                if (indexToHandle.ContainsKey(outer))
                {
                    handleToIndex.Remove(indexToHandle[outer]);
                    indexToHandle.Remove(outer);
                }
            }

            LateUpdateCoroutines = nextLateUpdateProcessSlot = inner.i;

            outer.seg = inner.seg = Segment.SlowUpdate;
            for (outer.i = inner.i = 0; outer.i < nextSlowUpdateProcessSlot; outer.i++)
            {
                if (SlowUpdateProcesses[outer.i] != null)
                {
                    if (outer.i != inner.i)
                    {
                        SlowUpdateProcesses[inner.i] = SlowUpdateProcesses[outer.i];
                        SlowUpdatePaused[inner.i] = SlowUpdatePaused[outer.i];
                        MoveTag(outer, inner);

                        if (indexToHandle.ContainsKey(inner))
                        {
                            handleToIndex.Remove(indexToHandle[inner]);
                            indexToHandle.Remove(inner);
                        }

                        handleToIndex[indexToHandle[outer]] = inner;
                        indexToHandle.Add(inner, indexToHandle[outer]);
                        indexToHandle.Remove(outer);
                    }
                    inner.i++;
                }
            }
            for (outer.i = inner.i; outer.i < nextSlowUpdateProcessSlot; outer.i++)
            {
                SlowUpdateProcesses[outer.i] = null;
                SlowUpdatePaused[outer.i] = false;
                RemoveTag(outer);

                if (indexToHandle.ContainsKey(outer))
                {
                    handleToIndex.Remove(indexToHandle[outer]);
                    indexToHandle.Remove(outer);
                }
            }

            SlowUpdateCoroutines = nextSlowUpdateProcessSlot = inner.i;
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, null, new CoroutineHandle(Instance.instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used for Kill operations.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, Segment.Update, tag, new CoroutineHandle(Instance.instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment timing)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, timing, null, new CoroutineHandle(Instance.instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">The segment that the coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used for Kill operations.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment timing, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                : Instance.RunCoroutineInternal(coroutine, timing, tag, new CoroutineHandle(Instance.instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, null, new CoroutineHandle(instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance in the Update segment.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used for Kill operations.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, Segment.Update, tag, new CoroutineHandle(instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">The segment that the coroutine should run in.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment timing)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, timing, null, new CoroutineHandle(instanceID), true);
        }

        /// <summary>
        /// Run a new coroutine on this Timing instance.
        /// </summary>
        /// <param name="coroutine">The new coroutine's handle.</param>
        /// <param name="timing">The segment that the coroutine should run in.</param>
        /// <param name="tag">An optional tag to attach to the coroutine which can later be used for Kill operations.</param>
        /// <returns>The coroutine's handle, which can be used for Wait and Kill operations.</returns>
        public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment timing, string tag)
        {
            return coroutine == null ? new CoroutineHandle()
                 : RunCoroutineInternal(coroutine, timing, tag, new CoroutineHandle(instanceID), true);
        }

        private CoroutineHandle RunCoroutineInternal(IEnumerator<float> coroutine, Segment timing, string tag, CoroutineHandle handle, bool prewarm)
        {
            ProcessIndex slot = new ProcessIndex { seg = timing };

            if (handleToIndex.ContainsKey(handle))
            {
                indexToHandle.Remove(handleToIndex[handle]);
                handleToIndex.Remove(handle);
            }

            switch (timing)
            {
                case Segment.Update:

                    if (nextUpdateProcessSlot >= UpdateProcesses.Length)
                    {
                        IEnumerator<float>[] oldProcArray = UpdateProcesses;
                        bool[] oldPausedArray = UpdatePaused;

                        UpdateProcesses = new IEnumerator<float>[UpdateProcesses.Length + (ProcessArrayChunkSize * expansions++)];
                        UpdatePaused = new bool[UpdateProcesses.Length];

                        for (int i = 0; i < oldProcArray.Length; i++)
                        {
                            UpdateProcesses[i] = oldProcArray[i];
                            UpdatePaused[i] = oldPausedArray[i];
                        }
                    }

                    slot.i = nextUpdateProcessSlot++;
                    UpdateProcesses[slot.i] = coroutine;

                    if (null != tag)
                        AddTag(tag, slot);

                    indexToHandle.Add(slot, handle);
                    handleToIndex.Add(handle, slot);

                    if (!runningUpdate && prewarm)
                    {
                        try
                        {
                            runningUpdate = true;
                            UpdateTimeValues(slot.seg);

                            if (!UpdateProcesses[slot.i].MoveNext())
                            {
                                UpdateProcesses[slot.i] = null;
                            }
                            else if (UpdateProcesses[slot.i] != null && float.IsNaN(UpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    UpdateProcesses[slot.i] = null;
                                }
                                else
                                {
                                    UpdateProcesses[slot.i] = ReplacementFunction(UpdateProcesses[slot.i], indexToHandle[slot]);

                                    ReplacementFunction = null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            UpdateProcesses[slot.i] = null;
                        }
                        finally
                        {
                            runningUpdate = false;
                        }
                    }

                    return handle;

                case Segment.FixedUpdate:

                    if (nextFixedUpdateProcessSlot >= FixedUpdateProcesses.Length)
                    {
                        IEnumerator<float>[] oldProcArray = FixedUpdateProcesses;
                        bool[] oldPausedArray = FixedUpdatePaused;

                        FixedUpdateProcesses = new IEnumerator<float>[FixedUpdateProcesses.Length + (ProcessArrayChunkSize * expansions++)];
                        FixedUpdatePaused = new bool[FixedUpdateProcesses.Length];

                        for (int i = 0; i < oldProcArray.Length; i++)
                        {
                            FixedUpdateProcesses[i] = oldProcArray[i];
                            FixedUpdatePaused[i] = oldPausedArray[i];
                        }
                    }

                    slot.i = nextFixedUpdateProcessSlot++;
                    FixedUpdateProcesses[slot.i] = coroutine;

                    if (null != tag)
                        AddTag(tag, slot);

                    indexToHandle.Add(slot, handle);
                    handleToIndex.Add(handle, slot);

                    if (!runningFixedUpdate && prewarm)
                    {
                        try
                        {
                            runningFixedUpdate = true;
                            UpdateTimeValues(slot.seg);

                            if (!FixedUpdateProcesses[slot.i].MoveNext())
                            {
                                FixedUpdateProcesses[slot.i] = null;
                            }
                            else if (FixedUpdateProcesses[slot.i] != null && float.IsNaN(FixedUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    FixedUpdateProcesses[slot.i] = null;
                                }
                                else
                                {
                                    FixedUpdateProcesses[slot.i] = ReplacementFunction(FixedUpdateProcesses[slot.i], indexToHandle[slot]);

                                    ReplacementFunction = null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            FixedUpdateProcesses[slot.i] = null;
                        }
                        finally
                        {
                            runningFixedUpdate = false;
                        }
                    }

                    return handle;

                case Segment.LateUpdate:

                    if (nextLateUpdateProcessSlot >= LateUpdateProcesses.Length)
                    {
                        IEnumerator<float>[] oldProcArray = LateUpdateProcesses;
                        bool[] oldPausedArray = LateUpdatePaused;

                        LateUpdateProcesses = new IEnumerator<float>[LateUpdateProcesses.Length + (ProcessArrayChunkSize * expansions++)];
                        LateUpdatePaused = new bool[LateUpdateProcesses.Length];

                        for (int i = 0; i < oldProcArray.Length; i++)
                        {
                            LateUpdateProcesses[i] = oldProcArray[i];
                            LateUpdatePaused[i] = oldPausedArray[i];
                        }
                    }

                    slot.i = nextLateUpdateProcessSlot++;
                    LateUpdateProcesses[slot.i] = coroutine;

                    if (tag != null)
                        AddTag(tag, slot);

                    indexToHandle.Add(slot, handle);
                    handleToIndex.Add(handle, slot);

                    if (!runningLateUpdate && prewarm)
                    {
                        try
                        {
                            runningLateUpdate = true;
                            UpdateTimeValues(slot.seg);

                            if (!LateUpdateProcesses[slot.i].MoveNext())
                            {
                                LateUpdateProcesses[slot.i] = null;
                            }
                            else if (LateUpdateProcesses[slot.i] != null && float.IsNaN(LateUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    LateUpdateProcesses[slot.i] = null;
                                }
                                else
                                {
                                    LateUpdateProcesses[slot.i] = ReplacementFunction(LateUpdateProcesses[slot.i], indexToHandle[slot]);

                                    ReplacementFunction = null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            LateUpdateProcesses[slot.i] = null;
                        }
                        finally
                        {
                            runningLateUpdate = false;
                        }
                    }

                    return handle;

                case Segment.SlowUpdate:

                    if (nextSlowUpdateProcessSlot >= SlowUpdateProcesses.Length)
                    {
                        IEnumerator<float>[] oldProcArray = SlowUpdateProcesses;
                        bool[] oldPausedArray = SlowUpdatePaused;

                        SlowUpdateProcesses = new IEnumerator<float>[SlowUpdateProcesses.Length + (ProcessArrayChunkSize * expansions++)];
                        SlowUpdatePaused = new bool[SlowUpdateProcesses.Length];

                        for (int i = 0; i < oldProcArray.Length; i++)
                        {
                            SlowUpdateProcesses[i] = oldProcArray[i];
                            SlowUpdatePaused[i] = oldPausedArray[i];
                        }
                    }

                    slot.i = nextSlowUpdateProcessSlot++;
                    SlowUpdateProcesses[slot.i] = coroutine;

                    if (tag != null)
                        AddTag(tag, slot);

                    indexToHandle.Add(slot, handle);
                    handleToIndex.Add(handle, slot);

                    if (!runningSlowUpdate && prewarm)
                    {
                        try
                        {
                            runningSlowUpdate = true;
                            UpdateTimeValues(slot.seg);

                            if (!SlowUpdateProcesses[slot.i].MoveNext())
                            {
                                SlowUpdateProcesses[slot.i] = null;
                            }
                            else if (SlowUpdateProcesses[slot.i] != null && float.IsNaN(SlowUpdateProcesses[slot.i].Current))
                            {
                                if (ReplacementFunction == null)
                                {
                                    SlowUpdateProcesses[slot.i] = null;
                                }
                                else
                                {
                                    SlowUpdateProcesses[slot.i] = ReplacementFunction(SlowUpdateProcesses[slot.i], indexToHandle[slot]);

                                    ReplacementFunction = null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError == null)
                                _exceptions.Enqueue(ex);
                            else
                                OnError(ex);

                            SlowUpdateProcesses[slot.i] = null;
                        }
                        finally
                        {
                            runningSlowUpdate = false;
                        }
                    }

                    return handle;

                default:
                    return new CoroutineHandle();
            }
        }

        /// <summary>
        /// This will kill all coroutines running on the main MEC instance and reset the context.
        /// </summary>
        /// <returns>The number of coroutines that were killed.</returns>
        public static int KillCoroutines()
        {
            return instance == null ? 0 : instance.KillCoroutinesOnInstance();
        }

        /// <summary>
        /// This will kill all coroutines running on the current MEC instance and reset the context.
        /// </summary>
        /// <returns>The number of coroutines that were killed.</returns>
        public int KillCoroutinesOnInstance()
        {
            int retVal = nextUpdateProcessSlot + nextLateUpdateProcessSlot + nextFixedUpdateProcessSlot + nextSlowUpdateProcessSlot;

            UpdateProcesses = new IEnumerator<float>[InitialBufferSizeLarge];
            UpdatePaused = new bool[InitialBufferSizeLarge];
            UpdateCoroutines = 0;
            nextUpdateProcessSlot = 0;

            LateUpdateProcesses = new IEnumerator<float>[InitialBufferSizeSmall];
            LateUpdatePaused = new bool[InitialBufferSizeSmall];
            LateUpdateCoroutines = 0;
            nextLateUpdateProcessSlot = 0;

            FixedUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
            FixedUpdatePaused = new bool[InitialBufferSizeMedium];
            FixedUpdateCoroutines = 0;
            nextFixedUpdateProcessSlot = 0;

            SlowUpdateProcesses = new IEnumerator<float>[InitialBufferSizeMedium];
            SlowUpdatePaused = new bool[InitialBufferSizeMedium];
            SlowUpdateCoroutines = 0;
            nextSlowUpdateProcessSlot = 0;

            processTags.Clear();
            taggedProcesses.Clear();
            handleToIndex.Clear();
            indexToHandle.Clear();
            waitingTriggers.Clear();
            expansions = (ushort)((expansions / 2) + 1);

            ResetTimeCountOnInstance();

            return retVal;
        }

        /// <summary>
        /// Kills the instances of the coroutine handle if it exists.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to kill.</param>
        /// <returns>The number of coroutines that were found and killed (0 or 1).</returns>
        public static int KillCoroutines(CoroutineHandle handle)
        {
            return ActiveInstances.ContainsKey(handle.Key) ? GetInstance(handle.Key).KillCoroutinesOnInstance(handle) : 0;
        }

        /// <summary>
        /// Kills the instance of the coroutine handle on this Timing instance if it exists.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to kill.</param>
        /// <returns>The number of coroutines that were found and killed (0 or 1).</returns>
        public int KillCoroutinesOnInstance(CoroutineHandle handle)
        {
            bool foundOne = false;

            if (handleToIndex.ContainsKey(handle))
            {
                if (waitingTriggers.ContainsKey(handle))
                    CloseWaitingProcess(handle);

                foundOne = CoindexExtract(handleToIndex[handle]) != null;
                RemoveTag(handleToIndex[handle]);
            }

            return foundOne ? 1 : 0;
        }

        /// <summary>
        /// Kills all coroutines that have the given tag.
        /// </summary>
        /// <param name="tag">All coroutines with this tag will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public static int KillCoroutines(string tag)
        {
            return instance == null ? 0 : instance.KillCoroutinesOnInstance(tag);
        }

        /// <summary> 
        /// Kills all coroutines that have the given tag.
        /// </summary>
        /// <param name="tag">All coroutines with this tag will be killed.</param>
        /// <returns>The number of coroutines that were found and killed.</returns>
        public int KillCoroutinesOnInstance(string tag)
        {
            if (tag == null) return 0;
            int numberFound = 0;

            while (taggedProcesses.ContainsKey(tag))
            {
                var matchEnum = taggedProcesses[tag].GetEnumerator();
                matchEnum.MoveNext();

                if (CoindexKill(matchEnum.Current))
                {
                    if (waitingTriggers.ContainsKey(indexToHandle[matchEnum.Current]))
                        CloseWaitingProcess(indexToHandle[matchEnum.Current]);

                    numberFound++;
                }

                RemoveTag(matchEnum.Current);

                if (indexToHandle.ContainsKey(matchEnum.Current))
                {
                    handleToIndex.Remove(indexToHandle[matchEnum.Current]);
                    indexToHandle.Remove(matchEnum.Current);
                }
            }

            return numberFound;
        }

        /// <summary>
        /// This will pause all coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines()
        {
            return instance == null ? 0 : instance.PauseCoroutinesOnInstance();
        }

        /// <summary>
        /// This will pause all coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance()
        {
            int count = 0;
            int i;
            for (i = 0; i < nextUpdateProcessSlot; i++)
            {
                if (!UpdatePaused[i] && UpdateProcesses[i] != null)
                {
                    UpdatePaused[i] = true;
                    count++;
                }
            }

            for (i = 0; i < nextLateUpdateProcessSlot; i++)
            {
                if (!LateUpdatePaused[i] && LateUpdateProcesses[i] != null)
                {
                    LateUpdatePaused[i] = true;
                    count++;
                }
            }

            for (i = 0; i < nextFixedUpdateProcessSlot; i++)
            {
                if (!FixedUpdatePaused[i] && FixedUpdateProcesses[i] != null)
                {
                    FixedUpdatePaused[i] = true;
                    count++;
                }
            }

            for (i = 0; i < nextSlowUpdateProcessSlot; i++)
            {
                if (!SlowUpdatePaused[i] && SlowUpdateProcesses[i] != null)
                {
                    SlowUpdatePaused[i] = true;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// This will pause any matching coroutines running on the current MEC instance until ResumeCoroutines is called.
        /// </summary>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public static int PauseCoroutines(string tag)
        {
            return instance == null ? 0 : instance.PauseCoroutinesOnInstance(tag);
        }

        /// <summary>
        /// This will pause any matching coroutines running on this MEC instance until ResumeCoroutinesOnInstance is called.
        /// </summary>
        /// <param name="tag">Any coroutines with a matching tag will be paused.</param>
        /// <returns>The number of coroutines that were paused.</returns>
        public int PauseCoroutinesOnInstance(string tag)
        {
            if (tag == null || !taggedProcesses.ContainsKey(tag))
                return 0;

            int count = 0;
            var matchesEnum = taggedProcesses[tag].GetEnumerator();

            while (matchesEnum.MoveNext())
                if (!CoindexIsNull(matchesEnum.Current) && !CoindexSetPause(matchesEnum.Current))
                    count++;

            return count;
        }

        /// <summary>
        /// This resumes all coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines()
        {
            return instance == null ? 0 : instance.ResumeCoroutinesOnInstance();
        }

        /// <summary>
        /// This resumes all coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance()
        {
            int count = 0;
            int i;
            for (i = 0; i < nextUpdateProcessSlot; i++)
            {
                if (UpdatePaused[i] && UpdateProcesses[i] != null)
                {
                    UpdatePaused[i] = false;
                    count++;
                }
            }

            for (i = 0; i < nextLateUpdateProcessSlot; i++)
            {
                if (LateUpdatePaused[i] && LateUpdateProcesses[i] != null)
                {
                    LateUpdatePaused[i] = false;
                    count++;
                }
            }

            for (i = 0; i < nextFixedUpdateProcessSlot; i++)
            {
                if (FixedUpdatePaused[i] && FixedUpdateProcesses[i] != null)
                {
                    FixedUpdatePaused[i] = false;
                    count++;
                }
            }

            for (i = 0; i < nextSlowUpdateProcessSlot; i++)
            {
                if (SlowUpdatePaused[i] && SlowUpdateProcesses[i] != null)
                {
                    SlowUpdatePaused[i] = false;
                    count++;
                }
            }

            var waitingEnum = waitingTriggers.GetEnumerator();
            while (waitingEnum.MoveNext())
            {
                int listCount = 0;
                var pausedList = waitingEnum.Current.Value.GetEnumerator();

                while (pausedList.MoveNext())
                {
                    if (handleToIndex.ContainsKey(pausedList.Current.Handle) && !CoindexIsNull(handleToIndex[pausedList.Current.Handle]))
                    {
                        CoindexSetPause(handleToIndex[pausedList.Current.Handle]);
                        listCount++;
                    }
                    else
                    {
                        waitingEnum.Current.Value.Remove(pausedList.Current);
                        listCount = 0;
                        pausedList = waitingEnum.Current.Value.GetEnumerator();
                    }
                }

                count -= listCount;
            }

            return count;
        }

        /// <summary>
        /// This resumes any matching coroutines on the current MEC instance if they are currently paused, otherwise it has
        /// no effect.
        /// </summary>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public static int ResumeCoroutines(string tag)
        {
            return instance == null ? 0 : instance.ResumeCoroutinesOnInstance(tag);
        }

        /// <summary>
        /// This resumes any matching coroutines on this MEC instance if they are currently paused, otherwise it has no effect.
        /// </summary>
        /// <param name="tag">Any coroutines previously paused with a matching tag will be resumend.</param>
        /// <returns>The number of coroutines that were resumed.</returns>
        public int ResumeCoroutinesOnInstance(string tag)
        {
            if (tag == null || !taggedProcesses.ContainsKey(tag))
                return 0;
            int count = 0;

            var indexesEnum = taggedProcesses[tag].GetEnumerator();
            while (indexesEnum.MoveNext())
                if (!CoindexIsNull(indexesEnum.Current) && CoindexSetPause(indexesEnum.Current, false))
                    count++;

            var waitingEnum = waitingTriggers.GetEnumerator();
            while (waitingEnum.MoveNext())
            {
                var pausedList = waitingEnum.Current.Value.GetEnumerator();
                while (pausedList.MoveNext())
                {
                    if (handleToIndex.ContainsKey(pausedList.Current.Handle) && !CoindexIsNull(handleToIndex[pausedList.Current.Handle])
                        && !CoindexSetPause(handleToIndex[pausedList.Current.Handle]))
                        count--;
                }
            }

            return count;
        }

        private void UpdateTimeValues(Segment segment)
        {
            switch (segment)
            {
                case Segment.Update:
                case Segment.LateUpdate:
                    if (currentUpdateFrame != Time.frameCount)
                    {
                        deltaTime = Time.deltaTime;
                        lastUpdateTime += deltaTime;
                        localTime = lastUpdateTime;
                        currentUpdateFrame = Time.frameCount;
                    }
                    else
                    {
                        deltaTime = Time.deltaTime;
                        localTime = lastUpdateTime;
                    }
                    return;
                case Segment.FixedUpdate:
                    if (currentFixedUpdateFrame != Time.frameCount)
                    {
                        deltaTime = Time.deltaTime;
                        lastFixedUpdateTime += deltaTime;
                        localTime = lastFixedUpdateTime;
                        currentFixedUpdateFrame = Time.frameCount;
                    }
                    else
                    {
                        deltaTime = Time.deltaTime;
                        localTime = lastFixedUpdateTime;
                    }
                    return;
                case Segment.SlowUpdate:
                    if (currentSlowUpdateFrame != Time.frameCount)
                    {
                        deltaTime = lastSlowUpdateDeltaTime = Time.realtimeSinceStartup - (float)lastSlowUpdateTime;
                        localTime = lastSlowUpdateTime = Time.realtimeSinceStartup;
                        currentSlowUpdateFrame = Time.frameCount;
                    }
                    else
                    {
                        deltaTime = lastSlowUpdateDeltaTime;
                        localTime = lastSlowUpdateTime;
                    }
                    return;
            }
        }

        private double GetSegmentTime(Segment segment)
        {
            switch (segment)
            {
                case Segment.Update:
                case Segment.LateUpdate:
                    if (currentUpdateFrame == Time.frameCount)
                        return lastUpdateTime;
                    else
                        return lastUpdateTime + Time.deltaTime;
                case Segment.FixedUpdate:
                    if (currentFixedUpdateFrame == Time.frameCount)
                        return lastFixedUpdateTime;
                    else
                        return lastFixedUpdateTime + Time.deltaTime;
                case Segment.SlowUpdate:
                    return Time.realtimeSinceStartup;
                default:
                    return 0d;
            }
        }

        /// <summary>
        /// Not all segments can have their local time value reset to zero, but the ones that can are reset through this function.
        /// </summary>
        public void ResetTimeCountOnInstance()
        {
            localTime = 0d;

            lastUpdateTime = 0d;
            lastFixedUpdateTime = 0d;
        }

        /// <summary>
        /// Retrieves the MEC manager that corresponds to the supplied instance id.
        /// </summary>
        /// <param name="ID">The instance ID.</param>
        /// <returns>The manager, or null if not found.</returns>
        public static TimingCoroutine GetInstance(byte ID)
        {
            return ActiveInstances.ContainsKey(ID) ? ActiveInstances[ID] : null;
        }

        private void AddTag(string tag, ProcessIndex coindex)
        {
            processTags.Add(coindex, tag);

            if (taggedProcesses.ContainsKey(tag))
                taggedProcesses[tag].Add(coindex);
            else
                taggedProcesses.Add(tag, new HashSet<ProcessIndex> { coindex });
        }

        private void RemoveTag(ProcessIndex coindex)
        {
            if (processTags.ContainsKey(coindex))
            {
                if (taggedProcesses[processTags[coindex]].Count > 1)
                    taggedProcesses[processTags[coindex]].Remove(coindex);
                else
                    taggedProcesses.Remove(processTags[coindex]);

                processTags.Remove(coindex);
            }
        }

        private void MoveTag(ProcessIndex coindexFrom, ProcessIndex coindexTo)
        {
            RemoveTag(coindexTo);

            if (processTags.ContainsKey(coindexFrom))
            {
                taggedProcesses[processTags[coindexFrom]].Remove(coindexFrom);
                taggedProcesses[processTags[coindexFrom]].Add(coindexTo);

                processTags.Add(coindexTo, processTags[coindexFrom]);
                processTags.Remove(coindexFrom);
            }
        }

        private bool CoindexKill(ProcessIndex coindex)
        {
            bool retVal;

            switch (coindex.seg)
            {
                case Segment.Update:
                    retVal = UpdateProcesses[coindex.i] != null;
                    UpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.FixedUpdate:
                    retVal = FixedUpdateProcesses[coindex.i] != null;
                    FixedUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.LateUpdate:
                    retVal = LateUpdateProcesses[coindex.i] != null;
                    LateUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.SlowUpdate:
                    retVal = SlowUpdateProcesses[coindex.i] != null;
                    SlowUpdateProcesses[coindex.i] = null;
                    return retVal;
            }

            return false;
        }

        private IEnumerator<float> CoindexExtract(ProcessIndex coindex)
        {
            IEnumerator<float> retVal;

            switch (coindex.seg)
            {
                case Segment.Update:
                    retVal = UpdateProcesses[coindex.i];
                    UpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.FixedUpdate:
                    retVal = FixedUpdateProcesses[coindex.i];
                    FixedUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.LateUpdate:
                    retVal = LateUpdateProcesses[coindex.i];
                    LateUpdateProcesses[coindex.i] = null;
                    return retVal;
                case Segment.SlowUpdate:
                    retVal = SlowUpdateProcesses[coindex.i];
                    SlowUpdateProcesses[coindex.i] = null;
                    return retVal;
                default:
                    return null;
            }
        }

        private IEnumerator<float> CoindexPeek(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdateProcesses[coindex.i];
                case Segment.FixedUpdate:
                    return FixedUpdateProcesses[coindex.i];
                case Segment.LateUpdate:
                    return LateUpdateProcesses[coindex.i];
                case Segment.SlowUpdate:
                    return SlowUpdateProcesses[coindex.i];
                default:
                    return null;
            }
        }

        private bool CoindexIsNull(ProcessIndex coindex)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    return UpdateProcesses[coindex.i] == null;
                case Segment.FixedUpdate:
                    return FixedUpdateProcesses[coindex.i] == null;
                case Segment.LateUpdate:
                    return LateUpdateProcesses[coindex.i] == null;
                case Segment.SlowUpdate:
                    return SlowUpdateProcesses[coindex.i] == null;
                default:
                    return true;
            }
        }

        private bool CoindexSetPause(ProcessIndex coindex, bool newPausedState = true)
        {
            bool isPaused;

            switch (coindex.seg)
            {
                case Segment.Update:
                    isPaused = UpdatePaused[coindex.i];
                    UpdatePaused[coindex.i] = newPausedState;
                    return isPaused;
                case Segment.FixedUpdate:
                    isPaused = FixedUpdatePaused[coindex.i];
                    FixedUpdatePaused[coindex.i] = newPausedState;
                    return isPaused;
                case Segment.LateUpdate:
                    isPaused = LateUpdatePaused[coindex.i];
                    LateUpdatePaused[coindex.i] = newPausedState;
                    return isPaused;
                case Segment.SlowUpdate:
                    isPaused = SlowUpdatePaused[coindex.i];
                    SlowUpdatePaused[coindex.i] = newPausedState;
                    return isPaused;
                default:
                    return false;
            }
        }

        private void CoindexReplace(ProcessIndex coindex, IEnumerator<float> replacement)
        {
            switch (coindex.seg)
            {
                case Segment.Update:
                    UpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.FixedUpdate:
                    FixedUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.LateUpdate:
                    LateUpdateProcesses[coindex.i] = replacement;
                    return;
                case Segment.SlowUpdate:
                    SlowUpdateProcesses[coindex.i] = replacement;
                    return;
            }
        }

        private static IEnumerator<float> _InjectDelay(IEnumerator<float> proc, double returnAt)
        {
            yield return (float)returnAt;

            ReplacementFunction = delegate { return proc; };
            yield return float.NaN;
        }

        /// <summary>
        /// Use in a yield return statement to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public static float WaitForSeconds(float waitTime)
        {
            if (float.IsNaN(waitTime)) waitTime = 0f;
            return LocalTime + waitTime;
        }

        /// <summary>
        /// Use in a yield return statement to wait for the specified number of seconds.
        /// </summary>
        /// <param name="waitTime">Number of seconds to wait.</param>
        public float WaitForSecondsOnInstance(float waitTime)
        {
            if (float.IsNaN(waitTime)) waitTime = 0f;
            return (float)localTime + waitTime;
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        public static float WaitUntilDone(CoroutineHandle otherCoroutine)
        {
            return WaitUntilDone(otherCoroutine, true);
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(otherCoroutine);" to pause the current 
        /// coroutine until otherCoroutine is done.
        /// </summary>
        /// <param name="otherCoroutine">The coroutine to pause for.</param>
        /// <param name="warnOnIssue">Post a warning to the console if no hold action was actually performed.</param>
        public static float WaitUntilDone(CoroutineHandle otherCoroutine, bool warnOnIssue)
        {
            TimingCoroutine inst = GetInstance(otherCoroutine.Key);

            if (inst != null && inst.handleToIndex.ContainsKey(otherCoroutine))
            {
                if (inst.CoindexIsNull(inst.handleToIndex[otherCoroutine]))
                    return 0f;

                if (!inst.waitingTriggers.ContainsKey(otherCoroutine))
                {
                    inst.CoindexReplace(inst.handleToIndex[otherCoroutine],
                        inst._StartWhenDone(otherCoroutine, inst.CoindexPeek(inst.handleToIndex[otherCoroutine])));
                    inst.waitingTriggers.Add(otherCoroutine, new HashSet<ProcessData>());
                }

                ReplacementFunction = (coptr, handle) =>
                {
                    if (handle == otherCoroutine)
                    {
                        if (warnOnIssue)
                            Debug.LogWarning("A coroutine attempted to wait for itself.");

                        return coptr;
                    }
                    if (handle.Key != otherCoroutine.Key)
                    {
                        if (warnOnIssue)
                            Debug.LogWarning("A coroutine attempted to wait for a coroutine running on a different MEC instance.");

                        return coptr;
                    }

                    inst.waitingTriggers[otherCoroutine].Add(new ProcessData
                    {
                        Handle = handle,
                        PauseTime = coptr.Current > inst.GetSegmentTime(inst.handleToIndex[handle].seg)
                            ? coptr.Current - (float)inst.GetSegmentTime(inst.handleToIndex[handle].seg) : 0f
                    });

                    inst.CoindexSetPause(inst.handleToIndex[handle]);

                    return coptr;
                };

                return float.NaN;
            }

            if (warnOnIssue)
                Debug.LogWarning("WaitUntilDone cannot hold: The coroutine handle that was passed in is invalid.\n" + otherCoroutine);

            return 0f;
        }

        private IEnumerator<float> _StartWhenDone(CoroutineHandle handle, IEnumerator<float> proc)
        {
            if (!waitingTriggers.ContainsKey(handle))
                yield break;

            try
            {
                if (proc.Current > localTime)
                    yield return proc.Current;

                while (proc.MoveNext())
                    yield return proc.Current;
            }
            finally
            {
                CloseWaitingProcess(handle);
            }
        }

        private void CloseWaitingProcess(CoroutineHandle handle)
        {
            if (!waitingTriggers.ContainsKey(handle)) return;

            var tasksEnum = waitingTriggers[handle].GetEnumerator();
            waitingTriggers.Remove(handle);

            while (tasksEnum.MoveNext())
            {
                if (handleToIndex.ContainsKey(tasksEnum.Current.Handle))
                {
                    ProcessIndex coIndex = handleToIndex[tasksEnum.Current.Handle];

                    if (tasksEnum.Current.PauseTime > 0d)
                        CoindexReplace(coIndex, _InjectDelay(CoindexPeek(coIndex), (float)(GetSegmentTime(coIndex.seg) + tasksEnum.Current.PauseTime)));

                    CoindexSetPause(coIndex, false);
                }
            }
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(wwwObject);" to pause the current 
        /// coroutine until the wwwObject is done.
        /// </summary>
        /// <param name="wwwObject">The www object to pause for.</param>
        public static float WaitUntilDone(WWW wwwObject)
        {
            if (wwwObject == null || wwwObject.isDone) return 0f;
            ReplacementFunction = (input, tag) => _StartWhenDone(wwwObject, input);
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(WWW www, IEnumerator<float> pausedProc)
        {
            while (!www.isDone)
                yield return 0f;

            ReplacementFunction = delegate { return pausedProc; };
            yield return float.NaN;
        }

        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(operation);" to pause the current 
        /// coroutine until the operation is done.
        /// </summary>
        /// <param name="operation">The operation variable returned.</param>
        public static float WaitUntilDone(AsyncOperation operation)
        {
            if (operation == null || operation.isDone) return 0f;
            ReplacementFunction = (input, tag) => _StartWhenDone(operation, input);
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(AsyncOperation operation, IEnumerator<float> pausedProc)
        {
            while (!operation.isDone)
                yield return 0f;

            ReplacementFunction = delegate { return pausedProc; };
            yield return float.NaN;
        }

#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
        /// <summary>
        /// Use the command "yield return Timing.WaitUntilDone(operation);" to pause the current 
        /// coroutine until the operation is done.
        /// </summary>
        /// <param name="operation">The operation variable returned.</param>
        public static float WaitUntilDone(CustomYieldInstruction operation)
        {
            if (operation == null || !operation.keepWaiting) return 0f;
            ReplacementFunction = (input, tag) => _StartWhenDone(operation, input);
            return float.NaN;
        }

        private static IEnumerator<float> _StartWhenDone(CustomYieldInstruction operation, IEnumerator<float> pausedProc)
        {
            while (operation.keepWaiting)
                yield return 0f;

            ReplacementFunction = delegate { return pausedProc; };
            yield return float.NaN;
        }
#endif

        /// <summary>
        /// Keeps this coroutine from executing until UnlockCoroutine is called with a matching key.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine to be locked.</param>
        /// <param name="key">The key to use. A new key can be generated by calling "new CoroutineHandle(0)".</param>
        /// <returns>Whether the lock was successful.</returns>
        public bool LockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
        {
            if (coroutine.Key != instanceID || key == new CoroutineHandle() || key.Key != 0)
                return false;

            if (!waitingTriggers.ContainsKey(key))
                waitingTriggers.Add(key, new HashSet<ProcessData>());

            waitingTriggers[key].Add(new ProcessData { Handle = coroutine });
            CoindexSetPause(handleToIndex[coroutine]);

            return true;
        }

        /// <summary>
        /// Unlocks a coroutine that has been locked, so long as the key matches.
        /// </summary>
        /// <param name="coroutine">The handle to the coroutine to be unlocked.</param>
        /// <param name="key">The key that the coroutine was previously locked with.</param>
        /// <returns>Whether the coroutine was successfully unlocked.</returns>
        public bool UnlockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
        {
            if (coroutine.Key != instanceID || key == new CoroutineHandle() ||
                !handleToIndex.ContainsKey(coroutine) || !waitingTriggers.ContainsKey(key))
                return false;

            ProcessData wrappedCoroutine = new ProcessData { Handle = coroutine };
            waitingTriggers[key].Remove(wrappedCoroutine);

            bool coroutineStillPaused = false;
            var triggersEnum = waitingTriggers.GetEnumerator();
            while (triggersEnum.MoveNext())
                if (triggersEnum.Current.Value.Contains(wrappedCoroutine))
                    coroutineStillPaused = true;

            CoindexSetPause(handleToIndex[coroutine], coroutineStillPaused);

            return true;
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallDelayed(float delay, Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._DelayedCall(delay, action));
        }

        /// <summary>
        /// Calls the specified action after a specified number of seconds.
        /// </summary>
        /// <param name="delay">The number of seconds to wait before calling the action.</param>
        /// <param name="action">The action to call.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallDelayedOnInstance(float delay, Action action)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_DelayedCall(delay, action));
        }

        private IEnumerator<float> _DelayedCall(float delay, Action action)
        {
            yield return WaitForSecondsOnInstance(delay);

            action();
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(timeframe, period, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(timeframe, period, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Segment timing, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(timeframe, period, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Segment timing, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(timeframe, period, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, Action action, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(timeframe, 0f, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(timeframe, 0f, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously(float timeframe, Action action, Segment timing, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutine(Instance._CallContinuously(timeframe, 0f, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Segment timing, Action onDone = null)
        {
            return action == null ? new CoroutineHandle() : RunCoroutineOnInstance(_CallContinuously(timeframe, 0f, action, onDone), timing);
        }

        private IEnumerator<float> _CallContinuously(float timeframe, float period, Action action, Action onDone)
        {
            double startTime = localTime;
            while (localTime <= startTime + timeframe)
            {
                yield return WaitForSecondsOnInstance(period);

                action();
            }

            if (onDone != null)
                onDone();
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>
            (T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutine(Instance._CallContinuously(reference, timeframe, period, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>
            (T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutineOnInstance(_CallContinuously(reference, timeframe, period, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action,
            Segment timing, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutine(Instance._CallContinuously(reference, timeframe, period, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action at the given rate for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each period.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="period">The amount of time between calls.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action,
            Segment timing, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutineOnInstance(_CallContinuously(reference, timeframe, period, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutine(Instance._CallContinuously(reference, timeframe, 0f, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutineOnInstance(_CallContinuously(reference, timeframe, 0f, action, onDone), Segment.Update);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action,
            Segment timing, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutine(Instance._CallContinuously(reference, timeframe, 0f, action, onDone), timing);
        }

        /// <summary>
        /// Calls the supplied action every frame for a given number of seconds.
        /// </summary>
        /// <param name="reference">A value that will be passed in to the supplied action each frame.</param>
        /// <param name="timeframe">The number of seconds that this function should run.</param>
        /// <param name="action">The action to call every frame.</param>
        /// <param name="timing">The timing segment to run in.</param>
        /// <param name="onDone">An optional action to call when this function finishes.</param>
        /// <returns>The handle to the coroutine that is started by this function.</returns>
        public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action,
            Segment timing, Action<T> onDone = null)
        {
            return action == null ? new CoroutineHandle() :
                RunCoroutineOnInstance(_CallContinuously(reference, timeframe, 0f, action, onDone), timing);
        }

        private IEnumerator<float> _CallContinuously<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
        {
            double startTime = localTime;
            while (localTime <= startTime + timeframe)
            {
                yield return WaitForSecondsOnInstance(period);

                action(reference);
            }

            if (onDone != null)
                onDone(reference);
        }

        private struct ProcessData : IEquatable<ProcessData>
        {
            public CoroutineHandle Handle;
            public float PauseTime;

            public bool Equals(ProcessData other)
            {
                return Handle == other.Handle;
            }

            public override bool Equals(object other)
            {
                if (other is ProcessData)
                    return Equals((ProcessData)other);
                return false;
            }

            public override int GetHashCode()
            {
                return Handle.GetHashCode();
            }
        }

        private struct ProcessIndex : IEquatable<ProcessIndex>
        {
            public Segment seg;
            public int i;

            public bool Equals(ProcessIndex other)
            {
                return seg == other.seg && i == other.i;
            }

            public override bool Equals(object other)
            {
                if (other is ProcessIndex)
                    return Equals((ProcessIndex)other);
                return false;
            }

            public static bool operator ==(ProcessIndex a, ProcessIndex b)
            {
                return a.seg == b.seg && a.i == b.i;
            }

            public static bool operator !=(ProcessIndex a, ProcessIndex b)
            {
                return a.seg != b.seg || a.i != b.i;
            }

            public override int GetHashCode()
            {
                return (((int)seg - 2) * (int.MaxValue / 3)) + i;
            }
        }

        #region Obsolete

        [Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(System.Collections.IEnumerator routine) { return null; }

        [Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }

        [Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine(string methodName) { return null; }

        [Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
        public new Coroutine StartCoroutine_Auto(System.Collections.IEnumerator routine) { return null; }

        [Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
        public new void StopCoroutine(string methodName) { }

        [Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
        public new void StopCoroutine(System.Collections.IEnumerator routine) { }

        [Obsolete("Unity coroutine function, use KillCoroutine instead.", true)]
        public new void StopCoroutine(Coroutine routine) { }

        [Obsolete("Unity coroutine function, use KillAllCoroutines instead.", true)]
        public new void StopAllCoroutines() { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void Destroy(UnityEngine.Object obj) { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void Destroy(UnityEngine.Object obj, float f) { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyObject(UnityEngine.Object obj) { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyObject(UnityEngine.Object obj, float f) { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyImmediate(UnityEngine.Object obj) { }

        [Obsolete("Use your own GameObject for this.", true)]
        public new static void DestroyImmediate(UnityEngine.Object obj, bool b) { }

        [Obsolete("Just.. no.", true)]
        public new static T FindObjectOfType<T>() where T : UnityEngine.Object { return null; }

        [Obsolete("Just.. no.", true)]
        public new static UnityEngine.Object FindObjectOfType(Type t) { return null; }

        [Obsolete("Just.. no.", true)]
        public new static T[] FindObjectsOfType<T>() where T : UnityEngine.Object { return null; }

        [Obsolete("Just.. no.", true)]
        public new static UnityEngine.Object[] FindObjectsOfType(Type t) { return null; }

        [Obsolete("Just.. no.", true)]
        public new static void print(object message) { }

        #endregion
    }

    #endregion

    #region Segment enum
    public enum Segment
    {
        Invalid = -1,
        Update,
        FixedUpdate,
        LateUpdate,
        SlowUpdate,
    }
    #endregion

    #region MEC handle
    /// <summary>
    /// A handle for a MEC coroutine.
    /// </summary>
    public struct CoroutineHandle : IEquatable<CoroutineHandle>
    {
        private const byte ReservedSpace = 0x1F;
        private readonly static int[] NextIndex =
            { ReservedSpace + 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int id;

        public byte Key { get { return (byte)(id & ReservedSpace); } }

        public CoroutineHandle(byte ind)
        {
            if (ind > ReservedSpace)
                ind -= ReservedSpace;

            id = NextIndex[ind] + ind;
            NextIndex[ind] += ReservedSpace + 1;
        }

        public bool Equals(CoroutineHandle other)
        {
            return id == other.id;
        }

        public override bool Equals(object other)
        {
            if (other is CoroutineHandle)
                return Equals((CoroutineHandle)other);
            return false;
        }

        public static bool operator ==(CoroutineHandle a, CoroutineHandle b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(CoroutineHandle a, CoroutineHandle b)
        {
            return a.id != b.id;
        }

        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// Is true if this handle may have been a valid handle at some point. (i.e. is not an uninitialized handle, error handle, or a key to a coroutine lock)
        /// </summary>
        public bool IsValid
        {
            get { return Key != 0; }
        }
        #endregion

    }

    #region Extension Methods
    public static class ExtensionMethods
    {
        /// <summary>
        /// Cancels this coroutine when the supplied game object is destroyed or made inactive.
        /// </summary>
        /// <param name="coroutine">The coroutine handle to act upon.</param>
        /// <param name="gameObject">The GameObject to test.</param>
        /// <returns>The modified coroutine handle.</returns>
        public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject)
        {
            while (TimingCoroutine.MainThread != Thread.CurrentThread || (gameObject && gameObject.activeInHierarchy && coroutine.MoveNext()))
                yield return coroutine.Current;
        }

        /// <summary>
        /// Cancels this coroutine when the supplied game objects are destroyed or made inactive.
        /// </summary>
        /// <param name="coroutine">The coroutine handle to act upon.</param>
        /// <param name="gameObject1">The first GameObject to test.</param>
        /// <param name="gameObject2">The second GameObject to test</param>
        /// <returns>The modified coroutine handle.</returns>
        public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
        {
            while (TimingCoroutine.MainThread != Thread.CurrentThread || (gameObject1 && gameObject1.activeInHierarchy &&
                    gameObject2 && gameObject2.activeInHierarchy && coroutine.MoveNext()))
                yield return coroutine.Current;
        }

        /// <summary>
        /// Cancels this coroutine when the supplied game objects are destroyed or made inactive.
        /// </summary>
        /// <param name="coroutine">The coroutine handle to act upon.</param>
        /// <param name="gameObject1">The first GameObject to test.</param>
        /// <param name="gameObject2">The second GameObject to test</param>
        /// <param name="gameObject3">The third GameObject to test.</param>
        /// <returns>The modified coroutine handle.</returns>
        public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2, GameObject gameObject3)
        {
            while (TimingCoroutine.MainThread != Thread.CurrentThread || (gameObject1 && gameObject1.activeInHierarchy &&
                    gameObject2 && gameObject2.activeInHierarchy && gameObject3 && gameObject3.activeInHierarchy && coroutine.MoveNext()))
                yield return coroutine.Current;
        }
    }
    #endregion
}
