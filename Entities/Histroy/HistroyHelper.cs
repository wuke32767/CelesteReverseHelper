using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.ReverseHelper.Entities.Histroy
{
    public class HistroyStateMachine<T> : Component
    {
        // Token: 0x1700007C RID: 124
        // (get) Token: 0x060006FA RID: 1786 RVA: 0x0000C06D File Offset: 0x0000A26D
        // (set) Token: 0x060006FB RID: 1787 RVA: 0x0000C075 File Offset: 0x0000A275
        public int PreviousState { get; private set; }

        // Token: 0x060006FC RID: 1788 RVA: 0x0000C080 File Offset: 0x0000A280
        public HistroyStateMachine(int maxStates = 10, int static_state = 0) : base(true, false)
        {
            PreviousState = state = -1;
            begins = new Action[maxStates];
            updates = new Func<int>[maxStates];
            ends = new Action[maxStates];
            frames = new Func<IEnumerator>[maxStates];

            rbegins = new Action[maxStates];
            rupdates = new Func<int>[maxStates];
            rends = new Action[maxStates];
            rframes = new Func<IEnumerator>[maxStates];
            currentCoroutine = new Coroutine(true);
            this.static_state = static_state;
        }

        // Token: 0x060006FD RID: 1789 RVA: 0x0000C0ED File Offset: 0x0000A2ED
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (Entity.Scene is not null && state == -1)
            {
                State = 0;
            }
        }

        // Token: 0x060006FE RID: 1790 RVA: 0x0000C113 File Offset: 0x0000A313
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            if (state == -1)
            {
                State = 0;
            }
        }

        // Token: 0x1700007D RID: 125
        // (get) Token: 0x060006FF RID: 1791 RVA: 0x0000C12C File Offset: 0x0000A32C
        // (set) Token: 0x06000700 RID: 1792 RVA: 0x0000C134 File Offset: 0x0000A334
        public int State
        {
            get
            {
                return state;
            }
            set
            {
                if (!Locked && state != value)
                {
                        if (Log)
                        {
                            Calc.Log(new object[]
                            {
                            string.Concat(new object[]
                            {
                                "Enter State ",
                                value,
                                " (leaving ",
                                state,
                                ")"
                            })
                            });
                        }
                        ChangedStates = true;
                        PreviousState = state;
                        state = value;
                    if (Engine.DeltaTime >= 0)
                    {
                        if (PreviousState != -1 && ends[PreviousState] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Calling End " + PreviousState
                                });
                            }
                            ends[PreviousState]();
                        }
                        if (begins[state] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Calling Begin " + state
                                });
                            }
                            begins[state]();
                        }
                        if (frames[state] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Starting Coroutine " + state
                                });
                            }
                            currentCoroutine.Replace(frames[state]());
                            return;
                        }
                        currentCoroutine.Cancel();
                    }
                    else
                    {
                        if (PreviousState != -1 && rbegins[PreviousState] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Calling End " + PreviousState
                                });
                            }
                            rbegins[PreviousState]();
                        }
                        if (rends[state] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Calling Begin " + state
                                });
                            }
                            rends[state]();
                        }
                        if (rframes[state] is not null)
                        {
                            if (Log)
                            {
                                Calc.Log(new object[]
                                {
                                "Starting Coroutine " + state
                                });
                            }
                            currentCoroutine.Replace(rframes[state]());
                            return;
                        }
                        currentCoroutine.Cancel();

                    }
                }
            }
        }

        public int static_state { get; set; }



        // [update] [update] [update] ExternalState[end begin] [update]
        public void ExternalState(int toState)
        {
            if (state != toState)
            {
                // However, sometimes it would be:
                // [update1] [update1 end1 begin2] ExternalState[end2 begin3] [update3]
                // and state2 is not recorded.
                // if reversed, what begin2 and end2 do will not be reverted.
                if (TopRecord()!=state)
                {
                    rbegins[state]?.Invoke();
                    ChangedStates = true;
                    state = toState;
                    begins[state]?.Invoke();
                    if (frames[state] is not null)
                    {
                        if (Log)
                        {
                            Calc.Log(new object[]
                            {
                                "Starting Coroutine " + state
                            });
                        }
                        currentCoroutine.Replace(frames[state]());
                    }
                    else
                    {
                        currentCoroutine.Cancel();
                    }
                }
                else
                {
                    State = toState;
                }
                
                return;
            }
            //if (Log)
            //{
            //    Calc.Log(new object[]
            //    {
            //        string.Concat(
            //            "Enter State ",
            //            toState,
            //            " (leaving ",
            //            state,
            //            ")"
            //        )
            //    });
            //}
            ////so... state==toState
            //ChangedStates = true;
            //PreviousState = state;
            //state = toState;
            //if (Engine.DeltaTime >= 0)
            //{
            //    if (PreviousState != -1 && ends[PreviousState] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Calling End " + state
            //            });
            //        }
            //        ends[PreviousState]();
            //    }
            //    if (begins[state] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Calling Begin " + state
            //            });
            //        }
            //        begins[state]!();
            //    }
            //    if (frames[state] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Starting Coroutine " + state
            //            });
            //        }
            //        currentCoroutine.Replace(frames[state]());
            //        return;
            //    }
            //    currentCoroutine.Cancel();
            //}
            //else
            //{ 
            //    if (PreviousState != -1 && rbegins[PreviousState] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Calling End " + state
            //            });
            //        }
            //        rbegins[PreviousState]();
            //    }
            //    if (rends[state] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Calling Begin " + state
            //            });
            //        }
            //        rends[state]!();
            //    }
            //    if (rframes[state] is not null)
            //    {
            //        if (Log)
            //        {
            //            Calc.Log(new object[]
            //            {
            //            "Starting Coroutine " + state
            //            });
            //        }
            //        currentCoroutine.Replace(rframes[state]());
            //        return;
            //    }
            //    currentCoroutine.Cancel();
            //}
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="onUpdate"></param>
        /// <param name="ronUpdate"></param>
        /// <param name="coroutine_donotuse">should leave blank.</param>
        /// <param name="rcoroutine_donotuse">should leave blank.</param>
        /// <param name="begin">what to do when changes to this state.</param>
        /// <param name="rbegin">
        /// revert what you did in begin. Not always call when Engine.Delta smaller than 0.see <see cref="ExternalState(int)"/>
        /// rbegin and begin will be called in pairs.
        /// </param>
        /// <param name="end">what to do when about to change to next state.</param>
        /// <param name="rend">
        /// revert what you did in end. Not always call when Engine.Delta smaller than  0.<see cref="ExternalState(int)"/>
        /// rend and end will be called in pairs.
        /// </param>
        public void SetCallbacks(int state, Func<int> onUpdate, Func<int> ronUpdate = null, Func<IEnumerator> coroutine_donotuse = null, Func<IEnumerator> rcoroutine_donotuse = null, Action begin = null, Action rbegin = null, Action end = null, Action rend = null)
        {
            updates[state] = onUpdate;
            begins[state] = begin;
            ends[state] = end;
            frames[state] = coroutine_donotuse;

            rupdates[state] = ronUpdate ?? onUpdate;
            rbegins[state] =  rbegin;
            rends[state] =    rend;
            rframes[state] =  rcoroutine_donotuse;
            //SetCallbacks(0, null, null, null, null, null,)
        }

        // Token: 0x06000703 RID: 1795 RVA: 0x0000C47C File Offset: 0x0000A67C
        //public void ReflectState(Entity from, int index, string name)
        //{
        //    updates[index] = (Func<int>)Calc.GetMethod<Func<int>>(from, name + "Update");
        //    begins[index] = (Action)Calc.GetMethod<Action>(from, name + "Begin");
        //    ends[index] = (Action)Calc.GetMethod<Action>(from, name + "End");
        //    worker[index] = (Func<IEnumerator>)Calc.GetMethod<Func<IEnumerator>>(from, name + "Coroutine");
        //}

        public List<(int state, int len)> histroy = new();

        ///
        ///            ------------------------>
        ///[record(1)] +frame1 [record(2)] +frame2 [record(3)] +frame3 --reverse
        ///            <------------------------
        /// reverse--                      -frame2 [ read(2) ] -frame3 [ read(3) ]
        ///            ------------------------>
        ///                    [record(2)] +frame2 [record(3)] +frame3
        ///                    
        /// frame structure:
        /// --->... begin] [update end begin(next state)] [update end begin] [update end begin]
        /// <---(start from right) [rend rbegin rupdate] [rend rbegin rupdate] [rend rbegin rupdate] [rend ...
        /// if state is not changed:
        /// --->... begin] [update] [update] [update end begin]
        /// <--- [rend rbegin rupdate] [rupdate] [rupdate] [rend ...
        /// if reverse happens:
        /// --->... begin] [update] [update] reverse [rupdate] [rupdate] reverse [update]
        /// and state changed:
        /// --->... begin] [update] [update end begin] reverse [rbegin rend rupdate] [rupdate] reverse [update]
        public override void Update()
        {
            if (Engine.DeltaTime >= 0)
            {
                PushRecord();
                ChangedStates = false;
                if (updates[state] is not null)
                {
                    State = updates[state]();
                }
                if (currentCoroutine.Active)
                {
                    currentCoroutine.Update();
                    if (!ChangedStates && Log && currentCoroutine.Finished)
                    {
                        Calc.Log(new object[]
                        {
                            "Finished Coroutine " + state
                        });
                    }
                }
            }
            else
            {
                int r = PopRecord();
                ChangedStates = false;
                State = r;
                updates[state]?.Invoke();
                if (currentCoroutine.Active)
                {
                    currentCoroutine.Update();
                    if (!ChangedStates && Log && currentCoroutine.Finished)
                    {
                        Calc.Log(
                        
                            "Finished Coroutine " + state
                        );
                    }
                }
            }
        }

        private int PopRecord()
        {
            int nc = histroy.Count - 1;
            if (nc >= 0)
            {
                var (s, l) = histroy[nc];
                if (l > 1)
                {
                    histroy[nc] = (s, l - 1);
                }
                else
                {
                    histroy.RemoveAt(nc);
                }
                return nc;
            }
            return static_state;
        }
        public int TopRecord()
        {
            return histroy.Last().state;
        }

        readonly List<T> extras = new();
        public void PushMessage(T msg)
        {
            extras.Add(msg);
        }
        public T GetMessage()
        {
            return extras.Last();
        }
        public T PopMessage()
        {
            T re = extras.Last();
            extras.RemoveAt(extras.Count - 1);
            return re;
        }
        private void PushRecord()
        {
            int nc = histroy.Count - 1;
            if (nc >= 0 && histroy[nc].state == state)
            {
                var nl = histroy[nc].len + 1;
                histroy[nc] = (state, nl);
            }
            else
            {
                histroy.Add((state, 1));
            }
        }

        // Token: 0x06000705 RID: 1797 RVA: 0x0000C12C File Offset: 0x0000A32C
        public static implicit operator int(HistroyStateMachine<T> s)
        {
            return s.state;
        }

        // Token: 0x06000706 RID: 1798 RVA: 0x0000C598 File Offset: 0x0000A798
        public void LogAllStates()
        {
            //for (int i = 0; i < updates.Length; i++)
            //{
            //    LogState(i);
            //}
        }

        // Token: 0x06000707 RID: 1799 RVA: 0x0000C5C0 File Offset: 0x0000A7C0
        public void LogState(int index)
        {
            //Calc.Log(new object[]
            //{
            //    string.Concat(new object[]
            //    {
            //        "State ",
            //        index,
            //        ": ",
            //        (updates[index] is not null) ? "U" : "",
            //        (begins[index] is not null) ? "B" : "",
            //        (ends[index] is not null) ? "E" : "",
            //        (coroutines[index] is not null) ? "C" : ""
            //    })
            //});
        }

        // Token: 0x04000522 RID: 1314
        private int state;

        // Token: 0x04000523 RID: 1315
        private Action[] begins;

        // Token: 0x04000524 RID: 1316
        private Func<int>[] updates;

        // Token: 0x04000525 RID: 1317
        private Action[] ends;

        // Token: 0x04000526 RID: 1318
        private Func<IEnumerator>[] frames;

        // Token: 0x04000523 RID: 1315
        private Action[] rbegins;

        // Token: 0x04000524 RID: 1316
        private Func<int>[] rupdates;

        // Token: 0x04000525 RID: 1317
        private Action[] rends;

        // Token: 0x04000526 RID: 1318
        private Func<IEnumerator>[] rframes;
        // Token: 0x04000527 RID: 1319
        private Coroutine currentCoroutine;

        // Token: 0x04000528 RID: 1320
        public bool ChangedStates;

        // Token: 0x04000529 RID: 1321
        public bool Log;

        // Token: 0x0400052B RID: 1323
        public bool Locked;
    }

}