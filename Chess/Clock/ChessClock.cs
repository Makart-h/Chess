﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Chess.AI;
using Chess.Pieces;

namespace Chess.Clock
{
    internal static class ChessClock
    {
        private static readonly Dictionary<Team, Controller> _controllers;
        private static readonly Dictionary<Team, ClockTimer> _timers;
        private static readonly Dictionary<Team, Double> _increment;
        private static readonly Dictionary<Controller, CancellationTokenSource> _cancelletionTokenSources;
        private static bool _isRunning;
        private static bool _isInitialized;
        private static Team _activeTeam;
        public static Controller ActiveController
        {
            get
            {
                CheckInitialization();
                return _controllers[_activeTeam];              
            }
        }
        public static event EventHandler<TimerExpiredEventArgs> TimerExpired;
        static ChessClock()
        {
            _isRunning = false;
            _isInitialized = false;
            _controllers = new Dictionary<Team, Controller>();
            _timers = new Dictionary<Team, ClockTimer>();
            _cancelletionTokenSources = new Dictionary<Controller, CancellationTokenSource>();
            _increment = new Dictionary<Team, double>();
        }
        public static void Initialize(Controller white, Controller black, TimeSpan interval, TimeSpan increment, Team activeTeam)
        {
            _activeTeam = activeTeam;
            AddCancelletionTokenSource(white, black);
            SetIncrement(increment);
            SetControllers(white: white, black: black);
            CreateTimers(interval);
            SubscribeToEvents();
            _isInitialized = true;
        }
        private static void SetIncrement(TimeSpan increment)
        {
            _increment[Team.White] = increment.TotalMilliseconds;
            _increment[Team.Black] = increment.TotalMilliseconds;
        }
        private static void SetControllers(Controller white, Controller black)
        {
            _controllers[Team.White] = white;
            _controllers[Team.Black] = black;
        }
        private static void CreateTimers(TimeSpan interval)
        {
            _timers[Team.White] = new ClockTimer(interval.TotalMilliseconds);
            _timers[Team.Black] = new ClockTimer(interval.TotalMilliseconds);
        }
        private static void SubscribeToEvents()
        {
            _timers[Team.White].Elapsed += OnTimerExpired;
            _timers[Team.Black].Elapsed += OnTimerExpired;
            Controller.MoveMade += OnMoveChosen;
            Arbiter.GameConcluded += OnGameConcluded;
        }
        private static void CheckInitialization()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("ChessClock not initilized!");
        }
        private static void AddCancelletionTokenSource(params Controller[] controllers)
        {
            foreach (Controller controller in controllers)
            {
                if (controller is AIController)
                    _cancelletionTokenSources[controller] = new CancellationTokenSource();
            }
        }
        private static void OnMoveChosen(object sender, MoveMadeEventArgs e) => Toggle();
        private static void OnTimerExpired(object sender, ElapsedEventArgs e)
        {
            if (_cancelletionTokenSources.TryGetValue(_controllers[_activeTeam], out CancellationTokenSource source))
                source.Cancel();
            StopAllTimers();                   
            TimerExpired?.Invoke(null, new TimerExpiredEventArgs(_controllers[~_activeTeam]));
        }
        private static void OnGameConcluded(object sender, GameResultEventArgs e) => StopAllTimers();
        private static void StopAllTimers()
        {
            _isRunning = false;
            _timers[_activeTeam].Pause();
            _timers[~_activeTeam].Pause();
        }      
        private static void Toggle()
        {
            if (_isRunning)
            {
                _timers[_activeTeam].Pause();
                _timers[_activeTeam].Increment(_increment[_activeTeam]);
                _activeTeam = ~_activeTeam;
                _timers[_activeTeam].Resume();
            }
        }
        public static (TimeSpan WhiteTimer, TimeSpan BlackTimer) GetTimers()
        {
            CheckInitialization();
            return (TimeSpan.FromMilliseconds(_timers[Team.White].RemainingTime), TimeSpan.FromMilliseconds(_timers[Team.Black].RemainingTime));               
        }
        public static void Start()
        {
            CheckInitialization();
            if (!_isRunning)
            {
                _isRunning = true;
                _timers[_activeTeam].Start();
            }
        }
        public static CancellationToken? GetCancelletionToken(Controller controller)
        {
            CheckInitialization();
            if (_cancelletionTokenSources.TryGetValue(controller, out var source))
                return source.Token;
            else
                return null;
        }
    }
}