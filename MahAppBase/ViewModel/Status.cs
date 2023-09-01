using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ChatUI
{
    /// <summary>
    /// 框架狀態，包含目前記憶體、CPU使用率
    /// </summary>
    public class Status : ViewModelBase
    {
        #region Declarations
        private static PerformanceCounter _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private static float _Cpu;
        private static float _Memory;

        private Task thUpdatStatus;
        #endregion

        #region Property
        /// <summary>
        /// 程式CPU使用率
        /// </summary>
        public float Cpu
        {
            get 
            {
                return _Cpu; 
            }
            set
            {
                _Cpu = value; 
                OnPropertyChanged("Cpu"); 
            }
        }

        /// <summary>
        /// 程式使用記憶體
        /// </summary>
        public float Memory
        {
            get
            {
                return _Memory; 
            }
            set
            {
                _Memory = value; 
                OnPropertyChanged("Memory"); 
            }
        }
        #endregion

        #region Memberfunction
        /// <summary>
        /// ViewModel建構子
        /// </summary>
        public Status()
        {
            //初始化、啟動獲取程式狀態Task
            this.thUpdatStatus = new Task(() => { GetPcStatus(); });
            thUpdatStatus.Start();
        }

        public void GetPcStatus()
        {
            using (Process pro = Process.GetProcessesByName("ChatUI")[0])
            {
                _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                Process proc = Process.GetCurrentProcess();
                while (true)
                {
                    try
                    {
                        proc = Process.GetCurrentProcess();
                        Cpu = _cpu.NextValue();
                        Memory = float.Parse((proc.PrivateMemorySize64 / Math.Pow(1024, 2)).ToString());
                    }
                    catch (Exception)
                    {
                        Cpu = 0;
                        Memory = 0;
                    }
                    finally
                    {
                        Thread.Sleep(10000);
                    }
                }
            }
        }
        #endregion
    }
}
