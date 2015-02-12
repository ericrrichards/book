using System.Diagnostics;

namespace dx_book {
    /// <summary>
    /// High-precision timer
    /// </summary>
    public class GameTimer {
        private readonly double _secondsPerCount;
        private double _deltaTime;

        private long _baseTime;
        private long _pausedTime;
        private long _stopTime;
        private long _prevTime;
        private long _currTime;

        private bool _stopped;

        public GameTimer() {
            _secondsPerCount = 0.0;
            _deltaTime = -1.0;
            _baseTime = 0;
            _pausedTime = 0;
            _prevTime = 0;
            _currTime = 0;
            _stopped = false;

            var countsPerSec = Stopwatch.Frequency;
            _secondsPerCount = 1.0 / countsPerSec;

        }
        /// <summary>
        /// Total time the timer has been running, excluding paused time
        /// </summary>
        public float TotalTime {
            get {
                if (_stopped) {
                    return (float)(((_stopTime - _pausedTime) - _baseTime) * _secondsPerCount);
                }
                return (float)(((_currTime - _pausedTime) - _baseTime) * _secondsPerCount);
            }
        }
        /// <summary>
        /// Time between two most recent calls to Tick()
        /// </summary>
        public float DeltaTime {
            get { return (float)_deltaTime; }
        }

        /// <summary>
        /// Reset to timer to zero and starts it
        /// </summary>
        public void Reset() {
            var curTime = Stopwatch.GetTimestamp();
            _baseTime = curTime;
            _prevTime = curTime;

            _stopTime = 0;
            _stopped = false;
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start() {
            var startTime = Stopwatch.GetTimestamp();
            if (_stopped) {
                _pausedTime += (startTime - _stopTime);
                _prevTime = startTime;
                _stopTime = 0;
                _stopped = false;
            }
        }
        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop() {
            if (!_stopped) {
                var curTime = Stopwatch.GetTimestamp();
                _stopTime = curTime;
                _stopped = true;
            }
        }
        /// <summary>
        /// Advance the timer one tick
        /// </summary>
        public void Tick() {
            if (_stopped) {
                _deltaTime = 0.0;
                return;
            }
            var curTime = Stopwatch.GetTimestamp();
            _currTime = curTime;

            _deltaTime = (_currTime - _prevTime) * _secondsPerCount;
            _prevTime = _currTime;
            if (_deltaTime < 0.0) {
                _deltaTime = 0.0;
            }
        }
    }
}