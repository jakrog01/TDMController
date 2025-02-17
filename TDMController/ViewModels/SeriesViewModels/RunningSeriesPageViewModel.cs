﻿using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TDMController.Models;
using TDMController.Models.TDMDevices;
using TDMController.Models.TDMDevices.States;
using TDMController.Services;

namespace TDMController.ViewModels
{
    internal partial class RunningSeriesPageViewModel : ViewModelBase
    {
        private readonly IProjectService _projectCollectionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Series? _series = null;
        private CancellationTokenSource _seriesCancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _seriesCancellationToken;
        public Action<ViewModelBase> ChangePageAction { get; set; }

        private bool _isRunning = false;
        public RunningSeriesPageViewModel(IProjectService projectService, IServiceProvider serviceProvider, Series series)
        {
            _projectCollectionService = projectService;
            _serviceProvider = serviceProvider;
            _series = series;
            _seriesCancellationToken = _seriesCancellationTokenSource.Token;

            Branches = projectService.BranchList;
            if (Branches.Count == 0)
            {
                Logs.Add($" {DateTime.Now} > Problem with loading project");
                _buttonCommand = null;
            }
            else
            {
                Logs.Add($"{DateTime.Now} > Project loaded successfully");

                if (_series is null)
                {
                    Logs.Add($"{DateTime.Now} > Problem with loaded series");
                    _buttonCommand = null;
                }
                else if (_series.ProjectKey != projectService.Key)
                {
                    Logs.Add($"{DateTime.Now} > Incompatible project keys");
                    _buttonCommand = null;
                }
                else
                {
                    Logs.Add($"{DateTime.Now} > Series loaded successfully");
                    Logs.Add($"{DateTime.Now} > The project key and series match");
                    _countOperationsInSeries();
                    _buttonCommand = new RelayCommand(OnButtonClick);
                }
            }

            if (_buttonCommand is not null)
            {
                Logs.Add($"{DateTime.Now} > Click start button to start");
            }
        }

        public ObservableCollection<Branch> Branches { get; private set; } = [];
        public ObservableCollection<string> Logs { get; private set; } = [];

        [ObservableProperty]
        private MaterialIconKind _buttonIcon = MaterialIconKind.PlayArrow;

        [ObservableProperty]
        private ICommand? _buttonCommand = null;

        [ObservableProperty]
        private int _maxProgressBar = 0;

        [ObservableProperty]
        private int _minProgressBar = 0;

        [ObservableProperty]
        private int _valueProgressBar = 0;

        public void OnButtonClick()
        {
            if (!_isRunning)
            {
                Task.Run(StartMeasurementAsync, _seriesCancellationToken);
            }
            else
            {
                StopMeasurement();
            }
        }

        private void _countOperationsInSeries()
        {
            var max = 0;
            foreach (var sequence in _series.Sequences)
            {
                max += sequence.Repeat;
            }
            MaxProgressBar = max;
        }

        public async Task StartMeasurementAsync()
        {
            _isRunning = true;
            ButtonIcon = MaterialIconKind.Stop;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Logs.Add($"{DateTime.Now} > Started new series");
            });


            int sequenceIndex = 0;
            string now = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"{now}.txt";
            string directoryPath = System.IO.Path.GetFullPath(Environment.CurrentDirectory + @"\PowerMeasurements");
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (IOException ex) when (ex.Message.Contains("already exists"))
            {

            }

            string fullFilePath = Path.Combine(directoryPath, filename);

            using (StreamWriter writer = File.AppendText(fullFilePath))
            {

                foreach (Sequence sequence in _series!.Sequences)
                {
                    sequenceIndex += 1;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Logs.Add($"{DateTime.Now} > Started {sequenceIndex}. sequence. {_series.Sequences.Count - sequenceIndex} left");
                    });

                    for (int sequenceRepetition = 0; sequenceRepetition < sequence.Repeat; sequenceRepetition++)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ValueProgressBar += 1;
                            Logs.Add($"{DateTime.Now} > Started {sequenceRepetition + 1}. repetition of {sequenceIndex}. sequence. {sequence.Repeat - (1 + sequenceRepetition)} left");

                            foreach (Branch branch in Branches)
                            {
                                if (branch.State == BranchStates.Error)
                                {
                                    Logs.Add($"{DateTime.Now} > Problem with Branch {branch.BranchIndex}");
                                }

                                if (branch.PositionDevice?.State.HasFlag(PositionDeviceStates.Error) == true)
                                {
                                    Logs.Add($"{DateTime.Now} > Problem with position device in Branch {branch.BranchIndex}");
                                }

                                if (branch.RotationDevice?.State.HasFlag(RotationDeviceStates.Error) == true)
                                {
                                    Logs.Add($"{DateTime.Now} > Problem with rotation device in Branch {branch.BranchIndex}");
                                }
                            }
                        });

                        int taskCounter = 0;
                        var Tasks = new List<Task>();

                        foreach (Branch branch in Branches)
                        {
                            var Commands = new List<int>();
                            if (branch.RotationDevice is not null)
                            {
                                Commands.Add(sequence.Commands[taskCounter]);
                                taskCounter++;
                            }

                            if (branch.PositionDevice is not null)
                            {
                                Commands.Add(sequence.Commands[taskCounter]);
                                taskCounter++;
                            }

                            var sequenceTask = Task.Run(() => branch.MoveMotorsInSequence(Commands));
                            Tasks.Add(sequenceTask);
                        }

                        await Task.WhenAll(Tasks);

                        for (int actionNumber = 0; actionNumber < sequence.ActionPerStep; actionNumber++)
                        {
                            if (_projectCollectionService.MeasureBranch is not null && _series.Measure)
                            {
                                var measureTask = Task.Run(() => _projectCollectionService.MeasureBranch.SendExternalDeviceTrigger());
                                Tasks.Add(measureTask);
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Logs.Add($"{DateTime.Now} > Making {actionNumber + 1}. maesurement");
                                });
                            }

                            if (_projectCollectionService.PhotoBranch is not null && _series.TakePhoto)
                            {
                                var photoTask = Task.Run(() => _projectCollectionService.PhotoBranch.SendExternalDeviceTrigger());
                                Tasks.Add(photoTask);
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Logs.Add($"{DateTime.Now} > Taking {actionNumber + 1}. photo");
                                });
                            }
                            await Task.WhenAll(Tasks);
                            Tasks.Clear();
                            _seriesCancellationToken.ThrowIfCancellationRequested();

                            if (_projectCollectionService.PowerMeter.PowerMeterDevice is not null)
                            {
                                var power = _projectCollectionService.PowerMeter.MeasurePower();
                                writer.WriteLine(power);
                            }
                        }
                    }
                }
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Logs.Add($"{DateTime.Now} > Series completed");
                });

            }
        }
        public void StopMeasurement()
        {
            _isRunning = false;
            _seriesCancellationTokenSource?.Cancel();
            ChangePageAction?.Invoke(new NewSeriesPageViewModel(_serviceProvider));
        }
    }
}
